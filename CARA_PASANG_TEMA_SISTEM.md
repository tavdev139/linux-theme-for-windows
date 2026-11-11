# Panduan Memasang Tema SISTEM (Bukan Web) di Komputer & HP Samsung 🐧💻📱

Panduan ini menjelaskan cara menerapkan tema Linux secara **langsung pada level sistem operasi** (bukan melalui browser/web):
- **Di Komputer (Windows PC / Laptop):** Menggunakan berkas tema asli Windows (`.theme`), registry warna aksen, dan wallpaper sistem.
- **Di HP Samsung (One UI):** Menggunakan wallpaper HD resolusi penuh (1080x2400), modul resmi **Samsung Theme Park**, dan icon pack Linux.

---

## 💻 1. CARA PASANG DI SISTEM KOMPUTER (WINDOWS PC)

Kami telah membuatkan berkas **Tema Sistem Resmi Windows (`.theme`)** dan skrip otomatis 1-klik:

### Cara Praktis (1-Klik via Skrip Otomatis):
1. Buka folder `c:\Users\labta\Downloads\theme`.
2. Klik kanan pada file [`pasang_tema_windows.bat`](file:///c:/Users/labta/Downloads/theme/pasang_tema_windows.bat) lalu pilih **"Run as administrator"** (atau klik dua kali).
3. Pilih nomor varian tema yang diinginkan:
   - **[1] Ubuntu Linux Dark** (Wallpaper Ubuntu + Dark Mode + Aksen Oranye `#E95420`).
   - **[2] Matrix Cyber Linux** (Wallpaper Matrix Terminal + Dark Mode + Aksen Hijau `#00FF66`).
   - **[3] Arch Linux Dark** (Wallpaper Arch Minimal + Dark Mode + Aksen Biru `#1793D1`).
4. Skrip akan secara otomatis:
   - Mengubah wallpaper desktop Windows Anda ke gambar HD PNG.
   - Mengaktifkan Windows Dark Mode untuk sistem dan aplikasi.
   - Mengubah warna aksen tombol, taskbar, dan batas jendela Windows.
   - Mendaftarkan dan membuka tema ke dalam Windows Settings.

### Cara Manual (Melalui Windows Theme):
1. Di folder `theme`, klik dua kali salah satu berkas tema:
   - [`Ubuntu_Linux_Dark.theme`](file:///c:/Users/labta/Downloads/theme/Ubuntu_Linux_Dark.theme)
   - [`Matrix_Cyber_Linux.theme`](file:///c:/Users/labta/Downloads/theme/Matrix_Cyber_Linux.theme)
   - [`Arch_Linux_Dark.theme`](file:///c:/Users/labta/Downloads/theme/Arch_Linux_Dark.theme)
2. Windows Settings akan otomatis terbuka dan langsung menerapkan tema ke seluruh sistem.

---

## 📱 2. CARA PASANG DI SISTEM HP SAMSUNG (ONE UI)

Untuk HP Samsung Galaxy, Samsung menggunakan sistem **Theme Overlay Engine**. Cara resmi, aman, dan tanpa root untuk memasang tema sistem khusus adalah melalui modul resmi Samsung: **Good Lock (Theme Park)**.

### Langkah 1: Pindahkan Wallpaper Linux HD ke HP Samsung
Di folder `wallpapers/`, kami telah mengonversi gambar ke format **PNG resolusi tinggi (1080x2400)** khusus layar AMOLED Samsung:
- [`ubuntu_galaxy.png`](file:///c:/Users/labta/Downloads/theme/wallpapers/ubuntu_galaxy.png) (Ubuntu Noble)
- [`matrix_terminal.png`](file:///c:/Users/labta/Downloads/theme/wallpapers/matrix_terminal.png) (Matrix Cyberpunk)
- [`arch_dark.png`](file:///c:/Users/labta/Downloads/theme/wallpapers/arch_dark.png) (Arch Linux Dark)

**Cara pasang:**
1. Kirim file PNG tersebut ke HP Samsung (bisa lewat kabel data, WhatsApp, Telegram, Google Drive, atau Quick Share).
2. Buka gambar di aplikasi **Galeri Samsung** ➔ tekan titik tiga ➔ pilih **"Atur sebagai wallpaper"** (Set as wallpaper) ➔ pilih **Layar Depan & Layar Kunci**.

---

### Langkah 2: Terapkan Tema Sistem Menggunakan Samsung Theme Park (Resmi)
Theme Park adalah aplikasi pembuat tema sistem resmi dari Samsung Electronics yang mengubah seluruh elemen One UI (Dialer, Pengaturan, Notifikasi, Pesan, Keyboard, dan Ikon).

1. Buka **Galaxy Store** di HP Samsung Anda.
2. Cari dan unduh aplikasi **Good Lock** (jika Good Lock belum tersedia di region Anda, gunakan **Fine Lock** atau **NiceLock** dari Google Play Store).
3. Di dalam Good Lock, pilih tab *Make up* ➔ pasang modul **Theme Park**.
4. Buka **Theme Park** ➔ pilih tab **Theme** ➔ ketuk **Create New**:
   - Pilih wallpaper yang baru Anda pasang (misal: `ubuntu_galaxy.png`).
   - Theme Park akan otomatis menganalisis warna wallpaper.
   - Atur palet warna sistem:
     - **Warna Aksen Utama:** Pilih warna oranye Ubuntu `#E95420` (atau hijau `#00FF66`).
     - **Warna Latar Belakang:** Pilih warna gelap `#1A0A2E` atau `#000000` (AMOLED Black).
5. Di menu atas Theme Park, Anda bisa menyesuaikan:
   - **Quick Panel:** Ubah warna tombol Wi-Fi/Bluetooth saat aktif menjadi oranye/hijau.
   - **Keyboard:** Ubah tema keyboard Samsung menjadi warna Linux Dark.
6. Ketuk ikon **Download / Simpan** di pojok kanan atas ➔ beri nama tema (contoh: `Ubuntu Linux`) ➔ ketuk **Install**.
7. Setelah selesai dipasang, ketuk tema tersebut lalu pilih **Apply** (Terapkan).
   - *Sekarang seluruh sistem Samsung One UI Anda sudah berganti menjadi tema Linux!*

---

### Langkah 3: Mengganti Ikon Sistem Menjadi Ikon Resmi Linux
Agar semua ikon aplikasi di HP Samsung Anda berbentuk ikon Linux:
1. Unduh **Papirus Icon Pack** dari Google Play Store (ini adalah icon pack resmi Linux paling populer).
2. Buka **Theme Park** di HP Samsung ➔ pilih tab **Icon** di bagian bawah.
3. Ketuk **Create New** ➔ ketuk **Iconpack** ➔ pilih **Papirus**.
4. Ketuk ikon Simpan di kanan atas, lalu ketuk **Apply**.
   - *Semua ikon aplikasi sistem di layar HP Anda kini berwujud ikon Linux!*

---

### Langkah 4 (Opsional): Menjadikan HP Samsung Memiliki Terminal Linux Nyata
Jika Anda ingin layar HP Anda memiliki terminal Linux interaktif sungguhan:
1. Pasang aplikasi **Termux** dari [F-Droid](https://f-droid.org/packages/com.termux/) (gratis dan open-source).
2. Pasang **T-UI Launcher** dari Play Store jika ingin seluruh antarmuka HP Anda digantikan oleh Linux CLI Terminal, atau pasang **Termux:Widget** untuk menaruh widget `neofetch` di layar depan.
