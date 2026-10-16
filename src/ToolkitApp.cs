// ====================================================================
//  ULTIMATE IT SUPPORT TOOLKIT - PROFESSIONAL EDITION (GUI)
//  Developed & Maintained By HASIF KHAN
//  Copyright (c) 2026 Hasif Khan. All Rights Reserved.
//  Project ID : HK-ITST-2026-001
// ====================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Ultimate IT Support Toolkit")]
[assembly: AssemblyProduct("Ultimate IT Support Toolkit")]
[assembly: AssemblyCompany("Hasif Khan")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Hasif Khan. All Rights Reserved.")]
[assembly: AssemblyVersion("3.2.1.0")]
[assembly: AssemblyFileVersion("3.2.1.0")]

namespace HK.ITToolkit
{
    // ------------------------------------------------------------ THEME
    static class Theme
    {
        public static float Scale = 1f;
        public static int S(int px) { return (int)Math.Round(px * Scale); }

        public static readonly Color Navy = Color.FromArgb(15, 23, 42);
        public static readonly Color NavHover = Color.FromArgb(30, 41, 59);
        public static readonly Color NavSel = Color.FromArgb(37, 99, 235);
        public static readonly Color NavText = Color.FromArgb(203, 213, 225);
        public static readonly Color Accent = Color.FromArgb(37, 99, 235);
        public static readonly Color AccentLight = Color.FromArgb(96, 165, 250);
        public static readonly Color Bg = Color.FromArgb(241, 245, 249);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color Text = Color.FromArgb(15, 23, 42);
        public static readonly Color Muted = Color.FromArgb(100, 116, 139);
        public static readonly Color Green = Color.FromArgb(16, 185, 129);
        public static readonly Color GreenLight = Color.FromArgb(74, 222, 128);
        public static readonly Color Amber = Color.FromArgb(245, 158, 11);
        public static readonly Color Red = Color.FromArgb(239, 68, 68);
        public static readonly Color RedLight = Color.FromArgb(248, 113, 113);
        public static readonly Color ConsoleBg = Color.FromArgb(11, 18, 32);
        public static readonly Color ConsoleFg = Color.FromArgb(203, 213, 225);

        public static Font F(float pt, FontStyle st) { return new Font("Segoe UI", pt, st); }
        public static readonly Font Regular9 = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font Bold105 = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        public static readonly Font Small85 = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font Big20 = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font Bold9 = new Font("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font Bold95 = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static readonly Font Tag75 = new Font("Segoe UI", 7.5f, FontStyle.Bold);
    }

    static class Gfx
    {
        public static GraphicsPath Round(Rectangle r, int rad)
        {
            int d = rad * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ------------------------------------------------------------ PATHS / LOG
    static class Paths
    {
        public static readonly string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "IT-Toolkit");
        public static readonly string Logs = Path.Combine(Base, "Logs");
        public static readonly string Reports = Path.Combine(Base, "Reports");
        public static readonly string LogFile = Path.Combine(Logs, "toolkit-activity.log");

        public static void Ensure()
        {
            try { Directory.CreateDirectory(Logs); Directory.CreateDirectory(Reports); } catch { }
        }
        public static void Log(string msg)
        {
            try
            {
                File.AppendAllText(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + Environment.UserName + " | " + msg + Environment.NewLine);
            }
            catch { }
        }
    }

    // ------------------------------------------------------------ SYSTEM HELPERS
    static class Sys
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        [DllImport("kernel32.dll")]
        static extern ulong GetTickCount64();

        public static bool IsAdmin()
        {
            try
            {
                WindowsPrincipal p = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        public static void Memory(out int loadPct, out double totalGb, out double usedGb)
        {
            loadPct = 0; totalGb = 0; usedGb = 0;
            try
            {
                MEMORYSTATUSEX m = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(m))
                {
                    loadPct = (int)m.dwMemoryLoad;
                    totalGb = m.ullTotalPhys / 1073741824.0;
                    usedGb = (m.ullTotalPhys - m.ullAvailPhys) / 1073741824.0;
                }
            }
            catch { }
        }

        public static TimeSpan Uptime() { return TimeSpan.FromMilliseconds((double)GetTickCount64()); }

        public static string OsName()
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    string name = Convert.ToString(k.GetValue("ProductName"));
                    string ver = Convert.ToString(k.GetValue("DisplayVersion"));
                    int build; int.TryParse(Convert.ToString(k.GetValue("CurrentBuild")), out build);
                    if (build >= 22000 && name.Contains("Windows 10")) name = name.Replace("Windows 10", "Windows 11");
                    return (name + " " + ver + " (build " + build + ")").Trim();
                }
            }
            catch { return Environment.OSVersion.ToString(); }
        }

        public static string CpuName()
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    return Regex.Replace(Convert.ToString(k.GetValue("ProcessorNameString")), @"\s+", " ").Trim();
                }
            }
            catch { return "Unknown"; }
        }

        public static string IPv4()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback || ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                    foreach (UnicastIPAddressInformation ua in ni.GetIPProperties().UnicastAddresses)
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork) return ua.Address.ToString();
                }
            }
            catch { }
            return "Not connected";
        }

        public static string FormatUptime(TimeSpan t)
        {
            return t.Days + "d " + t.Hours.ToString("00") + "h " + t.Minutes.ToString("00") + "m";
        }
    }

    // ------------------------------------------------------------ TOOL MODEL
    class Tool
    {
        public string Cat, Name, Desc, Kind, Cmd, Confirm, Prompt, Default;
        public bool Admin, Spaces;
        public Tool NeedAdmin() { Admin = true; return this; }
        public Tool Ask(string msg) { Confirm = msg; return this; }
        public Tool Input(string prompt, string def) { Prompt = prompt; Default = def; return this; }
        public Tool AllowSpaces() { Spaces = true; return this; }
    }

    // ------------------------------------------------------------ CATALOG
    static class Catalog
    {
        public static readonly List<string> Cats = new List<string> {
            "Dashboard", "PC Information", "Hardware", "Network", "Internet", "Windows Repair",
            "Disk & Storage", "Performance", "Services", "Users & Accounts", "Drivers & Devices",
            "Windows Update", "Security", "Wi-Fi", "Reports", "System Tools", "Power" };

        public static readonly List<Tool> All = new List<Tool>();

        static Tool Add(string cat, string name, string desc, string kind, string cmd)
        {
            Tool t = new Tool(); t.Cat = cat; t.Name = name; t.Desc = desc; t.Kind = kind; t.Cmd = cmd;
            All.Add(t); return t;
        }
        static Tool P(string c, string n, string d, string cmd) { return Add(c, n, d, "P", cmd); }   // PowerShell
        static Tool C(string c, string n, string d, string cmd) { return Add(c, n, d, "C", cmd); }   // cmd.exe
        static Tool L(string c, string n, string d, string cmd) { return Add(c, n, d, "L", cmd); }   // launch app

        public static Tool Find(string name)
        {
            foreach (Tool t in All) if (t.Name == name) return t;
            return null;
        }
        public static List<Tool> InCat(string cat)
        {
            List<Tool> r = new List<Tool>();
            foreach (Tool t in All) if (t.Cat == cat) r.Add(t);
            return r;
        }

        // ---- report building blocks
        const string SEC_SYS = @"
'== OPERATING SYSTEM =='; Get-CimInstance Win32_OperatingSystem | Select Caption,Version,BuildNumber,OSArchitecture,InstallDate,LastBootUpTime | Format-List;
'== COMPUTER =='; Get-CimInstance Win32_ComputerSystem | Select Manufacturer,Model,SystemType,@{N='RAM_GB';E={[math]::Round($_.TotalPhysicalMemory/1GB,2)}} | Format-List;
'== PROCESSOR =='; Get-CimInstance Win32_Processor | Select Name,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed | Format-List;
'== BIOS =='; Get-CimInstance Win32_BIOS | Select Manufacturer,SMBIOSBIOSVersion,SerialNumber,ReleaseDate | Format-List;
'== VOLUMES =='; Get-Volume | Where-Object DriveLetter | Select DriveLetter,FileSystemLabel,FileSystem,@{N='SizeGB';E={[math]::Round($_.Size/1GB,2)}},@{N='FreeGB';E={[math]::Round($_.SizeRemaining/1GB,2)}} | Format-Table -Auto;
";
        const string SEC_NET = @"
'== IP CONFIGURATION =='; ipconfig /all;
'== NETWORK ADAPTERS =='; Get-NetAdapter | Format-Table Name,Status,LinkSpeed,MacAddress -Auto;
'== ROUTING TABLE =='; route print;
'== ARP TABLE =='; arp -a;
'== FIREWALL =='; netsh advfirewall show allprofiles;
'== WI-FI =='; netsh wlan show interfaces;
";
        const string SEC_HW = @"
'== MEMORY MODULES =='; Get-CimInstance Win32_PhysicalMemory | Select Manufacturer,PartNumber,@{N='CapacityGB';E={[math]::Round($_.Capacity/1GB,2)}},Speed | Format-Table -Auto;
'== PHYSICAL DISKS =='; Get-PhysicalDisk | Select FriendlyName,MediaType,HealthStatus,OperationalStatus,@{N='SizeGB';E={[math]::Round($_.Size/1GB,2)}} | Format-Table -Auto;
'== PROBLEM DEVICES =='; Get-PnpDevice | Where-Object {$_.Status -ne 'OK'} | Format-Table Status,Class,FriendlyName -Auto;
";
        const string SEC_SEC = @"
'== DEFENDER =='; try{Get-MpComputerStatus -ErrorAction Stop | Select AntivirusEnabled,AntispywareEnabled,RealTimeProtectionEnabled,AMServiceEnabled | Format-List}catch{'Defender status unavailable.'};
";

        static string Report(string prefix, string title, string body)
        {
            return @"$f=Join-Path '{REPORTS}' ('" + prefix + @"_'+(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')+'.txt');
$sb={ '======================================================================'; '" + title + @"'; 'Developed & Maintained By HASIF KHAN  |  Project ID HK-ITST-2026-001'; 'Generated : '+(Get-Date); 'Computer  : '+$env:COMPUTERNAME; 'User      : '+$env:USERNAME; '======================================================================';
" + body + @" };
& $sb *>&1 | Out-String -Width 200 | Out-File -FilePath $f -Encoding UTF8;
'Report saved to:'; $f; Start-Process notepad.exe $f;";
        }

        const string HEALTH = @"
$global:OKc=0;$global:WNc=0;$global:FLc=0;
function R([string]$s,[string]$m){ if($s -eq 'OK'){$global:OKc++; '[OK]   '+$m} elseif($s -eq 'WARN'){$global:WNc++; '[WARN] '+$m} else {$global:FLc++; '[FAIL] '+$m} };
'== SYSTEM ==';
$os=Get-CimInstance Win32_OperatingSystem;
R 'OK' ('Operating system: '+$os.Caption+' (build '+$os.BuildNumber+')');
$up=(Get-Date)-$os.LastBootUpTime;
if($up.TotalDays -gt 14){R 'WARN' ('Last restart was '+[int]$up.TotalDays+' days ago - a restart is recommended')}else{R 'OK' ('Uptime: '+[int]$up.TotalDays+' day(s)')};
$pend=(Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending') -or (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired');
if($pend){R 'WARN' 'A restart is pending to finish updates'}else{R 'OK' 'No pending restart'};
'== PERFORMANCE ==';
$cpu=(Get-CimInstance Win32_Processor | Measure-Object LoadPercentage -Average).Average;
if($cpu -gt 90){R 'WARN' ('CPU load is high: '+$cpu+'%')}else{R 'OK' ('CPU load: '+$cpu+'%')};
$ram=[math]::Round(100-($os.FreePhysicalMemory/$os.TotalVisibleMemorySize*100));
if($ram -gt 90){R 'WARN' ('Memory usage is high: '+$ram+'%')}else{R 'OK' ('Memory usage: '+$ram+'%')};
'== STORAGE ==';
foreach($v in (Get-Volume | Where-Object {$_.DriveLetter -and $_.DriveType -eq 'Fixed' -and $_.Size -gt 0})){ $pc=[math]::Round($v.SizeRemaining/$v.Size*100); $msg='Drive '+$v.DriveLetter+': '+$pc+'% free ('+[math]::Round($v.SizeRemaining/1GB,1)+' GB)'; if($pc -lt 5){R 'FAIL' $msg}elseif($pc -lt 15){R 'WARN' $msg}else{R 'OK' $msg} };
$bad=@(Get-PhysicalDisk -ErrorAction SilentlyContinue | Where-Object {$_.HealthStatus -ne 'Healthy'});
if($bad.Count -gt 0){R 'FAIL' ('Disk health problem on '+$bad.Count+' drive(s)')}else{R 'OK' 'All physical disks report Healthy'};
'== NETWORK ==';
$ad=@(Get-NetAdapter -ErrorAction SilentlyContinue | Where-Object Status -eq 'Up');
if($ad.Count -gt 0){R 'OK' ('Active adapters: '+(($ad | ForEach-Object Name) -join ', '))}else{R 'FAIL' 'No active network adapter'};
if(Test-Connection 8.8.8.8 -Count 2 -Quiet -ErrorAction SilentlyContinue){R 'OK' 'Internet reachable (8.8.8.8)'}else{R 'FAIL' 'Cannot reach the internet (8.8.8.8)'};
try{Resolve-DnsName microsoft.com -ErrorAction Stop | Out-Null; R 'OK' 'DNS resolution works'}catch{R 'FAIL' 'DNS resolution failed'};
'== SECURITY ==';
foreach($fw in (Get-NetFirewallProfile -ErrorAction SilentlyContinue)){ if($fw.Enabled){R 'OK' ('Firewall enabled: '+$fw.Name)}else{R 'WARN' ('Firewall disabled: '+$fw.Name)} };
try{$d=Get-MpComputerStatus -ErrorAction Stop; if($d.RealTimeProtectionEnabled){R 'OK' 'Defender real-time protection is on'}else{R 'WARN' 'Defender real-time protection is off'}}catch{R 'WARN' 'Defender status unavailable (third-party antivirus?)'};
'== DEVICES ==';
$pd=@(Get-PnpDevice -ErrorAction SilentlyContinue | Where-Object {$_.Status -eq 'Error'});
if($pd.Count -gt 0){R 'WARN' ($pd.Count.ToString()+' device(s) reporting errors')}else{R 'OK' 'No device errors'};
'== SUMMARY ==';
'Passed: '+$global:OKc+'    Warnings: '+$global:WNc+'    Failed: '+$global:FLc
";

        const string FULL_NET = @"
'[1] Network adapters'; Get-NetAdapter | Where-Object Status -eq 'Up' | Format-Table Name,InterfaceDescription,LinkSpeed -Auto;
'[2] Default gateway'; $g=(Get-NetRoute -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue | Sort-Object RouteMetric | Select-Object -First 1).NextHop; if($g){ 'Gateway: '+$g; ping -n 2 $g } else { 'No default gateway detected.' };
'[3] Internet (8.8.8.8)'; ping -n 2 8.8.8.8;
'[4] DNS lookup'; try{Resolve-DnsName www.microsoft.com -ErrorAction Stop | Format-Table Name,Type,IPAddress -Auto}catch{'DNS lookup failed: '+$_.Exception.Message};
'[5] HTTPS'; try{$r=Invoke-WebRequest -Uri 'https://www.microsoft.com' -Method Head -UseBasicParsing -TimeoutSec 10; 'HTTP status: '+$r.StatusCode}catch{'HTTPS test failed: '+$_.Exception.Message};
'== TEST COMPLETE =='
";

        static Catalog()
        {
            // ---------------- DASHBOARD
            P("Dashboard", "Quick IT Health Check", "Scan system, storage, network, security and devices", HEALTH);
            L("Dashboard", "Open Toolkit Folder", "Open the folder with your reports and logs", "{BASE}");

            // ---------------- PC INFORMATION
            C("PC Information", "Complete System Information", "Full systeminfo report", "systeminfo");
            P("PC Information", "Operating System", "Windows edition, build, install and boot dates", "Get-CimInstance Win32_OperatingSystem | Select Caption,Version,BuildNumber,OSArchitecture,InstallDate,LastBootUpTime | Format-List");
            P("PC Information", "Computer / Manufacturer", "Make, model, system type and RAM", "Get-CimInstance Win32_ComputerSystem | Select Manufacturer,Model,SystemType,@{N='RAM_GB';E={[math]::Round($_.TotalPhysicalMemory/1GB,2)}} | Format-List");
            P("PC Information", "CPU Information", "Processor name, cores, threads and speed", "Get-CimInstance Win32_Processor | Select Name,Manufacturer,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed | Format-List");
            P("PC Information", "RAM Information", "Installed memory modules", "Get-CimInstance Win32_PhysicalMemory | Select Manufacturer,PartNumber,@{N='CapacityGB';E={[math]::Round($_.Capacity/1GB,2)}},Speed | Format-Table -Auto");
            P("PC Information", "BIOS Information", "BIOS vendor, version and serial number", "Get-CimInstance Win32_BIOS | Select Manufacturer,SMBIOSBIOSVersion,SerialNumber,ReleaseDate | Format-List");
            C("PC Information", "Windows Activation", "Check Windows activation expiry", @"cscript //nologo ""%windir%\system32\slmgr.vbs"" /xpr");
            C("PC Information", "Environment Variables", "List all environment variables", "set");
            P("PC Information", "Installed Software", "Every installed program with version", @"Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue | Where-Object DisplayName | Sort-Object DisplayName | Select DisplayName,DisplayVersion,Publisher | Format-Table -Auto");
            P("PC Information", "Installed Updates (Hotfixes)", "Most recent 30 installed Windows updates", "Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 30 HotFixID,Description,InstalledOn,InstalledBy | Format-Table -Auto");

            // ---------------- HARDWARE
            P("Hardware", "CPU Details", "Complete processor properties", "Get-CimInstance Win32_Processor | Format-List *");
            P("Hardware", "RAM Details", "Memory banks, parts and speed", "Get-CimInstance Win32_PhysicalMemory | Select BankLabel,Manufacturer,PartNumber,@{N='CapacityGB';E={[math]::Round($_.Capacity/1GB,2)}},Speed | Format-Table -Auto");
            P("Hardware", "Disk Health", "Health and status of physical disks", "Get-PhysicalDisk | Select FriendlyName,MediaType,HealthStatus,OperationalStatus,@{N='SizeGB';E={[math]::Round($_.Size/1GB,2)}} | Format-Table -Auto");
            P("Hardware", "Battery Report", "Create and open a battery health report", @"$f='{REPORTS}\battery-report.html'; powercfg /batteryreport /output $f; if(Test-Path $f){'Battery report saved to: '+$f; Start-Process $f}");
            P("Hardware", "Plug and Play Devices", "All detected hardware devices", "Get-PnpDevice | Sort Status,FriendlyName | Format-Table Status,Class,FriendlyName,InstanceId -Auto");
            P("Hardware", "Problem Devices", "Devices that are not working correctly", "$d=@(Get-PnpDevice | Where-Object {$_.Status -ne 'OK'}); if($d.Count -eq 0){'No problem devices found.'}else{$d | Format-Table Status,Class,FriendlyName,InstanceId -Auto}");
            L("Hardware", "System Information (GUI)", "Open Microsoft System Information", "msinfo32.exe");
            L("Hardware", "DirectX Diagnostic", "Open DirectX Diagnostic Tool", "dxdiag.exe");
            L("Hardware", "Memory Diagnostic", "Schedule a Windows memory test", "mdsched.exe");

            // ---------------- NETWORK
            C("Network", "IP Configuration", "Full ipconfig /all output", "ipconfig /all");
            P("Network", "Network Adapters", "Adapters, status, speed and MAC address", "Get-NetAdapter | Format-Table Name,InterfaceDescription,Status,LinkSpeed,MacAddress -Auto");
            P("Network", "Default Gateway", "Show default route and gateway", "Get-NetRoute -DestinationPrefix '0.0.0.0/0' | Select ifIndex,NextHop,RouteMetric | Format-Table -Auto");
            P("Network", "DNS Configuration", "DNS servers used by each adapter", "Get-DnsClientServerAddress | Format-Table -Auto");
            C("Network", "DNS Cache", "Display the local DNS resolver cache", "ipconfig /displaydns");
            C("Network", "ARP Table", "Show known IP to MAC mappings", "arp -a");
            C("Network", "Routing Table", "Show the full routing table", "route print");
            C("Network", "Active Connections", "Open connections with process IDs", "netstat -ano");
            P("Network", "Listening Ports", "Ports waiting for connections, with process", "Get-NetTCPConnection -State Listen | Select LocalAddress,LocalPort,OwningProcess,@{N='Process';E={(Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue).Name}} | Sort LocalPort | Format-Table -Auto");
            P("Network", "Ping Gateway", "Test connection to your router", "$g=(Get-NetRoute -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue | Sort-Object RouteMetric | Select-Object -First 1).NextHop; if($g){'Gateway: '+$g; ping -n 4 $g}else{'Default gateway could not be detected.'}");
            C("Network", "Ping Internet", "Ping Google DNS (8.8.8.8)", "ping -n 4 8.8.8.8");
            C("Network", "Ping Custom Host", "Ping any host name or IP address", "ping -n 4 {0}").Input("Enter host name or IP address:", "8.8.8.8");
            C("Network", "Traceroute", "Trace the route to a host", "tracert {0}").Input("Enter host name or IP address:", "8.8.8.8");
            C("Network", "Flush DNS", "Clear the DNS resolver cache", "ipconfig /flushdns");
            C("Network", "Winsock Reset", "Reset the Winsock catalog", "netsh winsock reset").NeedAdmin().Ask("This resets the Winsock catalog. A restart may be required afterwards.");
            C("Network", "TCP/IP Reset", "Reset the TCP/IP stack", "netsh int ip reset").NeedAdmin().Ask("This resets the TCP/IP stack. A restart may be required afterwards.");

            // ---------------- INTERNET
            P("Internet", "Check Network Adapter", "Status and speed of adapters", "Get-NetAdapter | Format-Table Name,Status,LinkSpeed -Auto");
            P("Internet", "Test Default Gateway", "Ping the router", "$g=(Get-NetRoute -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue | Sort-Object RouteMetric | Select-Object -First 1).NextHop; if($g){'Gateway: '+$g; ping -n 4 $g}else{'No default gateway detected.'}");
            P("Internet", "Test DNS", "Resolve a well-known domain name", "try{Resolve-DnsName www.google.com -ErrorAction Stop | Format-Table Name,Type,IPAddress -Auto; 'DNS is working.'}catch{'DNS lookup failed: '+$_.Exception.Message}");
            C("Internet", "Test Internet Connectivity", "Ping two public DNS servers", "ping -n 3 8.8.8.8 && ping -n 3 1.1.1.1");
            P("Internet", "Test Website", "Check HTTPS response of a website", "try{$r=Invoke-WebRequest -Uri ('https://'+'{0}') -Method Head -UseBasicParsing -TimeoutSec 10; 'HTTP status: '+$r.StatusCode+' '+$r.StatusDescription}catch{'Website test failed: '+$_.Exception.Message}").Input("Enter website (e.g. www.microsoft.com):", "www.microsoft.com");
            C("Internet", "NSLOOKUP", "Query DNS for a host name", "nslookup {0}").Input("Enter host name:", "www.google.com");
            C("Internet", "Renew IP Address", "Release and renew the IP lease", "ipconfig /release && ipconfig /renew").NeedAdmin().Ask("This releases and renews your IP address. Your connection will drop for a few seconds.");
            C("Internet", "Flush DNS Cache", "Clear the DNS resolver cache", "ipconfig /flushdns");
            P("Internet", "Full Internet Test", "Adapter, gateway, internet, DNS and HTTPS", FULL_NET);

            // ---------------- WINDOWS REPAIR
            C("Windows Repair", "SFC Scan", "Scan and repair protected system files", "sfc /scannow").NeedAdmin();
            C("Windows Repair", "DISM CheckHealth", "Quick check for image corruption", "DISM /Online /Cleanup-Image /CheckHealth").NeedAdmin();
            C("Windows Repair", "DISM ScanHealth", "Deep scan of the Windows image", "DISM /Online /Cleanup-Image /ScanHealth").NeedAdmin();
            C("Windows Repair", "DISM RestoreHealth", "Repair the Windows image (takes a while)", "DISM /Online /Cleanup-Image /RestoreHealth").NeedAdmin().Ask("RestoreHealth can take 10-30 minutes and needs an internet connection.");
            C("Windows Repair", "CHKDSK Online Scan", "Scan drive C: for errors (no restart)", "chkdsk C: /scan").NeedAdmin();
            C("Windows Repair", "Component Cleanup", "Clean up superseded Windows components", "DISM /Online /Cleanup-Image /StartComponentCleanup").NeedAdmin();
            L("Windows Repair", "Windows Troubleshooter", "Open Windows troubleshooters", "ms-settings:troubleshoot");
            L("Windows Repair", "Event Viewer", "Open Windows Event Viewer", "eventvwr.msc");
            P("Windows Repair", "Recent System Errors", "Critical and error events from the last 24 hours", @"$e=@(Get-WinEvent -FilterHashtable @{LogName='System';Level=1,2;StartTime=(Get-Date).AddDays(-1)} -MaxEvents 40 -ErrorAction SilentlyContinue); if($e.Count -eq 0){'No critical or error events in the last 24 hours.'}else{$e | Select TimeCreated,ProviderName,Id,@{N='Message';E={($_.Message -split [Environment]::NewLine)[0]}} | Format-Table -Wrap -Auto}");
            C("Windows Repair", "Reset Windows Update Components", "Stop services, clear update cache, restart",
              @"net stop wuauserv & net stop bits & net stop cryptsvc & net stop msiserver & ren ""%windir%\SoftwareDistribution"" SoftwareDistribution.old 2>nul & ren ""%windir%\System32\catroot2"" catroot2.old 2>nul & net start wuauserv & net start bits & net start cryptsvc & net start msiserver")
              .NeedAdmin().Ask("This stops the update services, renames the update cache folders and restarts the services.");

            // ---------------- DISK & STORAGE
            P("Disk & Storage", "Volume Information", "Drive letters, file system, size and free space", "Get-Volume | Where-Object DriveLetter | Select DriveLetter,FileSystemLabel,FileSystem,@{N='SizeGB';E={[math]::Round($_.Size/1GB,2)}},@{N='FreeGB';E={[math]::Round($_.SizeRemaining/1GB,2)}} | Format-Table -Auto");
            P("Disk & Storage", "Physical Disk Health", "Health status of every physical disk", "Get-PhysicalDisk | Select FriendlyName,MediaType,HealthStatus,OperationalStatus,@{N='SizeGB';E={[math]::Round($_.Size/1GB,2)}} | Format-Table -Auto");
            L("Disk & Storage", "Disk Management", "Open Disk Management", "diskmgmt.msc");
            C("Disk & Storage", "CHKDSK", "Scan drive C: for errors", "chkdsk C: /scan").NeedAdmin();
            L("Disk & Storage", "Optimize Drives", "Open Defragment and Optimize Drives", "dfrgui.exe");
            P("Disk & Storage", "Clean Temporary Files", "Delete user and Windows temp files",
              @"$t=0; foreach($p in @($env:TEMP,(Join-Path $env:windir 'Temp'))){ if(Test-Path $p){ Get-ChildItem $p -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {-not $_.PSIsContainer} | ForEach-Object { try{ $s=$_.Length; Remove-Item $_.FullName -Force -ErrorAction Stop; $t+=$s }catch{} } } }; 'Temporary files removed. Space freed: {0:N1} MB' -f ($t/1MB)")
              .Ask("This permanently deletes files in your Temp folder and C:\\Windows\\Temp. Files in use are skipped.");
            P("Disk & Storage", "Empty Recycle Bin", "Permanently empty the Recycle Bin", "Clear-RecycleBin -Force -ErrorAction SilentlyContinue; 'Recycle Bin emptied.'").Ask("This permanently deletes everything in the Recycle Bin.");
            L("Disk & Storage", "Open Temp Folder", "Browse your user temp folder", "%TEMP%");
            L("Disk & Storage", "Storage Settings", "Open Storage Sense settings", "ms-settings:storagesense");

            // ---------------- PERFORMANCE
            L("Performance", "Task Manager", "Open Task Manager", "taskmgr.exe");
            C("Performance", "Process List", "All running processes", "tasklist");
            P("Performance", "Top CPU Processes", "15 busiest processes by CPU time", "Get-Process | Sort-Object CPU -Descending | Select -First 15 Name,Id,@{N='CPU_s';E={[math]::Round($_.CPU,1)}} | Format-Table -Auto");
            P("Performance", "Top Memory Processes", "15 largest processes by memory", "Get-Process | Sort-Object WorkingSet64 -Descending | Select -First 15 Name,Id,@{N='MemoryMB';E={[math]::Round($_.WorkingSet64/1MB,1)}} | Format-Table -Auto");
            L("Performance", "Performance Monitor", "Open Performance Monitor", "perfmon.exe");
            L("Performance", "Resource Monitor", "Open Resource Monitor", "resmon.exe");
            L("Performance", "Startup Apps", "Manage apps that start with Windows", "ms-settings:startupapps");
            P("Performance", "Startup Programs List", "Programs configured to run at sign-in", "Get-CimInstance Win32_StartupCommand | Select Name,Command,Location,User | Format-Table -Wrap -Auto");
            P("Performance", "System Uptime", "Last boot time and how long ago", "$os=Get-CimInstance Win32_OperatingSystem; 'Last boot : '+$os.LastBootUpTime; $u=(Get-Date)-$os.LastBootUpTime; 'Uptime    : {0} days, {1} hours, {2} minutes' -f $u.Days,$u.Hours,$u.Minutes");

            // ---------------- SERVICES
            L("Services", "Services Console", "Open the Services management console", "services.msc");
            P("Services", "Running Services", "All running services", "Get-Service | Where Status -eq 'Running' | Sort DisplayName | Format-Table Status,Name,DisplayName -Auto");
            P("Services", "Stopped Services", "All stopped services", "Get-Service | Where Status -eq 'Stopped' | Sort DisplayName | Format-Table Status,Name,DisplayName -Auto");
            P("Services", "Search Service", "Find a service by name", "Get-Service -ErrorAction SilentlyContinue | Where-Object {$_.Name -like '*{0}*' -or $_.DisplayName -like '*{0}*'} | Format-Table Status,Name,DisplayName -Auto").Input("Enter part of a service name:", "").AllowSpaces();
            P("Services", "Start Service", "Start a service by its name", "try{Start-Service -Name '{0}' -ErrorAction Stop; Get-Service -Name '{0}' | Format-Table Status,Name,DisplayName -Auto}catch{'Failed: '+$_.Exception.Message}").Input("Service name (e.g. Spooler):", "").NeedAdmin();
            P("Services", "Stop Service", "Stop a service by its name", "try{Stop-Service -Name '{0}' -ErrorAction Stop; Get-Service -Name '{0}' | Format-Table Status,Name,DisplayName -Auto}catch{'Failed: '+$_.Exception.Message}").Input("Service name (e.g. Spooler):", "").NeedAdmin().Ask("Stopping a service can affect running programs.");
            P("Services", "Restart Service", "Restart a service by its name", "try{Restart-Service -Name '{0}' -ErrorAction Stop; Get-Service -Name '{0}' | Format-Table Status,Name,DisplayName -Auto}catch{'Failed: '+$_.Exception.Message}").Input("Service name (e.g. Spooler):", "").NeedAdmin();

            // ---------------- USERS & ACCOUNTS
            C("Users & Accounts", "Current User", "Who you are and your privileges", "whoami && whoami /all");
            C("Users & Accounts", "Local Users", "List local user accounts", "net user");
            C("Users & Accounts", "Local Administrators", "Members of the Administrators group", "net localgroup administrators");
            L("Users & Accounts", "Computer Management", "Open Computer Management", "compmgmt.msc");
            L("Users & Accounts", "User Accounts", "Open the User Accounts dialog", "netplwiz.exe");
            C("Users & Accounts", "Password Policy", "Show password and lockout policy", "net accounts");
            C("Users & Accounts", "Logged-on Users", "Users signed in to this PC", "query user");

            // ---------------- DRIVERS & DEVICES
            L("Drivers & Devices", "Device Manager", "Open Device Manager", "devmgmt.msc");
            P("Drivers & Devices", "Installed Drivers", "Driver names, versions and dates", "Get-CimInstance Win32_PnPSignedDriver | Select DeviceName,DriverVersion,DriverDate,Manufacturer | Sort DeviceName | Format-Table -Auto");
            P("Drivers & Devices", "Problem Device List", "Devices with errors", "$d=@(Get-PnpDevice | Where-Object {$_.Status -ne 'OK'}); if($d.Count -eq 0){'No problem devices found.'}else{$d | Format-Table Status,Class,FriendlyName,InstanceId -Auto}");
            P("Drivers & Devices", "All PnP Devices", "Every Plug and Play device", "Get-PnpDevice | Format-Table Status,Class,FriendlyName -Auto");
            L("Drivers & Devices", "Driver Verifier", "Open Driver Verifier Manager", "verifier.exe");
            L("Drivers & Devices", "DirectX Diagnostic Tool", "Open DirectX Diagnostic Tool", "dxdiag.exe");
            C("Drivers & Devices", "Driver Store", "List drivers in the Windows driver store", "pnputil /enum-drivers");

            // ---------------- WINDOWS UPDATE
            L("Windows Update", "Windows Update", "Open Windows Update settings", "ms-settings:windowsupdate");
            L("Windows Update", "Update History", "View installed updates", "ms-settings:windowsupdate-history");
            L("Windows Update", "Advanced Update Settings", "Open advanced update options", "ms-settings:windowsupdate-options");
            C("Windows Update", "Windows Update Service", "Query the Windows Update service", "sc query wuauserv");
            C("Windows Update", "Restart Update Service", "Restart the Windows Update service", "net stop wuauserv & net start wuauserv").NeedAdmin();
            C("Windows Update", "Reset Update Components", "Full reset of update components",
              @"net stop wuauserv & net stop bits & net stop cryptsvc & net stop msiserver & ren ""%windir%\SoftwareDistribution"" SoftwareDistribution.old 2>nul & ren ""%windir%\System32\catroot2"" catroot2.old 2>nul & net start wuauserv & net start bits & net start cryptsvc & net start msiserver")
              .NeedAdmin().Ask("This stops the update services, renames the update cache folders and restarts the services.");

            // ---------------- SECURITY
            L("Security", "Windows Security", "Open the Windows Security app", "windowsdefender:");
            C("Security", "Firewall Status", "Show status of all firewall profiles", "netsh advfirewall show allprofiles");
            P("Security", "Firewall Profiles", "Profile state and default actions", "Get-NetFirewallProfile | Format-Table Name,Enabled,DefaultInboundAction,DefaultOutboundAction -Auto");
            P("Security", "Firewall Rules", "All firewall rules", "Get-NetFirewallRule | Select DisplayName,Enabled,Direction,Action,Profile | Format-Table -Auto");
            P("Security", "Defender Status", "Antivirus and real-time protection state", "try{Get-MpComputerStatus -ErrorAction Stop | Select AntivirusEnabled,AntispywareEnabled,RealTimeProtectionEnabled,AMServiceEnabled,AntivirusSignatureLastUpdated | Format-List}catch{'Defender status unavailable: '+$_.Exception.Message}");
            P("Security", "Defender Quick Scan", "Run a Microsoft Defender quick scan", "Start-MpScan -ScanType QuickScan; 'Quick scan finished.'").NeedAdmin();
            P("Security", "Update Defender Signatures", "Download the latest virus definitions", "Update-MpSignature; 'Signatures updated.'").NeedAdmin();
            L("Security", "Advanced Firewall", "Open Windows Firewall with Advanced Security", "wf.msc");
            C("Security", "Enable Firewall", "Turn the firewall on for all profiles", "netsh advfirewall set allprofiles state on").NeedAdmin().Ask("This turns the Windows Firewall ON for all profiles.");
            C("Security", "Disable Firewall", "Turn the firewall off (not recommended)", "netsh advfirewall set allprofiles state off").NeedAdmin().Ask("WARNING: This turns the Windows Firewall OFF for all profiles and lowers your protection.");

            // ---------------- WI-FI
            C("Wi-Fi", "Wi-Fi Interface", "Current Wi-Fi connection details", "netsh wlan show interfaces");
            C("Wi-Fi", "Available Networks", "Nearby Wi-Fi networks and access points", "netsh wlan show networks mode=bssid");
            C("Wi-Fi", "Saved Wi-Fi Profiles", "Networks this PC has connected to", "netsh wlan show profiles");
            C("Wi-Fi", "Wi-Fi Driver", "Wireless adapter driver details", "netsh wlan show drivers");
            P("Wi-Fi", "Signal Information", "SSID, signal strength, channel and speed", "netsh wlan show interfaces | Select-String 'SSID|BSSID|Signal|Radio|Channel|Receive|Transmit|State'");
            P("Wi-Fi", "WLAN Report", "Create and open the Wi-Fi history report", @"netsh wlan show wlanreport; $f='C:\ProgramData\Microsoft\Windows\WlanReport\wlan-report-latest.html'; if(Test-Path $f){Start-Process $f}").NeedAdmin();

            // ---------------- REPORTS
            P("Reports", "System Report", "Save a system report to a text file", Report("System-Report", "ULTIMATE IT SUPPORT TOOLKIT - SYSTEM REPORT", SEC_SYS));
            P("Reports", "Network Report", "Save a network report to a text file", Report("Network-Report", "ULTIMATE IT SUPPORT TOOLKIT - NETWORK REPORT", SEC_NET));
            P("Reports", "Hardware Report", "Save a hardware report to a text file", Report("Hardware-Report", "ULTIMATE IT SUPPORT TOOLKIT - HARDWARE REPORT", SEC_HW));
            P("Reports", "Full Diagnostic Report", "System, network, hardware and security in one file", Report("Full-Diagnostic-Report", "ULTIMATE IT SUPPORT TOOLKIT - FULL DIAGNOSTIC REPORT", SEC_SYS + SEC_NET + SEC_HW + SEC_SEC));
            L("Reports", "Open Reports Folder", "Browse saved reports", "{REPORTS}");
            L("Reports", "Open Logs Folder", "Browse activity logs", "{LOGS}");
            P("Reports", "View Activity Log", "Last 200 actions run from this toolkit", "if(Test-Path '{LOGS}\\toolkit-activity.log'){Get-Content '{LOGS}\\toolkit-activity.log' -Tail 200}else{'No activity recorded yet.'}");

            // ---------------- SYSTEM TOOLS
            L("System Tools", "System Configuration", "Open msconfig", "msconfig.exe");
            L("System Tools", "Event Viewer ", "Open Event Viewer", "eventvwr.msc");
            L("System Tools", "Computer Management ", "Open Computer Management", "compmgmt.msc");
            L("System Tools", "Device Manager ", "Open Device Manager", "devmgmt.msc");
            L("System Tools", "Disk Management ", "Open Disk Management", "diskmgmt.msc");
            L("System Tools", "Services ", "Open Services", "services.msc");
            L("System Tools", "Task Scheduler", "Open Task Scheduler", "taskschd.msc");
            L("System Tools", "Local Security Policy", "Open Local Security Policy", "secpol.msc");
            L("System Tools", "Registry Editor", "Open Registry Editor", "regedit.exe");
            L("System Tools", "Group Policy", "Open Group Policy Editor", "gpedit.msc");
            L("System Tools", "System Information", "Open System Information", "msinfo32.exe");
            L("System Tools", "Control Panel", "Open Control Panel", "control.exe");
            L("System Tools", "Command Prompt", "Open a Command Prompt window", "cmd.exe");
            L("System Tools", "PowerShell", "Open a PowerShell window", "powershell.exe");

            // ---------------- POWER
            C("Power", "Restart Computer", "Restart in 30 seconds", @"shutdown /r /t 30 /c ""Restart initiated by IT Support Toolkit - HASIF KHAN""").Ask("The computer will restart in 30 seconds. Save your work first. You can cancel with 'Cancel Pending Shutdown'.");
            C("Power", "Shutdown Computer", "Shut down in 30 seconds", @"shutdown /s /t 30 /c ""Shutdown initiated by IT Support Toolkit - HASIF KHAN""").Ask("The computer will shut down in 30 seconds. Save your work first. You can cancel with 'Cancel Pending Shutdown'.");
            C("Power", "Restart to BIOS / UEFI", "Reboot directly into firmware settings", "shutdown /r /fw /t 0").Ask("The computer will restart IMMEDIATELY into BIOS / UEFI firmware settings.");
            C("Power", "Sign Out", "Sign out the current user", "shutdown /l").Ask("You will be signed out immediately. Unsaved work will be lost.");
            C("Power", "Cancel Pending Shutdown", "Abort a scheduled restart or shutdown", "shutdown /a");
        }
    }

    // ------------------------------------------------------------ CUSTOM CONTROLS
    class NavButton : Control
    {
        public bool Selected;
        bool hover;
        public NavButton(string text)
        {
            Text = text;
            Height = Theme.S(40);
            Width = Theme.S(222);
            Margin = Padding.Empty;
            Cursor = Cursors.Hand;
            Font = Theme.F(10f, FontStyle.Regular);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Color bg = Selected ? Theme.NavSel : (hover ? Theme.NavHover : Theme.Navy);
            using (SolidBrush b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, ClientRectangle);
            if (Selected)
                using (SolidBrush b2 = new SolidBrush(Theme.AccentLight)) e.Graphics.FillRectangle(b2, 0, 0, Theme.S(4), Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(Theme.S(24), 0, Width - Theme.S(30), Height),
                Selected ? Color.White : Theme.NavText, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }

    class StatCard : Panel
    {
        public string Title = "", Value = "--", Sub = "";
        public int Percent = -1;
        public Color AccentColor;
        public StatCard(string title, Color accent)
        {
            Title = title; AccentColor = accent;
            Size = new Size(Theme.S(262), Theme.S(118));
            Margin = new Padding(Theme.S(8));
            DoubleBuffered = true;
            BackColor = Theme.Bg;
        }
        public void Set(string value, string sub, int pct)
        {
            Value = value; Sub = sub; Percent = pct; Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath gp = Gfx.Round(r, Theme.S(10)))
            {
                using (SolidBrush b = new SolidBrush(Color.White)) g.FillPath(b, gp);
                using (Pen p = new Pen(Theme.Border)) g.DrawPath(p, gp);
            }
            int pad = Theme.S(16);
            TextRenderer.DrawText(g, Title, Theme.Tag75, new Rectangle(pad, Theme.S(12), Width - 2 * pad, Theme.S(16)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, Value, Theme.Big20, new Rectangle(pad, Theme.S(30), Width - 2 * pad, Theme.S(36)), Theme.Text, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, Sub, Theme.Small85, new Rectangle(pad, Theme.S(70), Width - 2 * pad, Theme.S(16)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            if (Percent >= 0)
            {
                Rectangle bar = new Rectangle(pad, Height - Theme.S(22), Width - 2 * pad, Theme.S(8));
                using (GraphicsPath tp = Gfx.Round(bar, Theme.S(4)))
                using (SolidBrush tb = new SolidBrush(Color.FromArgb(226, 232, 240))) g.FillPath(tb, tp);
                int w = (int)(bar.Width * Math.Min(100, Percent) / 100.0);
                if (w > Theme.S(8))
                {
                    Color c = Percent >= 90 ? Theme.Red : (Percent >= 75 ? Theme.Amber : AccentColor);
                    Rectangle fill = new Rectangle(bar.X, bar.Y, w, bar.Height);
                    using (GraphicsPath fp = Gfx.Round(fill, Theme.S(4)))
                    using (SolidBrush fb = new SolidBrush(c)) g.FillPath(fb, fp);
                }
            }
        }
    }

    class InfoPanel : Panel
    {
        public List<string[]> Rows = new List<string[]>();
        public InfoPanel()
        {
            Size = new Size(Theme.S(560), Theme.S(270));
            Margin = new Padding(Theme.S(8));
            DoubleBuffered = true;
            BackColor = Theme.Bg;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath gp = Gfx.Round(r, Theme.S(10)))
            {
                using (SolidBrush b = new SolidBrush(Color.White)) g.FillPath(b, gp);
                using (Pen p = new Pen(Theme.Border)) g.DrawPath(p, gp);
            }
            int pad = Theme.S(18);
            TextRenderer.DrawText(g, "SYSTEM OVERVIEW", Theme.Tag75, new Rectangle(pad, Theme.S(14), Width, Theme.S(16)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int colW = (Width - 2 * pad) / 2;
            int rowH = Theme.S(54);
            for (int i = 0; i < Rows.Count; i++)
            {
                int col = i % 2, row = i / 2;
                int x = pad + col * colW, y = Theme.S(42) + row * rowH;
                TextRenderer.DrawText(g, Rows[i][0], Theme.Small85, new Rectangle(x, y, colW - Theme.S(10), Theme.S(16)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, Rows[i][1], Theme.Bold95, new Rectangle(x, y + Theme.S(18), colW - Theme.S(10), Theme.S(22)), Theme.Text, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }

    class ToolCard : Panel
    {
        public Tool Tool;
        bool hover;
        public ToolCard(Tool t)
        {
            Tool = t;
            Size = new Size(Theme.S(268), Theme.S(104));
            Margin = new Padding(Theme.S(8));
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            BackColor = Theme.Bg;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
            using (GraphicsPath gp = Gfx.Round(r, Theme.S(10)))
            {
                using (SolidBrush b = new SolidBrush(hover ? Color.FromArgb(247, 250, 255) : Color.White)) g.FillPath(b, gp);
                using (Pen p = new Pen(hover ? Theme.Accent : Theme.Border, hover ? 2f : 1f)) g.DrawPath(p, gp);
            }
            int pad = Theme.S(16);
            TextRenderer.DrawText(g, Tool.Name.Trim(), Theme.Bold105, new Rectangle(pad, Theme.S(12), Width - 2 * pad, Theme.S(22)), Theme.Text, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, Tool.Desc, Theme.Regular9, new Rectangle(pad, Theme.S(38), Width - 2 * pad, Theme.S(36)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            string act = Tool.Kind == "L" ? "Open  \u203A" : "Run  \u203A";
            TextRenderer.DrawText(g, act, Theme.Bold9, new Rectangle(pad, Height - Theme.S(28), Theme.S(90), Theme.S(18)), Theme.Accent, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            if (Tool.Admin)
            {
                Rectangle tag = new Rectangle(Width - pad - Theme.S(54), Height - Theme.S(28), Theme.S(54), Theme.S(18));
                using (GraphicsPath tp = Gfx.Round(tag, Theme.S(8)))
                using (SolidBrush tb = new SolidBrush(Color.FromArgb(254, 243, 199))) g.FillPath(tb, tp);
                TextRenderer.DrawText(g, "ADMIN", Theme.Tag75, tag, Color.FromArgb(146, 64, 14), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }

    class SectionLabel : Label
    {
        public SectionLabel(string text)
        {
            Text = text;
            AutoSize = false;
            Size = new Size(Theme.S(600), Theme.S(34));
            Margin = new Padding(Theme.S(8), Theme.S(8), Theme.S(8), 0);
            Font = Theme.F(12f, FontStyle.Bold);
            ForeColor = Theme.Text;
            TextAlign = ContentAlignment.BottomLeft;
            BackColor = Theme.Bg;
        }
    }

    static class UI
    {
        public static Button Btn(string text, Color back, Color fore, int w, int h)
        {
            Button b = new Button();
            b.Text = text; b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = back; b.ForeColor = fore; b.Size = new Size(w, h);
            b.Cursor = Cursors.Hand; b.Font = Theme.F(9f, FontStyle.Bold); b.UseVisualStyleBackColor = false;
            return b;
        }
    }

    static class InputDialog
    {
        public static string Show(IWin32Window owner, string title, string prompt, string def)
        {
            using (Form f = new Form())
            {
                f.Text = title; f.FormBorderStyle = FormBorderStyle.FixedDialog; f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false; f.MinimizeBox = false; f.ShowInTaskbar = false; f.BackColor = Color.White;
                f.Font = Theme.F(9.5f, FontStyle.Regular);
                f.ClientSize = new Size(Theme.S(440), Theme.S(150));
                Label l = new Label(); l.Text = prompt; l.AutoSize = false; l.SetBounds(Theme.S(20), Theme.S(18), Theme.S(400), Theme.S(22));
                TextBox t = new TextBox(); t.Text = def ?? ""; t.SetBounds(Theme.S(20), Theme.S(48), Theme.S(400), Theme.S(26));
                Button ok = UI.Btn("OK", Theme.Accent, Color.White, Theme.S(90), Theme.S(32)); ok.Location = new Point(Theme.S(230), Theme.S(98)); ok.DialogResult = DialogResult.OK;
                Button cancel = UI.Btn("Cancel", Color.FromArgb(226, 232, 240), Theme.Text, Theme.S(90), Theme.S(32)); cancel.Location = new Point(Theme.S(330), Theme.S(98)); cancel.DialogResult = DialogResult.Cancel;
                f.Controls.Add(l); f.Controls.Add(t); f.Controls.Add(ok); f.Controls.Add(cancel);
                f.AcceptButton = ok; f.CancelButton = cancel;
                t.SelectAll();
                return f.ShowDialog(owner) == DialogResult.OK ? t.Text : null;
            }
        }
    }

    // ------------------------------------------------------------ MAIN FORM
    class MainForm : Form
    {
        readonly bool isAdmin = Sys.IsAdmin();
        readonly List<NavButton> navs = new List<NavButton>();
        FlowLayoutPanel flow;
        SplitContainer split;
        RichTextBox rtb;
        Label lblTitle, lblSub, lblStatus, lblAdmin;
        TextBox txtSearch;
        Button btnAdmin, btnStop, btnCopy, btnSave, btnClear;
        ProgressBar busy;
        Panel header, outHeader;
        Process current;
        bool running, suppressSearch;
        string currentCat = "Dashboard";
        System.Windows.Forms.Timer timer;
        PerformanceCounter cpuCounter;
        StatCard cCpu, cRam, cDisk, cUp;
        InfoPanel info;
        readonly List<FlowLayoutPanel> sections = new List<FlowLayoutPanel>();

        public MainForm()
        {
            Text = "Ultimate IT Support Toolkit - Professional Edition";
            Font = Theme.Regular9;
            BackColor = Theme.Bg;
            StartPosition = FormStartPosition.CenterScreen;
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(Theme.S(1260), wa.Width - 40), Math.Min(Theme.S(800), wa.Height - 60));
            MinimumSize = new Size(Math.Min(Theme.S(1080), wa.Width - 20), Math.Min(Theme.S(640), wa.Height - 40));
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            BuildUi();
            Load += delegate { ShowCategory("Dashboard"); WelcomeText(); };
            Shown += delegate
            {
                try { split.Panel1MinSize = Theme.S(180); split.Panel2MinSize = Theme.S(150); split.SplitterDistance = (int)(split.Height * 0.60); } catch { }
                StartTimer();
            };
            FormClosing += delegate { KillCurrent(); };
        }

        // ---------------- UI construction
        void BuildUi()
        {
            // ---- main area
            Panel main = new Panel(); main.Dock = DockStyle.Fill;

            split = new SplitContainer();
            split.Dock = DockStyle.Fill; split.Orientation = Orientation.Horizontal;
            split.SplitterWidth = Theme.S(6); split.BackColor = Theme.Border;
            split.Panel1.BackColor = Theme.Bg; split.Panel2.BackColor = Theme.ConsoleBg;

            flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill; flow.AutoScroll = true; flow.WrapContents = true;
            flow.FlowDirection = FlowDirection.LeftToRight; flow.BackColor = Theme.Bg;
            flow.Padding = new Padding(Theme.S(14), Theme.S(8), Theme.S(8), Theme.S(8));
            flow.Resize += delegate { ResizeSections(); };
            split.Panel1.Controls.Add(flow);

            // output console
            rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill; rtb.ReadOnly = true; rtb.BorderStyle = BorderStyle.None;
            rtb.BackColor = Theme.ConsoleBg; rtb.ForeColor = Theme.ConsoleFg;
            rtb.Font = new Font("Consolas", 9.5f); rtb.WordWrap = false; rtb.DetectUrls = false;
            rtb.ScrollBars = RichTextBoxScrollBars.Both;
            split.Panel2.Controls.Add(rtb);

            outHeader = new Panel(); outHeader.Dock = DockStyle.Top; outHeader.Height = Theme.S(42); outHeader.BackColor = Theme.Navy;
            Label ot = new Label(); ot.Text = "OUTPUT CONSOLE"; ot.ForeColor = Color.White; ot.Font = Theme.F(9f, FontStyle.Bold);
            ot.AutoSize = true; ot.Location = new Point(Theme.S(16), Theme.S(12)); outHeader.Controls.Add(ot);
            lblStatus = new Label(); lblStatus.Text = "Ready"; lblStatus.ForeColor = Theme.NavText; lblStatus.Font = Theme.Small85;
            lblStatus.AutoSize = true; lblStatus.Location = new Point(Theme.S(150), Theme.S(13)); outHeader.Controls.Add(lblStatus);
            busy = new ProgressBar(); busy.Style = ProgressBarStyle.Marquee; busy.MarqueeAnimationSpeed = 30;
            busy.Size = new Size(Theme.S(110), Theme.S(8)); busy.Visible = false; outHeader.Controls.Add(busy);

            Color btnBack = Color.FromArgb(51, 65, 85);
            btnCopy = UI.Btn("Copy", btnBack, Color.White, Theme.S(64), Theme.S(28)); btnCopy.Click += delegate { CopyOutput(); };
            btnSave = UI.Btn("Save", btnBack, Color.White, Theme.S(64), Theme.S(28)); btnSave.Click += delegate { SaveOutput(); };
            btnClear = UI.Btn("Clear", btnBack, Color.White, Theme.S(64), Theme.S(28)); btnClear.Click += delegate { rtb.Clear(); };
            btnStop = UI.Btn("Stop", Theme.Red, Color.White, Theme.S(64), Theme.S(28)); btnStop.Enabled = false; btnStop.Click += delegate { KillCurrent(); };
            outHeader.Controls.Add(btnCopy); outHeader.Controls.Add(btnSave); outHeader.Controls.Add(btnClear); outHeader.Controls.Add(btnStop);
            outHeader.Resize += delegate { LayoutOutHeader(); };
            split.Panel2.Controls.Add(outHeader);
            LayoutOutHeader();

            // header bar
            header = new Panel(); header.Dock = DockStyle.Top; header.Height = Theme.S(76); header.BackColor = Color.White;
            header.Paint += delegate(object s, PaintEventArgs e) { using (Pen p = new Pen(Theme.Border)) e.Graphics.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1); };
            lblTitle = new Label(); lblTitle.UseMnemonic = false; lblTitle.AutoSize = true; lblTitle.Font = Theme.F(16f, FontStyle.Bold); lblTitle.ForeColor = Theme.Text;
            lblTitle.Location = new Point(Theme.S(24), Theme.S(10)); header.Controls.Add(lblTitle);
            lblSub = new Label(); lblSub.UseMnemonic = false; lblSub.AutoSize = true; lblSub.Font = Theme.Regular9; lblSub.ForeColor = Theme.Muted;
            lblSub.Location = new Point(Theme.S(26), Theme.S(44)); header.Controls.Add(lblSub);

            txtSearch = new TextBox(); txtSearch.Width = Theme.S(230); txtSearch.Font = Theme.F(10f, FontStyle.Regular);
            txtSearch.TextChanged += delegate { OnSearch(); };
            header.Controls.Add(txtSearch);
            Label sl = new Label(); sl.Name = "lblSearch"; sl.Text = "Search tools:"; sl.AutoSize = true; sl.ForeColor = Theme.Muted; sl.Font = Theme.Small85;
            header.Controls.Add(sl);

            if (isAdmin)
            {
                lblAdmin = new Label(); lblAdmin.Text = "\u25CF  ADMINISTRATOR"; lblAdmin.ForeColor = Theme.Green; lblAdmin.Font = Theme.F(9f, FontStyle.Bold);
                lblAdmin.AutoSize = true; header.Controls.Add(lblAdmin);
            }
            else
            {
                btnAdmin = UI.Btn("Run as Administrator", Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14), Theme.S(170), Theme.S(32));
                btnAdmin.Click += delegate { RestartAdmin(); };
                header.Controls.Add(btnAdmin);
            }
            header.Resize += delegate { LayoutHeader(); };
            LayoutHeader();

            main.Controls.Add(split);
            main.Controls.Add(header);

            // ---- sidebar
            Panel sidebar = new Panel(); sidebar.Dock = DockStyle.Left; sidebar.Width = Theme.S(240); sidebar.BackColor = Theme.Navy;

            FlowLayoutPanel nav = new FlowLayoutPanel();
            nav.Dock = DockStyle.Fill; nav.FlowDirection = FlowDirection.TopDown; nav.WrapContents = false;
            nav.AutoScroll = true; nav.BackColor = Theme.Navy; nav.Padding = Padding.Empty;
            foreach (string c in Catalog.Cats)
            {
                string cat = c;
                NavButton nb = new NavButton(cat);
                nb.Click += delegate { suppressSearch = true; txtSearch.Text = ""; suppressSearch = false; ShowCategory(cat); };
                navs.Add(nb); nav.Controls.Add(nb);
            }

            Panel brand = new Panel(); brand.Dock = DockStyle.Top; brand.Height = Theme.S(92); brand.BackColor = Theme.Navy;
            brand.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle logo = new Rectangle(Theme.S(22), Theme.S(26), Theme.S(40), Theme.S(40));
                using (GraphicsPath gp = Gfx.Round(logo, Theme.S(10))) using (SolidBrush b = new SolidBrush(Theme.Accent)) g.FillPath(b, gp);
                TextRenderer.DrawText(g, "IT", Theme.F(13f, FontStyle.Bold), logo, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, "IT SUPPORT TOOLKIT", Theme.F(10.5f, FontStyle.Bold), new Rectangle(Theme.S(72), Theme.S(26), Theme.S(166), Theme.S(22)), Color.White, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, "PROFESSIONAL EDITION", Theme.Tag75, new Rectangle(Theme.S(72), Theme.S(48), Theme.S(166), Theme.S(18)), Theme.AccentLight, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };

            Panel foot = new Panel(); foot.Dock = DockStyle.Bottom; foot.Height = Theme.S(56); foot.BackColor = Theme.Navy;
            foot.Paint += delegate(object s, PaintEventArgs e)
            {
                TextRenderer.DrawText(e.Graphics, "v3.2.1  |  Hasif Khan", Theme.Small85, new Rectangle(Theme.S(24), Theme.S(8), Theme.S(200), Theme.S(18)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(e.Graphics, "\u00A9 2026 All Rights Reserved", Theme.Small85, new Rectangle(Theme.S(24), Theme.S(28), Theme.S(200), Theme.S(18)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };

            sidebar.Controls.Add(nav);
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(foot);

            Controls.Add(main);
            Controls.Add(sidebar);
        }

        void LayoutHeader()
        {
            int right = header.Width - Theme.S(24);
            Control rightMost = isAdmin ? (Control)lblAdmin : (Control)btnAdmin;
            rightMost.Location = new Point(right - rightMost.Width, isAdmin ? Theme.S(30) : Theme.S(22));
            right = rightMost.Left - Theme.S(24);
            txtSearch.Location = new Point(right - txtSearch.Width, Theme.S(26));
            Control sl = header.Controls["lblSearch"];
            if (sl != null) sl.Location = new Point(txtSearch.Left - sl.Width - Theme.S(6), Theme.S(30));
        }

        void LayoutOutHeader()
        {
            int right = outHeader.Width - Theme.S(12);
            Button[] bs = new Button[] { btnStop, btnClear, btnSave, btnCopy };
            foreach (Button b in bs) { right -= b.Width; b.Location = new Point(right, Theme.S(7)); right -= Theme.S(6); }
            busy.Location = new Point(right - busy.Width - Theme.S(8), Theme.S(17));
        }

        // ---------------- pages
        void ClearFlow()
        {
            cCpu = cRam = cDisk = cUp = null; info = null; sections.Clear();
            List<Control> old = new List<Control>();
            foreach (Control c in flow.Controls) old.Add(c);
            flow.Controls.Clear();
            foreach (Control c in old) c.Dispose();
        }

        void ToolCardClick(object sender, EventArgs e)
        {
            ToolCard tc = sender as ToolCard;
            if (tc != null) RunTool(tc.Tool);
        }

        void AddCard(Tool t)
        {
            if (t == null) return;
            ToolCard c = new ToolCard(t); c.Click += ToolCardClick; flow.Controls.Add(c);
        }

        void SelectNav(string cat)
        {
            foreach (NavButton n in navs) { n.Selected = (n.Text == cat); n.Invalidate(); }
        }

        void ShowCategory(string cat)
        {
            currentCat = cat;
            SelectNav(cat);
            flow.SuspendLayout();
            ClearFlow();
            lblTitle.Text = cat;
            if (cat == "Dashboard")
            {
                lblSub.Text = "Live system status and quick actions";
                BuildDashboard();
            }
            else
            {
                List<Tool> tools = Catalog.InCat(cat);
                lblSub.Text = tools.Count + " tools available. Click a card to run it.";
                foreach (Tool t in tools) AddCard(t);
            }
            flow.ResumeLayout();
            flow.AutoScrollPosition = new Point(0, 0);
        }

        void OnSearch()
        {
            if (suppressSearch) return;
            string q = txtSearch.Text.Trim();
            if (q.Length < 2) { ShowCategory(currentCat); return; }
            SelectNav("");
            flow.SuspendLayout();
            ClearFlow();
            lblTitle.Text = "Search results";
            int n = 0;
            foreach (Tool t in Catalog.All)
            {
                if (t.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || t.Desc.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || t.Cat.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                { AddCard(t); n++; }
            }
            lblSub.Text = n + " tool(s) match \"" + q + "\"";
            flow.ResumeLayout();
        }

        void ResizeSections()
        {
            int w = Math.Max(Theme.S(300), flow.ClientSize.Width - flow.Padding.Horizontal - Theme.S(8));
            foreach (FlowLayoutPanel sec in sections) sec.MaximumSize = new Size(w, 0);
        }

        FlowLayoutPanel NewSection()
        {
            FlowLayoutPanel sec = new FlowLayoutPanel();
            sec.AutoSize = true; sec.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            sec.WrapContents = true; sec.FlowDirection = FlowDirection.LeftToRight;
            sec.BackColor = Theme.Bg; sec.Margin = Padding.Empty; sec.Padding = Padding.Empty;
            sections.Add(sec);
            return sec;
        }

        void BuildDashboard()
        {
            FlowLayoutPanel col = new FlowLayoutPanel();
            col.FlowDirection = FlowDirection.TopDown; col.WrapContents = false;
            col.AutoSize = true; col.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            col.BackColor = Theme.Bg; col.Margin = Padding.Empty;
            flow.Controls.Add(col);

            FlowLayoutPanel s1 = NewSection();
            cCpu = new StatCard("CPU USAGE", Theme.Accent);
            cRam = new StatCard("MEMORY USAGE", Color.FromArgb(124, 58, 237));
            cDisk = new StatCard("SYSTEM DRIVE", Color.FromArgb(8, 145, 178));
            cUp = new StatCard("UPTIME", Theme.Green);
            s1.Controls.Add(cCpu); s1.Controls.Add(cRam); s1.Controls.Add(cDisk); s1.Controls.Add(cUp);
            col.Controls.Add(s1);

            col.Controls.Add(new SectionLabel("Quick Actions"));
            FlowLayoutPanel s2 = NewSection();
            string[] quick = new string[] { "Quick IT Health Check", "Full Internet Test", "Clean Temporary Files", "Flush DNS", "Full Diagnostic Report", "Top CPU Processes" };
            foreach (string qn in quick)
            {
                Tool qt = Catalog.Find(qn);
                if (qt == null) continue;
                ToolCard qc = new ToolCard(qt); qc.Click += ToolCardClick; s2.Controls.Add(qc);
            }
            col.Controls.Add(s2);

            col.Controls.Add(new SectionLabel("System Overview"));
            info = new InfoPanel();
            int memPct; double tot, used; Sys.Memory(out memPct, out tot, out used);
            info.Rows.Add(new string[] { "Computer name", Environment.MachineName });
            info.Rows.Add(new string[] { "Signed-in user", Environment.UserDomainName + "\\" + Environment.UserName });
            info.Rows.Add(new string[] { "Operating system", Sys.OsName() });
            info.Rows.Add(new string[] { "Processor", Sys.CpuName() });
            info.Rows.Add(new string[] { "Installed memory", tot.ToString("0.0") + " GB" });
            info.Rows.Add(new string[] { "IP address", Sys.IPv4() });
            info.Rows.Add(new string[] { "Privileges", isAdmin ? "Administrator" : "Standard user" });
            info.Rows.Add(new string[] { "Reports & logs folder", Paths.Base });
            col.Controls.Add(info);

            ResizeSections();
            UpdateStats();
        }

        // ---------------- live stats
        void StartTimer()
        {
            try { cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"); cpuCounter.NextValue(); } catch { cpuCounter = null; }
            timer = new System.Windows.Forms.Timer(); timer.Interval = 1500;
            timer.Tick += delegate { UpdateStats(); };
            timer.Start();
        }

        void UpdateStats()
        {
            if (cCpu == null || cCpu.IsDisposed) return;
            try
            {
                if (cpuCounter != null) { int c = (int)Math.Round(cpuCounter.NextValue()); cCpu.Set(c + "%", "Total processor load", c); }
                else cCpu.Set("N/A", "Counter unavailable", -1);

                int mp; double tot, used; Sys.Memory(out mp, out tot, out used);
                cRam.Set(mp + "%", used.ToString("0.0") + " GB of " + tot.ToString("0.0") + " GB used", mp);

                try
                {
                    DriveInfo d = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
                    double totD = d.TotalSize / 1073741824.0, freeD = d.AvailableFreeSpace / 1073741824.0;
                    int dp = (int)Math.Round((totD - freeD) / totD * 100);
                    cDisk.Set(dp + "%", freeD.ToString("0.0") + " GB free of " + totD.ToString("0.0") + " GB", dp);
                }
                catch { cDisk.Set("N/A", "", -1); }

                TimeSpan up = Sys.Uptime();
                cUp.Set(Sys.FormatUptime(up), "Since " + DateTime.Now.Subtract(up).ToString("dd MMM, HH:mm"), -1);
            }
            catch { }
        }

        // ---------------- running tools
        void RestartAdmin()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath);
                psi.UseShellExecute = true; psi.Verb = "runas";
                Process.Start(psi);
                KillCurrent();
                Application.Exit();
            }
            catch (Win32Exception) { /* user cancelled UAC */ }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Could not restart", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void RunTool(Tool t)
        {
            if (running)
            {
                MessageBox.Show(this, "Another task is still running. Please wait for it to finish or press Stop.", "Task in progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (t.Admin && !isAdmin)
            {
                DialogResult r = MessageBox.Show(this,
                    "\"" + t.Name.Trim() + "\" needs Administrator rights and may fail without them.\n\nYes = restart the toolkit as Administrator\nNo = try anyway\nCancel = go back",
                    "Administrator required", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (r == DialogResult.Yes) { RestartAdmin(); return; }
                if (r == DialogResult.Cancel) return;
            }
            string input = null;
            if (t.Prompt != null)
            {
                input = InputDialog.Show(this, t.Name.Trim(), t.Prompt, t.Default);
                if (input == null) return;
                input = input.Trim();
                if (input.Length == 0) input = t.Default ?? "";
                string rx = t.Spaces ? @"^[A-Za-z0-9._:*\- ]{1,100}$" : @"^[A-Za-z0-9._:*\-]{1,100}$";
                if (!Regex.IsMatch(input, rx))
                {
                    MessageBox.Show(this, "Please enter a valid value (letters, numbers, dots and dashes only).", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (t.Confirm != null)
            {
                if (MessageBox.Show(this, t.Confirm + "\n\nDo you want to continue?", t.Name.Trim(), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            }
            Execute(t, input);
        }

        static string Expand(string s)
        {
            return s.Replace("{REPORTS}", Paths.Reports).Replace("{LOGS}", Paths.Logs).Replace("{BASE}", Paths.Base);
        }

        void Execute(Tool t, string input)
        {
            string cmd = Expand(t.Cmd);
            if (input != null) cmd = cmd.Replace("{0}", input);
            string name = t.Name.Trim();

            if (t.Kind == "L")
            {
                try
                {
                    ProcessStartInfo ps = new ProcessStartInfo(Environment.ExpandEnvironmentVariables(cmd));
                    ps.UseShellExecute = true;
                    Process.Start(ps);
                    Line(">> Opened: " + name, Theme.AccentLight);
                    Paths.Log("Opened - " + name);
                }
                catch (Exception ex) { Line("!! Could not open " + name + ": " + ex.Message, Theme.RedLight); }
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false; psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true; psi.RedirectStandardError = true; psi.RedirectStandardInput = true;
            if (t.Kind == "P")
            {
                string script = "$ErrorActionPreference='Continue';$ProgressPreference='SilentlyContinue';try{[Console]::OutputEncoding=[Text.Encoding]::UTF8}catch{};"
                    + "& {" + cmd + "\r\n} 2>&1 | Out-String -Stream -Width 200";
                psi.FileName = "powershell.exe";
                psi.Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                psi.StandardOutputEncoding = Encoding.UTF8; psi.StandardErrorEncoding = Encoding.UTF8;
            }
            else
            {
                Encoding oem;
                try { oem = Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage); } catch { oem = Encoding.Default; }
                psi.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
                psi.Arguments = "/d /c " + cmd;
                psi.StandardOutputEncoding = oem; psi.StandardErrorEncoding = oem;
            }

            Line("", Theme.ConsoleFg);
            Line("=== " + name + "  [" + DateTime.Now.ToString("HH:mm:ss") + "] ===", Theme.AccentLight);

            Process p = new Process(); p.StartInfo = psi;
            p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null) Post(e.Data, false); };
            p.ErrorDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null) Post(e.Data, true); };
            try { p.Start(); }
            catch (Exception ex) { Line("!! Failed to start: " + ex.Message, Theme.RedLight); return; }
            try { p.StandardInput.Close(); } catch { }
            p.BeginOutputReadLine(); p.BeginErrorReadLine();
            current = p;
            SetRunning(true, "Running: " + name);
            DateTime started = DateTime.Now;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                int code = -1;
                try { p.WaitForExit(); code = p.ExitCode; } catch { }
                Done(name, started, code);
            });
        }

        void Post(string text, bool err)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(new Action(delegate { OutLine(text, err); })); } catch { }
        }

        void Done(string name, DateTime started, int code)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(delegate
                {
                    double secs = (DateTime.Now - started).TotalSeconds;
                    Line("--- finished in " + secs.ToString("0.0") + "s (exit code " + code + ") ---", code == 0 ? Theme.GreenLight : Theme.Amber);
                    Paths.Log(name + " (exit " + code + ")");
                    current = null;
                    SetRunning(false, "Ready");
                }));
            }
            catch { }
        }

        void SetRunning(bool on, string status)
        {
            running = on; busy.Visible = on; btnStop.Enabled = on; lblStatus.Text = status;
            Cursor = on ? Cursors.AppStarting : Cursors.Default;
        }

        void KillCurrent()
        {
            try
            {
                if (current != null && !current.HasExited)
                {
                    ProcessStartInfo k = new ProcessStartInfo("taskkill", "/PID " + current.Id + " /T /F");
                    k.CreateNoWindow = true; k.UseShellExecute = false;
                    Process.Start(k);
                    Line("!! Stopped by user.", Theme.Amber);
                }
            }
            catch { }
        }

        // ---------------- output console
        void OutLine(string text, bool err)
        {
            text = text.Replace("\0", "").TrimEnd('\r');
            Color c = Theme.ConsoleFg;
            if (err) c = Theme.RedLight;
            else if (text.StartsWith("[OK]")) c = Theme.GreenLight;
            else if (text.StartsWith("[WARN]")) c = Theme.Amber;
            else if (text.StartsWith("[FAIL]")) c = Theme.RedLight;
            else if (text.StartsWith("==")) c = Theme.AccentLight;
            Line(text, c);
        }

        void Line(string text, Color c)
        {
            if (rtb.TextLength > 3000000) rtb.Clear();
            rtb.SelectionStart = rtb.TextLength; rtb.SelectionLength = 0;
            rtb.SelectionColor = c;
            rtb.AppendText(text + "\n");
            rtb.SelectionStart = rtb.TextLength;
            rtb.ScrollToCaret();
        }

        void WelcomeText()
        {
            Line("ULTIMATE IT SUPPORT TOOLKIT  -  Professional Edition", Theme.AccentLight);
            Line("Developed & maintained by HASIF KHAN  |  Project HK-ITST-2026-001", Theme.Muted);
            Line("", Theme.ConsoleFg);
            Line("Choose a category on the left, then click a tool card. Results appear here.", Theme.ConsoleFg);
            Line("Tools marked ADMIN need administrator rights. Reports and logs are saved to:", Theme.ConsoleFg);
            Line("  " + Paths.Base, Theme.ConsoleFg);
        }

        void CopyOutput()
        {
            if (rtb.TextLength == 0) return;
            try { Clipboard.SetText(rtb.Text); lblStatus.Text = "Output copied to clipboard"; } catch { }
        }

        void SaveOutput()
        {
            if (rtb.TextLength == 0) return;
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.InitialDirectory = Paths.Reports;
                d.FileName = "Output_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
                d.Filter = "Text file (*.txt)|*.txt";
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    try { File.WriteAllText(d.FileName, rtb.Text, Encoding.UTF8); lblStatus.Text = "Saved: " + d.FileName; } catch (Exception ex) { MessageBox.Show(this, ex.Message); }
                }
            }
        }
    }

    // ------------------------------------------------------------ ENTRY POINT
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) Theme.Scale = g.DpiX / 96f; } catch { }
            Paths.Ensure();
            Paths.Log("Toolkit started");
            Application.Run(new MainForm());
        }
    }
}
