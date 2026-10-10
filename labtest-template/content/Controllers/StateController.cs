using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TemplateProject.Controllers;

// Optional demonstration API. It does not alter the default MVC pages.
[ApiController]
[Route("state")]
public class StateController(IConfiguration configuration) : ControllerBase
{
    private const string Key = "demo-value";
    private bool UsesSession => configuration["StateMode"] == "Session";

    [HttpGet]
    public IActionResult Get()
    {
        var value = UsesSession ? HttpContext.Session.GetString(Key) : Request.Cookies[Key];
        return Ok(new { mode = configuration["StateMode"], value });
    }

    [HttpPost]
    public IActionResult Set([FromBody] StateValue input)
    {
        if (UsesSession)
        {
            HttpContext.Session.SetString(Key, input.Value);
        }
        else
        {
            Response.Cookies.Append(Key, input.Value, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(30),
                IsEssential = true
            });
        }
        return Ok(new { mode = configuration["StateMode"], value = input.Value });
    }

    [HttpDelete]
    public IActionResult Clear()
    {
        if (UsesSession)
            HttpContext.Session.Remove(Key);
        else
            Response.Cookies.Delete(Key);
        return NoContent();
    }

    public sealed class StateValue
    {
        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;
    }
}
