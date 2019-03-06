using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;
using System.Collections.Generic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetItemsByName
{
    public class Function
    {
        private static opendkpContext fDBContext = new opendkpContext();
        /// <summary>
        /// Lambda to fetch all items matching a given string
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, used to pull parameter from url</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers.Keys.Contains("warmup")) { return HttpHelper.WarmupResponse(); }
            var vResponse = HttpHelper.HandleError("[GetItemsByName] Unknown Backend error", 500);
            try
            {
                if (pRequest != null)
                {
                    if (pRequest.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && 
                        pRequest.QueryStringParameters != null && pRequest.QueryStringParameters.Count > 0)
                    {
                        string vItem = pRequest.QueryStringParameters["item"];
                        int vLimit = 10;
                        if (pRequest.QueryStringParameters.Keys.Contains("limit"))
                        {
                            int.TryParse(pRequest.QueryStringParameters["limit"], out vLimit);
                            if (vLimit <= 0) vLimit = 10;
                        }

                        var vResult = fDBContext.Items.Where(x => x.Name.Contains(vItem)).Take(vLimit);
                        var vTransformed = new List<object>();
                        foreach (var vIndex in vResult)
                        {
                            vTransformed.Add(new { ItemName = vIndex.Name, ItemID = vIndex.IdItem });
                        }
                        vResponse = HttpHelper.HandleResponse(vTransformed, 200);
                    }
                    if (pRequest.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dynamic vItemList = JsonConvert.DeserializeObject(pRequest.Body);
                        var vTransformed = new List<object>();

                        foreach(string vItemName in vItemList)
                        {
                            var vResult = fDBContext.Items
                                .Where(x => x.Name.ToLower().Contains(vItemName.Trim().ToLower()))
                                .FirstOrDefault();
                            if ( vResult != null )
                            {
                                vTransformed.Add(new { ItemName = vResult.Name, ItemID = vResult.IdItem });
                            }
                        }

                        vResponse = HttpHelper.HandleResponse(vTransformed, 200);
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetItemsByName] {0}", vException.Message), 500);
            }

            return vResponse;
        }
    }
}
