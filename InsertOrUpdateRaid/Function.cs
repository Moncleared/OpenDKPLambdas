using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenDKPShared.ClientModels;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace InsertOrUpdateRaid
{
    public class Function
    {
        /// <summary>
        /// Lambda to insert character to character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, should be a PUT or POST with a BODY</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            if (pRequest == null) return HttpHelper.HandleError("Request appears to be null", 500);

            using (opendkpContext vDatabase = new opendkpContext())
            {
                if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                    return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                var vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);

                if (pRequest.HttpMethod.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
                {
                    return HandleInsert(pRequest, pContext, vCognitoUser, vDatabase);
                }
                else if (pRequest.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                {
                    return HandleUpdate(pRequest, pContext, vCognitoUser, vDatabase);
                }
            }

            return HttpHelper.HandleError("Only Methods PUT,POST are supported by this lamdba", 500);
        }

        private APIGatewayProxyResponse HandleUpdate(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser, opendkpContext pDatabase)
        {
            var vResponse = HttpHelper.HandleError("[InsertOrUpdateAdjustment] Unknown error backend...", 500);
            try
            {
                //Populate Model
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                int vIdRaid = vModel.IdRaid;
                //We need to retrieve the ClientId for multitenancy purposes
                var vClientId = pRequest.Headers["clientid"];


                Dictionary<string, Characters> vCharacterModels = RaidHelper.GetCharacterModels(pDatabase, vClientId, vModel);

                using (var dbContextTransaction = pDatabase.Database.BeginTransaction())
                {
                    Raids vRaid = pDatabase.Raids.
                    Include("Ticks.TicksXCharacters").
                    Include("ItemsXCharacters.Item").
                    Include("ItemsXCharacters.Character").
                    Include("IdPoolNavigation").
                    FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.IdRaid == vIdRaid);

                    dynamic vOldModel = GetAuditModel(vRaid);

                    //Update some attributes of the raid
                    vRaid.Name = vModel.Name;
                    vRaid.Timestamp = vModel.Timestamp;
                    vRaid.UpdatedTimestamp = DateTime.Now;
                    vRaid.UpdatedBy = vModel.UpdatedBy;
                    vRaid.IdPool = vModel.Pool.IdPool;
                    vRaid.ClientId = vClientId;

                    //Three Cases to handle: Raid Tick Added, Raid Tick Removed, Raid Tick Updated
                    #region Handle Raid Tick Removed here
                    SimpleTick[] vSimpleTicks = vModel.Ticks.ToObject<SimpleTick[]>();
                    List<Ticks> vRemoveTicks = new List<Ticks>();
                    foreach (Ticks vIndex in vRaid.Ticks)
                    {
                        var vFound = vSimpleTicks.FirstOrDefault(x => x.TickId == vIndex.TickId);
                        if (vFound == null)
                            vRemoveTicks.Add(vIndex);
                    }
                    foreach (Ticks vTick in vRemoveTicks) vRaid.Ticks.Remove(vTick);
                    #endregion
                    #region Handle Raid Tick Added & Raid Tick Updated Here
                    foreach(var vTick in vModel.Ticks)
                    {
                        int? vTickId = vTick.TickId;
                        //If tickId is null, we have to create an insert a new one
                        if (vTickId == null)
                        {
                            RaidHelper.CreateTick(pDatabase, vClientId, vRaid.IdRaid, vTick, vCharacterModels);
                        }
                        else
                        {
                            Ticks vFoundTick = vRaid.Ticks.FirstOrDefault(x => x.TickId == vTickId);
                            if (vFoundTick != null)
                            {
                                vFoundTick.Description = vTick.Description;
                                vFoundTick.Value = vTick.Value;
                                vFoundTick.ClientId = vClientId;

                                //Now that I've found the tick
                                vFoundTick.TicksXCharacters = new List<TicksXCharacters>();
                                foreach(string vAttendee in vTick.Attendees)
                                {
                                    vFoundTick.TicksXCharacters.Add(new TicksXCharacters
                                    {
                                        IdCharacterNavigation = vCharacterModels[vAttendee.ToLower()],
                                        IdTickNavigation = vFoundTick,
                                        ClientId = vClientId
                                    });
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("The tick id {0} does not exist, will not save raid", vTickId));
                            }
                        }
                    }
                    #endregion

                    //Handle Items
                    vRaid.ItemsXCharacters = new List<ItemsXCharacters>();
                    RaidHelper.InsertRaidItems(pDatabase, vClientId, vModel, vCharacterModels);

                    //Save
                    pDatabase.SaveChanges();
                    dbContextTransaction.Commit();

                    //Respond
                    vResponse = HttpHelper.HandleResponse(vModel, 200);

                    //Audit
                    AuditHelper.InsertAudit(pDatabase, vClientId, vOldModel, vModel, pCognitoUser.Username, Audit.ACTION_RAID_UPDATE);

                    //Update Caches
                    int vStatus = CacheManager.UpdateSummaryCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("SummaryCacheResponse=" + vStatus);
                    vStatus = CacheManager.UpdateItemCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("ItemCacheResponse=" + vStatus);
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }

            return vResponse;
        }

        private dynamic GetAuditModel(Raids vRaid)
        {
            dynamic vOldModel = new
            {
                vRaid.Name,
                vRaid.Timestamp,
                vRaid.UpdatedTimestamp,
                vRaid.UpdatedBy,
                vRaid.IdPool,
                Ticks = new List<object>(),
                Items = new List<object>(),
                Pool = new
                {
                    vRaid.IdPoolNavigation.IdPool,
                    vRaid.IdPoolNavigation.Name,
                    vRaid.IdPoolNavigation.Description,
                    vRaid.IdPoolNavigation.Order
                }
            };
            foreach (Ticks vTick in vRaid.Ticks)
            {
                var vAttendees = new List<string>();
                foreach (var vName in vTick.TicksXCharacters)
                {
                    vAttendees.Add(vName.IdCharacterNavigation.Name);
                }
                vOldModel.Ticks.Add(new
                {
                    vTick.Description,
                    vTick.Value,
                    Attendees = vAttendees
                });
            }

            foreach (var vItem in vRaid.ItemsXCharacters)
            {
                vOldModel.Items.Add(new
                {
                    ItemName = vItem.Item.Name,
                    DkpValue = vItem.Dkp,
                    CharacterName = vItem.Character.Name,
                    ItemID = vItem.ItemId
                });
            }
            return vOldModel;
        }

        private APIGatewayProxyResponse HandleInsert(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser,opendkpContext pDatabase)
        {
            var vResponse = HttpHelper.HandleError("[InsertOrUpdateRaid] Unknown error backend...", 500);
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];
            try
            {
                //Populate Model
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                using (var dbContextTransaction = pDatabase.Database.BeginTransaction())
                {
                    Dictionary<string, Characters> vCharacterModels = RaidHelper.GetCharacterModels(pDatabase, vClientId, vModel);

                    vModel.IdRaid = RaidHelper.CreateRaidId(pDatabase,vClientId, vModel);
                    RaidHelper.InsertRaidItems(pDatabase, vClientId, vModel, vCharacterModels);
                    RaidHelper.InsertRaidTicks(pDatabase, vClientId, vModel, vCharacterModels);
                    pDatabase.SaveChanges();
                    dbContextTransaction.Commit();

                    vResponse = HttpHelper.HandleResponse(vModel, 200);
                    AuditHelper.InsertAudit(pDatabase, vClientId, string.Empty, vModel, pCognitoUser.Username, Audit.ACTION_RAID_INSERT);

                    //Update Caches
                    int vStatus = CacheManager.UpdateSummaryCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("SummaryCacheResponse=" + vStatus);
                    vStatus = CacheManager.UpdateItemCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("ItemCacheResponse=" + vStatus);
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }

            return vResponse;
        }
    }
    class SimpleTick
    {
        public int? TickId { get; set; }
    }
}
