# ============================================
# GNOME Top Bar Panel for Windows 10 & 11
# ============================================

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Windows.Forms

[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="GNOME Top Bar"
        Height="30"
        Top="0"
        Left="0"
        WindowStyle="None"
        AllowsTransparency="True"
        Topmost="True"
        ShowInTaskbar="False"
        Background="#F00E0E18"
        FontFamily="Segoe UI, Ubuntu, sans-serif">
    
    <Window.Resources>
        <Style TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#E0E0E0"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Padding" Value="9,2"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border x:Name="border" Background="{TemplateBinding Background}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="border" Property="Background" Value="#33FFFFFF"/>
                                <Setter Property="Foreground" Value="#FFFFFF"/>
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter TargetName="border" Property="Background" Value="#E95420"/>
                                <Setter Property="Foreground" Value="#FFFFFF"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>

    <Border BorderThickness="0,0,0,1" BorderBrush="#25FFFFFF">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Left: Activities & Shortcuts -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Margin="6,0,0,0" VerticalAlignment="Center">
                <Button Name="BtnActivities" FontWeight="Bold" FontSize="12" Margin="2,0">
                    <TextBlock Text="Activities" VerticalAlignment="Center"/>
                </Button>
                <Button Name="BtnTerminal" FontSize="11" Margin="2,0" ToolTip="Terminal Linux / PowerShell">
                    <TextBlock Text="Terminal" VerticalAlignment="Center"/>
                </Button>
                <Button Name="BtnFiles" FontSize="11" Margin="2,0" ToolTip="Files (Explorer)">
                    <TextBlock Text="Files" VerticalAlignment="Center"/>
                </Button>
            </StackPanel>

            <!-- Center: Linux GNOME Clock -->
            <StackPanel Grid.Column="1" HorizontalAlignment="Center" VerticalAlignment="Center">
                <Button Name="BtnClock" FontWeight="SemiBold" FontSize="12" ToolTip="Kalender &amp; Notifikasi">
                    <TextBlock Name="TxtClock" Text="Fri 10:30" VerticalAlignment="Center"/>
                </Button>
            </StackPanel>

            <!-- Right: System Tray & Power -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Margin="0,0,8,0" VerticalAlignment="Center">
                <Button Name="BtnVolume" FontSize="11" Margin="1,0" ToolTip="Volume Mixer">
                    <TextBlock Text="Vol" VerticalAlignment="Center"/>
                </Button>
                <Button Name="BtnNetwork" FontSize="11" Margin="1,0" ToolTip="Network Settings">
                    <TextBlock Text="Wi-Fi" VerticalAlignment="Center"/>
                </Button>
                <Button Name="BtnSettings" FontSize="11" Margin="1,0" ToolTip="Settings">
                    <TextBlock Text="Settings" VerticalAlignment="Center"/>
                </Button>
                <Button Name="BtnPower" FontSize="11" Margin="3,0" ToolTip="Power Menu">
                    <TextBlock Text="Power" FontWeight="Bold" Foreground="#FF7B36" VerticalAlignment="Center"/>
                </Button>
            </StackPanel>
        </Grid>
    </Border>
</Window>
"@

$reader = (New-Object System.Xml.XmlNodeReader $xaml)
$window = [System.Windows.Markup.XamlReader]::Load($reader)

# Set full screen width
$window.Width = [System.Windows.SystemParameters]::PrimaryScreenWidth

# Elements
$btnActivities = $window.FindName("BtnActivities")
$btnTerminal   = $window.FindName("BtnTerminal")
$btnFiles      = $window.FindName("BtnFiles")
$btnClock      = $window.FindName("BtnClock")
$txtClock      = $window.FindName("TxtClock")
$btnVolume     = $window.FindName("BtnVolume")
$btnNetwork    = $window.FindName("BtnNetwork")
$btnSettings   = $window.FindName("BtnSettings")
$btnPower      = $window.FindName("BtnPower")

# Clock DispatcherTimer
$timer = New-Object System.Windows.Threading.DispatcherTimer
$timer.Interval = [TimeSpan]::FromSeconds(1)
$timer.Add_Tick({
    $now = Get-Date
    $txtClock.Text = "$($now.ToString('ddd, dd MMM HH:mm'))"
})
$timer.Start()
& {
    $now = Get-Date
    $txtClock.Text = "$($now.ToString('ddd, dd MMM HH:mm'))"
}

# Click Events
$btnActivities.Add_Click({
    [System.Windows.Forms.SendKeys]::SendWait("^{ESC}")
})

$btnTerminal.Add_Click({
    if (Get-Command wt.exe -ErrorAction SilentlyContinue) {
        Start-Process wt.exe
    } else {
        Start-Process powershell.exe
    }
})

$btnFiles.Add_Click({
    Start-Process explorer.exe
})

$btnClock.Add_Click({
    [System.Windows.Forms.SendKeys]::SendWait("^%{TAB}")
})

$btnVolume.Add_Click({
    Start-Process sndvol.exe
})

$btnNetwork.Add_Click({
    Start-Process "ms-settings:network"
})

$btnSettings.Add_Click({
    Start-Process "ms-settings:"
})

# Power Menu (Context Menu)
$powerMenu = New-Object System.Windows.Controls.ContextMenu
$powerMenu.Background = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#E81A1A2E")
$powerMenu.Foreground = [System.Windows.Media.Brushes]::White
$powerMenu.BorderBrush = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#33FFFFFF")

$itemLock = New-Object System.Windows.Controls.MenuItem
$itemLock.Header = "Lock Screen"
$itemLock.Add_Click({ [System.Diagnostics.Process]::Start("rundll32.exe", "user32.dll,LockWorkStation") })
$powerMenu.Items.Add($itemLock) | Out-Null

$itemSleep = New-Object System.Windows.Controls.MenuItem
$itemSleep.Header = "Sleep"
$itemSleep.Add_Click({ [System.Diagnostics.Process]::Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0") })
$powerMenu.Items.Add($itemSleep) | Out-Null

$itemRestart = New-Object System.Windows.Controls.MenuItem
$itemRestart.Header = "Restart PC"
$itemRestart.Add_Click({ shutdown /r /t 0 })
$powerMenu.Items.Add($itemRestart) | Out-Null

$itemShutdown = New-Object System.Windows.Controls.MenuItem
$itemShutdown.Header = "Power Off PC"
$itemShutdown.Add_Click({ shutdown /s /t 0 })
$powerMenu.Items.Add($itemShutdown) | Out-Null

$separator = New-Object System.Windows.Controls.Separator
$powerMenu.Items.Add($separator) | Out-Null

$itemExit = New-Object System.Windows.Controls.MenuItem
$itemExit.Header = "Exit GNOME Panel"
$itemExit.Foreground = [System.Windows.Media.BrushConverter]::new().ConvertFromString("#FF7B36")
$itemExit.Add_Click({
    $timer.Stop()
    $window.Close()
})
$powerMenu.Items.Add($itemExit) | Out-Null

$btnPower.Add_Click({
    $powerMenu.PlacementTarget = $btnPower
    $powerMenu.IsOpen = $true
})

# Run window
$window.ShowDialog() | Out-Null
