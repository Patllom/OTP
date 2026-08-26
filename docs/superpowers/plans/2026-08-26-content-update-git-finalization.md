# Content Update and Git Finalization Plan

**Goal:** ตรวจสอบงานหน้า Event Picture, Information และ responsive navigation ที่แก้ไขไว้ทั้งหมด บันทึกข้อจำกัดที่พบ และส่งงานที่ผ่านการตรวจขึ้น Git อย่างครบถ้วน

**Scope:** งานตรวจและส่งขึ้น Git เท่านั้น ไม่ขยายขอบเขตไปแก้ระบบแบบฟอร์มติดต่อหรือปัญหา legacy ที่ไม่เกี่ยวกับชุดการเปลี่ยนแปลงนี้

## Task 1: Audit repository state and assets

- ตรวจ branch, upstream, remote และความสัมพันธ์กับ `origin/main`
- ตรวจรายการไฟล์ modified/untracked และ `git diff --check`
- ตรวจจำนวน ชนิด ขนาด และความสามารถในการเปิดไฟล์รูปของ workshop 1–2 และข่าวฉบับที่ 17–20
- ตรวจว่า view อ้างอิงไฟล์รูปครบตามจำนวน

## Task 2: Verify implementation

- รัน `./scripts/verify.sh` ด้วย .NET SDK ของโปรเจกต์
- ตรวจ syntax ของ `wwwroot/asset/js/common.js` และ `wwwroot/asset/js/site-navigation.js`
- ตรวจหน้า `/Home/eventpic` และ `/Home/information` ทั้ง desktop/mobile
- ตรวจเมนู responsive, จำนวนรูป และ modal ข่าวฉบับล่าสุดจากหน้า preview

## Task 3: Record final audit

- สร้างบันทึกผลตรวจสุดท้าย รวมหลักฐานการทดสอบและข้อจำกัดที่ไม่บล็อกการส่งงาน
- ตรวจหา TODO, placeholder และ path ที่อ้างถึงไฟล์ในเครื่องผู้พัฒนาโดยไม่ตั้งใจ

## Task 4: Final verification and Git upload

- รัน verification ซ้ำหลังการแก้เอกสารหรือโค้ดรอบสุดท้าย
- ตรวจ final diff และ staged diff
- commit เฉพาะไฟล์ในขอบเขตงาน
- push branch `migration/aspnet-core-net10` ไปยัง `origin`
- ยืนยันว่า remote branch ชี้ commit เดียวกับ local และรายงานสถานะ PR โดยไม่ merge จนกว่าจะได้รับอนุมัติเฉพาะขั้นตอนนั้น
