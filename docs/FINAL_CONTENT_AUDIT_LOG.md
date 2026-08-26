# Final Content and Responsive Navigation Audit

วันที่ตรวจ: 2026-08-27
Branch: `migration/aspnet-core-net10`

## ขอบเขตที่ตรวจ

- หน้า `/Home/eventpic`
  - การประชุมเชิงปฏิบัติการ ครั้งที่ 1 จำนวน 18 รูป
  - การประชุมเชิงปฏิบัติการ ครั้งที่ 2 จำนวน 17 รูป
- หน้า `/Home/information`
  - ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17 จำนวน 5 รูป
  - ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 18 จำนวน 2 รูป
  - ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 19 จำนวน 3 รูป
  - ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 20 จำนวน 4 รูป
- Responsive navigation และเอกสารรูปแบบการพัฒนาใน `design.md`

## จุดที่พบและแก้ก่อนส่ง Git

- Modal ข่าวฉบับที่ 17 มีรูปครบและเปิดได้ แต่รูป 5 รูปยังไม่มีข้อความ `alt` และปุ่มปิดไม่มี `aria-label`
- เพิ่ม regression assertions ใน `RouteRenderingTests` และยืนยันว่า test ล้มด้วยสาเหตุที่คาดไว้ก่อนแก้
- เพิ่ม `alt`, `aria-label` และ title ของการ์ดฉบับที่ 17 ให้ใช้รูปแบบเดียวกับฉบับที่ 18–20
- รัน targeted test หลังแก้และผ่าน 1/1
- รูป Workshop 1/2 จำนวน 35 รูปยังไม่มี `alt`; เพิ่ม regression assertions ให้ล้ม 2/2 ก่อนแก้ แล้วเพิ่มข้อความบรรยายเรียงตามลำดับภาพ
- รัน targeted tests ของ Workshop หลังแก้และผ่าน 2/2

## หลักฐานการตรวจอัตโนมัติ

- `./scripts/verify.sh`
  - Build สำเร็จ
  - Warning: 0
  - Error: 0
  - Tests: ผ่าน 38/38, ไม่ผ่าน 0, ข้าม 0
- `node --check wwwroot/asset/js/common.js`: ผ่าน
- `node --check wwwroot/asset/js/site-navigation.js`: ผ่าน
- `git diff --check`: ผ่าน
- ตรวจไฟล์รูปด้วย `sips`: เปิดอ่าน metadata ได้ 49/49 ไฟล์, เสีย 0 ไฟล์
- ไฟล์รูปทุกไฟล์มีขนาดต่ำกว่าขีดจำกัดไฟล์เดี่ยว 100 MB ของ GitHub

## หลักฐาน Browser จาก source ล่าสุด

ใช้ instance ชั่วคราวที่ `http://127.0.0.1:5196` เพื่อหลีกเลี่ยง cache/process เก่าที่พอร์ต preview เดิม

- Desktop 1440×900
  - Navigation 6 รายการอยู่บรรทัดเดียว
  - Event Picture โหลด Workshop 1 ครบ 18/18 และ Workshop 2 ครบ 17/17
  - Information โหลดรูปข่าวฉบับ 17–20 ครบ 5/5, 2/2, 3/3 และ 4/4
  - ไม่มีรูปโหลดเสียและไม่มี horizontal overflow
- Mobile 390×844
  - Hamburger แสดง, desktop navigation ซ่อน
  - ก่อนเปิด `aria-expanded=false`
  - หลังเปิด drawer กว้าง 250 px, `aria-expanded=true` และ label เปลี่ยนเป็น “ปิดเมนูหลัก”
  - กด Escape แล้ว drawer ปิด, `aria-expanded=false`, label กลับเป็น “เปิดเมนูหลัก” และ focus กลับปุ่มเดิม
  - Modal ข่าวฉบับที่ 20 เปิดได้, มี 4 รูป, รูปกว้างไม่เกิน viewport และไม่มี horizontal overflow

## การตรวจเทียบไฟล์ต้นฉบับ

- Workshop 2 และข่าวฉบับที่ 17–20 รวม 31 ไฟล์ ตรงกับไฟล์ต้นฉบับใน Downloads แบบ byte-for-byte
- โฟลเดอร์ต้นฉบับ Workshop 1 ถูกนำออกจาก Downloads แล้ว จึงเทียบซ้ำในรอบสุดท้ายไม่ได้ อย่างไรก็ตามไฟล์ใน repository ทั้ง 18 รูปเปิดได้จริง, MIME ถูกต้อง, หน้าเว็บโหลดครบ และ integration test เรียกได้ครบทุก URL

## ข้อจำกัดเดิมที่ไม่อยู่ในชุดแก้ครั้งนี้

- ที่ viewport 768 px บาง content/footer legacy อาจล้นแนวนอนประมาณ 6–7 px จาก Bootstrap container และ negative row margins เดิม ตามที่บันทึกใน `RESPONSIVE_MENU_LOG.md`; navigation ที่แก้ไม่มี overflow
- หน้า Contact เดิมเป็น UI อย่างเดียว ยังไม่มี `<form>` action หรือฐานข้อมูลรับข้อความ และไฟล์หน้านี้ไม่ได้ถูกแก้ในชุดงานปัจจุบัน

ข้อจำกัดสองรายการนี้เป็นงานเดิมนอกขอบเขต content/navigation update และไม่บล็อกการ commit/push ชุดนี้
