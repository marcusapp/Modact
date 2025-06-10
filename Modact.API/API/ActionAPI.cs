using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modact.API
{
    internal class ActionAPI
    {
        public ActionAPI() { }

        public void RegisterAPI(WebApplication app)
        {
            app.MapPost("/action", async (HttpContext context) =>
            {
                ApiConnect? apiConnect = null;
                apiConnect = await apiConnect.NewProcess(app, context);
            });

            app.MapGet("/action", async (HttpContext context) =>
            {
                var apiRequest = new ApiRequest();
                apiRequest.RequestId = Ulid.NewUlid().ToString();
                apiRequest.RequestTime = DateTime.Now;
                apiRequest.IsGetSession = false;
                var reqInfo = new RequestInfo();
                reqInfo.AppId = Ulid.NewUlid().ToString();
                reqInfo.AppName = "DEMO";
                reqInfo.AppVersion = "1.0";
                reqInfo.AppSecret = Ulid.NewUlid().ToString();
                reqInfo.MachineName = "PC01";
                reqInfo.MacAddress = "00-00-00-00-00-00";
                reqInfo.MachineCode = Ulid.NewUlid().ToString();
                apiRequest.RequestInfo = reqInfo;
                apiRequest.Functions = new();
                for (int i = 1; i < 3; i++)
                {
                    var fun = new ApiFunction();
                    fun.Id = "f" + i.ToString();
                    fun.Fun = "Dll/Class/Function" + i.ToString();
                    fun.Params = new();
                    fun.Params.Add("param1", "value " + i.ToString());
                    fun.Params.Add("param2", "value " + i.ToString());
                    apiRequest.Functions.Add(fun);
                }
                apiRequest.Actions = new();
                for (int j = 1; j < 3; j++)
                {
                    var action = new ApiAction();
                    var functions = new List<ApiFunction>();
                    for (int i = 1; i < 3; i++)
                    {
                        var fun = new ApiFunction();
                        fun.Id = "a" + j.ToString() + i.ToString();
                        fun.Fun = "Dll/Class/Function" + j.ToString() + i.ToString();
                        fun.Params = new();
                        fun.Params.Add("param1", "value " + j.ToString() + i.ToString());
                        fun.Params.Add("param2", "value " + j.ToString() + i.ToString());
                        functions.Add(fun);
                    }
                    if (j == 2)
                    {
                        action.IsTrans = false;
                    }
                    action.Functions = functions;
                    apiRequest.Actions.Add(action);
                }
                await context.Response.WriteAsJsonAsync(apiRequest, new AppSettings(app.Configuration).GetJsonSerializerOptions());
            });
        }
    }
}
