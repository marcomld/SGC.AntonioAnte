using FluentValidation;
using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    public record AddBloqueCommand(
        Guid PredioId,
        int NumeroBloque,
        Guid TipoEstructuraId,
        Guid EstadoConservacionId,
        int NumeroPisos,
        decimal AreaConstruccion,
        int AnioConstruccion
    ) : IRequest<Guid>;

    public class AddBloqueCommandValidator : AbstractValidator<AddBloqueCommand>
    {
        public AddBloqueCommandValidator()
        {
            RuleFor(v => v.PredioId).NotEmpty().WithMessage("El ID del predio es obligatorio.");
            RuleFor(v => v.NumeroBloque).GreaterThan(0).WithMessage("El número de bloque debe ser un entero positivo.");
            RuleFor(v => v.TipoEstructuraId).NotEmpty().WithMessage("El tipo de estructura es obligatorio.");
            RuleFor(v => v.EstadoConservacionId).NotEmpty().WithMessage("El estado de conservación es obligatorio.");
            RuleFor(v => v.NumeroPisos).GreaterThanOrEqualTo(1).WithMessage("El número de pisos debe ser mayor o igual a 1.");
            RuleFor(v => v.AreaConstruccion).GreaterThan(0).WithMessage("El área de construcción debe ser mayor a 0.");
            RuleFor(v => v.AnioConstruccion).InclusiveBetween(1800, 2030).WithMessage("Ingrese un año de construcción válido.");
        }
    }

    public class AddBloqueCommandHandler : IRequestHandler<AddBloqueCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public AddBloqueCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(AddBloqueCommand request, CancellationToken cancellationToken)
        {
            var predio = await _context.Predios.FindAsync(new object[] { request.PredioId }, cancellationToken);
            if (predio == null || !predio.EstadoActivo)
                throw new Exception("El predio especificado no existe o se encuentra inactivo.");

            var tipoEstructura = await _context.TiposEstructura.FindAsync(new object[] { request.TipoEstructuraId }, cancellationToken);
            if (tipoEstructura == null || !tipoEstructura.EstadoActivo)
                throw new Exception("El tipo de estructura especificado no existe o se encuentra inactivo.");

            var estadoConservacion = await _context.EstadosConservacion.FindAsync(new object[] { request.EstadoConservacionId }, cancellationToken);
            if (estadoConservacion == null || !estadoConservacion.EstadoActivo)
                throw new Exception("El estado de conservación especificado no existe o se encuentra inactivo.");

            var nuevoBloque = BloqueConstruccion.Crear(
                request.PredioId,
                request.NumeroBloque,
                request.TipoEstructuraId,
                request.EstadoConservacionId,
                request.NumeroPisos,
                request.AreaConstruccion,
                request.AnioConstruccion
            );

            _context.BloquesConstruccion.Add(nuevoBloque);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoBloque.Id;
        }
    }
}
