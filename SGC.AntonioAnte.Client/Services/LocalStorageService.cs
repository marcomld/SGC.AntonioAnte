using Microsoft.JSInterop;
using SGC.AntonioAnte.Client.Services.Contracts;

namespace SGC.AntonioAnte.Client.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetItemAsync(string clave)
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", clave);
        }

        public async Task SetItemAsync(string clave, string valor)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", clave, valor);
        }

        public async Task RemoveItemAsync(string clave)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", clave);
        }
    }
}
