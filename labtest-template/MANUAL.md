# คู่มือสร้างและใช้งานเทมเพลต labmvc

เทมเพลตนี้อยู่ใน `labtest-template/content/` และทำจาก ASP.NET Core MVC มาตรฐานของ .NET 10 หน้า `Home/Index`, `Home/Privacy`, layout และไฟล์ JavaScript/CSS เริ่มต้นยังเป็นของ `dotnet new mvc` การปรับแต่งเพิ่มเฉพาะ SQLite, ตัวอย่างโค้ดฐานข้อมูล, Cookie/Session, VS Code และคู่มือ

## สิ่งที่ต้องมี

- .NET SDK 10 (`dotnet --version`)
- VS Code กับ C# Dev Kit หรือ C# extension หากต้องการกด F5 เพื่อ debug
- Internet สำหรับ restore NuGet packages ครั้งแรก

## ติดตั้งเทมเพลตลงในเครื่อง

จากโฟลเดอร์ที่มี `labtest-template`:

```sh
dotnet new install ./labtest-template/content
dotnet new list labmvc
dotnet new labmvc --help
```

หากแก้ไฟล์เทมเพลตแล้ว ให้ติดตั้งใหม่ด้วย `dotnet new uninstall ./labtest-template/content` ตามด้วย `dotnet new install ./labtest-template/content` จากตำแหน่งเดิม

## สร้างโปรเจกต์

เลือกหนึ่งในสองโหมดตามที่ต้องการ:

```sh
dotnet new labmvc -n MyExam --state-mode Cookie -o MyExam
dotnet new labmvc -n MyExamSession --state-mode Session -o MyExamSession
```

ค่า `-n` เป็นชื่อโปรเจกต์ และใช้เปลี่ยนชื่อ `.csproj`, namespace, และ `Data Source=MyExam.db` โดยอัตโนมัติ ใช้ชื่อที่เหมาะกับ C# identifier เช่น `MyExam` หรือ `labtest` หากไม่ระบุ `--state-mode` จะใช้ Cookie

เปิดโฟลเดอร์โปรเจกต์ที่สร้างใน VS Code จากนั้น:

```sh
cd MyExam
dotnet restore
dotnet tool restore
dotnet run --launch-profile http
```

ดูที่ `Now listening on:` เพื่อทราบ URL เพราะเว็บใช้พอร์ตว่างที่ระบบเลือกให้ หรือกด **F5** และเลือก `Debug MVC` ใน VS Code เพื่อ build และ debug โปรเจกต์โดยตรง

ฐานข้อมูล `<ชื่อโปรเจกต์>.db` ถูกสร้างในโฟลเดอร์โปรเจกต์ในการรันครั้งแรกของ environment `Development` หากต้องสร้างก่อนรัน ใช้ `dotnet ef database update` ในโฟลเดอร์โปรเจกต์ หลังสร้างแล้วอ่านคู่มือใน `README.md`, `docs/DATABASE_CRUD.md` และ `docs/COOKIE_SESSION.md` ที่อยู่ในโปรเจกต์ใหม่

## โครงสร้างเทมเพลตและวิธีสร้างเอง

1. เริ่มจาก `dotnet new mvc -n TemplateProject -o content --framework net10.0` โดย `TemplateProject` เป็นชื่อ placeholder ที่เทมเพลตใช้แทนเมื่อสร้างโปรเจกต์
2. เพิ่ม NuGet packages `Microsoft.EntityFrameworkCore.Sqlite` และ `Microsoft.EntityFrameworkCore.Design` รุ่นเดียวกับ EF Core 10 และเพิ่ม `ApplicationDbContext` กับ Model
3. กำหนด `ConnectionStrings:DefaultConnection` เป็น `Data Source=TemplateProject.db` ใน `appsettings.json` แล้วลงทะเบียน `AddDbContext` กับ `UseSqlite` ใน `Program.cs`
4. สร้าง migration เริ่มต้นด้วย `dotnet ef migrations add InitialCreate` และใส่ `Database.MigrateAsync()` ในโหมด Development เพื่อให้โปรเจกต์ใหม่พร้อมใช้ฐานข้อมูล
5. เพิ่ม `.template.config/template.json`: `sourceName` คือ `TemplateProject` เพื่อให้ .NET แทนข้อความนี้ในชื่อไฟล์และเนื้อหา; symbol `state-mode` แทน `STATE_MODE_VALUE` ใน `appsettings.json` ตามตัวเลือก Cookie/Session; exclusion ตัด `bin`, `obj`, ฐานข้อมูลและไฟล์ชั่วคราวออก
6. เพิ่ม `.vscode/launch.json`, `.vscode/tasks.json` และคู่มือ แล้วทดสอบด้วย `dotnet new install` + `dotnet new labmvc`

ก่อนเผยแพร่เทมเพลต ให้ตรวจว่ามี `.template.config/template.json`, Model, DbContext และ Migrations แต่ไม่มี `bin`, `obj` หรือ `.db` ในผลลัพธ์โปรเจกต์ใหม่

อ้างอิง: [Microsoft: Create a project template for dotnet new](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-project-template), [Install and manage SDK templates](https://learn.microsoft.com/en-us/dotnet/core/install/templates)
