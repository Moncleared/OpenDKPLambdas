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

namespace GetItemsByCharacter
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
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetItemsByCharacter] Unknown Backend error", 500);
            try
            {
                if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    string vCharacterName = pRequest.PathParameters["id"];

                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];
                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        var vItems = new List<object>();

                        var vResult = vDatabase.Characters
                            .Include("ItemsXCharacters.Item")
                            .Include("ItemsXCharacters.Raid").
                            First(x => x.ClientId.Equals(vClientId) && x.Name.Equals(vCharacterName, StringComparison.InvariantCultureIgnoreCase));
                            

                        foreach(var vItem in vResult.ItemsXCharacters)
                        {
                            vItems.Add(new
                            {
                                ItemName = vItem.Item.Name,
                                ItemID = vItem.ItemId,
                                Raid = vItem.Raid.Name,
                                RaidID = vItem.RaidId,
                                Date = vItem.Raid.Timestamp,
                                DkpValue = vItem.Dkp
                            });
                        }
             
                        vResponse = HttpHelper.HandleResponse(vItems, 200, true);
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(string.Format("[GetItemsByCharacter] {0}", vException.Message), 500);
            }

            return vResponse;
        }
    }
}
