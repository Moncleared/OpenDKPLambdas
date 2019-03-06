using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using OpenDKPShared.Helpers;
using OpenDKPShared.Operations;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetRaidsByCharacter
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetRaidsByCharacter] Unknown Backend error", 500);
            try
            {
                if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];

                    int vLookback = getLookBack(pRequest);
                    string vCharacterName = pRequest.PathParameters["id"];
                    var vItems = RaidOperations.GetCharacterRaids(vClientId, vCharacterName, vLookback);
                    vResponse = HttpHelper.HandleResponse(vItems, 200, true);
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetRaidsByCharacter] {0}", vException.Message), 500);
            }

            return vResponse;
        }

        private int getLookBack(APIGatewayProxyRequest pRequest)
        {
            int vLookBack = 0;
            string vParse = string.Empty;
            if (pRequest.QueryStringParameters != null)
                pRequest.QueryStringParameters.TryGetValue("lookback", out vParse);
            int.TryParse(vParse, out vLookBack);
            return vLookBack;
        }
    }
}
