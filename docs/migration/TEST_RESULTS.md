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
| Windows build/test | Windows 11 x64 | NOT RUN | ต้องยืนยันใน Step 6 บน Windows หรือ CI Windows runner |

กติกา: `NOT RUN` ไม่ถือว่า PASS และห้ามสรุปว่า cross-platform สมบูรณ์จนกว่า Mac และ Windows จะผ่าน build/test ตาม acceptance criteria
