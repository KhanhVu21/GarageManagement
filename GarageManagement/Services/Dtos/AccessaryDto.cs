namespace GarageManagement.Services.Dtos
{
    public class AccessaryDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? AccessaryGroup { get; set; }
        public string? UnitName { get; set; }
        public float? PriceImport { get; set; }
        public float? PriceExport { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Status { get; set; }
        public float? Inventory { get; set; }
        public float? InventoryAlert { get; set; }
        public Guid? GroupID { get; set; }

        //extention
        public Guid IdUserCurrent { get; set; }
    }
}
