# Migration Decisions

## D-001: ใช้ ASP.NET Core MVC บน .NET 10 LTS

- **วันที่:** 2026-08-25
- **สถานะ:** อนุมัติโดยผู้ใช้
- **เหตุผล:** ต้องรันและพัฒนาได้ทั้ง macOS และ Windows ด้วย codebase เดียว ขณะที่ .NET Framework 4.7.2 และ `System.Web` ผูกกับ Windows
- **ผลที่ตามมา:** ต้องเปลี่ยนเป็น SDK-style project, ใช้ ASP.NET Core routing/middleware และย้าย static files ไป `wwwroot`

## D-002: Migration แบบ in-place และใช้ Git เป็น rollback boundary

- **วันที่:** 2026-08-25
- **สถานะ:** อนุมัติเป็นส่วนหนึ่งของ Step 1
- **เหตุผล:** โปรเจกต์มี controller เดียว ไม่มีฐานข้อมูล/authentication/session ที่ตรวจพบ การย้าย in-place ลดการทำสำเนา assets ขนาดประมาณ 90.4 MiB และทำให้คำสั่งรันเรียบง่าย
- **ผลที่ตามมา:** Legacy state ต้องถูกเก็บใน initial Git commit ก่อนแก้ runtime code และแต่ละ Step ต้องมี commit แยก

## D-003: รักษา URL และ UI เดิมก่อนปรับปรุง Landing Page

- **วันที่:** 2026-08-25
- **สถานะ:** อนุมัติโดยผู้ใช้
- **เหตุผล:** แยกความเสี่ยงของ framework migration ออกจากงาน redesign
- **ผลที่ตามมา:** การแก้ SEO, form, performance และ visual redesign จาก `LANDING_PAGE_REVIEW.md` อยู่นอก scope ของ migration นี้ เว้นแต่จำเป็นต่อ compilation/runtime compatibility

## D-004: Approval gate ก่อนทุก Step

- **วันที่:** 2026-08-25
- **สถานะ:** ผู้ใช้กำหนด
- **กติกา:** หลังจบแต่ละ Step ผู้ดำเนินงานต้องรายงานไฟล์ที่เปลี่ยน, คำสั่งตรวจสอบ, ผลลัพธ์ และความเสี่ยงคงเหลือ จากนั้นหยุดและขออนุมัติก่อนเริ่ม Step ถัดไป

## D-005: Step 2 ต้อง build ผ่านด้วย temporary migration boundaries

- **วันที่:** 2026-08-25
- **สถานะ:** ใช้ระหว่าง Step 2 และต้องยกเลิกตามลำดับใน Step 3–4
- **เหตุผล:** scaffold checkpoint ที่ commit ทั้งที่ build fail จะทำให้แยก regression จาก legacy incompatibility ไม่ได้
- **การตัดสินใจ:** ยกเว้น `Controllers/**/*.cs` จาก compilation และปิด Razor build/publish compilation ชั่วคราว โดยไม่ลบ controller หรือ views เดิม
- **เงื่อนไขสิ้นสุด:** Step 3 ต้องนำ controller กลับเข้า compilation และ Step 4 ต้องเปิด Razor compilation ก่อนสรุป migration

## D-006: ให้ ASP.NET Core web root รักษา static public URLs เดิม

- **วันที่:** 2026-08-25
- **สถานะ:** ดำเนินการใน Step 4 ตามแผนที่ผู้ใช้อนุมัติ
- **เหตุผล:** `UseStaticFiles()` ให้บริการไฟล์จาก `wwwroot`; การคงชื่อ directory เดิมไว้ใต้ web root ทำให้ Razor, CSS และ bookmark ที่อ้าง `/asset`, `/Content`, `/Scripts`, `/fonts` และ `/favicon.ico` ใช้ URL เดิมได้
- **ผลที่ตามมา:** ตำแหน่ง source เปลี่ยนเป็น `wwwroot/...` แต่ public URL ไม่เปลี่ยน; Razor compilation ถูกเปิดถาวรและ temporary boundary ใน D-005 สิ้นสุดครบแล้ว

## D-007: ใช้ thin platform wrappers ที่มี verification contract เดียวกัน

- **วันที่:** 2026-08-25
- **สถานะ:** ดำเนินการใน Step 5 ตามแผนที่ผู้ใช้อนุมัติ
- **เหตุผล:** ผู้พัฒนาบน macOS และ Windows ควรเรียก workflow เดียวกันโดยไม่ต้องจำ path หรือ flag ของ restore/build/test และต้องหยุดทันทีเมื่อ SDK/คำสั่งใดล้มเหลว
- **การตัดสินใจ:** Bash ใช้ `set -euo pipefail`, PowerShell ใช้ `$ErrorActionPreference = 'Stop'`; ทั้งคู่ resolve repository จากตำแหน่ง script, ตรวจ .NET SDK major อย่างน้อย 10 และรองรับ `OTP_DOTNET`/`OTP_CONFIGURATION`
- **ผลที่ตามมา:** platform-selected automated tests ตรวจ observable command sequence ด้วย fake SDK; PowerShell runtime จริงยังต้องผ่าน Windows acceptance test ใน Step 6 ก่อนสรุป cross-platform
