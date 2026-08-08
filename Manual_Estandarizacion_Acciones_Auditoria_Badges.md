# 🛡️ Manual de Estandarización: Acciones de Auditoría y Badges de UI

**Proyecto:** `SGC.AntonioAnte` (Sistema de Gestión Catastral y Municipal - GAD Municipal de Antonio Ante)  
**Módulo:** Auditoría, Seguridad y Trazabilidad de Sistema  
**Tecnologías:** .NET 8 / C# 12, Blazor WebAssembly, Bootstrap 5.3 (Subtle Palette) / MudBlazor  
**Documento Versión:** 1.0.0  
**Fecha de Última Revisión:** Agosto 2026  

---

## 📋 Tabla de Contenidos
1. [Objetivo y Regla de Sintaxis VERBO_ENTIDAD](#1-objetivo-y-regla-de-sintaxis-verbo_entidad)
2. [Matriz Estándar de Acciones, Colores y Badges](#2-matriz-estándar-de-acciones-colores-y-badges)
3. [Constantes de Auditoría en C# (Shared)](#3-constantes-de-auditoría-en-c-shared)
4. [Método C# Estándar para Blazor (GetBadgeAccion)](#4-método-c-estándar-para-blazor-getbadgeaccion)
5. [Componente Reutilizable Blazor: `<AuditBadge />`](#5-componente-reutilizable-blazor-auditbadge-)
6. [Mapeo Automático desde Entity Framework Core Interceptor](#6-mapeo-automático-desde-entity-framework-core-interceptor)
7. [Guía de Buenas Prácticas y Verificación para Desarrolladores](#7-guía-de-buenas-prácticas-y-verificación-para-desarrolladores)

---

## 1. Objetivo y Regla de Sintaxis `VERBO_ENTIDAD`

Para garantizar la integridad, consistencia visual y la facilidad de filtrado en las bitácoras de auditoría del sistema **SGC.AntonioAnte**, queda estrictamente prohibido el uso de cadenas de texto arbitrarias, combinaciones de idiomas o formatos mixtos (por ejemplo: `UPDATE_USER`, `Editar_Usuario`, `ModificarUsuario`, `delete_predio`).

Toda acción registrada en la tabla de auditoría debe cumplir la **Regla de Sintaxis Nomenclatura Estándar**:

$$	ext{Sintaxis:} \quad \mathbf{VERBO\_ENTIDAD}$$

### 📏 Principios de la Nomenclatura:
* **Mayúsculas Habladas (UPPER_SNAKE_CASE):** Todas las letras deben estar en mayúsculas, separando palabras con guión bajo `_`.
* **Verbo en Infinitivo o Acción Clara:** Usar verbos estandarizados en español (`CREAR`, `ACTUALIZAR`, `DESACTIVAR`, `ASIGNAR`, `LOGIN`).
* **Sustantivo en Singular (Entidad):** Indicar la entidad o recurso afectado (`USUARIO`, `DEPARTAMENTO`, `PREDIO`, `FICHA_CATASTRAL`).
* **Especificidad opcional:** Si la acción requiere mayor detalle, se agrega el calificador al final (`LOGIN_EXITOSO`, `LOGIN_FALLIDO`, `DESASIGNAR_ROL_USUARIO`).

---

## 2. Matriz Estándar de Acciones, Colores y Badges

La siguiente tabla define la correspondencia oficial entre la categoría de la operación, los verbos admitidos, los colores de estado y el estilo CSS (basado en la paleta *Subtle* de Bootstrap 5.3):

| Categoría | Verbo Estándar | Ejemplos de Acciones | Color Badge UI | Estilo Bootstrap 5 CSS |
| :--- | :--- | :--- | :--- | :--- |
| **Creación / Registro** | `CREAR` / `REGISTRAR` | `CREAR_USUARIO`<br>`CREAR_DEPARTAMENTO`<br>`REGISTRAR_PREDIO` | 🟢 Verde (`Success`) | `bg-success-subtle text-success border-success-subtle` |
| **Edición / Modificación** | `ACTUALIZAR` | `ACTUALIZAR_USUARIO`<br>`ACTUALIZAR_DEPARTAMENTO`<br>`ACTUALIZAR_FICHA` | 🔵 Azul (`Primary`) | `bg-primary-subtle text-primary border-primary-subtle` |
| **Habilitación / Estado** | `ACTIVAR` | `ACTIVAR_USUARIO`<br>`ACTIVAR_DEPARTAMENTO`<br>`ACTIVAR_PREDIO` | 🩵 Cian (`Info`) | `bg-info-subtle text-info-emphasis border-info-subtle` |
| **Eliminación / Baja** | `ELIMINAR`<br>`DESACTIVAR`<br>`DESASIGNAR` | `ELIMINAR_ROL`<br>`DESACTIVAR_USUARIO`<br>`DESASIGNAR_ROL_USUARIO` | 🔴 Rojo (`Danger`) | `bg-danger-subtle text-danger border-danger-subtle` |
| **Asignación / Permisos** | `ASIGNAR`<br>`CONCEDER` | `ASIGNAR_ROL_USUARIO`<br>`ASIGNAR_PERMISOS_ROL`<br>`CONCEDER_ACCESO` | 🟣 Amarillo/Púrpura (`Warning`) | `bg-warning-subtle text-warning-emphasis border-warning-subtle` |
| **Seguridad / Acceso** | `LOGIN`<br>`LOGOUT`<br>`PASSWORD` | `LOGIN_EXITOSO`<br>`LOGOUT`<br>`RESTABLECER_PASSWORD` | 🩶 Oscuro (`Dark`) | `bg-dark-subtle text-dark-emphasis border-dark-subtle` |
| **Procesos Específicos** | *Otros verbos* | `GENERAR_AVALUO`<br>`RECALCULAR_IMPUESTO` | ⚪ Gris (`Secondary`) | `bg-secondary-subtle text-secondary border-secondary-subtle` |

---

## 3. Constantes de Auditoría en C# (`Shared`)

Para evitar la duplicación de *magic strings* en los Handlers y Controllers, la capa `SGC.AntonioAnte.Shared` provee la clase estática `AuditActions`:

```csharp
namespace SGC.AntonioAnte.Shared.Constants;

public static class AuditActions
{
    // Creación / Registro
    public const string CrearUsuario = "CREAR_USUARIO";
    public const string CrearDepartamento = "CREAR_DEPARTAMENTO";
    public const string RegistrarPredio = "REGISTRAR_PREDIO";

    // Edición
    public const string ActualizarUsuario = "ACTUALIZAR_USUARIO";
    public const string ActualizarDepartamento = "ACTUALIZAR_DEPARTAMENTO";
    public const string ActualizarFichaCatastral = "ACTUALIZAR_FICHA_CATASTRAL";

    // Estado
    public const string ActivarUsuario = "ACTIVAR_USUARIO";
    public const string ActivarDepartamento = "ACTIVAR_DEPARTAMENTO";

    // Bajas / Eliminación
    public const string DesactivarUsuario = "DESACTIVAR_USUARIO";
    public const string EliminarRol = "ELIMINAR_ROL";
    public const string DesasignarRolUsuario = "DESASIGNAR_ROL_USUARIO";

    // Permisos y Roles
    public const string AsignarRolUsuario = "ASIGNAR_ROL_USUARIO";
    public const string AsignarPermisosRol = "ASIGNAR_PERMISOS_ROL";

    // Seguridad y Autenticación
    public const string LoginExitoso = "LOGIN_EXITOSO";
    public const string LoginFallido = "LOGIN_FALLIDO";
    public const string Logout = "LOGOUT";
    public const string RestablecerPassword = "RESTABLECER_PASSWORD";
}
```

---

## 4. Método C# Estándar para Blazor (`GetBadgeAccion`)

Este método evalúa mediante *Pattern Matching* y *Switch Expressions* de C# 12 la acción recibida, devolviendo la combinación exacta de clases CSS de Bootstrap 5 para renderizar el badge redondeado (*pill*) con bordes y contraste accesible.

### 💻 Código Fuente Oficial

```csharp
namespace SGC.AntonioAnte.Client.Helpers;

public static class AuditUiMapper
{
    /// <summary>
    /// Devuelve las clases CSS de Bootstrap 5 necesarias para renderizar el badge de la acción de auditoría.
    /// </summary>
    /// <param name="accion">Cadena con la acción en formato VERBO_ENTIDAD</param>
    /// <returns>Clases CSS para el elemento HTML span/badge</returns>
    public static string GetBadgeAccion(string? accion)
    {
        if (string.IsNullOrWhiteSpace(accion))
        {
            return "bg-secondary-subtle text-secondary border border-secondary-subtle rounded-pill px-2.5 py-1 fw-bold small";
        }

        return accion.ToUpperInvariant() switch
        {
            // 🟢 CREAR / REGISTRAR
            string a when a.StartsWith("CREAR") || a.StartsWith("REGISTRAR") 
                => "bg-success-subtle text-success border border-success-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // 🔵 ACTUALIZAR / EDITAR
            string a when a.StartsWith("ACTUALIZAR") || a.StartsWith("EDITAR") 
                => "bg-primary-subtle text-primary border border-primary-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // 🩵 ACTIVAR
            string a when a.StartsWith("ACTIVAR") 
                => "bg-info-subtle text-info-emphasis border border-info-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // 🔴 ELIMINAR / DESACTIVAR / DESASIGNAR
            string a when a.StartsWith("ELIMINAR") || a.StartsWith("DESACTIVAR") || a.StartsWith("DESASIGNAR") 
                => "bg-danger-subtle text-danger border border-danger-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // 🟣 ASIGNAR / PERMISOS
            string a when a.StartsWith("ASIGNAR") || a.Contains("PERMISO") || a.Contains("CLAIM") 
                => "bg-warning-subtle text-warning-emphasis border border-warning-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // 🩶 LOGIN / LOGOUT / SESIÓN
            string a when a.Contains("LOGIN") || a.Contains("LOGOUT") || a.Contains("PASSWORD") || a.Contains("TOKEN") 
                => "bg-dark-subtle text-dark-emphasis border border-dark-subtle rounded-pill px-2.5 py-1 fw-bold small",

            // ⚪ OTROS PROCESOS / DEFAULT
            _ => "bg-secondary-subtle text-secondary border border-secondary-subtle rounded-pill px-2.5 py-1 fw-bold small"
        };
    }
}
```

---

## 5. Componente Reutilizable Blazor: `<AuditBadge />`

Para evitar replicar código HTML en distintas vistas del cliente Blazor WASM (como la tabla de bitácora de auditoría, paneles de perfil o detalles de predios), se define un componente dedicado:

### 📄 Archivo: `Shared/Components/AuditBadge.razor`

```razor
@using SGC.AntonioAnte.Client.Helpers

<span class="@AuditUiMapper.GetBadgeAccion(Accion)">
    @if (MostrarIcono)
    {
        <i class="@GetIconoAccion(Accion) me-1"></i>
    }
    @Accion
</span>

@code {
    [Parameter, EditorRequired]
    public string Accion { get; set; } = string.Empty;

    [Parameter]
    public bool MostrarIcono { get; set; } = true;

    private string GetIconoAccion(string? accion) => (accion ?? string.Empty).ToUpperInvariant() switch
    {
        string a when a.StartsWith("CREAR") || a.StartsWith("REGISTRAR") => "bi bi-plus-circle-fill",
        string a when a.StartsWith("ACTUALIZAR") => "bi bi-pencil-square",
        string a when a.StartsWith("ACTIVAR") => "bi bi-check-circle-fill",
        string a when a.StartsWith("ELIMINAR") || a.StartsWith("DESACTIVAR") => "bi bi-x-circle-fill",
        string a when a.StartsWith("ASIGNAR") => "bi bi-shield-lock-fill",
        string a when a.Contains("LOGIN") || a.Contains("LOGOUT") => "bi bi-key-fill",
        _ => "bi bi-info-circle-fill"
    };
}
```

### 💡 Ejemplo de Uso en una Tabla Blazor:

```razor
<table class="table table-hover align-middle">
    <thead>
        <tr>
            <th>Fecha</th>
            <th>Usuario</th>
            <th>Acción Registrada</th>
            <th>Entidad</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var log in ListadoAuditoria)
        {
            <tr>
                <td>@log.FechaCreacion.ToString("dd/MM/yyyy HH:mm")</td>
                <td>@log.Usuario</td>
                <td>
                    <AuditBadge Accion="@log.Accion" />
                </td>
                <td>@log.Entidad</td>
            </tr>
        }
    </tbody>
</table>
```

---

## 6. Mapeo Automático desde Entity Framework Core Interceptor

Cuando el sistema realiza cambios persistentes a nivel de base de datos a través del `DbContext`, el Interceptor de Auditoría infiere automáticamente el prefijo del verbo según el `EntityState` de EF Core si no se proporcionó una acción explícita:

```csharp
private string DerivarAccionEstandar(EntityState state, string nombreEntidad)
{
    var entidadUpper = nombreEntidad.ToUpperInvariant();

    return state switch
    {
        EntityState.Added => $"CREAR_{entidadUpper}",
        EntityState.Modified => $"ACTUALIZAR_{entidadUpper}",
        EntityState.Deleted => $"ELIMINAR_{entidadUpper}",
        _ => $"MODIFICAR_{entidadUpper}"
    };
}
```

---

## 7. Guía de Buenas Prácticas y Verificación para Desarrolladores

### ✅ Lista de Chequeo (Checklist) antes de Subir Código:

1. **¿Usaste constantes estáticas?**  
   Asegúrate de invocar `AuditActions.CrearUsuario` o el helper en lugar de escribir cadenas manuales `"Crear_Usuario"`.
2. **¿La acción está en MAYÚSCULAS?**  
   Verifica que no existan minúsculas ni mezcla de idiomas (`UPDATE_PREDIO` en lugar de `ACTUALIZAR_PREDIO`).
3. **¿El badge renderiza el color correcto?**  
   Revisa en la interfaz Blazor que la acción aparezca con el color de la matriz (ej. Verde para `CREAR`, Azul para `ACTUALIZAR`, Rojo para `DESACTIVAR`).
4. **¿La auditoría incluye usuario y fecha UTC?**  
   Asegúrate de que la petición pase por el `ICurrentUserService` para registrar quién ejecutó la acción.