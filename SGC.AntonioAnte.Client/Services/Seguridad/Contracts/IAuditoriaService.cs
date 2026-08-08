using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auditorias;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IAuditoriaService
    {
        Task<ResultadoPaginadoDto<AuditLogResponseDto>?> ConsultarBitacoraAsync(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            Guid? usuarioId,
            string? accion,
            string? entidad,
            string? busqueda,
            int pagina = 1,
            int registrosPorPagina = 15);
    }
}
