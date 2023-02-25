namespace GarageManagement.Services.Common.Model
{
    public class SaveDiaryModel
    {
        public Guid IdUserCurrent { get; set; }
        public Guid IdWith { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

    }
}
