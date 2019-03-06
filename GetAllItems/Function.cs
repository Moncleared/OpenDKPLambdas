using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using OpenDKPShared.ClientModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAllItems
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
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            bool vForceUpdate = false;
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("forceCache"))
            {
                Console.WriteLine("---ForceCache Requested");
                vForceUpdate = true;
            }
            if (HttpHelper.TriggeredLambda(pRequest))
            {
                Console.WriteLine("Auto triggered force cache update detected");
                vForceUpdate = true;
            }
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            List<ItemListModel> vModel = GetCachedModel(vForceUpdate, vClientId);
            return HttpHelper.HandleResponse(vModel, 200);
        }
        public List<ItemListModel> GetCachedModel(bool pForceUpdate, string pClientId)
        {
            List<ItemListModel> vReturnModel = new List<ItemListModel>();
            using (opendkpContext vDatabase = new opendkpContext())
            {
                Cache vCachedSummary = CacheManager.GetCache(vDatabase, CacheManager.ITEM_HISTORY, pClientId);
                if (!pForceUpdate && vCachedSummary != null && DateTime.Compare(vCachedSummary.CacheExpires, DateTime.Now) > 0)
                {
                    vReturnModel = JsonConvert.DeserializeObject<List<ItemListModel>>(vCachedSummary.CacheValue);
                }
                else
                {
                    vReturnModel = GetItemHistory(vDatabase, pClientId);
                    CacheManager.SetCache(vDatabase,
                        CacheManager.ITEM_HISTORY,
                        JsonConvert.SerializeObject(vReturnModel),
                        pClientId);
                }
            }
            return vReturnModel;
        }

        private List<ItemListModel> GetItemHistory(opendkpContext pDatabase, string pClientId)
        {
            var vItems = new List<ItemListModel>();
            var vResult = pDatabase.ItemsXCharacters.Where(x => x.ClientId.Equals(pClientId))
                .Include("Character")
                .Include("Item")
                .Include("Raid")
                .ToList();

            foreach (var vModel in vResult)
            {
                vItems.Add(new ItemListModel
                {
                    ItemName = vModel.Item.Name,
                    ItemID = vModel.ItemId,
                    CharacterName = vModel.Character.Name,
                    DKP = vModel.Dkp,
                    IdRaid = vModel.RaidId,
                    Raid = vModel.Raid.Name,
                    Timestamp = vModel.Raid.Timestamp
                });
            }
            return vItems;
        }
    }
}
