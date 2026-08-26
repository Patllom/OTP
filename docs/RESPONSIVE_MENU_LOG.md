# Responsive Menu Change Log

บันทึกการวิเคราะห์ อนุมัติ และดำเนินงาน responsive navigation แบบทีละขั้น โดยแต่ละ Step ต้องได้รับอนุมัติจากผู้ใช้ก่อนเริ่ม

## 2026-08-26 — Step 1: Desktop navigation หนึ่งแถว

- **Approval:** ผู้ใช้อนุมัติ Step 1 หลังตรวจ responsive-menu mockup
- **Scope:** ปรับเฉพาะ Desktop layout; ยังไม่เปลี่ยน mobile/hamburger breakpoint
- **Root cause:** ที่ viewport `1440px` เมนูถูกจำกัดไว้ประมาณ `994px` แต่รายการทั้ง 6 ต้องใช้ประมาณ `1,107px`; `inline-block` จึง wrap รายการสุดท้ายลงแถวสอง
- **Test-first RED:** Browser layout assertion ที่ `1440px` พบ 2 แถวที่ `y=10` และ `y=90`; Header สูง `160px`
- **Implementation:** เพิ่ม `site-navigation.css`, โหลดหลัง legacy responsive stylesheet, นำ inline `width: 70%` ออก และใช้ Flex layout ตั้งแต่ `1366px`
- **Boundary RED:** CSS รอบแรกทำให้รายการที่ `1366px` ล้ำเข้าพื้นที่ search; ขอบรายการสุดท้ายอยู่ที่ `x=1293.25` แต่ search เริ่มที่ `x=1280`
- **Boundary fix:** เปลี่ยน breakpoint เป็น `1366px` และลด link padding แนวนอนจาก `12px` เป็น `9px`
- **Browser GREEN 1366px:** navigation 1 แถว, Header `80.515625px`, รายการสุดท้ายจบที่ `x=1275.2421875` ก่อน search ที่ `x=1280`
- **Browser GREEN 1440px:** navigation 1 แถว, Header `80.515625px`, รายการสุดท้ายจบที่ `x=1318.90625` ก่อน search ที่ `x=1354`
- **Design record:** เพิ่ม Desktop CSS contract และ acceptance conditions ใน `design.md`
- **Full project GREEN:** `scripts/verify.sh` ผ่านด้วย workspace-local .NET SDK 10.0.400; build 0 warnings/0 errors และ tests 32/32 ผ่าน
- **Diff hygiene:** `git diff --check` ผ่าน
- **Scope boundary:** Step 2 mobile/hamburger breakpoint ยังไม่เริ่มและต้องรออนุมัติแยกต่างหาก

## 2026-08-26 — Step 2: Hamburger navigation ต่ำกว่า 1366px

- **Approval:** ผู้ใช้อนุมัติ Step 2 หลัง Step 1 ผ่านและได้รับแผนตรวจที่ `1280`, `1024`, `768`, `390` และ `320px`
- **Scope:** เปลี่ยนเฉพาะ Responsive Header/navigation; ไม่แก้ข้อความเมนู, routes หรือ page content
- **Initial RED 1280px:** Desktop navigation ยังมองเห็นและ Hamburger ถูกซ่อน
- **Semantic control:** เปลี่ยน `.mm-toggle` จาก `div` เป็น `<button type="button">` พร้อม `aria-controls="mobile-menu"`, `aria-expanded` และ label ภาษาไทย
- **Responsive layout:** ช่วงต่ำกว่า `1366px` ใช้ CSS Grid `minmax(0, 1fr) 52px 52px`; ซ่อน `.nav-inner`, แสดง Hamburger และ search; Logo mobile ลดเหลือสูง `60px`
- **Bootstrap clearfix RED/GREEN:** `.row::before`/`::after` กลายเป็น Grid items ทำให้ Logo ล้นที่ `1280px`; ปิด pseudo-elements เฉพาะ Responsive Header แล้ว Header กลับเป็นหนึ่งแถวและไม่มี overflow
- **ARIA RED/GREEN:** drawer เปิดได้แต่ `aria-expanded` ค้าง `false`; เพิ่ม `site-navigation.js` เพื่อซิงก์ expanded state และ label หลัง animation
- **Resize RED/GREEN:** legacy plugin ปิด drawer เมื่อ resize จาก `1280px` ไป `1024px` เพราะ `WindowsMaxWidth: 767`; เปลี่ยนเป็น `1366` แล้ว drawer คงเปิดภายใต้ breakpoint
- **Desktop boundary RED/GREEN:** drawer ปิดเมื่อเข้า `1366px` แต่ ARIA ค้าง `true`; resize handler รีเซ็ต `aria-expanded=false` และ label เป็น “เปิดเมนูหลัก”
- **Keyboard RED/GREEN:** `Escape` ไม่ปิด drawer; เพิ่ม handler ให้ปิดผ่าน toggle เดิมและคืน focus หลัง animation
- **Small-screen height RED/GREEN:** legacy `nav { height: 3px; }` และ Bootstrap form/collapse borders ทำให้ Header สูง `83px` ที่ `390px`; override nav เป็น `52px` และลบ borders เฉพาะ Responsive Header ทำให้สูง `80px`
- **Search visibility RED/GREEN:** legacy media query `992–1199px` ซ่อน `.navbar-form` ที่ `1024px`; override form ให้แสดงและ search กว้าง `52px`
- **Asset delivery:** เปิด `asp-append-version` ให้ `site-navigation.css`, `common.js` และ `site-navigation.js` เพื่อเปลี่ยน URL ตาม content hash
- **Header matrix GREEN:** `1280`, `1024`, `768`, `390`, `320px` แสดง Hamburger/search และซ่อน Desktop nav; `1366`, `1440px` แสดง Desktop nav หนึ่งแถว; Header สูง `80–80.515625px` และไม่มี internal overflow
- **Mobile drawer GREEN:** ที่ `320px` drawer กว้าง `250px`, อยู่ใน viewport, `aria-expanded=true` และมี top-level links 6 รายการ
- **Out-of-scope finding:** page content/footer เดิมมี horizontal overflow `6–7px` ที่ `768px` จาก fixed Bootstrap container, removed container padding และ negative row margins; ทดลอง body padding แล้วไม่แก้ต้นเหตุ จึงนำกฎทดลองออกและยังไม่เปลี่ยน content/footer
- **Design record:** อัปเดต responsive CSS/interaction contract และ acceptance matrix ใน `design.md`
- **JavaScript syntax:** `node --check` ผ่านทั้ง `site-navigation.js` และ `common.js`
- **Full project GREEN:** `scripts/verify.sh` ผ่านด้วย workspace-local .NET SDK 10.0.400; build 0 warnings/0 errors และ tests 32/32 ผ่าน
- **Scope boundary:** ไม่แก้ legacy content/footer overflow, ข้อความสะกด, routes, page content, commit, push หรือ merge
