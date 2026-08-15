using SGC.AntonioAnte.Client.Services.Catastro.Contracs;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System.Net.Http.Json;

namespace SGC.AntonioAnte.Client.Services.Catastro.Implementation
{
    public class PropietarioService : IPropietarioService
    {
        private readonly HttpClient _httpClient;

        public PropietarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResultadoPaginadoDto<PropietarioDto>> ObtenerPaginadoAsync(string? busqueda, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/catastro/propietarios?pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";
            }

            var response = await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<PropietarioDto>>(url);
            return response ?? new ResultadoPaginadoDto<PropietarioDto>();
        }

        public async Task<PropietarioDto?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PropietarioDto>($"api/v1/catastro/propietarios/{identificacion}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // Retorna null si no existe, útil para la búsqueda en la Ficha Catastral
            }
        }

        public async Task<OperacionResultadoDto> CrearAsync(CreatePropietarioDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/catastro/propietarios", dto);
            if (response.IsSuccessStatusCode)
                return OperacionResultadoDto.Exito("Propietario registrado exitosamente.");

            // Si falla, intentamos leer el mensaje de error del backend
            var error = await response.Content.ReadAsStringAsync();
            return OperacionResultadoDto.Fallo($"Error al crear: {error}");
        }

        public async Task<OperacionResultadoDto> ActualizarAsync(Guid id, UpdatePropietarioDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/catastro/propietarios/{id}", dto);
            if (response.IsSuccessStatusCode)
                return OperacionResultadoDto.Exito("Propietario actualizado exitosamente.");

            var error = await response.Content.ReadAsStringAsync();
            return OperacionResultadoDto.Fallo($"Error al actualizar: {error}");
        }
    }
}
