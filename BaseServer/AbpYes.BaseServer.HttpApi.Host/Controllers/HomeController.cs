using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace AbpYes.BaseServer.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HomeController : AbpController
{
    [HttpGet]
    public string HealthState()
    {
        return $"AbpYes.BaseServer正常运行中...";
    }
}