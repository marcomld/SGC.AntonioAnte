using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Policies
{
    public static class RecuperacionPasswordPolicy
    {
        public const int LongitudOtp = 6;

        public const int MinutosVigenciaOtp = 5;

        public const int MinutosVigenciaResetToken = 10;

        public const int MaximoIntentos = 5;

        public const int SegundosReenvio = 120;

        public const string PropositoProteccionOtp =
            "SGC.Seguridad.RecuperacionPassword.OTP.v1";

        public const string MensajeOtpInvalido =
            "El código de verificación es incorrecto, ha expirado " +
            "o ya fue utilizado.";

        public const string MensajeTokenInvalido =
            "La autorización de recuperación es inválida " +
            "o ha expirado.";

        public const string MensajeSolicitud =
            "Si el correo está registrado y habilitado, " +
            "recibirá un código de recuperación.";

        public static string GenerarOtp()
        {
            return RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6", CultureInfo.InvariantCulture);
        }

        public static string CalcularHashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        public static bool CompararSeguro(
            string valorEsperado,
            string valorRecibido)
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(valorEsperado),
                Encoding.UTF8.GetBytes(valorRecibido));
        }
    }
}
