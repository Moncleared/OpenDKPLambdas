using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAllAdjustments
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
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetAllAdjustments] Unknown Error in backend", 500);
            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];

                    var vItems = new List<object>();
                    var vResult = vDatabase.Adjustments.Where(x=> x.ClientId.Equals(vClientId))
                                            .Include("IdCharacterNavigation").ToList();
                    foreach (var vModel in vResult)
                    {
                        vItems.Add(new {
                            Character = vModel.IdCharacterNavigation.Name,
                            vModel.Name,
                            vModel.Description,
                            vModel.Value,
                            vModel.Timestamp,
                            Id = vModel.IdAdjustment
                        });
                    }
                    vResponse = HttpHelper.HandleResponse(vItems, 200, true);
                }
            }
            catch
            {
                vResponse = HttpHelper.HandleError("[GetAllAdjustments] Error Connecting to DB", 500);
            }
            return vResponse;
        }
    }
}
