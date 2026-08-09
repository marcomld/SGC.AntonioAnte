using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IUsuarioService
    {
        Task<ResultadoPaginadoDto<UsuarioResponseDto>?> ObtenerUsuariosPaginadosAsync(
            string? busqueda,
            bool? estadoActivo,
            Guid? departamentoId,
            int pagina = 1,
            int registrosPorPagina = 10);

        Task<List<UsuarioResponseDto>?> ObtenerTodosLosUsuariosAsync();
        Task<OperacionResultadoDto> RegistrarFuncionarioAsync(CreateUsuarioDto nuevoUsuario);
        Task<OperacionResultadoDto> AsignarPermisosAsync(Guid id, AddClaimDto claimDto);
        Task<OperacionResultadoDto> ActualizarFuncionarioAsync(Guid id, UpdateUsuarioDto usuarioDto);
        Task<OperacionResultadoDto> ActivarFuncionarioAsync(Guid id);
        Task<OperacionResultadoDto> DesactivarFuncionarioAsync(Guid id);
    }
}