namespace OpenDKPShared.ClientModels
{
    public class PlayerSummaryModel
    {
        public PlayerSummaryModel()
        {
            CurrentDKP = 0.0;
        }
        public double CurrentDKP { get; set; }

        public string CharacterName { get; set; }
        public string CharacterClass { get; set; }

        public string CharacterRank { get; set; }

        public string CharacterStatus { get; set; }

        public double AttendedTicks_30 { get; set; }
        public double TotalTicks_30 { get; set; }

        public double Calculated_30 { get; set; }

        public double AttendedTicks_60 { get; set; }
        public double TotalTicks_60 { get; set; }

        public double Calculated_60 { get; set; }

        public double AttendedTicks_90 { get; set; }
        public double TotalTicks_90 { get; set; }

        public double Calculated_90 { get; set; }

        public double AttendedTicks_Life { get; set; }
        public double TotalTicks_Life { get; set; }

        public double Calculated_Life { get; set; }
    }
}
