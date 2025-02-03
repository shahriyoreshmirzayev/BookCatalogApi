﻿using AutoMapper;
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
    private readonly IMapper _mapper;

    public AccountController(ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _mapper = mapper;
    }



    [HttpGet("[action]")]
    public async Task<IActionResult> Login([FromForm] UserCredentials userCredentials)
    {
        var user = (await _userRepository.GetAsync(x => x.Password.GetHash() == userCredentials.Password.GetHash() &&
        x.Email == userCredentials.Email)).FirstOrDefault();
        if (user != null)
        {
            return Ok(_tokenService.CreateToken(user));
        }
        return BadRequest("Login pr passwrod is Incorrect ......!");
    }
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        if (ModelState.IsValid)
        {
            user.Password = user.Password.GetHash();
            user = await _userRepository.AddAsync(user);
            if (user != null)
            {
                string token = _tokenService.CreateToken(user);
                return Ok(token);
            }
        }
        return BadRequest();
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
        if(ModelState.IsValid)
        {
            user = await _userRepository.UpdateAsync(user);
            return Ok(user);
        }
        return BadRequest();
    }

}
