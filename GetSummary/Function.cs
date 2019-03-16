using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenDKPShared.ClientModels;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetSummary
{
    public class Function
    {
        private double totalTicks30 = 0.0;
        private double totalTicks60 = 0.0;
        private double totalTicks90 = 0.0;

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
            if ( HttpHelper.TriggeredLambda(pRequest))
            {
                Console.WriteLine("Auto triggered force cache update detected");
                vForceUpdate = true;
            }

            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            SummaryModel vModel = GetCachedModel(vForceUpdate, vClientId);
            return HttpHelper.HandleResponse(vModel, 200);
        }

        public SummaryModel GetCachedModel(bool pForceUpdate, string pClientId)
        {
            SummaryModel vReturnModel = new SummaryModel();
            using (opendkpContext vDatabase = new opendkpContext())
            {
                Cache vCachedSummary = CacheManager.GetCache(vDatabase, CacheManager.SUMMARY, pClientId);
                if (!pForceUpdate && vCachedSummary != null && DateTime.Compare(vCachedSummary.CacheExpires, DateTime.Now) > 0)
                {
                    vReturnModel = JsonConvert.DeserializeObject<SummaryModel>(vCachedSummary.CacheValue);
                }
                else
                {
                    vReturnModel = GetSummary(vDatabase, pClientId);
                    CacheManager.SetCache(vDatabase,
                        CacheManager.SUMMARY,
                        JsonConvert.SerializeObject(vReturnModel),
                        pClientId);
                }
            }
            return vReturnModel;
        }

        public SummaryModel GetSummary(opendkpContext pDatabase, string pClientId)
        {
            SummaryModel vModel = new SummaryModel();
            try
            {
                var vResult = pDatabase.Characters
                    .Where(x => x.ClientId.Equals(pClientId) && x.Active == 1)
                    .Include("TicksXCharacters.IdTickNavigation.Raid")
                    .Include("Adjustments")
                    .Include("ItemsXCharacters").ToList();

                var vResults = new List<object>();

                totalTicks30 = (double)pDatabase.Ticks.Include("Raid")
                    .Count(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.Raid.Timestamp.Date, DateTime.Now.AddDays(-30).Date) >= 0);

                totalTicks60 = (double)pDatabase.Ticks.Include("Raid")
                    .Count(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.Raid.Timestamp.Date, DateTime.Now.AddDays(-60).Date) >= 0);

                totalTicks90 = (double)pDatabase.Ticks.Include("Raid")
                    .Count(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.Raid.Timestamp.Date, DateTime.Now.AddDays(-90).Date) >= 0);

                foreach (Characters vCharacter in vResult)
                {
                    PlayerSummaryModel vPlayerSummary = new PlayerSummaryModel();
                    vPlayerSummary.CurrentDKP = GetCharacterDkp(vCharacter, pClientId);
                    SetAttendance(pDatabase,pClientId, vCharacter, vPlayerSummary);
                    vModel.Models.Add(vPlayerSummary);
                }
                vModel.AsOfDate = DateTime.Now;
            }
            catch (Exception vException)
            {
                Console.WriteLine("Exception creating latest summary: " + vException);
            }
            return vModel;
        }

        public PlayerSummaryModel SetAttendance(opendkpContext pDatabase, string pClientId, Characters pCharacter, PlayerSummaryModel pPlayerSummaryModel)
        {

            pPlayerSummaryModel.CharacterName = pCharacter.Name;
            pPlayerSummaryModel.CharacterClass = pCharacter.Class;
            pPlayerSummaryModel.CharacterRank = pCharacter.Rank;
            pPlayerSummaryModel.CharacterStatus = pCharacter.Active.ToString();

            var vHasAttendedRaid = pCharacter.TicksXCharacters
                .Where(y => y.ClientId.Equals(pClientId))
                .OrderBy(x => x.IdTickNavigation.Raid.Timestamp)
                .FirstOrDefault();

            DateTime vCalculatedDate = DateTime.Now;
            if (vHasAttendedRaid != null)
                vCalculatedDate = vHasAttendedRaid.IdTickNavigation.Raid.Timestamp.Date;

            DateTime v30Day = DateTime.Now.AddDays(-30).Date;

            //If a player has main changed, we calculate their 30 day RA based off their latest main change date
            DateTime vMainChange = pCharacter.MainChange ?? DateTime.MinValue;
            v30Day = new[] { v30Day, vMainChange }.Max();

            pPlayerSummaryModel.AttendedTicks_30 = (double)pCharacter.TicksXCharacters
                .Where(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date, v30Day) >= 0)
                .Count();
            pPlayerSummaryModel.TotalTicks_30 = totalTicks30;
            pPlayerSummaryModel.Calculated_30 = pPlayerSummaryModel.AttendedTicks_30 / pPlayerSummaryModel.TotalTicks_30;

            pPlayerSummaryModel.AttendedTicks_60 = (double)pCharacter.TicksXCharacters
                .Where(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date, DateTime.Now.AddDays(-60).Date) >= 0)
                .Count();

            pPlayerSummaryModel.TotalTicks_60 = totalTicks60;
            pPlayerSummaryModel.Calculated_60 = pPlayerSummaryModel.AttendedTicks_60 / pPlayerSummaryModel.TotalTicks_60;

            pPlayerSummaryModel.AttendedTicks_90 = (double)pCharacter.TicksXCharacters
                .Where(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date, DateTime.Now.AddDays(-90).Date) >= 0)
                .Count();
            pPlayerSummaryModel.TotalTicks_90 = totalTicks90;
            pPlayerSummaryModel.Calculated_90 = pPlayerSummaryModel.AttendedTicks_90 / pPlayerSummaryModel.TotalTicks_90;

            pPlayerSummaryModel.AttendedTicks_Life = (double)pCharacter.TicksXCharacters
                .Where(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date, vCalculatedDate.Date) >= 0)
                .Count();

            pPlayerSummaryModel.TotalTicks_Life = (double)pDatabase.Ticks.Include("Raid")
                .Count(x => x.ClientId.Equals(pClientId) && DateTime.Compare(x.Raid.Timestamp.Date, vCalculatedDate.Date) >= 0);
            if (pPlayerSummaryModel.TotalTicks_Life > 0)
                pPlayerSummaryModel.Calculated_Life = pPlayerSummaryModel.AttendedTicks_Life / pPlayerSummaryModel.TotalTicks_Life;
            else
                pPlayerSummaryModel.Calculated_Life = 0.0;

            return pPlayerSummaryModel;
        }

        private double GetCharacterDkp(Characters pCharacter, string pClientId)
        {
            double vDkp = 0;
            vDkp += pCharacter.TicksXCharacters.Where(y => y.ClientId.Equals(pClientId)).Sum(x => x.IdTickNavigation.Value);
            vDkp -= pCharacter.ItemsXCharacters.Where(y => y.ClientId.Equals(pClientId)).Sum(x => x.Dkp);
            vDkp += pCharacter.Adjustments.Where(y => y.ClientId.Equals(pClientId)).Sum(x => x.Value);
            return vDkp;
        }
    }
}
