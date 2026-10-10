using labtest.Models;
using Microsoft.EntityFrameworkCore;

namespace labtest.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<LabItem> LabItems => Set<LabItem>();
}
