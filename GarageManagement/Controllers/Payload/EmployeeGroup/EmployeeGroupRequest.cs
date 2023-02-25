namespace GarageManagement.Controllers.Payload.EngineerGroup
{
    public class EmployeeGroupRequest
    {
        public Guid? Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupCode { get; set; }
    }
}
