using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Controllers.Payload.Unit
{
    public class UnitRequest
    {
        public Guid? Id { get; set; }
        [Required]
        public string UnitName { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        [Required]
        public string UnitCode { get; set; } = string.Empty;
    }
}
