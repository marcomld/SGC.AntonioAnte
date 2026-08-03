using SGC.AntonioAnte.Shared.DTOs.Seguridad;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IUsuarioService
    {
        Task<LoginResultadoDto> LoginAsync(LoginDto credenciales);
        Task LogoutAsync();
        Task<OperacionResultadoDto> SolicitarRecuperacionAsync(ForgotPasswordDto correoDto);
        Task<OperacionResultadoDto> RestablecerPasswordAsync(ResetPasswordDto restablecerDto);
        Task<List<CreateUsuarioDto>?> ObtenerTodosLosUsuariosAsync();
        Task<OperacionResultadoDto> RegistrarFuncionarioAsync(CreateUsuarioDto nuevoUsuario);
        Task<OperacionResultadoDto> AsignarPermisosAsync(string identificacion, AddClaimDto claimDto);
    }
}
