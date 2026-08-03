using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;

namespace SGC.AntonioAnte.Client.Services.Seguridad
{
    public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<LoginResultadoDto> LoginAsync(LoginDto credenciales)
        {
            var resultado = new LoginResultadoDto();

            try
            {
                var respuestaServidor = await _httpClient.PostAsJsonAsync("api/v1/seguridad/usuarios/login", credenciales);

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
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.CuentaBloqueada = false;
                resultado.Mensaje = $"Error de infraestructura de red: {ex.Message}";
                return resultado;
            }

            resultado.Exitoso = false;
            resultado.Mensaje = "Identificación o contraseña incorrectas. Intente nuevamente.";
            return resultado;
        }

        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("api/v1/seguridad/usuarios/logout", null);
        }

        public async Task<OperacionResultadoDto> RegistrarFuncionarioAsync(CreateUsuarioDto nuevoUsuario)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/usuarios", nuevoUsuario);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Funcionario registrado con éxito en la BD Municipal.";
                    return resultado;
                }

                resultado.Exitoso = false;
                var contenidoError = await respuesta.Content.ReadAsStringAsync();

                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de comunicación de red: {ex.Message}";
            }

            return resultado;
        }

        public async Task<OperacionResultadoDto> AsignarPermisosAsync(string identificacion, AddClaimDto claimDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync($"api/v1/seguridad/usuarios/{identificacion}/permisos", claimDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    return resultado;
                }

                resultado.Exitoso = false;
                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Fallo de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> SolicitarRecuperacionAsync(ForgotPasswordDto correoDto)
        {
            var resultado = new OperacionResultadoDto();

            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/usuarios/solicitar-recuperacion", correoDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Código seguro generado y despachado con éxito.";
                    return resultado;
                }

                resultado.Exitoso = false;
                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de comunicación con la API catastral: {ex.Message}";
            }

            return resultado;
        }

        public async Task<OperacionResultadoDto> RestablecerPasswordAsync(ResetPasswordDto restablecerDto)
        {
            var resultado = new OperacionResultadoDto();

            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/usuarios/restablecer-password", restablecerDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Contraseña restablecida de forma exitosa.";
                    return resultado;
                }

                resultado.Exitoso = false;
                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red al aplicar la nueva clave: {ex.Message}";
            }

            return resultado;
        }

        public async Task<List<CreateUsuarioDto>?> ObtenerTodosLosUsuariosAsync()
        {
            try
            {
                var respuesta = await _httpClient.GetAsync("api/v1/seguridad/usuarios");
                if (respuesta.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<List<CreateUsuarioDto>>>(_jsonOptions);
                    return resultadoJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar nómina: {ex.Message}");
            }
            return null;
        }

        // =========================================================================
        // EXTRACTOR UNIVERSAL DE ERRORES (Garantiza que NUNCA devuelva null o vacío)
        // =========================================================================
        private string ExtraerMensajeErrorUniversal(string contenidoRaw, HttpStatusCode statusCode)
        {
            if (string.IsNullOrWhiteSpace(contenidoRaw))
                return $"El servidor rechazó la solicitud (Código HTTP: {(int)statusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(contenidoRaw);
                var root = doc.RootElement;

                // 1. Evaluamos "detail" o "Detail" (ProblemDetails de ASP.NET Core / ExceptionHandler)
                if (TryGetProp(root, "detail", out var propDetail) && !string.IsNullOrWhiteSpace(propDetail.GetString()))
                    return LimpiarTexto(propDetail.GetString()!);

                // 2. Evaluamos "error" o "Error" (Result Pattern de la API)
                if (TryGetProp(root, "error", out var propError) && !string.IsNullOrWhiteSpace(propError.GetString()))
                    return LimpiarTexto(propError.GetString()!);

                // 3. Evaluamos "mensaje" o "Mensaje" / "message"
                if (TryGetProp(root, "mensaje", out var propMensaje) && !string.IsNullOrWhiteSpace(propMensaje.GetString()))
                    return LimpiarTexto(propMensaje.GetString()!);

                if (TryGetProp(root, "message", out var propMessage) && !string.IsNullOrWhiteSpace(propMessage.GetString()))
                    return LimpiarTexto(propMessage.GetString()!);

                // 4. Evaluamos "errors" o "Errors" (ValidationProblemDetails de FluentValidation)
                if (TryGetProp(root, "errors", out var propErrores))
                {
                    if (propErrores.ValueKind == JsonValueKind.Object)
                    {
                        var primerProp = propErrores.EnumerateObject().FirstOrDefault();
                        if (primerProp.Value.ValueKind == JsonValueKind.Array)
                        {
                            var msg = primerProp.Value.EnumerateArray().FirstOrDefault().GetString();
                            if (!string.IsNullOrWhiteSpace(msg)) return LimpiarTexto(msg);
                        }
                    }
                    else if (propErrores.ValueKind == JsonValueKind.Array)
                    {
                        var msg = propErrores.EnumerateArray().FirstOrDefault().GetString();
                        if (!string.IsNullOrWhiteSpace(msg)) return LimpiarTexto(msg);
                    }
                }

                // 5. Evaluamos "title" o "Title"
                if (TryGetProp(root, "title", out var propTitle) && !string.IsNullOrWhiteSpace(propTitle.GetString()))
                    return LimpiarTexto(propTitle.GetString()!);
            }
            catch (JsonException)
            {
                // Si no es un JSON estructurado, procesa el texto plano
            }

            return LimpiarTexto(contenidoRaw);
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

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Error de validación en la API.";

            string limpio = texto.Trim('"').Trim();

            // Si es la cadena cruda de FluentValidation ("Validation failed: -- Campo: Mensaje Severity: Error")
            if (limpio.Contains("Validation failed") || limpio.Contains("--"))
            {
                // 1. Si la cadena tiene '--', tomamos la parte a la DERECHA del '--' (donde está el mensaje real)
                if (limpio.Contains("--"))
                {
                    var partesDash = limpio.Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                    limpio = partesDash.Length > 1 ? partesDash[1] : partesDash[0];
                }

                // 2. Quitamos la coletilla 'Severity: Error' si está presente
                if (limpio.Contains("Severity:", StringComparison.OrdinalIgnoreCase))
                {
                    var partesSeverity = limpio.Split(new[] { "Severity:", "severity:" }, StringSplitOptions.RemoveEmptyEntries);
                    limpio = partesSeverity[0];
                }

                // 3. Tomamos lo que está a la DERECHA del nombre del campo (ej: 'Identificacion: mensaje...')
                if (limpio.Contains(":"))
                {
                    var partesColon = limpio.Split(new[] { ':' }, 2);
                    limpio = partesColon.Length > 1 ? partesColon[1] : partesColon[0];
                }

                return limpio.Trim();
            }

            return limpio;
        }
    }
}