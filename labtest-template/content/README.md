# TemplateProject

ASP.NET Core MVC (.NET 10) พร้อม EF Core + SQLite โดยหน้า Home และ Privacy เป็นหน้าเริ่มต้นจาก `dotnet new mvc` ตามเดิม

## เริ่มใช้งานใน VS Code

ติดตั้ง .NET SDK 10 และส่วนขยาย **C# Dev Kit** หรือ **C#** สำหรับการ debug เปิด **โฟลเดอร์โปรเจกต์ที่สร้าง** ใน VS Code แล้วรัน:

```sh
dotnet restore
dotnet tool restore
dotnet run --launch-profile http
```

ดู URL ที่บรรทัด `Now listening on:` เพราะพอร์ตถูกเลือกอัตโนมัติเพื่อลดปัญหาพอร์ตชนกัน ใน VS Code กด **F5** แล้วเลือก `Debug MVC`; `launch.json` และ `tasks.json` จะ build ก่อน debug และเปิดเบราว์เซอร์ให้

เมื่อรันใน `Development` แอปจะใช้ migration สร้าง/อัปเดตฐานข้อมูล `TemplateProject.db` ในโฟลเดอร์โปรเจกต์ให้ ฐานข้อมูลถูก ignore จาก Git

## คู่มือ

- [การเชื่อมต่อฐานข้อมูล แก้ Model และ CRUD](docs/DATABASE_CRUD.md)
- [การใช้ Cookie หรือ Session ทั้งหน้าเว็บและเซิร์ฟเวอร์](docs/COOKIE_SESSION.md)

การตั้งค่าอยู่ใน `appsettings.json`: `ConnectionStrings:DefaultConnection` และ `StateMode` ค่า `StateMode` คือ `Cookie` หรือ `Session` ตามที่เลือกตอนสร้างโปรเจกต์

ตัวอย่าง `Services/SampleItemRepository.cs` มี CRUD ทั้ง LINQ lambda และ Raw SQL แบบส่งพารามิเตอร์ ตาราง `SampleItems` สร้างโดย migration เริ่มต้น ตัวอย่าง `Controllers/StateController.cs` ให้ลอง Cookie/Session ผ่าน API โดยไม่เปลี่ยนหน้า MVC เริ่มต้น
