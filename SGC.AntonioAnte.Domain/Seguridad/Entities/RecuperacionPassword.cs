using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Seguridad.Entities
{
    public class RecuperacionPassword
    {
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string? CodigoOtpProtegido { get; set; }

        public DateTime FechaExpiracionOtp { get; set; }

        public DateTime FechaUltimoEnvio { get; set; }

        public int IntentosFallidos { get; set; }

        public DateTime? FechaValidacionOtp { get; set; }

        public string? ResetTokenHash { get; set; }

        public DateTime? FechaExpiracionResetToken { get; set; }

        public DateTime? FechaUtilizacion { get; set; }

        public string SecurityStamp { get; set; } = string.Empty;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
