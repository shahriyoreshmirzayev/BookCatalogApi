using AutoMapper;
using BookApplication.DTOs.UserDTO;
using BookApplication.Extensions;
using BookApplication.Repositories;
using BookCatalogApi.Filters;
using BookCatalogApiDomain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public UserController(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    [CustomAuthorizationFilter("GetUserById")]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetUserById([FromQuery] int id)
    {
        User user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound($"Author Id {id} not found. .....!");
        }
        return Ok(user);
    }

    [HttpGet("[action]")]
    //[OutputCache(Duration = 30)]
    //[Authorize]
    public async Task<IActionResult> GetAllUser()
    {
        IQueryable<User> Users = await _userRepository.GetAsync(x => true);
        return Ok(Users);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO createDTO)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        User user = _mapper.Map<User>(createDTO);
        List<Role> permissions = new();

        for (int i = 0; i < user.Roles.Count; i++)
        {
            Role permission = user.Roles.ToArray()[i];
            permission = await _roleRepository.GetByIdAsync(permission.RoleId);
            if (permission == null)
            {
                return NotFound($"Role not found ......");
            }
            permissions.Add(permission);
        }
        user.Roles = permissions;
        user = await _userRepository.AddAsync(user);
        if (user == null) return BadRequest(ModelState);


        UserGetDTO userGet = _mapper.Map<UserGetDTO>(user);
        return Ok(userGet);
    }


    /*[HttpPost("[action]")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO createDTO)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (createDTO == null)
        {
            return BadRequest("Invalid request: User data is required.");
        }

        User user = _mapper.Map<User>(createDTO);
        if (user == null)
        {
            return BadRequest("Mapping error: Unable to convert DTO to User.");
        }

        if (user.Roles == null || user.Roles.Count == 0)
        {
            return BadRequest("User must have at least one role.");
        }

        List<Role> permissions = new();

        for (int i = 0; i < user.Roles.Count; i++)
        {
            Role permission = user.Roles.ToArray()[i];

            if (permission.RoleId == 0) // Ensure RoleId is valid
            {
                return BadRequest("Invalid RoleId.");
            }

            permission = await _roleRepository.GetByIdAsync(permission.RoleId);
            if (permission == null)
            {
                return NotFound($"Role not found ......");
            }
            permissions.Add(permission);
        }

        user.Roles = permissions;
        user = await _userRepository.AddAsync(user);

        if (user == null)
            return BadRequest("Failed to create user.");

        UserGetDTO userGet = _mapper.Map<UserGetDTO>(user);
        return Ok(userGet);
    }*/





    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO updateDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        User user = _mapper.Map<User>(updateDTO);

        user = await _userRepository.UpdateAsync(user);
        if (user == null) return BadRequest(ModelState);

        UserGetDTO userGet = _mapper.Map<UserGetDTO>(user);
        return Ok(userGet);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteUser([FromQuery] int id)
    {
        bool isDelete = await _userRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....")
            : BadRequest("Delete operation failed");
    }
    [HttpPut("[action]")]
    public async Task<IActionResult> ChangeUserPassword(UserChangedPasswordDTO userChangedPassword)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.GetByIdAsync(userChangedPassword.UserId);

            if (user != null)
            {
                string CurrentHash = userChangedPassword.CurrentPassword.GetHash();
                //string DbHash = user.Password;
                if (CurrentHash == user.Password
                    && userChangedPassword.NewPassword == userChangedPassword.ConfirmNewPassword)
                {
                    user.Password = userChangedPassword.NewPassword.GetHash();
                    await _userRepository.UpdateAsync(user);
                    return Ok();
                }
                else return BadRequest("Incorrect password .......");
            }
            return BadRequest("User not found .....");
        }
        return BadRequest(ModelState);
    }
}
