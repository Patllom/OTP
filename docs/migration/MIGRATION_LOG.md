# Migration Log

เอกสารนี้เป็นบันทึกตามลำดับเวลา ห้ามลบหรือแก้ไขรายการเดิมเพื่อซ่อนประวัติ หากต้องแก้ข้อมูลให้เพิ่มรายการ correction ใหม่

## รูปแบบรายการ

แต่ละรายการต้องมีเวลา, Step, สถานะอนุมัติ, วัตถุประสงค์, คำสั่ง/การกระทำ, ไฟล์ที่เปลี่ยน, ผลลัพธ์, หลักฐาน และวิธีย้อนกลับ

## 2026-08-25 — Step 1: Baseline และระบบบันทึก

- **Approval:** ผู้ใช้ตอบ “เริ่ม” หลังได้รับรายละเอียด Step 1
- **Scope:** เอกสาร baseline, checksum manifest, migration design/plan, Local Git และไฟล์ควบคุม Git เท่านั้น
- **Production code:** ไม่มีการแก้ controller, views, routes, configuration หรือ assets ของเว็บไซต์
- **Environment checks:**
  - `git --version`
  - `uname -srm`
  - `sw_vers`
  - ตรวจ `dotnet --info` และ `msbuild -version` เมื่อ command มีอยู่
- **Inventory checks:**
  - นับ Razor views, controllers และ files ใต้ `asset/`
  - รวมขนาด asset ด้วย `stat` และ `awk`
  - อ่าน actions จาก `Controllers/HomeController.cs`
- **Observed result:** macOS 26.6.2 arm64, Git 2.50.1, ไม่มี .NET SDK/MSBuild, 13 views, 1 controller, 9 actions, 322 asset files รวม 94,789,916 bytes
- **Checksum result:** `shasum -c docs/migration/baseline-sha256.txt` ผ่าน 382/382 ไฟล์
- **Git result:** สร้าง Local Git repository บน branch `main`; initial baseline commit คือ `d0afeec` (`chore: capture legacy baseline and migration plan`)
- **Git scope check:** commit มี 390 files; `packages/`, `bin/`, `obj/` และ `OTP.csproj.user` ถูก ignore และไม่อยู่ใน commit
- **Files created:**
  - `.gitignore`
  - `.gitattributes`
  - `docs/migration/BASELINE.md`
  - `docs/migration/MIGRATION_LOG.md`
  - `docs/migration/DECISIONS.md`
  - `docs/migration/TEST_RESULTS.md`
  - `docs/migration/baseline-sha256.txt`
  - `docs/superpowers/specs/2026-08-25-cross-platform-aspnet-core-design.md`
  - `docs/superpowers/plans/2026-08-25-cross-platform-aspnet-core.md`
- **Rollback:** ย้อนกลับ Git commits ของ Step 1 หรือลบเฉพาะไฟล์ที่ระบุด้านบนหากยังไม่มี commit
