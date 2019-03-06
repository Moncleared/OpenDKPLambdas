using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Model;
using Newtonsoft.Json;
using OpenDKPShared.ClientModels;
using OpenDKPShared.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDKPShared.Helpers
{
    public static class CacheManager
    {
        public static string SUMMARY = "summary";
        public static string ITEM_HISTORY = "item_history";

        /// <summary>
        /// Fetches the cached value from the database context passed in
        /// </summary>
        /// <param name="pContext">The database context</param>
        /// <param name="pCacheName">The name of the cahce you are retriving, case-insensitive</param>
        /// <returns></returns>
        public static Cache GetCache(opendkpContext pContext, string pCacheName, string pClientId)
        {
            Cache vReturnModel = null;
            try
            {
                vReturnModel = pContext.Cache.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.CacheName.Equals(pCacheName, StringComparison.InvariantCultureIgnoreCase));
            }
            catch(Exception vException)
            {
                Console.WriteLine("Error Retriving Cache: " + vException);
            }
            return vReturnModel;
        }

        /// <summary>
        /// Set the cache value for a particular cache
        /// </summary>
        /// <param name="pContext">The database context in which to set the cache in</param>
        /// <param name="pCacheName">The name of the cache you are storing, case-insensitive</param>
        /// <param name="pCacheValue">The value to be stored for this cache</param>
        /// <param name="pCacheExpires">The time the cache should expire</param>
        public static void SetCache(opendkpContext pContext, string pCacheName, string pCacheValue, string pClientId, DateTime? pCacheExpires = null)
        {
            if (pCacheExpires == null) pCacheExpires = DateTime.Now.AddHours(12);
            try
            {
                Cache vCachedSummary = pContext.Cache.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.CacheName.Equals(pCacheName, StringComparison.InvariantCultureIgnoreCase));
                if (vCachedSummary == null)
                {
                    vCachedSummary = new Cache{
                        CacheName = pCacheName,
                        ClientId = pClientId
                    };
                    pContext.Add<Cache>(vCachedSummary);
                }
                vCachedSummary.CacheUpdated = DateTime.Now;
                vCachedSummary.CacheExpires = DateTime.Now.AddHours(12);
                vCachedSummary.CacheValue = pCacheValue;
                
                pContext.SaveChanges();
            }
            catch(Exception vException)
            {
                Console.WriteLine("Error Cacheing: " + vException);
            }
        }

        /// <summary>
        /// Fetches the Player Summary Cache, mostly a helper function to pull out PlayerSummaryModel
        /// </summary>
        /// <param name="pContext">The database to retrieve the cache from</param>
        /// <param name="pCharacterName">The name of the character to pull from, case-insensitive</param>
        /// <returns></returns>
        public static PlayerSummaryModel GetPlayerSummary(opendkpContext pContext, string pCharacterName, string pClientId)
        {
            var vReturnModel = new PlayerSummaryModel();
            Cache vCachedSummary = GetCache(pContext, SUMMARY, pClientId);
            var vModel = JsonConvert.DeserializeObject<SummaryModel>(vCachedSummary.CacheValue);
            foreach (var vItem in vModel.Models)
                if (vItem.CharacterName.Equals(pCharacterName, StringComparison.InvariantCultureIgnoreCase))
                    return vItem;
            return vReturnModel;
        }

        /// <summary>
        /// Async function to update the summary cache, ultimately calls a Lambda
        /// </summary>
        /// <returns></returns>
        public static async Task<int> UpdateSummaryCacheAsync(string pClientId)
        {
            APIGatewayProxyRequest vProxyRequest = new APIGatewayProxyRequest();
            vProxyRequest.Headers = new Dictionary<string, string>
                {
                    { "forceCache", "" },
                    { "clientid", pClientId }
                };
            string vPayload = JsonConvert.SerializeObject(vProxyRequest);
            InvokeResponse vResponse = await LambdaHelper.InvokeLambdaAsync("GetSummary", vPayload);
            return vResponse.StatusCode;
        }

        /// <summary>
        /// Async function to update the items cache, ultimately calls another lambda
        /// </summary>
        /// <returns></returns>
        public static async Task<int> UpdateItemCacheAsync(string pClientId)
        {
            APIGatewayProxyRequest vProxyRequest = new APIGatewayProxyRequest();
            vProxyRequest.Headers = new Dictionary<string, string>
                {
                    { "forceCache", "" },
                    { "clientid", pClientId }
                };
            string vPayload = JsonConvert.SerializeObject(vProxyRequest);
            InvokeResponse vResponse = await LambdaHelper.InvokeLambdaAsync("GetAllItems", vPayload);
            return vResponse.StatusCode;
        }
    }
}
