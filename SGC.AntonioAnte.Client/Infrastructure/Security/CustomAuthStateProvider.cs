using Microsoft.AspNetCore.Components.Authorization;
using SGC.AntonioAnte.Client.Services.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System.Security.Claims;

namespace SGC.AntonioAnte.Client.Infrastructure.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _estadoAnonimo;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _estadoAnonimo = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var tokenGuardado = await _localStorage.GetItemAsync("authToken");

            if (string.IsNullOrWhiteSpace(tokenGuardado))
            {
                return _estadoAnonimo;
            }

            var claimsObtenidos = JwtParser.ParseClaimsFromJwt(tokenGuardado);
            var identidadUsuario = new ClaimsIdentity(claimsObtenidos, "jwt");
            var usuarioPrincipal = new ClaimsPrincipal(identidadUsuario);

            return new AuthenticationState(usuarioPrincipal);
        }

        public async Task NotificarLogin(TokenResponseDto tokensRecibidos)
        {
            await _localStorage.SetItemAsync("authToken", tokensRecibidos.AccessToken);
            await _localStorage.SetItemAsync("refreshToken", tokensRecibidos.RefreshToken);

            var claimsNuevos = JwtParser.ParseClaimsFromJwt(tokensRecibidos.AccessToken);
            var identidadAutenticada = new ClaimsIdentity(claimsNuevos, "jwt");
            var usuarioPrincipal = new ClaimsPrincipal(identidadAutenticada);

            var estadoAutenticacion = Task.FromResult(new AuthenticationState(usuarioPrincipal));
            NotifyAuthenticationStateChanged(estadoAutenticacion);
        }

        public async Task NotificarLogout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");

            var estadoDesconectado = Task.FromResult(_estadoAnonimo);
            NotifyAuthenticationStateChanged(estadoDesconectado);
        }
    }
}
