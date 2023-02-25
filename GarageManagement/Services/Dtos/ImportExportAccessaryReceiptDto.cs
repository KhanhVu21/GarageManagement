namespace GarageManagement.Services.Dtos
{
    public class ImportExportAccessaryReceiptDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid? IdAccessary { get; set; }
        public Guid? IdEmployee { get; set; }
        public string? DescriptionIEX { get; set; }
        public float? TotalMoney { get; set; }
        public int? ImportExport { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
