using System.Text;

namespace Modact.API
{
    internal class CmdAPI
    {
        public CmdAPI()
        {

        }
        public void RegisterAPI(WebApplication app)
        {
            app.MapGet("/cmd/{command}", async (HttpContext context, string command, IHostApplicationLifetime applife) =>
            {
                IHostApplicationLifetime hostApplicationLifetime = applife;
                string result = Modact.AppInfo.AppName + " " + Modact.API.AppInfo.AppVersionString + Environment.NewLine;
                string requestIP = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                Log.Information(requestIP + " sent command: " + command.ToLower(), true);

                if (command.ToLower() == "ipconfig")
                {
                    try
                    {
                        result = "Your IP: " + context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                        //var ipList = MachineInfo.GetAllLocalIPv4();
                        //if (ipList.Count() > 0)
                        //{
                        //    foreach ( var ip in ipList )
                        //    {
                        //        result += Environment.NewLine + "Machine IP: " + ip;
                        //    }                
                        //}
                    }
                    catch (Exception e)
                    {
                        Log.Error("/cmd/ipconfig error." + Environment.NewLine + e.ToString());
                    }
                }
                if (command.ToLower() == "shutdown")
                {
                    try
                    {
                        if (requestIP == "0.0.0.1" || requestIP == "127.0.0.1")
                        {
                            applife.StopApplication();
                            result = "Shutdown success.";
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("/cmd/shutdown error." + Environment.NewLine + e.ToString());
                    }
                }
                await context.Response.WriteAsync(result);
            });
        }
    }
}