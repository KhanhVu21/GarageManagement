namespace GarageManagement.Services.Common.Model
{
    public class GetListUserByUnit
    {
        public string NameUnit { get; set; } = string.Empty;
        public IEnumerable<LstUser> LstUsers { get; set; } = new List<LstUser>();
    }
    public class LstUser
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
    }
}
