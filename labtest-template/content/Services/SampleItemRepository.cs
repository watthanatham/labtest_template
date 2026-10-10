using Microsoft.EntityFrameworkCore;
using TemplateProject.Data;
using TemplateProject.Models;

namespace TemplateProject.Services;

public class SampleItemRepository(ApplicationDbContext db)
{
    // LINQ / EF Core CRUD
    public async Task<SampleItem> CreateAsync(string name)
    {
        var item = new SampleItem { Name = name };
        db.SampleItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public Task<SampleItem?> GetByIdAsync(int id) =>
        db.SampleItems.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);

    public Task<List<SampleItem>> SearchAsync(string term) =>
        db.SampleItems.AsNoTracking()
            .Where(item => item.Name.Contains(term))
            .OrderBy(item => item.Name)
            .ToListAsync();

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var item = await db.SampleItems.FindAsync(id);
        if (item is null) return false;
        item.Name = name;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await db.SampleItems.FindAsync(id);
        if (item is null) return false;
        db.SampleItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }

    // Parameterized SQLite Raw SQL CRUD
    public Task<int> CreateRawAsync(string name) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"SampleItems\" (\"Name\") VALUES ({name})");

    public Task<SampleItem?> GetByIdRawAsync(int id) =>
        db.SampleItems.FromSqlInterpolated(
                $"SELECT \"Id\", \"Name\" FROM \"SampleItems\" WHERE \"Id\" = {id}")
            .AsNoTracking().FirstOrDefaultAsync();

    public Task<List<SampleItem>> SearchRawAsync(string term)
    {
        var pattern = $"%{term}%";
        return db.SampleItems.FromSqlInterpolated(
                $"SELECT \"Id\", \"Name\" FROM \"SampleItems\" WHERE \"Name\" LIKE {pattern}")
            .AsNoTracking().ToListAsync();
    }

    public Task<int> UpdateRawAsync(int id, string name) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"SampleItems\" SET \"Name\" = {name} WHERE \"Id\" = {id}");

    public Task<int> DeleteRawAsync(int id) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM \"SampleItems\" WHERE \"Id\" = {id}");
}
