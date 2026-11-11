using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

public class Program {
    [STAThread]
    public static void Main() {
        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "island_error.txt");
        AppDomain.CurrentDomain.UnhandledException += (s, e) => {
            try { File.WriteAllText(logPath, e.ExceptionObject.ToString()); } catch {}
            try { WindowsTaskbarVisibility.RestoreNativeTaskbar(); } catch {}
        };
        AppDomain.CurrentDomain.ProcessExit += (s, e) => {
            try { WindowsTaskbarVisibility.RestoreNativeTaskbar(); } catch {}
        };
        try {
            App app = new App();
            IslandWindow island = new IslandWindow();
            app.MainWindow = island;
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.Exit += (s, e) => WindowsTaskbarVisibility.RestoreNativeTaskbar();
            app.Startup += (s, e) => {
                try {
                    new ArchDockWindow().Show();
                } catch (Exception dockError) {
                    try { WindowsTaskbarVisibility.RestoreNativeTaskbar(); } catch {}
                    try { File.AppendAllText(logPath, dockError.ToString()); } catch {}
                }
            };
            app.Run(island);
        } catch (Exception ex) {
            try { File.WriteAllText(logPath, ex.ToString()); } catch {}
        }
    }
}

public class App : Application { }

public class IslandWindow : Window {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

    private const uint SPI_GETWORKAREA = 0x0030;
    private const uint SPI_SETWORKAREA = 0x002F;
    private const uint SPIF_SENDCHANGE = 0x0002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MemoryStatusEx {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx status);

    private static int ReadPhysicalMemoryPercent() {
        MemoryStatusEx status = new MemoryStatusEx();
        status.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
        return GlobalMemoryStatusEx(status) ? (int)status.dwMemoryLoad : -1;
    }

    private static RECT s_origRect;
    private static bool s_hasSaved = false;
    private static bool s_isReserved = false;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private Border m_islandBorder;
    private StackPanel m_pillCollapsed;
    private Border m_panelExpanded;
    private TextBlock m_pillClock;
    private TextBlock m_pillSubtitle;
    private TextBlock m_expandTime;
    private TextBlock m_expandDate;
    private TextBlock m_txtCpu;
    private ProgressBar m_barCpu;
    private TextBlock m_txtRam;
    private ProgressBar m_barRam;
    private Border m_mediaCard;
    private Image m_mediaArtwork;
    private TextBlock m_mediaGlyph;
    private TextBlock m_txtMedia;
    private TextBlock m_txtMediaArtist;
    private TextBlock m_txtMediaStatus;
    private Button m_btnMediaPrevious;
    private Button m_btnMediaPlayPause;
    private Button m_btnMediaNext;
    private TextBlock m_btnPin;
    private Ellipse m_statusDot;

    private bool m_isExpanded = false;
    private bool m_isPinned = false;
    private DispatcherTimer m_clockTimer;
    private DispatcherTimer m_statsTimer;
    private DispatcherTimer m_mediaTimer;
    private DispatcherTimer m_browserTimer;
    private bool m_autoHideOnBrowser;
    private bool m_hasMedia;
    private bool m_mediaIsPlaying;
    private bool m_mediaQueryPending;
    private PerformanceCounter m_cpuCounter;

    public IslandWindow() {
        Title = "Arch Panel";
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        Background = Brushes.Transparent;
        FontFamily = new FontFamily("Segoe UI, Cascadia Code, sans-serif");
        WindowStartupLocation = WindowStartupLocation.Manual;
        Top = 2;
        m_autoHideOnBrowser = LoadAutoHidePreference();

        BuildUI();

        Loaded += (s, e) => {
            CenterIsland();
            ReserveTop(38);
            WindowsTaskbarVisibility.HideNativeTaskbar();
        };

        Closing += (s, e) => {
            if (m_browserTimer != null) m_browserTimer.Stop();
            RestoreTop();
        };

        StartTimers();
    }

    private void ReserveTop(int height) {
        try {
            if (!s_hasSaved) {
                SystemParametersInfo(SPI_GETWORKAREA, 0, ref s_origRect, 0);
                s_hasSaved = true;
            }
            RECT r = s_origRect;
            r.Top = height;
            SystemParametersInfo(SPI_SETWORKAREA, 0, ref r, SPIF_SENDCHANGE);
            s_isReserved = true;
        } catch {}
    }

    private void RestoreTop() {
        try {
            if (s_hasSaved && s_isReserved) {
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref s_origRect, SPIF_SENDCHANGE);
                s_isReserved = false;
            }
        } catch {}
    }

    private void CenterIsland() {
        double screenW = SystemParameters.PrimaryScreenWidth;
        if (ActualWidth > 0) {
            Left = Math.Max(10, (screenW - ActualWidth) / 2);
        }
    }

    private void BuildUI() {
        Border container = new Border { Margin = new Thickness(8) };

        m_islandBorder = new Border {
            Background = new SolidColorBrush(Color.FromArgb(242, 16, 16, 21)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(10, 6, 10, 6),
            MinWidth = 220,
            Cursor = Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        DropShadowEffect shadow = new DropShadowEffect {
            Color = Colors.Black,
            BlurRadius = 24,
            ShadowDepth = 3,
            Opacity = 0.65
        };
        m_islandBorder.Effect = shadow;

        Grid mainGrid = new Grid();

        // ================= COLLAPSED PILL =================
        m_pillCollapsed = new StackPanel {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 2, 6, 2)
        };

        m_statusDot = new Ellipse {
            Width = 9,
            Height = 9,
            Fill = new SolidColorBrush(Color.FromRgb(46, 209, 88)),
            Margin = new Thickness(0, 0, 9, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        Border distroTag = new Border {
            Background = new SolidColorBrush(Color.FromArgb(40, 23, 147, 209)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(7, 2, 7, 2),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock {
                Text = "ARCH",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(23, 147, 209))
            }
        };

        m_pillClock = new TextBlock {
            Text = DateTime.Now.ToString("HH:mm"),
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock sep = new TextBlock {
            Text = "·",
            Foreground = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
            FontSize = 11,
            Margin = new Thickness(8, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        m_pillSubtitle = new TextBlock {
            Text = "Arch desktop",
            Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 180)),
            FontSize = 11,
            MaxWidth = 120,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };

        m_pillCollapsed.Children.Add(m_statusDot);
        m_pillCollapsed.Children.Add(distroTag);
        m_pillCollapsed.Children.Add(m_pillClock);
        m_pillCollapsed.Children.Add(sep);
        m_pillCollapsed.Children.Add(m_pillSubtitle);

        // ================= EXPANDED PANEL =================
        m_panelExpanded = new Border {
            Visibility = Visibility.Collapsed,
            Padding = new Thickness(8, 4, 8, 4),
            MinWidth = 410
        };

        Grid expGrid = new Grid();
        expGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        expGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        expGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        expGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Row 0: Header
        DockPanel header = new DockPanel { Margin = new Thickness(4, 2, 4, 8) };
        StackPanel hLeft = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(hLeft, Dock.Left);

        Ellipse hDot = new Ellipse {
            Width = 10,
            Height = 10,
            Fill = new SolidColorBrush(Color.FromRgb(46, 209, 88)),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        Border hTag = new Border {
            Background = new SolidColorBrush(Color.FromRgb(23, 147, 209)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 2, 8, 2),
            Margin = new Thickness(0, 0, 8, 0),
            Child = new TextBlock { Text = "ARCH PANEL", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.White }
        };

        m_expandTime = new TextBlock {
            Text = DateTime.Now.ToString("HH:mm:ss"),
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };

        hLeft.Children.Add(hDot);
        hLeft.Children.Add(hTag);
        hLeft.Children.Add(m_expandTime);

        StackPanel hRight = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(hRight, Dock.Right);

        m_expandDate = new TextBlock {
            Text = DateTime.Now.ToString("ddd, dd MMM yyyy"),
            Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 160)),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        m_btnPin = new TextBlock {
            Text = "PIN",
            Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };
        m_btnPin.MouseDown += (s, e) => {
            m_isPinned = !m_isPinned;
            m_btnPin.Text = m_isPinned ? "PINNED" : "PIN";
            m_btnPin.Foreground = m_isPinned ? new SolidColorBrush(Color.FromRgb(46, 209, 88)) : new SolidColorBrush(Color.FromRgb(89, 199, 239));
        };

        hRight.Children.Add(m_expandDate);
        hRight.Children.Add(m_btnPin);

        header.Children.Add(hLeft);
        header.Children.Add(hRight);
        Grid.SetRow(header, 0);
        expGrid.Children.Add(header);

        // Row 1: System Stats (CPU & RAM)
        Border statsBox = new Border {
            Background = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(2, 0, 2, 8)
        };
        Grid statsGrid = new Grid();
        statsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        statsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // CPU
        StackPanel cpuPanel = new StackPanel { Margin = new Thickness(0, 0, 10, 4) };
        DockPanel cpuDock = new DockPanel();
        TextBlock cpuLbl = new TextBlock { Text = "CPU", Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)), FontSize = 10, FontWeight = FontWeights.SemiBold };
        DockPanel.SetDock(cpuLbl, Dock.Left);
        m_txtCpu = new TextBlock { Text = "0%", Foreground = new SolidColorBrush(Color.FromRgb(23, 147, 209)), FontSize = 10, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
        DockPanel.SetDock(m_txtCpu, Dock.Right);
        cpuDock.Children.Add(cpuLbl);
        cpuDock.Children.Add(m_txtCpu);
        m_barCpu = new ProgressBar { Height = 5, Margin = new Thickness(0, 4, 0, 0), Foreground = new SolidColorBrush(Color.FromRgb(23, 147, 209)), Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), BorderThickness = new Thickness(0), Value = 15, Maximum = 100 };
        cpuPanel.Children.Add(cpuDock);
        cpuPanel.Children.Add(m_barCpu);
        Grid.SetRow(cpuPanel, 0);
        Grid.SetColumn(cpuPanel, 0);
        statsGrid.Children.Add(cpuPanel);

        // RAM
        StackPanel ramPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 4) };
        DockPanel ramDock = new DockPanel();
        TextBlock ramLbl = new TextBlock { Text = "RAM", Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)), FontSize = 10, FontWeight = FontWeights.SemiBold };
        DockPanel.SetDock(ramLbl, Dock.Left);
        m_txtRam = new TextBlock { Text = "0%", Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)), FontSize = 10, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
        DockPanel.SetDock(m_txtRam, Dock.Right);
        ramDock.Children.Add(ramLbl);
        ramDock.Children.Add(m_txtRam);
        m_barRam = new ProgressBar { Height = 5, Margin = new Thickness(0, 4, 0, 0), Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)), Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), BorderThickness = new Thickness(0), Value = 40, Maximum = 100 };
        ramPanel.Children.Add(ramDock);
        ramPanel.Children.Add(m_barRam);
        Grid.SetRow(ramPanel, 0);
        Grid.SetColumn(ramPanel, 1);
        statsGrid.Children.Add(ramPanel);

        // Now playing card (shown only while Windows reports a media session).
        m_mediaCard = new Border {
            Background = new SolidColorBrush(Color.FromArgb(34, 23, 147, 209)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(64, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8),
            Margin = new Thickness(2, 6, 2, 0),
            Visibility = Visibility.Collapsed
        };
        Grid mediaGrid = new Grid();
        mediaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
        mediaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mediaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid artworkGrid = new Grid();
        m_mediaArtwork = new Image { Stretch = Stretch.UniformToFill, Visibility = Visibility.Collapsed };
        m_mediaGlyph = new TextBlock {
            Text = "\u266B",
            FontFamily = new FontFamily("Segoe UI Symbol"),
            FontSize = 24,
            Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        artworkGrid.Children.Add(m_mediaArtwork);
        artworkGrid.Children.Add(m_mediaGlyph);
        Border artwork = new Border {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(9),
            Background = new SolidColorBrush(Color.FromRgb(18, 43, 61)),
            ClipToBounds = true,
            Child = artworkGrid
        };
        Grid.SetColumn(artwork, 0);
        mediaGrid.Children.Add(artwork);

        StackPanel mediaInfo = new StackPanel {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 8, 0)
        };
        m_txtMediaStatus = new TextBlock {
            Text = "NOW PLAYING",
            Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
            FontSize = 8,
            FontWeight = FontWeights.Bold
        };
        m_txtMedia = new TextBlock {
            Text = "",
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 220
        };
        m_txtMediaArtist = new TextBlock {
            Text = "",
            Foreground = new SolidColorBrush(Color.FromRgb(175, 185, 195)),
            FontSize = 10,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 220
        };
        mediaInfo.Children.Add(m_txtMediaStatus);
        mediaInfo.Children.Add(m_txtMedia);
        mediaInfo.Children.Add(m_txtMediaArtist);
        Grid.SetColumn(mediaInfo, 1);
        mediaGrid.Children.Add(mediaInfo);

        StackPanel mediaControls = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        m_btnMediaPrevious = CreateMediaControlButton("\u23EE", "Previous track");
        m_btnMediaPlayPause = CreateMediaControlButton("\u25B6", "Play or pause");
        m_btnMediaNext = CreateMediaControlButton("\u23ED", "Next track");
        m_btnMediaPrevious.Click += (s, e) => { Task.Run(() => NowPlayingProvider.SkipPrevious()); };
        m_btnMediaPlayPause.Click += (s, e) => { Task.Run(() => NowPlayingProvider.TogglePlayPause()); };
        m_btnMediaNext.Click += (s, e) => { Task.Run(() => NowPlayingProvider.SkipNext()); };
        mediaControls.Children.Add(m_btnMediaPrevious);
        mediaControls.Children.Add(m_btnMediaPlayPause);
        mediaControls.Children.Add(m_btnMediaNext);
        Grid.SetColumn(mediaControls, 2);
        mediaGrid.Children.Add(mediaControls);

        m_mediaCard.Child = mediaGrid;
        Grid.SetRow(m_mediaCard, 1);
        Grid.SetColumnSpan(m_mediaCard, 2);
        statsGrid.Children.Add(m_mediaCard);

        statsBox.Child = statsGrid;
        Grid.SetRow(statsBox, 1);
        expGrid.Children.Add(statsBox);

        // Row 2: Action Buttons
        UniformGrid actionsGrid = new UniformGrid { Columns = 5, Margin = new Thickness(2, 0, 2, 6) };
        actionsGrid.Children.Add(CreateActionButton("TERM", "Terminal", Color.FromRgb(23, 147, 209), () => LaunchApp("wt.exe", "powershell.exe")));
        actionsGrid.Children.Add(CreateActionButton("FILES", "Files", Color.FromRgb(52, 152, 219), () => LaunchApp("explorer.exe", "")));
        actionsGrid.Children.Add(CreateActionButton("WEB", "Browser", Color.FromRgb(100, 205, 245), () => LaunchApp("msedge.exe", "https://www.google.com")));
        actionsGrid.Children.Add(CreateActionButton("LOCK", "Lock", Color.FromRgb(23, 147, 209), () => LaunchLockScreen()));
        actionsGrid.Children.Add(CreateActionButton("PWR", "Power", Color.FromRgb(231, 76, 60), () => ShowPowerMenu()));

        Grid.SetRow(actionsGrid, 2);
        expGrid.Children.Add(actionsGrid);

        // Row 3: Drag Hint
        TextBlock hint = new TextBlock {
            Text = "Drag to move | Right-click for options",
            Foreground = new SolidColorBrush(Color.FromRgb(90, 90, 110)),
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        Grid.SetRow(hint, 3);
        expGrid.Children.Add(hint);

        m_panelExpanded.Child = expGrid;

        mainGrid.Children.Add(m_pillCollapsed);
        mainGrid.Children.Add(m_panelExpanded);

        m_islandBorder.Child = mainGrid;
        container.Child = m_islandBorder;
        Content = container;

        // ================= EVENTS =================
        m_islandBorder.MouseLeftButtonDown += (s, e) => {
            if (e.ButtonState == MouseButtonState.Pressed && !IsInteractiveSource(e.OriginalSource as DependencyObject)) {
                DragMove();
            }
        };

        m_islandBorder.MouseEnter += (s, e) => {
            if (!m_isExpanded) {
                Expand();
            }
        };

        m_islandBorder.MouseLeave += (s, e) => {
            if (m_isExpanded && !m_isPinned && !m_mediaIsPlaying) {
                Collapse();
            }
        };

        SetupContextMenu();
    }

    private static bool LoadAutoHidePreference() {
        try {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\ArchPanel")) {
                object value = key == null ? 1 : key.GetValue("AutoHideOnBrowser", 1);
                return Convert.ToInt32(value) != 0;
            }
        } catch {
            return false;
        }
    }

    private void SaveAutoHidePreference() {
        try {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\ArchPanel")) {
                if (key != null) key.SetValue("AutoHideOnBrowser", m_autoHideOnBrowser ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        } catch { }
    }

    private void UpdateBrowserAutoHide() {
        if (!m_autoHideOnBrowser) {
            if (Visibility != System.Windows.Visibility.Visible) {
                Visibility = System.Windows.Visibility.Visible;
                ReserveTop(38);
            }
            return;
        }

        string processName = null;
        try {
            IntPtr foreground = GetForegroundWindow();
            uint processId;
            GetWindowThreadProcessId(foreground, out processId);
            if (processId != 0) {
                using (Process process = Process.GetProcessById((int)processId)) {
                    processName = process.ProcessName;
                }
            }
        } catch { }

        bool shouldHide = !m_mediaIsPlaying && BrowserVisibilityPolicy.ShouldHide(true, processName);
        if (shouldHide) {
            if (m_isExpanded) Collapse();
            if (Visibility != System.Windows.Visibility.Hidden) {
                RestoreTop();
                Visibility = System.Windows.Visibility.Hidden;
            }
        } else if (Visibility != System.Windows.Visibility.Visible) {
            Visibility = System.Windows.Visibility.Visible;
            ReserveTop(38);
        }
    }

    private void SetBrowserAutoHide(bool enabled) {
        m_autoHideOnBrowser = enabled;
        SaveAutoHidePreference();
        UpdateBrowserAutoHide();
    }

    private void UpdateNowPlaying(NowPlayingInfo info) {
        bool wasPlaying = m_mediaIsPlaying;
        bool show = info != null && MediaVisibilityPolicy.ShouldShow(info.PlaybackStatus, info.Title);
        m_hasMedia = show;
        m_mediaIsPlaying = show && info.IsPlaying;
        if (m_mediaIsPlaying && !wasPlaying && !m_isExpanded) Expand();
        if (wasPlaying && !m_mediaIsPlaying && m_isExpanded && !m_isPinned && !m_islandBorder.IsMouseOver) Collapse();

        m_mediaCard.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        if (!show) {
            SetMediaArtwork(null);
            m_txtMedia.Text = String.Empty;
            m_txtMediaArtist.Text = String.Empty;
            m_txtMediaStatus.Text = "NOW PLAYING";
            m_pillSubtitle.Text = "CPU " + m_txtCpu.Text;
            m_statusDot.Fill = new SolidColorBrush(Color.FromRgb(52, 152, 219));
            return;
        }

        string artist = info.Artist;
        if (String.IsNullOrWhiteSpace(artist)) artist = info.Album;
        if (String.IsNullOrWhiteSpace(artist)) artist = info.SourceApp;
        if (String.IsNullOrWhiteSpace(artist)) artist = "Media player";

        m_txtMedia.Text = info.Title;
        m_txtMediaArtist.Text = artist;
        m_txtMediaStatus.Text = info.IsPlaying ? "NOW PLAYING" : "PAUSED";
        m_btnMediaPlayPause.Content = info.IsPlaying ? "\u23F8" : "\u25B6";
        m_btnMediaPlayPause.ToolTip = info.IsPlaying ? "Pause" : "Resume";
        m_pillSubtitle.Text = "\u266B " + info.Title;
        m_statusDot.Fill = new SolidColorBrush(info.IsPlaying ? Color.FromRgb(46, 209, 88) : Color.FromRgb(89, 199, 239));
        SetMediaArtwork(info.ThumbnailBytes);
    }

    private void SetMediaArtwork(byte[] bytes) {
        if (bytes == null || bytes.Length == 0) {
            m_mediaArtwork.Source = null;
            m_mediaArtwork.Visibility = Visibility.Collapsed;
            m_mediaGlyph.Visibility = Visibility.Visible;
            return;
        }

        try {
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(bytes)) {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            m_mediaArtwork.Source = bitmap;
            m_mediaArtwork.Visibility = Visibility.Visible;
            m_mediaGlyph.Visibility = Visibility.Collapsed;
        } catch {
            m_mediaArtwork.Source = null;
            m_mediaArtwork.Visibility = Visibility.Collapsed;
            m_mediaGlyph.Visibility = Visibility.Visible;
        }
    }

    private void RequestMediaUpdate() {
        if (m_mediaQueryPending) return;
        m_mediaQueryPending = true;
        Task.Run(() => NowPlayingProvider.GetCurrent()).ContinueWith(mediaTask => {
            try {
                Dispatcher.BeginInvoke(new Action(() => {
                    m_mediaQueryPending = false;
                    UpdateNowPlaying(mediaTask.Status == TaskStatus.RanToCompletion ? mediaTask.Result : null);
                }), DispatcherPriority.Background);
            } catch { }
        });
    }

    private bool IsInteractiveSource(DependencyObject source) {
        while (source != null && source != m_islandBorder) {
            if (source is Button || ReferenceEquals(source, m_btnPin)) return true;
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }

    private Button CreateMediaControlButton(string glyph, string tooltip) {
        return new Button {
            Content = new TextBlock {
                Text = glyph,
                FontFamily = new FontFamily("Segoe UI Symbol"),
                FontSize = 14,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            },
            Width = 29,
            Height = 28,
            Margin = new Thickness(2, 0, 2, 0),
            Padding = new Thickness(0),
            Background = new SolidColorBrush(Color.FromArgb(48, 23, 147, 209)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            ToolTip = tooltip
        };
    }

    private Button CreateActionButton(string iconText, string label, Color color, Action onClick) {
        Button btn = new Button {
            Margin = new Thickness(2, 0, 2, 0),
            Padding = new Thickness(0, 6, 0, 6),
            Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand
        };

        StackPanel p = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        TextBlock icon = new TextBlock { Text = iconText, FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(color), HorizontalAlignment = HorizontalAlignment.Center };
        TextBlock lbl = new TextBlock { Text = label, FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) };
        p.Children.Add(icon);
        p.Children.Add(lbl);
        btn.Content = p;

        btn.Click += (s, e) => onClick();
        return btn;
    }

    private void Expand() {
        m_isExpanded = true;
        m_pillCollapsed.Visibility = Visibility.Collapsed;
        m_panelExpanded.Visibility = Visibility.Visible;
        m_islandBorder.CornerRadius = new CornerRadius(20);
        Dispatcher.BeginInvoke(new Action(CenterIsland), DispatcherPriority.Loaded);
    }

    private void Collapse() {
        m_isExpanded = false;
        m_panelExpanded.Visibility = Visibility.Collapsed;
        m_pillCollapsed.Visibility = Visibility.Visible;
        m_islandBorder.CornerRadius = new CornerRadius(20);
        Dispatcher.BeginInvoke(new Action(CenterIsland), DispatcherPriority.Loaded);
    }

    private void StartTimers() {
        // Clock
        m_clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        m_clockTimer.Tick += (s, e) => {
            DateTime now = DateTime.Now;
            m_pillClock.Text = now.ToString("HH:mm");
            m_expandTime.Text = now.ToString("HH:mm:ss");
            m_expandDate.Text = now.ToString("ddd, dd MMM yyyy");
        };
        m_clockTimer.Start();

        // CPU & RAM
        try {
            m_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            m_cpuCounter.NextValue();
        } catch {}

        m_statsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        m_statsTimer.Tick += (s, e) => {
            try {
                if (m_cpuCounter != null) {
                    int cpu = (int)Math.Min(100, Math.Max(0, m_cpuCounter.NextValue()));
                    m_txtCpu.Text = cpu + "%";
                    m_barCpu.Value = cpu;
                    if (!m_hasMedia) {
                        m_pillSubtitle.Text = "CPU " + cpu + "%";
                    }
                }
            } catch {}

            try {
                int ram = ReadPhysicalMemoryPercent();
                if (ram >= 0) {
                    m_txtRam.Text = ram + "%";
                    m_barRam.Value = ram;
                } else {
                    m_txtRam.Text = "--";
                }
            } catch {
                m_txtRam.Text = "--";
            }
        };
        m_statsTimer.Start();

        // Now-playing metadata from Windows media sessions (music and browser video).
        m_mediaTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        m_mediaTimer.Tick += (s, e) => RequestMediaUpdate();
        m_mediaTimer.Start();

        m_browserTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        m_browserTimer.Tick += (s, e) => UpdateBrowserAutoHide();
        m_browserTimer.Start();
    }

    private void LaunchApp(string main, string fallback) {
        try {
            Process.Start(main);
        } catch {
            if (!string.IsNullOrEmpty(fallback)) {
                try { Process.Start(fallback); } catch {}
            }
        }
    }

    private void LaunchLockScreen() {
        string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string ps1 = System.IO.Path.Combine(dir, "linux_lockscreen.ps1");
        if (File.Exists(ps1)) {
            Process.Start("powershell.exe", "-NoProfile -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File \"" + ps1 + "\"");
        } else {
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }
    }

    private void ShowPowerMenu() {
        ContextMenu menu = new ContextMenu {
            Background = new SolidColorBrush(Color.FromArgb(232, 16, 16, 24)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
        };

        MenuItem miLock = new MenuItem { Header = "Linux UI Preview (not a lock)" };
        miLock.Click += (s, e) => LaunchLockScreen();
        menu.Items.Add(miLock);

        MenuItem miSleep = new MenuItem { Header = "Sleep" };
        miSleep.Click += (s, e) => Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
        menu.Items.Add(miSleep);

        MenuItem miRestart = new MenuItem { Header = "Restart PC" };
        miRestart.Click += (s, e) => Process.Start("shutdown", "/r /t 5");
        menu.Items.Add(miRestart);

        MenuItem miShut = new MenuItem { Header = "Power Off PC", Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)) };
        miShut.Click += (s, e) => Process.Start("shutdown", "/s /t 5");
        menu.Items.Add(miShut);

        menu.Items.Add(new Separator());

        MenuItem miExit = new MenuItem { Header = "Exit Dynamic Island", Foreground = new SolidColorBrush(Color.FromRgb(23, 147, 209)) };
        miExit.Click += (s, e) => {
            RestoreTop();
            Close();
        };
        menu.Items.Add(miExit);

        menu.IsOpen = true;
    }

    private void SetupContextMenu() {
        ContextMenu ctx = new ContextMenu {
            Background = new SolidColorBrush(Color.FromArgb(232, 16, 16, 24)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
        };

        MenuItem miCenter = new MenuItem { Header = "Center on Screen" };
        miCenter.Click += (s, e) => {
            Top = 2;
            CenterIsland();
        };
        ctx.Items.Add(miCenter);

        MenuItem miTopmost = new MenuItem { Header = "Always on Top: ON" };
        miTopmost.Click += (s, e) => {
            Topmost = !Topmost;
            miTopmost.Header = Topmost ? "Always on Top: ON" : "Always on Top: OFF";
        };
        ctx.Items.Add(miTopmost);

        MenuItem miAutoHide = new MenuItem {
            Header = "Sembunyikan saat browser aktif",
            ToolTip = "Sembunyikan panel saat Chrome, Edge, Firefox, atau browser lain sedang digunakan.",
            IsCheckable = true,
            IsChecked = m_autoHideOnBrowser
        };
        miAutoHide.Click += (s, e) => SetBrowserAutoHide(miAutoHide.IsChecked);
        ctx.Items.Add(miAutoHide);

        MenuItem miNoCover = new MenuItem { Header = "No-Cover Mode (Reserve Top): ON" };
        miNoCover.Click += (s, e) => {
            if (s_isReserved) {
                RestoreTop();
                miNoCover.Header = "No-Cover Mode (Reserve Top): OFF";
            } else {
                ReserveTop(38);
                miNoCover.Header = "No-Cover Mode (Reserve Top): ON";
            }
        };
        ctx.Items.Add(miNoCover);

        ctx.Items.Add(new Separator());

        MenuItem miLock = new MenuItem { Header = "Pratinjau Linux (bukan pengunci)" };
        miLock.Click += (s, e) => LaunchLockScreen();
        ctx.Items.Add(miLock);

        MenuItem miExit = new MenuItem { Header = "Exit Dynamic Island", Foreground = new SolidColorBrush(Color.FromRgb(23, 147, 209)) };
        miExit.Click += (s, e) => {
            RestoreTop();
            Close();
        };
        ctx.Items.Add(miExit);

        m_islandBorder.ContextMenu = ctx;
    }
}
