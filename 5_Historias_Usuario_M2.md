# Historias de Usuario - Módulo 2: Ficha Catastral

## Épica: Gestión de Predios Multifinalitarios
Como Técnico Catastral, necesito registrar y mantener actualizada la información física, legal y constructiva de los predios del cantón, cumpliendo con la normativa del MIDUVI.

### HU-CAT-01: Gestión de Catálogos Dinámicos
**Como** Administrador del Sistema  
**Quiero** realizar un CRUD de Tipos de Tenencia, Estructuras y Estados de Conservación  
**Para** que los técnicos tengan opciones actualizadas en sus formularios sin depender de actualizaciones de código.  
*Nota: Este es el paso previo obligatorio para que la Ficha Catastral funcione.*

### HU-CAT-02: Creación del Predio Base (Terreno)
**Como** Técnico Catastral  
**Quiero** ingresar un nuevo predio validando su Clave Catastral Nacional (DPA)  
**Para** registrar el área física y la ubicación del terreno en la base de datos.  
**Criterio de Aceptación:** La API debe rechazar claves con menos de 19 dígitos. 

### HU-CAT-03: Asignación de Propietarios y Dominios
**Como** Técnico Catastral  
**Quiero** buscar un ciudadano por cédula y asignarlo a un predio con un porcentaje de propiedad  
**Para** establecer la responsabilidad jurídica y tributaria del lote.  
**Criterio de Aceptación:** La suma total del `PorcentajePropiedad` de los dominios activos en un predio no puede superar el 100.00%.

### HU-CAT-04: Registro de Edificaciones (Bloques)
**Como** Técnico Catastral  
**Quiero** agregar múltiples bloques de construcción a un predio  
**Para** detallar los pisos, áreas y materiales necesarios para el futuro cálculo de avalúos.