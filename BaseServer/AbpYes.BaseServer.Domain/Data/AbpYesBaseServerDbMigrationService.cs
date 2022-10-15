using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace AbpYes.BaseServer.Data;

/*
 *  数据迁移服务
 *  Tips:
 *       为保证程序初始化能够正常运行，必须先执行此服务来应用相关默认数据。
 */
public class AbpYesBaseServerDbMigrationService : ITransientDependency
{
    public ILogger<AbpYesBaseServerDbMigrationService> Logger { get; }
    private readonly IDataSeeder _dataSeeder;


    public AbpYesBaseServerDbMigrationService(IDataSeeder dataSeeder)
    {
        _dataSeeder = dataSeeder;
        Logger = NullLogger<AbpYesBaseServerDbMigrationService>.Instance;
    }


    public async Task MigrateAsync()
    {
        Logger.LogInformation("开始执行数据库迁移...");
        await SeedDataAsync();
        Logger.LogInformation("数据库迁移完成...");
    }


    private async Task SeedDataAsync(Tenant tenant = null)
    {
        await _dataSeeder.SeedAsync(new DataSeedContext(tenant?.Id)
            .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName,
                IdentityDataSeedContributor.AdminEmailDefaultValue)
            .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName,
                IdentityDataSeedContributor.AdminPasswordDefaultValue)
        );
    }
}