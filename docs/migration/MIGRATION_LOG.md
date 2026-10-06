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

## 2026-08-25 — Step 5: Cross-Platform Developer Experience

- **Approval:** ผู้ใช้ตอบ “เริ่ม 5” หลังได้รับรายงานและ approval gate ของ Step 4
- **Scope:** เพิ่ม run/verify wrappers และ README สำหรับ macOS/Windows เท่านั้น; ไม่มีการเปลี่ยน controller, routes, views หรือ static content และยังไม่เริ่ม final Windows verification ของ Step 6
- **Test-first RED:** เพิ่ม `DeveloperExperienceScriptTests.cs` ซึ่งเลือก Bash บน Unix และ Windows PowerShell บน Windows แล้ว exercise scripts จริงผ่าน controlled fake `dotnet`; รอบแรก fail 4/4 เพราะ `scripts/run.*` และ `scripts/verify.*` ยังไม่มี
- **Script contracts:** verify scripts ตรวจ SDK แล้วเรียก restore → build `--no-restore` → test `--no-build --no-restore`; run scripts ตรวจ SDK แล้วเรียก `dotnet run --project OTP.csproj` พร้อม forward application arguments
- **Configuration:** ใช้ `OTP_DOTNET` เพื่อเลือก dotnet executable และ `OTP_CONFIGURATION` เพื่อเลือก Debug/Release โดยค่าเริ่มต้นคือ `dotnet` และ `Debug`; ทุก script resolve project path จากตำแหน่งไฟล์ จึงไม่ขึ้นกับ current working directory
- **Initial GREEN:** developer-experience tests ผ่าน 4/4 หลังเพิ่ม Bash/PowerShell scripts และตั้ง executable bit ให้ shell scripts
- **Path-with-spaces RED/GREEN:** review พบ Bash version-check ไม่ quote `OTP_DOTNET`; เพิ่ม regression test ซึ่ง fail 1/1 ด้วย controlled executable path ที่มีช่องว่าง จากนั้น quote command invocation และยืนยัน targeted 1/1 กับ developer-experience suite 5/5 ผ่าน
- **README:** เพิ่มวิธีติดตั้ง/ตรวจ .NET 10, verify/run บน macOS และ Windows, Apple Silicon, PowerShell execution policy แบบ process-only, URL/หยุด server, custom port, manual commands, `OTP_DOTNET`, `OTP_CONFIGURATION` และ troubleshooting
- **Bash syntax:** `bash -n scripts/run.sh` และ `bash -n scripts/verify.sh` ผ่าน
- **Mac verify script:** รันด้วย workspace-local SDK 10.0.400; restore สำเร็จ, build 0 warnings/0 errors และ tests ผ่าน 25/25 ณ รอบก่อนเพิ่ม path-with-spaces case
- **Fresh final Step 5 verification:** รัน `bash -n` ทั้งสอง shell scripts และ `scripts/verify.sh` หลังเพิ่ม regression ล่าสุด; restore สำเร็จ, build 0 warnings/0 errors และ full suite ผ่าน 26/26, 0 failed, 0 skipped
- **Mac run script:** `scripts/run.sh --urls http://127.0.0.1:5190` เริ่ม Kestrel Production สำเร็จ; `GET /` ตอบ HTTP 200 `text/html`, 34,837 bytes และมี marker `วัตถุประสงค์โครงการ`; หยุดด้วย Ctrl+C อย่างสะอาด
- **PowerShell status:** เครื่อง macOS นี้ไม่มี `pwsh` หรือ `powershell`; PowerShell scripts ถูกเตรียมและมี platform-selected behavior tests แต่ syntax/runtime จริงต้องรันบน Windows ใน Step 6 จึงยังเป็น `NOT RUN`
- **Step commit:** `b35dcc0` (`docs: add cross-platform setup and verification`); เพิ่ม/แก้ 10 files รวม 498 insertions โดย shell scripts ถูกบันทึกด้วย executable mode `100755`
- **Files changed:** `scripts/run.sh`, `scripts/run.ps1`, `scripts/verify.sh`, `scripts/verify.ps1`, `README.md`, `tests/OTP.Web.Tests/DeveloperExperienceScriptTests.cs` และเอกสาร migration/plan
- **Rollback:** revert commit ของ Step 5; runtime migration และ tests จาก Step 4 ยังคงอยู่

## 2026-08-25 — Step 6: Cross-Platform Verification และส่งมอบ (เสร็จสิ้น)

- **Approval:** ผู้ใช้ตอบ “เริ่ม” หลังได้รับ approval gate สำหรับ Step 6
- **Starting state:** branch `migration/aspnet-core-net10` ที่ commit `c3994dc`; migration worktree และ `main` สะอาด
- **Fresh macOS environment:** macOS 26.6.2 build 25G83, arm64, workspace-local .NET SDK 10.0.400
- **Fresh macOS verify:** `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet bash scripts/verify.sh` ที่ commit `c3994dc` สำเร็จ; restore ผ่าน, build 0 warnings/0 errors และ tests 26/26 ผ่าน, 0 failed, 0 skipped
- **Fresh macOS runtime:** `scripts/run.sh --urls http://127.0.0.1:5191` เริ่ม Kestrel ใน Production; compatibility routes 11/11 ตอบ HTTP 200 `text/html`; legacy logo ตอบ HTTP 200 `image/png`, 30,127 bytes; หยุด process ด้วย Ctrl+C อย่างสะอาด
- **Windows runner inventory:** ไม่พบ `pwsh`, Windows PowerShell, Docker, Podman, QEMU หรือ GitHub CLI บนเครื่อง Mac นี้ จึงไม่มี local Windows execution environment
- **Remote check:** `git ls-remote origin` ติดต่อ `https://github.com/Patllom/OTP.git` สำเร็จแต่ไม่มี refs; branch migration ยังอยู่เฉพาะ local และ Windows CI ยังเข้าถึง source ไม่ได้
- **Current blocker:** acceptance criteria ต้องมีการรัน `verify.ps1` และ runtime smoke บน Windows จริง; ทางเลือกคืออนุมัติให้เพิ่ม Windows GitHub Actions และ push branch หรือให้ผู้ใช้รัน scripts บนเครื่อง Windows แล้วส่ง output กลับมา
- **External writes:** ยังไม่มีการ push, สร้าง workflow run หรือเปลี่ยน remote state
- **Windows CI approval:** ผู้ใช้อนุมัติให้เพิ่ม GitHub Actions, push เฉพาะ `migration/aspnet-core-net10`, ติดตาม Windows verification และห้าม merge/release
- **Workflow preparation:** เพิ่ม `.github/workflows/windows-verify.yml` สำหรับ `windows-latest`; permissions `contents: read`, timeout 20 นาที, branch-specific/manual triggers, PowerShell verify และ runtime probes 11 routes + logo พร้อม process-tree cleanup
- **Action provenance:** ตรวจ releases/tags ทางการและ pin `actions/checkout` v6.0.2 ที่ `de0fac2e...` กับ `actions/setup-dotnet` v6.0.0 ที่ `a98b5685...`
- **Pre-push checks:** YAML parse ผ่าน, `git diff --check` ผ่าน, largest tracked blob ประมาณ 7.9 MB และ targeted history scan ไม่พบ credential/private key; `connectionStrings` ที่พบเป็น commented Visual Studio transform examples เท่านั้น
- **Workflow commit:** `481dfdb` (`ci: verify migration on Windows`) เพิ่ม workflow และบันทึก approval/decision โดยไม่แก้ production code
- **Push diagnostics:** HTTPS push รอบแรกหยุดก่อนส่งข้อมูลเพราะเครื่องไม่มี GitHub HTTPS credential; ยืนยัน SSH authentication เป็นผู้ใช้ `Patllom`, ตรวจ object database ด้วย `git fsck`, ทดสอบ `git push --dry-run` แล้ว push ซ้ำผ่าน SSH พร้อม keepalive
- **External write result:** push สำเร็จเฉพาะ branch `migration/aspnet-core-net10` ไป `origin`; ไม่มีการแก้ `main`, merge, tag, deployment หรือ release
- **Windows workflow evidence:** GitHub Actions run `32839304728` สำหรับ commit `481dfdbd9d039147ec15ec6546fe0d0c04bd01d8` จบสถานะ `success` เมื่อ 2026-08-25 เวลา 10:52:27 UTC; job ใช้เวลาประมาณ 1 นาที 3 วินาที
- **Fresh Windows environment:** GitHub-hosted `windows-latest` รายงาน Windows Server 2025 Datacenter 64-bit, OS version 10.0.26100, RID `win-x64`, .NET SDK 10.0.400, MSBuild 18.9.6 และ .NET runtime 10.0.11
- **Fresh Windows verify:** `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1` สำเร็จ; restore ผ่าน, build 0 warnings/0 errors และ full suite ผ่าน 26/26, 0 failed, 0 skipped
- **Fresh Windows runtime:** `scripts/run.ps1` เริ่ม Kestrel ใน Production; compatibility routes 11/11 ตอบ HTTP 200 `text/html; charset=utf-8`; legacy logo ตอบ HTTP 200 `image/png`, 30,127 bytes และ cleanup ปิด process tree สำเร็จ
- **Platform evidence boundary:** การยืนยัน Windows x64 ใช้ Windows Server 2025 ตาม runner ที่ผู้ใช้อนุมัติ ไม่ได้รันตรงบน Windows 11 client edition; ความแตกต่างนี้ไม่ขวาง acceptance ของ ASP.NET Core/.NET 10 runtime แต่ต้องทำ manual smoke เพิ่มหากต้องการรับรอง edition นั้นโดยชื่อ
- **Final macOS re-verification:** หลังบันทึกผล Windows รัน `scripts/verify.sh` ซ้ำด้วย SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors และ tests 26/26 ผ่าน
- **Documentation completeness:** `git diff --check` ผ่าน; matches ของ `NOT RUN`/`FAIL` ที่คงอยู่ใน test log ถูกระบุเป็น baseline/test-first history หรือ Windows 11 client-edition optional smoke ไม่ใช่ current acceptance failure
- **Conclusion:** acceptance สำหรับ macOS arm64 และ Windows x64 ผ่านครบทั้ง restore/build/test/run; final branch state ต้องสะอาดและตรงกับ remote หลัง commit ส่งมอบ โดยยังคงห้าม merge/release จนกว่าผู้ใช้จะสั่งแยกต่างหาก

## 2026-08-25 — Post-migration: Website Design Guide

- **Approval 1:** ผู้ใช้ขอให้เก็บ CSS และ design เป็น `design.md` เพื่อใช้เป็นรูปแบบพัฒนาเว็บไซต์ จากนั้นอนุมัติ short design ที่ยึด landing-page preview เป็น baseline และกำหนดว่ายังไม่ commit/push
- **Approval 2:** หลังตรวจไฟล์ ผู้ใช้อนุมัติให้ commit และ push ไปที่ branch migration
- **Scope:** เพิ่ม design documentation และบันทึกรายการนี้เท่านั้น; ไม่แก้ Razor, production CSS, routes, controller หรือ assets และไม่ merge/release
- **Sources:** สกัดจาก `LANDING_PAGE_PREVIEW.html`, ตรวจเทียบ `Views/Home/Index.cshtml`, `Views/Shared/_LayoutMain.cshtml` และข้อควรระวังใน `LANDING_PAGE_REVIEW.md`
- **Output:** `design.md` ครอบคลุม design direction, color/type/layout tokens, component patterns, canonical CSS, asset baseline, responsive/accessibility rules, Do/Don't, implementation checklist และ change policy
- **Legacy boundary:** ไม่คัดลอก theme CSS กว่า 13,000 บรรทัดเป็นมาตรฐานใหม่; ระบุ `style.css`/`responsive.css` เป็น compatibility layer และให้ canonical CSS เป็นฐานสำหรับ stylesheet ใหม่ในอนาคต
- **Documentation verification:** ไม่พบ placeholder หรือ trailing whitespace, Markdown code fence ครบคู่, CSS block มีจำนวนวงเล็บปีกกาเปิด/ปิดเท่ากัน 51/51 และ local assets ที่อ้างอิงมีอยู่ครบ
- **External-write boundary:** push เฉพาะ `migration/aspnet-core-net10`; ไม่มีการเปลี่ยน `main`, merge, tag, deploy หรือ release
- **Rollback:** revert commit ของ design guide หรือลบ `design.md` และรายการ log นี้ โดยไม่กระทบ runtime migration

## 2026-08-26 — Post-PR Review Step 1: Solution-Level Test Discovery

- **Approval:** ผู้ใช้อนุมัติให้แก้ review findings ทีละ step และกำหนดให้เสนอขอบเขตก่อนเริ่มทุก step; รอบนี้ทำเฉพาะการเพิ่ม test project เข้า solution และหยุดก่อน Step 2
- **Review finding:** `OTT.sln` มีเฉพาะ `OTP.csproj` ทำให้ `dotnet test OTT.sln` exit 0 โดยไม่ค้นพบหรือรัน migration tests
- **Test-first RED:** เพิ่ม `SolutionConfigurationTests.SolutionIncludesWebTestProject`; targeted run fail 1/1 เพราะผล `dotnet sln OTT.sln list` มีเพียง `OTP.csproj`
- **Implementation:** เพิ่ม `tests/OTP.Web.Tests/OTP.Web.Tests.csproj` เข้า `OTT.sln` ภายใต้ solution folder `tests` พร้อม Debug/Release `Any CPU` mappings โดยไม่แก้ production code
- **Targeted GREEN:** `SolutionConfigurationTests` ผ่าน 1/1 และยืนยัน solution tooling แสดง `tests/OTP.Web.Tests/OTP.Web.Tests.csproj`
- **Solution verification:** `dotnet test OTT.sln --configuration Debug --no-restore` ค้นพบ `OTP.Web.Tests.dll` และผ่าน 27/27, 0 failed, 0 skipped
- **Full macOS verification:** `scripts/verify.sh` ด้วย workspace-local .NET SDK 10.0.400 restore สำเร็จ, build 0 warnings/0 errors และ full suite ผ่าน 27/27
- **Windows verification:** GitHub Actions run `32878509685` สำหรับ implementation commit `135ca4b` จบ `success`; PowerShell restore/build/test wrapper และ compatibility route smoke tests ผ่านทุกขั้น
- **Scope boundary:** ยังไม่แก้ working-directory/global.json behavior ของ scripts, Development/Production environment, HTTPS/proxy configuration, production UI หรือ `main`
- **Rollback:** revert การแก้ `OTT.sln`, regression test และบันทึก Step 1 นี้; migration implementation ก่อน review จะยังอยู่ครบ

## 2026-08-26 — Post-PR Review Step 2: Script Working Directory และ global.json

- **Approval:** ผู้ใช้อนุมัติ Step 2 หลังได้รับขอบเขต RED/GREEN, macOS/Windows verification, documentation และ approval gate ก่อน Step 3
- **Review finding:** run/verify wrappers resolve absolute project paths แต่ไม่เปลี่ยน working directory ก่อนเรียก `dotnet`; การเลือก SDK จึงอาจไม่พบ repository `global.json` เมื่อเรียก script จาก directory อื่น
- **Test-first RED:** เพิ่ม behavior theory ที่รัน `run` และ `verify` scripts จริงจาก temporary caller directory พร้อม fake `dotnet` ที่บันทึก current directory; fail 2/2 เพราะทุก invocation ยังคงทำงานใน caller directory
- **Implementation:** เพิ่ม `cd -- "$otp_project_root"` ใน Bash wrappers และ `Set-Location -LiteralPath $otpProjectRoot` ใน PowerShell wrappers ก่อนตรวจ SDK และเรียก `dotnet`; project paths และ argument forwarding เดิมไม่เปลี่ยน
- **Targeted GREEN:** working-directory regression ผ่าน 2/2; `run` บันทึก project root 2 calls และ `verify` บันทึก project root 4 calls
- **Bash syntax:** `bash -n` ผ่านทั้ง `scripts/run.sh` และ `scripts/verify.sh`
- **Outside-root macOS verification:** เรียก absolute `scripts/verify.sh` จาก `/tmp` ด้วย workspace-local .NET SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors และ full suite ผ่าน 29/29
- **Outside-root macOS runtime:** เรียก absolute `scripts/run.sh` จาก `/tmp` ที่ port 5192; content root เป็น migration worktree, หน้าแรกตอบ HTTP 200 พร้อม marker, logo ตอบ HTTP 200 `image/png` และ process/port ปิดสะอาด
- **Windows verification:** GitHub Actions run `32879477776` สำหรับ implementation commit `0d0620e` จบ `success`; PowerShell restore/build/test wrapper รวม working-directory regression และ compatibility route smoke tests ผ่านทุกขั้น
- **Documentation:** README ระบุว่า wrappers เปลี่ยนไป project root เพื่อให้เรียกจาก directory อื่นได้โดยยังใช้ SDK policy จาก repository `global.json`
- **Scope boundary:** ยังไม่เปลี่ยน Development/Production defaults, HTTPS/proxy contract, controller, routes, views, static assets หรือ `main`
- **Rollback:** revert การแก้ scripts, regression tests, README และบันทึก Step 2 นี้; Step 1 solution discovery จะยังอยู่ครบ

## 2026-08-26 — Post-PR Review Step 3: Development Default และ Production HTTPS Contract

- **Approval:** ผู้ใช้สั่ง “ดำเนินการ” หลังได้รับขอบเขต environment tests, Development default, reverse-proxy/TLS documentation, macOS/Windows verification และข้อห้าม merge
- **Official guidance:** ตรวจเอกสาร ASP.NET Core .NET 10 ทางการเรื่อง runtime environments, HTTPS, reverse proxies และ Kestrel security; environment ที่ไม่กำหนดจะ default เป็น Production และ forwarded headers ต้องจำกัด trusted proxies/networks
- **Test-first RED:** เพิ่ม contract test ที่รัน wrapper จริงสองรอบ; default case fail เพราะ fake `dotnet` เห็น environment ว่าง 2/2 calls ขณะที่ configured Production case ถูกเตรียมไว้เพื่อป้องกันการทับค่า
- **Implementation:** Bash export `ASPNETCORE_ENVIRONMENT` เป็น Development เฉพาะเมื่อไม่มีค่า; PowerShell กำหนด Development เฉพาะเมื่อ environment ว่าง; verify scripts และ `Program.cs` ไม่เปลี่ยน
- **Targeted GREEN:** environment contract ผ่าน 1/1 และยืนยันทุก fake `dotnet` invocation เห็น Development ใน default run กับ Production ใน configured run
- **Development runtime smoke:** เรียก run wrapper จาก `/tmp` โดยไม่กำหนด environment ที่ port 5193; log แสดง Development, หน้าแรก/โลโก้ตอบ HTTP 200 และ port ปิดสะอาด
- **Production override smoke:** เรียก run wrapper จาก `/tmp` ด้วย `ASPNETCORE_ENVIRONMENT=Production` ที่ port 5194; log คง Production, หน้าแรก/โลโก้ตอบ HTTP 200 และ port ปิดสะอาด
- **Full macOS verification:** workspace-local SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors, `scripts/verify.sh` และ solution-level tests ผ่าน 30/30
- **Windows verification:** GitHub Actions run `32880635141` สำหรับ implementation commit `0a72b6b` จบ `success`; PowerShell restore/build/test wrapper รวม environment contract และ compatibility route smoke tests ผ่านทุกขั้น
- **Production contract:** เพิ่ม `docs/deployment/PRODUCTION_HTTPS.md`; public TLS/HTTP redirect/HSTS อยู่ที่ trusted edge, Kestrel อยู่ private, forwarded headers ต้องรอ deployment-specific KnownProxies/KnownNetworks และ middleware ordering
- **Scope boundary:** ไม่ deploy, ไม่ออก certificate, ไม่เลือก hosting provider, ไม่เปิด unconditional HTTPS redirect/forwarded headers, ไม่แก้ UI หรือ `main`
- **Rollback:** revert run wrappers, environment test, README, production contract และบันทึก Step 3 นี้; Step 1–2 จะยังอยู่ครบ

## 2026-08-26 — Final Cleanup: PowerShell Session State และ PR Hygiene

- **Approval:** ผู้ใช้อนุมัติ Final Cleanup หลัง independent review ไม่พบ Critical/Important และพบเฉพาะ Minor เรื่อง PR-wide whitespace, PowerShell session side effects, navigation spec gap และ PR metadata ที่ล้าสมัย
- **Review finding:** เมื่อเรียก `run.ps1` หรือ `verify.ps1` โดยตรงใน interactive PowerShell, `Set-Location` เปลี่ยน working directory ของ caller ถาวร และ `run.ps1` ทิ้งค่า Development ไว้ใน `ASPNETCORE_ENVIRONMENT` เมื่อเดิมไม่มีค่า
- **Test-first RED:** เพิ่ม Windows-only behavior theory ที่เรียก PowerShell wrappers จริงจาก temporary caller session แล้วอ่าน location/environment หลัง script จบ; GitHub Actions run `32882673103` fail 2/2 ตามคาด เพราะทั้ง `run` และ `verify` คง location ไว้ที่ repository แทน caller directory (suite 30 passed, 2 failed)
- **Implementation:** `run.ps1` และ `verify.ps1` เก็บ caller location แล้วคืนค่าใน `finally`; `run.ps1` เก็บทั้งสถานะมี/ไม่มีและค่าเดิมของ `ASPNETCORE_ENVIRONMENT` แล้ว restore หลัง `dotnet run`; exit code ของ child process ยังคงถูกส่งกลับ
- **Windows GREEN:** GitHub Actions run `32883111200` สำหรับ implementation commit `dcd87c3` จบ `success`; PowerShell verification รวม session-state regression และ compatibility route/static smoke tests ผ่านทุกขั้น
- **macOS verification:** เรียก absolute `scripts/verify.sh` จาก `/tmp` ด้วย SDK 10.0.400; build 0 warnings/0 errors และ Debug suite ผ่าน 32/32; `dotnet test OTT.sln --configuration Release` ผ่าน 32/32 เช่นกัน
- **Verification diagnostic:** การรัน Debug build สองคำสั่งพร้อมกันทำให้ static-web-assets cache ถูกล็อกชั่วคราว 1 รอบ; rerun full verify แบบลำดับเดียวผ่าน 32/32 จึงไม่ใช่ repository regression
- **PR hygiene:** ลบบรรทัดว่างเกินท้าย `Program.cs`, `appsettings.json` และ `global.json` เพื่อให้ range-level `git diff --check` ผ่าน
- **Navigation decision:** ตาม D-009 เลื่อน conversion จาก legacy URL concatenation ไป follow-up หลัง migration เพราะ links ปัจจุบัน render ได้และการเปลี่ยน link generation ก่อน merge อาจเปลี่ยน user-visible default-route URLs; follow-up ต้องเริ่มจาก rendered-link assertions
- **Scope boundary:** ไม่แก้ controller/routes/UI/assets, ไม่อัปเดต PR description ใน code step นี้ และไม่ merge/release
- **Rollback:** revert test commit `ee45c6a`, implementation commit `dcd87c3` และ final-cleanup documentation/whitespace commit โดย Step 1–3 ก่อนหน้ายังคงอยู่ครบ

## 2026-08-27 — Environment Operating Model

- **Decision:** Development และ local preview ทำบน macOS; production รันบน Windows Server ผ่าน IIS
- **Development runtime:** ใช้ Kestrel ผ่าน `scripts/run.sh` หรือ `dotnet run` โดย workflow ประจำวันไม่พึ่ง IIS Express
- **Production runtime:** ใช้ Release publish output กับ IIS, .NET Hosting Bundle และ ASP.NET Core Module v2 พร้อมกำหนด `ASPNETCORE_ENVIRONMENT=Production`
- **Compatibility boundary:** คง Windows scripts และ Windows verification ไว้เพื่อป้องกัน regression แม้เครื่องพัฒนาหลักเป็น Mac
- **Documentation:** เพิ่ม environment matrix ใน `README.md` และปรับ `docs/deployment/PRODUCTION_HTTPS.md` ให้ระบุ IIS บน Windows Server เป็น production target
- **Scope boundary:** เป็นการบันทึก operating model เท่านั้น; ไม่แก้ application code, IIS server, certificate, deployment หรือ production data

## 2026-08-27 — IIS Workshop 2 Image Filename Portability

- **Approval:** ผู้ใช้อนุมัติให้แก้ปัญหารูป Workshop 2 ไม่แสดงบน Windows Server/IIS และให้ดำเนินการตรวจสอบกับสร้าง production package ใหม่จนเสร็จ
- **Root cause:** ZIP production เดิมมีรูป Workshop 2 จำนวน 17 ไฟล์ที่ใช้ชื่อภาษาไทย แต่ archive entries ไม่ได้ตั้ง UTF-8 filename flag ทำให้ Windows อาจแตกชื่อไฟล์เป็น mojibake และ URL จาก Razor จึงหาไฟล์จริงไม่พบ
- **Test-first RED:** เปลี่ยน regression expectation เป็นชื่อ ASCII `workshop2-01.jpg` ถึง `workshop2-17.jpg`; targeted test fail 1/1 เพราะ view ยังคง render ชื่อเดิม
- **Implementation:** เปลี่ยนชื่อไฟล์ Workshop 2 ทั้ง 17 ไฟล์เป็น ASCII ตามลำดับและอัปเดต `Views/Home/eventpic.cshtml` ให้ render path ใหม่ โดยไม่เปลี่ยนภาพหรือรูปแบบ gallery
- **Targeted GREEN:** `EventPicturePageRendersEverySecondWorkshopPhotoFromLocalAssets` ผ่าน 1/1 หลังแก้ path
- **Full verification:** Release restore/build สำเร็จ, build 0 warnings/0 errors และ full test suite ผ่าน 41/41; `git diff --check` ผ่าน
- **Filename audit:** `wwwroot` มี 416 ไฟล์และไม่มีชื่อไฟล์ non-ASCII; source/view/test ไม่มี reference ชื่อ Workshop 2 เดิมตกค้าง
- **Production package:** สร้าง `OTP-PRODUCTION-IIS-C-inetpub-WEBWORK-OTP-20260827-081005.zip` สำหรับคัดลอกตรงไปยัง `C:\inetpub\WEBWORK\OTP`; ZIP SHA-256 คือ `28bab26c81c2c7927440f4cf63882b013124d8af1191e52c793100eb5bae7dd2`
- **Archive verification:** `unzip -t` ผ่าน, internal manifest ผ่าน 545/545 ไฟล์, archive มี 0 non-ASCII filenames, มีรูป Workshop 2 ครบ 17 ไฟล์ และไม่มี wrapper directory ซ้อน
- **Extracted-package smoke:** รันไฟล์ที่แตกจาก ZIP ด้วย environment Production; routes ผ่าน 11/11, local static assets ผ่าน 156/156 และ Workshop 2 JPEG URLs ผ่าน 17/17 ด้วย HTTP 200
- **Deployment boundary:** ยังไม่ได้คัดลอกไฟล์ไปยัง Windows Server, แก้ IIS, commit, push หรือสร้าง PR; ผู้ดูแลต้องสำรองและหยุด Application Pool ก่อนแทนที่ไฟล์ production
