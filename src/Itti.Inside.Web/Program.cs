using Itti.Inside.Application;
using Itti.Inside.Infrastructure;
using Itti.Inside.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddPortalWeb(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.Services.InitializePortalDatabaseAsync();

app.UsePortalPipeline();

await app.RunAsync();
