namespace GarageManagement.Controllers.Payload.ImportExportAccessaryReceipt
{
    public class ImportExportAccessaryReceiptModel
    {
        public Guid? Id { get; set; }
        public Guid? IdAccessary { get; set; }
        public Guid? IdEmployee { get; set; }
        public string? DescriptionIEX { get; set; }
        public float? TotalMoney { get; set; }
        public int? ImportExport { get; set; }
    }
}
