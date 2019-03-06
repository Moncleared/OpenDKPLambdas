using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAllPools
{
    public class Function
    {
        /// <summary>
        /// Lambda to fetch all pool from the pool table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetAllPools] Unknown Error in backend", 500);
            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    var vResult = vDatabase.Pools.ToList();
                    vResponse = HttpHelper.HandleResponse(vResult, 200);
                }
            }
            catch
            {
                vResponse = HttpHelper.HandleError("[GetAllPools] Error Connecting to DB", 500);
            }
            return vResponse;
        }
    }
}
