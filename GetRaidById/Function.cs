using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetRaidById
{
    public class Function
    {
        /// <summary>
        /// Lambda to fetch character by name from character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, used to pull parameter from url</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetRaidById] Unknown Backend error", 500);
            try
            {
                if ( pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    int vId = int.Parse(pRequest.PathParameters["id"]);

                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];

                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        var vResult = vDatabase.Raids.Where( x=> x.ClientId.Equals(vClientId) && x.IdRaid==vId)
                            .Include("Ticks.TicksXCharacters.IdCharacterNavigation")
                            .Include("IdPoolNavigation")
                            .Include("ItemsXCharacters.Character")
                            .Include("ItemsXCharacters.Item")
                            .FirstOrDefault();
                        if ( vResult != null )
                        {

                            var vRaids = new
                            {
                                vResult.IdRaid,
                                Pool = new { vResult.IdPoolNavigation.Name, vResult.IdPoolNavigation.Description, vResult.IdPoolNavigation.IdPool },
                                vResult.Name,
                                vResult.Timestamp,
                                vResult.UpdatedBy,
                                vResult.UpdatedTimestamp,
                                Ticks = new List<object>(),
                                Items = new List<object>()
                            };

                            foreach (var vItem in vResult.ItemsXCharacters)
                            {
                                vRaids.Items.Add(new
                                {
                                    CharacterName = vItem.Character.Name,
                                    ItemName = vItem.Item.Name,
                                    ItemID = vItem.ItemId,
                                    DkpValue = vItem.Dkp
                                });
                            }

                            foreach (var vItem in vResult.Ticks)
                            {
                                var vTick = new
                                {
                                    vItem.Description,
                                    vItem.Value,
                                    RaidId = vResult.IdRaid,
                                    vItem.TickId,
                                    Attendees = new List<string>()
                                };

                                //Add Attendees to Tick
                                foreach (var vAttendee in vItem.TicksXCharacters)
                                {
                                    vTick.Attendees.Add(vAttendee.IdCharacterNavigation.Name);
                                }
                                vRaids.Ticks.Add(vTick);
                            }

                            vResponse = HttpHelper.HandleResponse(vRaids, 200);
                        }
                        else
                        {
                            vResponse = HttpHelper.HandleError(string.Format("[GetRaidById] {0} Raid not found", vId), 500);
                        }
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetRaidById] {0}", vException.Message), 500);
            }

            return vResponse;
        }
    }
}
