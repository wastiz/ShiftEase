using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ShiftEaseAPI.Helpers
{
    public static class JwtHelper
    {
        public static int GetUserIdFromRequest(HttpRequest request)
        {
            var authorizationHeader = request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return 0;
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch
            {
                throw new Exception("Cannot find UserId from jwt token.");
            }

            return 0;
        }
    }
}
