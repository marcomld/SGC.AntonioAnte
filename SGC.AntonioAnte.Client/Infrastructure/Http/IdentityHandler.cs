using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using SGC.AntonioAnte.Client.Infrastructure.Security;
using SGC.AntonioAnte.Client.Services.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;

namespace SGC.AntonioAnte.Client.Infrastructure.Http
{
    public class IdentityHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public IdentityHandler(
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider,
            IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticionOriginal, CancellationToken cancellationToken)
        {
            string path = peticionOriginal.RequestUri?.AbsolutePath.ToLower() ?? string.Empty;

            if (path.Contains("/login") || path.Contains("/refresh-token") || path.Contains("/solicitar-recuperacion"))
            {
                return await base.SendAsync(peticionOriginal, cancellationToken);
            }

            bool esReintento = peticionOriginal.Headers.Contains("X-Is-Retry");

            var tokenActual = await _localStorage.GetItemAsync("authToken");

            if (!string.IsNullOrWhiteSpace(tokenActual))
            {
                tokenActual = tokenActual.Trim('"').Trim();
                peticionOriginal.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenActual);
            }

            var respuestaServidor = await base.SendAsync(peticionOriginal, cancellationToken);

            if (respuestaServidor.StatusCode == HttpStatusCode.Unauthorized && !esReintento)
            {
                bool renovacionExitosa = await IntentarRenovarTokenAsync(tokenActual, cancellationToken);

                if (renovacionExitosa)
                {
                    var nuevoToken = await _localStorage.GetItemAsync("authToken");
                    if (!string.IsNullOrWhiteSpace(nuevoToken))
                    {
                        nuevoToken = nuevoToken.Trim('"').Trim();
                    }

                    var peticionClonada = await ClonarPeticionAsync(peticionOriginal);
                    peticionClonada.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nuevoToken);
                    peticionClonada.Headers.Add("X-Is-Retry", "true");

                    return await base.SendAsync(peticionClonada, cancellationToken);
                }
                else
                {
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        await customProvider.NotificarLogout();
                    }
                }
            }

            return respuestaServidor;
        }

        private async Task<bool> IntentarRenovarTokenAsync(string? tokenQueFallo, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var accessGuardado = await _localStorage.GetItemAsync("authToken");
                var refreshGuardado = await _localStorage.GetItemAsync("refreshToken");

                if (!string.IsNullOrWhiteSpace(accessGuardado)) accessGuardado = accessGuardado.Trim('"').Trim();
                if (!string.IsNullOrWhiteSpace(refreshGuardado)) refreshGuardado = refreshGuardado.Trim('"').Trim();

                if (!string.IsNullOrWhiteSpace(accessGuardado) && accessGuardado != tokenQueFallo)
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(accessGuardado) || string.IsNullOrWhiteSpace(refreshGuardado))
                {
                    return false;
                }

                var payloadRenovacion = new RefreshTokenDto
                {
                    AccessToken = accessGuardado,
                    RefreshToken = refreshGuardado
                };

                var clienteDirecto = _httpClientFactory.CreateClient("AuthClient");
                var respuestaRefresco = await clienteDirecto.PostAsJsonAsync("api/v1/seguridad/auth/refresh-token", payloadRenovacion, cancellationToken);

                if (respuestaRefresco.IsSuccessStatusCode)
                {
                    var rawJson = await respuestaRefresco.Content.ReadAsStringAsync(cancellationToken);

                    string? nuevoAccessToken = null;
                    string? nuevoRefreshToken = null;

                    using (var doc = JsonDocument.Parse(rawJson))
                    {
                        var root = doc.RootElement;

                        // Si la respuesta viene envuelta en "data"
                        if (root.TryGetProperty("data", out var dataElem) || root.TryGetProperty("Data", out dataElem))
                        {
                            if (dataElem.ValueKind == JsonValueKind.Object)
                            {
                                if (dataElem.TryGetProperty("accessToken", out var at) || dataElem.TryGetProperty("AccessToken", out at))
                                    nuevoAccessToken = at.GetString();

                                if (dataElem.TryGetProperty("refreshToken", out var rt) || dataElem.TryGetProperty("RefreshToken", out rt))
                                    nuevoRefreshToken = rt.GetString();
                            }
                        }

                        // Si la respuesta viene directa en la raíz
                        if (string.IsNullOrWhiteSpace(nuevoAccessToken) && root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("accessToken", out var at) || root.TryGetProperty("AccessToken", out at))
                                nuevoAccessToken = at.GetString();

                            if (root.TryGetProperty("refreshToken", out var rt) || root.TryGetProperty("RefreshToken", out rt))
                                nuevoRefreshToken = rt.GetString();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(nuevoAccessToken) && !string.IsNullOrWhiteSpace(nuevoRefreshToken))
                    {
                        if (_authStateProvider is CustomAuthStateProvider customProvider)
                        {
                            var dto = new TokenResponseDto
                            {
                                AccessToken = nuevoAccessToken,
                                RefreshToken = nuevoRefreshToken
                            };
                            await customProvider.NotificarLogin(dto);
                        }
                        else
                        {
                            await _localStorage.SetItemAsync("authToken", nuevoAccessToken);
                            await _localStorage.SetItemAsync("refreshToken", nuevoRefreshToken);
                        }

                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<HttpRequestMessage> ClonarPeticionAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri)
            {
                Version = req.Version
            };

            if (req.Content != null)
            {
                var ms = new MemoryStream();
                await req.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (req.Content.Headers != null)
                {
                    foreach (var header in req.Content.Headers)
                    {
                        clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var prop in req.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
            }

            return clone;
        }
    }
}