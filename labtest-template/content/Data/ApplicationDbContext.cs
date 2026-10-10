using Microsoft.EntityFrameworkCore;
using TemplateProject.Models;

namespace TemplateProject.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<SampleItem> SampleItems => Set<SampleItem>();
}
