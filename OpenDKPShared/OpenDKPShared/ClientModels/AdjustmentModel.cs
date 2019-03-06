using OpenDKPShared.DBModels;
using System;

namespace OpenDKPShared.ClientModels
{
    public class AdjustmentModel
    {
        public AdjustmentModel(Adjustments pAdjustment)
        {
            this.Id = pAdjustment.IdAdjustment;
            this.Name = pAdjustment.Name;
            this.Description = pAdjustment.Description;
            this.Value = pAdjustment.Value;
            this.Timestamp = pAdjustment.Timestamp;
            this.Character = new CharacterModel(pAdjustment.IdCharacterNavigation);
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public CharacterModel Character { get; set; }
    }
}
