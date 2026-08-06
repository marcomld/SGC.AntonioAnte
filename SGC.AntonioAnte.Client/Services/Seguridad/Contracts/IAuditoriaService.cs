using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditoria;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IAuditoriaService
    {
        Task<ResultadoPaginadoAuditDto?> ConsultarBitacoraAsync(
            DateTime? desde,
            DateTime? hasta,
            Guid? usuarioId,
            string? accion,
            string? entidad,
            string? busqueda,
            int pagina,
            int registrosPorPagina);
    }
}
