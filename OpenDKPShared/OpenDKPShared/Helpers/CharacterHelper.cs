using OpenDKPShared.DBModels;
using System;
using System.Linq;

namespace OpenDKPShared.Helpers
{
    public class CharacterHelper
    {
        /// <summary>
        /// As the name suggests, creates a character if they are needed. Need is determined by whether or not they already exist in the database
        /// </summary>
        /// <param name="pDBContext">The database context to search for the character</param>
        /// <param name="pCharacterName">The character name to search for, case-insensitive</param>
        /// <returns></returns>
        public static Characters CreateCharacterIfNeeded(opendkpContext pDBContext,string pClientId, string pCharacterName)
        {
            Characters vCharacterModel = pDBContext.Characters
                .FirstOrDefault(x => x.Name.Equals(pCharacterName, StringComparison.InvariantCultureIgnoreCase));
            //If the character doesn't exist, we need to create a blank one first
            if (vCharacterModel == null)
            {
                vCharacterModel = new Characters
                {
                    Name = char.ToUpper(pCharacterName[0]) + pCharacterName.Substring(1),
                    Class = "UNKNOWN",
                    Race = "UNKNOWN",
                    Active = 1,
                    Rank = "Inactive",
                    Level = 1,
                    Gender = "Male",
                    Guild = "Original Gangster Club",
                    ClientId = pClientId
                };
                pDBContext.Add(vCharacterModel);
                pDBContext.SaveChanges();
            }

            return vCharacterModel;
        }
        /// <summary>
        /// Fetches a character by their name
        /// </summary>
        /// <param name="pDBContext">The database context in which to search for the character</param>
        /// <param name="pCharacterName">The character name, case-insensitive</param>
        /// <returns></returns>
        public static Characters GetCharacterByName(opendkpContext pDBContext, string pClientId, string pCharacterName)
        {
            Characters vCharacterModel = pDBContext.Characters
                .FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Name.Equals(pCharacterName, StringComparison.InvariantCultureIgnoreCase));
            return vCharacterModel;
        }
    }
}
