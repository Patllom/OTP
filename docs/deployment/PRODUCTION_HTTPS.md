# Production HTTPS Contract

เอกสารนี้กำหนดขอบเขตความปลอดภัยสำหรับนำ OTP ขึ้น production หลัง migration เป็น ASP.NET Core MVC บน .NET 10 เอกสารนี้ไม่ใช่ deployment และไม่ได้ออก certificate หรือกำหนด hosting provider ให้โปรเจกต์

## Local Development

`scripts/run.sh` และ `scripts/run.ps1` กำหนด `ASPNETCORE_ENVIRONMENT=Development` เฉพาะเมื่อผู้เรียกยังไม่ได้กำหนด environment เพื่อให้ local HTTP preview ใช้ development behavior ตามเจตนา

ค่าที่กำหนดไว้แล้วจะไม่ถูกทับ ตัวอย่างการรัน production behavior แบบ local smoke:

macOS:

```bash
ASPNETCORE_ENVIRONMENT=Production ./scripts/run.sh --urls http://127.0.0.1:5180
```

Windows PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
.\scripts\run.ps1
```

คำสั่งเหล่านี้เป็น runtime smoke ภายในเครื่อง ไม่ใช่ production HTTPS deployment

## Supported Production Boundary

production ที่รองรับต้องใช้ trusted reverse proxy หรือ managed edge เป็น public endpoint:

```text
Browser -- HTTPS --> Trusted reverse proxy / managed edge -- private HTTP or HTTPS --> Kestrel
```

- Public HTTP ต้อง redirect ไป HTTPS ที่ edge
- Edge ต้องจัดการ certificate และ HSTS สำหรับ public origin
- Kestrel ต้อง bind เฉพาะ loopback หรือ private network ที่ไม่เปิดตรงสู่อินเทอร์เน็ต
- กำหนด `ASPNETCORE_ENVIRONMENT=Production` ที่ deployment environment
- ตรวจและจำกัด Host header ที่ edge
- ห้ามเชื่อ `X-Forwarded-*` จากอินเทอร์เน็ตโดยไม่มี trusted proxy boundary

`Program.cs` ยังใช้ production exception handler และ `UseHsts()` แต่ไม่มี `UseHttpsRedirection()` หรือ `UseForwardedHeaders()` แบบกว้าง เพราะ repository ยังไม่ทราบ hosting target, proxy IP หรือ trusted network จริง การเปิด forwarded headers โดยไม่จำกัด proxy ทำให้เกิด header spoofing ได้

หาก production feature ต้องใช้ original scheme/client IP เช่น secure redirects, authentication หรือ absolute URL generation ให้เพิ่ม deployment-specific configuration ดังนี้ใน step แยกต่างหาก:

1. ระบุ proxy addresses หรือ CIDR ที่เชื่อถือได้
2. กำหนด `KnownProxies` หรือ `KnownNetworks`
3. เปิดเฉพาะ `X-Forwarded-For` และ `X-Forwarded-Proto` ที่จำเป็น
4. เรียก forwarded-headers middleware ก่อน HSTS, redirects, authentication และ middleware ที่อ่าน scheme/client IP
5. ทดสอบ unknown proxy ว่าถูกปฏิเสธและ trusted proxy ว่าประมวลผลถูกต้อง

ไม่เปิด `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` เป็นค่าเริ่มต้น เพราะโหมดนี้ออกแบบสำหรับบาง cloud environments และไม่ได้กำหนด trusted proxy restrictions ให้ตรงกับ deployment นี้

## Deployment Acceptance Checklist

- [ ] Hosting target และ reverse proxy ถูกระบุชื่อ/รุ่น
- [ ] Public HTTP redirect ไป HTTPS
- [ ] HTTPS certificate chain ผ่านและมี renewal plan
- [ ] HSTS ถูกส่งจาก edge หรือ application ที่รับรู้ HTTPS scheme ถูกต้อง
- [ ] Kestrel ไม่เปิด public plain-HTTP endpoint
- [ ] Host header ถูกจำกัด
- [ ] Trusted proxy IP/network ถูกกำหนดก่อนเปิด forwarded headers
- [ ] หน้า compatibility routes และ static assets ผ่าน HTTPS smoke tests
- [ ] Error route ไม่เปิดเผย stack trace ใน Production
- [ ] Rollback target และ health check พร้อมใช้งาน

## References

- [ASP.NET Core runtime environments (.NET 10)](https://learn.microsoft.com/aspnet/core/fundamentals/environments?view=aspnetcore-10.0)
- [Configure ASP.NET Core for proxy servers and load balancers (.NET 10)](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)
- [Enforce HTTPS in ASP.NET Core (.NET 10)](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0)
- [Kestrel security considerations (.NET 10)](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/security-considerations?view=aspnetcore-10.0)
