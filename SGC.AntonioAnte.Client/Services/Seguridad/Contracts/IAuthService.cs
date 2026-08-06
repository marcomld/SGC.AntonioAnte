using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IAuthService
    {
        Task<LoginResultadoDto> LoginAsync(LoginDto credenciales);
        Task LogoutAsync();
        Task<OperacionResultadoDto> SolicitarRecuperacionAsync(ForgotPasswordDto correoDto);
        Task<OperacionResultadoDto> RestablecerPasswordAsync(ResetPasswordDto restablecerDto);
    }
}
