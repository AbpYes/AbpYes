using System;
using System.Linq;
using AbpYes.BaseServer;
using AbpYes.BaseServer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.File("Logs/logs.txt"))
    .WriteTo.Async(c => c.Console())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

try
{
    Log.Information($"开始启动{configuration["App:Name"]}服务...");

    builder.Host.AddAppSettingsSecretsJson()
        .UseAutofac()
        .UseSerilog();

    await builder.AddApplicationAsync<AbpYesBaseServerHttpApiHostModule>();
    var app = builder.Build();
    await app.InitializeApplicationAsync();

    if (IsMigrationDatabase(args))
    {
        await app.Services.GetRequiredService<AbpYesBaseServerDbMigrationService>().MigrateAsync();
    }

    await app.RunAsync();
}
catch
{
    Log.Fatal($"启动{configuration["App:Name"]}服务失败!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

bool IsMigrationDatabase(string[] args)
{
    return args.Any(o => o.Contains("--migrate-database", StringComparison.OrdinalIgnoreCase));
}