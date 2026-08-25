# Migration Test Results

## Baseline — 2026-08-25

| Test | Platform | Result | Evidence |
|---|---|---|---|
| Source inventory | macOS 26.6.2 arm64 | PASS | พบ 13 Razor views, 1 controller และ 9 actions |
| Asset inventory | macOS 26.6.2 arm64 | PASS | พบ 322 files รวม 94,789,916 bytes |
| Baseline checksum manifest | macOS 26.6.2 arm64 | PASS | `shasum -c` ยืนยัน 382/382 ไฟล์เป็น `OK` |
| Local Git baseline | macOS 26.6.2 arm64 | PASS | Initial commit `d0afeec`; ignored build/dependency directories ไม่ถูก stage |
| Legacy build | macOS 26.6.2 arm64 | NOT RUN | .NET Framework/MSBuild ไม่มีบน macOS baseline |
| ASP.NET Core build | macOS 26.6.2 arm64 | NOT RUN | ยังไม่เริ่ม Step 2 และยังไม่มี .NET SDK |
| Windows build/test | Windows 11 x64 | NOT RUN | ต้องยืนยันใน Step 6 บน Windows หรือ CI Windows runner |

กติกา: `NOT RUN` ไม่ถือว่า PASS และห้ามสรุปว่า cross-platform สมบูรณ์จนกว่า Mac และ Windows จะผ่าน build/test ตาม acceptance criteria
