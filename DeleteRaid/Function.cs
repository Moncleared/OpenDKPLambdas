using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeleteRaid
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
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[DeleteRaid] Unknown Backend error", 500);

            try
            {
                if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    int vId = int.Parse(pRequest.PathParameters["id"]);
                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        //Authorized Users only for Deleting Raids
                        if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                        !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                            return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                        var vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);

                        //We need to retrieve the ClientId for multitenancy purposes
                        var vClientId = pRequest.Headers["clientid"];

                        Raids vResult = RaidHelper.DeleteRaid(vDatabase, vId, vClientId);
                        vDatabase.SaveChanges();
                        vResponse = HttpHelper.HandleResponse(vResult, 200);

                        //Audit
                        AuditHelper.InsertAudit(vDatabase, vClientId, vResult, string.Empty, vCognitoUser.Username, Audit.ACTION_RAID_DELETE);

                        //Update Caches
                        int vStatus = CacheManager.UpdateSummaryCacheAsync(vClientId).GetAwaiter().GetResult();
                        Console.WriteLine("SummaryCacheResponse=" + vStatus);
                        vStatus = CacheManager.UpdateItemCacheAsync(vClientId).GetAwaiter().GetResult();
                        Console.WriteLine("ItemCacheResponse=" + vStatus);
                    }
                }
            }
            catch
            {
                vResponse = HttpHelper.HandleError("[DeleteRaid] Issue with DB or Query", 500);
            }

            return vResponse;
        }
    }
}
