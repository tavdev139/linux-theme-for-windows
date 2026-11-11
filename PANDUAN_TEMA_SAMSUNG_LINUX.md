# Panduan Tema Linux Multi-Device: HP Samsung & Komputer (PC/Laptop) 🐧📱💻

Tema ini dirancang secara **Universal & Adaptif (Multi-Device)**, sehingga dapat digunakan langsung di:
1. **HP Samsung Galaxy** (Layar sentuh, One UI, PWA fullscreen, format potret 20:9 AMOLED).
2. **Komputer / PC / Laptop** (Windows, Mac, Linux dengan jendela melayang yang bisa digeser, multi-window multitasking, shortcut keyboard, dan format lanskap 16:9).
3. **Samsung DeX Mode** (Saat HP Samsung dihubungkan ke monitor/TV/komputer, tampilan langsung bertransformasi menjadi desktop Linux sesungguhnya!).

---

## 📁 Struktur Berkas Lengkap

```text
c:\Users\labta\Downloads\theme\
├── index.html                   # Entrypoint utama simulator Linux Multi-Device
├── manifest.json                # Web App Manifest (PWA untuk HP & PC)
├── PANDUAN_TEMA_SAMSUNG_LINUX.md# Panduan lengkap ini
├── css/
│   ├── style.css                # Base colors, typography, lockscreen
│   ├── desktop.css              # Top panel, dock, context menu, mode switch
│   ├── terminal.css             # Terminal, Neofetch, htop
│   ├── apps.css                 # Floating multi-window, settings, editor, dll.
│   └── filemanager.css          # Nautilus file manager
├── js/
│   ├── clock.js                 # Real-time clock & date
│   ├── lockscreen.js            # Lockscreen gestures & password unlock
│   ├── desktop.js               # Multi-device controller, shortcuts, wallpaper
│   ├── terminal.js              # Linux bash shell commands engine
│   ├── apps.js                  # Apps logic (SysMonitor, Calculator, VSCode, Music)
│   ├── filemanager.js           # Directory tree & file explorer
│   └── windowmanager.js         # Window drag & drop, multi-window layering
└── wallpapers/
    ├── ubuntu_galaxy.svg        # Wallpaper Ubuntu Mobile (1080x2400)
    ├── matrix_terminal.svg      # Wallpaper Matrix Terminal Mobile (1080x2400)
    ├── arch_dark.svg            # Wallpaper Arch Minimalist Mobile (1080x2400)
    ├── ubuntu_desktop.svg       # Wallpaper Ubuntu Desktop PC (1920x1080)
    ├── matrix_desktop.svg       # Wallpaper Matrix Desktop PC (1920x1080)
    └── arch_desktop.svg         # Wallpaper Arch Desktop PC (1920x1080)
```

---

## 💻 Penggunaan di Komputer / PC / Laptop

### 1. Cara Menjalankan
Cukup buka berkas `index.html` dengan browser apa saja (Google Chrome, Microsoft Edge, Firefox, Brave):
- **Cara 1 (Langsung):** Klik dua kali pada berkas `index.html`.
- **Cara 2 (Web Server lokal):**
  ```powershell
  cd c:\Users\labta\Downloads\theme
  python -m http.server 8080
  ```
  Lalu buka `http://localhost:8080` di browser.

### 2. Fitur Desktop & Multitasking di Komputer:
- **🪟 Jendela Melayang (Floating Windows):**
  - Klik dan tahan pada *titlebar* jendela untuk **menggeser posisi jendela (Drag & Drop)**.
  - **Double-click titlebar** untuk memaksimalkan (*maximize*) atau memulihkan ukuran jendela.
  - Tarik sudut kanan-bawah jendela untuk mengubah ukuran (*resize*).
  - Buka banyak aplikasi sekaligus berdampingan (contoh: Terminal + VS Code + System Monitor).
- **🖱️ Klik Kanan Desktop (Linux Context Menu):**
  - Klik kanan di area desktop mana saja untuk membuka menu: Buka Terminal, Ubah Wallpaper, Buka Files, Pengaturan, atau Layar Penuh.
- **⌨️ Pintasan Keyboard Komputer (Linux Shortcuts):**
  - `Ctrl + Alt + T` : Langsung membuka Terminal Linux.
  - `Alt + F1` atau tombol `Windows / Super` : Membuka Activities / Menu Aplikasi.
  - `F11` : Beralih ke mode Layar Penuh (*Fullscreen OS Mode*).
  - `Escape` : Menutup menu, popup, dan overlay.
- **🖼️ Pengganti Wallpaper Instan:**
  - Klik ikon gambar di panel kanan atas atau klik kanan desktop -> **Change Background** untuk memilih tema (Ubuntu Noble, Matrix Cyber, Arch Dark, Cyan, atau Pure AMOLED Black).

---

## 📱 Penggunaan di HP Samsung Galaxy

### 1. Cara Memasang sebagai Aplikasi di Layar Utama (PWA):
1. Pastikan HP dan komputer berada di jaringan Wi-Fi yang sama, jalankan server di PC (`python -m http.server 8080`).
2. Di HP Samsung, buka browser **Samsung Internet** atau **Google Chrome**, lalu kunjungi `http://<IP_KOMPUTER>:8080`.
3. Tekan tombol menu browser (titik 3 di Chrome, atau garis 3 di Samsung Internet), pilih:
   - **"Add to Home screen"** / **"Tambahkan ke Layar Depan"**.
4. Ikon **LinuxTheme** akan terpasang di HP Anda. Saat dibuka, aplikasi akan berjalan **layar penuh tanpa bingkai browser**, memberikan pengalaman sistem operasi Linux portabel seutuhnya!

### 2. Fitur Khusus HP:
- **Lockscreen Swipe & Touch:** Geser layar ke atas atau ketuk ikon avatar untuk membuka kunci.
- **Mobile Touch Dock:** Ikon berukuran pas untuk sentuhan jari dengan indikator aplikasi aktif.
- **Quick Settings Dropdown:** Sentuh jam di bagian atas layar untuk memunculkan toggle Wi-Fi, Bluetooth, Dark Mode, dan slider kecerahan.
- **Samsung DeX Auto-Adapt:** Jika Anda menyambungkan HP Samsung ke monitor via Samsung DeX (kabel HDMI atau nirkabel), tampilan otomatis beradaptasi menjadi mode desktop dengan jendela melayang!
- **Tombol Alih Mode Manual:** Anda juga bisa menekan tombol **"DeX Mode / Phone View"** di panel kiri atas kapan saja untuk berpindah antara tampilan HP dan tampilan Desktop PC.

---

## 🎨 Menerapkan Tampilan Linux pada One UI Asli HP Samsung

Jika Anda ingin sistem operasi bawaan Samsung One UI Anda juga bertema Linux:

1. **Gunakan Wallpaper Linux:**
   - Gunakan berkas `wallpapers/ubuntu_galaxy.svg`, `wallpapers/matrix_terminal.svg`, atau `wallpapers/arch_dark.svg` sebagai latar layar depan dan layar kunci HP Anda.
2. **Kustomisasi via Good Lock & Theme Park:**
   - Unduh **Good Lock** dari **Galaxy Store**, lalu pasang modul **Theme Park**.
   - Buat tema baru dengan palet warna:
     - **Aksen Ubuntu**: `#E95420` (Oranye) & `#77216F` (Aubergine).
     - **Background AMOLED**: `#1A0A2E` atau `#000000`.
     - **Aksen Matrix Hacker**: `#00FF66` & `#050A07`.
3. **Icon Pack:**
   - Di Theme Park -> pilih menu **Icon**, lalu pasang **Papirus Icon Pack** dari Play Store.
4. **Terminal Widget:**
   - Pasang **Termux** + **Termux:Widget** dari F-Droid untuk menampilkan informasi `neofetch` langsung di layar depan HP Anda.

---

## 🖥️ Menerapkan Wallpaper di Komputer PC
Bagi komputer PC/laptop Anda, berkas wallpaper lanskap resolusi tinggi (1920x1080) telah disiapkan di:
- `wallpapers/ubuntu_desktop.svg`
- `wallpapers/matrix_desktop.svg`
- `wallpapers/arch_desktop.svg`
Berkas SVG ini berbasis vektor murni dan dapat diperbesar hingga resolusi 4K / 8K tanpa pecah!
