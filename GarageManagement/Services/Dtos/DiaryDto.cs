namespace GarageManagement.Services.Dtos
{
    public class DiaryDto
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime DateCreate { get; set; }
        public string? Title { get; set; }
        public string? Operation { get; set; }
        public string? Table { get; set; }
        public bool IsSuccess { get; set; }
        public Guid WithId { get; set; }
    }
}
