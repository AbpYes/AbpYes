using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace AbpYes.BaseServer.Openiddict;

/*
 *  创建Opendiddict相关的初始化种子数据
 */
public class OpeniddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IAbpApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IStringLocalizer<OpenIddictResponse> L;


    public OpeniddictDataSeedContributor(
        IConfiguration configuration,
        IAbpApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IPermissionDataSeeder permissionDataSeeder,
        IStringLocalizer<OpenIddictResponse> l
    )
    {
        _configuration = configuration;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _permissionDataSeeder = permissionDataSeeder;
        L = l;
    }


    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }


    /// <summary>
    /// 创建资源范围
    /// </summary>
    private async Task CreateScopesAsync()
    {
        if (await _scopeManager.FindByNameAsync("AbpYes") == null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "AbpYes",
                DisplayName = "AbpYes API",
                Resources =
                {
                    "AbpYes"
                }
            });
        }
    }


    /// <summary>
    /// 创建应用程序
    /// </summary>
    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string>
        {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            "AbpYes"
        };

        var configurationSection = _configuration.GetSection("OpenIddict:Applications");

        //AbpYes_Web Client
        var webClientId = configurationSection["AbpYes_Web:ClientId"];
        if (!webClientId.IsNullOrWhiteSpace())
        {
            var consoleAndAngularClientRootUrl = configurationSection["AbpYes_Web:RootUrl"]?.TrimEnd('/');
            await CreateApplicationAsync(
                name: webClientId,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "AbpYes_Web Client",
                secret: null,
                grantTypes: new List<string>
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken
                },
                scopes: commonScopes,
                redirectUri: consoleAndAngularClientRootUrl,
                clientUri: consoleAndAngularClientRootUrl,
                postLogoutRedirectUri: consoleAndAngularClientRootUrl
            );
        }
    }


    private async Task CreateApplicationAsync(
        [NotNull] string name,
        [NotNull] string type,
        [NotNull] string consentType,
        string displayName,
        string secret,
        List<string> grantTypes,
        List<string> scopes,
        string clientUri = null,
        string redirectUri = null,
        string postLogoutRedirectUri = null,
        List<string> permissions = null
    )
    {
        if (!string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Public,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["NoClientSecretCanBeSetForPublicApplications"]);
        }

        if (string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Confidential,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["TheClientSecretIsRequiredForConfidentialApplications"]);
        }

        if (!string.IsNullOrEmpty(name) && await _applicationManager.FindByClientIdAsync(name) != null)
        {
            throw new BusinessException(L["TheClientIdentifierIsAlreadyTakenByAnotherApplication"]);
        }

        var client = await _applicationManager.FindByClientIdAsync(name);
        if (client == null)
        {
            var application = new AbpApplicationDescriptor
            {
                ClientId = name,
                Type = type,
                ClientSecret = secret,
                ConsentType = consentType,
                DisplayName = displayName,
                ClientUri = clientUri,
            };

            Check.NotNullOrEmpty(grantTypes, nameof(grantTypes));
            Check.NotNullOrEmpty(scopes, nameof(scopes));

            if (new[] { OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.Implicit }.All(
                    grantTypes.Contains))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

                if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
                }
            }

            if (!redirectUri.IsNullOrWhiteSpace() || !postLogoutRedirectUri.IsNullOrWhiteSpace())
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
            }

            foreach (var grantType in grantTypes)
            {
                if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
                }

                if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                    grantType == OpenIddictConstants.GrantTypes.Implicit)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
                }

                if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                    grantType == OpenIddictConstants.GrantTypes.ClientCredentials ||
                    grantType == OpenIddictConstants.GrantTypes.Password ||
                    grantType == OpenIddictConstants.GrantTypes.RefreshToken ||
                    grantType == OpenIddictConstants.GrantTypes.DeviceCode)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
                }

                if (grantType == OpenIddictConstants.GrantTypes.ClientCredentials)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
                }

                if (grantType == OpenIddictConstants.GrantTypes.Implicit)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
                }

                if (grantType == OpenIddictConstants.GrantTypes.Password)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
                }

                if (grantType == OpenIddictConstants.GrantTypes.RefreshToken)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
                }

                if (grantType == OpenIddictConstants.GrantTypes.DeviceCode)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Device);
                }

                if (grantType == OpenIddictConstants.GrantTypes.Implicit)
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                    if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                    {
                        application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                        application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                    }
                }
            }

            var buildInScopes = new[]
            {
                OpenIddictConstants.Permissions.Scopes.Address,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Phone,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles
            };

            foreach (var scope in scopes)
            {
                if (buildInScopes.Contains(scope))
                {
                    application.Permissions.Add(scope);
                }
                else
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
                }
            }

            if (redirectUri != null)
            {
                if (!redirectUri.IsNullOrEmpty())
                {
                    if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException(L["InvalidRedirectUri", redirectUri]);
                    }

                    if (application.RedirectUris.All(x => x != uri))
                    {
                        application.RedirectUris.Add(uri);
                    }
                }
            }

            if (postLogoutRedirectUri != null)
            {
                if (!postLogoutRedirectUri.IsNullOrEmpty())
                {
                    if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var uri) ||
                        !uri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException(L["InvalidPostLogoutRedirectUri", postLogoutRedirectUri]);
                    }

                    if (application.PostLogoutRedirectUris.All(x => x != uri))
                    {
                        application.PostLogoutRedirectUris.Add(uri);
                    }
                }
            }

            if (permissions != null)
            {
                await _permissionDataSeeder.SeedAsync(
                    ClientPermissionValueProvider.ProviderName,
                    name,
                    permissions,
                    null
                );
            }

            await _applicationManager.CreateAsync(application);
        }
    }
}