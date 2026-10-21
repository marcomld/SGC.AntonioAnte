using SGC.AntonioAnte.Client.Services.Avaluos.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Client.Services.Avaluos.Implementation
{
    public class AvaluosService : IAvaluosService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AvaluosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<(bool Exitoso, string Mensaje, AvaluoSimuladoDto? Datos)> SimularAvaluoAsync(Guid predioId, int anioFiscal)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/avaluos/simulaciones/{predioId}/anio/{anioFiscal}");
                var rawContent = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(rawContent);
                string mensaje = doc.RootElement.TryGetProperty("mensaje", out var mElem) ? mElem.GetString() ?? "" : "";

                if (response.IsSuccessStatusCode)
                {
                    // Buscamos la propiedad "datos" o "data" del OperacionResultadoDto
                    if (doc.RootElement.TryGetProperty("datos", out var dataElem) || doc.RootElement.TryGetProperty("data", out dataElem))
                    {
                        var avaluo = JsonSerializer.Deserialize<AvaluoSimuladoDto>(dataElem.GetRawText(), _jsonOptions);
                        return (true, mensaje, avaluo);
                    }
                    return (false, "No se encontraron los datos de la simulación.", null);
                }

                return (false, string.IsNullOrWhiteSpace(mensaje) ? "Error del servidor." : mensaje, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error de comunicación: {ex.Message}", null);
            }
        }
    }
}