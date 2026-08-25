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

## D-008: ใช้ GitHub-hosted Windows runner สำหรับ final Windows acceptance

- **วันที่:** 2026-08-25
- **สถานะ:** ผู้ใช้อนุมัติ external push และ workflow ใน Step 6
- **เหตุผล:** เครื่องพัฒนาเป็น macOS และไม่มี Windows VM/PowerShell/container runtime; การอ้างว่า cross-platform ต้องมี execution evidence บน Windows จริง
- **การตัดสินใจ:** เพิ่ม `.github/workflows/windows-verify.yml` สำหรับ `windows-latest`, trigger เฉพาะ push ที่ branch `migration/aspnet-core-net10` และ manual dispatch, จำกัด permission เป็น `contents: read`, timeout 20 นาที และไม่มี deployment/merge/release step
- **Supply-chain control:** pin `actions/checkout` v6.0.2 และ `actions/setup-dotnet` v6.0.0 ด้วย full commit SHA ที่ตรวจจาก repositories ทางการ
- **Acceptance:** job ต้องรัน `scripts/verify.ps1`, ผ่าน Windows-selected script behavior tests และเปิด `scripts/run.ps1` เพื่อตรวจ 11 compatibility routes กับ static logo ก่อนปิด process tree
- **ผลการยืนยัน:** GitHub Actions run `32839304728` ผ่านบน Windows Server 2025 Datacenter x64 ด้วย SDK 10.0.400; build 0 warnings/0 errors, tests 26/26, routes 11/11 และ static logo ผ่านทั้งหมด
- **ขอบเขตหลักฐาน:** `windows-latest` ที่รันจริงเป็น Windows Server 2025 ไม่ใช่ Windows 11 client edition; ถือเป็น Windows x64 acceptance ตามตัวเลือกที่ผู้ใช้อนุมัติ และไม่มีผลให้ workflow ได้สิทธิ์ deploy/merge/release

## D-009: เลื่อน navigation helper refactor ไปหลัง migration

- **วันที่:** 2026-08-26
- **สถานะ:** ผู้ใช้อนุมัติใน Final Cleanup
- **เหตุผล:** ลิงก์เดิมใน `_LayoutMain.cshtml` render และนำทางได้ถูกต้องอยู่แล้ว ขณะที่การเปลี่ยนเป็น tag helpers หรือ `Url.Action` อาจยุบ default route หรือเปลี่ยนรูปแบบ URL ที่ผู้ใช้เห็น ซึ่งขัดกับเป้าหมายหลักของ migration ที่ต้องรักษา URL และ UI เดิม
- **การตัดสินใจ:** คงรูปแบบ internal navigation เดิมไว้ใน migration branch และทำ helper refactor เป็นงานแยกหลัง merge โดยต้องเพิ่ม rendered-link assertions ก่อนเปลี่ยน production Razor
- **ผลที่ตามมา:** ไม่มี runtime blocker ใน migration นี้ แต่ source ยังคงมี legacy URL-concatenation pattern ชั่วคราว; follow-up ต้องรักษาปลายทาง desktop/mobile navigation เดิมและผ่าน compatibility tests
