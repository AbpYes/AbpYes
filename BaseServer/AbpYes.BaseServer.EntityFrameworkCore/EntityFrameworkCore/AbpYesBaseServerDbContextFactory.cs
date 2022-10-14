using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace AbpYes.BaseServer.EntityFrameworkCore;

public class AbpYesBaseServerDbContextFactory : IDesignTimeDbContextFactory<AbpYesBaseServerDbContext>
{
    public AbpYesBaseServerDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<AbpYesBaseServerDbContext>()
            .UseMySql(configuration.GetConnectionString("Default"), ServerVersion.Parse("5.7"));

        builder.ReplaceService<IMigrationsModelDiffer, AbpYesMigrationsModelDiffer>();

        return new AbpYesBaseServerDbContext(builder.Options);
    }


    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AbpYes.BaseServer.HttpApi.Host/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}