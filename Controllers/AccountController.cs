using AutoMapper;
using BookApplication.Abstraction;
using BookApplication.DTOs.UserDTO;
using BookApplication.Extensions;
using BookApplication.Models;
using BookApplication.Repositories;
using BookCatalogApi.Filters;
using BookCatalogApiDomain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[ValidationActionFilters]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AccountController(ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _mapper = mapper;
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromForm] UserCredentials userCredentials)
    {
        var user = (await _userRepository.GetAsync(x => x.Password == userCredentials.Password.GetHash() &&
        x.Email == userCredentials.Email)).FirstOrDefault();
        if (user != null)
        {
            //bu mening birinchi bot
            RegistiredUserDTO userDTO = new()
            {
                User = user,
                UsersTokens = await _tokenService.CreateTokenAsync(user)
            };
            return Ok(userDTO);
        }
        return BadRequest("Login pr passwrod is Incorrect ......!");
    }
    // Login uchun ChatGPT qilib bergan kod
    /*public async Task<IActionResult> Login([FromForm] UserCredentials userCredentials)
    {
        string hashedPassword = userCredentials.Password.GetHash(); // Parolni oldindan xesh qilish

        var user = (await _userRepository.GetAsync(x => x.Email == userCredentials.Email))
            .FirstOrDefault(x => x.Password == hashedPassword); // Faqat bazadan email bo‘yicha olish

        if (user != null)
        {
            RegistiredUserDTO userDTO = new()
            {
                User = user,
                UsersTokens = await _tokenService.CreateTokenAsync(user)
            };
            return Ok(userDTO);
        }
        return BadRequest("Login yoki parol noto‘g‘ri!");
    }*/

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Create([FromBody] UserCreateDTO NewUser)
    {
        User user = _mapper.Map<User>(NewUser);
        user.Password = user.Password.GetHash();
        user = await _userRepository.AddAsync(user);
        if (user != null)
        {
            RegistiredUserDTO userDTO = new()
            {
                User = user,
                UsersTokens = await _tokenService.CreateTokenAsync(user)
            };
            return Ok(userDTO);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] Token tokens)
    {
        var principal = _tokenService.GetClaimsFromExpiredToken(tokens.AccesToken);
        string? email = principal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            return NotFound("Refresh token not found ....");
        }
        RefreshToken? savedRefreshToken = _tokenService.Get(x => x.Email == email &&
                                                        x.RefreshTokenValue == tokens.RefreshToken)
                                                        .FirstOrDefault();

        if (savedRefreshToken == null)
        {
            return BadRequest("Refresh token or Acces token inValid ......");
        }
        if (savedRefreshToken.ExpiredDate < DateTime.UtcNow)
        {
            _tokenService.Delete(savedRefreshToken);
            return StatusCode(405, "Refresh token already expired");
        }
        Token newTokens = await _tokenService.CreateTokensFromRefresh(principal, savedRefreshToken);

        return Ok(newTokens);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllUsers()
    {
        IQueryable<User> res = await _userRepository.GetAsync(x => true);
        return Ok(res);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateUser([FromBody] User user)
    {
        user = await _userRepository.UpdateAsync(user);
        return Ok(user);
    }
}
