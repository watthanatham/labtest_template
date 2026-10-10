# คู่มือฐานข้อมูล SQLite, Model และ CRUD

โปรเจกต์ตั้งค่า `ApplicationDbContext` ใน `Program.cs` ผ่าน `AddDbContext` และอ่าน `ConnectionStrings:DefaultConnection` จาก `appsettings.json` ค่าเริ่มต้นคือ `Data Source=TemplateProject.db` ชื่อไฟล์นี้ถูกแทนตามชื่อโปรเจกต์เมื่อใช้ `dotnet new labmvc -n ...` และใช้ path สัมพัทธ์กับโฟลเดอร์โปรเจกต์อย่างคงที่

เมื่อรันใน `Development` แอปเรียก `Database.MigrateAsync()` เพื่อสร้างหรืออัปเดตฐานข้อมูลอัตโนมัติ สำหรับ environment อื่นให้รัน `dotnet ef database update` ก่อนเปิดเว็บ ฐานข้อมูลอยู่ในไฟล์ `.db` และไม่ควร commit ขึ้น Git

## เรียกใช้ DbContext ใน Controller

EF Core ส่ง `ApplicationDbContext` เข้าทาง constructor โดยอัตโนมัติหลังลงทะเบียนใน `Program.cs` ตัวอย่าง:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateProject.Data;
using TemplateProject.Models;

public class ItemsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await db.SampleItems.AsNoTracking()
            .OrderBy(x => x.Name).ToListAsync();
        return View(items); // ต้องสร้าง Views/Items/Index.cshtml เองเมื่อจะใช้หน้าเว็บนี้
    }
}
```

หากต้องการแยกโค้ดฐานข้อมูลออกจาก Controller ให้ inject `SampleItemRepository` ที่เตรียมไว้ใน `Services/` แทน บริการนี้ลงทะเบียนด้วย `AddScoped` แล้ว:

```csharp
public class ItemsController(SampleItemRepository items) : Controller
{
    public async Task<IActionResult> Search(string term) =>
        Json(await items.SearchAsync(term));
}
```

ตัวอย่างสองชุดด้านล่างใช้งานได้กับตาราง `SampleItems` ที่ migration เริ่มต้นสร้างไว้ `SampleItem` มี `Id` และ `Name` โดย `Name` ต้องไม่ว่างและไม่เกิน 100 ตัวอักษร ตรวจสอบข้อมูลก่อนบันทึกจากผู้ใช้จริง

## CRUD ด้วย LINQ lambda + EF Core

ไฟล์ `Services/SampleItemRepository.cs` มีเมธอดเหล่านี้ครบแล้ว:

```csharp
// Create: SaveChangesAsync บันทึกลง SQLite และเติม Id ให้ entity
var item = new SampleItem { Name = "First item" };
db.SampleItems.Add(item);
await db.SaveChangesAsync();

// Read one / list / filter / sort: lambda expressions
var one = await db.SampleItems.AsNoTracking()
    .FirstOrDefaultAsync(x => x.Id == item.Id);
var list = await db.SampleItems.AsNoTracking()
    .Where(x => x.Name.Contains("First"))
    .OrderBy(x => x.Name)
    .ToListAsync();

// Update: ต้องดึง entity ให้ EF track ก่อนแก้
var tracked = await db.SampleItems.FindAsync(item.Id);
if (tracked is not null)
{
    tracked.Name = "Changed";
    await db.SaveChangesAsync();
}

// Delete
var deleting = await db.SampleItems.FindAsync(item.Id);
if (deleting is not null)
{
    db.SampleItems.Remove(deleting);
    await db.SaveChangesAsync();
}
```

สำหรับเมธอดที่มีอยู่ ใช้ `CreateAsync`, `GetByIdAsync`, `SearchAsync`, `UpdateAsync`, `DeleteAsync` บน `SampleItemRepository` ได้เลย

## CRUD ด้วย Raw SQL ที่ส่งพารามิเตอร์

เมธอด `CreateRawAsync`, `GetByIdRawAsync`, `SearchRawAsync`, `UpdateRawAsync`, `DeleteRawAsync` ใน repository ใช้โค้ดรูปแบบนี้:

```csharp
var name = "Raw item";
var id = 1;

// Create: คืนจำนวนแถวที่เปลี่ยน (โดยปกติ 1)
var inserted = await db.Database.ExecuteSqlInterpolatedAsync(
    $"INSERT INTO \"SampleItems\" (\"Name\") VALUES ({name})");

// Read: SELECT ต้องคืนทุกคอลัมน์ของ SampleItem คือ Id และ Name
var one = await db.SampleItems.FromSqlInterpolated(
        $"SELECT \"Id\", \"Name\" FROM \"SampleItems\" WHERE \"Id\" = {id}")
    .AsNoTracking().FirstOrDefaultAsync();

var pattern = "%Raw%";
var list = await db.SampleItems.FromSqlInterpolated(
        $"SELECT \"Id\", \"Name\" FROM \"SampleItems\" WHERE \"Name\" LIKE {pattern}")
    .AsNoTracking().ToListAsync();

// Update / Delete: คืนจำนวนแถวที่เปลี่ยน; 0 คือไม่พบ id
var updated = await db.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE \"SampleItems\" SET \"Name\" = {name} WHERE \"Id\" = {id}");
var deleted = await db.Database.ExecuteSqlInterpolatedAsync(
    $"DELETE FROM \"SampleItems\" WHERE \"Id\" = {id}");
```

`FromSqlInterpolated` และ `ExecuteSqlInterpolatedAsync` ส่งค่าที่แทรก `{...}` เป็น SQL parameters จึงใช้กับค่าจากผู้ใช้ได้ ห้ามต่อ string จากผู้ใช้เพื่อสร้าง SQL เอง ชื่อ table/column ไม่สามารถแทนด้วย SQL parameter ได้ ถ้าต้องเปลี่ยนชื่อแบบ dynamic ต้องจำกัดไว้ในรายการที่อนุญาตเท่านั้น Raw SQL ที่แก้ข้อมูลไม่อัปเดต entity ที่ EF กำลัง track ใน `DbContext` เดียวกัน ให้ query ใหม่หรือใช้ context ใหม่เมื่อจำเป็น ถ้าหลายคำสั่งต้องสำเร็จพร้อมกัน ให้ครอบ transaction ด้วย `await using var tx = await db.Database.BeginTransactionAsync(); ... await tx.CommitAsync();`

## แก้ Model และอัปเดตฐานข้อมูล

ตัวอย่างเพิ่มคอลัมน์ `Description` ใน `Models/SampleItem.cs`:

```csharp
public string? Description { get; set; }
```

เปิด Terminal ในโฟลเดอร์โปรเจกต์ แล้วรัน:

```sh
dotnet tool restore
dotnet ef migrations add AddSampleItemDescription
dotnet ef database update
dotnet build
```

ตรวจไฟล์ใหม่ใน `Migrations/` ก่อนใช้กับข้อมูลจริง คำสั่ง `dotnet ef database update` ใช้ได้ทั้ง Development และ environment อื่น ใน Development การรันเว็บครั้งถัดไปจะ apply migration ที่ยังไม่ใช้ให้อัตโนมัติ จึงไม่จำเป็นต้องรัน `database update` ซ้ำ หลังเพิ่ม entity ใหม่ ให้เพิ่ม `public DbSet<NewModel> NewModels => Set<NewModel>();` ใน `ApplicationDbContext` แล้วทำขั้นตอน migration เช่นเดียวกัน อย่าใช้ `EnsureCreated()` ร่วมกับ migrations

เมื่อเพิ่มคอลัมน์ใน Model ให้ปรับ Raw SQL ที่ `SELECT` entity ด้วย เช่นตัวอย่าง `Description` ต้องเปลี่ยนเป็น `SELECT "Id", "Name", "Description" ...` เพราะ EF Core ต้องได้รับทุกคอลัมน์ที่แมปกับ `SampleItem` และตรวจคำสั่ง `INSERT`/`UPDATE` ว่าต้องจัดการคอลัมน์ใหม่หรือไม่

ตรวจชื่อฐานข้อมูลและตารางจาก command line ได้ด้วย SQLite CLI (ถ้าติดตั้ง):

```sh
sqlite3 TemplateProject.db '.tables'
sqlite3 TemplateProject.db 'SELECT * FROM SampleItems;'
```

ถ้าจะเปลี่ยนชื่อ/ลบคอลัมน์ใน SQLite ให้ตรวจ SQL ที่ EF Core สร้างใน migration เพราะ SQLite มีข้อจำกัดด้านการเปลี่ยน schema และ EF อาจต้องสร้างตารางใหม่

## Stored Procedure

**SQLite ไม่มี Stored Procedure ให้สร้างหรือเรียกด้วย `CREATE PROCEDURE` / `EXEC`** ดังนั้นโปรเจกต์นี้ไม่สามารถสาธิตการเรียก Stored Procedure บน SQLite จริงได้ ใช้เมธอดใน repository หรือ Raw SQL แบบ parameterized ข้างบนแทน เพื่อคง SQLite ตามข้อกำหนด

หากอนาคตเปลี่ยนไปใช้ **SQL Server** ซึ่งรองรับ Stored Procedure ให้เปลี่ยน EF provider เป็น `Microsoft.EntityFrameworkCore.SqlServer`, ใช้ `UseSqlServer` และสร้าง migration สำหรับ provider นั้นใหม่ จากนั้นสร้าง procedure ใน SQL Server เช่น:

```sql
CREATE OR ALTER PROCEDURE dbo.CreateSampleItem @Name nvarchar(100)
AS
BEGIN
    INSERT INTO dbo.SampleItems (Name) VALUES (@Name);
END;
GO

CREATE OR ALTER PROCEDURE dbo.GetSampleItem @Id int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name FROM dbo.SampleItems WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.UpdateSampleItem @Id int, @Name nvarchar(100)
AS
BEGIN
    UPDATE dbo.SampleItems SET Name = @Name WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.DeleteSampleItem @Id int
AS
BEGIN
    DELETE FROM dbo.SampleItems WHERE Id = @Id;
END;
GO
```

เรียก procedure ครบ CRUD ผ่าน EF Core (ใช้บน SQL Server เท่านั้น):

```csharp
await db.Database.ExecuteSqlInterpolatedAsync(
    $"EXEC dbo.CreateSampleItem @Name={name}");

var rows = await db.SampleItems
    .FromSqlInterpolated($"EXEC dbo.GetSampleItem @Id={id}")
    .AsNoTracking()
    .ToListAsync();
var item = rows.FirstOrDefault();

await db.Database.ExecuteSqlInterpolatedAsync(
    $"EXEC dbo.UpdateSampleItem @Id={id}, @Name={name}");

await db.Database.ExecuteSqlInterpolatedAsync(
    $"EXEC dbo.DeleteSampleItem @Id={id}");
```

Procedure ต้องมีอยู่ใน SQL Server ก่อน รัน SQL ข้างต้นผ่านเครื่องมือของ SQL Server โดย `GO` เป็นตัวคั่น batch สำหรับเครื่องมือดังกล่าว การเรียก Stored Procedure ของ SQL Server ไม่ควรต่อ LINQ ที่ทำให้ EF สร้าง subquery เหนือคำสั่ง `EXEC` ให้ materialize ด้วย `ToListAsync()` ก่อน

อ้างอิง: [EF Core SQL queries](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries), [EF Core SQLite limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations), [SQLite forum: stored procedures](https://sqlite.org/forum/forumpost/a45b0b6676)
