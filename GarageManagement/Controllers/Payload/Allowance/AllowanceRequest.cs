namespace GarageManagement.Controllers.Payload.Allowance
{
    public class AllowanceRequest
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public float? Amount { get; set; }
    }
}
