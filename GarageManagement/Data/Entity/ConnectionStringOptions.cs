namespace GarageManagement.Data.Entity
{
    public class ConnectionStringOptions
    {
        public const string Position = "ConnectionStrings";
        public string SqlConnection { get; set; } = String.Empty;
    }
}
