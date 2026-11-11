# ==============================================================
# LINUX DYNAMIC ISLAND FOR WINDOWS 10 & 11
# ==============================================================
# Floating, non-intrusive pill widget at top center of screen.
# Leaves tabs and window controls 100% unobstructed.
# Features:
#   - Draggable: Left-click and drag anywhere to reposition
#   - Expand on hover or click: Live CPU, RAM, Media, Quick Launch
#   - Arch Linux Panel • blue accent and dark glass
#   - Right-click menu: Pin, Center, Exit
#   - ASCII only (no emoji to prevent PowerShell encoding errors)
# ==============================================================

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Windows.Forms

# ================= WORK AREA HELPER =================
# Reserves 38px at the top of the monitor so maximized windows
# (Chrome, WhatsApp, VS Code, etc.) NEVER get covered!
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class DesktopWorkArea {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

    public const uint SPI_GETWORKAREA = 0x0030;
    public const uint SPI_SETWORKAREA = 0x002F;
    public const uint SPIF_SENDCHANGE = 0x0002;

    public static RECT OriginalRect;
    public static bool HasSaved = false;
    public static bool IsReserved = false;

    public static void ReserveTop(int height) {
        try {
            if (!HasSaved) {
                SystemParametersInfo(SPI_GETWORKAREA, 0, ref OriginalRect, 0);
                HasSaved = true;
            }
            RECT r = OriginalRect;
            r.Top = height;
            SystemParametersInfo(SPI_SETWORKAREA, 0, ref r, SPIF_SENDCHANGE);
            IsReserved = true;
        } catch {}
    }

    public static void Restore() {
        try {
            if (HasSaved && IsReserved) {
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref OriginalRect, SPIF_SENDCHANGE);
                IsReserved = false;
            }
        } catch {}
    }
}
"@ -ReferencedAssemblies "PresentationFramework"

[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Arch Panel"
        SizeToContent="WidthAndHeight"
        WindowStyle="None"
        AllowsTransparency="True"
        Topmost="True"
        ShowInTaskbar="False"
        Background="Transparent"
        FontFamily="Segoe UI, Cascadia Code, sans-serif">

    <Border Name="IslandContainer" Margin="10">
        <Border Name="IslandBorder"
                Background="#F2101015"
                BorderBrush="#501793D1"
                BorderThickness="1"
                CornerRadius="20"
                Padding="8,5"
                MinWidth="220"
                Cursor="Hand"
                HorizontalAlignment="Center">
            
            <Border.Effect>
                <DropShadowEffect Color="#000000" BlurRadius="22" ShadowDepth="4" Opacity="0.65"/>
            </Border.Effect>

            <Grid Name="MainGrid">
                <!-- ================= COLLAPSED PILL ================= -->
                <StackPanel Name="PillCollapsed" Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="6,1">
                    <!-- Status dot -->
                    <Ellipse Name="StatusDot" Width="9" Height="9" Fill="#2ED158" Margin="0,0,10,0" VerticalAlignment="Center"/>
                    
                    <!-- Distro tag -->
                    <Border Background="#251793D1" CornerRadius="8" Padding="6,1" Margin="0,0,8,0" VerticalAlignment="Center">
                        <TextBlock Text="ARCH" FontSize="9" FontWeight="Bold" Foreground="#1793D1" VerticalAlignment="Center"/>
                    </Border>
                    
                    <!-- Clock -->
                    <TextBlock Name="PillClock" Text="12:00" Foreground="#FFFFFF" FontSize="12" FontWeight="SemiBold" VerticalAlignment="Center"/>
                    
                    <TextBlock Text="·" Foreground="#40FFFFFF" FontSize="11" Margin="8,0" VerticalAlignment="Center"/>
                    
                    <!-- Media or CPU badge -->
                    <TextBlock Name="PillSubtitle" Text="Idle" Foreground="#A0A0A0" FontSize="11" MaxWidth="110" TextTrimming="CharacterEllipsis" VerticalAlignment="Center"/>
                </StackPanel>

                <!-- ================= EXPANDED CARD ================= -->
                <Border Name="PanelExpanded" Visibility="Collapsed" Padding="6,4" MinWidth="410">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <!-- Header: Distro badge, Date/Time, Pin status -->
                        <DockPanel Grid.Row="0" Margin="4,2,4,8">
                            <StackPanel Orientation="Horizontal" DockPanel.Dock="Left" VerticalAlignment="Center">
                                <Ellipse Width="10" Height="10" Fill="#2ED158" Margin="0,0,8,0" VerticalAlignment="Center"/>
                                <Border Background="#1793D1" CornerRadius="8" Padding="7,2" Margin="0,0,8,0">
                                    <TextBlock Text="ARCH PANEL" FontSize="10" FontWeight="Bold" Foreground="#FFFFFF"/>
                                </Border>
                                <TextBlock Name="ExpandTime" Text="12:00:00" Foreground="#FFFFFF" FontSize="13" FontWeight="Bold" VerticalAlignment="Center"/>
                            </StackPanel>

                            <StackPanel Orientation="Horizontal" DockPanel.Dock="Right" HorizontalAlignment="Right" VerticalAlignment="Center">
                                <TextBlock Name="ExpendDate" Text="Fri, 11 Nov 2026" Foreground="#888899" FontSize="11" VerticalAlignment="Center" Margin="0,0,8,0"/>
                                <TextBlock Name="BtnPin" Text="PIN" Foreground="#5BC7EF" FontSize="10" FontWeight="Bold" Cursor="Hand" ToolTip="Pin expanded card"/>
                            </StackPanel>
                        </DockPanel>

                        <!-- Middle Row: Live System Stats (CPU & RAM Bars) -->
                        <Border Grid.Row="1" Background="#16FFFFFF" CornerRadius="12" Padding="12,8" Margin="2,0,2,8">
                            <Grid>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height="Auto"/>
                                    <RowDefinition Height="Auto"/>
                                </Grid.RowDefinitions>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>

                                <!-- CPU Bar -->
                                <StackPanel Grid.Row="0" Grid.Column="0" Margin="0,0,10,4">
                                    <DockPanel>
                                        <TextBlock Text="CPU" Foreground="#CCCCCC" FontSize="10" FontWeight="SemiBold" DockPanel.Dock="Left"/>
                                        <TextBlock Name="TxtCpu" Text="0%" Foreground="#1793D1" FontSize="10" FontWeight="Bold" DockPanel.Dock="Right" HorizontalAlignment="Right"/>
                                    </DockPanel>
                                    <ProgressBar Name="BarCpu" Height="5" Margin="0,4,0,0" Background="#33FFFFFF" Foreground="#1793D1" BorderThickness="0" Value="15" Maximum="100"/>
                                </StackPanel>

                                <!-- RAM Bar -->
                                <StackPanel Grid.Row="0" Grid.Column="1" Margin="10,0,0,4">
                                    <DockPanel>
                                        <TextBlock Text="RAM" Foreground="#CCCCCC" FontSize="10" FontWeight="SemiBold" DockPanel.Dock="Left"/>
                                        <TextBlock Name="TxtRam" Text="0%" Foreground="#5BC7EF" FontSize="10" FontWeight="Bold" DockPanel.Dock="Right" HorizontalAlignment="Right"/>
                                    </DockPanel>
                                    <ProgressBar Name="BarRam" Height="5" Margin="0,4,0,0" Background="#33FFFFFF" Foreground="#5BC7EF" BorderThickness="0" Value="40" Maximum="100"/>
                                </StackPanel>

                                <!-- Media / Player info -->
                                <DockPanel Grid.Row="1" Grid.ColumnSpan="2" Margin="0,6,0,0">
                                    <TextBlock Text="MEDIA" Foreground="#888888" FontSize="9" FontWeight="Bold" DockPanel.Dock="Left" Margin="0,0,8,0" VerticalAlignment="Center"/>
                                    <TextBlock Name="TxtMedia" Text="No active media player" Foreground="#E0E0E0" FontSize="11" TextTrimming="CharacterEllipsis" VerticalAlignment="Center"/>
                                </DockPanel>
                            </Grid>
                        </Border>

                        <!-- Row 2: Quick Launchers (Linux-style shortcuts) -->
                        <UniformGrid Grid.Row="2" Columns="5" Margin="2,0,2,6">
                            <!-- Terminal -->
                            <Button Name="BtnTerm" Margin="2,0" Padding="0,6" Background="#20FFFFFF" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border Name="b" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="b" Property="Background" Value="#351793D1"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                                <StackPanel HorizontalAlignment="Center">
                                    <TextBlock Text="TERM" FontSize="12" FontWeight="Bold" Foreground="#1793D1" HorizontalAlignment="Center"/>
                                    <TextBlock Text="Terminal" FontSize="10" Foreground="#CCCCCC" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                                </StackPanel>
                            </Button>

                            <!-- Files (Explorer) -->
                            <Button Name="BtnFiles" Margin="2,0" Padding="0,6" Background="#20FFFFFF" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border Name="b" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="b" Property="Background" Value="#353498DB"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                                <StackPanel HorizontalAlignment="Center">
                                    <TextBlock Text="FILES" FontSize="12" FontWeight="Bold" Foreground="#3498DB" HorizontalAlignment="Center"/>
                                    <TextBlock Text="Files" FontSize="10" Foreground="#CCCCCC" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                                </StackPanel>
                            </Button>

                            <!-- Browser -->
                            <Button Name="BtnBrowser" Margin="2,0" Padding="0,6" Background="#20FFFFFF" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border Name="b" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="b" Property="Background" Value="#3564CDF5"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                                <StackPanel HorizontalAlignment="Center">
                                    <TextBlock Text="WEB" FontSize="11" FontWeight="Bold" Foreground="#64CDF5" HorizontalAlignment="Center"/>
                                    <TextBlock Text="Browser" FontSize="10" Foreground="#CCCCCC" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                                </StackPanel>
                            </Button>

                            <!-- Settings -->
                            <Button Name="BtnSettings" Margin="2,0" Padding="0,6" Background="#20FFFFFF" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border Name="b" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="b" Property="Background" Value="#351ABC9C"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                                <StackPanel HorizontalAlignment="Center">
                                    <TextBlock Text="SET" FontSize="11" FontWeight="Bold" Foreground="#1ABC9C" HorizontalAlignment="Center"/>
                                    <TextBlock Text="Settings" FontSize="10" Foreground="#CCCCCC" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                                </StackPanel>
                            </Button>

                            <!-- Power Menu -->
                            <Button Name="BtnPower" Margin="2,0" Padding="0,6" Background="#20FFFFFF" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border Name="b" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="b" Property="Background" Value="#35E74C3C"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                                <StackPanel HorizontalAlignment="Center">
                                    <TextBlock Text="PWR" FontSize="11" FontWeight="Bold" Foreground="#FF6B6B" HorizontalAlignment="Center"/>
                                    <TextBlock Text="Power" FontSize="10" Foreground="#CCCCCC" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                                </StackPanel>
                            </Button>
                        </UniformGrid>

                        <!-- Hint / Drag info -->
                        <TextBlock Grid.Row="3" Text="Drag pill to move | Right-click for options" Foreground="#555566" FontSize="9" HorizontalAlignment="Center" Margin="0,2,0,0"/>
                    </Grid>
                </Border>
            </Grid>
        </Border>
    </Border>
</Window>
"@

$reader = (New-Object System.Xml.XmlNodeReader $xaml)
$window = [System.Windows.Markup.XamlReader]::Load($reader)

# Screen resolution
$screenWidth = [System.Windows.SystemParameters]::PrimaryScreenWidth
$screenHeight = [System.Windows.SystemParameters]::PrimaryScreenHeight

# Position window at top center
$window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::Manual
$window.Top = 6

# Elements
$islandBorder   = $window.FindName("IslandBorder")
$pillCollapsed  = $window.FindName("PillCollapsed")
$panelExpanded  = $window.FindName("PanelExpanded")
$pillClock      = $window.FindName("PillClock")
$pillSubtitle   = $window.FindName("PillSubtitle")
$expandTime     = $window.FindName("ExpandTime")
$expendDate     = $window.FindName("ExpendDate")
$txtCpu         = $window.FindName("TxtCpu")
$barCpu         = $window.FindName("BarCpu")
$txtRam         = $window.FindName("TxtRam")
$barRam         = $window.FindName("BarRam")
$txtMedia       = $window.FindName("TxtMedia")
$btnPin         = $window.FindName("BtnPin")
$btnTerm        = $window.FindName("BtnTerm")
$btnFiles       = $window.FindName("BtnFiles")
$btnBrowser     = $window.FindName("BtnBrowser")
$btnSettings    = $window.FindName("BtnSettings")
$btnPower       = $window.FindName("BtnPower")
$statusDot      = $window.FindName("StatusDot")

# State
$script:isExpanded = $false
$script:isPinned = $false
$script:isDragging = $false

# Helper: Center Window Horizontally
function Center-Island {
    if ($window.ActualWidth -gt 0) {
        $window.Left = [Math]::Max(10, ($screenWidth - $window.ActualWidth) / 2)
    }
}

# Window Loaded
$window.Add_Loaded({
    Center-Island
    [DesktopWorkArea]::ReserveTop(38)
})

$window.Add_Closing({
    [DesktopWorkArea]::Restore()
})

# ================= MOUSE DRAG =================
$islandBorder.Add_MouseLeftButtonDown({
    param($sender, $e)
    $source = $e.OriginalSource
    $interactive = $false
    while ($source -and $source -ne $islandBorder) {
        if ($source -is [System.Windows.Controls.Button] -or $source -eq $btnPin) {
            $interactive = $true
            break
        }
        $source = [System.Windows.Media.VisualTreeHelper]::GetParent($source)
    }
    if ($e.ButtonState -eq [System.Windows.Input.MouseButtonState]::Pressed -and -not $interactive) {
        $window.DragMove()
    }
})

# ================= EXPAND / COLLAPSE =================
$expandAction = {
    if (-not $script:isExpanded) {
        $script:isExpanded = $true
        $pillCollapsed.Visibility = [System.Windows.Visibility]::Collapsed
        $panelExpanded.Visibility = [System.Windows.Visibility]::Visible
        $islandBorder.CornerRadius = [System.Windows.CornerRadius]::new(20)
        
        # Center after expansion unless manually moved
        $window.Dispatcher.InvokeAsync([Action]{ Center-Island }) | Out-Null
    }
}

$collapseAction = {
    if ($script:isExpanded -and (-not $script:isPinned)) {
        $script:isExpanded = $false
        $panelExpanded.Visibility = [System.Windows.Visibility]::Collapsed
        $pillCollapsed.Visibility = [System.Windows.Visibility]::Visible
        $islandBorder.CornerRadius = [System.Windows.CornerRadius]::new(20)
        
        $window.Dispatcher.InvokeAsync([Action]{ Center-Island }) | Out-Null
    }
}

$islandBorder.Add_MouseEnter({
    & $expandAction
})

$islandBorder.Add_MouseLeave({
    & $collapseAction
})

# Pin Button Toggle
$btnPin.Add_MouseDown({
    $script:isPinned = -not $script:isPinned
    if ($script:isPinned) {
        $btnPin.Text = "PINNED"
        $btnPin.Foreground = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#2ED158")
    } else {
        $btnPin.Text = "PIN"
        $btnPin.Foreground = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#5BC7EF")
    }
})

# ================= REAL-TIME CLOCK =================
$clockTimer = New-Object System.Windows.Threading.DispatcherTimer
$clockTimer.Interval = [TimeSpan]::FromSeconds(1)
$clockTimer.Add_Tick({
    $now = Get-Date
    $pillClock.Text = $now.ToString("HH:mm")
    $expandTime.Text = $now.ToString("HH:mm:ss")
    $expendDate.Text = $now.ToString("ddd, dd MMM yyyy")
})
$clockTimer.Start()

# Initial clock
& {
    $now = Get-Date
    $pillClock.Text = $now.ToString("HH:mm")
    $expandTime.Text = $now.ToString("HH:mm:ss")
    $expendDate.Text = $now.ToString("ddd, dd MMM yyyy")
}

# ================= SYSTEM STATS TIMER =================
$statsTimer = New-Object System.Windows.Threading.DispatcherTimer
$statsTimer.Interval = [TimeSpan]::FromSeconds(2)
$statsTimer.Add_Tick({
    # CPU
    try {
        $cpuVal = (Get-Counter '\Processor(_Total)\% Processor Time' -ErrorAction SilentlyContinue).CounterSamples[0].CookedValue
        $cpuInt = [Math]::Min(100, [Math]::Max(0, [Math]::Round($cpuVal)))
        $txtCpu.Text = "$cpuInt%"
        $barCpu.Value = $cpuInt
        
        if (-not $script:hasMedia) {
            $pillSubtitle.Text = "CPU $cpuInt%"
        }
    } catch {
        $txtCpu.Text = "--"
    }

    # RAM
    try {
        $os = Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue
        if ($os) {
            $totalRam = $os.TotalVisibleMemorySize
            $freeRam = $os.FreePhysicalMemory
            $usedPct = [Math]::Round((($totalRam - $freeRam) / $totalRam) * 100)
            $txtRam.Text = "$usedPct%"
            $barRam.Value = $usedPct
        }
    } catch {
        $txtRam.Text = "--"
    }
})
$statsTimer.Start()

# ================= MEDIA DETECTION TIMER =================
$script:hasMedia = $false
$mediaTimer = New-Object System.Windows.Threading.DispatcherTimer
$mediaTimer.Interval = [TimeSpan]::FromSeconds(4)
$mediaTimer.Add_Tick({
    try {
        $procs = Get-Process -Name "Spotify","MusicBee","AIMP","foobar2000","vlc","wmplayer" -ErrorAction SilentlyContinue
        $activeMedia = $null
        foreach ($p in $procs) {
            if ($p.MainWindowTitle -and $p.MainWindowTitle -ne "") {
                $activeMedia = $p.MainWindowTitle
                break
            }
        }
        
        if ($activeMedia) {
            $script:hasMedia = $true
            $txtMedia.Text = $activeMedia
            $pillSubtitle.Text = $activeMedia
            $statusDot.Fill = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#2ED158")
        } else {
            $script:hasMedia = $false
            $txtMedia.Text = "No active media playback"
            $statusDot.Fill = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#3498DB")
        }
    } catch {
        $script:hasMedia = $false
    }
})
$mediaTimer.Start()

# ================= BUTTON ACTIONS =================
$btnTerm.Add_Click({
    if (Get-Command wt.exe -ErrorAction SilentlyContinue) {
        Start-Process wt.exe
    } else {
        Start-Process powershell.exe
    }
})

$btnFiles.Add_Click({
    Start-Process explorer.exe
})

$btnBrowser.Add_Click({
    if (Get-Command msedge.exe -ErrorAction SilentlyContinue) {
        Start-Process msedge.exe
    } elseif (Get-Command chrome.exe -ErrorAction SilentlyContinue) {
        Start-Process chrome.exe
    } else {
        Start-Process "https://www.google.com"
    }
})

$btnSettings.Add_Click({
    Start-Process "ms-settings:"
})

# Power Menu (Context Menu)
$powerMenu = New-Object System.Windows.Controls.ContextMenu
$powerMenu.Background = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#E8101018")
$powerMenu.Foreground = [System.Windows.Media.Brushes]::White
$powerMenu.BorderBrush = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#33FFFFFF")

$mkItem = {
    param($title, $action, $color)
    $item = New-Object System.Windows.Controls.MenuItem
    $item.Header = $title
    if ($color) {
        $item.Foreground = [System.Windows.Media.BrushConverter]::new().ConvertFromString($color)
    }
    $item.Add_Click($action)
    return $item
}

$powerMenu.Items.Add((& $mkItem "Linux UI Preview (not a lock)" { 
    $lsPath = Join-Path $PSScriptRoot "linux_lockscreen.ps1"
    if (-not (Test-Path $lsPath)) { $lsPath = "c:\Users\labta\Downloads\theme\linux_lockscreen.ps1" }
    Start-Process powershell.exe -ArgumentList "-NoProfile -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$lsPath`""
} "#1793D1")) | Out-Null
$powerMenu.Items.Add((& $mkItem "Lock Screen (Windows)" { [System.Diagnostics.Process]::Start("rundll32.exe", "user32.dll,LockWorkStation") } $null)) | Out-Null
$powerMenu.Items.Add((& $mkItem "Sleep" { [System.Diagnostics.Process]::Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0") } $null)) | Out-Null
$powerMenu.Items.Add((& $mkItem "Restart PC" { shutdown /r /t 5 } $null)) | Out-Null
$powerMenu.Items.Add((& $mkItem "Power Off PC" { shutdown /s /t 5 } "#FF6B6B")) | Out-Null
$powerMenu.Items.Add((New-Object System.Windows.Controls.Separator)) | Out-Null
$powerMenu.Items.Add((& $mkItem "Exit Dynamic Island" {
    $clockTimer.Stop()
    $statsTimer.Stop()
    $mediaTimer.Stop()
    [DesktopWorkArea]::Restore()
    $window.Close()
} "#1793D1")) | Out-Null

$btnPower.Add_Click({
    $powerMenu.PlacementTarget = $btnPower
    $powerMenu.IsOpen = $true
})

# ================= RIGHT CLICK CONTEXT MENU ON PILL =================
$contextMenu = New-Object System.Windows.Controls.ContextMenu
$contextMenu.Background = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#E8101018")
$contextMenu.Foreground = [System.Windows.Media.Brushes]::White
$contextMenu.BorderBrush = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#33FFFFFF")

$cCenter = & $mkItem "Center on Screen" {
    $window.Top = 6
    Center-Island
} $null
$contextMenu.Items.Add($cCenter) | Out-Null

$cToggleTop = & $mkItem "Always on Top: ON" {
    $window.Topmost = -not $window.Topmost
    if ($window.Topmost) {
        $cToggleTop.Header = "Always on Top: ON"
    } else {
        $cToggleTop.Header = "Always on Top: OFF"
    }
} $null
$contextMenu.Items.Add($cToggleTop) | Out-Null

$cToggleReserve = & $mkItem "No-Cover Mode (Reserve Top): ON" {
    if ([DesktopWorkArea]::IsReserved) {
        [DesktopWorkArea]::Restore()
        $cToggleReserve.Header = "No-Cover Mode (Reserve Top): OFF"
    } else {
        [DesktopWorkArea]::ReserveTop(38)
        $cToggleReserve.Header = "No-Cover Mode (Reserve Top): ON"
    }
} $null
$contextMenu.Items.Add($cToggleReserve) | Out-Null

$contextMenu.Items.Add((New-Object System.Windows.Controls.Separator)) | Out-Null

$cExit = & $mkItem "Exit Dynamic Island" {
    $clockTimer.Stop()
    $statsTimer.Stop()
    $mediaTimer.Stop()
    [DesktopWorkArea]::Restore()
    $window.Close()
} "#1793D1"
$contextMenu.Items.Add($cExit) | Out-Null

$islandBorder.ContextMenu = $contextMenu

# Run window
$window.ShowDialog() | Out-Null
