using BookApplication.DTOs.PermissionDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PermissionController : ApiControllerBase
{
    private readonly IPermissionRepository _permissionRepository;
    //private readonly IValidator<Permission> _validator;

    public PermissionController(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
        //_validator = validator;
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
        Permission permission = _mapper.Map<Permission>(permissionCreateDTO);
        permission = await _permissionRepository.AddAsync(permission);
        if (permission == null) return BadRequest(ModelState);
        return Ok(permission);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdatePermission([FromBody] Permission PermissionUpdateDTO)
    {
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
