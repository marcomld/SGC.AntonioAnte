namespace SGC.AntonioAnte.Client.Services.Contracts
{
    public interface ILocalStorageService
    {
        Task<string?> GetItemAsync(string clave);
        Task SetItemAsync(string clave, string valor);
        Task RemoveItemAsync(string clave);
    }
}
