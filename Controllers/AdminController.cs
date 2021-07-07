using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagerApi.DTOs;
using SchoolManagerApi.Models;
using SchoolManagerApi.Services;
using SchoolManagerApi.Utils;

namespace SchoolManagerApi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : DefaultController
    {
        private readonly UserManager<EntityUser> _userManager;
        private readonly SignInManager<EntityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenService _tokenService;

        public AdminController(
            UserManager<EntityUser> userManager,
            SignInManager<EntityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            TokenService tokenService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserAuthDTO>> RegisterAdmin(RegisterDTO registerDTO)
        {
            var adminExists = await _roleManager.RoleExistsAsync(Constants.Roles.Admin);
            if (!adminExists)
            {
                var role = new IdentityRole(Constants.Roles.Admin);
                var roleResult = await _roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult);
            }
            var user = new EntityUser
            {
                UserName = registerDTO.Email.ToLower().Trim(),
                Email = registerDTO.Email.ToLower().Trim(),
                PhoneNumber = registerDTO.Phone,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName
            };
            var result = await _userManager.CreateAsync(user, password: registerDTO.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var addToRoleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.Admin);
            if (addToRoleResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return await UserToDto(user, roles.ToList());
            }
            return BadRequest("Can't add Admin");
        }
        private async Task<UserAuthDTO> UserToDto(EntityUser user, List<string> roles)
        {
            return new UserAuthDTO
            {
                Username = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                Id = user.Id,
                Email = user.Email,
                Roles = roles
            };
        }
    }
}