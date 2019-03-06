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

namespace AuditLambda
{
    public class Function
    {
        /// <summary>
        /// Lambda to fetch all characters from the character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers !=null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[AuditLambda] Unknown Error in backend", 500);
            
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            try
            {
                int vId = -1;
                if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    vId = int.Parse(pRequest.PathParameters["id"]);
                }
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    //Authorized Users only for Audit information
                    if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                        !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                        return HttpHelper.HandleError("You do not have permission to perform this action", 401);

                    if (pRequest.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                    {
                        List<Audit> vResult;
                        if (vId > -1)
                            vResult = vDatabase.Audit.Where(x => x.ClientId.Equals(vClientId) && x.Id == vId).ToList();
                        else
                        {
                            //We don't want to transmit a ton of data for fetching all audits
                            vResult = vDatabase.Audit.Where(x => x.ClientId.Equals(vClientId)).ToList();
                            foreach(Audit vItem in vResult )
                            {
                                vItem.NewValue = string.Empty;
                                vItem.OldValue = string.Empty;
                            }
                        }
                        vResponse = HttpHelper.HandleResponse(vResult, 200, true);
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError("[AuditLambda] " + vException.Message, 500);
            }
            return vResponse;
        }
    }
}
