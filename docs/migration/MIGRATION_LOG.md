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

## 2026-08-25 — Git Remote Setup

- **Approval:** ผู้ใช้ขอให้ผูก Git กับ `https://github.com/Patllom/OTP.git`
- **Scope:** ตั้งค่า Local Git remote และตรวจการเข้าถึงเท่านั้น; ไม่มีการ push
- **Pre-check:** branch ปัจจุบันคือ `main`, working tree สะอาด และยังไม่มี remote ชื่อ `origin`
- **Command:** `git remote add origin https://github.com/Patllom/OTP.git`
- **Verification:** `git remote get-url origin` คืน URL ตรงตามที่ผู้ใช้ให้ และ `git ls-remote --exit-code origin` exit 0
- **Remote state:** ไม่พบ branch หรือ tag จาก `ls-remote`; repository ปลายทางดูเป็น repository ว่าง ณ เวลาที่ตรวจ
- **External writes:** ไม่มี; commit ของโปรเจกต์ยังอยู่เฉพาะเครื่อง local
- **Rollback:** `git remote remove origin`

### Correction — Remote verification semantics

- รายการเดิมระบุว่า `git ls-remote --exit-code origin` exit 0 ซึ่งไม่ถูกต้องสำหรับ remote ที่ไม่มี refs เพราะ option `--exit-code` สามารถคืน non-zero เมื่อไม่พบ refs
- การตรวจที่ใช้ยืนยันจริงคือ `git ls-remote origin` ซึ่งติดต่อ GitHub สำเร็จและคืนรายการ 0 บรรทัด โดยไม่มี authentication/network error
- ผลสรุปไม่เปลี่ยน: remote URL ถูกต้อง, เชื่อมต่อได้ และ repository ปลายทางยังไม่มี branch/tag ณ เวลาที่ตรวจ

## 2026-08-25 — Step 2: ASP.NET Core .NET 10 Scaffold

- **Approval:** ผู้ใช้ตอบ “เริ่ม 2” หลังได้รับรายละเอียด worktree, workspace-local SDK และ scaffold scope
- **Isolation:** เพิ่ม `.worktrees/` ใน `.gitignore`, commit บน `main` คือ `7f74729`, แล้วสร้าง branch `migration/aspnet-core-net10` ที่ `.worktrees/aspnet-core-net10`
- **SDK install:** ดาวน์โหลด installer จาก `https://dot.net/v1/dotnet-install.sh`, ตรวจด้วย `bash -n` และติดตั้งแบบไม่ใช้ `sudo` ที่ `/Users/theerapat_k/Codex/OTP/.dotnet`
- **Installed toolchain:** .NET SDK 10.0.400, MSBuild 18.9.6, .NET/ASP.NET Core Runtime 10.0.11, RID `osx-arm64`
- **Project changes:** เปลี่ยน `OTP.csproj` เป็น `Microsoft.NET.Sdk.Web` target `net10.0`; เพิ่ม `Program.cs`, `global.json`, `appsettings.json` และ `wwwroot/.gitkeep`
- **Legacy startup removed:** `Global.asax`, `Global.asax.cs`, `App_Start/`, `Web*.config`, `packages.config` และ `Properties/AssemblyInfo.cs`; ทุกไฟล์กู้คืนได้จาก baseline commit
- **Temporary boundary:** ยังเก็บ `Controllers/` และ `Views/` ไว้ครบ แต่ exclude controller compilation และปิด Razor build/publish compilation ชั่วคราวตาม D-005
- **Restore:** `/Users/theerapat_k/Codex/OTP/.dotnet/dotnet restore OTP.csproj` สำเร็จ
- **Build:** Debug build สำเร็จด้วย 0 warnings และ 0 errors
- **Runtime smoke test:** Kestrel ฟังที่ `http://127.0.0.1:5180`; `GET /migration-health` ตอบ HTTP 200 และ `{"status":"ok","framework":".NET 10"}`; process หยุดอย่างสะอาด
- **Production routes:** ยังไม่เปิดใช้งานใน Step 2 เพราะ controller/views จะย้ายใน Step 3–4
- **Rollback:** revert commit ของ Step 2 บน branch migration หรือลบ worktree/branch โดย baseline บน `main` ไม่ได้รับผลกระทบ
