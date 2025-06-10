using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace Modact
{
    public partial class AppInfo
    {
        public static string? AppCode
        {
            get { return "MODACT"; }
        }
        public static string? AppName
        {
            get { return "Modact"; }
        }
        public static string? AppVersionString
        {
            get
            {
                //Remember same to <VersionPrefix></VersionPrefix> in "Modact.csproj"
                return Assembly.GetExecutingAssembly().GetName().Version.ToString(); //for single file publish mode
                //return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; //product version - only work on DLL mode, in single file publish mode will throw error
                //return Assembly.GetExecutingAssembly().GetName().Version.ToString(); //assembly version  - only work on DLL mode, in single file publish mode will throw error
                //return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion; //file version - only work on DLL mode, in single file publish mode will throw error
            }
        }
        public static Version? AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public static string AppPath { get { try { return AppContext.BaseDirectory; } catch { return string.Empty; } } }
        public static DateTime AppStartTime { get; set; } = DateTime.Now;
        public static string MachineName { get { try { return Environment.MachineName; } catch { return string.Empty; } } }
        public static string MachineNameWithDomain { get { try { return Dns.GetHostName(); } catch { return string.Empty; } } }
        public static int ProcessId { get { return Process.GetCurrentProcess().Id; } }
        public static int ThreadId { get { return Thread.CurrentThread.ManagedThreadId; } }
        public static IEnumerable<string> GetAllLocalIPv4(NetworkInterfaceType? type = null)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((type == null || type == item.NetworkInterfaceType) && item.OperationalStatus == OperationalStatus.Up && item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList;
        }
        public static IEnumerable<string> GetAllMacAddress(NetworkInterfaceType? type = null)
        {
            List<string> macAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((type == null || type == item.NetworkInterfaceType) && item.OperationalStatus == OperationalStatus.Up && item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    macAddrList.Add(item.GetPhysicalAddress().ToString());
                }
            }
            return macAddrList;
        }
    }
}