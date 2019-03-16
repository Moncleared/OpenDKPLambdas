using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Characters
    {
        public Characters()
        {
            Adjustments = new HashSet<Adjustments>();
            ItemsXCharacters = new HashSet<ItemsXCharacters>();
            TicksXCharacters = new HashSet<TicksXCharacters>();
            UserXCharacter = new HashSet<UserXCharacter>();
        }

        public string ClientId { get; set; }
        public int IdCharacter { get; set; }
        public int IdAssociated { get; set; }
        public sbyte Active { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Class { get; set; }
        public int? Level { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string Guild { get; set; }
        public DateTime? MainChange { get; set; }

        public Clients Client { get; set; }
        public ICollection<Adjustments> Adjustments { get; set; }
        public ICollection<ItemsXCharacters> ItemsXCharacters { get; set; }
        public ICollection<TicksXCharacters> TicksXCharacters { get; set; }
        public ICollection<UserXCharacter> UserXCharacter { get; set; }
    }
}
