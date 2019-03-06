using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetClient
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
            var vResponse = HttpHelper.HandleError("[GetClient] Unknown Backend error", 500);
            try
            {
                if ( pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    string vClient = pRequest.PathParameters["client"];
                    using (opendkpContext vDatabase = new opendkpContext())
                    { 
                        var vResult = vDatabase.Clients.First(x => x.Subdomain.Equals(vClient.Trim().ToLower()));
                        vResponse = HttpHelper.HandleResponse(vResult, 200);
                    }
                }
                else
                {
                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        var vResult = vDatabase.Clients.ToList();
                        vResponse = HttpHelper.HandleResponse(vResult, 200);
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetClient] {0}",vException.Message), 500);
            }

            return vResponse;
        }
    }
}
