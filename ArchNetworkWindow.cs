using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public class ArchNetworkWindow : Window {
    private ListBox m_networks;
    private TextBlock m_status;
    private Button m_refresh;
    private Button m_toggleWifi;
    private Border m_wifiCard;

    public ArchNetworkWindow() {
        Title = "Jaringan WiFi";
        Width = 460;
        Height = 420;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = true;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = new FontFamily("Segoe UI");

        Border root = new Border {
            CornerRadius = new CornerRadius(20),
            Background = new SolidColorBrush(Color.FromArgb(245, 18, 22, 28)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 23, 147, 209)),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect {
                Color = Colors.Black,
                BlurRadius = 40,
                ShadowDepth = 8,
                Opacity = 0.7
            },
            Padding = new Thickness(16)
        };

        StackPanel panel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

        // Header
        Border headerBar = new Border {
            Background = new SolidColorBrush(Color.FromArgb(60, 23, 147, 209)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 14)
        };
        StackPanel headerRow = new StackPanel { Orientation = Orientation.Horizontal };
        TextBlock wifiIcon = new TextBlock {
            Text = "📶",
            FontSize = 20,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        TextBlock headerTitle = new TextBlock {
            Text = "Wi-Fi & Jaringan",
            Foreground = Brushes.White,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerRow.Children.Add(wifiIcon);
        headerRow.Children.Add(headerTitle);
        headerBar.Child = headerRow;
        panel.Children.Add(headerBar);

        // Status
        m_status = new TextBlock {
            Text = "Memuat daftar jaringan…",
            Foreground = new SolidColorBrush(Color.FromRgb(170, 180, 195)),
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        panel.Children.Add(m_status);

        // Network list card
        m_wifiCard = new Border {
            CornerRadius = new CornerRadius(14),
            Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10)
        };
        m_networks = new ListBox {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontSize = 13,
            ItemContainerStyle = new Style(typeof(ListBoxItem)) {
                Setters = {
                    new Setter(Control.BackgroundProperty, Brushes.Transparent),
                    new Setter(Control.BorderThicknessProperty, new Thickness(0)),
                    new Setter(Control.PaddingProperty, new Thickness(0)),
                    new Setter(Control.MarginProperty, new Thickness(0, 0, 0, 6)),
                    new Setter(Control.ForegroundProperty, Brushes.White)
                }
            }
        };
        m_networks.SelectionChanged += (s, e) => {
            object item = m_networks.SelectedItem;
            if (item is NetworkEntry) {
                NetworkEntry entry = (NetworkEntry)item;
                ConnectNetwork(entry);
                m_networks.SelectedIndex = -1;
            }
        };
        m_wifiCard.Child = m_networks;
        panel.Children.Add(m_wifiCard);

        // Bottom actions
        Border bottomBar = new Border { Margin = new Thickness(0, 14, 0, 0) };
        StackPanel bottomRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        m_refresh = MakeRoundedButton(bottomRow, "Segarkan", new SolidColorBrush(Color.FromArgb(90, 23, 147, 209)), 110, 36, new Thickness(0, 0, 12, 0));
        m_refresh.Click += (s, e) => RefreshNetworks();
        m_toggleWifi = MakeRoundedButton(bottomRow, "Wi-Fi: ON", new SolidColorBrush(Color.FromArgb(90, 46, 209, 88)), 110, 36, new Thickness(0));
        m_toggleWifi.Click += (s, e) => ToggleWifi();
        bottomBar.Child = bottomRow;
        panel.Children.Add(bottomBar);

        root.Child = panel;
        Content = root;

        KeyDown += (s, e) => {
            if (e.Key == Key.Escape) Close();
        };

        Loaded += (s, e) => RefreshNetworks();
    }

    private Button MakeRoundedButton(StackPanel parent, string content, Brush bg, double width, double height, Thickness margin) {
        Border border = new Border {
            CornerRadius = new CornerRadius(18),
            Background = bg,
            Padding = new Thickness(0),
            Margin = margin,
            Width = width,
            Height = height
        };
        Button btn = new Button {
            Content = content,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        border.Child = btn;
        parent.Children.Add(border);
        return btn;
    }

    private void RefreshNetworks() {
        m_networks.ItemsSource = null;
        List<NetworkEntry> list = ScanNetworks();
        m_networks.ItemsSource = list;
        m_status.Text = list.Count == 0 ? "Tidak ada jaringan ditemukan" : list.Count + " jaringan tersedia";
    }

    private List<NetworkEntry> ScanNetworks() {
        List<NetworkEntry> result = new List<NetworkEntry>();
        try {
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "netsh",
                Arguments = "wlan show networks mode=bssid",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.GetEncoding(850)
            };
            using (Process p = Process.Start(psi)) {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(5000);
                string[] blocks = Regex.Split(output, @"SSID \d+ : ");
                foreach (string block in blocks.Skip(1)) {
                    string ssid = Extract(block, @"^(.+)");
                    if (string.IsNullOrWhiteSpace(ssid)) continue;
                    string auth = Extract(block, @"Authentication\s*:\s*(.+)");
                    string cipher = Extract(block, @"Cipher\s*:\s*(.+)");
                    string signal = Extract(block, @"Signal\s*:\s*(\d+)%");
                    int sig = 0;
                    int.TryParse(signal, out sig);
                    result.Add(new NetworkEntry {
                        Ssid = ssid.Trim(),
                        Authentication = auth.Trim(),
                        Cipher = cipher.Trim(),
                        SignalPercent = sig
                    });
                }
            }
        } catch { }
        return result.OrderByDescending(n => n.SignalPercent).ToList();
    }

    private static string Extract(string text, string pattern) {
        Match m = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return m.Success ? m.Groups[1].Value : string.Empty;
    }

    private void ConnectNetwork(NetworkEntry entry) {
        try {
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "netsh",
                Arguments = "wlan connect name=\"" + entry.Ssid + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process p = Process.Start(psi)) {
                p.StandardOutput.ReadToEnd();
                p.WaitForExit(5000);
            }
                m_status.Text = "Mencoba menghubungkan ke " + entry.Ssid;
        } catch { }
    }

    private void ToggleWifi() {
        try {
            string current = m_toggleWifi.Content.ToString();
            string args = current.Contains("ON") ? "wlan set hostednetwork mode=disallow" : "wlan set hostednetwork mode=allow";
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "netsh",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process p = Process.Start(psi)) {
                p.WaitForExit(3000);
            }
            m_toggleWifi.Content = current.Contains("ON") ? "WiFi: OFF" : "WiFi: ON";
        } catch { }
    }
}

public class NetworkEntry {
    public string Ssid { get; set; }
    public string Authentication { get; set; }
    public string Cipher { get; set; }
    public int SignalPercent { get; set; }
    public string Display {
        get {
            string bars = SignalPercent >= 75 ? "●●●●" : SignalPercent >= 50 ? "●●●○" : SignalPercent >= 25 ? "●●○○" : "●○○○";
            return bars + "  " + Ssid;
        }
    }
}
