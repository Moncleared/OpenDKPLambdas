using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAllRaids
{
    public class Function
    {
        /// <summary>
        /// Lambda to fetch all raids from the raid table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetAllRaids] Unknown Error in backend", 500);
            try
            {
                //We need to retrieve the ClientId for multitenancy purposes
                var vClientId = pRequest.Headers["clientid"];

                using (opendkpContext vDatabase = new opendkpContext())
                {
                    var vResult = vDatabase.Raids.Where(x => x.ClientId.Equals(vClientId))
                        .Include("Ticks")
                        .Include("IdPoolNavigation")
                        .Include("ItemsXCharacters")
                        .ToList();

                    var vRaids = new List<object>();
                    foreach(var vItem in vResult)
                    {
                        vRaids.Add(new {
                            vItem.IdRaid,
                            Pool = new { vItem.IdPoolNavigation.Name, vItem.IdPoolNavigation.Description, vItem.IdPoolNavigation.IdPool },
                            vItem.Name,
                            vItem.Timestamp,
                            ItemCount = vItem.ItemsXCharacters.Count,
                            DKPValue = vItem.Ticks.Sum(x => x.Value),
                            DKPSpent = vItem.ItemsXCharacters.Sum(x => x.Dkp),
                            TotalTicks = vItem.Ticks.Count
                        });
                    }

                    vResponse = HttpHelper.HandleResponse(vRaids, 200);
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError("[GetAllRaids] " + vException.Message, 500);
            }
            return vResponse;
        }
    }
}
