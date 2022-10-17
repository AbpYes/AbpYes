using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpYesBaseServerApplicationContractsModule)
)]
public class AbpYesBaseServerHttpApiModule : AbpModule
{
}