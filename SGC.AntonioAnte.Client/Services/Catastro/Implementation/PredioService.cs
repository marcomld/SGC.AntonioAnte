using SGC.AntonioAnte.Client.Services.Catastro.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Net.Http.Json;

namespace SGC.AntonioAnte.Client.Services.Catastro.Implementation
{
    public class PredioService : IPredioService
    {
        private readonly HttpClient _httpClient;

        public PredioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResultadoPaginadoDto<PredioDto>> ObtenerPaginadoAsync(string? busqueda, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/catastro/predios?pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";
            }

            var response = await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<PredioDto>>(url);
            return response ?? new ResultadoPaginadoDto<PredioDto>();
        }

        public async Task<PredioDto?> ObtenerPorIdAsync(Guid id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PredioDto>($"api/v1/catastro/predios/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<OperacionResultadoDto> CrearPredioBaseAsync(CreatePredioDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/catastro/predios", dto);
            if (response.IsSuccessStatusCode)
                return OperacionResultadoDto.Exito("Predio base creado con éxito.");

            var error = await response.Content.ReadAsStringAsync();
            return OperacionResultadoDto.Fallo($"Error al registrar predio: {error}");
        }

        public async Task<OperacionResultadoDto> AgregarDominiosAsync(Guid predioId, List<AddDominioDto> dominios)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/catastro/predios/{predioId}/dominios", dominios);
            if (response.IsSuccessStatusCode)
                return OperacionResultadoDto.Exito("Dominios asignados correctamente.");

            var error = await response.Content.ReadAsStringAsync();
            return OperacionResultadoDto.Fallo($"Error al asignar dominios: {error}");
        }

        public async Task<OperacionResultadoDto> AgregarBloquesAsync(Guid predioId, List<AddBloqueDto> bloques)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/catastro/predios/{predioId}/bloques", bloques);
            if (response.IsSuccessStatusCode)
                return OperacionResultadoDto.Exito("Bloques constructivos registrados correctamente.");

            var error = await response.Content.ReadAsStringAsync();
            return OperacionResultadoDto.Fallo($"Error al registrar bloques: {error}");
        }
    }
}
