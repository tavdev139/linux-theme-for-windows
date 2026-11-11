# Arch Linux Theme for Windows

Tema dan shell overlay Arch Linux untuk Windows 10/11. Mengubah tampilan Windows menjadi lebih mirip desktop Arch Linux yang rapi dan modern, dengan dock, dynamic island, search bar, WiFi panel, auto-hide taskbar, dan kartu media.

## Fitur Utama

- **Arch Dock** — bar di bawah layar dengan launcher aplikasi (Files, Browser, Terminal, Code, Arduino IDE, Antigravity, Network, Volume, Settings), search bar, tombol WiFi, dan daftar aplikasi yang sedang terbuka.
- **Arch Panel (Dynamic Island)** — bar di atas layar dengan jam, CPU/RAM, dan kartu Now Playing untuk musik/video yang sedang diputar (dari Windows Media Session).
- **Auto-hide taskbar** — taskbar Windows disembunyikan saat Arch Panel berjalan, dan kembali saat aplikasi ditutup. Native auto-hide setting Windows tidak diubah.
- **Search aplikasi** — buka jendela pencarian dari dock untuk mencari dan meluncur aplikasi terpasang.
- **Panel WiFi** — scan jaringan, lihat kekuatan sinyal, dan coba koneksi ke WiFi.
- **Lock screen Linux style** — preview visual lock screen bergaya Linux (autentikasi tetap bawaan Windows, bukan pengganti login).
- **Wallpaper & tema** — wallpaper Arch Linux, dark mode, dan aksen biru Arch `#1793D1`.

## Cara Menjalankan

1. Pastikan file `ArchPanel.exe` ada di folder `C:\Users\labta\Downloads\theme`.
2. Klik dua kali `jalankan_dynamic_island.bat` untuk langsung menjalankan Arch Panel + Arch Dock.
3. Arch Dock akan muncul di bawah layar dan Arch Panel di atas. Taskbar Windows akan disembunyikan selama Arch Panel berjalan.
4. Arahkan pointer mouse ke tepi bawah layar untuk memunculkan dock jika sedang tersembunyi.
5. Klik kanan pada dock untuk menu opsi (Center Dock, Auto-hide, Exit Arch widgets).

## Menghentikan & Memulihkan Taskbar Windows

- Jalankan `stop_panels.ps1` untuk menghentikan Arch Panel dan memulihkan taskbar Windows.
- Jika taskbar tidak kembali setelah proses dihentikan paksa, jalankan `restore_windows_taskbar.ps1`.
- Untuk mencopot sepenuhnya dari startup, jalankan `uninstall.ps1`.

## Struktur Folder

```
theme/
├── ArchPanel.exe              # Build aplikasi panel + dock
├── jalankan_dynamic_island.bat # Peluncur cepat
├── stop_panels.ps1            # Hentikan panel & kembalikan taskbar
├── restore_windows_taskbar.ps1 # Pemulihan paksa taskbar
├── uninstall.ps1              # Cabut dari startup & registry
├── install_permanent.ps1      # Pemasangan permanen ke startup
├── ArchDockWindow.cs          # Kode sumber dock (launcher, search, WiFi, open apps)
├── DynamicIsland.cs           # Kode sumber panel atas (jam, CPU/RAM, media)
├── ArchAppSearchCatalog.cs    # Katalog pencarian aplikasi
├── ArchDockVisibilityPolicy.cs# Kebijakan auto-hide dock
├── BrowserVisibilityPolicy.cs # Deteksi browser aktif
├── NowPlayingProvider.cs      # Sesi media Windows (Now Playing)
├── WindowsTaskbarVisibility.cs# Sembunyikan/pulihkan taskbar Windows
├── ArchNetworkWindow.cs       # Panel WiFi & jaringan
├── CARA_UBAH_WINDOWS_JADI_LINUX.md # Panduan pemasangan
├── wallpapers/                # Wallpaper Arch Linux
└── tests/                     # Tes otomatis (auto-hide, launcher, search, media)
```

## Catatan Keamanan

- Taskbar Windows hanya disembunyikan (ShowWindow SW_HIDE), tidak dihapus. Taskbar akan kembali saat Arch Panel ditutup atau `stop_panels.ps1` dijalankan.
- Lock screen yang disertakan hanya visual preview; autentikasi tetap menggunakan Windows native (Win+L).
- Build artifacts (`.exe`, `.dll`, backup) tidak disimpan di GitHub dan hanya ada secara lokal.

## Lisensi

MIT License.
