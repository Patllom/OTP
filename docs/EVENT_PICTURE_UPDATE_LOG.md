# Event Picture Update Log

บันทึกการแก้ไขหน้า `/Home/eventpic` สำหรับการประชุมเชิงปฏิบัติการ ครั้งที่ 1 และ 2 โดยทำตามขั้นตอนอนุมัติของผู้ใช้และใช้รูปแบบ gallery เดิมของหน้าเป็นต้นแบบ

## Gallery pattern ที่ใช้ร่วมกัน

- Desktop ตั้งแต่ Bootstrap `md` ใช้ `col-lg-2 col-md-2` แสดงสูงสุด 6 รูปต่อแถว
- หน้าจอเล็กใช้ `col-sm-12 col-xs-12` แสดง 1 รูปต่อแถว
- แต่ละช่องมี `padding: 3px`
- Thumbnail ใช้ `width: 100%`, `height: 150px`, `object-fit: cover` และ `object-position: center`
- ใช้ class `img-rounded`
- `<a href>` และ `<img src>` ชี้ไฟล์เดียวกัน เพื่อให้กดเปิดรูปต้นฉบับได้
- รูปถูกเก็บเป็น local static assets ใต้ `wwwroot/asset/images/event`

## 2026-08-26 — Workshop ครั้งที่ 1

### ขั้นตอนและการอนุมัติ

1. **ตรวจต้นแบบ:** ผู้ใช้อนุมัติให้ตรวจโครงสร้าง gallery เดิมแบบ read-only พบรูปแบบ 6 คอลัมน์, thumbnail สูง 150px และลิงก์เปิดไฟล์จริง
2. **เปลี่ยนหัวข้อ:** เปลี่ยน “การประชุมกลุ่มย่อย ครั้งที่ 1” เป็น “การประชุมเชิงปฏิบัติการ ครั้งที่ 1” หลังได้รับอนุมัติแยกขั้นตอน
3. **คัดลอกรูป:** คัดลอกรูปที่ผู้ใช้เลือก 18 ไฟล์ไป `wwwroot/asset/images/event/workshop1` โดยคงชื่อและลำดับเดิม ตรวจด้วย `cmp` แล้วตรงกับต้นฉบับทุกไฟล์
4. **สร้าง gallery:** นำ placeholder ออกและ render รูปทั้ง 18 ไฟล์ด้วย loop ใน `Views/Home/eventpic.cshtml`
5. **TDD RED:** เพิ่ม integration test `EventPicturePageRendersEveryWorkshopPhotoFromLocalAssets`; test ล้มก่อนแก้เพราะ placeholder ยังอยู่และยังไม่มี image paths
6. **TDD GREEN:** หลังเพิ่ม gallery test เฉพาะผ่าน 1/1 และชุดทดสอบทั้งหมดผ่าน 33/33
7. **Build:** `dotnet build OTT.sln --no-restore` ผ่านด้วย 0 warnings และ 0 errors
8. **Browser 1440px:** หัวข้อถูกต้อง, รูป 18 ไฟล์แสดงครบเป็น 6 คอลัมน์ × 3 แถว, thumbnail สูง 150px และ `href` ตรงกับ `src`
9. **Browser 390px:** รูปแสดง 1 คอลัมน์ × 18 แถว, gallery อยู่ใน viewport และไม่มี horizontal overflow
10. **รูปต้นฉบับ:** เปิดไฟล์แรกผ่าน Browser ได้เป็น `image/jpeg` ขนาด 1280×968

## 2026-08-26 — Workshop ครั้งที่ 2

### ขั้นตอนและการอนุมัติ

1. **ตรวจตำแหน่ง:** ผู้ใช้อนุมัติให้ตรวจ section เดิมแบบ read-only พบหัวข้อ “การจัดฝึกอบรม ครั้งที่ 1” และ placeholder ที่ `Views/Home/eventpic.cshtml`
2. **อนุมัติทำต่อเนื่อง:** ผู้ใช้อนุญาตให้ดำเนินขั้นตอนที่เหลือจนจบ เพราะใช้ pattern เดียวกับ Workshop ครั้งที่ 1
3. **TDD RED:** เพิ่ม integration test `EventPicturePageRendersEverySecondWorkshopPhotoFromLocalAssets`; test ล้มตามคาดเพราะยังไม่มีหัวข้อ “การประชุมเชิงปฏิบัติการ ครั้งที่ 2”
4. **คัดลอกรูป:** คัดลอกรูปที่ผู้ใช้เลือก 17 ไฟล์ไป `wwwroot/asset/images/event/workshop2` โดยคงชื่อและลำดับที่แนบมา ตรวจด้วย `cmp` แล้วตรงกับต้นฉบับทุกไฟล์
5. **เปลี่ยนหัวข้อและสร้าง gallery:** เปลี่ยนหัวข้อเป็น “การประชุมเชิงปฏิบัติการ ครั้งที่ 2”, นำ placeholder ออก และ render รูป 17 ไฟล์ด้วย pattern เดียวกับ Workshop ครั้งที่ 1
6. **HTML encoding investigation:** GREEN รอบแรกพบว่า Razor encode อักษรไทยในชื่อไฟล์เป็น HTML entity เช่น `พ` เป็น `&#xE1E;`; ปรับ test ให้ใช้ `WebUtility.HtmlDecode` ก่อนตรวจ rendered paths โดยไม่เปลี่ยน production URLs
7. **TDD GREEN:** test เฉพาะ Workshop ครั้งที่ 2 ผ่าน 1/1 และชุดทดสอบทั้งหมดผ่าน 34/34
8. **Build:** `dotnet build OTT.sln --no-restore` ผ่านด้วย 0 warnings และ 0 errors
9. **Browser 1440px:** หัวข้อปรากฏ 1 จุด, รูป Workshop ครั้งที่ 2 ครบ 17 ไฟล์เป็นแถว 6/6/5, Workshop ครั้งที่ 1 ยังคงครบ 18 ไฟล์ และ `href` ตรงกับ `src`
10. **Browser 390px:** รูปแสดง 1 คอลัมน์ × 17 แถว, thumbnail สูง 150px, gallery อยู่ใน viewport และไม่มี horizontal overflow
11. **รูปต้นฉบับ:** เปิด URL ที่มีช่องว่างและภาษาไทยได้เป็น `image/jpeg` ขนาด 1280×853

### ลำดับรูป Workshop ครั้งที่ 2

คงลำดับหมายเลขตามรายการที่ผู้ใช้แนบ: `5, 8, 10, 16, 19, 20, 22, 25, 26, 35, 29, 40, 49, 52, 53, 56, 57`

## Automated verification

- `OTP_DOTNET=/Users/theerapat_k/Codex/OTP/.dotnet/dotnet ./scripts/verify.sh`
- Restore: dependencies เป็นปัจจุบัน
- Build: 0 warnings, 0 errors
- Tests: 34 passed, 0 failed, 0 skipped
- `git diff --check`: ผ่าน

## ไฟล์ที่เกี่ยวข้อง

- `Views/Home/eventpic.cshtml`
- `tests/OTP.Web.Tests/RouteRenderingTests.cs`
- `wwwroot/asset/images/event/workshop1/` — 18 ไฟล์
- `wwwroot/asset/images/event/workshop2/` — 17 ไฟล์
- `docs/EVENT_PICTURE_UPDATE_LOG.md`

## สถานะ Git

- ยังไม่ได้ commit, push, สร้าง pull request หรือ merge จากการแก้ไขชุดนี้
