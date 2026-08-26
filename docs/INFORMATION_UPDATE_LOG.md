# Information Page Update Log

บันทึกการเพิ่ม “ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17” ในหน้า `/Home/information` โดยใช้ฉบับที่ 16 เป็น pattern ต้นแบบ ดำเนินงานตามจุดอนุมัติของผู้ใช้ทีละขั้น และยังไม่มีการ commit, push, สร้าง pull request หรือ merge

## 2026-08-26 — ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17

### ขอบเขตงาน

- เพิ่มการ์ดฉบับที่ 17 ในตำแหน่งว่างถัดจากฉบับที่ 16
- ใช้ภาพที่ผู้ใช้แนบ 5 ไฟล์ตามลำดับเดิม
- ใช้ภาพหน้าแรกเป็น thumbnail ของการ์ด
- เมื่อคลิก thumbnail ให้เปิด modal แบบ `:target` และแสดงภาพทั้ง 5 หน้าเรียงแนวตั้ง
- รักษา carousel และ responsive pattern เดิมของหน้า
- ไม่เปลี่ยนเนื้อหา รูป หรือ interaction ของข่าวฉบับก่อนหน้า

### ขั้นตอนและการอนุมัติ

1. **ตรวจต้นแบบแบบ read-only:** ผู้ใช้อนุมัติให้ตรวจ D1 ซึ่งเป็นฉบับที่ 16 พบว่าการ์ดใช้ลิงก์ `#m16-modal`, ใช้ `m16/1.jpg` เป็น thumbnail และ modal แสดงรูปเรียงแนวตั้งโดยอาศัย CSS `:target` ไม่ได้ใช้ lightbox plugin
2. **ตรวจ responsive pattern:** `#best-seller .slider-items` ใช้ Owl Carousel แสดง 3 รายการบน Desktop, 2 รายการบน Tablet และ 1 รายการบน Mobile
3. **TDD RED:** หลังได้รับอนุมัติ เพิ่ม integration test `InformationPageRendersSeventeenthIssueFromLocalAssets` ใน `RouteRenderingTests.cs`; test ล้มตามคาดเพราะยังไม่มีข้อความ “ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17”
4. **คัดลอกรูป:** หลังได้รับอนุมัติ ตรวจว่า `wwwroot/asset/images/event/m17` ยังไม่มี แล้วคัดลอกรูปทั้ง 5 ไฟล์เป็น `1.jpg` ถึง `5.jpg` ตามลำดับที่ผู้ใช้แนบ ตรวจด้วย `cmp` แล้วตรงกับต้นฉบับทุกไฟล์
5. **สร้างการ์ดและ modal:** เพิ่มการ์ดฉบับที่ 17 ถัดจากฉบับที่ 16, เพิ่ม CSS `#m17-modal:target` และเพิ่ม modal ที่แสดงภาพทั้ง 5 หน้า
6. **TDD GREEN:** รัน test เดิมโดยไม่เปลี่ยนเกณฑ์ แล้วผ่าน 1/1
7. **Full automated verification:** หลังได้รับอนุมัติ ชุดทดสอบทั้งหมดผ่าน 35/35 และ `scripts/verify.sh` ผ่านด้วย build 0 warnings และ 0 errors
8. **Browser Desktop 1440×900:** หลังได้รับอนุมัติ รีสตาร์ต Preview ด้วยโค้ดล่าสุด พบการ์ดฉบับที่ 17 อยู่ถัดจากฉบับที่ 16, กลุ่มสุดท้ายมี 2 การ์ด และไม่มี horizontal overflow
9. **Browser modal:** คลิกการ์ดฉบับที่ 17 แล้ว URL เปลี่ยนเป็น `#m17-modal`; modal แสดงและโหลดภาพครบ 5/5 ตามลำดับ `1.jpg` ถึง `5.jpg`
10. **Browser Mobile 390×844:** รายการเป็น carousel 1 การ์ดต่อ viewport, สามารถกดปุ่ม Next เพื่อเลื่อนไปยังฉบับที่ 17, การ์ดอยู่ภายใน viewport และไม่มี horizontal overflow
11. **Browser modal บน Mobile:** ภาพแรกกว้าง 370px ภายใน viewport 390px, ภาพทั้ง 5 โหลดครบ, ปุ่มปิดอยู่ใน viewport และหน้าไม่ล้นแนวนอน
12. **คืนค่า Preview:** ยกเลิก viewport override และเปิดหน้า `/Home/information` ค้างไว้ตรงฉบับที่ 17 ที่ viewport ปกติ 857×798

### การแมปรูปต้นทาง

| ลำดับ | ไฟล์ต้นทาง | ไฟล์ในเว็บไซต์ | ขนาดต้นฉบับ |
| --- | --- | --- | --- |
| 1 | `เดือน 17-01.jpg` | `wwwroot/asset/images/event/m17/1.jpg` | 3556×2528 |
| 2 | `เดือน 17-02_0.jpg` | `wwwroot/asset/images/event/m17/2.jpg` | 3556×2528 |
| 3 | `เดือน 17-03_0.jpg` | `wwwroot/asset/images/event/m17/3.jpg` | 3556×2528 |
| 4 | `เดือน 17-04_0.jpg` | `wwwroot/asset/images/event/m17/4.jpg` | 3556×2529 |
| 5 | `เดือน 17-05_0.jpg` | `wwwroot/asset/images/event/m17/5.jpg` | 3556×2529 |

## Automated verification

- Target test RED: 0 passed, 1 failed เพราะยังไม่มีหัวข้อฉบับที่ 17
- Target test GREEN: 1 passed, 0 failed
- Full test suite: 35 passed, 0 failed, 0 skipped
- `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet ./scripts/verify.sh`
- Restore: dependencies เป็นปัจจุบัน
- Build: 0 warnings, 0 errors
- Browser Desktop: การ์ดและ modal แสดงถูกต้อง, ภาพโหลด 5/5, ไม่มี horizontal overflow
- Browser Mobile: การ์ดเข้าถึงได้ผ่าน carousel, modal อยู่ใน viewport, ภาพโหลด 5/5, ไม่มี horizontal overflow

## ไฟล์ที่เกี่ยวข้อง

- `Views/Home/information.cshtml`
- `tests/OTP.Web.Tests/RouteRenderingTests.cs`
- `wwwroot/asset/images/event/m17/1.jpg`
- `wwwroot/asset/images/event/m17/2.jpg`
- `wwwroot/asset/images/event/m17/3.jpg`
- `wwwroot/asset/images/event/m17/4.jpg`
- `wwwroot/asset/images/event/m17/5.jpg`
- `docs/INFORMATION_UPDATE_LOG.md`

## สถานะ Git

- เก็บการเปลี่ยนแปลงไว้ใน branch `migration/aspnet-core-net10`
- รักษาการเปลี่ยนแปลงเดิมที่ยังไม่ commit ใน worktree โดยไม่แก้หรือลบงานที่ไม่เกี่ยวข้อง
- ยังไม่ได้ commit, push, สร้าง pull request หรือ merge จากงานชุดนี้

## 2026-08-26 — ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 18

### ขอบเขตงาน

- เพิ่มการ์ดฉบับที่ 18 ในตำแหน่งว่างถัดจากฉบับที่ 17 โดยใช้ pattern และ style เดียวกัน
- ใช้ภาพที่ผู้ใช้แนบ 2 ไฟล์ตามลำดับเดิม
- ใช้ภาพหน้า 1 เป็น thumbnail ของการ์ด
- เมื่อคลิก thumbnail ให้เปิด modal แบบ `:target` และแสดงภาพทั้ง 2 หน้าเรียงแนวตั้ง
- รักษา carousel, responsive pattern และเนื้อหาของฉบับก่อนหน้าไว้

### ขั้นตอนการดำเนินงาน

1. **ตรวจต้นแบบแบบ read-only:** ตรวจการ์ดและ modal ของฉบับที่ 17 พบว่าการ์ดอยู่ในกลุ่มเดียวกับฉบับที่ 16 และตำแหน่ง D1 เป็นช่องลำดับที่ 3 ของกลุ่ม
2. **ตรวจไฟล์ต้นฉบับ:** ยืนยันว่าภาพทั้ง 2 ไฟล์เป็น JPEG ขนาด 3750×2651 พิกเซล
3. **TDD RED:** เพิ่ม integration test `InformationPageRendersEighteenthIssueFromLocalAssets` โดยกำหนดให้หน้าแสดงหัวข้อ ลิงก์ `#m18-modal`, modal และ local assets ทั้ง 2 ไฟล์ จากนั้นรันแล้วล้มตามคาดเพราะยังไม่มีหัวข้อฉบับที่ 18
4. **คัดลอกรูป:** สร้าง `wwwroot/asset/images/event/m18`, คัดลอกต้นฉบับเป็น `1.jpg` และ `2.jpg` แล้วตรวจด้วย `cmp` ว่าตรงกับต้นฉบับแบบ byte-for-byte
5. **สร้างการ์ด:** เพิ่มการ์ดฉบับที่ 18 ถัดจากฉบับที่ 17 ใช้ `m18/1.jpg` เป็น thumbnail และเพิ่มข้อความ “ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 18”
6. **สร้าง modal:** เพิ่ม CSS `#m18-modal:target` และ modal ที่แสดง `1.jpg` กับ `2.jpg` เรียงแนวตั้ง พร้อมปุ่มปิดและข้อความ alternative text
7. **TDD GREEN:** รัน test เดิมโดยไม่เปลี่ยนเกณฑ์แล้วผ่าน 1/1
8. **Full automated verification:** ชุดทดสอบทั้งหมดผ่าน 36/36 และ `scripts/verify.sh` ผ่านด้วย build 0 warnings และ 0 errors
9. **Browser Desktop 1440×900:** การ์ดฉบับที่ 16–18 เรียงครบ 3 คอลัมน์, ฉบับที่ 18 อยู่ใน D1, ไม่มี horizontal overflow
10. **Browser modal บน Desktop:** คลิกการ์ดแล้ว URL เปลี่ยนเป็น `#m18-modal`, modal แสดงแบบ `block`, โหลดภาพครบ 2/2 และไม่มี horizontal overflow
11. **Browser Mobile 390×844:** ใช้ปุ่ม Next ของ carousel เลื่อนไปฉบับที่ 18 ได้, การ์ดกว้าง 294px อยู่ภายใน viewport และหน้าไม่ล้นแนวนอน
12. **Browser modal บน Mobile:** ภาพแรกกว้างประมาณ 370.5px ภายใน viewport 390px, ปุ่มปิดอยู่ภายใน viewport, โหลดภาพครบ 2/2 และไม่มี horizontal overflow
13. **ตรวจเครื่องมือ Browser:** คำสั่งอ่าน `boundingBox` โดยตรงไม่รองรับกับ locator binding จึงตรวจ root cause และใช้ DOM evaluation ที่รองรับแทน โดยไม่มีการเปลี่ยนโค้ดเว็บไซต์จากเหตุการณ์นี้
14. **คืนค่า Preview:** ยกเลิก viewport override, เปิด `/Home/information` ที่ขนาดปกติ 857×798 และเลื่อนค้างไว้ตรงกลุ่มฉบับที่ 16–18

### การแมปรูปต้นทาง

| ลำดับ | ไฟล์ต้นทาง | ไฟล์ในเว็บไซต์ | ขนาดต้นฉบับ |
| --- | --- | --- | --- |
| 1 | `ข่าวประชาสัมพันธ์ฉบับที่ 18-1.jpg` | `wwwroot/asset/images/event/m18/1.jpg` | 3750×2651 |
| 2 | `ข่าวประชาสัมพันธ์ฉบับที่ 18-2.jpg` | `wwwroot/asset/images/event/m18/2.jpg` | 3750×2651 |

### ผลการตรวจสอบฉบับที่ 18

- Target test RED: 0 passed, 1 failed เพราะยังไม่มีหัวข้อฉบับที่ 18
- Target test GREEN: 1 passed, 0 failed
- Full test suite: 36 passed, 0 failed, 0 skipped
- `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet ./scripts/verify.sh`
- Restore: dependencies เป็นปัจจุบัน
- Build: 0 warnings, 0 errors
- Browser Desktop: การ์ดและ modal แสดงถูกต้อง, ภาพโหลด 2/2, ไม่มี horizontal overflow
- Browser Mobile: เข้าถึงฉบับที่ 18 ผ่าน carousel, modal อยู่ใน viewport, ภาพโหลด 2/2, ไม่มี horizontal overflow

### ไฟล์ที่เกี่ยวข้องกับฉบับที่ 18

- `Views/Home/information.cshtml`
- `tests/OTP.Web.Tests/RouteRenderingTests.cs`
- `wwwroot/asset/images/event/m18/1.jpg`
- `wwwroot/asset/images/event/m18/2.jpg`
- `docs/INFORMATION_UPDATE_LOG.md`

### สถานะ Git หลังเพิ่มฉบับที่ 18

- เก็บการเปลี่ยนแปลงไว้ใน branch `migration/aspnet-core-net10`
- รักษาการเปลี่ยนแปลงเดิมที่ยังไม่ commit ใน worktree โดยไม่แก้หรือลบงานที่ไม่เกี่ยวข้อง
- ยังไม่ได้ commit, push, สร้าง pull request หรือ merge

## 2026-08-26 — ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 19

### ขอบเขตงาน

- เพิ่มการ์ดฉบับที่ 19 ในตำแหน่ง D1 แถวใหม่ใต้กลุ่มฉบับที่ 16–18 โดยใช้ pattern และ style เดียวกัน
- ใช้ภาพ PNG ที่ผู้ใช้แนบ 3 ไฟล์ตามลำดับเดิม
- ใช้ภาพหน้า 1 เป็น thumbnail ของการ์ด
- เมื่อคลิก thumbnail ให้เปิด modal แบบ `:target` และแสดงภาพทั้ง 3 หน้าเรียงแนวตั้ง
- รักษา carousel, responsive pattern และเนื้อหาของฉบับก่อนหน้าไว้

### ขั้นตอนการดำเนินงาน

1. **ตรวจต้นแบบแบบ read-only:** ตรวจโครงสร้างการ์ดและ modal ของฉบับที่ 18 รวมถึงตำแหน่ง D1 ซึ่งเป็นพื้นที่ว่างใต้กลุ่มฉบับที่ 16–18
2. **ตรวจไฟล์ต้นฉบับ:** ยืนยันว่าภาพทั้ง 3 ไฟล์เป็น PNG แบบ RGB ขนาด 1536×1024 พิกเซล
3. **TDD RED:** เพิ่ม integration test `InformationPageRendersNineteenthIssueFromLocalAssets` โดยกำหนดให้หน้าแสดงหัวข้อ ลิงก์ `#m19-modal`, modal และ local assets ทั้ง 3 ไฟล์ จากนั้นรันแล้วล้มตามคาดเพราะยังไม่มีหัวข้อฉบับที่ 19
4. **คัดลอกรูป:** สร้าง `wwwroot/asset/images/event/m19`, คัดลอกต้นฉบับเป็น `1.png` ถึง `3.png` แล้วตรวจด้วย `cmp` ว่าตรงกับต้นฉบับแบบ byte-for-byte
5. **สร้างการ์ด:** เพิ่มกลุ่มการ์ดแถวใหม่ในตำแหน่ง D1 ใช้ `m19/1.png` เป็น thumbnail และเพิ่มข้อความ “ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 19”
6. **สร้าง modal:** เพิ่ม CSS `#m19-modal:target` และ modal ที่แสดง `1.png` ถึง `3.png` เรียงแนวตั้ง พร้อมปุ่มปิดและข้อความ alternative text
7. **TDD GREEN:** รัน test เดิมโดยไม่เปลี่ยนเกณฑ์แล้วผ่าน 1/1
8. **Full automated verification:** ชุดทดสอบทั้งหมดผ่าน 37/37 และ `scripts/verify.sh` ผ่านด้วย build 0 warnings และ 0 errors
9. **Browser Desktop 1440×900:** การ์ดฉบับที่ 19 อยู่ในตำแหน่ง D1 ใต้กลุ่มฉบับที่ 16–18, แสดง thumbnail และหัวข้อครบ และไม่มี horizontal overflow
10. **Browser modal บน Desktop:** คลิกการ์ดแล้ว URL เปลี่ยนเป็น `#m19-modal`, modal แสดงแบบ `block`, โหลดภาพครบ 3/3 ตามลำดับ และไม่มี horizontal overflow
11. **Browser Mobile 390×844:** การ์ดฉบับที่ 19 แสดงภายใน viewport, หัวข้อครบ และหน้าไม่ล้นแนวนอน
12. **Browser modal บน Mobile:** ภาพแรกกว้างประมาณ 370.5px ภายใน viewport 390px, โหลดภาพครบ 3/3, ปุ่มปิดอยู่ภายใน viewport และไม่มี horizontal overflow
13. **ตรวจตัวตรวจปุ่มปิด:** selector ที่ตัวตรวจคาดไว้ครั้งแรกไม่ตรงกับ markup จริง จึงตรวจ source และเปลี่ยนไปเลือกจาก `aria-label` ที่มีอยู่ โดยไม่ต้องแก้โค้ดหน้าเว็บ ผลตรวจปุ่มปิดจึงผ่าน
14. **คืนค่า Preview:** ยกเลิก viewport override และเปิดหน้า `/Home/information` ค้างไว้ตรงการ์ดฉบับที่ 19 ที่ viewport ปกติ

### การแมปรูปต้นทาง

| ลำดับ | ไฟล์ต้นทาง | ไฟล์ในเว็บไซต์ | ขนาดต้นฉบับ |
| --- | --- | --- | --- |
| 1 | `ข่าวประชาสัมพันธ์ฉบับที่ 19-1.png` | `wwwroot/asset/images/event/m19/1.png` | 1536×1024 |
| 2 | `ข่าวประชาสัมพันธ์ฉบับที่ 19-2.png` | `wwwroot/asset/images/event/m19/2.png` | 1536×1024 |
| 3 | `ข่าวประชาสัมพันธ์ฉบับที่ 19-3.png` | `wwwroot/asset/images/event/m19/3.png` | 1536×1024 |

### ผลการตรวจสอบฉบับที่ 19

- Target test RED: 0 passed, 1 failed เพราะยังไม่มีหัวข้อฉบับที่ 19
- Target test GREEN: 1 passed, 0 failed
- Full test suite: 37 passed, 0 failed, 0 skipped
- `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet ./scripts/verify.sh`
- Restore: dependencies เป็นปัจจุบัน
- Build: 0 warnings, 0 errors
- Browser Desktop: การ์ดและ modal แสดงถูกต้อง, ภาพโหลด 3/3, ไม่มี horizontal overflow
- Browser Mobile: การ์ดอยู่ภายใน viewport, modal และปุ่มปิดอยู่ใน viewport, ภาพโหลด 3/3, ไม่มี horizontal overflow

### ไฟล์ที่เกี่ยวข้องกับฉบับที่ 19

- `Views/Home/information.cshtml`
- `tests/OTP.Web.Tests/RouteRenderingTests.cs`
- `wwwroot/asset/images/event/m19/1.png`
- `wwwroot/asset/images/event/m19/2.png`
- `wwwroot/asset/images/event/m19/3.png`
- `docs/INFORMATION_UPDATE_LOG.md`

### สถานะ Git หลังเพิ่มฉบับที่ 19

- เก็บการเปลี่ยนแปลงไว้ใน branch `migration/aspnet-core-net10`
- รักษาการเปลี่ยนแปลงเดิมที่ยังไม่ commit ใน worktree โดยไม่แก้หรือลบงานที่ไม่เกี่ยวข้อง
- ยังไม่ได้ commit, push, สร้าง pull request หรือ merge

## 2026-08-26 — ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 20

### ขอบเขตงาน

- เพิ่มการ์ดฉบับที่ 20 ในตำแหน่ง D1 ถัดจากฉบับที่ 19 โดยใช้ pattern และ style เดียวกัน
- ใช้ภาพ PNG ที่ผู้ใช้แนบ 4 ไฟล์ตามลำดับเดิม
- ใช้ภาพหน้า 1 เป็น thumbnail ของการ์ด
- เมื่อคลิก thumbnail ให้เปิด modal แบบ `:target` และแสดงภาพทั้ง 4 หน้าเรียงแนวตั้ง
- รักษา carousel, responsive pattern และเนื้อหาของฉบับก่อนหน้าไว้

### ขั้นตอนการดำเนินงาน

1. **ตรวจต้นแบบแบบ read-only:** ตรวจโครงสร้างการ์ดและ modal ของฉบับที่ 19 รวมถึงตำแหน่ง D1 ซึ่งเป็นช่องว่างลำดับถัดไปในกลุ่มเดียวกัน
2. **ตรวจไฟล์ต้นฉบับ:** ยืนยันว่าภาพหน้า 1–3 เป็น PNG แบบ RGB ขนาด 1536×1024 พิกเซล และหน้า 4 ขนาด 1469×1071 พิกเซล
3. **TDD RED:** เพิ่ม integration test `InformationPageRendersTwentiethIssueFromLocalAssets` โดยกำหนดให้หน้าแสดงหัวข้อ ลิงก์ `#m20-modal`, modal และ local assets ทั้ง 4 ไฟล์ จากนั้นรันแล้วล้มตามคาดเพราะยังไม่มีหัวข้อฉบับที่ 20
4. **คัดลอกรูป:** สร้าง `wwwroot/asset/images/event/m20`, คัดลอกต้นฉบับเป็น `1.png` ถึง `4.png` แล้วตรวจด้วย `cmp` ว่าตรงกับต้นฉบับแบบ byte-for-byte
5. **สร้างการ์ด:** เพิ่มการ์ดฉบับที่ 20 ถัดจากฉบับที่ 19 ใช้ `m20/1.png` เป็น thumbnail และเพิ่มข้อความ “ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 20”
6. **ตรวจและแก้ตำแหน่ง:** การแทรกครั้งแรกจับโครงสร้าง HTML ที่ซ้ำกันและไปอยู่หลังฉบับที่ 15 จึงตรวจ root cause จากบริบทของแต่ละกลุ่ม แล้วเคลื่อนการ์ดมาไว้ในกลุ่มเดียวกับฉบับที่ 19 ที่ตำแหน่ง D1 ตามคำขอ
7. **สร้าง modal:** เพิ่ม CSS `#m20-modal:target` และ modal ที่แสดง `1.png` ถึง `4.png` เรียงแนวตั้ง พร้อมปุ่มปิดและข้อความ alternative text
8. **TDD GREEN:** รัน test เดิมโดยไม่เปลี่ยนเกณฑ์แล้วผ่าน 1/1
9. **Full automated verification:** ชุดทดสอบทั้งหมดผ่าน 38/38 และ `scripts/verify.sh` ผ่านด้วย build 0 warnings และ 0 errors
10. **Browser Desktop 1440×900:** การ์ดฉบับที่ 19–20 อยู่ใน carousel กลุ่มเดียวกัน, ฉบับที่ 20 อยู่ถัดจากฉบับที่ 19 และไม่มี horizontal overflow
11. **Browser modal บน Desktop:** คลิกการ์ดแล้ว URL เปลี่ยนเป็น `#m20-modal`, modal แสดงแบบ `block`, โหลดภาพครบ 4/4 ตามลำดับและขนาดต้นฉบับ และไม่มี horizontal overflow
12. **ตรวจตัวตรวจ Desktop:** Owl Carousel ย้าย `.item` เข้า `.owl-wrapper` หลังโหลด ทำให้ selector แบบ direct child นับได้ 0 จึงตรวจ root cause และปรับตัวตรวจให้ใช้ descendant โดยไม่เปลี่ยนโค้ดหน้าเว็บ
13. **Browser Mobile 390×844:** ใช้ปุ่ม Next ของ carousel ที่ระบุกลุ่มฉบับที่ 19–20 โดยตรงเพื่อเลื่อนไปฉบับที่ 20, การ์ดอยู่ภายใน viewport และหน้าไม่ล้นแนวนอน
14. **Browser modal บน Mobile:** ภาพทั้ง 4 โหลดครบ, ทุกภาพกว้างไม่เกิน viewport 390px, ปุ่มปิดอยู่ภายใน viewport และไม่มี horizontal overflow
15. **ตรวจตัวตรวจ Mobile:** การตรวจจากกรอบ element เพียงอย่างเดียวระบุฉบับที่ 20 ว่ามองเห็นทั้งที่ยังอยู่นอกพื้นที่ overflow จึงเพิ่มการตรวจ `elementFromPoint` และเลือกปุ่ม Next ของ carousel กลุ่มเป้าหมายโดยตรง
16. **คืนค่า Preview:** ยกเลิก viewport override และเปิดหน้า `/Home/information` ค้างไว้ตรงการ์ดฉบับที่ 20 ที่ viewport ปกติ

### การแมปรูปต้นทาง

| ลำดับ | ไฟล์ต้นทาง | ไฟล์ในเว็บไซต์ | ขนาดต้นฉบับ |
| --- | --- | --- | --- |
| 1 | `tdc-1_0.png` | `wwwroot/asset/images/event/m20/1.png` | 1536×1024 |
| 2 | `tdc-2_0.png` | `wwwroot/asset/images/event/m20/2.png` | 1536×1024 |
| 3 | `tdc-3_0.png` | `wwwroot/asset/images/event/m20/3.png` | 1536×1024 |
| 4 | `tdc-4_0.png` | `wwwroot/asset/images/event/m20/4.png` | 1469×1071 |

### ผลการตรวจสอบฉบับที่ 20

- Target test RED: 0 passed, 1 failed เพราะยังไม่มีหัวข้อฉบับที่ 20
- Target test GREEN: 1 passed, 0 failed
- Full test suite: 38 passed, 0 failed, 0 skipped
- `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet ./scripts/verify.sh`
- Restore: dependencies เป็นปัจจุบัน
- Build: 0 warnings, 0 errors
- Browser Desktop: การ์ดอยู่ใน D1 และ modal แสดงถูกต้อง, ภาพโหลด 4/4, ไม่มี horizontal overflow
- Browser Mobile: เข้าถึงฉบับที่ 20 ผ่าน carousel, modal และปุ่มปิดอยู่ใน viewport, ภาพโหลด 4/4, ไม่มี horizontal overflow

### ไฟล์ที่เกี่ยวข้องกับฉบับที่ 20

- `Views/Home/information.cshtml`
- `tests/OTP.Web.Tests/RouteRenderingTests.cs`
- `wwwroot/asset/images/event/m20/1.png`
- `wwwroot/asset/images/event/m20/2.png`
- `wwwroot/asset/images/event/m20/3.png`
- `wwwroot/asset/images/event/m20/4.png`
- `docs/INFORMATION_UPDATE_LOG.md`

### สถานะ Git หลังเพิ่มฉบับที่ 20

- เก็บการเปลี่ยนแปลงไว้ใน branch `migration/aspnet-core-net10`
- รักษาการเปลี่ยนแปลงเดิมที่ยังไม่ commit ใน worktree โดยไม่แก้หรือลบงานที่ไม่เกี่ยวข้อง
- ยังไม่ได้ commit, push, สร้าง pull request หรือ merge
