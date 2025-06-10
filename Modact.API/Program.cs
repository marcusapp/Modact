using Modact;
using Modact.API;
using Modact.Data.Cache;

Modact.AppInfo.AppStartTime = DateTime.Now;

Log.Information($":+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+: :+:");
Log.Information($"{Modact.AppInfo.AppName}.API {Modact.API.AppInfo.AppVersionString} (PID:{Modact.AppInfo.ProcessId}) (TID:{Modact.AppInfo.ThreadId})", true);
Log.Information($"{Modact.AppInfo.AppName} {Modact.AppInfo.AppVersionString}", true);
Log.Information($"Program starting, path={Modact.AppInfo.AppPath}", true);
Log.Information($"MachineName={Modact.AppInfo.MachineName}, HostName={Modact.AppInfo.MachineNameWithDomain}", true);

#region builder create
var builder = WebApplication.CreateBuilder(args);
Log.Information("Web application create builder", true);

AppSettings.LogModact = new(builder.Configuration.GetSection("ModactConfig:LogOption:LogModactOption:LogToFilePath").Value, "modact");
Log.IsWriteToFile = builder.Configuration.GetSection("ModactConfig:LogOption:LogModactOption").GetValue<bool>("LogToFileEnable");
if (Log.IsWriteToFile)
{
    Log.Information("AppSettings LogModact enable, logging to path=" + builder.Configuration.GetSection("ModactConfig:LogOption:LogModactOption").GetValue<string>("LogToFilePath"), true);
}
else
{
    Log.Information("AppSettings LogModact disabled", true);
}

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Listen(IPAddress.Any, 442, listenOptions =>
//    {

//    });

//    serverOptions.ConfigureHttpsDefaults(listenOptions =>
//    {

//    });
//});

builder.Services.AddCors(c =>
{
    c.AddPolicy(name: "allCors",
       policy =>
       {
           policy
           .SetIsOriginAllowed((host) => true)
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
       });
});
Log.Information("Builder services add cors policy: \"allCors\"", true);

builder.Services.AddScoped<ActionAPI>();
Log.Information("Builder services add scoped: <ActionAPI>", true);

builder.Services.AddScoped<CmdAPI>();
Log.Information("Builder services add scoped: <CmdAPI>", true);

builder.Services.AddScoped<SessionAPI>();
Log.Information("Builder services add scoped: <SessionAPI>", true);

builder.Services.AddScoped<UlidAPI>();
Log.Information("Builder services add scoped: <UlidAPI>", true);

#endregion

#region app build
var app = builder.Build();
Log.Information("Builder build app", true);

app.UseCors("allCors");
Log.Information("App use cors: \"allCors\"", true);

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetService<ActionAPI>();
    service.RegisterAPI(app);
}
Log.Information("App service create scope: <ActionAPI>", true);
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetService<CmdAPI>();
    service.RegisterAPI(app);
}
Log.Information("App service create scope: <CmdAPI>", true);
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetService<SessionAPI>();
    service.RegisterAPI(app);
}
Log.Information("App service create scope: <SessionAPI>", true);
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetService<UlidAPI>();
    service.RegisterAPI(app);
}
Log.Information("App service create scope: <UlidAPI>", true);

#endregion

#region app MapGet
app.MapGet("/", () => Modact.AppInfo.AppName + " " + Modact.API.AppInfo.AppVersionString + Environment.NewLine
    );
#endregion

#region app start
try
{
    await app.StartAsync();
    Log.Information($"App started, used {(DateTime.Now - Modact.AppInfo.AppStartTime).TotalSeconds}s, url={string.Join(", ", app.Urls)}", true);

    await app.WaitForShutdownAsync();
    Log.Warning("App terminated.");
}
catch (Exception e)
{
    Log.Fatal("App crashed: " + e.ToString());
}
finally
{
    Log.CloseAndFlush();
}

#endregion