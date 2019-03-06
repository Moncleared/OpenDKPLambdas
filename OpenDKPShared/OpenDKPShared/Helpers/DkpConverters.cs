using OpenDKPShared.DBModels;
using System;
using System.Linq;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// Simple conversion class
    /// </summary>
    public static class DkpConverters
    {
        /// <summary>
        /// Converts a Character Name into the Character Id
        /// </summary>
        /// <param name="pDatabase">The database context in which to look up the character</param>
        /// <param name="pCharacterName">The character name, case-insensitive</param>
        /// <returns></returns>
        public static int CharacterNameToId(opendkpContext pDatabase, string pClientId, string pCharacterName)
        {
            int vCharacterId = -1;
            try
            {
                Characters vModel = pDatabase.Characters
                    .FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Name.Equals(pCharacterName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                vCharacterId = vModel.IdCharacter;
            }
            catch 
            {
                throw new Exception(string.Format("{0} does not exist in the database", pCharacterName));
            }
            return vCharacterId;
        }
    }
}
