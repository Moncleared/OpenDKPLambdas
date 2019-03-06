using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Cache
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string CacheName { get; set; }
        public string CacheValue { get; set; }
        public DateTime CacheExpires { get; set; }
        public DateTime CacheUpdated { get; set; }
    }
}
