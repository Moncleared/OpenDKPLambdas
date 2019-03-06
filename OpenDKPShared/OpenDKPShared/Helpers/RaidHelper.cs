using OpenDKPShared.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// Raid helper utility for standard actions on raid
    /// </summary>
    public class RaidHelper
    {
        /// <summary>
        /// Creates a raid Id, probably isn't needed anymore but early on in development I needed to get the RaidID first. 99.99% sure this isn't needed anymore but no time to refactor
        /// </summary>
        /// <param name="pDBContext">The database context to create the raid id in</param>
        /// <param name="pModel">The model containing the raid information to generate the id</param>
        /// <returns></returns>
        public static int CreateRaidId(opendkpContext pDBContext, string pClientId, dynamic pModel)
        {
            //Insert the raid first to get the RaidId
            Raids vRaid = new Raids()
            {
                Name = pModel.Name,
                Timestamp = pModel.Timestamp,
                IdPool = pModel.Pool.IdPool,
                UpdatedBy = pModel.UpdatedBy,
                UpdatedTimestamp = pModel.UpdatedTimestamp,
                ClientId = pClientId
            };
            pDBContext.Add(vRaid);
            pDBContext.SaveChanges();
            //Set Raid Id to the Model we return to client for future updates
            return vRaid.IdRaid;
        }
        /// <summary>
        /// Inserts the raid items into the database associated with the raid passed in
        /// </summary>
        /// <param name="pDBContext">The database context to insert items to</param>
        /// <param name="pModel">The raid model containing the items</param>
        /// <param name="pCharacterModels">A pre-populated character list so we dont have to look them all up on the fly</param>
        public static void InsertRaidItems(opendkpContext pDBContext,string pClientId, dynamic pModel, Dictionary<string, Characters> pCharacterModels)
        {
            //Lets insert the items from this raid
            foreach (var vItem in pModel.Items)
            {
                string vCharName = vItem.CharacterName;
                Items vNewItem;
                if (vItem.ItemID != null)
                    vNewItem = new Items() { IdItem = vItem.ItemID, Name = vItem.ItemName };
                else
                    vNewItem = new Items() { IdItem = -1, Name = vItem.ItemName };

                vNewItem = ItemHelper.CreateItemIfNeeded(pDBContext, vNewItem);
                ItemsXCharacters vTransaction = new ItemsXCharacters()
                {
                    Dkp = vItem.DkpValue,
                    CharacterId = pCharacterModels[vCharName.ToLower()].IdCharacter,
                    ItemId = vNewItem.IdItem,
                    RaidId = pModel.IdRaid,
                    ClientId = pClientId
                };
                pDBContext.Add(vTransaction);
            }
        }

        /// <summary>
        /// Gets All of the CharacterModels associated to the raid via attendance via Raid.Ticks
        /// </summary>
        /// <param name="pDBContext">The database context to retrieve the character models from</param>
        /// <param name="pModel">The raid model containing Ticks and Attendees</param>
        /// <returns></returns>
        public static Dictionary<string, Characters> GetCharacterModels(opendkpContext pDBContext,string pClientId, dynamic pModel)
        {
            var vReturnModel = new Dictionary<string, Characters>();
            foreach (var vItem in pModel.Items)
            {
                string vCharacterName = vItem.CharacterName;
                vItem.CharacterName = vCharacterName = vCharacterName.Split(' ')[0];
                if (!vReturnModel.ContainsKey(vCharacterName.ToLower()))
                    vReturnModel.Add(vCharacterName.ToLower(),
                        CharacterHelper.CreateCharacterIfNeeded(pDBContext, pClientId, vCharacterName));
            }

            foreach (var vTick in pModel.Ticks)
            {
                string[] vAttendeeArray = vTick.Attendees.ToObject<string[]>();
                for (int i=0; i< vAttendeeArray.Length; i++)
                {
                    string vAttendee = vAttendeeArray[i].Split(' ')[0];
                    vTick.Attendees[i] = vAttendee.Trim();
                    if (!vReturnModel.ContainsKey(vAttendee.ToLower()))
                        vReturnModel.Add(vAttendee.ToLower(),
                            CharacterHelper.CreateCharacterIfNeeded(pDBContext, pClientId, vAttendee));
                }
            }
            return vReturnModel;
        }

        /// <summary>
        /// Inserts the raid ticks
        /// </summary>
        /// <param name="pDBContext">The database context to insert the raid ticks to</param>
        /// <param name="pModel">The raid model containing the Ticks</param>
        /// <param name="pCharacterModels">The character models should already be prepopulated and passed in</param>
        public static void InsertRaidTicks(opendkpContext pDBContext, string pClientId, dynamic pModel, Dictionary<string, Characters> pCharacterModels)
        {
            //Insert each Tick and Add Raid Attendees
            foreach (var vModelTick in pModel.Ticks)
            {
                Ticks vTick = new Ticks()
                {
                    Description = vModelTick.Description,
                    Value = vModelTick.Value,
                    RaidId = pModel.IdRaid,
                    ClientId = pClientId
                };
                pDBContext.Add(vTick);

                //Make sure we don't have duplicate attendees for each tick
                //client should handle this but we'll verify here
                string[] vAttendeeArray = vModelTick.Attendees.ToObject<string[]>();
                vAttendeeArray = vAttendeeArray.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                //Add Attendees to Tick
                foreach (string vAttendee in vAttendeeArray)
                {
                    pDBContext.Add(
                        new TicksXCharacters()
                        {
                            IdCharacterNavigation = pCharacterModels[vAttendee.ToLower()],
                            IdTickNavigation = vTick,
                            ClientId = pClientId
                        });
                }
            }
        }
        /// <summary>
        /// Creates an individual tick
        /// </summary>
        /// <param name="pDBContext">The database context to store the tick</param>
        /// <param name="pRaidId">The raid id the tick should be associated with</param>
        /// <param name="pModel">The raid model the tick is associated with</param>
        /// <param name="pCharacterModels">Prepopulated character models</param>
        public static void CreateTick(opendkpContext pDBContext, string pClientId, int pRaidId, dynamic pModel, Dictionary<string, Characters> pCharacterModels)
        {

            Ticks vTick = new Ticks()
            {
                Description = pModel.Description,
                Value = pModel.Value,
                RaidId = pRaidId,
                ClientId = pClientId
            };

            //Make sure we don't have duplicate attendees for each tick
            //client should handle this but we'll verify here
            string[] vAttendeeArray = pModel.Attendees.ToObject<string[]>();
            vAttendeeArray = vAttendeeArray.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            
            //Add Attendees to Tick
            foreach (string vAttendee in vAttendeeArray)
            {
                pDBContext.Add(
                    new TicksXCharacters()
                    {
                        IdCharacterNavigation = pCharacterModels[vAttendee.ToLower()],
                        IdTickNavigation = vTick,
                        ClientId = pClientId
                    });
            }
        }
        /// <summary>
        /// Removes duplicate attendees
        /// </summary>
        /// <param name="vAttendees">String array of attendees, removes duplicates</param>
        /// <returns></returns>
        private static string[] RemoveDuplicateAttendees(string[] vAttendees)
        {
            return vAttendees.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        /// <summary>
        /// Deletes a raid based off the raid id
        /// </summary>
        /// <param name="pDBContext">The database context in which to delete the raid</param>
        /// <param name="pRaidId">The raid id to delete</param>
        /// <returns></returns>
        public static Raids DeleteRaid(opendkpContext pDBContext, int pRaidId, string pClientId)
        {
            Raids vResult;
            vResult = pDBContext.Raids
                .FirstOrDefault<Raids>(x => x.ClientId.Equals(pClientId) && x.IdRaid == pRaidId);
            if (vResult != null)
            {
                pDBContext.Raids.Remove(vResult);
            }

            return vResult;
        }
    }
}
