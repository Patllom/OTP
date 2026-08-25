# Cross-Platform ASP.NET Core Migration Design

วันที่: 2026-08-25  
สถานะ: ผู้ใช้เลือกแนวทาง ASP.NET Core MVC บน .NET 10 และกำหนด approval gate ก่อนทุก Step

## เป้าหมาย

เปลี่ยนเว็บ ASP.NET MVC 5 บน .NET Framework 4.7.2 ให้เป็น ASP.NET Core MVC บน .NET 10 LTS ซึ่ง build, test และ run ได้จาก codebase เดียวบน macOS และ Windows โดยรักษา URL, เนื้อหา และหน้าตาเดิมระหว่าง migration

## สิ่งที่ไม่รวมในขอบเขต

- ไม่ redesign landing page ใน migration นี้
- ไม่แก้ visitor counter, contact form backend, SEO หรือ dependency ด้านหน้า เว้นแต่จำเป็นต่อการรันบน ASP.NET Core
- ไม่เพิ่มฐานข้อมูล, authentication หรือระบบจัดการเนื้อหา
- ไม่ลบ Git history หรือ legacy baseline

## สถาปัตยกรรมเป้าหมาย

โปรเจกต์จะถูกเปลี่ยนแบบ in-place เป็น SDK-style web project ที่ target `net10.0` และใช้ `Microsoft.NET.Sdk.Web` จุดเริ่มต้นแอปคือ `Program.cs` ซึ่งลงทะเบียน MVC, static files, exception handling, HTTPS redirection เมื่อเหมาะสม และ conventional route `{controller=Home}/{action=Index}/{id?}`

`HomeController` จะใช้ `Microsoft.AspNetCore.Mvc.Controller` โดยคงชื่อ actions ทั้งเก้ารายการ Razor views จะอยู่ใน `Views/` ตามเดิม ส่วน static assets จะย้ายด้วย Git history ไปใต้ `wwwroot/` ทำให้ URL เช่น `/asset/images/...` คงเดิม

## โครงสร้างไฟล์เป้าหมาย

- `OTP.csproj` — SDK-style ASP.NET Core project target `net10.0`
- `Program.cs` — service registration และ HTTP pipeline
- `Controllers/HomeController.cs` — actions เดิมบน ASP.NET Core MVC
- `Views/` — Razor views และ layouts ที่ปรับ syntax เฉพาะจุด
- `wwwroot/asset/` — assets ของธีมเดิม
- `wwwroot/Content/`, `wwwroot/Scripts/`, `wwwroot/fonts/` — static resources ที่ legacy views อาจอ้างถึง
- `tests/OTP.Web.Tests/` — xUnit integration tests ผ่าน `WebApplicationFactory<Program>`
- `global.json` — กำหนด .NET 10 SDK baseline และอนุญาต roll-forward ภายใน .NET 10
- `scripts/run.sh`, `scripts/run.ps1` — คำสั่งรันที่ตรวจ dependency และส่งต่อให้ `dotnet run`
- `README.md` — setup/run/test/troubleshooting สำหรับ macOS และ Windows

## Routing และ compatibility

ต้องตอบ HTTP 200 สำหรับ `/`, `/Home/Index`, `/Home/objective`, `/Home/scopeofwork`, `/Home/eventpic`, `/Home/knowledgetech`, `/Home/information`, `/Home/projectresult`, `/Home/About` และ `/Home/Contact` ตัว route matching ยังคงไม่แยกตัวพิมพ์ใหญ่เล็กตามพฤติกรรมเว็บบน Windows/ASP.NET

เดิมวางแผนเปลี่ยนลิงก์ภายในจากการต่อ string กับ `@Url.Content("~/")` เป็น ASP.NET Core tag helpers หรือ `Url.Action` โดย URL ที่ผู้ใช้เห็นต้องคงเดิม อย่างไรก็ตาม Final Cleanup เลื่อน refactor นี้ไปเป็นงานหลัง migration ตาม D-009 เพื่อไม่ให้การเปลี่ยน link generation กระทบ URL/UI compatibility ก่อน merge

## Static files

ย้าย static directories ด้วย `git mv` เพื่อรักษาประวัติ ไม่ทำสำเนา asset ชุดใหญ่ไว้สองตำแหน่ง URL ภายนอกใน views จะยังคงเดิมใน migration pass แรก การบีบอัด/เปลี่ยนรูปเป็น WebP เป็นงานแยกหลัง migration

## Configuration และ environment

แทนที่ `Web.config`, `Global.asax` และ `App_Start` ด้วย `Program.cs` และ `appsettings*.json` เฉพาะเมื่อมีค่าที่ต้องใช้จริง โปรเจกต์ปัจจุบันไม่มี connection string หรือ application secret ที่ใช้งาน จึงไม่เพิ่ม configuration ที่ไม่จำเป็น

Development run ใช้ Kestrel และไม่พึ่ง IIS Express คำสั่งมาตรฐานคือ `dotnet restore`, `dotnet build`, `dotnet test` และ `dotnet run --project OTP.csproj`

## Error handling

Development ใช้ developer exception page ผ่านค่า environment มาตรฐาน Production ใช้ exception handler ไปยัง error action/view และ HSTS ตาม template ASP.NET Core การเรียก visitor endpoint ฝั่ง client ที่ล้มเหลวต้องไม่ทำให้ server route ล้ม

## Testing

Integration tests จะเริ่ม host แอปใน memory และตรวจทุก compatibility route ว่าได้ 200, content type เป็น HTML และมี marker text ประจำหน้า Static-file tests ตรวจ `/asset/images/content/logo1.png` ว่าตอบ 200 และมี content type รูปภาพ

Mac acceptance ต้องผ่าน restore/build/test/run บน macOS arm64 Windows acceptance ต้องผ่าน restore/build/test/run บน Windows 11 x64 หลักฐานทั้งสองแพลตฟอร์มต้องบันทึกใน `docs/migration/TEST_RESULTS.md`

## Rollback

Initial Git commit เป็น immutable legacy baseline ทุก Step มี commit boundary ของตนเอง หาก Step ใดไม่ผ่าน ให้ revert เฉพาะ commits ของ Step นั้น ไม่ใช้ destructive reset และไม่ลบ baseline

## Approval workflow

ผู้ดำเนินงานทำได้เฉพาะ Step ที่ผู้ใช้อนุมัติแล้ว หลัง verification ต้องอัปเดต log, รายงานผล และหยุด การถามเพื่อเริ่ม Step ถัดไปต้องเป็นข้อความแยกจากการลงมือทำ Step นั้น
