using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using OpenDKPShared.ClientModels;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace InsertOrUpdateAdjustment
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
            //For these operations, we should have the CognitoInfo user available to us
            //Authorized Users only for Deleting Raids
            using (opendkpContext vDatabase = new opendkpContext() )
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
                    return HandleUpdateAsync(pRequest, pContext, vCognitoUser, vDatabase);
                }
            }

            return HttpHelper.HandleError("Only Methods PUT,POST are supported by this lamdba", 500);
        }

        private APIGatewayProxyResponse HandleUpdateAsync(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser, opendkpContext pDatabase)
        {
            var vResponse = HttpHelper.HandleError("[InsertOrUpdateAdjustment] Unknown error backend...", 500);
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            try
            {
                //Populate Model
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                Adjustments vAdjustment = new Adjustments() {
                        Name = vModel.Name,
                        Description = vModel.Description,
                        Value = vModel.Value,
                        Timestamp = vModel.Timestamp,
                        IdAdjustment = vModel.Id,
                        ClientId = vClientId
                };
                int vId = vModel.Id;
                if (vAdjustment != null)
                {
                    Adjustments vExists = pDatabase.Adjustments
                        .FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.IdAdjustment == vId);
                    string vOld = JsonConvert.SerializeObject(vExists,new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

                    vAdjustment.IdCharacter = vExists.IdCharacter;
                    pDatabase.Entry(vExists).CurrentValues.SetValues(vAdjustment);
                    pDatabase.SaveChanges();
                    vResponse = HttpHelper.HandleResponse(vModel, 200);

                    //Audit
                    AuditHelper.InsertAudit(pDatabase, vClientId, vOld, pRequest.Body, pCognitoUser.Username, Audit.ACTION_ADJUST_UPDATE);

                    //Update Caches
                    int vStatus = CacheManager.UpdateSummaryCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("StatusCode for CacheUpdate=" + vStatus);
                }
            }
            catch (Exception vException)
            {
                Console.WriteLine("1 Exception: " + vException);
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }
            return vResponse;
        }

        private APIGatewayProxyResponse HandleInsert(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser, opendkpContext pDatabase)
        {
            var vResponse = HttpHelper.HandleError("[InsertOrUpdateAdjustment] Unknown error backend...", 500);
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];
            try
            {
                //Populate Model
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                string vInputCharacters = (string)vModel.Character;
                vInputCharacters = vInputCharacters.Trim();

                string[] vCharacterList = vInputCharacters.Split(',');

                using (var dbContextTransaction = pDatabase.Database.BeginTransaction())
                {
                    foreach(string vCharacter in vCharacterList)
                    {
                        //Create our Adjustment for each Character
                        Adjustments vAdjustment = new Adjustments
                        {
                            Name = vModel.Name,
                            Description = vModel.Description,
                            Value = vModel.Value,
                            Timestamp = vModel.Timestamp,
                            ClientId = vClientId
                        };

                        //Convert Character Name to Character Id
                        vAdjustment.IdCharacter = DkpConverters.CharacterNameToId(pDatabase, vClientId, vCharacter);
                        if (vAdjustment != null && vAdjustment.IdCharacter > -1)
                        {
                            pDatabase.Add(vAdjustment);
                            pDatabase.SaveChanges();
                        }
                    }
                    //Commit changes to DB
                    dbContextTransaction.Commit();
                    //Create success response
                    vResponse = HttpHelper.HandleResponse(vModel, 200);

                    //Audit
                    AuditHelper.InsertAudit(pDatabase, vClientId, string.Empty, vModel, pCognitoUser.Username, Audit.ACTION_ADJUST_INSERT);

                    //Update Caches
                    int vStatus = CacheManager.UpdateSummaryCacheAsync(vClientId).GetAwaiter().GetResult();
                    Console.WriteLine("StatusCode for CacheUpdate=" + vStatus);
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }

            return vResponse;
        }
    }
}
