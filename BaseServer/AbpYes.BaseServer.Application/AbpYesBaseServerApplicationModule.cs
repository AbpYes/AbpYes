using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpYesBaseServerDomainModule),
    typeof(AbpYesBaseServerApplicationContractsModule)
)]
public class AbpYesBaseServerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AbpYesBaseServerApplicationModule>(); });
    }
}