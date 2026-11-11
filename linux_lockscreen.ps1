# Pratinjau visual layar bergaya Linux untuk Windows.
# Ini BUKAN layar kunci Windows dan tidak pernah meminta kata sandi.

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$themeDir = $PSScriptRoot
$wpPath = Join-Path $themeDir 'wallpapers\ubuntu_desktop.png'
$userName = $env:USERNAME
if ([string]::IsNullOrWhiteSpace($userName)) { $userName = 'user' }
$initial = $userName.Substring(0, 1).ToUpperInvariant()

[xml]$xaml = @'
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Pratinjau Layar Linux"
        WindowState="Maximized" WindowStyle="None" Topmost="True"
        ShowInTaskbar="False" Background="#17141D"
        FontFamily="Segoe UI" ResizeMode="NoResize">
    <Grid>
        <Image Name="BgImage" Stretch="UniformToFill"/>
        <Border>
            <Border.Background>
                <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                    <GradientStop Color="#E6100F18" Offset="0"/>
                    <GradientStop Color="#D91B1420" Offset="0.52"/>
                    <GradientStop Color="#EC101018" Offset="1"/>
                </LinearGradientBrush>
            </Border.Background>
            <Grid Margin="42,30">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <DockPanel Grid.Row="0" LastChildFill="False">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <Border Width="34" Height="34" CornerRadius="17" Background="#28E95420"
                                BorderBrush="#99E95420" BorderThickness="1">
                            <TextBlock Text="●" Foreground="#FFF0A58A" FontSize="17"
                                       HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                        <TextBlock Text="  TAMPILAN BERGAYA LINUX" Foreground="#E8E3EA"
                                   FontSize="12" FontWeight="SemiBold" VerticalAlignment="Center"/>
                    </StackPanel>
                    <Border DockPanel.Dock="Right" Background="#28E95420" BorderBrush="#66E95420"
                            BorderThickness="1" CornerRadius="14" Padding="12,7">
                        <TextBlock Text="PRATINJAU · BELUM TERKUNCI" Foreground="#FFFFB49A"
                                   FontSize="10" FontWeight="Bold"/>
                    </Border>
                </DockPanel>

                <StackPanel Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center">
                    <TextBlock Name="TxtClock" Text="09:41" FontSize="92" FontWeight="Light"
                               Foreground="#FFF9F7FA" HorizontalAlignment="Center"/>
                    <TextBlock Name="TxtDate" Text="Rabu, 11 November 2026"
                               FontSize="17" Foreground="#C9C1CD" HorizontalAlignment="Center"
                               Margin="0,0,0,36"/>

                    <Border Width="78" Height="78" CornerRadius="39" HorizontalAlignment="Center"
                            Background="#E95420" BorderBrush="#70FFFFFF" BorderThickness="2">
                        <TextBlock Name="TxtInitial" Text="U" FontSize="31" FontWeight="SemiBold"
                                   Foreground="White" HorizontalAlignment="Center"
                                   VerticalAlignment="Center"/>
                    </Border>
                    <TextBlock Name="TxtUser" Text="user" FontSize="17" FontWeight="SemiBold"
                               Foreground="#FFF9F7FA" HorizontalAlignment="Center" Margin="0,14,0,4"/>
                    <TextBlock Text="Desktop yang tenang, terinspirasi GNOME" FontSize="12"
                               Foreground="#A99FAB" HorizontalAlignment="Center"/>

                    <Border MaxWidth="420" Background="#33110F16" BorderBrush="#44FFFFFF"
                            BorderThickness="1" CornerRadius="16" Padding="18,13"
                            Margin="0,28,0,0">
                        <TextBlock Text="Ini hanya pratinjau tampilan. Windows belum terkunci. Tekan tombol di bawah untuk beralih ke layar kunci Windows yang sebenarnya."
                                   Foreground="#DED8E1" FontSize="13" TextWrapping="Wrap"
                                   TextAlignment="Center" MaxWidth="380"/>
                    </Border>

                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,22,0,0">
                        <Button Name="BtnLock" Content="Kunci Windows" MinWidth="158" Padding="20,12"
                                Background="#FFE95420" Foreground="White" BorderThickness="0"
                                FontSize="13" FontWeight="SemiBold" Cursor="Hand" Margin="0,0,10,0"/>
                        <Button Name="BtnClose" Content="Tutup pratinjau" MinWidth="138" Padding="18,12"
                                Background="#30FFFFFF" Foreground="#F3EEF4" BorderBrush="#40FFFFFF"
                                BorderThickness="1" FontSize="13" Cursor="Hand"/>
                    </StackPanel>
                    <TextBlock Text="Esc menutup pratinjau · Win+L mengunci Windows kapan saja"
                               Foreground="#968D9A" FontSize="11" HorizontalAlignment="Center"
                               Margin="0,14,0,0"/>
                </StackPanel>

                <DockPanel Grid.Row="2" LastChildFill="False">
                    <TextBlock Text="Nuansa Ubuntu · desktop Windows" Foreground="#9C94A0"
                               FontSize="11" VerticalAlignment="Center"/>
                    <TextBlock DockPanel.Dock="Right" Text="Masuk aman tetap dikelola Windows"
                               Foreground="#9C94A0" FontSize="11" VerticalAlignment="Center"/>
                </DockPanel>
            </Grid>
        </Border>
    </Grid>
</Window>
'@

$reader = New-Object System.Xml.XmlNodeReader $xaml
$window = [System.Windows.Markup.XamlReader]::Load($reader)
$bgImage = $window.FindName('BgImage')
$txtClock = $window.FindName('TxtClock')
$txtDate = $window.FindName('TxtDate')
$txtInitial = $window.FindName('TxtInitial')
$txtUser = $window.FindName('TxtUser')
$btnLock = $window.FindName('BtnLock')
$btnClose = $window.FindName('BtnClose')
$txtInitial.Text = $initial
$txtUser.Text = $userName

if (Test-Path -LiteralPath $wpPath) {
    try {
        $bitmap = New-Object System.Windows.Media.Imaging.BitmapImage
        $bitmap.BeginInit()
        $bitmap.UriSource = [Uri]::new($wpPath)
        $bitmap.CacheOption = [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad
        $bitmap.EndInit()
        $bgImage.Source = $bitmap
    } catch { }
}

$updateClock = {
    $now = Get-Date
    $txtClock.Text = $now.ToString('HH:mm')
    $txtDate.Text = $now.ToString('dddd, dd MMMM yyyy')
}
& $updateClock
$clockTimer = New-Object System.Windows.Threading.DispatcherTimer
$clockTimer.Interval = [TimeSpan]::FromSeconds(1)
$clockTimer.Add_Tick($updateClock)
$clockTimer.Start()

if (-not ([System.Management.Automation.PSTypeName]'LockWorkStationNative').Type) {
    Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;
public static class LockWorkStationNative {
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool LockWorkStation();
}
'@
}

$btnClose.Add_Click({ $window.Close() })
$btnLock.Add_Click({
    $clockTimer.Stop()
    $window.Close()
    [LockWorkStationNative]::LockWorkStation() | Out-Null
})
$window.Add_KeyDown({
    param($sender, $e)
    if ($e.Key -eq [System.Windows.Input.Key]::Escape) { $window.Close() }
})
$window.Add_Closed({ $clockTimer.Stop() })
$window.ShowDialog() | Out-Null
