using SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracts
{
    public interface ICatalogoService
    {
        Task<List<CatalogoDto>> ObtenerTiposTenenciaAsync();
        Task<List<CatalogoDto>> ObtenerTiposEstructuraAsync();
        Task<List<CatalogoDto>> ObtenerEstadosConservacionAsync();
    }
}
