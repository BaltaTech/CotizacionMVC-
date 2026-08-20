using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Auth;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CotizacionMVC.Servicios.Infraestructura
{
    public class JwtService : IJwtServicio
    {
        private readonly IConfiguration _configuration;
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiresInMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;

            _key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key no está configurada en appsettings.json");

            _issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer no está configurada en appsettings.json");

            _audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience no está configurada en appsettings.json");

            _expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
        }

        public string GenerarToken(Guid usuarioId, string email, string nombreCompleto, IList<string> roles, Guid? empresaId = null)
        {
            var claims = CrearClaims(usuarioId, email, nombreCompleto, roles, empresaId);
            var token = CrearJwtToken(claims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenResultDto GenerarTokenCompleto(Guid usuarioId, string email, string nombreCompleto, IList<string> roles, Guid? empresaId = null)
        {
            var claims = CrearClaims(usuarioId, email, nombreCompleto, roles, empresaId);
            var token = CrearJwtToken(claims);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerarRefreshToken();
            var expiresInSeconds = _expiresInMinutes * 60;

            return new TokenResultDto
            {
                Token = tokenString,
                RefreshToken = refreshToken,
                ExpiresInSeconds = expiresInSeconds
            };
        }

        private List<Claim> CrearClaims(Guid usuarioId, string email, string nombreCompleto, IList<string> roles, Guid? empresaId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, nombreCompleto)
            };

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            if (empresaId.HasValue)
            {
                claims.Add(new Claim("EmpresaId", empresaId.Value.ToString()));
            }

            return claims;
        }

        private JwtSecurityToken CrearJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
                signingCredentials: credentials
            );
        }

        public ClaimsPrincipal? ValidarToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Guid? ObtenerUsuarioIdDesdeToken(string token)
        {
            var principal = ValidarToken(token);
            if (principal == null) return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }

        public string GenerarRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public bool TokenHaExpirado(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}