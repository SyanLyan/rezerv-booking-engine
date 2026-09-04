using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Common;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get() =>
        OkResponse<object>(new { status = "healthy" });
}