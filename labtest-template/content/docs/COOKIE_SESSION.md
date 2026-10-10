# คู่มือ Cookie และ Session

ตอนสร้างโปรเจกต์เลือก `--state-mode Cookie` หรือ `--state-mode Session` โดยค่าเริ่มต้นคือ Cookie ค่าอยู่ใน `appsettings.json` (`StateMode`) และเปลี่ยนได้ภายหลังโดยแก้ค่าเป็นหนึ่งในสองคำนี้แล้วรีสตาร์ตแอป โค้ดใน `Program.cs` ลงทะเบียน session เฉพาะเมื่อเลือก Session

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
