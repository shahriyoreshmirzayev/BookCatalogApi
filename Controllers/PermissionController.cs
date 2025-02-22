using BookApplication.DTOs.PermissionDTO;
using BookApplication.Repositories;
using BookApplication.UseCases.Permission.Commands;
using BookApplication.UseCases.Permission.Query;
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
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllPermissions()
    {
        return await _mediator.Send(new GetAllPermissionQuery());
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand permissionCreateDTO)
    {
        return await _mediator.Send(permissionCreateDTO);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionCommand PermissionUpdateDTO)
    {
        return await _mediator.Send(PermissionUpdateDTO);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeletePermission([FromQuery] int id)
    {
        bool isDelete = await _permissionRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }
}
