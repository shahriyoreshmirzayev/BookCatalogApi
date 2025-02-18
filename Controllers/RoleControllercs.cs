using AutoMapper;
using BookApplication.DTOs.RoleDTO;
using BookApplication.Repositories;
using BookCatalogApi.Filters;
using BookCatalogApiDomain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[ValidationActionFilters]
public class RoleControllercs : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMapper _mapper;

    public RoleControllercs(IRoleRepository roleRepository, IMapper mapper, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
        _permissionRepository = permissionRepository;
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
    //[Authorize]
    public async Task<IActionResult> GetAllRole()
    {
        IQueryable<Role> Roles = await _roleRepository.GetAsync(x => true);
        return Ok(Roles);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateDTO createDTO)
    {
        Role role = _mapper.Map<Role>(createDTO);
        List<Permission> permissions = new(); 
        for (int i = 0; i < role.Permissions.Count; i++)
        {
            Permission permission = role.Permissions.ToArray()[i];
            permission = await _permissionRepository.GetByIdAsync(permission.PermissionId);
            if (permission == null)
            {
                return NotFound($"Permission not found ID: {permission.PermissionId}");
            }
            else
            {
                permissions.Add(permission);
            }
        }
        role.Permissions = permissions;
        role = await _roleRepository.AddAsync(role);
        if (role == null) return BadRequest(ModelState);
        RoleGetDTO roleGet = _mapper.Map<RoleGetDTO>(role);
        return Ok(roleGet);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateDTO updateDTO)
    {
        Role role = _mapper.Map<Role>(updateDTO);
       
        role = await _roleRepository.UpdateAsync(role);
        if (role == null) return BadRequest(ModelState);
       
        RoleGetDTO roleGet = _mapper.Map<RoleGetDTO>(role);
        return Ok(roleGet);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteRole([FromQuery] int id)
    {
        bool isDelete = await _roleRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....") 
            : BadRequest("Delete operation failed");
    }
}
