# Website Design Guide

เอกสารนี้เก็บรูปแบบภาพและ CSS ของเว็บไซต์โครงการ เพื่อให้หน้าใหม่มีภาษาออกแบบเดียวกันและลดการเพิ่ม inline style หรือกฎ CSS ที่ซ้ำกัน

## 1. สถานะและแหล่งอ้างอิง

- รูปแบบหลักมาจาก `LANDING_PAGE_PREVIEW.html`
- สีและตัวอักษรตรวจเทียบกับ `Views/Home/Index.cshtml` และ `Views/Shared/_LayoutMain.cshtml`
- รายงานปัญหาและข้อควรระวังอ้างอิงจาก `LANDING_PAGE_REVIEW.md`
- เอกสารนี้เป็น design baseline สำหรับงานใหม่ ส่วนไฟล์ธีมเก่า เช่น `wwwroot/asset/stylesheet/style.css` และ `responsive.css` ใช้เพื่อ compatibility เท่านั้น
- เมื่อเริ่มนำรูปแบบนี้ไปใช้จริง ควรย้าย canonical CSS ด้านล่างไปไว้ใน stylesheet แยก เช่น `wwwroot/css/site-design.css` และโหลดหลัง Bootstrap/legacy styles

## 2. Design direction

บุคลิกของเว็บไซต์คือ “หน่วยงานภาครัฐที่ทันสมัย น่าเชื่อถือ และเข้าถึงง่าย” โดยใช้พื้นขาวและน้ำเงินเป็นแกน สีเหลืองใช้เฉพาะ action สำคัญ และสีเขียวเทาอ่อนใช้แบ่ง section

หลักสำคัญ:

1. เนื้อหาภาษาไทยอ่านง่าย มีพื้นที่ว่างเพียงพอ
2. หนึ่งหน้ามี primary CTA ที่เด่นเพียงหนึ่งแบบ
3. ใช้ static hero แทน auto-playing slider
4. ใช้ local assets ที่องค์กรมีสิทธิ์ หลีกเลี่ยง hotlink รูปจากเว็บไซต์ภายนอก
5. Responsive และ keyboard accessibility เป็นข้อกำหนด ไม่ใช่งานเก็บรายละเอียด

## 3. Design tokens

### สี

| Token | ค่า | การใช้งาน |
|---|---|---|
| `--color-text` | `#17315b` | ข้อความหลัก |
| `--color-primary` | `#1176bc` | ลิงก์และ navigation |
| `--color-heading` | `#042da1` | หัวข้อและชื่อ card |
| `--color-navy` | `#0b2c56` | Footer และพื้นเข้ม |
| `--color-hero-text` | `#ffffff` | ข้อความบน hero |
| `--color-accent` | `#f5b41b` | Primary CTA |
| `--color-accent-text` | `#092b58` | ข้อความบนปุ่มสีเหลือง |
| `--color-surface` | `#ffffff` | พื้น card/header |
| `--color-surface-soft` | `#f5f7fa` | Section ติดต่อหรือพื้นรอง |
| `--color-section-alt` | `#d3dbcb` | Section สลับสี |
| `--color-process` | `#f4f4f5` | พื้นภาพกระบวนการ |
| `--color-border` | `#ccd4df` | เส้นขอบ input |

ข้อกำหนดการใช้สี:

- สีเหลืองสงวนไว้สำหรับ primary CTA และสถานะที่ต้องการดึงสายตา
- ห้ามใช้สีเพียงอย่างเดียวเพื่อสื่อ error, success หรือสถานะ ต้องมีข้อความหรือ icon ประกอบ
- ข้อความปกติควรใช้ `--color-text`; หลีกเลี่ยงการใช้ `darkblue` หรือสีใหม่แบบเฉพาะจุด

### ตัวอักษร

- Heading: `Kanit`, fallback `sans-serif`, น้ำหนัก `500–600`
- Body/UI: `Noto Sans Thai`, fallback `Tahoma, sans-serif`, น้ำหนัก `400–600`
- ข้อความยาว: line-height อย่างน้อย `1.7`
- หลีกเลี่ยงการโหลด Merriweather, Oswald, Open Sans และ Rubik หากหน้าไม่ได้ใช้จริง

| บทบาท | Desktop | Mobile | น้ำหนัก/ระยะบรรทัด |
|---|---:|---:|---|
| Hero `h1` | `52px` | `38px` | `600 / 1.18` |
| Section `h2` | `40px` | `34px` | `600 / 1.3` |
| Card `h3` | `19px` | `19px` | `500 / 1.55` |
| Hero lead | `20px` | `18px` | `400 / 1.7` |
| Body | `16–19px` | `16–18px` | `400 / 1.7–1.8` |

### Layout, spacing และ shape

- Content width สูงสุด: `1170px`
- Page gutter: `20px`
- Section spacing: `60px` ด้านบนและล่าง
- Card grid gap: `22px`
- Two-column gap: `32px`
- Small radius: `5px`; button radius: `6px`; card radius: `10px`
- Responsive breakpoint หลัก: `900px`
- Content ที่เป็นข้อความยาวไม่ควรกว้างเกิน `780px`

## 4. Component patterns

### Header และ navigation

- Header ใช้พื้นขาวและเงาบาง ความสูงกำหนดจาก content ไม่ fix ด้วย pixel
- Logo baseline คือ `145 × 62px` และใช้ `object-fit: contain`
- Desktop navigation ชิดขวา ใช้สีน้ำเงินและ gap `23px`
- Mobile ต้องมีปุ่มเปิด/ปิดเมนูที่ keyboard ใช้งานได้ ห้ามซ่อน navigation โดยไม่มีทางเลือก
- ระบุ `aria-label="เมนูหลัก"` และมีสถานะ focus ที่มองเห็นได้

### Hero

- ใช้ภาพ local `/asset/images/content/cover1.png` เป็น baseline
- ความสูงขั้นต่ำ desktop `560px`, mobile `430px`
- ใช้ gradient overlay จากน้ำเงินเข้มไปโปร่ง เพื่อรักษาความอ่านง่ายของข้อความ
- เนื้อหา hero กว้างไม่เกิน `780px` และมี `h1` เพียงหนึ่งรายการต่อหน้า
- มี primary CTA หนึ่งรายการ; secondary CTA เพิ่มได้เมื่อมี destination ชัดเจน

### Section และ heading

- ใช้ `.section` สำหรับ vertical rhythm และ `.wrap` สำหรับจำกัดความกว้าง
- Section ปกติใช้พื้นขาว; `.section.alt` ใช้พื้นเขียวเทาอ่อน
- `h2` จัดกึ่งกลางเมื่อเป็นหัวข้อระดับ landing page
- เนื้อหายาวควรแบ่ง paragraph หรือ list ไม่ใช้ข้อความก้อนใหญ่ต่อเนื่อง

### Cards

- Desktop แสดงสามคอลัมน์; mobile ลดเป็นหนึ่งคอลัมน์
- Card ใช้พื้นขาว radius `10px` และเงาอ่อน
- ภาพ baseline เป็นวงกลม `220 × 220px` พร้อม `object-fit: cover`
- Cards ในชุดเดียวกันควรมีโครงสร้างและระดับ heading เดียวกัน

### Buttons และ links

- `.btn-main` คือ primary CTA: พื้นเหลือง ตัวอักษรน้ำเงินเข้ม
- Button/link ต้องมี hover, keyboard focus และ disabled state
- ข้อความ CTA ต้องเป็นคำกริยาที่บอกผล เช่น “ดูวัตถุประสงค์โครงการ” ไม่ใช้ “คลิกที่นี่”
- ห้ามวาง `<a href="#">` หากยังไม่มี destination; ใช้ข้อความธรรมดาหรือซ่อน feature จนพร้อม

### Form

- ทุก input มี `<label>` ที่ผูกด้วย `for`/`id`
- ใช้ชนิด input ตามข้อมูลจริง เช่น `email` และ `tel`
- Field กว้างเต็ม container, padding `11px`, border `#ccd4df`, radius `5px`
- ต้องมี validation ทั้ง client/server, anti-forgery, success/error feedback และ privacy notice ก่อนเปิดใช้งานจริง
- Error text ต้องอยู่ใกล้ field และประกาศผ่าน `aria-describedby`/live region ตามความเหมาะสม

### Map และ media

- Map กว้าง `100%`, สูง baseline `390px` และมี `title`
- รูป content ใช้ `width: 100%; height: auto;` พร้อมระบุ intrinsic `width`/`height` ใน markup เพื่อลด layout shift
- ภาพใต้ fold ใช้ `loading="lazy"`; hero image แรกเท่านั้นที่ควร preload
- ส่งออกภาพหลายขนาดเป็น AVIF/WebP และเก็บ fallback เมื่อจำเป็น

### Footer

- ใช้พื้น `#0b2c56`, ข้อความขาว และจัดกึ่งกลาง
- แสดงชื่อโครงการ ช่องทางติดต่อ และข้อมูลลิขสิทธิ์ที่เป็นปัจจุบัน
- ลิงก์ใน footer ต้องมี contrast และ focus state เทียบเท่าลิงก์ส่วนอื่น

## 5. Canonical CSS

CSS ชุดนี้เป็น baseline ที่สกัดจาก preview และเพิ่ม state ที่จำเป็นสำหรับการพัฒนา production ห้ามแก้ค่าหลักเฉพาะหน้า; หากต้องเปลี่ยน visual language ให้แก้ token หรืออัปเดตเอกสารนี้ก่อน

```css
@import url("https://fonts.googleapis.com/css2?family=Kanit:wght@400;500;600&family=Noto+Sans+Thai:wght@300;400;500;600&display=swap");

:root {
  --color-text: #17315b;
  --color-primary: #1176bc;
  --color-heading: #042da1;
  --color-navy: #0b2c56;
  --color-hero-text: #ffffff;
  --color-accent: #f5b41b;
  --color-accent-text: #092b58;
  --color-surface: #ffffff;
  --color-surface-soft: #f5f7fa;
  --color-section-alt: #d3dbcb;
  --color-process: #f4f4f5;
  --color-border: #ccd4df;
  --color-focus: #042da1;

  --font-heading: "Kanit", sans-serif;
  --font-body: "Noto Sans Thai", Tahoma, sans-serif;
  --content-max: 1170px;
  --page-gutter: 20px;
  --section-space: 60px;
  --radius-field: 5px;
  --radius-button: 6px;
  --radius-card: 10px;
  --shadow-header: 0 2px 12px rgb(0 0 0 / 8%);
  --shadow-card: 0 6px 20px rgb(18 48 88 / 8%);
}

*,
*::before,
*::after {
  box-sizing: border-box;
}

html {
  scroll-behavior: smooth;
}

body {
  margin: 0;
  color: var(--color-text);
  background: var(--color-surface);
  font-family: var(--font-body);
  line-height: 1.7;
}

button,
input,
textarea,
select {
  font: inherit;
}

img {
  max-width: 100%;
}

a {
  color: var(--color-primary);
}

a:focus-visible,
button:focus-visible,
input:focus-visible,
textarea:focus-visible,
select:focus-visible {
  outline: 3px solid var(--color-focus);
  outline-offset: 3px;
}

.wrap,
.header-inner,
.hero-copy {
  width: 100%;
  max-width: var(--content-max);
  margin-inline: auto;
}

.site-header {
  position: relative;
  z-index: 2;
  background: var(--color-surface);
  box-shadow: var(--shadow-header);
}

.header-inner {
  display: flex;
  gap: 28px;
  align-items: center;
  padding: 12px var(--page-gutter);
}

.logo-img {
  width: 145px;
  height: 62px;
  object-fit: contain;
}

.nav {
  display: flex;
  gap: 23px;
  align-items: center;
  margin-left: auto;
  font-size: 14px;
}

.nav a {
  color: var(--color-primary);
  font-weight: 500;
  text-decoration: none;
}

.nav a:hover {
  text-decoration: underline;
  text-underline-offset: 4px;
}

.hero {
  position: relative;
  min-height: 560px;
  background: url("/asset/images/content/cover1.png") center / cover no-repeat;
}

.hero::after {
  position: absolute;
  inset: 0;
  content: "";
  background: linear-gradient(90deg, rgb(2 34 88 / 78%), rgb(4 74 137 / 12%));
}

.hero-copy {
  position: relative;
  z-index: 1;
  padding: 120px var(--page-gutter);
  color: var(--color-hero-text);
}

.hero h1 {
  max-width: 780px;
  margin: 0 0 20px;
  font: 600 52px/1.18 var(--font-heading);
}

.hero p {
  max-width: 690px;
  font-size: 20px;
  line-height: 1.7;
}

.btn-main {
  display: inline-block;
  margin-top: 12px;
  padding: 14px 25px;
  border: 0;
  border-radius: var(--radius-button);
  color: var(--color-accent-text);
  background: var(--color-accent);
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
}

.btn-main:hover {
  filter: brightness(0.95);
  transform: translateY(-1px);
}

.btn-main:disabled,
.btn-main[aria-disabled="true"] {
  cursor: not-allowed;
  opacity: 0.55;
  transform: none;
}

.section {
  padding: var(--section-space) var(--page-gutter);
}

.section.alt {
  background: var(--color-section-alt);
}

.section h2 {
  margin: 0 0 35px;
  color: var(--color-heading);
  font: 600 40px/1.3 var(--font-heading);
  text-align: center;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 22px;
}

.card {
  padding: 22px 18px;
  border-radius: var(--radius-card);
  background: var(--color-surface);
  box-shadow: var(--shadow-card);
  text-align: center;
}

.card img {
  width: 220px;
  height: 220px;
  border-radius: 50%;
  object-fit: cover;
}

.card h3 {
  color: var(--color-heading);
  font: 500 19px/1.55 var(--font-heading);
}

.process {
  display: block;
  width: 100%;
  height: auto;
  background: var(--color-process);
}

.story {
  max-width: 780px;
  margin-inline: auto;
  color: var(--color-text);
  font-size: 19px;
  line-height: 1.8;
}

.contact {
  background: var(--color-surface-soft);
}

.contact-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 32px;
}

.field {
  margin-bottom: 15px;
}

.field label {
  display: block;
  margin-bottom: 6px;
  font-weight: 500;
}

.field input,
.field textarea,
.field select {
  width: 100%;
  max-width: none;
  padding: 11px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-field);
  background: var(--color-surface);
}

.map {
  width: 100%;
  height: 390px;
  border: 0;
}

.footer {
  padding: 28px var(--page-gutter);
  color: var(--color-surface);
  background: var(--color-navy);
  text-align: center;
  line-height: 1.7;
}

@media (max-width: 900px) {
  .header-inner {
    gap: 16px;
  }

  .nav {
    display: none;
  }

  .hero {
    min-height: 430px;
  }

  .hero-copy {
    padding: 80px var(--page-gutter);
  }

  .hero h1 {
    font-size: 38px;
  }

  .hero p {
    font-size: 18px;
  }

  .cards,
  .contact-grid {
    grid-template-columns: 1fr;
  }

  .section h2 {
    font-size: 34px;
  }
}

@media (prefers-reduced-motion: reduce) {
  html {
    scroll-behavior: auto;
  }

  *,
  *::before,
  *::after {
    scroll-behavior: auto !important;
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
  }
}
```

หมายเหตุ: media query ด้านบนซ่อน `.nav` เพื่อให้ตรงกับ preview แต่ production ต้องเพิ่ม mobile-menu button และ panel ที่เข้าถึงได้ก่อนใช้งานจริง

## 6. Asset baseline

| หน้าที่ | Asset |
|---|---|
| Project logo | `/asset/images/content/logo1.png` |
| Hero baseline | `/asset/images/content/cover1.png` |
| Alternate hero | `/asset/images/content/cover2.png` |
| Process diagram | `/asset/images/content/content1.png` |

ใน Razor ใช้ `~/asset/...`; ใน CSS ที่เสิร์ฟจาก `wwwroot` ใช้ root-relative URL `/asset/...` เพื่อไม่ให้ path ขึ้นกับหน้าปัจจุบัน

รูป objective cards ที่ใช้ URL ภายนอกใน preview เป็นเพียงตัวอย่าง ห้ามถือเป็น approved assets สำหรับ production ต้องเปลี่ยนเป็นไฟล์ local ที่ตรวจสิทธิ์แล้ว

## 7. Responsive behavior

ที่ viewport ไม่เกิน `900px`:

- เปลี่ยน cards และ contact grid เป็นหนึ่งคอลัมน์
- ลด hero height และ spacing
- ลด `h1` เป็น `38px`, lead เป็น `18px`, section heading เป็น `34px`
- แทน desktop navigation ด้วย mobile menu ที่ใช้งานผ่าน keyboard และ screen reader ได้
- ห้ามเกิด horizontal scrolling ที่ความกว้าง `320px`

ควรตรวจอย่างน้อยที่ `320px`, `375px`, `768px`, `900px`, `1024px` และ `1440px`

## 8. Accessibility และ content rules

- ใช้ `<html lang="th">`
- แต่ละหน้ามี `<h1>` หนึ่งรายการและ heading ลดหลั่นตามลำดับ
- ภาพที่มีความหมายต้องมี alt text ภาษาไทย; ภาพตกแต่งใช้ `alt=""`
- Interactive element ทุกชิ้นใช้งานด้วย keyboard ได้และมี visible focus
- Text contrast ต้องผ่าน WCAG AA
- Form status ต้องไม่พึ่ง alert สีเพียงอย่างเดียว
- CTA และ navigation ใช้ข้อความอธิบาย destination ได้โดยไม่ต้องอ่านบริบทรอบข้าง
- หลีกเลี่ยง auto-play; หากจำเป็นต้องมี motion ให้เคารพ `prefers-reduced-motion`

## 9. Do / Don't

### Do

- ใช้ token แทนการเขียนค่าสีซ้ำ
- ใช้ `.section` + `.wrap` เพื่อคุม spacing และความกว้าง
- ใช้ Kanit สำหรับ heading และ Noto Sans Thai สำหรับ body/UI
- ใช้ local, optimized, responsive images
- เขียน component state ให้ครบ: default, hover, focus, disabled, error และ success

### Don't

- อย่าเพิ่ม inline style ใน Razor view
- อย่านำ selector จากธีม e-commerce เก่ามาใช้กับ component ใหม่โดยไม่จำเป็น
- อย่าเพิ่ม font family หรือสีใหม่เฉพาะหน้า
- อย่า hotlink รูปจาก third-party
- อย่าซ่อน navigation บน mobile โดยไม่มีเมนูทดแทน
- อย่าใช้ slider เพื่อแทนข้อความนำและ CTA ที่ชัดเจน

## 10. Checklist สำหรับหน้าใหม่

- [ ] ใช้ tokens และ component classes จากเอกสารนี้
- [ ] มี `title`, description, canonical และ social metadata ที่ตรงกับหน้า
- [ ] มี semantic `h1` และ heading hierarchy ที่ถูกต้อง
- [ ] Primary CTA มี destination/behavior จริง
- [ ] Responsive ที่ `320–1440px` โดยไม่มี horizontal overflow
- [ ] Keyboard navigation และ focus state ผ่าน
- [ ] รูปมี local ownership, alt, dimensions และ format/size ที่เหมาะสม
- [ ] ไม่มี broken link, placeholder action หรือ mixed content
- [ ] Form มี validation, anti-forgery, feedback และ privacy handling เมื่อเกี่ยวข้อง
- [ ] ตรวจ runtime บน macOS และ Windows หลังเปลี่ยน CSS/markup

## 11. Change policy

หากต้องเปลี่ยนสีหลัก ฟอนต์ spacing, breakpoint หรือ component shape ให้แก้ design token และเอกสารนี้ก่อน แล้วจึงปรับ stylesheet/หน้าเว็บที่ใช้งาน การสร้าง variant ต้องตั้งชื่อตามหน้าที่ เช่น `.btn-secondary` หรือ `.section-muted` ไม่ใช้ชื่อจากสีอย่าง `.blue-button`
