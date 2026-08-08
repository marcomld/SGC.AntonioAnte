using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Implementation
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DepartamentoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<DepartamentoDto>?> ObtenerTodosAsync()
        {
            try
            {
                var respuesta = await _httpClient.GetAsync("api/v1/seguridad/departamentos");
                if (respuesta.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<List<DepartamentoDto>>>(_jsonOptions);
                    return resultadoJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar departamentos: {ex.Message}");
            }
            return null;
        }

        public async Task<OperacionResultadoDto> CrearAsync(CreateDepartamentoDto dto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/departamentos", dto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Departamento registrado exitosamente.";
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeError(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> ActualizarAsync(Guid id, CreateDepartamentoDto dto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PutAsJsonAsync($"api/v1/seguridad/departamentos/{id}", dto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Departamento actualizado correctamente.";
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeError(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PutAsync($"api/v1/seguridad/departamentos/{id}/toggle-status", null);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeError(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> EliminarAsync(Guid id)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.DeleteAsync($"api/v1/seguridad/departamentos/{id}");
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Departamento eliminado permanentemente.";
                    return resultado;
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeError(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        private static string ExtraerMensajeError(string contenidoRaw, HttpStatusCode statusCode)
        {
            if (string.IsNullOrWhiteSpace(contenidoRaw)) return $"Error HTTP {(int)statusCode}.";
            try
            {
                using var doc = JsonDocument.Parse(contenidoRaw);
                var root = doc.RootElement;
                if (root.TryGetProperty("mensaje", out var propMsg) && !string.IsNullOrWhiteSpace(propMsg.GetString()))
                    return propMsg.GetString()!;
            }
            catch { }
            return contenidoRaw.Trim('"').Trim();
        }
    }
}
