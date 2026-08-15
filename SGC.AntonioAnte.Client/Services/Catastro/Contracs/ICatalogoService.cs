using SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracs
{
    public interface ICatalogoService
    {
        Task<List<CatalogoDto>> ObtenerTiposTenenciaAsync();
        Task<List<CatalogoDto>> ObtenerTiposEstructuraAsync();
        Task<List<CatalogoDto>> ObtenerEstadosConservacionAsync();
    }
}
