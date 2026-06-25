# Plan Maestro: Sistema de Gestión Catastral y Avalúos
**GADM Antonio Ante - Proyecto de Integración Curricular**
**Autor:** Marco Antonio Maldonado Amaguaña

## 1. Visión General
Migración y modernización del sistema catastral legacy (VB6) a una plataforma web moderna, interoperable y multifinalitaria, cumpliendo estrictamente con la **Norma Técnica Nacional de Catastros (MIDUVI)**.

## 2. Pila Tecnológica (Tech Stack)
- **Backend:** ASP.NET Core 8 Web API.
- **Arquitectura:** Clean Architecture + CQRS (MediatR).
- **ORM & Base de Datos:** Entity Framework Core + Microsoft SQL Server (Esquemas separados y NetTopologySuite para datos espaciales).
- **Seguridad:** ASP.NET Core Identity + JWT (JSON Web Tokens) estructurado bajo las 4 A's (Autenticación, Autorización, Acceso y Auditoría).
- **Frontend:** Blazor WebAssembly (C#) con manejo de Local Storage nativo (vía JSInterop).
- **Geospatial (GIS):** ArcGIS Server + ArcGIS Maps SDK for JavaScript.

## 3. Patrón Arquitectónico: CQRS + MediatR
El sistema separa estrictamente las lecturas de las escrituras:
- **Commands (Escrituras):** Todo lo que altere el estado usa MediatR blindados por un `ValidationBehavior` (FluentValidation) y un `AuditBehavior` (registro automático del usuario y fecha).
- **Queries (Lecturas):** Consultas de datos optimizadas.

## 4. Módulos y MVP (Producto Mínimo Viable)

### Módulo 1: Seguridad y Control de Acceso
- **MVP:** Autenticación con JWT, gestión extendida de usuarios (`ApplicationUser` con `Guid` como PK) y control de roles. Sistema base para auditoría.

### Módulo 2: Ficha Catastral Multifinalitaria (Core)
- **MVP:** CRUD de Predio usando la clave catastral DPA (19+ dígitos). Gestión de Propietarios, Terreno y Bloques de Construcción (Anexos MIDUVI).

### Módulo 3: Visor Geoespacial (ArcGIS)
- **MVP:** Renderizado del mapa base y Feature Layers desde ArcGIS Server en Blazor. Interacción bidireccional.

### Módulo 4: Motor de Valoración Masiva
- **MVP:** Ingreso de parámetros base. Cálculo automatizado de avalúos aplicando fórmulas de depreciación y factores de afectación.