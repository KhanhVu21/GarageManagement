namespace GarageManagement.Services.Common.Model
{
    public class TemplateApi
    {
        public object? Payload { get; set; }
        public object[]? ListPayload { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public bool Fail { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalElement { get; set; }
        public int TotalPages { get; set; }
    }
}
