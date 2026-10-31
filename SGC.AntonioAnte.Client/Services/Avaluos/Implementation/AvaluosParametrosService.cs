using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SGC.AntonioAnte.Client.Services.Avaluos.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Avaluos.Implementation
{
    public class AvaluosParametrosService : IAvaluosParametrosService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AvaluosParametrosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ==========================================
        // 1. COSTOS DE REPOSICIÓN
        // ==========================================
        public async Task<ResultadoPaginadoDto<CostoReposicionDto>> ObtenerCostosReposicionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/avaluos/costos-reposicion?anioFiscal={anioFiscal}&pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            return await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<CostoReposicionDto>>(url, _jsonOptions)
                   ?? new ResultadoPaginadoDto<CostoReposicionDto>();
        }

        public async Task<OperacionResultadoDto> CrearCostoReposicionAsync(CreateCostoReposicionDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/costos-reposicion", dto);
            return await ProcesarRespuesta(response, "Costo de reposición registrado correctamente.");
        }

        public async Task<OperacionResultadoDto> ActualizarCostoReposicionAsync(Guid id, UpdateCostoReposicionDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/avaluos/costos-reposicion/{id}", dto);
            return await ProcesarRespuesta(response, "Costo de reposición actualizado correctamente.");
        }

        public async Task<OperacionResultadoDto> CambiarEstadoCostoReposicionAsync(Guid id)
        {
            var response = await _httpClient.PutAsync($"api/v1/avaluos/costos-reposicion/{id}/cambiar-estado", null);
            return await ProcesarRespuesta(response, "Estado actualizado correctamente.");
        }

        // ==========================================
        // 2. PRECIOS BASE DE SUELO
        // ==========================================
        public async Task<ResultadoPaginadoDto<PrecioBaseSueloDto>> ObtenerPreciosSueloPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/avaluos/precios-suelo?anioFiscal={anioFiscal}&pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            return await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<PrecioBaseSueloDto>>(url, _jsonOptions)
                   ?? new ResultadoPaginadoDto<PrecioBaseSueloDto>();
        }

        public async Task<OperacionResultadoDto> CrearPrecioSueloAsync(CreatePrecioBaseSueloDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/precios-suelo", dto);
            return await ProcesarRespuesta(response, "Precio base de suelo registrado correctamente.");
        }

        public async Task<OperacionResultadoDto> ActualizarPrecioSueloAsync(Guid id, UpdatePrecioBaseSueloDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/avaluos/precios-suelo/{id}", dto);
            return await ProcesarRespuesta(response, "Precio base de suelo actualizado correctamente.");
        }

        public async Task<OperacionResultadoDto> CambiarEstadoPrecioSueloAsync(Guid id)
        {
            var response = await _httpClient.PutAsync($"api/v1/avaluos/precios-suelo/{id}/cambiar-estado", null);
            return await ProcesarRespuesta(response, "Estado actualizado correctamente.");
        }

        // ==========================================
        // 3. FACTORES DE DEPRECIACIÓN
        // ==========================================
        public async Task<ResultadoPaginadoDto<FactorDepreciacionDto>> ObtenerFactoresDepreciacionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/avaluos/factores-depreciacion?anioFiscal={anioFiscal}&pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            return await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<FactorDepreciacionDto>>(url, _jsonOptions)
                   ?? new ResultadoPaginadoDto<FactorDepreciacionDto>();
        }

        public async Task<OperacionResultadoDto> CrearFactorDepreciacionAsync(CreateFactorDepreciacionDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/factores-depreciacion", dto);
            return await ProcesarRespuesta(response, "Factor de depreciación registrado correctamente.");
        }

        public async Task<OperacionResultadoDto> ActualizarFactorDepreciacionAsync(Guid id, UpdateFactorDepreciacionDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/avaluos/factores-depreciacion/{id}", dto);
            return await ProcesarRespuesta(response, "Factor de depreciación actualizado correctamente.");
        }

        public async Task<OperacionResultadoDto> CambiarEstadoFactorDepreciacionAsync(Guid id)
        {
            var response = await _httpClient.PutAsync($"api/v1/avaluos/factores-depreciacion/{id}/cambiar-estado", null);
            return await ProcesarRespuesta(response, "Estado actualizado correctamente.");
        }

        // ==========================================
        // 4. FACTORES DE AFECTACIÓN
        // ==========================================
        public async Task<ResultadoPaginadoDto<FactorAfectacionDto>> ObtenerFactoresAfectacionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina)
        {
            var url = $"api/v1/avaluos/factores-afectacion?anioFiscal={anioFiscal}&pagina={pagina}&registrosPorPagina={registrosPorPagina}";
            return await _httpClient.GetFromJsonAsync<ResultadoPaginadoDto<FactorAfectacionDto>>(url, _jsonOptions)
                   ?? new ResultadoPaginadoDto<FactorAfectacionDto>();
        }

        public async Task<OperacionResultadoDto> CrearFactorAfectacionAsync(CreateFactorAfectacionDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/factores-afectacion", dto);
            return await ProcesarRespuesta(response, "Factor de afectación registrado correctamente.");
        }

        public async Task<OperacionResultadoDto> ActualizarFactorAfectacionAsync(Guid id, UpdateFactorAfectacionDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/avaluos/factores-afectacion/{id}", dto);
            return await ProcesarRespuesta(response, "Factor de afectación actualizado correctamente.");
        }

        public async Task<OperacionResultadoDto> CambiarEstadoFactorAfectacionAsync(Guid id)
        {
            var response = await _httpClient.PutAsync($"api/v1/avaluos/factores-afectacion/{id}/cambiar-estado", null);
            return await ProcesarRespuesta(response, "Estado actualizado correctamente.");
        }

        // ==========================================
        // HELPER TRANSVERSAL DE RESPUESTA
        // ==========================================
        private async Task<OperacionResultadoDto> ProcesarRespuesta(HttpResponseMessage response, string mensajeExitoDefault)
        {
            var rawContent = await response.Content.ReadAsStringAsync();
            string? mensajeServidor = null;
            bool? exitosoServidor = null;

            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                try
                {
                    using var doc = JsonDocument.Parse(rawContent);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("mensaje", out var mElem) || doc.RootElement.TryGetProperty("Mensaje", out mElem))
                            mensajeServidor = mElem.GetString();

                        if (doc.RootElement.TryGetProperty("exitoso", out var eElem) || doc.RootElement.TryGetProperty("Exitoso", out eElem))
                        {
                            if (eElem.ValueKind == JsonValueKind.True || eElem.ValueKind == JsonValueKind.False)
                                exitosoServidor = eElem.GetBoolean();
                        }
                    }
                }
                catch { }
            }

            if (response.IsSuccessStatusCode)
            {
                if (exitosoServidor == false)
                    return OperacionResultadoDto.Fallo(mensajeServidor ?? "No se pudo completar la operación.");

                return OperacionResultadoDto.Exito(string.IsNullOrWhiteSpace(mensajeServidor) ? mensajeExitoDefault : mensajeServidor);
            }

            return OperacionResultadoDto.Fallo(string.IsNullOrWhiteSpace(mensajeServidor) ? "Error en la operación del servidor." : mensajeServidor);
        }
    }
}