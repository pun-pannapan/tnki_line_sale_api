using tnki_line_sale_api.Models;
using tnki_line_sale_api.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace tnki_line_sale_api.Utilities
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                attachUserToContext(context, userService, token);

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, IUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                string idValue = jwtToken.Claims.First(x => x.Type == "id").Value;

                string role = jwtToken.Claims.First(x => x.Type == "role").Value;
                if (role == "CUST")
                {
                    Guid cust_guid = new Guid(idValue);
                    context.Items["Role"] = "user";
                    context.Items["cust_guid"] = cust_guid;

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim("role", "user"));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, cust_guid.ToString()));
                    context.User.AddIdentity(claimsIdentity);
                }
                else if (role == "ADMIN")
                {
                    Guid userGuid = new Guid(idValue);

                    context.Items["Role"] = "admin";
                    context.Items["UserGuid"] = userGuid;

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim("role", "admin"));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userGuid.ToString()));
                    context.User.AddIdentity(claimsIdentity);
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
