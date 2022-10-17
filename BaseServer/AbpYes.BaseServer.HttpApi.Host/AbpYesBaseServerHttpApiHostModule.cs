using System;
using System.Collections.Generic;
using System.Linq;
using AbpYes.BaseServer.MultiTenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace AbpYes.BaseServer;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpYesBaseServerApplicationModule),
    typeof(AbpYesBaseServerEntityFrameworkCoreModule),
    typeof(AbpYesBaseServerHttpApiModule)
)]
public class AbpYesBaseServerHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureCors(context, configuration);
        ConfigureConventionalControllers();
        ConfigureAuthentication(context, configuration);
        ConfigureSwaggerServices(context, configuration);
    }


    /// <summary>
    /// 配置身份认证
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configuration"></param>
    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "AbpYes";
            });
    }


    /// <summary>
    /// 配置Swagger 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configuration"></param>
    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"],
            new Dictionary<string, string>
            {
                { "AbpYes.BaseServer", "AbpYes.BaseServer API" }
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AbpYes.BaseServer API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }


    /// <summary>
    /// 配置自动约定控制器
    /// </summary>
    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AbpYesBaseServerApplicationModule).Assembly);
        });
    }


    /// <summary>
    /// 配置跨域
    /// </summary>
    /// <param name="context"></param>
    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(configuration["App:CorsOrigins"]
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.GetConfiguration();

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", configuration["App:Name"]);
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("AbpYes");
        });

        // app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseUnitOfWork();
        app.UseConfiguredEndpoints();
    }
}