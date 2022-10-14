using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpHttpClientModule),
    typeof(AbpYesBaseServerApplicationContractsModule)
)]
public class AbpYesBaseServerHttpApiClientModule : AbpModule
{
}