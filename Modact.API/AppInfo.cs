global using Modact;

using System.Diagnostics;
using System.Reflection;

namespace Modact.API
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
                //To modify version in <VersionPrefix></VersionPrefix> in "Modact.API.csproj" by double click project in VS IDE or edit it in file system
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //return FileVersionInfo.GetVersionInfo(System.Environment.ProcessPath).ProductVersion; //product version in single file mode
                //return Assembly.GetExecutingAssembly().GetName().Version.ToString(); //assembly version 
                //return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion; //file version  
            }
        }
        //public static Version? AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public static string AppPath { get { try { return AppContext.BaseDirectory; } catch { return string.Empty; } } }
        public static int ProcessId { get { return Process.GetCurrentProcess().Id; } }
        public static int ThreadId { get { return Thread.CurrentThread.ManagedThreadId; } }
    }
}