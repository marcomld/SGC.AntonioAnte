# Estándares de Desarrollo y Repositorio

## 1. Convención de Idioma ("Spanglish" Técnico)
- **Español (Sin tildes/eñes):** Entidades de Dominio, Propiedades de BD, Documentación. Ej: `Predio`, `BloqueConstruccion`.
- **Inglés:** Framework, interfaces, patrones (CQRS), sufijos. Ej: `ApplicationUser`, `IUserRepository`, `CrearPredioCommandHandler`.

## 2. Convención de Base de Datos
- **Identity Customizado:** Se hereda de las clases base de Identity usando `Guid` como Clave Primaria.
- **Esquemas:** Las tablas se separan lógicamente (ej. esquema `Seguridad` para usuarios y roles, esquema `Catastro` para predios).

## 3. Formato de Commits (Conventional Commits)
Estructura: `<tipo>(<alcance>): <descripción>`
- `feat`: Nueva característica.
- `fix`: Corrección de bug.
- `docs`: Cambios en documentación.
- `refactor`: Reestructuración de código.
- `db`: Cambios de Entity Framework (Migraciones).
Ejemplo: `feat(seguridad): implementar capa application para login con mediatR`