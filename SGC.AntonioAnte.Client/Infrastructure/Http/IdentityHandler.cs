using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SGC.AntonioAnte.Client.Infrastructure.Security;
using SGC.AntonioAnte.Client.Services.Contracts;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System.Net.Http; // Agregado para IHttpClientFactory

namespace SGC.AntonioAnte.Client.Infrastructure.Http
{
    public class IdentityHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;

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
            var tokenActual = await _localStorage.GetItemAsync("authToken");

            if (!string.IsNullOrWhiteSpace(tokenActual))
            {
                peticionOriginal.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenActual);
            }

            var respuestaServidor = await base.SendAsync(peticionOriginal, cancellationToken);

            if (respuestaServidor.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool renovacionExitosa = await IntentarRenovarTokenAsync();

                if (renovacionExitosa)
                {
                    var nuevoToken = await _localStorage.GetItemAsync("authToken");
                    peticionOriginal.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nuevoToken);

                    // Reintentamos la petición original transparente para el usuario
                    return await base.SendAsync(peticionOriginal, cancellationToken);
                }
                else
                {
                    // CORRECCIÓN: Uso correcto de pattern matching en C# (palabra clave 'is')
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        await customProvider.NotificarLogout();
                    }
                }
            }

            return respuestaServidor;
        }

        private async Task<bool> IntentarRenovarTokenAsync()
        {
            var accessGuardado = await _localStorage.GetItemAsync("authToken");
            var refreshGuardado = await _localStorage.GetItemAsync("refreshToken");

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

            var respuestaRefresco = await clienteDirecto.PostAsJsonAsync("api/v1/seguridad/usuarios/refresh-token", payloadRenovacion);

            if (respuestaRefresco.IsSuccessStatusCode)
            {
                var nuevosTokens = await respuestaRefresco.Content.ReadFromJsonAsync<TokenResponseDto>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (nuevosTokens != null && !string.IsNullOrWhiteSpace(nuevosTokens.AccessToken))
                {
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        await customProvider.NotificarLogin(nuevosTokens);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}