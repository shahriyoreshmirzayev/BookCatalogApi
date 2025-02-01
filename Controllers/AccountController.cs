using BookApplication.Abstraction;
using BookApplication.Extensions;
using BookApplication.Models;
using BookApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AccountController(ITokenService tokenService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }



    [HttpGet]
    public IActionResult Login([FromForm] UserCredentials userCredentials)
    {
        var user = _userRepository.GetAsync(x => x.Password.GetHash() == userCredentials.Password.GetHash() &&
        x.Email == userCredentials.Email);
        if (user != null)
        {
            return Ok(_tokenService.CreateToken());
        }
        return BadRequest("Login pr passwrod is Incorrect ......!");


    }
}
