using BookApplication.Abstraction;
using BookApplication.Extensions;
using BookApplication.Models;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
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
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        if (ModelState.IsValid)
        {
            if (await _userRepository.AddAsync(user) != null)
            {
                return Ok();
            }
        }
        return BadRequest();
    }
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        IQueryable<User> res = await _userRepository.GetAsync(x => true);
        return Ok(res);
    }
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] User user)
    {
        if(ModelState.IsValid)
        {
            user = await _userRepository.UpdateAsync(user);
            return Ok(user);
        }
        return BadRequest();
    }

}
