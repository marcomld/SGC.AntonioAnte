using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<ResultadoPaginadoDto<DepartamentoDto>?> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, int pagina, int registrosPorPagina)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pagina={pagina}",
                    $"registrosPorPagina={registrosPorPagina}"
                };

                if (!string.IsNullOrWhiteSpace(busqueda))
                    queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda.Trim())}");

                if (estadoActivo.HasValue)
                    queryParams.Add($"estadoActivo={estadoActivo.Value.ToString().ToLower()}");

                string url = $"api/v1/seguridad/departamentos?{string.Join("&", queryParams)}";

                var respuesta = await _httpClient.GetAsync(url);
                if (respuesta.IsSuccessStatusCode)
                {
                    return await respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<DepartamentoDto>>(_jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar departamentos paginados: {ex.Message}");
            }
            return null;
        }

        public async Task<List<DepartamentoDto>?> ObtenerTodosAsync()
        {
            // Consulta los departamentos activos para alimentarlos a los comboboxes / selects
            var resultado = await ObtenerPaginadoAsync(busqueda: null, estadoActivo: true, pagina: 1, registrosPorPagina: 1000);
            return resultado?.Items;
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
                if (root.TryGetProperty("detail", out var propDetail) && !string.IsNullOrWhiteSpace(propDetail.GetString()))
                    return propDetail.GetString()!;
            }
            catch { }
            return contenidoRaw.Trim('"').Trim();
        }
    }
}