using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<LoginResultadoDto> LoginAsync(LoginDto credenciales)
        {
            var resultado = new LoginResultadoDto();

            try
            {
                // Apunta al nuevo AuthController
                var respuestaServidor = await _httpClient.PostAsJsonAsync("api/v1/seguridad/auth/login", credenciales);

                if (respuestaServidor.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuestaServidor.Content.ReadFromJsonAsync<RespuestaApi<TokenResponseDto>>(_jsonOptions);
                    if (resultadoJson != null && resultadoJson.Data != null)
                    {
                        resultado.Exitoso = true;
                        resultado.Tokens = resultadoJson.Data;
                        resultado.Mensaje = resultadoJson.Mensaje;
                    }
                    return resultado;
                }

                if (respuestaServidor.StatusCode == HttpStatusCode.Locked)
                {
                    var bloqueoJson = await respuestaServidor.Content.ReadFromJsonAsync<LoginResultadoDto>(_jsonOptions);
                    if (bloqueoJson != null)
                    {
                        resultado.CuentaBloqueada = true;
                        resultado.SegundosRestantes = bloqueoJson.SegundosRestantes;
                        resultado.Mensaje = bloqueoJson.Mensaje;
                    }
                    return resultado;
                }

                var contenidoError = await respuestaServidor.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuestaServidor.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.CuentaBloqueada = false;
                resultado.Mensaje = $"Error de infraestructura de red: {ex.Message}";
            }

            return resultado;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("api/v1/seguridad/auth/logout", null);
            }
            catch
            {
                // Silencioso si falla la red en el logout
            }
        }

        public async Task<OperacionResultadoDto> SolicitarRecuperacionAsync(ForgotPasswordDto correoDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/auth/solicitar-recuperacion", correoDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Código de verificación (OTP) enviado exitosamente al correo.";
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de comunicación con la API: {ex.Message}";
            }

            return resultado;
        }

        public async Task<OperacionResultadoDto> RestablecerPasswordAsync(ResetPasswordDto restablecerDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/auth/restablecer-password", restablecerDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Contraseña restablecida exitosamente.";
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red al aplicar la nueva clave: {ex.Message}";
            }

            return resultado;
        }

        private static string ExtraerMensajeErrorUniversal(string contenidoRaw, HttpStatusCode statusCode)
        {
            if (string.IsNullOrWhiteSpace(contenidoRaw))
                return $"El servidor rechazó la solicitud (Código HTTP: {(int)statusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(contenidoRaw);
                var root = doc.RootElement;

                if (TryGetProp(root, "mensaje", out var propMensaje) && !string.IsNullOrWhiteSpace(propMensaje.GetString()))
                    return propMensaje.GetString()!;

                if (TryGetProp(root, "detail", out var propDetail) && !string.IsNullOrWhiteSpace(propDetail.GetString()))
                    return propDetail.GetString()!;
            }
            catch { }

            return contenidoRaw.Trim('"').Trim();
        }

        private static bool TryGetProp(JsonElement element, string propName, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = prop.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }
    }
}
