using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Clients
    {
        public Clients()
        {
            Adjustments = new HashSet<Adjustments>();
            AdminSettings = new HashSet<AdminSettings>();
            Audit = new HashSet<Audit>();
            Characters = new HashSet<Characters>();
            ItemsXCharacters = new HashSet<ItemsXCharacters>();
            Raids = new HashSet<Raids>();
            Ticks = new HashSet<Ticks>();
            TicksXCharacters = new HashSet<TicksXCharacters>();
            UserRequests = new HashSet<UserRequests>();
            UserXCharacter = new HashSet<UserXCharacter>();
        }

        public string ClientId { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public string Identity { get; set; }
        public string UserPool { get; set; }
        public string WebClientId { get; set; }
        public string AssumedRole { get; set; }
        public string Website { get; set; }
        public string Forums { get; set; }

        public ICollection<Adjustments> Adjustments { get; set; }
        public ICollection<AdminSettings> AdminSettings { get; set; }
        public ICollection<Audit> Audit { get; set; }
        public ICollection<Characters> Characters { get; set; }
        public ICollection<ItemsXCharacters> ItemsXCharacters { get; set; }
        public ICollection<Raids> Raids { get; set; }
        public ICollection<Ticks> Ticks { get; set; }
        public ICollection<TicksXCharacters> TicksXCharacters { get; set; }
        public ICollection<UserRequests> UserRequests { get; set; }
        public ICollection<UserXCharacter> UserXCharacter { get; set; }
    }
}
