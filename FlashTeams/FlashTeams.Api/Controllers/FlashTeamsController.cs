using Microsoft.AspNetCore.Mvc;

namespace FlashTeams.Api.Controllers;

[ApiController]
public class FlashTeamsController : ControllerBase
{
    [HttpGet("/")]
    public string Get()
    {
        return "FlashTeams is working!.";
    }
}