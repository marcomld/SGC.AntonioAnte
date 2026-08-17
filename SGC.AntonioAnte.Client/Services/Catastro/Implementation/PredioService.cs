using SGC.AntonioAnte.Client.Services.Catastro.Contracts;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Catastro.Implementation
{
    public class PredioService : IPredioService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PredioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<ResultadoPaginadoDto<PredioDto>> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, TipoPredio? tipoPredio, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/catastro/predios?pagina={pagina}&registrosPorPagina={registrosPorPagina}";

            if (!string.IsNullOrWhiteSpace(busqueda))
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

            if (estadoActivo.HasValue)
                url += $"&estadoActivo={estadoActivo.Value}";

            if (tipoPredio.HasValue)
                url += $"&tipoPredio={(int)tipoPredio.Value}";

            var response = await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<PredioDto>>(url, _jsonOptions);
            return response ?? new ResultadoPaginadoDto<PredioDto>();
        }

        public async Task<PredioDto?> ObtenerPorIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/catastro/predios/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataElem))
                    {
                        return JsonSerializer.Deserialize<PredioDto>(dataElem.GetRawText(), _jsonOptions);
                    }
                    return JsonSerializer.Deserialize<PredioDto>(content, _jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<OperacionResultadoDto> CrearPredioBaseAsync(CreatePredioDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/catastro/predios", dto);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return res ?? OperacionResultadoDto.Exito("Predio registrado exitosamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return OperacionResultadoDto.Fallo(ExtraerMensajeError(errorContent, response.StatusCode));
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error de comunicación: {ex.Message}");
            }
        }

        public async Task<OperacionResultadoDto> AgregarDominiosAsync(Guid predioId, List<AddDominioDto> dominios)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/v1/catastro/predios/{predioId}/dominios", dominios);
                if (response.IsSuccessStatusCode)
                {
                    return OperacionResultadoDto.Exito("Dominios asignados correctamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return OperacionResultadoDto.Fallo(ExtraerMensajeError(errorContent, response.StatusCode));
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error de comunicación: {ex.Message}");
            }
        }

        public async Task<OperacionResultadoDto> AgregarBloquesAsync(Guid predioId, List<AddBloqueDto> bloques)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/v1/catastro/predios/{predioId}/bloques", bloques);
                if (response.IsSuccessStatusCode)
                {
                    return OperacionResultadoDto.Exito("Bloques constructivos registrados correctamente.");
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
                var response = await _httpClient.PutAsync($"api/v1/catastro/predios/{id}/cambiar-estado", null);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return res ?? OperacionResultadoDto.Exito("Estado del predio modificado exitosamente.");
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
