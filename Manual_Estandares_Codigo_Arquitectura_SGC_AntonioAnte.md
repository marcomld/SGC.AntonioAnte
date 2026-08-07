# 📖 Manual de Estándares de Código y Arquitectura

**Proyecto:** `SGC.AntonioAnte` (Sistema de Gestión Catastral y Municipal - GAD Municipal de Antonio Ante)  
**Arquitectura:** Clean Architecture + CQRS (Command Query Responsibility Segregation) + Pattern Mediator  
**Tecnologías:** .NET 8 / C# 12, Entity Framework Core, ASP.NET Core Web API, Blazor WebAssembly  
**Documento Versión:** 1.0.0  
**Fecha de Última Revisión:** Agosto 2026  

---

## 📋 Tabla de Contenidos
1. [Visión General y Justificación Arquitectónica](#1-visión-general-y-justificación-arquitectónica)
2. [Matriz Estándar de Selección de Tipos C# (class vs record)](#2-matriz-estándar-de-selección-de-tipos-c-class-vs-record)
3. [Estructura del Proyecto y Capas de la Arquitectura Limpia](#3-estructura-del-proyecto-y-capas-de-la-arquitectura-limpia)
4. [Flujo Estándar A: Operaciones de ESCRITURA (Comandos)](#4-flujo-estándar-a-operaciones-de-escritura-comandos)
5. [Flujo Estándar B: Operaciones de LECTURA (Consultas)](#5-flujo-estándar-b-operaciones-de-lectura-consultas)
6. [Estructura Uniforme de Carpetas y Vertical Slicing](#6-estructura-uniforme-de-carpetas-y-vertical-slicing)
7. [Transversales (Cross-Cutting Concerns) y Patrones Avanzados](#7-transversales-cross-cutting-concerns-y-patrones-avanzados)
8. [Guía de Defensa Académica / Tribunal de Tesis](#8-guía-de-defensa-académica--tribunal-de-tesis)

---

## 1. Visión General y Justificación Arquitectónica

El presente manual establece los estándares formales de programación, diseño y estructuración de software para el desarrollo del **Sistema de Gestión Catastral (SGC.AntonioAnte)**. 

La combinación de **Clean Architecture** y **CQRS** garantiza:
* **Mantenibilidad:** Separación estricta de responsabilidades en capas concéntricas aisladas de dependencias externas.
* **Escalabilidad y Desacoplamiento:** Las reglas de negocio residen en el núcleo (`Domain` / `Application`) independientemente del framework UI (Blazor WASM) o del motor de persistencia (SQL Server).
* **Auditabilidad Integral:** Registro automático e inalterable de operaciones sobre predios, contribuyentes y fichas catastrales.
* **Rendimiento Optimizado:** Consultas de lectura directas y ligeras sin sobrecarga del rastreador de estados (*ChangeTracker*), combinadas con comandos de escritura fuertemente convalidados.

---

## 2. Matriz Estándar de Selección de Tipos C# (`class` vs `record`)

En C# 9+, la decisión entre declarar un tipo como `class` o `record` no es estética, sino una decisión técnica fundamental basada en la mutabilidad, la semántica de igualdad y la interoperabilidad con los frameworks utilitarios.

### 📊 Tabla Comparativa de Selección de Tipos

| Capa | Ubicación / Proyecto | Tipo C# a Usar | Mutabilidad | Razón Técnica y Arquitectónica |
| :--- | :--- | :--- | :--- | :--- |
| **Domain** | `SGC.AntonioAnte.Domain` | `class` | Mutable (`{ get; set; }` / métodos con estado) | Entity Framework Core requiere clases de referencia estándar con identidad de clave primaria (`Id`) para que el `ChangeTracker` supervise cambios de estado (`Added`, `Modified`, `Deleted`) y mapee a tablas en SQL Server. |
| **Shared** | `SGC.AntonioAnte.Shared` | `class` | Mutable (`{ get; set; }`) | Requerido por componentes de UI en Blazor (`EditForm`, `InputText`). Permite *Two-Way Data Binding* (`@bind-Value`) y la integración directa con validaciones con atributos `[DataAnnotations]` sin regenerar objetos en memoria. |
| **Application** | `SGC.AntonioAnte.Application` | `record` | Inmutable (`init-only` / constructores posicionales) | Representa mensajes inmutables de intenciones de uso (IRequest<T>). Proporciona semántica de igualdad por valor, sintaxis concisa de una sola línea, inmutabilidad segura para hilos y *logs* de auditoría/diagnóstico limpios (`ToString()` predeterminado). |
| **Infrastructure** | `SGC.AntonioAnte.Infrastructure` | `class` | N/A | Clases de configuración de Fluent API (`IEntityTypeConfiguration<T>`), clases derivadas de `DbContext`, interceptores y adaptadores de servicios externos (servicios de correo, PDF, firma electrónica). |

---

## 3. Estructura del Proyecto y Capas de la Arquitectura Limpia

El sistema sigue el modelo de capas concéntricas con la **Regla de Dependencia**: *las dependencias del código fuente solo pueden apuntar hacia adentro.*

```
                 +-------------------------------------------------+
                 |                PRESENTATION                     |
                 |      [ SGC.AntonioAnte.Client (Blazor) ]        |
                 |      [ SGC.AntonioAnte.Server (Web API) ]       |
                 +------------------------+------------------------+
                                          |
                                          v
                 +-------------------------------------------------+
                 |               INFRASTRUCTURE                    |
                 |    [ SGC.AntonioAnte.Infrastructure ]          |
                 |    - EF Core, SQL Server, Interceptors          |
                 +------------------------+------------------------+
                                          |
                                          v
                 +-------------------------------------------------+
                 |                APPLICATION                      |
                 |    [ SGC.AntonioAnte.Application ]              |
                 |    - CQRS (Commands, Queries, Handlers)         |
                 |    - FluentValidation, MediatR Behaviors        |
                 +------------------------+------------------------+
                                          |
                                          v
                 +-------------------------------------------------+
                 |                  DOMAIN                         |
                 |    [ SGC.AntonioAnte.Domain ]                   |
                 |    - Entidades, Objetos de Valor, Eventos       |
                 +-------------------------------------------------+
                                          ^
                                          |
                 +------------------------+------------------------+
                 |                  SHARED                         |
                 |    [ SGC.AntonioAnte.Shared ]                   |
                 |    - DTOs, Enums, Contratos de Interfaz         |
                 +-------------------------------------------------+
```

---

## 4. Flujo Estándar A: Operaciones de ESCRITURA (Comandos)
*(Crear, Editar, Desactivar, Eliminar)*

Para mantener una arquitectura homogénea y defendible ante un tribunal de tesis, toda acción que modifique el estado del sistema debe seguir de forma estricta un flujo de **5 pasos desacoplados**:

### 🔄 Diagrama de Flujo de Trabajo (Comandos)

```
[ Client: Blazor WASM ]
       │
       │ (1. Completa formulario con [DataAnnotations] en UpdateDepartamentoDto)
       ▼
[ Web API Controller ]
       │
       │ (2. Recibe DTO por HTTP PUT/POST, crea el record Command con los datos)
       ▼
[ Application: Command & MediatR ]
       │
       │ (3. Invocación: _mediator.Send(command) -> Pasa por Pipeline Behaviors: Validation & Audit)
       ▼
[ Application: Command Handler ]
       │
       │ (4. Procesa reglas de negocio, carga la Entidad del Domain y actualiza su estado)
       ▼
[ Infrastructure: DbContext & SQL Server ]
       │
       │ (5. Invocación: SaveChangesAsync() -> Interceptor ejecuta Auditoría automática en SQL)
       ▼
[ Base de Datos SQL Server ]
```

---

### 📄 Ejemplo Estándar Completo de Implementación: Editar Departamento

#### 1. DTO de Transferencia UI (Capa `Shared`)
*Ruta: `SGC.AntonioAnte.Shared/DTOs/Departamentos/UpdateDepartamentoDto.cs`*

```csharp
using System.ComponentModel.DataAnnotations;

namespace SGC.AntonioAnte.Shared.DTOs.Departamentos;

public class UpdateDepartamentoDto
{
    [Required(ErrorMessage = "El nombre del departamento es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
```

#### 2. Command Message (Capa `Application`)
*Ruta: `SGC.AntonioAnte.Application/Departamentos/Commands/UpdateDepartamento/UpdateDepartamentoCommand.cs`*

```csharp
using MediatR;
using SGC.AntonioAnte.Shared.Responses;

namespace SGC.AntonioAnte.Application.Departamentos.Commands.UpdateDepartamento;

/// <summary>
/// Mensaje inmutable que representa la solicitud de actualización de un departamento.
/// </summary>
public record UpdateDepartamentoCommand(
    Guid Id, 
    string Nombre, 
    string Descripcion, 
    bool Activo
) : IRequest<Result<bool>>;
```

#### 3. Command Handler (Capa `Application`)
*Ruta: `SGC.AntonioAnte.Application/Departamentos/Commands/UpdateDepartamento/UpdateDepartamentoCommandHandler.cs`*

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.Responses;

namespace SGC.AntonioAnte.Application.Departamentos.Commands.UpdateDepartamento;

public class UpdateDepartamentoCommandHandler : IRequestHandler<UpdateDepartamentoCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartamentoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateDepartamentoCommand request, CancellationToken cancellationToken)
    {
        var departamento = await _context.Departamentos
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (departamento == null)
        {
            return Result<bool>.Failure($"El departamento con ID '{request.Id}' no fue encontrado.");
        }

        // Aplicar cambios sobre la entidad de dominio
        departamento.Nombre = request.Nombre;
        departamento.Descripcion = request.Descripcion;
        departamento.Activo = request.Activo;

        // SaveChangesAsync dispara automáticamente el Interceptor de Auditoría en Infrastructure
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Departamento actualizado correctamente.");
    }
}
```

#### 4. Controller Web API (Capa `Server / Web API`)
*Ruta: `SGC.AntonioAnte.Server/Controllers/DepartamentosController.cs`*

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Departamentos.Commands.UpdateDepartamento;
using SGC.AntonioAnte.Shared.DTOs.Departamentos;

namespace SGC.AntonioAnte.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartamentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartamentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartamentoDto dto)
    {
        var command = new UpdateDepartamentoCommand(id, dto.Nombre, dto.Descripcion, dto.Activo);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
```

---

## 5. Flujo Estándar B: Operaciones de LECTURA (Consultas)
*(Listar, Buscar por ID, Filtrar, Paginación)*

Para consultas de lectura, se elimina la sobrecarga de seguimiento de entidades. **Regla de Oro:** *Nunca exponer las entidades puras del Domain hacia el cliente Blazor WASM. Toda lectura se proyecta directamente a DTOs de lectura.*

### 🔄 Diagrama de Flujo de Trabajo (Consultas)

```
[ Client: Blazor WASM ]
       │
       │ (1. Solicita datos mediante GET con filtros o paginación en Query String)
       ▼
[ Web API Controller ]
       │
       │ (2. Mapea la petición HTTP GET a un record Query)
       ▼
[ Application: Query & MediatR ]
       │
       │ (3. Invocación: _mediator.Send(query))
       ▼
[ Application: Query Handler ]
       │
       │ (4. Ejecuta consulta SQL con .AsNoTracking() + .Select() proyectado directamente a DTO)
       ▼
[ Client: Blazor WASM ] <── (5. Recibe respuesta DTO inmutable lista para renderizar en tablas/grids)
```

---

### 📄 Ejemplo Estándar Completo de Implementación: Consultar Auditorías

#### 1. DTO de Respuesta (Capa `Shared`)
*Ruta: `SGC.AntonioAnte.Shared/DTOs/Auditoria/AuditLogResponseDto.cs`*

```csharp
namespace SGC.AntonioAnte.Shared.DTOs.Auditoria;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string ClavePrimaria { get; set; } = string.Empty;
    public string ValoresAntiguos { get; set; } = string.Empty;
    public string ValoresNuevos { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
```

#### 2. Query Message (Capa `Application`)
*Ruta: `SGC.AntonioAnte.Application/Auditoria/Queries/GetAuditorias/GetAuditoriasQuery.cs`*

```csharp
using MediatR;
using SGC.AntonioAnte.Shared.DTOs.Auditoria;
using SGC.AntonioAnte.Shared.Responses;

namespace SGC.AntonioAnte.Application.Auditoria.Queries.GetAuditorias;

public record GetAuditoriasQuery(
    DateTime? FechaInicio, 
    DateTime? FechaFin, 
    string? Entidad
) : IRequest<Result<List<AuditLogResponseDto>>>;
```

#### 3. Query Handler con Proyección Optimizada (Capa `Application`)
*Ruta: `SGC.AntonioAnte.Application/Auditoria/Queries/GetAuditorias/GetAuditoriasQueryHandler.cs`*

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Auditoria;
using SGC.AntonioAnte.Shared.Responses;

namespace SGC.AntonioAnte.Application.Auditoria.Queries.GetAuditorias;

public class GetAuditoriasQueryHandler : IRequestHandler<GetAuditoriasQuery, Result<List<AuditLogResponseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditoriasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AuditLogResponseDto>>> Handle(GetAuditoriasQuery request, CancellationToken cancellationToken)
    {
        var queryable = _context.Auditorias
            .AsNoTracking(); // APAGA el ChangeTracker: Cero uso de memoria para rastreo de entidades

        if (request.FechaInicio.HasValue)
            queryable = queryable.Where(a => a.FechaCreacion >= request.FechaInicio.Value);

        if (request.FechaFin.HasValue)
            queryable = queryable.Where(a => a.FechaCreacion <= request.FechaFin.Value);

        if (!string.IsNullOrWhiteSpace(request.Entidad))
            queryable = queryable.Where(a => a.Entidad.Contains(request.Entidad));

        // Proyección directa en SQL a DTO mediante Select()
        var listado = await queryable
            .OrderByDescending(a => a.FechaCreacion)
            .Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                Usuario = a.Usuario,
                Accion = a.Accion,
                Entidad = a.Entidad,
                ClavePrimaria = a.ClavePrimaria,
                ValoresAntiguos = a.ValoresAntiguos,
                ValoresNuevos = a.ValoresNuevos,
                FechaCreacion = a.FechaCreacion
            })
            .ToListAsync(cancellationToken);

        return Result<List<AuditLogResponseDto>>.Success(listado, $"Se recuperaron {listado.Count} registros de auditoría.");
    }
}
```

#### 4. Controller Web API (Capa `Server / Web API`)
*Ruta: `SGC.AntonioAnte.Server/Controllers/AuditoriaController.cs`*

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Auditoria.Queries.GetAuditorias;
using SGC.AntonioAnte.Shared.DTOs.Auditoria;
using SGC.AntonioAnte.Shared.Responses;

namespace SGC.AntonioAnte.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador, Auditor")]
public class AuditoriaController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditoriaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<List<AuditLogResponseDto>>>> GetAuditorias(
        [FromQuery] DateTime? inicio, 
        [FromQuery] DateTime? fin,
        [FromQuery] string? entidad)
    {
        var query = new GetAuditoriasQuery(inicio, fin, entidad);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## 6. Estructura Uniforme de Carpetas y Vertical Slicing

Para garantizar que el crecimiento del sistema sea sostenible en módulos complejos como **Catastro**, **Predios**, **Contribuyentes** y **Fichas Catastrales**, la capa de `Application` adopta una organización por **Vertical Feature Slices** dividida estrictamente en `Commands` y `Queries`.

```
SGC.AntonioAnte.Application/
├── Common/
│   ├── Behaviors/               <-- ValidationBehavior, LoggingBehavior, PerformanceBehavior
│   ├── Exceptions/              <-- ValidationException, NotFoundException
│   └── Interfaces/              <-- IApplicationDbContext, ICurrentUserService, IDateTime
│
└── Catastro/
    ├── Predios/
    │   ├── Commands/            <-- Operaciones de Escritura
    │   │   ├── CreatePredio/
    │   │   │   ├── CreatePredioCommand.cs
    │   │   │   ├── CreatePredioCommandValidator.cs
    │   │   │   └── CreatePredioCommandHandler.cs
    │   │   └── UpdatePredio/
    │   │       ├── UpdatePredioCommand.cs
    │   │       ├── UpdatePredioCommandValidator.cs
    │   │       └── UpdatePredioCommandHandler.cs
    │   │
    │   └── Queries/             <-- Operaciones de Lectura
    │       ├── GetPrediosList/
    │       │   ├── GetPrediosListQuery.cs
    │       │   └── GetPrediosListQueryHandler.cs
    │       └── GetPredioById/
    │           ├── GetPredioByIdQuery.cs
    │           └── GetPredioByIdQueryHandler.cs
    │
    └── FichasCatastrales/
        ├── Commands/
        │   ├── GenerarFichaCatastral/
        │   │   ├── GenerarFichaCatastralCommand.cs
        │   │   └── GenerarFichaCatastralCommandHandler.cs
        │   └── AnularFichaCatastral/
        │       ├── AnularFichaCatastralCommand.cs
        │       └── AnularFichaCatastralCommandHandler.cs
        └── Queries/
            ├── GetFichaByClaveCatastral/
            │   ├── GetFichaByClaveCatastralQuery.cs
            │   └── GetFichaByClaveCatastralQueryHandler.cs
            └── ExportarFichaPdf/
                ├── ExportarFichaPdfQuery.cs
                └── ExportarFichaPdfQueryHandler.cs
```

### 📏 Convenciones de Nomenclatura Obligatorias

1. **Comandos:** `{Acción}{Entidad}Command` (Ejemplo: `CreatePredioCommand`, `DisableContribuyenteCommand`).
2. **Handlers de Comando:** `{Acción}{Entidad}CommandHandler`.
3. **Validadores de Comando:** `{Acción}{Entidad}CommandValidator`.
4. **Consultas:** `Get{Entidades/Criterio}Query` (Ejemplo: `GetPrediosPaginadosQuery`, `GetFichaByClaveCatastralQuery`).
5. **Handlers de Consulta:** `Get{Entidades/Criterio}QueryHandler`.
6. **DTOs de Entrada (UI):** `{Acción}{Entidad}Dto` (Ejemplo: `CreatePredioDto`).
7. **DTOs de Salida (Lectura):** `{Entidad}ResponseDto` o `{Entidad}ListDto`.

---

## 7. Transversales (Cross-Cutting Concerns) y Patrones Avanzados

### 🛡️ 1. Interceptor de Auditoría Automática en EF Core
En `Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`, la persistencia captura automáticamente la auditoría de cada entidad que implemente `IAuditableEntity`:

```csharp
public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData eventData, 
    InterceptionResult<int> result, 
    CancellationToken cancellationToken = default)
{
    UpdateEntities(eventData.Context);
    return base.SavingChangesAsync(eventData, result, cancellationToken);
}

private void UpdateEntities(DbContext? context)
{
    if (context == null) return;

    var utcNow = DateTime.UtcNow;
    var user = _currentUserService.UserId ?? "SYSTEM";

    foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreadoPor = user;
            entry.Entity.FechaCreacion = utcNow;
        }
        if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
        {
            entry.Entity.UltimaModificacionPor = user;
            entry.Entity.FechaUltimaModificacion = utcNow;
        }
    }
}
```

### ⚡ 2. Validation Pipeline Behavior con FluentValidation
Todos los Comandos enviados a través de MediatR pasan automáticamente por una tubería de validación previa al Handler:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
                throw new CustomValidationException(failures);
        }
        return await next();
    }
}
```

---

## 8. Guía de Defensa Académica / Tribunal de Tesis

Si los miembros del tribunal evaluador preguntan sobre las decisiones arquitectónicas del sistema `SGC.AntonioAnte`, utiliza las siguientes justificaciones técnicas:

### ❓ P: "¿Por qué utilizar CQRS en un sistema catastral municipal?"
> **Respuesta:** En un sistema catastral, el volumen y comportamiento de las lecturas y escrituras son asimétricos. El 80% de las operaciones son consultas masivas, búsquedas de predios y reportes para la ciudadanía y administradores, mientras que el 20% son modificaciones críticas (avalúos, transferencias de dominio). CQRS nos permite optimizar las lecturas apando el *ChangeTracker* (`.AsNoTracking()`) y proyectando directamente SQL a DTOs sin cargar grafos de entidades complejos, mientras garantizamos la máxima rigurosidad y auditoría en los comandos de escritura.

### ❓ P: "¿Por qué usar `record` para los Comandos/Consultas y `class` para las Entidades de Dominio?"
> **Respuesta:** Se utilizó `record` para los Comandos y Consultas porque representan **mensajes inmutables** de intención dentro del patrón Mediator. Los `records` proporcionan semántica de igualdad por valor y garantizan que los datos enviados desde la Web API no puedan ser alterados a mitad de la ejecución del pipeline. Por otro lado, las Entidades de Dominio deben ser `class` porque Entity Framework Core requiere tipos mutables basados en identidad de referencia (`Id`) para llevar el rastreo de estados en su mapa de memoria (`ChangeTracker`) y persistir los cambios correctamente en SQL Server.

### ❓ P: "¿Por qué se utiliza el patrón Mediator en lugar de llamar servicios directamente desde los Controllers?"
> **Respuesta:** MediatR nos permite implementar el principio de **Inversión de Dependencias (DIP)** y el **Principio de Responsabilidad Única (SRP)**. Los API Controllers quedan reducidos a simples enrutadores HTTP (*Thin Controllers*). Además, el patrón Mediator nos permite conectar fácilmente *Pipeline Behaviors* transversales como validación automática con FluentValidation, logging de rendimiento y manejo global de excepciones sin duplicar código en cada endpoint.