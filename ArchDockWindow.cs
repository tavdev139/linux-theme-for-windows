using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

public sealed class ArchDockWindow : Window {
    private Border m_dockBorder;
    private TextBlock m_clock;
    private TextBlock m_date;
    private DispatcherTimer m_timer;
    private DispatcherTimer m_visibilityTimer;
    private DateTime m_lastRevealAt = DateTime.MinValue;
    private DateTime m_lastTaskbarHideAt = DateTime.MinValue;
    private StackPanel m_openAppsGroup;
    private Border m_networkButton;
    private DispatcherTimer m_openAppsTimer;
    private List<OpenAppInfo> m_openApps = new List<OpenAppInfo>();

    public int LauncherCount { get; private set; }
    public bool AutoHideEnabled { get; private set; }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X; public int Y; }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int maxCount);
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    public ArchDockWindow() {
        Title = "Arch Dock";
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        ResizeMode = ResizeMode.NoResize;
        FontFamily = new FontFamily("Segoe UI");
        WindowStartupLocation = WindowStartupLocation.Manual;

        AutoHideEnabled = LoadAutoHideDock();
        BuildDock();
        Loaded += (s, e) => { CenterAtBottom(); UpdateVisibility(); };
        m_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        m_timer.Tick += (s, e) => UpdateClock();
        m_timer.Start();
        m_visibilityTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        m_visibilityTimer.Tick += (s, e) => UpdateVisibility();
        m_visibilityTimer.Start();
        m_openAppsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        m_openAppsTimer.Tick += (s, e) => RefreshOpenApps();
        m_openAppsTimer.Start();
        UpdateClock();
        Closed += (s, e) => {
            if (m_timer != null) m_timer.Stop();
            if (m_visibilityTimer != null) m_visibilityTimer.Stop();
        };
    }

    private void BuildDock() {
        m_dockBorder = new Border {
            Margin = new Thickness(8),
            Padding = new Thickness(10, 6, 10, 6),
            CornerRadius = new CornerRadius(14),
            Background = new SolidColorBrush(Color.FromArgb(150, 12, 16, 20)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 3,
                Opacity = 0.45
            }
        };

        DockPanel root = new DockPanel { LastChildFill = false };

        StackPanel brand = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        };
        Border brandMark = new Border {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(45, 23, 147, 209)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(125, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock {
                Text = "A",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        TextBlock brandText = new TextBlock {
            Text = "ARCH",
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 175, 190)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0)
        };
        brand.Children.Add(brandMark);
        brand.Children.Add(brandText);
        DockPanel.SetDock(brand, Dock.Left);
        root.Children.Add(brand);

        Border leftSeparator = CreateSeparator();
        DockPanel.SetDock(leftSeparator, Dock.Left);
        root.Children.Add(leftSeparator);

        StackPanel appGroup = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0)
        };
        DockLauncher[] launchers = ArchDockCatalog.GetItems();
        LauncherCount = launchers.Length;
        foreach (DockLauncher launcher in launchers) appGroup.Children.Add(CreateLauncherButton(launcher));
        Border searchButton = CreateSearchButton();
        appGroup.Children.Add(searchButton);
        Border networkButton = CreateNetworkButton();
        appGroup.Children.Add(networkButton);
        m_networkButton = networkButton;
        root.Children.Add(appGroup);

        StackPanel openAppsGroup = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0)
        };
        m_openAppsGroup = openAppsGroup;
        root.Children.Add(openAppsGroup);

        Border rightSeparator = CreateSeparator();
        DockPanel.SetDock(rightSeparator, Dock.Right);
        root.Children.Add(rightSeparator);

        StackPanel clockPanel = new StackPanel {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(14, 0, 4, 0),
            MinWidth = 70
        };
        m_clock = new TextBlock {
            Text = "00:00",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        m_date = new TextBlock {
            Text = "Mon, 01 Jan",
            Foreground = new SolidColorBrush(Color.FromRgb(170, 180, 195)),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        clockPanel.Children.Add(m_clock);
        clockPanel.Children.Add(m_date);
        DockPanel.SetDock(clockPanel, Dock.Right);
        root.Children.Add(clockPanel);

        m_dockBorder.Child = root;
        Content = m_dockBorder;
        MouseLeftButtonDown += (s, e) => {
            if (!IsLauncherSource(e.OriginalSource as DependencyObject)) {
                try { DragMove(); } catch { }
            }
        };
        ContextMenu = BuildContextMenu();
    }

    private bool IsLauncherSource(DependencyObject source) {
        while (source != null && source != m_dockBorder) {
            if (source is Button) return true;
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }

    private Border CreateSeparator() {
        return new Border {
            Width = 1,
            Height = 30,
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0)
        };
    }

    private Button CreateLauncherButton(DockLauncher launcher) {
        Color accent = (Color)ColorConverter.ConvertFromString(launcher.AccentColor);
        Brush accentBrush = new SolidColorBrush(accent);
        Button button = new Button {
            Width = 46,
            Height = 46,
            Margin = new Thickness(3, 0, 3, 0),
            Padding = new Thickness(2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            ToolTip = launcher.Label
        };
        StackPanel content = new StackPanel {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Border icon = new Border {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(7),
            Background = new SolidColorBrush(Color.FromArgb(30, accent.R, accent.G, accent.B)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(40, accent.R, accent.G, accent.B)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock {
                Text = launcher.Glyph,
                Foreground = accentBrush,
                FontSize = launcher.Glyph.Length > 3 ? 7 : 10,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        TextBlock label = new TextBlock {
            Text = launcher.Label,
            Foreground = new SolidColorBrush(Color.FromRgb(170, 180, 195)),
            FontSize = 7,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 44
        };
        content.Children.Add(icon);
        content.Children.Add(label);
        button.Content = content;
        button.MouseEnter += (s, e) => {
            button.Background = new SolidColorBrush(Color.FromArgb(40, 23, 147, 209));
            button.RenderTransform = new ScaleTransform(1.04, 1.04);
        };
        button.MouseLeave += (s, e) => {
            button.Background = Brushes.Transparent;
            button.RenderTransform = Transform.Identity;
        };
        button.Click += (s, e) => Launch(launcher);
        return button;
    }

    private Border CreateSearchButton() {
        Button button = new Button {
            Width = 150,
            Height = 36,
            Margin = new Thickness(6, 0, 6, 0),
            Padding = new Thickness(10, 0, 8, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            ToolTip = "Cari aplikasi"
        };
        StackPanel content = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        TextBlock icon = new TextBlock {
            Text = "🔍",
            FontSize = 14,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        TextBlock label = new TextBlock {
            Text = "Cari aplikasi",
            Foreground = Brushes.White,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        content.Children.Add(icon);
        content.Children.Add(label);
        button.Content = content;
        Border searchBorder = new Border {
            CornerRadius = new CornerRadius(18),
            Background = new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Child = button
        };
        button.MouseEnter += (s, e) => {
            searchBorder.Background = new SolidColorBrush(Color.FromArgb(130, 23, 147, 209));
            searchBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(200, 23, 147, 209));
        };
        button.MouseLeave += (s, e) => {
            searchBorder.Background = new SolidColorBrush(Color.FromArgb(90, 0, 0, 0));
            searchBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));
        };
        button.Click += (s, e) => {
            try {
                ArchSearchWindow search = new ArchSearchWindow();
                search.Owner = this;
                search.ShowDialog();
            } catch { }
        };
        return searchBorder;
    }

    private Border CreateNetworkButton() {
        Button button = new Button {
            Width = 110,
            Height = 36,
            Margin = new Thickness(6, 0, 6, 0),
            Padding = new Thickness(10, 0, 8, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            ToolTip = "Jaringan Wi-Fi"
        };
        StackPanel content = new StackPanel {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        TextBlock icon = new TextBlock {
            Text = "📶",
            FontSize = 14,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        TextBlock label = new TextBlock {
            Text = "Wi-Fi",
            Foreground = Brushes.White,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        content.Children.Add(icon);
        content.Children.Add(label);
        button.Content = content;
        Border netBorder = new Border {
            CornerRadius = new CornerRadius(18),
            Background = new SolidColorBrush(Color.FromArgb(90, 46, 209, 88)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(90, 46, 209, 88)),
            BorderThickness = new Thickness(1),
            Child = button
        };
        button.MouseEnter += (s, e) => {
            netBorder.Background = new SolidColorBrush(Color.FromArgb(130, 46, 209, 88));
        };
        button.MouseLeave += (s, e) => {
            netBorder.Background = new SolidColorBrush(Color.FromArgb(90, 46, 209, 88));
        };
        button.Click += (s, e) => {
            try {
                ArchNetworkWindow net = new ArchNetworkWindow();
                net.Owner = this;
                net.ShowDialog();
            } catch { }
        };
        return netBorder;
    }

    private static void Launch(DockLauncher launcher) {
        try {
            Process.Start(new ProcessStartInfo {
                FileName = launcher.Target,
                Arguments = launcher.Arguments,
                UseShellExecute = true
            });
        } catch { }
    }

    private ContextMenu BuildContextMenu() {
        ContextMenu menu = new ContextMenu {
            Background = new SolidColorBrush(Color.FromArgb(245, 16, 23, 33)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(90, 23, 147, 209))
        };
        MenuItem center = new MenuItem { Header = "Center Dock" };
        center.Click += (s, e) => CenterAtBottom();
        menu.Items.Add(center);
        MenuItem autoHide = new MenuItem {
            Header = "Auto-hide when an app is open",
            IsCheckable = true,
            IsChecked = AutoHideEnabled
        };
        autoHide.Click += (s, e) => {
            AutoHideEnabled = autoHide.IsChecked;
            SaveAutoHideDock(AutoHideEnabled);
            UpdateVisibility();
        };
        menu.Items.Add(autoHide);
        MenuItem exit = new MenuItem { Header = "Exit Arch widgets" };
        exit.Click += (s, e) => Application.Current.Shutdown();
        menu.Items.Add(exit);
        return menu;
    }

    private static bool LoadAutoHideDock() {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\ArchPanel", false)) {
                object value = key == null ? null : key.GetValue("AutoHideDock", null);
                if (value != null) return Convert.ToInt32(value) != 0;
            }
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ArchPanel")) {
                key.SetValue("AutoHideDock", 1, RegistryValueKind.DWord);
            }
        } catch { }
        return true;
    }

    private static void SaveAutoHideDock(bool enabled) {
        try {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ArchPanel")) {
                key.SetValue("AutoHideDock", enabled ? 1 : 0, RegistryValueKind.DWord);
            }
        } catch { }
    }

    private void UpdateVisibility() {
        if (!IsLoaded) return;
        DateTime now = DateTime.UtcNow;
        if ((now - m_lastTaskbarHideAt).TotalSeconds >= 1) {
            WindowsTaskbarVisibility.HideNativeTaskbar();
            m_lastTaskbarHideAt = now;
        }
        NativePoint point;
        bool atRevealEdge = GetCursorPos(out point) && point.Y >= GetSystemMetrics(1) - 3;
        if (atRevealEdge) m_lastRevealAt = now;
        bool revealGrace = (now - m_lastRevealAt).TotalMilliseconds < 900;

        IntPtr window = GetForegroundWindow();
        uint processId;
        GetWindowThreadProcessId(window, out processId);
        string processName = String.Empty;
        if (processId != 0) {
            try { processName = Process.GetProcessById((int)processId).ProcessName; } catch { }
        }
        StringBuilder title = new StringBuilder(256);
        GetWindowText(window, title, title.Capacity);

        bool show = ArchDockVisibilityPolicy.ShouldShow(
            AutoHideEnabled, processName, title.ToString(), atRevealEdge || revealGrace, IsMouseOver);
        if (show) {
            if (Visibility != Visibility.Visible) {
                Show();
                CenterAtBottom();
            }
        } else if (Visibility == Visibility.Visible) {
            Hide();
        }
    }

    private void UpdateClock() {
        DateTime now = DateTime.Now;
        m_clock.Text = now.ToString("HH:mm");
        m_date.Text = now.ToString("ddd, dd MMM");
    }

    private void CenterAtBottom() {
        double width = SystemParameters.PrimaryScreenWidth;
        double height = SystemParameters.PrimaryScreenHeight;
        Left = Math.Max(8, (width - ActualWidth) / 2);
        Top = Math.Max(8, height - ActualHeight - 14);
    }

    private void RefreshOpenApps() {
        List<OpenAppInfo> current = EnumerateOpenWindows();
        bool changed = current.Count != m_openApps.Count;
        if (!changed) {
            for (int i = 0; i < current.Count; i++) {
                if (current[i].Title != m_openApps[i].Title || current[i].HWnd != m_openApps[i].HWnd) {
                    changed = true;
                    break;
                }
            }
        }
        if (!changed) return;
        m_openApps = current;
        RebuildOpenAppsButtons();
    }

    private List<OpenAppInfo> EnumerateOpenWindows() {
        List<OpenAppInfo> result = new List<OpenAppInfo>();
        try {
            EnumWindows((hWnd, lParam) => {
                try {
                    if (!IsWindowVisible(hWnd)) return true;
                    if (IsIconic(hWnd)) return true;
                    int len = GetWindowTextLength(hWnd);
                    if (len == 0) return true;
                    StringBuilder sb = new StringBuilder(len + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    string title = sb.ToString();
                    if (String.IsNullOrWhiteSpace(title)) return true;
                    StringBuilder cls = new StringBuilder(256);
                    GetClassName(hWnd, cls, cls.Capacity);
                    string className = cls.ToString();
                    if (String.Equals(className, "Shell_TrayWnd", StringComparison.OrdinalIgnoreCase)) return true;
                    if (String.Equals(className, "Progman", StringComparison.OrdinalIgnoreCase)) return true;
                    if (String.Equals(className, "WorkerW", StringComparison.OrdinalIgnoreCase)) return true;
                    uint pid;
                    GetWindowThreadProcessId(hWnd, out pid);
                    string processName = String.Empty;
                    try { processName = Process.GetProcessById((int)pid).ProcessName; } catch { }
                    if (String.Equals(processName, "ArchPanel", StringComparison.OrdinalIgnoreCase)) return true;
                    if (String.Equals(processName, "cua-driver", StringComparison.OrdinalIgnoreCase)) return true;
                    if (String.Equals(processName, "TextInputHost", StringComparison.OrdinalIgnoreCase)) return true;
                    result.Add(new OpenAppInfo { HWnd = hWnd, Title = title, ProcessName = processName });
                } catch { }
                return true;
            }, IntPtr.Zero);
        } catch { }
        return result;
    }

    private void RebuildOpenAppsButtons() {
        m_openAppsGroup.Children.Clear();
        foreach (OpenAppInfo app in m_openApps) {
            m_openAppsGroup.Children.Add(CreateOpenAppButton(app));
        }
    }

    private Button CreateOpenAppButton(OpenAppInfo app) {
        Button button = new Button {
            Width = 44,
            Height = 44,
            Margin = new Thickness(3, 0, 3, 0),
            Padding = new Thickness(2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.Transparent,
            Cursor = Cursors.Hand,
            ToolTip = app.Title
        };
        StackPanel content = new StackPanel {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Border icon = new Border {
            Width = 24,
            Height = 24,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromArgb(30, 23, 147, 209)),
            Child = new TextBlock {
                Text = app.Title.Length > 0 ? app.Title[0].ToString().ToUpper() : "?",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        TextBlock label = new TextBlock {
            Text = app.Title.Length > 9 ? app.Title.Substring(0, 9) + "…" : app.Title,
            Foreground = new SolidColorBrush(Color.FromRgb(170, 180, 195)),
            FontSize = 6.5,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 40
        };
        content.Children.Add(icon);
        content.Children.Add(label);
        button.Content = content;
        button.MouseEnter += (s, e) => {
            button.Background = new SolidColorBrush(Color.FromArgb(40, 23, 147, 209));
            button.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 23, 147, 209));
            button.RenderTransform = new ScaleTransform(1.03, 1.03);
        };
        button.MouseLeave += (s, e) => {
            button.Background = Brushes.Transparent;
            button.BorderBrush = Brushes.Transparent;
            button.RenderTransform = Transform.Identity;
        };
        button.Click += (s, e) => ActivateWindow(app.HWnd);
        return button;
    }

    private void ActivateWindow(IntPtr hWnd) {
        try {
            if (IsIconic(hWnd)) ShowWindow(hWnd, 9); // SW_RESTORE
            SetForegroundWindow(hWnd);
            ShowWindow(hWnd, 9); // SW_RESTORE
        } catch { }
    }
}

public class OpenAppInfo {
    public IntPtr HWnd { get; set; }
    public string Title { get; set; }
    public string ProcessName { get; set; }
}

public class ArchSearchWindow : Window {
    private TextBox m_searchBox;
    private ListBox m_results;

    public ArchSearchWindow() {
        Title = "Cari Aplikasi";
        Width = 420;
        Height = 420;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = true;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FontFamily = new FontFamily("Segoe UI");

        Border root = new Border {
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromArgb(240, 15, 21, 30)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(140, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect {
                Color = Colors.Black,
                BlurRadius = 30,
                ShadowDepth = 6,
                Opacity = 0.6
            },
            Padding = new Thickness(14)
        };

        DockPanel panel = new DockPanel { LastChildFill = true };

        TextBlock header = new TextBlock {
            Text = "CARI APLIKASI",
            Foreground = new SolidColorBrush(Color.FromRgb(89, 199, 239)),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);

        Border inputBorder = new Border {
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 0, 8, 0),
            Margin = new Thickness(0, 0, 0, 10)
        };
        m_searchBox = new TextBox {
            Text = "",
            Foreground = Brushes.White,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontSize = 13,
            Padding = new Thickness(0)
        };
        m_searchBox.TextChanged += (s, e) => RefreshResults();
        inputBorder.Child = m_searchBox;
        DockPanel.SetDock(inputBorder, Dock.Top);
        panel.Children.Add(inputBorder);

        m_results = new ListBox {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontSize = 12,
            DisplayMemberPath = "Name",
            ItemContainerStyle = new Style(typeof(ListBoxItem)) {
                Setters = {
                    new Setter(Control.BackgroundProperty, Brushes.Transparent),
                    new Setter(Control.BorderThicknessProperty, new Thickness(0)),
                    new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)),
                    new Setter(Control.ForegroundProperty, Brushes.White)
                }
            }
        };
        m_results.SelectionChanged += (s, e) => {
            object item = m_results.SelectedItem;
            if (item is AppSearchEntry) {
                AppSearchEntry entry = (AppSearchEntry)item;
                LaunchEntry(entry);
                m_results.SelectedIndex = -1;
            }
        };
        panel.Children.Add(m_results);

        root.Child = panel;
        Content = root;

        KeyDown += (s, e) => {
            if (e.Key == Key.Escape) Close();
        };

        Loaded += (s, e) => {
            m_searchBox.Focus();
            RefreshResults();
        };
    }

    private void RefreshResults() {
        AppSearchEntry[] all = ArchAppSearchCatalog.All();
        AppSearchEntry[] filtered = ArchAppSearchCatalog.Filter(all, m_searchBox.Text);
        m_results.ItemsSource = filtered;
    }

    private static void LaunchEntry(AppSearchEntry entry) {
        try {
            Process.Start(new ProcessStartInfo {
                FileName = entry.Target,
                Arguments = entry.Arguments,
                UseShellExecute = true
            });
        } catch { }
    }
}
