# Migration Test Results

## Baseline — 2026-08-25

| Test | Platform | Result | Evidence |
|---|---|---|---|
| Source inventory | macOS 26.6.2 arm64 | PASS | พบ 13 Razor views, 1 controller และ 9 actions |
| Asset inventory | macOS 26.6.2 arm64 | PASS | พบ 322 files รวม 94,789,916 bytes |
| Baseline checksum manifest | macOS 26.6.2 arm64 | PASS | `shasum -c` ยืนยัน 382/382 ไฟล์เป็น `OK` |
| Local Git baseline | macOS 26.6.2 arm64 | PASS | Initial commit `d0afeec`; ignored build/dependency directories ไม่ถูก stage |
| Legacy build | macOS 26.6.2 arm64 | NOT RUN | .NET Framework/MSBuild ไม่มีบน macOS baseline |
| Workspace-local .NET SDK | macOS 26.6.2 arm64 | PASS | SDK 10.0.400, ASP.NET Core Runtime 10.0.11, RID `osx-arm64` |
| ASP.NET Core restore | macOS 26.6.2 arm64 | PASS | `dotnet restore OTP.csproj` exit 0 |
| ASP.NET Core scaffold build | macOS 26.6.2 arm64 | PASS | Debug build, 0 warnings, 0 errors |
| Scaffold runtime health | macOS 26.6.2 arm64 | PASS | `GET /migration-health` ตอบ HTTP 200 พร้อม status `ok` |
| Controller contract RED | macOS 26.6.2 arm64 | EXPECTED FAIL | 9/9 cases fail เพราะ Home actions ยังไม่อยู่ใน action descriptors |
| Controller contract GREEN | macOS 26.6.2 arm64 | PASS | 9/9 actions ถูก discover และคืน `ViewResult` |
| ASP.NET Core build after controller migration | macOS 26.6.2 arm64 | PASS | Debug build, 0 warnings, 0 errors |
| Test dependency vulnerability audit | NuGet sources | PASS | ไม่พบ vulnerable direct/transitive packages ณ เวลาที่ตรวจ |
| Razor/routes/static RED | macOS 26.6.2 arm64 | EXPECTED FAIL | 12/12 cases fail: legacy/root pages 10 cases เป็น 500, Error และ logo เป็น 404 ก่อน migration |
| Version-controlled asset move inventory | Git worktree | PASS | `wwwroot/asset` ตรงกับ pre-Step-4 Git tree 321/321; baseline รายการที่ 322 คือ ignored `Thumbs.db` |
| Razor compile after compatibility edits | macOS 26.6.2 arm64 | PASS | Debug build, 0 warnings, 0 errors |
| Initial route GREEN attempt | macOS 26.6.2 arm64 | DIAGNOSTIC FAIL | ผ่าน Error/static 2 cases; legacy pages 10 cases เป็น 500 เพราะ layout render section `Scripts` ซ้ำ |
| Route rendering and static contract GREEN | macOS 26.6.2 arm64 | PASS | 12/12 cases ผ่านหลังลบ duplicate section invocation |
| Full ASP.NET Core test suite after view migration | macOS 26.6.2 arm64 | PASS | 21/21 tests ผ่าน, 0 failed, 0 skipped |
| Production runtime compatibility routes | macOS 26.6.2 arm64 | PASS | Kestrel Production: 11/11 routes ตอบ HTTP 200 `text/html` |
| Production runtime static logo | macOS 26.6.2 arm64 | PASS | legacy URL ตอบ HTTP 200, `image/png`, 30,127 bytes |
| Developer scripts contract RED | macOS 26.6.2 arm64 | EXPECTED FAIL | 4/4 cases fail เพราะ run/verify scripts ยังไม่มี |
| Developer scripts initial GREEN | macOS 26.6.2 arm64 | PASS | 4/4 behavior cases ผ่านด้วย controlled fake SDK |
| Custom SDK path with spaces RED | macOS 26.6.2 arm64 | EXPECTED FAIL | 1/1 case fail เพราะ Bash version-check ไม่ quote executable path |
| Custom SDK path with spaces GREEN | macOS 26.6.2 arm64 | PASS | targeted case 1/1 และ developer scripts suite 5/5 ผ่านหลัง quote invocation |
| Bash syntax | macOS 26.6.2 arm64 | PASS | `bash -n` ผ่านทั้ง `run.sh` และ `verify.sh` |
| macOS verify script before final Step 5 check | macOS 26.6.2 arm64 | PASS | SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors, tests 25/25 ผ่านก่อนเพิ่ม regression case ล่าสุด |
| Fresh final Step 5 verification | macOS 26.6.2 arm64 | PASS | Bash syntax ผ่าน; restore ผ่าน, build 0 warnings/0 errors, full suite 26/26 ผ่าน, 0 failed, 0 skipped |
| macOS run script | macOS 26.6.2 arm64 | PASS | Kestrel Production ที่ port 5190; `GET /` ตอบ 200 `text/html`, 34,837 bytes และ marker ถูกต้อง |
| Step 6 fresh macOS verification | macOS 26.6.2 arm64 | PASS | commit `c3994dc`, SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors, tests 26/26 ผ่าน |
| Step 6 fresh macOS runtime routes | macOS 26.6.2 arm64 | PASS | Kestrel Production port 5191; compatibility routes 11/11 ตอบ HTTP 200 `text/html`; logo ตอบ 200 `image/png` |
| Final macOS re-verification after Windows evidence | macOS 26.6.2 arm64 | PASS | SDK 10.0.400; restore ผ่าน, build 0 warnings/0 errors และ full suite 26/26 ผ่าน |
| PowerShell scripts and Windows build/test | Windows Server 2025 Datacenter x64 | PASS | GitHub Actions run `32839304728`; SDK 10.0.400, restore ผ่าน, build 0 warnings/0 errors และ tests 26/26 ผ่าน |
| Windows runtime compatibility routes | Windows Server 2025 Datacenter x64 | PASS | Kestrel Production: compatibility routes 11/11 ตอบ HTTP 200 `text/html; charset=utf-8` |
| Windows static logo and process cleanup | Windows Server 2025 Datacenter x64 | PASS | legacy logo ตอบ HTTP 200 `image/png`, 30,127 bytes และ `taskkill` ปิด process tree สำเร็จ |
| Exact Windows 11 client-edition smoke | Windows 11 x64 | NOT RUN | Standard `windows-latest` x64 runner ที่อนุมัติเป็น Windows Server 2025; Windows x64 acceptance ผ่านแล้ว แต่ต้องรัน manual เพิ่มหากต้องการรับรอง client edition โดยชื่อ |

กติกา: `EXPECTED FAIL`/`DIAGNOSTIC FAIL` ด้านบนเป็นหลักฐาน test-first และ defect history ไม่ใช่สถานะปัจจุบัน ส่วน `NOT RUN` ไม่ถือว่า PASS ปัจจุบัน acceptance สำหรับ macOS arm64 และ Windows x64 ผ่าน build/test/run แล้ว; รายการ Windows 11 ที่เหลือเป็น edition-specific manual smoke ไม่ใช่ runtime blocker ของ acceptance ที่ผู้ใช้อนุมัติ
