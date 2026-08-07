# Arquitectura de la Solución y Flujo de Peticiones (SGC)
## GAD Municipal de Antonio Ante
### Documento Técnico de Arquitectura de Software, Estructura de Proyectos y Trazabilidad Transaccional (Módulo 1: Seguridad y Auditoría - Versión 2.0)

---

## 1. Resumen Ejecutivo y Principios de Diseño

El Sistema de Gestión Catastral (**SGC**) para el Gobierno Autónomo Descentralizado (**GAD**) Municipal de Antonio Ante ha sido diseñado bajo los estándares más exigentes de la ingeniería de software moderna. La solución adopta el patrón de **Clean Architecture** (Arquitectura Limpia / Cebolla), combinado con **CQRS** (*Command Query Responsibility Segregation*) mediante la librería **MediatR**, sobre la plataforma **.NET 8**.

### Principios Fundamentales
* **Inversión de Dependencias (DIP - SOLID):** Las capas de alto nivel (Dominio y Aplicación) no dependen de las capas de bajo nivel (Infraestructura, Base de Datos o Frameworks Web). Todas las dependencias apuntan hacia el interior.
* **Separación Estricta de Responsabilidades (SoC):** Cada proyecto dentro de la solución posee un único propósito acotado y definido, eliminando el acoplamiento no deseado y facilitando las pruebas unitarias y de integración.
* **Seguridad y Contratos Neutros:** El cliente Blazor WebAssembly se ejecuta de forma aislada en el navegador del usuario y se comunica exclusivamente mediante contratos neutros (DTOs) expuestos por la API REST, impidiendo la exposición directa de entidades de base de datos o lógica sensible.
* **Auditoría Inmutable e Implícita:** Cualquier mutación de datos en el sistema es interceptada de manera transparente a nivel de infraestructura para registrar la traza completa (Usuario, Acción, IP, Navegador y Payload JSON) sin contaminar la lógica de negocio.

---

## 2. Diagrama de la Solución y Mapa de Dependencias

El diagrama a continuación ilustra la disposición en capas de los 6 proyectos que componen la solución Visual Studio (`SGC.AntonioAnte.sln`), destacando la dirección de las dependencias entre proyectos:

```
+-----------------------------------------------------------------------------------+
|                                 CLIENTE (FRONTEND)                                |
|                                                                                   |
|    [ 🔴 SGC.AntonioAnte.Client ] (Blazor WASM .NET 8)                             |
|          │                                                                        |
|          └─── Depende de ───┐                                                     |
+──────────┼──────────────────┼─────────────────────────────────────────────────────+
           │                  │
   (Peticiones HTTP JSON)     │
           │                  │
+──────────▼──────────────────┼─────────────────────────────────────────────────────+
|          │                  │                   SERVIDOR (BACKEND)                |
|    [ 🔵 SGC.AntonioAnte.API ] │ (.NET 8 Web API REST)                                 |
|          │                  │                                                     |
|          ├─── Depende de ───┤                                                     |
|          │                  │                                                     |
|          ▼                  │                                                     |
|    [ 🟡 SGC.AntonioAnte.Application ] (CQRS, MediatR, Handlers, FluentValidation) |
|          │                  │                                                     |
|          ├─── Depende de ───┤                                                     |
|          │                  │                                                     |
|          ▼                  ▼                                                     |
|    [ 🟣 SGC.AntonioAnte.Domain ] <─── Depende de ─── [ 🟢 SGC.AntonioAnte.Shared ]  |
|          ▲                                                 (DTOs, Validaciones,   |
|          │                                                  Enums & Contratos)    |
|    [ 🖤 SGC.AntonioAnte.Infrastructure ] (EF Core 8, Azure SQL, Audit, Identity)  |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

---

## 3. Análisis Detallado por Proyecto / Capa

### 3.1. 🟢 `SGC.AntonioAnte.Shared` (Los Contratos Públicos)
* **Tipo de Proyecto:** Class Library (.NET 8)
* **Propósito:** Funciona como el puente de comunicación agnóstico y seguro entre la aplicación cliente (Blazor WASM) y la API del servidor.
* **¿Por qué existe?** Blazor se ejecuta dentro del Sandbox WebAssembly del navegador. Darle acceso directo a las entidades de base de datos o referencias de SQL Server sería un riesgo crítico de seguridad y un antipatrón de arquitectura. `Shared` actúa como una zona neutral.
* **Componentes Principales:**
  * **DTOs de Transferencia (Data Transfer Objects):** `CreateUsuarioDto`, `UpdateUsuarioDto`, `UsuarioResponseDto`, `AuditLogResponseDto`, `ResultadoPaginadoAuditDto`, `DepartamentoDto`, `RoleResponseDto`.
  * **Validaciones Integradas (Data Annotations):** Uso de atributos como `[Required]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]` para que EditForm en Blazor valide los formularios en el navegador *antes* de consumir ancho de banda o enviar la petición HTTP a la API.
  * **Enumeraciones y Constantes Globale:** `EstadoUsuario`, `TipoPermiso`, `ClaimsUsuario`, clases de constantes con rutas de endpoints.

### 3.2. 🔴 `SGC.AntonioAnte.Client` (El Frontend / Blazor WebAssembly)
* **Tipo de Proyecto:** Blazor WebAssembly Standalone (.NET 8)
* **Propósito:** Representa la interfaz de usuario reactiva, moderna y completamente responsiva de la aplicación SPA (*Single Page Application*).
* **Responsabilidades Clave:**
  * **Componentes Visuales y Páginas (`Pages/`):** Vistas especializadas como `Usuarios.razor`, `Roles.razor`, `Departamentos.razor` y `Auditoria.razor`, construidas sobre Bootstrap 5 y Bootstrap Icons.
  * **Servicios de Cliente (`Services/`):** Implementación de interfaces como `IUsuarioService`, `IRoleService`, `IAuditoriaService` mediante `HttpClient` para abstraer las llamadas REST.
  * **Gestión de Autenticación y Tokens:** Manejo de `AuthenticationStateProvider` personalizado, almacenamiento seguro de tokens JWT / Refresh Token en `LocalStorage`, e inyección de cabeceras de autorización HTTP mediante Delegating Handlers.
  * **Experiencia de Usuario (UI/UX):** Modales reutilizables estandarizados (`modal-lg`, `modal-xl`), filtros dinámicos, paginador interactivo y sistema de notificaciones flotantes (*Toasts*).

### 3.3. 🔵 `SGC.AntonioAnte.API` (La Puerta de Entrada REST)
* **Tipo de Proyecto:** ASP.NET Core Web API (.NET 8)
* **Propósito:** Exponer los puntos de acceso HTTPS (Endpoints RESTful) consumibles por el cliente WebAssembly o sistemas externos autorizados.
* **Responsabilidades Clave:**
  * **Controladores Delgados (*Slim Controllers*):** Clases como `UsuariosController`, `RolesController`, `AuditoriaController`. No contienen lógica de negocio ni sentencias SQL. Su único trabajo es recibir el DTO por HTTP, envolverlo en un Command/Query de MediatR y ejecutar `await _mediator.Send(command)`.
  * **Middleware Pipeline:** Captura global de excepciones desatendidas (`ExceptionHandlingMiddleware`), políticas CORS, Rate Limiting, compresión de respuestas y configuración Swagger/OpenAPI.
  * **Inyección de Dependencias:** Punto de composición global de la aplicación (`Program.cs`) donde se registran los servicios de la API, Application, Infrastructure y Shared.

### 3.4. 🟡 `SGC.AntonioAnte.Application` (El Cerebro / CQRS con MediatR)
* **Tipo de Proyecto:** Class Library (.NET 8)
* **Propósito:** Encapsular toda la lógica de negocio, reglas institucionales del GAD Municipal y la orquestación de casos de uso.
* **Estructura Interna CQRS:**
  * **Commands (Escritura):** Solicitudes que alteran el estado del sistema (`CreateUsuarioCommand`, `UpdateUsuarioCommand`, `DesactivarUsuarioCommand`).
  * **Queries (Lectura):** Solicitudes de consulta de datos que no alteran el estado (`GetUsuariosQuery`, `GetAuditoriaPaginadaQuery`).
  * **Handlers (Manejadores):** Clases que implementan `IRequestHandler<TRequest, TResponse>` (ej. `UpdateUsuarioCommandHandler`). Reciben el Command/Query, interactúan con la capa de infraestructura y ejecutan la transacción.
  * **Validadores Especializados (FluentValidation):** Reglas complejas que van más allá del DTO (por ejemplo, algoritmo de verificación del dígito modulador de la Cédula Ecuatoriana de 10 dígitos o verificación de duplicados en BD).
  * **Pipeline Behaviors:** Middleware en memoria para MediatR que ejecuta automáticamente validación, logging y medición de tiempos de ejecución antes de tocar el Handler.

### 3.5. 🟣 `SGC.AntonioAnte.Domain` (El Corazón del Sistema)
* **Tipo de Proyecto:** Class Library (.NET 8)
* **Propósito:** Definir el modelo de dominio y las entidades centrales de la base de datos.
* **Características Clave:**
  * **Cero Dependencias Externas:** No conoce frameworks Web, ni EF Core, ni bibliotecas de terceros. Es C# puro (*POCOs*).
  * **Entidades Principales:** `Usuario` (extiende `IdentityUser<Guid>`), `Rol` (extiende `IdentityRole<Guid>`), `Departamento`, `Auditoria`, `UsuarioToken`.
  * **Interfaces de Dominio:** Definición de contratos de auditoría (`IAuditableEntity`) y contratos de servicios de infraestructura que el dominio requiere sin saber cómo se implementan.

### 3.6. 🖤 `SGC.AntonioAnte.Infrastructure` (Persistencia y Servicios Externos)
* **Tipo de Proyecto:** Class Library (.NET 8)
* **Propósito:** Conectar el sistema con el mundo exterior: motor de base de datos SQL Server / Azure SQL, ASP.NET Core Identity, servicios de correo SMTP y cliente de contexto de red HTTP.
* **Componentes Fundamentales:**
  * **`ApplicationDbContext`:** Configuración de Entity Framework Core 8 mediante Fluent API, aislamiento del esquema `Seguridad`, configuración de claves primarias `Guid` y relaciones relacionales.
  * **Auditoría Automática e Inmutable:** Intercepción del método `SaveChangesAsync()`. Antes de confirmar los cambios en la base de datos, EF Core examina el `ChangeTracker` para identificar qué registros fueron creados, modificados o eliminados. Extrae automáticamente la IP del cliente (IPv4/IPv6) y el User-Agent del navegador mediante `ICurrentUserService` y genera la fila en la tabla `Seguridad.Auditorias` con el delta JSON.
  * **Implementación de Servicios:** Emisión y validación de tokens JWT, hashing de contraseñas con PBKDF2 y servicios de comunicación externa.

---

## 4. Estructura de Directorios y Organización del Código

```
SGC.AntonioAnte/
├── src/
│   ├── 🟢 SGC.AntonioAnte.Shared/
│   │   ├── Dtos/
│   │   │   ├── Usuarios/
│   │   │   │   ├── CreateUsuarioDto.cs
│   │   │   │   ├── UpdateUsuarioDto.cs
│   │   │   │   └── UsuarioResponseDto.cs
│   │   │   ├── Roles/
│   │   │   │   └── RoleResponseDto.cs
│   │   │   └── Auditoria/
│   │   │       ├── AuditLogResponseDto.cs
│   │   │       └── ResultadoPaginadoAuditDto.cs
│   │   ├── Enums/
│   │   │   └── EstadoUsuario.cs
│   │   └── Constants/
│   │       └── ApiRoutes.cs
│   │
│   ├── 🔴 SGC.AntonioAnte.Client/
│   │   ├── Pages/
│   │   │   └── Seguridad/
│   │   │       ├── Usuarios.razor
│   │   │       ├── Roles.razor
│   │   │       ├── Departamentos.razor
│   │   │       └── Auditoria.razor
│   │   ├── Services/
│   │   │   ├── UsuarioService.cs
│   │   │   └── AuditoriaService.cs
│   │   ├── Shared/
│   │   │   ├── MainLayout.razor
│   │   │   └── ConfirmModal.razor
│   │   └── Program.cs
│   │
│   ├── 🔵 SGC.AntonioAnte.API/
│   │   ├── Controllers/
│   │   │   └── v1/
│   │   │       ├── UsuariosController.cs
│   │   │       ├── RolesController.cs
│   │   │       └── AuditoriaController.cs
│   │   ├── Middlewares/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── 🟡 SGC.AntonioAnte.Application/
│   │   ├── Features/
│   │   │   ├── Usuarios/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── UpdateUsuarioCommand.cs
│   │   │   │   │   └── UpdateUsuarioCommandHandler.cs
│   │   │   │   └── Queries/
│   │   │   │       └── GetUsuariosQuery.cs
│   │   │   └── Auditoria/
│   │   │       └── Queries/
│   │   │           └── GetAuditLogsPagedQuery.cs
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   └── Validators/
│   │       └── CedulaEcuatorianaValidator.cs
│   │
│   ├── 🟣 SGC.AntonioAnte.Domain/
│   │   ├── Entities/
│   │   │   ├── Usuario.cs
│   │   │   ├── Rol.cs
│   │   │   ├── Departamento.cs
│   │   │   └── Auditoria.cs
│   │   └── Common/
│   │       └── IAuditableEntity.cs
│   │
│   └── 🖤 SGC.AntonioAnte.Infrastructure/
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   └── Configurations/
│       │       └── UsuarioConfiguration.cs
│       ├── Identity/
│       │   └── CurrentUserService.cs
│       └── Services/
│           └── DateTimeService.cs
└── SGC.AntonioAnte.sln
```

---

## 5. El Viaje Transaccional de una Petición (Ejemplo: Editar un Usuario)

Para comprender el flujo completo de información y la interacción sincrónica entre todas las capas, analicemos el recorrido técnico cuando un funcionario municipal edita los datos de un usuario en la interfaz gráfica:

### Diagrama de Secuencia

```
[ Usuario (Navegador) ]
        │
        │  1. Edita datos en formulario HTML / Blazor EditForm
        ▼
[ 🔴 SGC.AntonioAnte.Client ] (Usuarios.razor)
        │
        │  2. Modifica 'UpdateUsuarioDto' (Validación local DataAnnotations)
        │  3. Invocación HTTP PUT vía 'IUsuarioService'
        ▼
[ Network / HTTP REST ]  Headers: Authorization Bearer <JWT>
        │
        │  4. Petición PUT /api/v1/seguridad/usuarios/{id}
        ▼
[ 🔵 SGC.AntonioAnte.API ] (UsuariosController.cs)
        │
        │  5. Deserialización DTO, validación Token/Claims
        │  6. Instancia 'UpdateUsuarioCommand'
        │  7. Ejecuta: await _mediator.Send(command)
        ▼
[ 🟡 SGC.AntonioAnte.Application ] (MediatR Pipeline)
        │
        │  8. ValidationBehavior: Ejecuta FluentValidation (Cédula, Nombres)
        │  9. Entrega a 'UpdateUsuarioCommandHandler'
        │ 10. Consulta usuario en BD vía DbContext
        │ 11. Aplica cambios en entidad 'Usuario'
        ▼
[ 🖤 SGC.AntonioAnte.Infrastructure ] (ApplicationDbContext.cs)
        │
        │ 12. Invocación de SaveChangesAsync()
        │ 13. Interceptor ChangeTracker detecta estado 'Modified'
        │ 14. Extrae IP y User-Agent desde 'ICurrentUserService'
        │ 15. Genera fila inmutable en 'Seguridad.Auditorias' con Delta JSON
        │ 16. Transacción COMMIT en SQL Server / Azure SQL
        ▼
[ 🔵 API / HTTP REST ]
        │
        │ 17. Retorna HTTP 200 OK con 'UsuarioResponseDto'
        ▼
[ 🔴 Client (Blazor WASM) ]
        │
        │ 18. Actualiza lista en pantalla en tiempo real
        │ 19. Dispara Toast flotante de éxito ("Usuario actualizado correctamente")
```

---

### Descripción Paso a Paso del Flujo

1. **Interacción en la Vista Blazor (`Client`):**
   El funcionario edita el nombre, departamento o rol de un usuario en la ventana modal de la página `Usuarios.razor`. El formulario está enlazado (*data binding*) a un objeto `UpdateUsuarioDto`.

2. **Validación Preventiva en el Cliente (`Shared`):**
   Antes de realizar cualquier transmisión de red, el motor de Blazor procesa los atributos de DataAnnotations declarados en `UpdateUsuarioDto` dentro del proyecto `Shared`. Si falta un campo requerido, la petición se detiene de inmediato en el cliente.

3. **Invocación del Servicio HTTP (`Client`):**
   El componente llama a `IUsuarioService.ActualizarFuncionarioAsync(dto)`. El servicio serializa el DTO a JSON y emite una solicitud HTTP `PUT` hacia la API, adjuntando automáticamente el Token JWT en la cabecera `Authorization: Bearer <token>`.

4. **Recepción en la API REST (`API`):**
   El endpoint en `UsuariosController.cs` recibe la petición en la ruta `/api/v1/seguridad/usuarios/{id}`. El controlador mapea los datos de la URL y el cuerpo hacia un objeto `UpdateUsuarioCommand` y llama a MediatR:
   ```csharp
   var command = new UpdateUsuarioCommand(id, dto);
   var result = await _mediator.Send(command);
   return Ok(result);
   ```

5. **Procesamiento de Reglas de Negocio (`Application`):**
   MediatR hace pasar el comando por el pipeline de validación (`ValidationBehavior`). FluentValidation verifica que la cédula cumpla con el algoritmo de verificación ecuatoriano y que el correo electrónico pertenezca al dominio institucional. Posteriormente, el comando llega a `UpdateUsuarioCommandHandler.cs`, donde se carga la entidad `Usuario` desde la base de datos y se actualizan sus propiedades.

6. **Auditoría e Inserción Transaccional (`Infrastructure`):**
   Al llamar a `_context.SaveChangesAsync()`, la clase `ApplicationDbContext` intercepta la llamada. El `ChangeTracker` analiza los valores anteriores y los valores nuevos de la entidad `Usuario`. `CurrentUserService` lee de la cabecera de la petición la dirección IP pública/privada y el agente del navegador (*User-Agent*). Se crea automáticamente un objeto `Auditoria` con el payload JSON de la diferencia y se guarda en la base de datos dentro de una única transacción atómica.

7. **Retorno y Actualización Reactiva de la UI (`Client`):**
   La API responde con un estado `HTTP 200 OK` que contiene el `UsuarioResponseDto` actualizado. El cliente Blazor recibe la respuesta, refresca la tabla de datos localmente sin recargar la página completa y activa una notificación flotante (*Toast*) confirmando el éxito de la operación.

---

## 6. Matriz Resumen de Tecnologías y Componentes

| Proyecto | Capa | Tecnología Principal | Elemento Clave | Ejemplo de Clase |
| :--- | :--- | :--- | :--- | :--- |
| `SGC.AntonioAnte.Shared` | Contratos | .NET 8 / DataAnnotations | DTOs & Enums | `UpdateUsuarioDto.cs` |
| `SGC.AntonioAnte.Client` | Frontend | Blazor WASM / Bootstrap 5 | Razor Components | `Usuarios.razor` |
| `SGC.AntonioAnte.API` | Entrada REST | ASP.NET Core 8 Web API | Slim Controllers | `UsuariosController.cs` |
| `SGC.AntonioAnte.Application` | Cerebro | MediatR 12 / FluentValidation | CQRS Handlers | `UpdateUsuarioCommandHandler.cs` |
| `SGC.AntonioAnte.Domain` | Corazón | C# POCOs Pure | Entities & Interfaces | `Usuario.cs` |
| `SGC.AntonioAnte.Infrastructure` | Persistencia | EF Core 8 / Azure SQL | DbContext & Interceptors | `ApplicationDbContext.cs` |

---

## 7. Ventajas Competitivas de esta Arquitectura

1. **Mantenibilidad a Largo Plazo:** Modificar la interfaz de usuario no afecta la lógica de negocio ni la estructura de la base de datos.
2. **Escalabilidad y Rendimiento:** La separación entre comandos (escritura) y consultas (lectura) permite optimizar de forma independiente las consultas masivas a la base de datos (por ejemplo, la paginación en servidor de la bitácora de auditoría).
3. **Cumplimiento Normativo:** La captura de auditoría transparente y descentralizada garantiza el cumplimiento estricto con las normativas de la Contraloría General del Estado y la norma **ISO 27001**.
4. **Facilidad de Pruebas (Testability):** Al no tener dependencias acopladas a frameworks en la capa de `Application`, es posible escribir pruebas unitarias automatizadas con NUnit / xUnit para evaluar reglas de negocio en milisegundos sin necesidad de levantar un servidor web o conectar una base de datos real.