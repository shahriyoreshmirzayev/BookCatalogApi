using AutoMapper;
using BookApplication.DTOs.PermissionDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly IPermissionRepository _PermissionRepository;
    private readonly IValidator<Permission> _validator;
    private readonly IMapper _mapper;

    public PermissionController(IPermissionRepository PermissionRepository, IValidator<Permission> validator, IMapper mapper)
    {
        _PermissionRepository = PermissionRepository;
        _validator = validator;
        _mapper = mapper;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetPermissionById([FromQuery] int id)
    {

        Permission permission = await _PermissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            return NotFound($"Permission Id {id} not found. .....!");
        }
        return Ok(permission);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllPermissions()
    {
        IQueryable<Permission> Permissions = await _PermissionRepository.GetAsync(x => true);
        return Ok(Permissions);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionCreateDTO permissionCreateDTO)
    {
        if (ModelState.IsValid)
            return BadRequest(ModelState);
        Permission permission = _mapper.Map<Permission>(permissionCreateDTO);
        permission = await _PermissionRepository.AddAsync(permission);
        if (permission == null) return BadRequest(ModelState);
        return BadRequest(permission);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdatePermission([FromBody] Permission PermissionUpdateDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        Permission permission1 = _mapper.Map<Permission>(PermissionUpdateDTO);
        var validationRes = _validator.Validate(PermissionUpdateDTO);
        if (validationRes.IsValid)
        {
            return BadRequest(validationRes);
        }
        PermissionUpdateDTO = await _PermissionRepository.AddAsync(PermissionUpdateDTO);
        if (PermissionUpdateDTO == null) return NotFound();
        return Ok(PermissionUpdateDTO);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeletePermission([FromQuery] int id)
    {
        bool isDelete = await _PermissionRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }


}
