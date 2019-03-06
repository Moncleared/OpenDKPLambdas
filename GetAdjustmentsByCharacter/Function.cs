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

namespace GetAdjustmentsByCharacter
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
            var vResponse = HttpHelper.HandleError("[GetAdjustmentsByCharacter] Unknown Backend error", 500);
            try
            {
                if ( pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    var vItems = new List<object>();
                    string vCharacterName = pRequest.PathParameters["id"];

                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];

                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        var vResult = vDatabase.Characters
                            .Include("Adjustments")
                            .First(x => x.ClientId.Equals(vClientId) && x.Name.Equals(vCharacterName, StringComparison.InvariantCultureIgnoreCase));

                        foreach( var vAdjustment in vResult.Adjustments)
                        {
                            vItems.Add(new { vAdjustment.Name, Date = vAdjustment.Timestamp, vAdjustment.Value });
                        }
                        vResponse = HttpHelper.HandleResponse(vItems, 200, true);
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetAdjustmentsByCharacter] {0}", vException.Message), 500);
            }

            return vResponse;
        }
    }
}
