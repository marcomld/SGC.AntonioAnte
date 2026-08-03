using System.Security.Claims;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Utils
{
    public class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string token)
        {
            var claims = new List<Claim>();
            var payload = token.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var paresLlaveValor = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (paresLlaveValor == null) return claims;

            ExtraerRolesDeArreglo(claims, paresLlaveValor);

            foreach (var par in paresLlaveValor)
            {
                claims.Add(new Claim(par.Key, par.Value.ToString() ?? string.Empty));
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        private static void ExtraerRolesDeArreglo(List<Claim> claims, Dictionary<string, object> paresLlaveValor)
        {
            paresLlaveValor.TryGetValue(ClaimTypes.Role, out object? roles);

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var rolesParseados = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    if (rolesParseados != null)
                    {
                        foreach (var rolAsignado in rolesParseados)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, rolAsignado));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }
                paresLlaveValor.Remove(ClaimTypes.Role);
            }
        }
    }
}
