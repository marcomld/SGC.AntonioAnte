namespace SGC.AntonioAnte.Client.Services.Contracts
{
    public interface ILocalStorageService
    {
        Task<string?> GetItemAsync(string clave);
        Task SetItemAsync(string clave, string valor);
        Task RemoveItemAsync(string clave);

        // Métodos para sesión volátil
        Task<string?> GetSessionItemAsync(string clave);
        Task SetSessionItemAsync(string clave, string valor);
        Task RemoveSessionItemAsync(string clave);
    }
}
