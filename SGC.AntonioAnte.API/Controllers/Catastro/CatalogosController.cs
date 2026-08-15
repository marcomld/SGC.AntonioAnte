using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Catastro.Catalogos.Commands;
using SGC.AntonioAnte.Application.Catastro.Catalogos.Queries;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos;

namespace SGC.AntonioAnte.API.Controllers.Catastro
{
    [ApiController]
    [Route("api/v1/catastro/catalogos")]
    [Authorize]
    public class CatalogosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CatalogosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- TIPOS DE TENENCIA ---
        [HttpGet("tipos-tenencia")]
        public async Task<IActionResult> GetTiposTenencia()
        {
            var resultado = await _mediator.Send(new GetTiposTenenciaQuery());
            return Ok(new { data = resultado, mensaje = "Tipos de tenencia recuperados exitosamente." });
        }

        [HttpPost("tipos-tenencia")]
        [Authorize(Roles = "AdminSistemas,SuperAdmin")]
        public async Task<IActionResult> CreateTipoTenencia([FromBody] CreateCatalogoDto dto)
        {
            var command = new CreateTipoTenenciaCommand(dto.Nombre, dto.Descripcion);
            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Tipo de tenencia registrado exitosamente." });
        }

        // --- TIPOS DE ESTRUCTURA ---
        [HttpGet("tipos-estructura")]
        public async Task<IActionResult> GetTiposEstructura()
        {
            var resultado = await _mediator.Send(new GetTiposEstructuraQuery());
            return Ok(new { data = resultado, mensaje = "Tipos de estructura recuperados exitosamente." });
        }

        [HttpPost("tipos-estructura")]
        [Authorize(Roles = "AdminSistemas,SuperAdmin")]
        public async Task<IActionResult> CreateTipoEstructura([FromBody] CreateCatalogoDto dto)
        {
            var command = new CreateTipoEstructuraCommand(dto.Nombre, dto.Descripcion);
            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Tipo de estructura registrado exitosamente." });
        }

        // --- ESTADOS DE CONSERVACIÓN ---
        [HttpGet("estados-conservacion")]
        public async Task<IActionResult> GetEstadosConservacion()
        {
            var resultado = await _mediator.Send(new GetEstadosConservacionQuery());
            return Ok(new { data = resultado, mensaje = "Estados de conservación recuperados exitosamente." });
        }

        [HttpPost("estados-conservacion")]
        [Authorize(Roles = "AdminSistemas,SuperAdmin")]
        public async Task<IActionResult> CreateEstadoConservacion([FromBody] CreateCatalogoDto dto)
        {
            var command = new CreateEstadoConservacionCommand(dto.Nombre, dto.Descripcion);
            var id = await _mediator.Send(command);
            return Ok(new { id, mensaje = "Estado de conservación registrado exitosamente." });
        }
    }
}
