using AutoMapper;
using BookApplication.DTOs.AuthorDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleControllercs : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public RoleControllercs(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetRoleById([FromQuery] int id)
    {
        Role role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            return NotFound($"Author Id {id} not found. .....!");
        }
        return Ok(role);
    }

    [HttpGet("[action]")]
    //[OutputCache(Duration = 30)]
    [Authorize]
    public async Task<IActionResult> GetAllRole()
    {
        IQueryable<Role> Roles = await _roleRepository.GetAsync(x => true);
        return Ok(Roles);  
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateRole([FromBody] AuthorCreateDTO createDTO)
    {
        if (ModelState.IsValid)
        {
            Role role = _mapper.Map<Role>(createDTO);
            var validResult = _validator.Validate(role);
            if (!validResult.IsValid) return BadRequest(validResult);


            role = await _roleRepository.AddAsync(role);
            if (role == null) return NotFound();
            AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(role);
            return Ok(authorGet);
        }
        return BadRequest(ModelState);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateRole([FromBody] AuthorUpdateDTO createDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        Author author = _mapper.Map<Author>(createDTO);
        var validationRes = _validator.Validate(author);
        if (validationRes.IsValid)
        {
            return BadRequest(validationRes);
        }

        author = await _authorRepository.AddAsync(author);
        if (author == null) return NotFound();
        AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(author);
        _memoryCache.Remove(authorGet.Id);                             //Keyni o'chirib yuborish
        _memoryCache.Remove(_Cashe_Key);
        return Ok(authorGet);

    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteRole([FromQuery] int id)
    {
        bool isDelete = await _roleRepository.DeleteAsync(id);

        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }


}
