using SGC.AntonioAnte.Client.Services.Catastro.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Catastro.Implementation
{
    public class CatalogoService : ICatalogoService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CatalogoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Helper para extraer la data cuando la API responde { data: [...], mensaje: "..." }
        private async Task<List<CatalogoDto>> ExtraerLista(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("data", out var dataElement) || doc.RootElement.TryGetProperty("Data", out dataElement))
                {
                    return JsonSerializer.Deserialize<List<CatalogoDto>>(dataElement.GetRawText(), _jsonOptions) ?? new();
                }
            }
            return new List<CatalogoDto>();
        }

        public async Task<List<CatalogoDto>> ObtenerTiposTenenciaAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/catastro/catalogos/tipos-tenencia");
            return await ExtraerLista(response);
        }

        public async Task<List<CatalogoDto>> ObtenerTiposEstructuraAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/catastro/catalogos/tipos-estructura");
            return await ExtraerLista(response);
        }

        public async Task<List<CatalogoDto>> ObtenerEstadosConservacionAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/catastro/catalogos/estados-conservacion");
            return await ExtraerLista(response);
        }
    }
}
