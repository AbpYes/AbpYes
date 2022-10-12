using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule)
)]
public class AbpYesBaseServerDomainModule : AbpModule
{
}