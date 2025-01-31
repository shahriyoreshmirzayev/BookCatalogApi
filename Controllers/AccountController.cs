using BookApplication.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AccountController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }


    
    [HttpGet]
    public IActionResult Login([FromQuery] string Login)
    {
        if (Login == "userLogin")
        {
            return Ok(_tokenService.CreateToken());
        }
        return BadRequest("Login Incorrect ......!");


    }
}
