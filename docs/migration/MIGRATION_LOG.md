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
- **Step commit:** `b914d31` (`build: scaffold ASP.NET Core application`)
- **Production routes:** ยังไม่เปิดใช้งานใน Step 2 เพราะ controller/views จะย้ายใน Step 3–4
- **Rollback:** revert commit ของ Step 2 บน branch migration หรือลบ worktree/branch โดย baseline บน `main` ไม่ได้รับผลกระทบ

## 2026-08-25 — Step 3: Controller และ Action Discovery Tests

- **Approval:** ผู้ใช้ตอบ “อนุมัติ” หลังได้รับขอบเขต Step 3 ที่แยก controller migration ออกจาก Razor rendering
- **Test stack:** `Microsoft.AspNetCore.Mvc.Testing` 10.0.11, `Microsoft.NET.Test.Sdk` 17.14.1, xUnit 2.9.3 และ xUnit runner 3.1.4
- **Test boundary:** ใช้ `WebApplicationFactory<Program>`, `IActionDescriptorCollectionProvider` และ controller instance จริง; ไม่มี mock และยังไม่ request/render Razor views
- **Test setup correction 1:** RED รอบแรก compile error เพราะ test project ขาด implicit `Xunit` using; เพิ่ม `<Using Include="Xunit" />` แล้วรันใหม่
- **Test setup correction 2:** แก้ xUnit analyzer warning ด้วย `Assert.Single(collection, predicate)` และ exclude `tests/**` จาก web project content/compile items เพื่อป้องกัน recursive test build outputs
- **Verified RED:** 9/9 theory cases fail ที่ `Assert.Single()` เพราะไม่มี matching Home action descriptor ขณะ controller ยังถูก exclude; test run ไม่มี warning หลัง correction
- **Implementation:** เปลี่ยน `Controllers/HomeController.cs` จาก `System.Web.Mvc` เป็น `Microsoft.AspNetCore.Mvc`, เปลี่ยน return type เป็น `IActionResult` และนำ controller กลับเข้า compilation โดยรักษา action names และ `ViewBag.Message`
- **Verified GREEN:** 9/9 theory cases ผ่าน; แต่ละ action ถูก discover เป็น `Home` controller action และ invoke แล้วคืน `ViewResult`
- **Build:** `dotnet build OTP.csproj --no-restore --configuration Debug` สำเร็จด้วย 0 warnings และ 0 errors
- **Dependency audit:** `dotnet list ... package --vulnerable --include-transitive` ไม่พบ vulnerable packages จาก NuGet sources ปัจจุบัน
- **Step commit:** `27aa481` (`test: migrate and verify Home controller`)
- **Deferred to Step 4:** HTTP route responses, Razor compilation/rendering และ static assets
- **Rollback:** revert commit ของ Step 3; Step 2 scaffold และ legacy baseline ไม่ได้รับผลกระทบ

## 2026-08-25 — Step 4: Razor Views และ Static Assets

- **Approval:** ผู้ใช้ตอบ “เริ่ม” หลังได้รับรายงาน Step 3 และขอบเขต Step 4
- **Scope:** เปิด Razor compilation, ทำให้ legacy views render บน ASP.NET Core, เพิ่ม Error route, ย้าย static assets เข้า `wwwroot` โดยรักษา public URLs และเพิ่ม HTTP integration tests; ไม่มีการ redesign หรือเปลี่ยนเนื้อหาหน้า
- **Test-first RED:** เพิ่ม `RouteRenderingTests.cs` สำหรับ 11 page cases (รวม `/`, legacy Home URLs และ `/Home/Error`) กับ static logo 1 case แล้วรันเฉพาะ test class; ผลคือ fail 12/12 ตาม boundary เดิม: 10 หน้าเป็น HTTP 500, Error route เป็น 404 และ logo เป็น 404
- **Razor migration:** นำ `RazorCompileOnBuild=false` และ `RazorCompileOnPublish=false` ออก, เพิ่ม `Views/_ViewImports.cshtml`, เพิ่ม `HomeController.Error()`, กำหนด Error view เป็น layout-free, ลบ legacy `Views/Web.config` และแทนที่ `Styles.Render`/`Scripts.Render`/legacy link helpers ใน fallback layout
- **Static migration:** ใช้ `git mv` ย้าย `asset/`, `Content/`, `Scripts/`, `fonts/` และ `favicon.ico` เข้า `wwwroot/`; ชื่อใต้ web root เดิมไม่เปลี่ยน จึงยัง request `/asset/...`, `/Content/...`, `/Scripts/...`, `/fonts/...` และ `/favicon.ico` ได้
- **Asset inventory reconciliation:** baseline filesystem count 322 รายการประกอบด้วย site assets ที่อยู่ใน Git 321 ไฟล์และ ignored Windows thumbnail cache `asset/images/Thumbs.db` 1 ไฟล์; เปรียบเทียบ `git ls-tree` กับ `wwwroot/asset` แล้วพบว่า version-controlled site assets ถูกย้ายครบ 321/321 โดย `Thumbs.db` ไม่เคยอยู่ใน migration worktree
- **Build after Razor enablement:** `dotnet build OTP.csproj --no-restore` สำเร็จด้วย 0 warnings และ 0 errors
- **Observed integration defect:** รอบ GREEN แรกผ่าน Error/static 2 cases แต่ legacy pages 10 cases เป็น HTTP 500; stack trace ระบุ `_LayoutMain.cshtml` เรียก section `Scripts` ซ้ำ
- **Root-cause verification:** `rg` พบ existing `@RenderSection("Scripts", false)` ที่บรรทัดเดิมประมาณ 305 และ `RenderSectionAsync` ที่เพิ่มใหม่ท้าย layout; แก้ขั้นต่ำโดยลบเฉพาะ invocation ที่เพิ่มซ้ำ
- **Targeted GREEN:** `RouteRenderingTests` ผ่าน 12/12
- **Full verification:** Debug build สำเร็จด้วย 0 warnings/0 errors และ full test suite ผ่าน 21/21 (9 controller contracts + 11 page cases + 1 static-file case)
- **Production runtime smoke:** Kestrel ที่ `http://127.0.0.1:5184` ตอบ HTTP 200 `text/html` ครบทั้ง 11 routes; `/asset/images/content/logo1.png` ตอบ HTTP 200, `image/png`, 30,127 bytes; หยุด process ด้วย Ctrl+C อย่างสะอาด
- **Step commit:** `dd78fe9` (`feat: migrate views and static assets to ASP.NET Core`); 363 files changed โดย static files 351 รายการเป็น Git rename 100%
- **Files changed:** `OTP.csproj`, `Controllers/HomeController.cs`, Razor shared/config files, `tests/OTP.Web.Tests/RouteRenderingTests.cs`, static directories ที่ย้าย และเอกสาร migration/plan
- **Rollback:** revert commit ของ Step 4; static assets จะกลับไปตำแหน่ง legacy ด้วย Git rename history โดย Step 3 controller tests ยังคงอยู่
