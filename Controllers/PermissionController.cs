using BookApplication.DTOs.PermissionDTO;
using BookApplication.Repositories;
using BookApplication.UseCases.Permission;
using BookApplication.UseCases.Permission.Commands;
using BookCatalogApiDomain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PermissionController : ApiControllerBase
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMediator _mediator;
    public PermissionController(IPermissionRepository permissionRepository, IMediator mediator = null)
    {
        _permissionRepository = permissionRepository;
        _mediator = mediator;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetPermissionById([FromQuery] GetPermissionByIdQuery query)
    {
        return await _mediator.Send(query);
        //Permission permission = await _permissionRepository.GetByIdAsync(id);
        //if (permission == null)
        //{
        //    return NotFound($"Permission Id {id} not found. .....!");
        //}
        //return Ok(permission);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllPermissions()
    {
        IQueryable<Permission> Permissions = await _permissionRepository.GetAsync(x => true);
        return Ok(Permissions);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand permissionCreateDTO)
    {
        return await _mediator.Send(permissionCreateDTO);
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
