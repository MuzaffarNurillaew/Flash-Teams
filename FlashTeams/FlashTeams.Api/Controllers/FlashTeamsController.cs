using Microsoft.AspNetCore.Mvc;

namespace FlashTeams.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FlashTeamsController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "FlashTeams is working!.";
    }
}
