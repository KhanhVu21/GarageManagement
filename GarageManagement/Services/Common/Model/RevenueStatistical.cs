namespace GarageManagement.Services.Common.Model
{
    public class RevenueStatistical
    {
        public int TotalVehicle { get; set; } 
        public int ProcessingVehicle { get; set; } 
        public int CompletedVehicle { get; set; }
        public double Revenue { get; set; }
        public double Repair { get; set; }
        public double Sell { get; set; }
        public double Debt { get; set; }
        public double Receivables { get; set; }
        public double Payment { get; set; }
    }
}
