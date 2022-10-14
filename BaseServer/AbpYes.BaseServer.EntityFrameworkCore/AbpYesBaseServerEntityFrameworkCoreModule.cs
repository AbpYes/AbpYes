using AbpYes.BaseServer.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpEntityFrameworkCoreMySQLModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpYesBaseServerDomainModule)
)]
public class AbpYesBaseServerEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureMySqlDbContext(context);
    }


    /// <summary>
    /// 配置MySql数据库上下文
    /// </summary>
    /// <param name="context"></param>
    private void ConfigureMySqlDbContext(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AbpYesBaseServerDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options => { options.UseMySQL(); });
    }
}