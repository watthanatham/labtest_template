# labtest

หากต้องการสร้างโปรเจกต์ใหม่จากต้นแบบนี้ ดู [คู่มือเทมเพลต labmvc](labtest-template/MANUAL.md)

โปรเจกต์ ASP.NET Core MVC (.NET 10) สำหรับฝึกทำแบบทดสอบ เชื่อมต่อ SQLite ด้วย Entity Framework Core 10.0.11

## เริ่มใช้งาน

ต้องติดตั้ง .NET SDK 10 (เครื่องนี้ใช้ 10.0.400) เปิด Terminal ในโฟลเดอร์ที่มี `labtest.sln` แล้วรัน:

```sh
dotnet restore
dotnet tool restore
dotnet run --project labtest --launch-profile http
```

เปิด http://localhost:5196 และเลือก **ทดสอบฐานข้อมูล** เพื่อเพิ่ม ดู และลบรายการ
ใน Development แอปจะสร้างฐานข้อมูลและใช้ migrations อัตโนมัติเมื่อเริ่มทำงาน
ไฟล์ฐานข้อมูลอยู่ที่ `labtest/labtest.db` และถูกยกเว้นจาก Git

## โครงสร้างสำคัญ

- `labtest/Controllers/` รับคำขอและประมวลผล
- `labtest/Models/LabItem.cs` โมเดลตัวอย่างพร้อม validation
- `labtest/Views/` หน้า Razor ของ MVC
- `labtest/Data/ApplicationDbContext.cs` DbContext สำหรับเข้าถึงฐานข้อมูล
- `labtest/Migrations/` ประวัติ schema ฐานข้อมูล
- `labtest/appsettings.json` ค่า connection string ชื่อ `DefaultConnection`
- `dotnet-tools.json` เครื่องมือ dotnet-ef แบบ local

## เมื่อเพิ่มหรือแก้ Model

เพิ่ม `DbSet` ใน `ApplicationDbContext` ตามความจำเป็น แล้วสร้าง migration จากโฟลเดอร์ solution:

```sh
dotnet ef migrations add DescribeYourChange --project labtest
dotnet ef database update --project labtest
```

สำหรับ environment อื่นนอก Development ให้ใช้คำสั่ง database update ก่อนรันแอป

## ตรวจสอบ Build

```sh
dotnet build labtest.sln
```

## HTTPS (เลือกใช้)

```sh
dotnet dev-certs https --trust
dotnet run --project labtest --launch-profile https
```

เปิด https://localhost:7236

หน้าทดสอบข้อมูลเป็นตัวอย่างสำหรับการพัฒนาในเครื่องและยังไม่มีระบบล็อกอิน

อ้างอิงการตั้งค่า EF Core และ migrations: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/adding-model?view=aspnetcore-10.0
