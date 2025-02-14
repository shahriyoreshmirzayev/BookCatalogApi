using AutoMapper;
using BookApplication.DTOs.PermissionDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly IPermissionRepository _permissionRepository;
    //private readonly IValidator<Permission> _validator;
    private readonly IMapper _mapper;

    public PermissionController(IPermissionRepository permissionRepository, /*IValidator<Permission> validator,*/ IMapper mapper)
    {
        _permissionRepository = permissionRepository;
        //_validator = validator;
        _mapper = mapper;
    }   

    [HttpGet("[action]")]
    public async Task<IActionResult> GetPermissionById([FromQuery] int id)
    {

        Permission permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            return NotFound($"Permission Id {id} not found. .....!");
        }
        return Ok(permission);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllPermissions()
    {
        IQueryable<Permission> Permissions = await _permissionRepository.GetAsync(x => true);
        return Ok(Permissions);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionCreateDTO permissionCreateDTO)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        Permission permission = _mapper.Map<Permission>(permissionCreateDTO);
        permission = await _permissionRepository.AddAsync(permission);
        if (permission == null) return BadRequest(ModelState);
        return Ok(permission);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdatePermission([FromBody] Permission PermissionUpdateDTO)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Permission permission = _mapper.Map<Permission>(PermissionUpdateDTO);

        permission = await _permissionRepository.UpdateAsync(PermissionUpdateDTO);
        if (PermissionUpdateDTO == null) return NotFound();
        return Ok(permission);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeletePermission([FromQuery] int id)
    {
        bool isDelete = await _permissionRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }


}
