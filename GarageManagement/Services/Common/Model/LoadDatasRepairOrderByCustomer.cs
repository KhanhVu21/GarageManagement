namespace GarageManagement.Services.Common.Model
{
    public class LoadDatasRepairOrderByCustomer
    {
        public Guid? IdCustomer { get; set; }
        public string? NameCustomer { get; set; }
        public string? Code { get; set; }
        public bool? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AddressCus { get; set; }
        public Guid? IdCity { get; set; }
        public Guid? IdDistrict { get; set; }
        public Guid? IdWard { get; set; }
        public string? TaxNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? NoteCus { get; set; }
        public string? Avatar { get; set; }
        public bool? TypeOfCustomer { get; set; }
        public Guid? IdGroup { get; set; }

        public Guid? IdRepairOrder { get; set; }
        public int? Kilometer { get; set; }
        public int? TaxPercent { get; set; }
        public float? TotalMoneyRepair { get; set; }      
        public string? RO_RepairOdersCode { get; set; }
        public DateTime? CreatedDate { get; set; }

        public Guid? IdDebt { get; set; }
        public string? DebtContent { get; set; }
        public float? Deposit { get; set; }
        public float? TotalPay { get; set; }
        public float? DebtNumber { get; set; }
        public bool? LastPay { get; set; }

        public Guid? IdRequest { get; set; }
        public string? RequestContent { get; set; }
        public bool? IsProcessing { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCanceled { get; set; }
        public string? Note { get; set; }

        public Guid? IdRepairOrdersAccessary { get; set; }
        public Guid? IdCategoryAccessary { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
        public float? TotalMoneyAcs { get; set; }

        public Guid? IdRepairOrdersEmployee { get; set; }
        public Guid? IdEmployee { get; set; }

        public Guid? IdOrtherCost { get; set; }
        public string? CostContent { get; set; }
        public float? QuantityOrt { get; set; }
        public float? PriceOrt { get; set; }
        public float? TotalMoneyOrt { get; set; }
    }
    public class LoadDatasRepairOrderByCustomerGr
    {
        public Guid? IdCustomer { get; set; }
        public string? NameCustomer { get; set; }
        public string? Code { get; set; }
        public bool? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AddressCus { get; set; }
        public Guid? IdCity { get; set; }
        public Guid? IdDistrict { get; set; }
        public Guid? IdWard { get; set; }
        public string? TaxNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? NoteCus { get; set; }
        public string? Avatar { get; set; }
        public bool? TypeOfCustomer { get; set; }
        public Guid? IdGroup { get; set; }

        public IEnumerable<RcRepairOrders>? rcRepairOrders { get; set; }
    }
    public record RcRepairOrders
    {
        public Guid? IdRepairOrder { get; set; }
        public int? Kilometer { get; set; }
        public int? TaxPercent { get; set; }
        public float? TotalMoneyRepair { get; set; }
        public string? RO_RepairOdersCode { get; set; }
        public DateTime? CreatedDate { get; set; }

        public IEnumerable<RcDebt>? rcDebts { get; set; }
        public IEnumerable<RcRequestList>? rcRequestLists { get; set; }
        public IEnumerable<RcRepairOrdersAcs>? rcRepairOrdersAcs { get; set; }
        public IEnumerable<RcRepairOrdersEmp>? rcRepairOrdersEmps { get; set; }
        public IEnumerable<RcOrtherCost>? rcOrtherCosts { get; set; }
    }
    public record RcDebt
    {
        public Guid? IdDebt { get; set; }
        public string? DebtContent { get; set; }
        public float? Deposit { get; set; }
        public float? TotalPay { get; set; }
        public float? DebtNumber { get; set; }
        public bool? LastPay { get; set; }
    }
    public record RcRequestList
    {
        public Guid? IdRequest { get; set; }
        public string? RequestContent { get; set; }
        public bool? IsProcessing { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCanceled { get; set; }
        public string? Note { get; set; }
    }
    public record RcRepairOrdersAcs
    {
        public Guid? IdRepairOrdersAccessary { get; set; }
        public Guid? IdCategoryAccessary { get; set; }
        public float? Quantity { get; set; }
        public float? Price { get; set; }
        public int? DiscountPercent { get; set; }
        public float? DiscountPrice { get; set; }
        public float? TotalMoneyAcs { get; set; }
    }
    public record RcRepairOrdersEmp
    {
        public Guid? IdRepairOrdersEmployee { get; set; }
        public Guid? IdEmployee { get; set; }
    }
    public record RcOrtherCost
    {
        public Guid? IdOrtherCost { get; set; }
        public string? CostContent { get; set; }
        public float? QuantityOrt { get; set; }
        public float? PriceOrt { get; set; }
        public float? TotalMoneyOrt { get; set; }
    }
}
