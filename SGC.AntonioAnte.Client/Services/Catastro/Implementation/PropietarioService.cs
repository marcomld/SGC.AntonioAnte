using SGC.AntonioAnte.Client.Services.Catastro.Contracts;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Catastro.Implementation
{
    public class PropietarioService : IPropietarioService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PropietarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<ResultadoPaginadoDto<PropietarioDto>> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, TipoPropietario? tipoPropietario, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/catastro/propietarios?pagina={pagina}&registrosPorPagina={registrosPorPagina}";

            if (!string.IsNullOrWhiteSpace(busqueda))
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

            if (estadoActivo.HasValue)
                url += $"&estadoActivo={estadoActivo.Value}";

            if (tipoPropietario.HasValue)
                url += $"&tipoPropietario={(int)tipoPropietario.Value}";

            var response = await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<PropietarioDto>>(url, _jsonOptions);
            return response ?? new ResultadoPaginadoDto<PropietarioDto>();
        }

        public async Task<PropietarioDto?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/catastro/propietarios/{identificacion}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataElem))
                    {
                        return JsonSerializer.Deserialize<PropietarioDto>(dataElem.GetRawText(), _jsonOptions);
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<OperacionResultadoDto> CrearAsync(CreatePropietarioDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/catastro/propietarios", dto);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return res ?? OperacionResultadoDto.Exito("Propietario registrado exitosamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return OperacionResultadoDto.Fallo(ExtraerMensajeError(errorContent, response.StatusCode));
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error de comunicación: {ex.Message}");
            }
        }

        public async Task<OperacionResultadoDto> ActualizarAsync(Guid id, UpdatePropietarioDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/catastro/propietarios/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return OperacionResultadoDto.Exito("Propietario actualizado exitosamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return OperacionResultadoDto.Fallo(ExtraerMensajeError(errorContent, response.StatusCode));
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error de comunicación: {ex.Message}");
            }
        }

        public async Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/v1/catastro/propietarios/{id}/cambiar-estado", null);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return res ?? OperacionResultadoDto.Exito("Estado actualizado exitosamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return OperacionResultadoDto.Fallo(ExtraerMensajeError(errorContent, response.StatusCode));
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error de comunicación: {ex.Message}");
            }
        }

        private static string ExtraerMensajeError(string raw, System.Net.HttpStatusCode code)
        {
            if (string.IsNullOrWhiteSpace(raw)) return $"Error del servidor ({code}).";
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("mensaje", out var m) && !string.IsNullOrWhiteSpace(m.GetString()))
                    return m.GetString()!;
                if (doc.RootElement.TryGetProperty("detail", out var d) && !string.IsNullOrWhiteSpace(d.GetString()))
                    return d.GetString()!;
            }
            catch { }
            return raw.Trim('"');
        }
    }
}
