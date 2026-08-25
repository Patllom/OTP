# Migration Baseline

บันทึกเมื่อ: 2026-08-25 (Asia/Bangkok)

## ขอบเขต

เอกสารนี้บันทึกสถานะของโปรเจกต์ก่อนย้ายจาก ASP.NET MVC 5 บน .NET Framework ไป ASP.NET Core MVC บน .NET 10 โดยยังไม่มีการเปลี่ยนแปลงโค้ดการทำงานของเว็บไซต์

## สภาพแวดล้อมต้นทาง

| รายการ | ค่า |
|---|---|
| Operating system | macOS 26.6.2 |
| Architecture | arm64 |
| Git | 2.50.1 |
| .NET SDK | ไม่ได้ติดตั้ง |
| MSBuild | ไม่ได้ติดตั้ง |
| Project type | ASP.NET MVC 5 / .NET Framework |
| Target framework | .NET Framework 4.7.2 |
| ASP.NET MVC package | 5.2.7 |
| Solution | `OTT.sln` |
| Project | `OTP.csproj` |
| Assembly name | `OTT` |
| Root namespace | `OTT` |

## ปริมาณไฟล์

| กลุ่ม | จำนวน/ขนาด |
|---|---:|
| Razor views | 13 ไฟล์ |
| Controllers | 1 ไฟล์ |
| Controller actions | 9 actions |
| Files ใต้ `asset/` | 322 ไฟล์ |
| ขนาดรวม `asset/` | 94,789,916 bytes (ประมาณ 90.4 MiB) |

## Routes ที่ต้องรักษา

Legacy route pattern คือ `{controller}/{action}/{id}` โดยค่าเริ่มต้นเป็น `Home/Index`

| Action | URL ที่ต้องผ่านหลัง migration |
|---|---|
| `Index` | `/` และ `/Home/Index` |
| `objective` | `/Home/objective` |
| `scopeofwork` | `/Home/scopeofwork` |
| `eventpic` | `/Home/eventpic` |
| `knowledgetech` | `/Home/knowledgetech` |
| `information` | `/Home/information` |
| `projectresult` | `/Home/projectresult` |
| `About` | `/Home/About` |
| `Contact` | `/Home/Contact` |

## Razor views ปัจจุบัน

- `Views/Home/About.cshtml`
- `Views/Home/Contact.cshtml`
- `Views/Home/Index.cshtml`
- `Views/Home/eventpic.cshtml`
- `Views/Home/information.cshtml`
- `Views/Home/knowledgetech.cshtml`
- `Views/Home/objective.cshtml`
- `Views/Home/projectresult.cshtml`
- `Views/Home/scopeofwork.cshtml`
- `Views/Shared/Error.cshtml`
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_LayoutMain.cshtml`
- `Views/_ViewStart.cshtml`

## Dependency baseline

- Antlr 3.5.0.2
- Bootstrap 3.4.1
- jQuery 3.4.1
- jQuery.Validation 1.17.0
- Microsoft.AspNet.Mvc 5.2.7
- Microsoft.AspNet.Razor 3.2.7
- Microsoft.AspNet.Web.Optimization 1.1.3
- Microsoft.AspNet.WebPages 3.2.7
- Microsoft.CodeDom.Providers.DotNetCompilerPlatform 2.0.1
- Microsoft.jQuery.Unobtrusive.Validation 3.2.11
- Microsoft.Web.Infrastructure 1.0.0.0
- Modernizr 2.8.3
- Newtonsoft.Json 12.0.2
- WebGrease 1.6.0

## ข้อจำกัดของ baseline build

โปรเจกต์เดิมไม่สามารถ build บนเครื่อง baseline นี้ได้ เนื่องจากเป็น .NET Framework 4.7.2 และเครื่อง macOS ไม่มี MSBuild/.NET Framework toolchain การตรวจ baseline จึงใช้ source inventory, route inventory และ SHA-256 manifest แทน การยืนยัน build ฝั่ง Windows ของ legacy project ไม่ใช่เงื่อนไขของ Step 1

## หลักฐานความสมบูรณ์

ค่า SHA-256 ของ source/config/static assets ก่อน migration อยู่ที่ `docs/migration/baseline-sha256.txt` ไฟล์ดังกล่าวไม่รวม `bin/`, `obj/`, `packages/`, IDE files และเอกสารที่สร้างใน Step 1

