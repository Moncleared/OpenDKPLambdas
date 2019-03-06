using OpenDKPShared.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDKPShared.ClientModels
{
    public class RaidModel
    {
        public RaidModel(Raids pRaid)
        {
            this.IdRaid = pRaid.IdRaid;
            this.Pool = new PoolModel(pRaid.IdPoolNavigation);
            this.Name = pRaid.Name;
            this.Timestamp = pRaid.Timestamp;
            this.Ticks = new List<TickModel>();
        }
        public int IdRaid { get; set; }

        public int TotalTicks { get; set; }

        public PoolModel Pool { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public List<TickModel> Ticks { get; set; }
    }

    public class TickModel
    {
        public TickModel(Ticks pTick)
        {
            this.Id = pTick.TickId;
            this.RaidId = pTick.RaidId;
            this.Value = pTick.Value;
            this.Description = pTick.Description;
        }
        public int Id { get; set; }
        public int RaidId { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
    }

    public class PoolModel
    {
        public PoolModel(Pools pPool)
        {
            this.Id = pPool.IdPool;
            this.Name = pPool.Name;
            this.Description = pPool.Description;
            this.Order = pPool.Order;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }
}
