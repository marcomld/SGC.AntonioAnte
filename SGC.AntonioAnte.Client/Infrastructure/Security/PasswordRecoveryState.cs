using System;

namespace SGC.AntonioAnte.Client.Infrastructure.Security
{
    public class PasswordRecoveryState
    {
        public string? Email { get; private set; }

        public string? ResetToken { get; private set; }

        public DateTimeOffset? FechaExpiracion { get; private set; }

        public bool TieneAutorizacion =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(ResetToken) &&
            FechaExpiracion.HasValue &&
            FechaExpiracion.Value > DateTimeOffset.UtcNow;

        public void Autorizar(
            string email,
            string resetToken,
            int expiraEnSegundos)
        {
            Email = email;
            ResetToken = resetToken;

            FechaExpiracion = DateTimeOffset.UtcNow
                .AddSeconds(expiraEnSegundos);
        }

        public void Limpiar()
        {
            Email = null;
            ResetToken = null;
            FechaExpiracion = null;
        }
    }
}