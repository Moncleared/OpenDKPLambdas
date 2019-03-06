using Microsoft.EntityFrameworkCore;
using OpenDKPShared.DBModels;
using OpenDKPShared.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenDKPShared.Operations
{
    /// <summary>
    /// Really is kind of duplicative fo the RaidHelper class, need to refactor this later
    /// </summary>
    public static class RaidOperations
    {

        /// <summary>
        /// Gets all of a characters raids using a lookback period specified as an integer representing days
        /// </summary>
        /// <param name="pCharacterName">The character name, case-insensitive</param>
        /// <param name="pLookback">The lookback period specified in days</param>
        /// <returns></returns>
        public static IEnumerable GetCharacterRaids(string pClientId, string pCharacterName, int pLookback = 0)
        {
            Dictionary<int, RaidModel> vRaids = new Dictionary<int, RaidModel>();
            using (opendkpContext vDatabase = new opendkpContext())
            {
                var vCharacter = vDatabase.Characters.First(x => x.ClientId.Equals(pClientId) && x.Name.Equals(pCharacterName, StringComparison.InvariantCultureIgnoreCase));
                List<TicksXCharacters> vResult = null;
                if ( pLookback > 0 )
                {
                    vResult = vDatabase.TicksXCharacters
                                .Include("IdTickNavigation.Raid.IdPoolNavigation")
                                .Where(x => x.ClientId.Equals(pClientId) && x.IdCharacter == vCharacter.IdCharacter &&
                                            DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date,
                                            DateTime.Now.AddDays(pLookback*-1).Date) >= 0)
                                            .ToList(); 
                }
                else
                {
                    vResult = vDatabase.TicksXCharacters
                                        .Where(x => x.ClientId.Equals(pClientId) && x.IdCharacter == vCharacter.IdCharacter)
                                        .Include("IdTickNavigation.Raid.IdPoolNavigation")
                                        .ToList();
                }
                
                foreach (var vModel in vResult)
                {
                    TickModel vTick = new TickModel(vModel.IdTickNavigation);
                    if (!vRaids.ContainsKey(vModel.IdTickNavigation.RaidId))
                    {
                        RaidModel vRaid = new RaidModel(vModel.IdTickNavigation.Raid);
                        vRaid.TotalTicks = vDatabase.Ticks.Include("Raid").Count(x => x.ClientId.Equals(pClientId) && x.RaidId == vRaid.IdRaid);
                        vRaids.Add(vModel.IdTickNavigation.RaidId, vRaid);
                    }
                    vRaids[vModel.IdTickNavigation.RaidId].Ticks.Add(vTick);
                }
            }
            return vRaids.Values.ToList();
        }
    }
}
