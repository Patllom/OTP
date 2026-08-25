# OTP

เว็บไซต์ OTP ใช้ ASP.NET Core MVC บน .NET 10 และใช้ codebase เดียวกันสำหรับ macOS และ Windows

## สิ่งที่ต้องติดตั้ง

- .NET 10 SDK — โปรเจกต์กำหนด SDK `10.0.400` และยอมรับ patch ที่ใหม่กว่าใน feature band เดียวกันผ่าน `global.json`
- Git หากต้องการ clone หรือทำงานร่วมกับ repository

ตรวจ SDK ก่อนเริ่ม:

```text
dotnet --version
```

ผลลัพธ์ต้องเป็น .NET SDK 10 ที่เข้ากับ `global.json` ถ้าคำสั่งไม่ทำงาน ให้ติดตั้ง .NET 10 SDK จากเว็บไซต์ .NET ก่อน

## macOS

บน Apple Silicon ให้เลือก .NET SDK สำหรับ macOS Arm64; เครื่อง Intel ให้เลือกแพ็กเกจ x64

```bash
git clone https://github.com/Patllom/OTP.git
cd OTP
./scripts/verify.sh
./scripts/run.sh
```

ถ้า executable bit หายไประหว่างการคัดลอกไฟล์ สามารถรันแบบนี้ได้:

```bash
bash scripts/verify.sh
bash scripts/run.sh
```

## Windows 10/11

เปิด PowerShell ที่ directory ของโปรเจกต์:

```powershell
git clone https://github.com/Patllom/OTP.git
Set-Location OTP
.\scripts\verify.ps1
.\scripts\run.ps1
```

ถ้า PowerShell ไม่อนุญาตให้รัน local scripts ให้เปลี่ยน policy เฉพาะ process ปัจจุบัน:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\verify.ps1
```

หรือเรียกจาก Command Prompt โดยไม่แก้ policy ถาวร:

```text
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run.ps1
```

## เปิดเว็บไซต์

เมื่อ `run` script แสดงว่า application กำลังฟังอยู่ ให้เปิด URL ที่แสดงใน terminal โดยค่าปริยายมักเป็น:

```text
http://localhost:5000
```

หยุด server ด้วย `Ctrl+C`

หาก port ถูกใช้งานแล้ว ให้เลือก port อื่น

macOS:

```bash
./scripts/run.sh --urls http://127.0.0.1:5180
```

Windows PowerShell:

```powershell
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5180'
.\scripts\run.ps1
```

Run/verify scripts จะเปลี่ยน working directory ไปที่ project root ก่อนตรวจ SDK และเรียก `dotnet` เสมอ จึงสามารถเรียก script ด้วย absolute path จาก directory อื่นได้โดยยังใช้ SDK policy จาก `global.json` ของ repository นี้

## ตรวจสอบด้วยคำสั่งมาตรฐาน

`verify` scripts จะตรวจ SDK แล้วทำ restore, build และ test ตามลำดับ หากต้องการรันเอง:

```text
dotnet restore tests/OTP.Web.Tests/OTP.Web.Tests.csproj
dotnet build tests/OTP.Web.Tests/OTP.Web.Tests.csproj --no-restore --configuration Debug
dotnet test tests/OTP.Web.Tests/OTP.Web.Tests.csproj --no-build --no-restore --configuration Debug
dotnet run --project OTP.csproj
```

เลือก Release configuration สำหรับ verification ได้ด้วย environment variable

macOS:

```bash
OTP_CONFIGURATION=Release ./scripts/verify.sh
```

Windows PowerShell:

```powershell
$env:OTP_CONFIGURATION = 'Release'
.\scripts\verify.ps1
```

หาก `dotnet` ไม่อยู่ใน `PATH` สามารถกำหนด executable ที่ต้องการด้วย `OTP_DOTNET`

macOS:

```bash
OTP_DOTNET=/absolute/path/to/dotnet ./scripts/verify.sh
```

Windows PowerShell:

```powershell
$env:OTP_DOTNET = 'C:\Program Files\dotnet\dotnet.exe'
.\scripts\verify.ps1
```

## แก้ปัญหาเบื้องต้น

- **ไม่พบ SDK หรือ SDK ไม่ตรง:** รัน `dotnet --list-sdks` และตรวจว่ามี SDK ที่ตรงกับ `global.json`
- **Address already in use:** หยุด process ที่ใช้ port เดิม หรือกำหนด `ASPNETCORE_URLS` เป็น port ใหม่ตามตัวอย่างด้านบน
- **PowerShell execution policy:** ใช้ `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` ซึ่งมีผลเฉพาะ terminal ปัจจุบัน
- **หน้าเว็บไม่มีรูปหรือ CSS:** ตรวจว่าเริ่มแอปจาก repository นี้ผ่าน `scripts/run.*`; static files อยู่ใต้ `wwwroot` และยังใช้ URL เดิม เช่น `/asset/...`
