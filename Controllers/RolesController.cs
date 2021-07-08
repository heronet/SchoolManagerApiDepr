using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagerApi.DTOs;
using SchoolManagerApi.Security;
using SchoolManagerApi.Utils;

namespace SchoolManagerApi.Controllers
{
    [Authorize(Policy = Security.Policies.RolesManagement.ManageRolesPolicy)]
    public class RolesController : DefaultController
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult> AddRole(string name)
        {
            var roleExists = await _roleManager.RoleExistsAsync(name);
            if (!roleExists)
            {
                IdentityRole newRole;
                if (name.ToLower() == Constants.Roles.Admin.ToLower())
                    newRole = new IdentityRole(Constants.Roles.Admin);
                else if (name.ToLower() == Constants.Roles.Teacher.ToLower())
                    newRole = new IdentityRole(Constants.Roles.Teacher);
                else if (name.ToLower() == Constants.Roles.StoreKeeper.ToLower())
                    newRole = new IdentityRole(Constants.Roles.StoreKeeper);
                else if (name.ToLower() == Constants.Roles.Student.ToLower())
                    newRole = new IdentityRole(Constants.Roles.Student);
                else
                    return BadRequest("Invalid Role");
                var roleResult = await _roleManager.CreateAsync(newRole);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
            }
            if (name.ToLower() == Constants.Roles.Admin.ToLower())
            {
                var role = await _roleManager.FindByNameAsync(Constants.Roles.Admin);

                // Store Permissions
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Read));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Add));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Modify));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Delete));

                // Role Management Permissions
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.RolesManagement.Access));

            }
            else if (name.ToLower() == Constants.Roles.StoreKeeper.ToLower())
            {
                var role = await _roleManager.FindByNameAsync(Constants.Roles.StoreKeeper);
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Read));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Add));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Modify));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Delete));
            }
            else if (name.ToLower() == Constants.Roles.Teacher.ToLower())
            {
                var role = await _roleManager.FindByNameAsync(Constants.Roles.StoreKeeper);
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, Permissions.Products.Read));
            }
            return Ok("Role Added Succesfully");
        }
        [HttpPatch]
        public async Task<ActionResult> ModifyRoleClaims(ModifyRoleClaimsDTO roleClaimsDTO)
        {
            var role = await _roleManager.FindByNameAsync(roleClaimsDTO.Name);
            if (role == null) return BadRequest("Invalid Role");

            var roleClaims = (await _roleManager.GetClaimsAsync(role)).Select(c => c.Value);
            if (roleClaimsDTO.Mode == "Add")
            {
                foreach (var permission in roleClaimsDTO.Permissions)
                {
                    if (roleClaims.Contains(permission)) return BadRequest("Can't Add Duplicate Claim");
                    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                }
                var claims = await GetRoleClaims(role);
                return Ok(new { Message = "Claims Added Succesfully", Claims = claims });
            }
            else if (roleClaimsDTO.Mode == "Remove")
            {
                foreach (var permission in roleClaimsDTO.Permissions)
                {
                    if (!roleClaims.Contains(permission)) return BadRequest("Can't Remove non existent Claim");
                    await _roleManager.RemoveClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                }
                var claims = await GetRoleClaims(role);
                return Ok(new { Message = "Claims Deleted Succesfully", Claims = claims });
            }
            return BadRequest("Invalid Modify Mode");
        }
        [HttpGet("claims")]
        public async Task<ActionResult<List<string>>> GetRoleClaimsByName(string name)
        {
            if (name == null) return BadRequest("Invalid Role");
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null) return BadRequest("Invalid Role");

            var roleCaimObjects = await _roleManager.GetClaimsAsync(role);
            var claims = roleCaimObjects.Select(c => c.Value).ToList();

            return Ok(new { Claims = claims });
        }
        private async Task<List<string>> GetRoleClaims(IdentityRole role)
        {
            var roleCaimObjects = await _roleManager.GetClaimsAsync(role);
            var claims = roleCaimObjects.Select(c => c.Value).ToList();
            return claims;
        }
    }
}