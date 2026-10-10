# คู่มือฐานข้อมูล SQLite, Model และ CRUD

โปรเจกต์ตั้งค่า `ApplicationDbContext` ใน `Program.cs` ผ่าน `AddDbContext` และอ่าน `ConnectionStrings:DefaultConnection` จาก `appsettings.json` ค่าเริ่มต้นคือ `Data Source=TemplateProject.db` ชื่อไฟล์นี้ถูกแทนตามชื่อโปรเจกต์เมื่อใช้ `dotnet new labmvc -n ...` และใช้ path สัมพัทธ์กับโฟลเดอร์โปรเจกต์อย่างคงที่

ชื่อไฟล์ในคู่มือนี้อ้างจาก **โฟลเดอร์รากของโปรเจกต์ที่สร้าง** (โฟลเดอร์เดียวกับไฟล์ `.csproj`) หากกำลังแก้ต้นแบบ ให้เติม `labtest-template/content/` หน้าชื่อไฟล์ ชื่อ `TemplateProject` ในตัวอย่างจะถูกแทนด้วยชื่อโปรเจกต์จริง เช่น `MyExam`

## แผนที่ไฟล์: ต้องแก้อะไรที่ไหน

| งาน | ไฟล์ | สิ่งที่แก้ |
| --- | --- | --- |
| เลือก SQLite provider | `TemplateProject.csproj` | มีแพ็กเกจ `Microsoft.EntityFrameworkCore.Sqlite` และ `Microsoft.EntityFrameworkCore.Design` อยู่แล้ว |
| เปลี่ยนชื่อ/ที่อยู่ไฟล์ฐานข้อมูล | `appsettings.json` | แก้ `ConnectionStrings:DefaultConnection` เช่น `Data Source=MyExam.db` |
| เปิดการเชื่อมต่อและรัน migration ตอนเริ่มเว็บ | `Program.cs` | อ่าน connection string, ลงทะเบียน `AddDbContext<ApplicationDbContext>(...UseSqlite(...))`, และ `MigrateAsync()` ใน Development |
| สร้างหรือแก้ตาราง | `Models/SampleItem.cs` หรือไฟล์ Model ใหม่ และ `Data/ApplicationDbContext.cs` | แก้ property ของ Model และเพิ่ม `DbSet<T>` สำหรับ entity ใหม่ |
| บันทึกการเปลี่ยน schema | `Migrations/` | สร้างด้วย `dotnet ef migrations add <ชื่อ>`; ตรวจไฟล์ที่สร้างก่อน `dotnet ef database update` |
| เขียนคำสั่งเรียก table | `Services/SampleItemRepository.cs` | มีตัวอย่าง CRUD ด้วย LINQ lambda และ Raw SQL; เพิ่มเมธอดสำหรับ table ใหม่ได้ |
| ส่งข้อมูลไปหน้าเว็บ | `Controllers/` และ `Views/<ชื่อ Controller>/` | inject `ApplicationDbContext` หรือ repository ใน Controller; สร้าง Razor view เมื่อต้องแสดงหน้าใหม่ |
| ใช้ฐานข้อมูลผ่าน CLI | `dotnet-tools.json` | ระบุ `dotnet-ef`; รัน `dotnet tool restore` ก่อนใช้ `dotnet ef` ครั้งแรก |
| ใช้ database view | `Models/` + `Data/ApplicationDbContext.cs` + `Migrations/` | สร้างชนิดข้อมูลสำหรับผลลัพธ์, แมป `HasNoKey().ToView(...)`, สร้าง view ด้วย SQL ใน migration |
| ใช้ Stored Procedure | **SQLite ใช้ไม่ได้**; ดูหัวข้อท้ายคู่มือสำหรับ SQL Server | เมื่อย้าย provider จึงแก้ `.csproj`, `appsettings.json`, `Program.cs`, `Migrations/` และโค้ดเรียกใน `Services/` |

**เริ่มจากฐานข้อมูลที่เตรียมไว้:** เปิด Terminal ในโฟลเดอร์โปรเจกต์แล้วรัน `dotnet tool restore`, `dotnet ef database update` จากนั้นดูไฟล์ `<ชื่อโปรเจกต์>.db` ในโฟลเดอร์เดียวกัน ตัวอย่าง `SampleItems` มี migration ให้แล้ว การรันเว็บใน Development จะทำ `database update` ให้อัตโนมัติด้วย

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

## สร้างและเรียกใช้ Database View บน SQLite

View ในฐานข้อมูลเป็นผลลัพธ์สำหรับอ่านข้อมูล ตัวอย่างนี้สร้าง `vw_SampleItems` จากตาราง `SampleItems` โดยไม่เปลี่ยนหน้า MVC เริ่มต้น ต้องแก้ 3 จุดในโปรเจกต์:

1. สร้าง `Models/SampleItemView.cs` สำหรับคอลัมน์ที่ view คืน:

   ```csharp
   namespace TemplateProject.Models;

   public class SampleItemView
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
   }
   ```

2. ใน `Data/ApplicationDbContext.cs` เพิ่ม `DbSet` และแมปเป็น keyless view (เพิ่ม `using TemplateProject.Models;` มีอยู่แล้ว):

   ```csharp
   public DbSet<SampleItemView> SampleItemViews => Set<SampleItemView>();

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<SampleItemView>()
           .HasNoKey()
           .ToView("vw_SampleItems");
   }
   ```

   วางเมธอด `OnModelCreating` ภายในคลาส `ApplicationDbContext` ถ้าคลาสนี้มีเมธอดดังกล่าวอยู่แล้ว ให้เพิ่มเฉพาะคำสั่ง `modelBuilder.Entity<SampleItemView>()...` ลงในเมธอดเดิม

3. จากโฟลเดอร์โปรเจกต์รัน `dotnet ef migrations add AddSampleItemsView` แล้วเปิด `Migrations/<timestamp>_AddSampleItemsView.cs` ที่สร้างใหม่ เติม SQL ใน `Up` และ `Down`:

   ```csharp
   protected override void Up(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.Sql(
           @"CREATE VIEW ""vw_SampleItems"" AS SELECT ""Id"", ""Name"" FROM ""SampleItems"";");
   }

   protected override void Down(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.Sql(@"DROP VIEW ""vw_SampleItems"";");
   }
   ```

   `ToView(...)` บอก EF Core ว่าชนิดข้อมูลนี้แมปกับ view ที่มีอยู่แล้ว จึง **ไม่สร้าง view ให้เอง** ต้องใส่ SQL ใน migration ดังตัวอย่าง หาก view ถูกสร้างไว้ในฐานข้อมูลอยู่แล้ว ให้แมปชื่อให้ตรงและอย่าสั่งสร้างซ้ำ

รัน `dotnet ef database update` แล้วอ่านข้อมูลผ่าน `DbContext` หรือเพิ่มเมธอดใน `Services/SampleItemRepository.cs`:

```csharp
var rows = await db.SampleItemViews.AsNoTracking()
    .Where(x => x.Name.Contains("test"))
    .OrderBy(x => x.Name)
    .ToListAsync();
```

ถ้าจะให้หน้าเว็บแสดงผล ให้ inject `ApplicationDbContext` หรือ repository ใน `Controllers/<ชื่อ Controller>Controller.cs` แล้วสร้าง `Views/<ชื่อ Controller>/<ชื่อ Action>.cshtml` สำหรับแสดงรายการ ตรวจ view ด้วย `sqlite3 <ชื่อโปรเจกต์>.db 'SELECT * FROM vw_SampleItems;'` ได้หากติดตั้ง SQLite CLI แล้ว การแก้ข้อมูลให้ทำที่ `SampleItems` ผ่าน entity ตาราง เพราะ keyless view นี้เป็นแบบอ่านอย่างเดียว

## Stored Procedure

**SQLite ไม่มี Stored Procedure ให้สร้างหรือเรียกด้วย `CREATE PROCEDURE` / `EXEC`** ดังนั้นโปรเจกต์นี้ไม่สามารถสาธิตการเรียก Stored Procedure บน SQLite จริงได้ ใช้เมธอดใน repository หรือ Raw SQL แบบ parameterized ข้างบนแทน เพื่อคง SQLite ตามข้อกำหนด

หากอนาคตเปลี่ยนไปใช้ **SQL Server** ซึ่งรองรับ Stored Procedure ให้แก้ไฟล์ในโปรเจกต์ที่ใช้ SQL Server ตามลำดับนี้:

1. `TemplateProject.csproj`: เพิ่มแพ็กเกจ `Microsoft.EntityFrameworkCore.SqlServer` รุ่นเดียวกับ EF Core ที่ใช้อยู่ (เช่น `dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.11`) หากเลิกใช้ SQLite ให้เอาแพ็กเกจ SQLite ออก
2. `appsettings.json`: เปลี่ยน `ConnectionStrings:DefaultConnection` เป็นค่าที่ชี้ไป SQL Server; เก็บรหัสผ่านใน user secrets หรือ environment variable `ConnectionStrings__DefaultConnection` แทนการ commit ลง Git
3. `Program.cs`: เอา `SqliteConnectionStringBuilder` และการแปลง path ของไฟล์ `.db` ออก แล้วเปลี่ยนการลงทะเบียนเป็น `builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuredConnection));`
4. `Migrations/`: migrations ของ SQLite ใช้กับ SQL Server โดยตรงไม่ได้ ให้สร้างชุด migrations สำหรับ SQL Server แยกจากชุดเดิม และจัดให้แอปเลือกชุดที่ตรงกับ provider จากนั้นเพิ่ม migration สำหรับสร้าง Stored Procedure; `migrationBuilder.Sql(...)` ใช้ SQL Server script ใน `Up` และ `DROP PROCEDURE` ใน `Down`
5. `Services/SampleItemRepository.cs` หรือ service ใหม่: ใส่คำสั่ง `FromSqlInterpolated` สำหรับ procedure ที่คืนแถว และ `ExecuteSqlInterpolatedAsync` สำหรับ procedure ที่แก้ข้อมูล; Controller เรียก service ผ่าน constructor เช่นเดียวกับ table

เมื่อมีชุด SQL Server migration ที่ถูกต้องแล้ว รัน `dotnet ef migrations add AddGetSampleItemProcedure` เปิดไฟล์ใหม่ใน `Migrations/` และเพิ่มโค้ดลักษณะนี้ใน `Up`/`Down` (ตัวอย่างเฉพาะ SQL Server):

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE dbo.GetSampleItem @Id int
        AS
        BEGIN
            SET NOCOUNT ON;
            SELECT Id, Name FROM dbo.SampleItems WHERE Id = @Id;
        END");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.GetSampleItem");
}
```

จากนั้นรัน `dotnet ef database update` กับฐานข้อมูล SQL Server เมื่อต้องสร้าง procedure อื่น ให้เพิ่ม SQL ใน migration ตามรูปแบบเดียวกัน

ตัวอย่าง SQL ที่รันในเครื่องมือของ SQL Server เพื่อสร้าง procedure:

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

Procedure ต้องมีอยู่ใน SQL Server ก่อน รัน SQL ข้างต้นผ่านเครื่องมือของ SQL Server โดย `GO` เป็นตัวคั่น batch สำหรับเครื่องมือดังกล่าว **อย่าใส่ `GO` ใน `migrationBuilder.Sql(...)`** เพราะไม่ใช่คำสั่ง SQL Server หากต้องเก็บ procedure ใน migration ให้แยกแต่ละ procedure เป็นการเรียก `migrationBuilder.Sql(...)` คนละครั้ง การเรียก Stored Procedure ของ SQL Server ไม่ควรต่อ LINQ ที่ทำให้ EF สร้าง subquery เหนือคำสั่ง `EXEC` ให้ materialize ด้วย `ToListAsync()` ก่อน

อ้างอิง: [EF Core SQL queries](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries), [EF Core keyless entity types](https://learn.microsoft.com/en-us/ef/core/modeling/keyless-entity-types), [EF Core entity view mapping](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types#view-mapping), [EF Core SQLite limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations), [SQLite forum: stored procedures](https://sqlite.org/forum/forumpost/a45b0b6676)
