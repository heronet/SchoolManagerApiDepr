using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolManagerApi.Models;

namespace SchoolManagerApi.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<EntityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public readonly string _jwtSecret;
        public readonly string _env;
        public TokenService(
            IConfiguration configuration,
            UserManager<EntityUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;

            _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (_env == "Development")
                _jwtSecret = _configuration["JWT_SECRET"];
            else
                _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        }

        public async Task<string> GenerateToken(EntityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRoles = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToArray();
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roleClaims = await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(roles.FirstOrDefault()));
            var claims = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Name, user.UserName)
            }.Union(userClaims).Union(userRoles).Union(roleClaims);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var descriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(3)
            };
            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }

    }
}