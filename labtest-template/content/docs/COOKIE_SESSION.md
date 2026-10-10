# คู่มือ Cookie และ Session

ตอนสร้างโปรเจกต์เลือก `--state-mode Cookie` หรือ `--state-mode Session` โดยค่าเริ่มต้นคือ Cookie ค่าอยู่ใน `appsettings.json` (`StateMode`) และเปลี่ยนได้ภายหลังโดยแก้ค่าเป็นหนึ่งในสองคำนี้แล้วรีสตาร์ตแอป โค้ดใน `Program.cs` ลงทะเบียน session เฉพาะเมื่อเลือก Session

ชื่อไฟล์ในคู่มือนี้อ้างจาก **โฟลเดอร์รากของโปรเจกต์ที่สร้าง** (โฟลเดอร์เดียวกับไฟล์ `.csproj`) หากกำลังแก้ต้นแบบ ให้เติม `labtest-template/content/` หน้าชื่อไฟล์

## ต้องตั้งค่าที่ไฟล์ไหน

| งาน | ไฟล์ | สิ่งที่แก้ |
| --- | --- | --- |
| เลือกโหมดตอนสร้างโปรเจกต์ | `.template.config/template.json` ในต้นแบบเท่านั้น | ตัวเลือก `--state-mode` แทนค่า `STATE_MODE_VALUE` ในไฟล์ตั้งค่า; โปรเจกต์ที่สร้างแล้วไม่มีไฟล์นี้ |
| เปลี่ยนโหมดของโปรเจกต์ที่สร้างแล้ว | `appsettings.json` | ตั้ง `"StateMode": "Cookie"` หรือ `"Session"` แล้วรีสตาร์ตเว็บ |
| ตั้งค่า Cookie ฝั่งเซิร์ฟเวอร์ | `Controllers/StateController.cs` | ดู `Response.Cookies.Append`, `Request.Cookies`, `Response.Cookies.Delete` และ `CookieOptions` |
| เปิด Session ฝั่งเซิร์ฟเวอร์ | `Program.cs` | ดู `AddDistributedMemoryCache`, `AddSession` และ `UseSession`; โค้ดเดิมเปิดให้เมื่อ `StateMode` เป็น `Session` |
| อ่าน/เขียน Session ใน Controller | `Controllers/StateController.cs` หรือ Controller ที่สร้างเอง | ใช้ `HttpContext.Session.SetString`, `GetString`, `Remove` |
| เรียก backend จากหน้าเว็บ | `wwwroot/js/site.js` หรือไฟล์ `Views/<ชื่อ Controller>/<ชื่อ Action>.cshtml` | ใส่ `fetch('/state', ...)` หรือ JavaScript สำหรับ Cookie ตามตัวอย่างด้านล่าง |
| เปลี่ยนชื่อ Cookie หรือเวลาหมดอายุ | `Program.cs` สำหรับ Session cookie; `Controllers/StateController.cs` สำหรับ Cookie ของตัวอย่าง | แก้ `options.Cookie.Name`, `IdleTimeout`, `MaxAge` และนโยบาย Cookie ตามต้องการ |

**ตัวอย่างเปลี่ยนเป็น Session:** แก้ `appsettings.json` ให้มี `"StateMode": "Session"`; ตรวจว่า `Program.cs` ยังมีทั้ง `AddDistributedMemoryCache()`, `AddSession(...)` และ `UseSession()`; รีสตาร์ตเว็บ แล้วเรียก `GET /state` เพื่อดู `mode` เป็น `Session` โหมด Cookie ใช้ขั้นตอนเดียวกันโดยตั้งค่าเป็น `Cookie` ค่า `demo-value` จากโหมดเดิมไม่ถูกย้ายให้โดยอัตโนมัติ

ถ้าจะให้ JavaScript ทำงานบนทุกหน้า ให้วางโค้ดใน `wwwroot/js/site.js` ซึ่งหน้าเริ่มต้นโหลดผ่าน `Views/Shared/_Layout.cshtml` อยู่แล้ว หากใช้เฉพาะหน้าหนึ่ง ให้วาง script ใน Razor view ของหน้านั้น หน้า Home/Privacy ของเทมเพลตยังเป็นหน้าเริ่มต้นเดิมจนกว่าคุณจะแก้เอง

## แนวทางเลือก

| วิธี | ข้อมูลอยู่ที่ไหน | JavaScript อ่านโดยตรงได้ไหม | เหมาะกับ |
| --- | --- | --- | --- |
| Cookie แบบ HttpOnly ในตัวอย่าง | เบราว์เซอร์ | ไม่ได้; ใช้ `fetch` เรียกเซิร์ฟเวอร์ | ค่าขนาดเล็กที่ต้องส่งพร้อมทุก request |
| ASP.NET Core Session | หน่วยความจำเซิร์ฟเวอร์; เบราว์เซอร์เก็บเพียง session ID ใน cookie | อ่านค่าบนเซิร์ฟเวอร์โดยตรงไม่ได้; ใช้ `fetch` | ค่าชั่วคราวที่ใช้ระหว่างการเข้าเว็บ |
| `sessionStorage` ของเบราว์เซอร์ | เบราว์เซอร์แท็บนั้น | ได้ | ค่าฝั่งหน้าเว็บเท่านั้น; **ไม่ใช่** ASP.NET Core Session |

ตัวอย่างนี้ใช้ข้อมูลชื่อ `demo-value` สำหรับฝึกเท่านั้น ไม่ใช่ระบบล็อกอินหรือที่เก็บความลับ ค่า Session ใช้ memory cache จึงหายเมื่อแอปรีสตาร์ต หากต้องการ session ข้ามหลายเซิร์ฟเวอร์หรือรีสตาร์ต ต้องเปลี่ยนเป็น distributed cache ที่คงข้อมูลได้

## ใช้จากฝั่งหน้าเว็บกับ backend

หลังรันเว็บ เปิด Developer Tools → Console ที่หน้าเว็บ แล้วทดลองโค้ดนี้ได้ทั้งสองโหมด Cookie และ Session:

```js
// บันทึกค่าผ่าน backend; browser จัดการ cookie ของ origin เดียวกัน
await fetch('/state', {
  method: 'POST',
  credentials: 'same-origin',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ value: 'hello' })
});

// อ่านค่าผ่าน backend
console.log(await (await fetch('/state', { credentials: 'same-origin' })).json());

// ลบค่า
await fetch('/state', { method: 'DELETE', credentials: 'same-origin' });
```

`StateController` ใช้ `Response.Cookies.Append(...)` และ `Request.Cookies[...]` ในโหมด Cookie โดยกำหนด `HttpOnly`, `SameSite=Lax`, อายุ 30 นาที และ `Secure` เมื่อใช้ HTTPS ในโหมด Session ใช้ `HttpContext.Session.SetString/GetString/Remove` ซึ่งถูกเปิดด้วย `AddDistributedMemoryCache`, `AddSession` และ `UseSession` ใน `Program.cs`

## เขียนโค้ดเองฝั่ง backend

ใน controller ที่ต้องการค่าแบบ Cookie:

```csharp
Response.Cookies.Append("theme", "dark", new CookieOptions
{
    HttpOnly = true,
    Secure = Request.IsHttps,
    SameSite = SameSiteMode.Lax,
    MaxAge = TimeSpan.FromDays(7)
});
var theme = Request.Cookies["theme"];
Response.Cookies.Delete("theme");
```

ใน controller ที่ต้องการ ASP.NET Core Session ให้ตั้ง `StateMode` เป็น `Session` ก่อน:

```csharp
HttpContext.Session.SetString("theme", "dark");
var theme = HttpContext.Session.GetString("theme");
HttpContext.Session.Remove("theme");
```

## เขียนโค้ดเองฝั่งหน้าเว็บ

สำหรับค่าที่ **ไม่ใช่ข้อมูลลับ** และต้องการให้ JavaScript อ่าน Cookie ได้ ให้สร้าง Cookie แยกจากตัวอย่าง HttpOnly:

```js
document.cookie = 'theme=dark; Path=/; SameSite=Lax; Max-Age=604800';
console.log(document.cookie);
document.cookie = 'theme=; Path=/; Max-Age=0';
```

Cookie ที่ JavaScript สร้างเองไม่มี `HttpOnly` และอ่านได้จาก JavaScript สำหรับค่า Session บนเซิร์ฟเวอร์ JavaScript ต้องเรียก backend ผ่าน `fetch` ตามตัวอย่างด้านบน หากต้องการค่าเฉพาะแท็บโดยไม่ส่งไป backend:

```js
sessionStorage.setItem('theme', 'dark');
console.log(sessionStorage.getItem('theme'));
sessionStorage.removeItem('theme');
```

## ข้อควรระวัง

หากจะใช้ Cookie หรือ Session เพื่อยืนยันตัวตนจริง ให้ใช้ระบบ authentication ของ ASP.NET Core พร้อม CSRF protection และ HTTPS ค่า `demo-value` ใน API นี้ใช้เพื่อเรียนรู้วิธีเก็บสถานะเท่านั้น

อ้างอิง: [ASP.NET Core Session](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0), [Cookie authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)
