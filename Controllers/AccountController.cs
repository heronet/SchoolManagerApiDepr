using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagerApi.DTOs;
using SchoolManagerApi.Models;
using SchoolManagerApi.Services;

namespace SchoolManagerApi.Controllers
{
    public class AccountController : DefaultController
    {
        private readonly UserManager<EntityUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly SignInManager<EntityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<EntityUser> userManager, SignInManager<EntityUser> signInManager, RoleManager<IdentityRole> roleManager, TokenService tokenService)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _userManager = userManager;
        }
        /// <summary>
        /// POST api/account/login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns><see cref="UserAuthDTO" /></returns>
        [HttpPost("login")]
        public async Task<ActionResult<UserAuthDTO>> LoginUser(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username.Trim());

            // Return If user was not found
            if (user == null) return BadRequest("Invalid Username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, password: loginDTO.Password, false);
            if (result.Succeeded)
            {
                var roles = (await _userManager.GetRolesAsync(user)).ToList();
                var claims = await GetClaims(roles);
                return await UserToDto(user, roles.ToList(), claims);
            }

            return BadRequest("Invalid Password");
        }
        /// <summary>
        /// POST api/account/refresh
        /// </summary>
        /// <param name="userAuthDTO"></param>
        /// <returns><see cref="UserAuthDTO" /></returns>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<ActionResult<UserAuthDTO>> RefreshToken(UserAuthDTO userAuthDTO)
        {

            var user = await _userManager.FindByIdAsync(userAuthDTO.Id);

            // Return If user was not found
            if (user == null) return BadRequest("Invalid User");

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var claims = await GetClaims(roles);
            return await UserToDto(user, roles.ToList(), claims);
        }

        /// <summary>
        /// Utility Method.
        /// Converts a WhotUser to an AuthUserDto
        /// </summary>
        /// <param name="user"></param>
        /// <returns><see cref="UserAuthDTO" /></returns>
        private async Task<UserAuthDTO> UserToDto(EntityUser user, List<string> roles, List<string> claims)
        {
            return new UserAuthDTO
            {
                Username = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                Id = user.Id,
                Roles = roles,
                Claims = claims
            };
        }
        private async Task<List<string>> GetClaims(List<string> roles)
        {
            List<string> claims = new List<string>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims.AddRange(roleClaims.Select(c => c.Value).ToList());
            }
            return claims;
        }
    }
}