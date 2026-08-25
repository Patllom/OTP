# รายงานตรวจสอบ Landing Page

วันที่ตรวจสอบ: 25 สิงหาคม 2026  
ขอบเขต: หน้า `Views/Home/Index.cshtml` และ layout หลัก `Views/Shared/_LayoutMain.cshtml` รวมถึง asset และ dependency ที่เกี่ยวข้อง  
วิธีตรวจ: Static code review และตรวจขนาดไฟล์ใน repository (ยังไม่ได้ทดสอบบน production URL หรือวัด Lighthouse/Core Web Vitals จริง)

## สรุปสำหรับผู้บริหาร

หน้าเว็บสื่อชื่อโครงการและมีข้อมูลพื้นฐานครบในระดับหนึ่ง แต่ยังไม่พร้อมทำหน้าที่เป็น landing page ที่น่าเชื่อถือและนำผู้ใช้ไปสู่เป้าหมายอย่างมีประสิทธิภาพ จุดที่ต้องแก้ก่อนเผยแพร่หรือประชาสัมพันธ์คือ **ฟอร์มติดต่อส่งไม่ได้**, **SEO metadata ไม่ตรงกับโครงการ**, **ภาพหน้าแรกมีขนาดใหญ่มาก**, และ **พึ่งพารูป/บริการจากเว็บภายนอกโดยไม่มี fallback**

ภาพรวมคะแนนจากการตรวจโค้ด:

| ด้าน | คะแนนโดยประมาณ | หมายเหตุ |
|---|---:|---|
| ความชัดเจนของเนื้อหา/Conversion | 5/10 | บอกข้อมูลโครงการได้ แต่ไม่มี value proposition และ CTA หลัก |
| UX/UI และ Mobile | 5/10 | ใช้ Bootstrap grid แต่มี fixed size/inline style จำนวนมาก |
| Accessibility | 3/10 | ไม่มี `h1`, alt text ไม่เหมาะสม, slider และ form ยังไม่พร้อมสำหรับผู้ใช้ทุกกลุ่ม |
| SEO/Social sharing | 2/10 | description/keywords ผิดบริบท, ไม่มี canonical/OG/Twitter metadata |
| Performance | 3/10 | hero images ใหญ่รวมประมาณ 11.7 MB และโหลด script/font หลายชุด |
| ความน่าเชื่อถือ/ความเสถียร | 4/10 | hotlink รูปภายนอก, social link ปลอม, มีโค้ดจากธีมเดิมหลงเหลือ |

## ประเด็นเร่งด่วน

### P0 — ฟอร์ม “ติดต่อโครงการ” ส่งข้อมูลไม่ได้

- ตำแหน่ง: `Views/Home/Index.cshtml:336-399`
- ช่องกรอกและปุ่ม submit ไม่ได้อยู่ใน `<form>` และไม่มี `action`, `method`, controller action หรือ JavaScript handler สำหรับส่งข้อมูล
- ช่องชื่อและอีเมลใช้ `value="ชื่อ"` / `value="อีเมล์"` แทน `placeholder` ทำให้ค่าตัวอย่างอาจถูกส่งเป็นข้อมูลจริง
- `type="text"` ถูกใช้กับอีเมลและโทรศัพท์ แทน `email` และ `tel`
- ไม่มี server-side validation, anti-forgery token, consent/privacy notice, success/error state หรือมาตรการป้องกัน spam

**ผลกระทบ:** ผู้ใช้กด “ส่งข้อความ” แล้วข้อมูลไม่ถูกส่ง ทำให้เสีย lead และลดความน่าเชื่อถือทันที

**ข้อเสนอ:** สร้าง ViewModel และ `[HttpPost]` action, ใส่ `@using (Html.BeginForm(...))`, `@Html.AntiForgeryToken()`, validation ทั้ง client/server, เปลี่ยนชนิด input ให้ถูกต้อง และแสดงผลสำเร็จ/ผิดพลาดที่ผู้ใช้รับรู้ได้

### P0 — SEO metadata เป็นข้อความจากธีมร้านค้า

- ตำแหน่ง: `Views/Shared/_LayoutMain.cshtml:2, 8-10`
- `<html lang="en">` ไม่ตรงกับเนื้อหาภาษาไทย ควรเป็น `lang="th"`
- description เป็น `Default Description`
- keywords เป็น `fashion, store, E-commerce` ซึ่งไม่เกี่ยวกับโครงการ
- title ถูก hard-code ทำให้ทุกหน้ามี title เดียว และ `ViewBag.Title` ในหน้า Index ไม่ถูกนำมาใช้

**ผลกระทบ:** Search engine เข้าใจเนื้อหาผิด, snippet คุณภาพต่ำ และทุกหน้ามีโอกาสแย่ง ranking กันเอง

**ข้อเสนอ:** ใช้ title/description รายหน้า, เพิ่ม canonical URL, Open Graph และ Twitter Card พร้อมรูป preview ของโครงการ

### P1 — น้ำหนักหน้าและภาพ Hero สูงมาก

- ตำแหน่ง: `Views/Home/Index.cshtml:53-61, 153-160`
- ภาพ slider ที่โหลดทันทีมีขนาดไฟล์ประมาณ:
  - `cover1.png` 1.0 MB
  - `cover2.png` 3.2 MB
  - `info1.png` 7.5 MB
- รวมเฉพาะสามภาพประมาณ **11.7 MB**; `info1.png` มีความละเอียด 5280×2970 และ `cover2.png` 5040×2835 ซึ่งเกินความจำเป็นสำหรับเว็บทั่วไป
- `content1.png` เพิ่มอีกประมาณ 1.4 MB (4800×2700)
- ไม่มี `srcset`, `sizes`, WebP/AVIF, explicit width/height หรือ lazy loading สำหรับภาพใต้ fold

**ผลกระทบ:** Largest Contentful Paint ช้า, ใช้ data สูง และประสบการณ์บนมือถือ/เครือข่ายช้าไม่ดี

**ข้อเสนอ:** ส่งออกภาพเป็น AVIF/WebP หลายขนาด, จำกัด hero desktop ราว 1600–1920 px, preload เฉพาะภาพแรก, lazy-load ภาพที่เหลือ และกำหนด `width`/`height` เพื่อลด layout shift

### P1 — พึ่งพารูปจากเว็บไซต์บุคคลที่สาม

- ตำแหน่ง: `Views/Home/Index.cshtml:93, 114, 135`
- รูปวัตถุประสงค์ทั้งสาม hotlink จาก `landbridgethai.com`, `wu.ac.th` และ `abss.co.th`
- เจ้าของเว็บภายนอกสามารถย้าย/ลบ/บล็อก hotlink หรือเปลี่ยนเนื้อหารูปได้ และควรตรวจสิทธิ์การใช้งาน

**ผลกระทบ:** รูปแตกโดยควบคุมไม่ได้, privacy/performance แปรผัน และมีความเสี่ยงเรื่องลิขสิทธิ์

**ข้อเสนอ:** ใช้ภาพที่องค์กรมีสิทธิ์และเก็บไว้ใน asset ของโครงการ พร้อมบีบอัดและกำหนด alt text ที่อธิบายเนื้อหา

### P1 — Mixed content และ external dependencies มากเกินจำเป็น

- ตำแหน่ง: `Views/Shared/_LayoutMain.cshtml:22-31`
- Open Sans ถูกโหลดผ่าน `http://fonts.googleapis.com` ซึ่งอาจถูกบล็อกเมื่อเว็บทำงานผ่าน HTTPS
- โหลด Adobe Typekit และ Google Fonts หลาย family/หลาย weight ทั้งที่หน้าใช้หลัก ๆ เพียง Kanit และ Noto Sans Thai
- หน้าโหลดปลั๊กอิน slider/carousel/countdown จากธีมหลายชุด (`_LayoutMain.cshtml:300-304`) แม้บางส่วนไม่จำเป็นกับหน้า

**ผลกระทบ:** render blocking, request จำนวนมาก, privacy exposure และความเสี่ยงจาก third-party outage

**ข้อเสนอ:** เปลี่ยนทุก URL เป็น HTTPS, ตัด font/weight/script ที่ไม่ได้ใช้, พิจารณา self-host font และใช้ `defer` กับ script ที่ไม่จำเป็นต่อ first paint

## ประเด็นสำคัญรองลงมา

### P2 — ไม่มีหัวข้อหลักและลำดับ heading ไม่เป็น semantic

- หน้า Index เริ่มด้วย slider ที่เป็นรูปอย่างเดียว แล้วใช้ `h2` และ `h4`; ไม่มี `h1`
- โลโก้ใช้ alt ว่า `logo` ซึ่งไม่บอกชื่อหน่วยงาน (`_LayoutMain.cshtml:86`)
- ภาพ slider และ `content1.png` ไม่มี alt text; ภาพวัตถุประสงค์ใช้ alt `No Picture!` ซึ่งไม่ตรงกับภาพ

**ข้อเสนอ:** เพิ่ม hero ที่มี `h1` ชื่อ/คุณค่าหลักของโครงการ, subtitle และ CTA; ใช้ heading ตามลำดับ; เขียน alt ตามความหมาย หรือ `alt=""` หากเป็นภาพตกแต่ง

### P2 — Landing page ไม่มี CTA หรือเส้นทางหลักที่ชัด

- Hero เป็นภาพสไลด์อย่างเดียว ไม่มีข้อความ HTML และปุ่มนำทาง
- ผู้ใช้ต้องเลื่อนยาวก่อนถึงส่วนติดต่อ และยังไม่มี CTA เช่น “ดูผลการดำเนินงาน”, “เข้าถึงข้อมูล”, “ดาวน์โหลดรายงาน” หรือ “ติดต่อโครงการ”
- เนื้อหา “ความเป็นมา” เป็นย่อหน้ายาว จึงสแกนบนมือถือยาก

**ข้อเสนอ:** กำหนด primary CTA เพียงหนึ่งรายการและ secondary CTA หนึ่งรายการใน hero; ย่อเนื้อหาเป็น bullet/key outcomes และเชื่อมไปหน้ารายละเอียด

### P2 — Slider อาจรบกวนการใช้งานและ accessibility

- slider หมุนอัตโนมัติทุก 5 วินาทีและวนไม่สิ้นสุด (`_LayoutMain.cshtml:308-375`)
- ไม่พบปุ่ม pause ที่ชัดเจน และ `keyboardNavigation` ถูกปิด
- `fullScreen: 'on'` ร่วมกับภาพขนาดใหญ่ทำให้เนื้อหาหลักอาจอยู่ใต้ fold มากเกินไป

**ข้อเสนอ:** ใช้ static hero จะชัดและเร็วกว่า; หากต้องคง slider ควรมี pause/play, keyboard support, accessible label และเคารพ `prefers-reduced-motion`

### P2 — โครงสร้าง HTML ของส่วนติดต่อไม่ถูกต้อง

- มี `div` และ `p` เป็นลูกโดยตรงของ `ul` (`Index.cshtml:343-379`) ซึ่งไม่ใช่โครงสร้าง list ที่ถูกต้อง
- ใช้ `<center>` ซึ่งเลิกใช้แล้ว (`Index.cshtml:387`)
- iframe แผนที่กำหนด `width="600" height="450"` โดยไม่มี `title` และ responsive wrapper

**ข้อเสนอ:** ใช้ semantic form markup, CSS สำหรับจัดกึ่งกลาง, เพิ่ม `title` ให้ iframe และทำอัตราส่วน responsive

### P2 — ลิงก์ที่ดูเหมือนกดได้แต่ไม่ทำงาน

- รูปวัตถุประสงค์ครอบด้วย `href="#"` (`Index.cshtml:93, 114, 135`)
- social icons ใน footer ทุกอันชี้ `#` (`_LayoutMain.cshtml:228-234`)
- search form ไม่มีช่องค้นหาบน desktop เหลือเพียงปุ่ม (`_LayoutMain.cshtml:168-176`); mobile search ไม่มี action

**ผลกระทบ:** ผู้ใช้คาดหวังการทำงานแต่กลับถูกพาไปบนสุดของหน้า หรือไม่มีผลลัพธ์

**ข้อเสนอ:** ลบองค์ประกอบที่ยังไม่ทำงาน หรือใส่ URL/behavior จริง; ถ้ารูปไม่ใช่ลิงก์ให้ใช้ `<figure>`/`<div>` แทน `<a>`

### P2 — Visitor counter ส่ง request ภายนอกทุกหน้า

- หน้า Index เรียก `updatevisitor` ด้วย GET (`Index.cshtml:10-21`)
- layout ทุกหน้าเรียก `getvisitor` และ log response (`_LayoutMain.cshtml:384-403`)
- หาก endpoint/CORS ล้มเหลว ผู้ใช้จะเห็นตัวเลข fallback `4` ทุกค่าใน footer

**ข้อเสนอ:** ยืนยันฐานกฎหมาย/นโยบาย privacy, เปลี่ยน mutation ให้เป็น POST หรือเก็บสถิติฝั่ง server, ใส่ timeout/fallback ที่ไม่แสดงตัวเลขปลอม และถอด console log ใน production

### P2 — เทคโนโลยีและ package เก่า

- โปรเจกต์ใช้ .NET Framework 4.7.2, ASP.NET MVC 5.2.7, jQuery 3.4.1, Modernizr 2.8.3 และ Newtonsoft.Json 12.0.2
- ยังไม่ได้ยืนยันช่องโหว่ด้วย vulnerability scanner ในการตรวจครั้งนี้ แต่ควรวางแผนอัปเดตและทดสอบ compatibility

**ข้อเสนอ:** ทำ software composition analysis, อัปเดต package ที่ยังรองรับได้ และวาง roadmap ไปยัง .NET รุ่นที่ยังอยู่ใน lifecycle หากระบบต้องดูแลระยะยาว

## งานเก็บรายละเอียด (P3)

- แก้คำผิดในเมนูทั้ง desktop/mobile:
  - `การฝึกอบรมสัมนา` → `การฝึกอบรมสัมมนา` (`_LayoutMain.cshtml:109, 283`)
  - `ติตดต่อโครงการ` → `ติดต่อโครงการ` (`_LayoutMain.cshtml:112, 286`)
  - `อีเมลล์` → `อีเมล` (`_LayoutMain.cshtml:222`)
- Copyright hard-code เป็น 2025 (`_LayoutMain.cshtml:220`); ควรใช้ปีปัจจุบันอัตโนมัติหรือช่วงปีที่เหมาะสม
- CSS กระจายอยู่ใน inline style และ `<style>` หลายจุด ทำให้ดูแลและสร้าง responsive design ยาก
- `.logo` ถูกประกาศ background สองครั้งด้วยค่าสีต่างกัน (`_LayoutMain.cshtml:34-36, 50-52`)
- มี mini-cart/product text, countdown ปี 2017 และโค้ดจากธีมอีคอมเมิร์ซหลงเหลือ แม้บางส่วนถูกซ่อน (`_LayoutMain.cshtml:125-163, 419 เป็นต้น`)
- มี Razor comment ขนาดใหญ่ใน Index; ควรลบจาก production source เมื่อยืนยันว่าไม่ใช้แล้ว
- `alt`, `title`, label และข้อความปุ่มบางส่วนยังเป็นอังกฤษจาก template ทำให้ประสบการณ์ไม่สม่ำเสมอ

## โครงสร้าง Landing Page ที่แนะนำ

1. **Header แบบกระชับ** — โลโก้, เมนูหลัก 4–5 รายการ, ปุ่ม “ติดต่อโครงการ”
2. **Hero แบบ static** — `h1`, คำอธิบาย 1–2 บรรทัด, primary CTA และ secondary CTA
3. **ตัวเลข/ผลลัพธ์สำคัญ** — เช่น ชุดข้อมูล, หน่วยงานที่เชื่อมโยง, ผลงานที่ส่งมอบ (ใช้ตัวเลขจริงเท่านั้น)
4. **วัตถุประสงค์ 3 ข้อ** — icon/ภาพ local ที่มีสิทธิ์, ข้อความสั้นและความสูง card เท่ากัน
5. **ขั้นตอนการดำเนินงาน** — ทำเป็น HTML timeline แทนการฝังข้อความทั้งหมดไว้ในภาพ เพื่อ SEO และ accessibility
6. **ความเป็นมาแบบย่อ** — 3–5 bullet พร้อมลิงก์ “อ่านรายละเอียด”
7. **ผลงาน/ข่าวล่าสุด** — แสดงเฉพาะข้อมูลจริง 3 รายการ พร้อมวันที่และลิงก์
8. **Contact CTA** — ช่องทางติดต่อที่กดโทร/ส่งอีเมลได้; แยก form ไปหน้าติดต่อหากยังไม่มี backend ที่ปลอดภัย
9. **Footer** — เจ้าของโครงการ, นโยบายความเป็นส่วนตัว, accessibility statement และ social link ที่ใช้งานจริงเท่านั้น

## ลำดับการดำเนินงานที่แนะนำ

### ระยะที่ 1: แก้ก่อนเผยแพร่ (ประมาณ 1–2 วัน)

- ทำให้ฟอร์มส่งได้จริง หรือซ่อนฟอร์มและใช้ลิงก์อีเมล/โทรศัพท์ชั่วคราว
- แก้ `lang`, title, description, keywords และคำผิด
- นำ dead links/social icons/search ที่ไม่ทำงานออก
- ย้ายภาพ external มาเป็น asset ที่ได้รับอนุญาต
- แปลงและ resize ภาพ hero/content เป็น WebP/AVIF

### ระยะที่ 2: ปรับ conversion และ accessibility (ประมาณ 2–4 วัน)

- สร้าง static hero พร้อม `h1` และ CTA
- ปรับ heading, alt text, focus state, keyboard navigation และ reduced motion
- ทำ form และ map ให้ responsive พร้อม validation และ feedback
- ย้าย inline styles ไป stylesheet และกำหนด design tokens เบื้องต้น

### ระยะที่ 3: คุณภาพ production (ประมาณ 2–5 วัน)

- เพิ่ม OG/Twitter/canonical, sitemap และ structured data ที่เหมาะสม
- ตัด dependency/template code ที่ไม่ใช้, bundle/minify อย่างเหมาะสม และตั้ง cache headers
- รัน Lighthouse บน mobile/desktop และทดสอบด้วย screen reader/keyboard
- สแกน dependency/security headers และทดสอบ form abuse/spam protection

## Acceptance criteria หลังปรับปรุง

- ฟอร์มส่งสำเร็จจริง มี validation, anti-forgery, feedback และไม่บันทึกข้อมูลตัวอย่าง
- หน้าแต่ละหน้ามี title/description เฉพาะ, หน้าแรกมี `h1` เพียงหนึ่งจุด และ `lang="th"`
- ไม่มี HTTP resource, broken link, `href="#"` ที่ไม่มี behavior หรือรูป hotlink ที่ไม่ได้รับอนุญาต
- Hero แรกแสดงได้โดยไม่ต้องรอ slider และภาพเหนือ fold รวมมีขนาดเหมาะสม (เป้าหมายเบื้องต้นไม่เกินประมาณ 500 KB)
- ใช้งานเมนู, CTA และ form ด้วย keyboard ได้ครบ; animation เคารพ reduced motion
- แผนที่และทุก section ไม่ล้นจอบน viewport 320 px
- Lighthouse production เป้าหมาย: Performance ≥ 85, Accessibility ≥ 90, Best Practices ≥ 90, SEO ≥ 90 (วัดทั้ง mobile และ desktop)

## หมายเหตุการตรวจ

Repository นี้ไม่มี Git metadata ใน workspace จึงไม่สามารถรายงาน diff/status ผ่าน Git ได้ และการตรวจครั้งนี้ไม่ได้เปิดเว็บจริงบน IIS/production ดังนั้นค่าความเร็ว, CORS, HTTP status, responsive rendering และพฤติกรรมการ submit ต้องยืนยันเพิ่มเติมใน runtime environment
