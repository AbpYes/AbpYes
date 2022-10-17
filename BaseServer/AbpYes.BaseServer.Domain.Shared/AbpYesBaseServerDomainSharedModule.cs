using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule)
)]
public class AbpYesBaseServerDomainSharedModule : AbpModule
{
}