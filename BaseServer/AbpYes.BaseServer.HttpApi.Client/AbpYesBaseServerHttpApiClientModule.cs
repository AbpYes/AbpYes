using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpHttpClientModule),
    typeof(AbpYesBaseServerApplicationContractsModule)
)]
public class AbpYesBaseServerHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";


    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(AbpYesBaseServerApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}