using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditorias;
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

        public async Task<ResultadoPaginadoDto<AuditLogResponseDto>?> ConsultarBitacoraAsync(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            Guid? usuarioId,
            string? accion,
            string? entidad,
            string? busqueda,
            int pagina = 1,
            int registrosPorPagina = 15)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pagina={pagina}",
                    $"registrosPorPagina={registrosPorPagina}"
                };

                if (fechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");

                if (fechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

                if (usuarioId.HasValue)
                    queryParams.Add($"usuarioId={usuarioId.Value}");

                if (!string.IsNullOrWhiteSpace(accion))
                    queryParams.Add($"accion={Uri.EscapeDataString(accion.Trim())}");

                if (!string.IsNullOrWhiteSpace(entidad))
                    queryParams.Add($"entidad={Uri.EscapeDataString(entidad.Trim())}");

                if (!string.IsNullOrWhiteSpace(busqueda))
                    queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda.Trim())}");

                string url = $"api/v1/seguridad/auditoria?{string.Join("&", queryParams)}";

                var respuesta = await _httpClient.GetAsync(url);
                if (respuesta.IsSuccessStatusCode)
                {
                    return await respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<AuditLogResponseDto>>(_jsonOptions);
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
