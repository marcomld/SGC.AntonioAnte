using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.CreateUsuario
{
    public class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
    {
        public CreateUsuarioCommandValidator()
        {
            // TAREA 2: Validación de la Cédula Ecuatoriana (Módulo 10)
            RuleFor(x => x.Identificacion)
                .NotEmpty().WithMessage("La identificación (Cédula) es totalmente obligatoria.")
                .Length(10).WithMessage("La cédula ecuatoriana debe contener exactamente 10 dígitos.")
                .Must(ValidarCedulaEcuatoriana).WithMessage("La identificación ingresada no es una cédula válida para la República del Ecuador.");

            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Los nombres completos del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los nombres no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Los apellidos completos del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los apellidos no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico institucional es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");

            RuleFor(x => x.RolAsignado)
                .NotEmpty().WithMessage("Debe asignar un rol al funcionario para el sistema de catastros.");
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10) return false;
            if (!long.TryParse(cedula, out _)) return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24) return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito < 0 || tercerDigito > 5) return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula.Substring(i, 1)) * coeficientes[i];
                if (valor > 9) valor -= 9;
                suma += valor;
            }

            int digitoVerificador = int.Parse(cedula.Substring(9, 1));
            int decenaSuperior = ((suma + 9) / 10) * 10;
            int resultado = decenaSuperior - suma;

            if (resultado == 10) resultado = 0;

            return resultado == digitoVerificador;
        }
    }
}
