using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SGC.AntonioAnte.Client.Services.Avaluos.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using SGC.AntonioAnte.Shared.DTOs.Common;

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
                string mensaje = doc.RootElement.TryGetProperty("mensaje", out var mElem) || doc.RootElement.TryGetProperty("Mensaje", out mElem)
                    ? mElem.GetString() ?? ""
                    : "";

                if (response.IsSuccessStatusCode)
                {
                    if (doc.RootElement.TryGetProperty("datos", out var dataElem) || doc.RootElement.TryGetProperty("data", out dataElem) || doc.RootElement.TryGetProperty("Datos", out dataElem))
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

        public async Task<(bool Existe, string Mensaje, LiquidacionVigenteDto? Datos)> ObtenerLiquidacionVigenteAsync(Guid predioId, int anioFiscal)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/avaluos/liquidaciones/predio/{predioId}/anio/{anioFiscal}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (false, "No existe liquidación vigente para el año consultado.", null);
                }

                var rawContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(rawContent))
                {
                    return (false, "Sin respuesta del servidor.", null);
                }

                using var doc = JsonDocument.Parse(rawContent);
                string mensaje = doc.RootElement.TryGetProperty("mensaje", out var mElem) || doc.RootElement.TryGetProperty("Mensaje", out mElem)
                    ? mElem.GetString() ?? ""
                    : "";

                if (response.IsSuccessStatusCode)
                {
                    if (doc.RootElement.TryGetProperty("exitoso", out var eElem) || doc.RootElement.TryGetProperty("Exitoso", out eElem))
                    {
                        if (eElem.ValueKind == JsonValueKind.False)
                            return (false, mensaje, null);
                    }

                    if (doc.RootElement.TryGetProperty("datos", out var dataElem) || doc.RootElement.TryGetProperty("data", out dataElem) || doc.RootElement.TryGetProperty("Datos", out dataElem))
                    {
                        if (dataElem.ValueKind == JsonValueKind.Null)
                            return (false, mensaje, null);

                        var liquidacion = JsonSerializer.Deserialize<LiquidacionVigenteDto>(dataElem.GetRawText(), _jsonOptions);
                        return (liquidacion != null, mensaje, liquidacion);
                    }

                    // Fallback por si el endpoint devuelve el DTO directo sin wrapper
                    var dtoDirecto = JsonSerializer.Deserialize<LiquidacionVigenteDto>(rawContent, _jsonOptions);
                    return (dtoDirecto != null, mensaje, dtoDirecto);
                }

                return (false, mensaje, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al consultar liquidación vigente: {ex.Message}", null);
            }
        }

        public async Task<OperacionResultadoDto> EmitirLiquidacionAsync(EmitirLiquidacionDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/liquidaciones/emitir", dto);
            return await ProcesarRespuestaComando(response, "Obligación fiscal emitida correctamente.");
        }

        public async Task<OperacionResultadoDto> ReliquidarAvaluoAsync(ReliquidarLiquidacionDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/avaluos/liquidaciones/reliquidar", dto);
            return await ProcesarRespuestaComando(response, "Baja y reliquidación procesada correctamente.");
        }

        private async Task<OperacionResultadoDto> ProcesarRespuestaComando(HttpResponseMessage response, string mensajeExitoDefault)
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