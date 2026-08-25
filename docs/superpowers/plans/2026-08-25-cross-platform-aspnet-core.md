# Cross-Platform ASP.NET Core Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ย้าย OTP จาก ASP.NET MVC 5/.NET Framework 4.7.2 เป็น ASP.NET Core MVC/.NET 10 ที่ build, test และ run ได้บน macOS และ Windows โดยรักษา URL และ UI เดิม

**Architecture:** เปลี่ยนโปรเจกต์แบบ in-place เป็น SDK-style ASP.NET Core MVC, ใช้ Kestrel และ conventional routing, ย้าย static files ไป `wwwroot` โดยคง public URL เดิม และเพิ่ม integration tests สำหรับทุกหน้า

**Tech Stack:** .NET 10 LTS, ASP.NET Core MVC, Razor, Kestrel, xUnit, `Microsoft.AspNetCore.Mvc.Testing`

**Spec:** `docs/superpowers/specs/2026-08-25-cross-platform-aspnet-core-design.md`

## Global Constraints

- ต้องขออนุมัติผู้ใช้ก่อนเริ่มทุก Task และหยุดหลังรายงานผล Task นั้น
- Target framework ต้องเป็น `net10.0`
- ต้องรักษา compatibility routes ทั้งสิบ URL ที่ระบุใน spec
- ห้าม redesign หรือเปลี่ยนเนื้อหา ยกเว้น compatibility fix ที่บันทึกไว้
- ทุก Task ต้องอัปเดต `docs/migration/MIGRATION_LOG.md` และ `docs/migration/TEST_RESULTS.md`
- ห้ามสรุปว่า cross-platform สมบูรณ์ก่อน Mac และ Windows ผ่าน restore/build/test/run

---

### Task 1: Baseline, Documentation และ Local Git

**Files:**
- Create: `.gitignore`
- Create: `.gitattributes`
- Create: `docs/migration/BASELINE.md`
- Create: `docs/migration/MIGRATION_LOG.md`
- Create: `docs/migration/DECISIONS.md`
- Create: `docs/migration/TEST_RESULTS.md`
- Create: `docs/migration/baseline-sha256.txt`
- Create: `docs/superpowers/specs/2026-08-25-cross-platform-aspnet-core-design.md`
- Create: `docs/superpowers/plans/2026-08-25-cross-platform-aspnet-core.md`

**Interfaces:**
- Consumes: legacy repository files as read-only input
- Produces: immutable Git baseline, checksum manifest และ approval-gated migration documents

- [x] **Step 1: บันทึก environment และ source inventory**

Run:

```bash
git --version
uname -srm
sw_vers
find Views -type f -name '*.cshtml' | wc -l
find asset -type f | wc -l
```

Expected: environment, 13 Razor views และ 322 asset files ถูกบันทึกใน `BASELINE.md`

- [x] **Step 2: สร้างและตรวจ SHA-256 manifest**

Run manifest generation over source/config/static assets, then:

```bash
test -s docs/migration/baseline-sha256.txt
shasum -c docs/migration/baseline-sha256.txt
```

Expected: checksum ทุกไฟล์รายงาน `OK`

- [x] **Step 3: เริ่ม Local Git และสร้าง baseline commit**

```bash
git init -b main
git add .
git commit -m "chore: capture legacy baseline and migration plan"
git status --short
```

Expected: commit สำเร็จและ working tree ว่าง

### Task 2: .NET 10 SDK และ ASP.NET Core Scaffold

**Files:**
- Modify: `OTP.csproj`
- Create: `Program.cs`
- Create: `global.json`
- Create: `appsettings.json`
- Remove after Git baseline: `Global.asax`, `Global.asax.cs`, `App_Start/`, `Web.config`, `Web.Debug.config`, `Web.Release.config`, `packages.config`, `Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: route requirement จาก spec และ legacy baseline commit
- Produces: `public partial class Program`, ASP.NET Core service container และ conventional MVC route

- [x] **Step 1: ตรวจหรือเตรียม workspace-local .NET 10 SDK โดยไม่แก้ system-wide configuration**

```bash
dotnet --info
dotnet --list-sdks
```

Expected: มี .NET 10 SDK; หากไม่มีให้ติดตั้งแบบ workspace-local ที่ `.dotnet/` และบันทึก version จริงก่อนทำต่อ ผลของ Step 2 คือ SDK 10.0.400 และ ASP.NET Core Runtime 10.0.11 บน macOS arm64

- [x] **Step 2: เขียน SDK-style project file**

`OTP.csproj` ต้องมีแกนดังนี้:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>OTT</RootNamespace>
    <AssemblyName>OTP</AssemblyName>
    <RazorCompileOnBuild>false</RazorCompileOnBuild>
    <RazorCompileOnPublish>false</RazorCompileOnPublish>
  </PropertyGroup>
  <ItemGroup>
    <Compile Remove="Controllers/**/*.cs" />
  </ItemGroup>
</Project>
```

Temporary exclusions ต้องถูกนำออกใน Task 3–4 หลังย้าย controller และ Razor views เพื่อให้ build ของแต่ละ checkpoint ผ่านจริง

- [x] **Step 3: เขียน HTTP pipeline ขั้นต่ำ**

`Program.cs` ต้องลงทะเบียน `AddControllersWithViews()`, `UseStaticFiles()` และ route:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
public partial class Program { }
```

- [x] **Step 4: ลบเฉพาะ startup/config files ที่ถูกแทนที่แล้ว**

ตรวจ target list แบบ read-only ก่อนใช้ `git rm` และยืนยันว่าไม่มี connection string หรือ secret ที่ถูกใช้งาน

- [x] **Step 5: Restore และ build scaffold**

```bash
dotnet restore OTP.csproj
dotnet build OTP.csproj --no-restore
```

Expected: restore และ build ต้อง exit 0; legacy controller/views ยังไม่ถูก compile ตาม temporary migration boundaries ที่บันทึกใน D-005

- [x] **Step 6: Commit Task 2**

```bash
git add OTP.csproj Program.cs global.json appsettings.json docs/migration
git commit -m "build: scaffold ASP.NET Core application"
```

### Task 3: Controller, Routing และ Integration Test Harness

**Files:**
- Modify: `Controllers/HomeController.cs`
- Modify: `OTP.csproj`
- Create: `tests/OTP.Web.Tests/OTP.Web.Tests.csproj`
- Create: `tests/OTP.Web.Tests/HomeControllerContractTests.cs`

**Interfaces:**
- Consumes: `public partial class Program` และ conventional route จาก Task 2
- Produces: ASP.NET Core `HomeController` actions เดิมและ action-descriptor test host ผ่าน `WebApplicationFactory<Program>`

- [x] **Step 1: สร้าง xUnit integration test project และแยก test sources จาก web compilation**

Test project target `net10.0` และอ้างอิง `OTP.csproj`, `Microsoft.AspNetCore.Mvc.Testing` 10.0.11, `Microsoft.NET.Test.Sdk` 17.14.1, xUnit 2.9.3 และ xUnit runner 3.1.4 ส่วน `OTP.csproj` ต้อง exclude `tests/**/*.cs` เพื่อไม่ compile test source ซ้ำใน web assembly

- [x] **Step 2: เขียน failing controller contract theory**

```csharp
[Theory]
[MemberData(nameof(LegacyActions))]
public void LegacyActionPreservesMvcViewContract(string actionName)
{
    using var scope = _factory.Services.CreateScope();
    var provider = scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
    var descriptor = Assert.Single(provider.ActionDescriptors.Items
        .OfType<ControllerActionDescriptor>()
        .Where(item => item.ControllerName == "Home" && item.ActionName == actionName));
    var controller = Activator.CreateInstance(descriptor.ControllerTypeInfo.AsType());
    var result = descriptor.MethodInfo.Invoke(controller, null);
    Assert.IsType<ViewResult>(result);
}
```

- [x] **Step 3: รัน test และยืนยัน RED เพราะ controller ยังไม่ถูก discover**

```bash
dotnet test tests/OTP.Web.Tests/OTP.Web.Tests.csproj
```

Expected: FAIL ทั้ง 9 cases ที่ `Assert.Single()` เพราะ controller ยังถูก exclude จาก web assembly

- [x] **Step 4: ย้าย HomeController แบบ minimal**

เปลี่ยน namespace import และ return types เป็น ASP.NET Core โดยคง action names และ `ViewBag.Message` จากนั้นนำ `<Compile Remove="Controllers/**/*.cs" />` ออกจาก `OTP.csproj`

- [x] **Step 5: รัน tests และ build เพื่อยืนยัน GREEN**

```bash
dotnet test tests/OTP.Web.Tests/OTP.Web.Tests.csproj
dotnet build OTP.csproj --no-restore
```

Expected: 9/9 test cases ผ่านและ build สำเร็จด้วย 0 warnings/0 errors; Razor rendering ยังถูก defer ไป Task 4

- [x] **Step 6: Commit Task 3**

```bash
git add Controllers tests docs/migration
git commit -m "test: preserve legacy MVC routes"
```

### Task 4: Razor Views และ Static Assets

**Files:**
- Modify: `Controllers/HomeController.cs`
- Modify: `OTP.csproj`
- Modify: `Views/**/*.cshtml`
- Create: `Views/_ViewImports.cshtml`
- Move: `asset/` to `wwwroot/asset/`
- Move when referenced: `Content/`, `Scripts/`, `fonts/`, `favicon.ico` to `wwwroot/`
- Create: `tests/OTP.Web.Tests/RouteRenderingTests.cs`

**Interfaces:**
- Consumes: HomeController routes จาก Task 3
- Produces: rendered HTML และ public static URLs เดิม

- [x] **Step 1: เขียน HTTP page-marker, error-route และ static-file tests**

แต่ละหน้า assertion ต้องตรวจ HTTP 200, `text/html` และข้อความ/asset marker จริงของหน้านั้น รวม `/` และ legacy Home URLs ทั้งหมด เพิ่ม `/Home/Error` เพื่อปกป้อง production exception-handler target และตรวจ logo URL แยกด้วย content type `image/png`

- [x] **Step 2: รัน tests และยืนยัน Razor compilation failures**

```bash
dotnet test tests/OTP.Web.Tests/OTP.Web.Tests.csproj
```

Expected: page cases fail ด้วย HTTP 500/404 เพราะ Razor ยังไม่ compile และ Error action ยังไม่มี; static-file case fail ด้วย HTTP 404

- [x] **Step 3: เพิ่ม ASP.NET Core tag helpers และปรับ Razor syntax**

`Views/_ViewImports.cshtml`:

```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

เปิด Razor build/publish compilation, เพิ่ม `Error()` action, ลบ legacy `Views/Web.config`, แทนที่ `Styles.Render`/`Scripts.Render` ใน fallback layout และปรับ URL syntax ที่ไม่ compatible โดยไม่เปลี่ยน destination

- [x] **Step 4: ย้าย static directories ด้วย Git**

```bash
mkdir -p wwwroot
git mv asset wwwroot/asset
git mv Content wwwroot/Content
git mv Scripts wwwroot/Scripts
git mv fonts wwwroot/fonts
git mv favicon.ico wwwroot/favicon.ico
```

- [x] **Step 5: ยืนยัน static-file integration contract**

```csharp
[Fact]
public async Task ProjectLogoIsServed()
{
    var response = await _client.GetAsync("/asset/images/content/logo1.png");
    response.EnsureSuccessStatusCode();
    Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
}
```

- [x] **Step 6: รัน build และ tests จนผ่านทั้งชุด**

```bash
dotnet build OTP.csproj
dotnet test tests/OTP.Web.Tests/OTP.Web.Tests.csproj
```

Expected: 11 page cases (รวม root alias และ Error route), static-file test และ 9 controller contract cases ผ่านทั้งหมด

- [x] **Step 7: Commit Task 4**

```bash
git add Views wwwroot tests docs/migration
git commit -m "feat: migrate views and static assets to ASP.NET Core"
```

### Task 5: Cross-Platform Developer Experience

**Files:**
- Create: `scripts/run.sh`
- Create: `scripts/run.ps1`
- Create: `scripts/verify.sh`
- Create: `scripts/verify.ps1`
- Create or Modify: `README.md`

**Interfaces:**
- Consumes: buildable `OTP.csproj` และ test project
- Produces: documented platform-neutral setup/run/verify commands

- [x] **Step 1: เขียน shell verification script**

`scripts/verify.sh` ต้องใช้ `set -euo pipefail` แล้วรัน restore, build `--no-restore` และ test `--no-build`

- [x] **Step 2: เขียน PowerShell verification script**

`scripts/verify.ps1` ต้องใช้ `$ErrorActionPreference = 'Stop'` และรันชุดคำสั่งเทียบเท่า shell script

- [x] **Step 3: เขียน run scripts**

ทั้งสอง script ต้องตรวจว่ามี .NET 10 SDK แล้วเรียก:

```text
dotnet run --project OTP.csproj
```

- [x] **Step 4: เขียน README สำหรับ macOS และ Windows**

README ต้องระบุ .NET 10 SDK, คำสั่ง restore/build/test/run, URL local, วิธีหยุด server, Apple Silicon note, PowerShell execution note และ troubleshooting สำหรับ port conflict

- [x] **Step 5: รัน Mac scripts**

```bash
bash scripts/verify.sh
bash scripts/run.sh
```

Expected: verification ผ่านและ `/` ตอบ 200 จาก local Kestrel

- [x] **Step 6: Commit Task 5**

```bash
git add scripts README.md docs/migration
git commit -m "docs: add cross-platform setup and verification"
```

### Task 6: Cross-Platform Verification และส่งมอบ

**Files:**
- Create: `.github/workflows/windows-verify.yml`
- Modify: `docs/migration/TEST_RESULTS.md`
- Modify: `docs/migration/MIGRATION_LOG.md`
- Modify: `docs/migration/DECISIONS.md` เฉพาะเมื่อมี decision ใหม่

**Interfaces:**
- Consumes: scripts และ tests จาก Task 5
- Produces: หลักฐาน Mac/Windows, final migration status และ rollback instructions

- [x] **Step 1: Fresh verification บน macOS arm64**

```bash
bash scripts/verify.sh
```

Expected: restore, build และ tests exit 0

- [x] **Step 2: Runtime smoke test บน macOS**

Start app, request compatibility routes และหยุด process อย่างสะอาด บันทึก SDK, OS, commit และผลลัพธ์

- [x] **Step 2A: เตรียม Windows GitHub Actions ตาม external-write approval**

Workflow ใช้ `windows-latest`, read-only repository permission, pinned official actions, `verify.ps1` และ runtime smoke probes โดย trigger เฉพาะ migration branch หรือ manual dispatch; ห้าม deploy, merge หรือ release

- [ ] **Step 3: Fresh verification บน Windows 11 x64**

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Expected: restore, build และ tests exit 0

- [ ] **Step 4: Runtime smoke test บน Windows**

รัน `scripts/run.ps1`, request `/` และ compatibility routes แล้วบันทึก SDK, OS, commit และผลลัพธ์

- [ ] **Step 5: ตรวจ Git diff และ documentation completeness**

```bash
git status --short
git log --oneline --decorate -10
rg -n "NOT RUN|FAIL" docs/migration/TEST_RESULTS.md
```

Expected: working tree ว่างและไม่มี `NOT RUN`/`FAIL` ใน acceptance tests

- [ ] **Step 6: Final commit**

```bash
git add docs/migration
git commit -m "docs: record cross-platform migration verification"
```
