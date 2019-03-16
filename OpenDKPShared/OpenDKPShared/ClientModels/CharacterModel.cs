using OpenDKPShared.DBModels;
using System;

namespace OpenDKPShared.ClientModels
{
    public class CharacterModel
    {
        public CharacterModel(Characters pCharacter)
        {
            this.Name = pCharacter.Name;
            this.Rank = pCharacter.Rank;
            this.Class = pCharacter.Class;
            this.Level = pCharacter.Level;
            this.Race = pCharacter.Race;
            this.Gender = pCharacter.Gender;
            this.Guild = pCharacter.Guild;
            this.Active = pCharacter.Active;
            this.MainChange = pCharacter.MainChange;
        }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Class { get; set; }
        public int? Level { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string Guild { get; set; }

        public DateTime? MainChange { get; set; }

        public sbyte Active { get; set; }
    }
}
