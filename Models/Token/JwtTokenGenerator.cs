using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Models.Token
{
	public class JwtTokenGenerator
	{
		public static string GenerateToken(string userId, string username, IConfiguration configuration) 
		{
			var jwtSettings = configuration.GetSection("Jwt");
			var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? string.Empty);
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(
				[
					new(ClaimTypes.NameIdentifier, userId),
					new(ClaimTypes.Name, username)
				]),
				Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? string.Empty)),
				Issuer = jwtSettings["Issuer"],
				Audience = jwtSettings["Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}