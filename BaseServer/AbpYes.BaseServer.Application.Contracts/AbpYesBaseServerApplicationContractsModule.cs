using Volo.Abp.Modularity;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpYesBaseServerDomainSharedModule)
)]
public class AbpYesBaseServerApplicationContractsModule : AbpModule
{
}