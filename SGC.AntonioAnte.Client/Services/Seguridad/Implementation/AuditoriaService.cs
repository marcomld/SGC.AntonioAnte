using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditoria;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Implementation
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditoriaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<ResultadoPaginadoAuditDto?> ConsultarBitacoraAsync(
            DateTime? desde,
            DateTime? hasta,
            Guid? usuarioId,
            string? accion,
            string? entidad,
            string? busqueda,
            int pagina,
            int registrosPorPagina)
        {
            try
            {
                string url = $"api/v1/seguridad/auditoria?pagina={pagina}&registrosPorPagina={registrosPorPagina}";

                if (desde.HasValue) url += $"&desde={desde.Value:yyyy-MM-dd}";
                if (hasta.HasValue) url += $"&hasta={hasta.Value:yyyy-MM-dd}";
                if (usuarioId.HasValue) url += $"&usuarioId={usuarioId.Value}";
                if (!string.IsNullOrWhiteSpace(accion)) url += $"&accion={Uri.EscapeDataString(accion)}";
                if (!string.IsNullOrWhiteSpace(entidad)) url += $"&entidad={Uri.EscapeDataString(entidad)}";
                if (!string.IsNullOrWhiteSpace(busqueda)) url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

                var respuesta = await _httpClient.GetAsync(url);
                if (respuesta.IsSuccessStatusCode)
                {
                    var resJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<ResultadoPaginadoAuditDto>>(_jsonOptions);
                    return resJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar bitácora: {ex.Message}");
            }
            return null;
        }
    }
}
