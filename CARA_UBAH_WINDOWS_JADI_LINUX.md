# Panduan Mengubah Tampilan Windows & Taskbar Jadi Mirip Linux 🐧💻

Panduan ini membuat antarmuka Windows 10 / 11 Anda berubah menjadi **Desktop Linux Ubuntu GNOME & Dynamic Island modern**:
1. **Bagian Atas Layar**: Memiliki **Dynamic Island Linux** (Pill mengambang modern, tidak menghalangi tab browser atau tombol close jendela, dapat dipindah/drag, dan menampilkan CPU, RAM, Media, jam, serta jalan pintas aplikasi).
2. **Bagian Bawah Layar (Taskbar)**: Berubah menjadi **Floating Linux Dock** (ikon aplikasi melayang bening tanpa kotak taskbar Windows kaku).
3. **Sistem**: Wallpaper resmi Linux HD + Windows Dark Mode + Aksen Oranye Ubuntu `#E95420`.

---

## 🚀 Cara Paling Praktis (1-Klik Tanpa Perlu Buka Terminal)

Jika Anda **tidak ingin repot membuka CMD / PowerShell / Terminal**, gunakan file peluncur VBScript otomatis berikut:

### 1. Pasang Permanen Otomatis (Tanpa Terminal)
1. Buka folder `c:\Users\labta\Downloads\theme` di File Explorer Windows Anda.
2. Cukup klik dua kali file:
   👉 [`PASANG_PERMANEN_LINUX.vbs`](file:///c:/Users/labta/Downloads/theme/PASANG_PERMANEN_LINUX.vbs)
3. Apa yang otomatis terjadi:
   - ✅ **Bilah lama yang menutupi layar langsung dihentikan dan dibersihkan**.
   - ✅ **Dynamic Island langsung aktif melayang di tengah atas**.
   - ✅ **Mode No-Cover otomatis aktif**: Windows mengatur agar jendela aplikasi penuh (Chrome, WhatsApp, VS Code) selalu berada di bawah Dynamic Island, sehingga **tab browser tidak akan pernah tertutup**.
   - ✅ **Otomatis terdaftar ke Startup Windows**: Setiap kali Anda menyalakan laptop/PC, tema dan Dynamic Island otomatis aktif sendiri di latar belakang tanpa jendela hitam terminal!
   - ✅ Wallpaper Ubuntu HD dan Windows Dark Mode langsung aktif.

---

### 2. Pratinjau Tampilan Lock Screen Bergaya Linux
Untuk melihat mockup visual:
- Klik dua kali [`KUNCI_LAYAR_LINUX.vbs`](file:///c:/Users/labta/Downloads/theme/KUNCI_LAYAR_LINUX.vbs).
- Ini hanya pratinjau; Windows **belum terkunci** dan pratinjau tidak meminta atau memeriksa kata sandi.
- Klik **Kunci Windows** untuk menutup pratinjau dan membuka lockscreen Windows yang sebenarnya. Tombol **Tutup pratinjau** atau Esc hanya menutup mockup.
- Pintasan aman Windows kapan saja: **Win+L**.
- Windows tidak mengizinkan aplikasi biasa mengganti UI autentikasi lockscreen yang aman dengan tampilan custom. Wallpaper lockscreen dapat diubah, tetapi proses masuk tetap milik Windows.

---

### 3. Cara Menghentikan / Mencopot (Jika Ingin Kembali Normal)
Jika suatu saat Anda ingin mematikan Dynamic Island dan mengembalikan layar normal:
- Cukup klik dua kali file:
  👉 [`HAPUS_DAN_HENTIKAN_TEMA.vbs`](file:///c:/Users/labta/Downloads/theme/HAPUS_DAN_HENTIKAN_TEMA.vbs)
- Dynamic Island akan langsung ditutup, dicopot dari startup, dan ukuran layar dikembalikan 100% normal.

---

## 🏝️ Fitur Dynamic Island Linux (Tidak Menghalangi Layar)

Jika bilah top bar sebelumnya menutupi tab browser atau judul jendela, **Dynamic Island** adalah solusinya:

- **Bentuk Pill Ringkas di Tengah Atas**:
  - Hanya berada di area tengah atas (kosong), sehingga tab browser di kiri dan tombol close/minimize di kanan **100% bebas tidak terhalang**.
- **Dapat Dipindahkan (Draggable)**:
  - Klik kiri mouse tahan pada pill Dynamic Island, lalu geser ke mana pun Anda suka.
- **Interaktif saat Diarahkan Mouse (Hover / Klik)**:
  - Mengembang (expand) menjadi dasbor kontrol Linux lengkap:
    - **Monitor Sistem Real-time**: Grafik penggunaan CPU dan RAM.
    - **Now Playing**: Saat musik/video mulai diputar, panel mengembang otomatis dengan artwork, judul, artis, dan kontrol Previous/Play-Pause/Next.
    - **Peluncur Cepat 1-Klik**: Tombol cepat untuk membuka Terminal (`wt.exe` / `powershell`), Files (File Explorer), Browser, dan Pengaturan.
    - **Menu Daya**: Pratinjau Linux (bukan pengunci), kunci Windows asli, Sleep, Restart, Power Off, dan Tutup Island.
- **Klik Kanan pada Pill**:
  - **Center on Screen**: Mengembalikan pill ke posisi tengah atas.
  - **Always on Top (ON/OFF)**: Mengatur apakah pill selalu di atas aplikasi lain.
  - **Sembunyikan saat browser aktif**: Centang agar panel bersembunyi saat jendela Chrome, Edge, Firefox, atau browser yang terdeteksi sedang aktif; pengaturan disimpan untuk sesi berikutnya.
  - **Exit Dynamic Island**: Menutup aplikasi.
- **Arch Dock di Bawah**: Dock melayang dengan launcher Files, Browser, Terminal, Code, Arduino IDE, Antigravity, Network, Volume, Settings, dan jam. Dock otomatis bersembunyi saat aplikasi lain aktif; arahkan pointer ke tepi bawah untuk menampilkannya. Taskbar Windows disembunyikan saat Arch Panel berjalan dan kembali saat aplikasi ditutup.
- Jika taskbar tidak kembali setelah proses dihentikan paksa, jalankan [`restore_windows_taskbar.ps1`](file:///c:/Users/labta/Downloads/theme/restore_windows_taskbar.ps1).

File peluncur cepat:
- Klik dua kali [`jalankan_dynamic_island.bat`](file:///c:/Users/labta/Downloads/theme/jalankan_dynamic_island.bat) untuk langsung menyalakan Dynamic Island.

---

## 🛠️ Langkah-Langkah Manual & Kustomisasi Taskbar

### 1. Menjadikan Taskbar Windows Seperti Dock Linux (Bening/Melayang)
Agar taskbar Windows tidak lagi berbentuk balok kotak kaku, melainkan transparan seperti dock Linux:

- **Langkah A (Otomatis via winget):**
  Buka PowerShell / CMD, lalu jalankan:
  ```powershell
  winget install --id CharlesMilette.TranslucentTB -e --accept-source-agreements --accept-package-agreements
  ```
  *Setelah terpasang, taskbar Windows Anda otomatis menjadi 100% bening tembus pandang (ikon aplikasi tampak melayang di atas wallpaper seperti dock Linux).*

- **Langkah B (Mengatur Posisi Taskbar ala Ubuntu):**
  1. Klik kanan pada area kosong Taskbar ➔ pilih **Taskbar settings** (Pengaturan taskbar).
  2. Buka menu **Taskbar behaviors** (Perilaku taskbar):
     - **Taskbar alignment**:
       - Pilih **Center** jika ingin dock di tengah (mirip GNOME 45 / modern Linux dock).
       - Pilih **Left** jika ingin dock di sebelah kiri (mirip Ubuntu klasik).
     - Centang **"Automatically hide the taskbar"** jika ingin taskbar otomatis bersembunyi saat Anda membuka jendela aplikasi penuh.

---

### 2. Pilihan Panel Atas Alternatif (GNOME Top Bar Klasik)
Jika Anda tetap ingin mencoba bilah penuh ala GNOME klasik:
- Jalankan file [`jalankan_gnome_panel.bat`](file:///c:/Users/labta/Downloads/theme/jalankan_gnome_panel.bat).
- *(Untuk menutup panel: klik tombol Power di kanan atas ➔ pilih "Exit GNOME Panel").*

---

### 3. Mengubah Tema Warna Sistem Windows
Di folder `theme`, klik dua kali salah satu tema sistem:
- [`Ubuntu_Linux_Dark.theme`](file:///c:/Users/labta/Downloads/theme/Ubuntu_Linux_Dark.theme) (Aksen Oranye Ubuntu `#E95420`)
- [`Matrix_Cyber_Linux.theme`](file:///c:/Users/labta/Downloads/theme/Matrix_Cyber_Linux.theme) (Aksen Hijau Cyber Matrix `#00FF66`)
- [`Arch_Linux_Dark.theme`](file:///c:/Users/labta/Downloads/theme/Arch_Linux_Dark.theme) (Aksen Biru Cyan Arch `#1793D1`)
- [`Matrix_Cyber_Linux.theme`](file:///c:/Users/labta/Downloads/theme/Matrix_Cyber_Linux.theme) (Aksen Hijau Hacker `#00FF66`)
- [`Arch_Linux_Dark.theme`](file:///c:/Users/labta/Downloads/theme/Arch_Linux_Dark.theme) (Aksen Biru Arch `#1793D1`)
