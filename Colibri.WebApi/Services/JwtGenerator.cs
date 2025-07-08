using Colibri.WebApi.Services.Abstract;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Colibri.WebApi.Services
{
    /// <summary>
    /// Генератор JSON Web Token (JWT).
    /// Предоставляет функциональность для создания JWT на основе предоставленных данных о пользователе.
    /// </summary>
    /// <param name="key">Секретный ключ для подписи токена</param>
    /// <param name="issuer">Издатель токена</param>
    /// <param name="audience">Аудитория токена</param>
    /// <param name="secret">Секрет для создания ключа</param>
    public class JwtGenerator(string key, string issuer, string audience, string secret) : IJwtGenerator
    {
        private readonly string _key = key;
        private readonly string _issuer = issuer;
        private readonly string _audience = audience;
        private readonly string _secret = secret;
        /// <summary>
        ///  Создает новый JWT на основе имени пользователя и ролей.
        /// </summary>
        /// <param name="userName">Имя пользователя, для которого будет создан токен.</param>
        /// <param name="roles">Список ролей пользователя.</param>
        /// <returns>Созданный JWT как строка.</returns>
        public string Seed(string userName, List<string> roles)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.NameIdentifier, userName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds,
                NotBefore = DateTime.Now,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
