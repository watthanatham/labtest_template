using System.ComponentModel.DataAnnotations;

namespace TemplateProject.Models;

public class SampleItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
