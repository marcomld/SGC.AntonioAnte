# 🗺️ Plan Maestro de Desarrollo: Módulo 3 - Visor Geoespacial (ArcGIS)
## Sistema de Gestión Catastral (SGC) - GAD Municipal de Antonio Ante

---

> [!IMPORTANT]
> **Estrategia de Simulación y Renderizado Híbrido**  
> Ante la falta temporal de acceso al servidor ArcGIS oficial del GAD, este módulo implementará un renderizado híbrido de alta fidelidad. Los polígonos espaciales se extraerán de la base de datos local (PostGIS/SQL Server) en formato WKT y se dibujarán dinámicamente sobre el mapa base usando **ArcGIS Maps SDK for JavaScript** a través de **JSInterop** en Blazor. 

---

## 📑 Tabla de Contenidos
1. [Arquitectura de Integración (Blazor + ArcGIS JS)](#1-arquitectura-de-integración-blazor--arcgis-js)
2. [Estrategia de Simulación Híbrida](#2-estrategia-de-simulación-híbrida)
3. [Desglose Detallado de Pasos (Sprints)](#3-desglose-detallado-de-pasos-sprints)

---

## 🏛️ 1. Arquitectura de Integración (Blazor + ArcGIS JS)

El ecosistema de Esri (ArcGIS) está basado nativamente en JavaScript. Dado que nuestro frontend está construido en C# (Blazor WebAssembly), la arquitectura exige un puente estricto:
- **`arcgis-visor.js`:** Archivo JavaScript aislado en `wwwroot/js/` que encapsula toda la lógica de la API de ArcGIS (creación del `MapView`, `GraphicsLayer`, y eventos `hitTest`).
- **`IJSRuntime` (C# -> JS):** Blazor invocará funciones de inicialización y pintado en JS, enviando los datos espaciales (WKT) recuperados del backend.
- **`[JSInvokable]` (JS -> C#):** Cuando el usuario haga clic en un predio del mapa, JS capturará la clave catastral e invocará un método en C# para abrir el Super-Modal de la Ficha Catastral.

---

## 🔄 2. Estrategia de Simulación Híbrida

Para garantizar que el sistema esté **listo para producción (Plug & Play)** el día que el municipio proporcione la URL de su *Feature Layer*:
1. El backend (`PrediosController`) entregará un listado de predios que incluya la `ClaveCatastral` y el `PoligonoWkt`.
2. El cliente (Blazor) inyectará estos datos en el script de ArcGIS, el cual creará *Graphics* (polígonos) sobre el mapa.
3. El código JavaScript quedará estructurado de manera que la transición de una capa local (`GraphicsLayer`) a una capa remota del servidor del GAD (`FeatureLayer`) requiera modificar únicamente la función de inicialización, sin alterar la lógica de comunicación con Blazor.

---

## 🚀 3. Desglose Detallado de Pasos (Sprints)

### Paso 1: Semilla Geoespacial (Data Seeding)
- **Objetivo:** Generar datos reales para el prototipo.
- **Acción:** Crear un *Seed* en Entity Framework Core que inyecte 5 a 10 polígonos contiguos reales del centro de Atuntaqui (ej. Parque Central) en la tabla `Catastro.Predios`.
- **Datos:** Claves catastrales iniciando con `1002...` (código DPA de Antonio Ante), propietarios de prueba y bloques constructivos del Módulo 2.

### Paso 2: El Motor JSInterop (`arcgis-visor.js`)
- **Objetivo:** Construir el motor de renderizado espacial.
- **Acción:** 
  - Incluir el SDK de ArcGIS vía CDN en `index.html`.
  - Crear el script que inicialice el mapa centrado en Atuntaqui.
  - Implementar la función que reciba el WKT desde C#, lo convierta a geometría de Esri y lo dibuje en un `GraphicsLayer`.
  - Configurar los estilos visuales (iluminación del polígono al pasar el cursor o hacer clic).

### Paso 3: Visor Catastral y Conexión con Ficha (Blazor)
- **Objetivo:** Interfaz de usuario e interacción bidireccional.
- **Acción:** 
  - Crear el componente `VisorCatastral.razor`.
  - Implementar el método `[JSInvokable]` que reciba el `id` o `ClaveCatastral` desde el mapa.
  - Al recibir el evento, ejecutar `ObtenerPorIdAsync(id)` y desplegar el Super-Modal de la Ficha Catastral con los datos en tiempo real.

### Paso 4: Dibujo y Auto-Cálculo de Área (Plus/Mejora)
- **Objetivo:** Facilitar el ingreso de nuevos predios.
- **Acción:** Habilitar el módulo de dibujo (Sketch) de ArcGIS en el mapa para que, al crear un predio nuevo, el técnico trace el polígono en pantalla. El sistema calculará el `AreaTerrenoGrafica` automáticamente y extraerá el `PoligonoWkt` para enviarlo a la API.