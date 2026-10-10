using System.ComponentModel.DataAnnotations;

namespace labtest.Models;

public class LabItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรุณากรอกชื่อ")]
    [StringLength(100, ErrorMessage = "ชื่อต้องไม่เกิน 100 ตัวอักษร")]
    [Display(Name = "ชื่อรายการ")]
    public string Name { get; set; } = string.Empty;
}
