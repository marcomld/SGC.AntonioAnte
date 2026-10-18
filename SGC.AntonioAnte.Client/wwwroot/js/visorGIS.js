window.visorGIS = {
    view: null,
    graphicsLayer: null,
    dotNetRef: null,

    setDotNetRef: function (ref) {
        this.dotNetRef = ref;
    },

    abrirFicha: function (idPredio) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('AbrirFichaDesdeMapa', idPredio);
        }
    },

    initMap: async function (containerElement) {
        try {
            if (this.view) {
                this.view.destroy();
                this.view = null;
            }

            const [MapModule, MapViewModule, GraphicsLayerModule] = await Promise.all([
                import("https://js.arcgis.com/4.28/@arcgis/core/Map.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/views/MapView.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/layers/GraphicsLayer.js")
            ]);

            const Map = MapModule.default;
            const MapView = MapViewModule.default;
            const GraphicsLayer = GraphicsLayerModule.default;

            const map = new Map({
                basemap: "streets-navigation-vector"
            });

            this.graphicsLayer = new GraphicsLayer();
            map.add(this.graphicsLayer);

            this.view = new MapView({
                container: containerElement,
                map: map,
                center: [-78.2160, 0.3320],
                zoom: 17
            });

            await this.view.when();

        } catch (error) {
            console.error("Error crítico inicializando ArcGIS:", error);
        }
    },

    loadPredios: async function (predios) {
        try {
            if (!this.graphicsLayer) return;
            this.graphicsLayer.removeAll();

            const [GraphicModule, PolygonModule, SimpleFillSymbolModule] = await Promise.all([
                import("https://js.arcgis.com/4.28/@arcgis/core/Graphic.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/geometry/Polygon.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/symbols/SimpleFillSymbol.js")
            ]);

            const Graphic = GraphicModule.default;
            const Polygon = PolygonModule.default;
            const SimpleFillSymbol = SimpleFillSymbolModule.default;

            var wktParser = new Wkt.Wkt();

            predios.forEach(p => {
                if (p.poligonoWkt) {
                    wktParser.read(p.poligonoWkt);
                    var geojson = wktParser.toJson();

                    var polygon = new Polygon({
                        rings: geojson.coordinates,
                        spatialReference: { wkid: 4326 }
                    });

                    var isUrbano = p.tipoPredio === 1;
                    var fillSymbol = new SimpleFillSymbol({
                        color: isUrbano ? [59, 130, 246, 0.4] : [34, 197, 94, 0.4],
                        outline: {
                            color: isUrbano ? [37, 99, 235, 1] : [22, 163, 74, 1],
                            width: 2
                        }
                    });

                    // 🚀 SOLUCIÓN DEFINITIVA: Bypass del Sanitizador creando Nodos DOM nativos
                    var popupTemplate = {
                        title: "Ficha: {claveCatastral}",
                        content: function (feature) {
                            var attr = feature.graphic.attributes;

                            // 1. Contenedor Maestro
                            var divContenedor = document.createElement("div");

                            // 2. Información del Predio
                            var divInfo = document.createElement("div");
                            divInfo.style.fontSize = "13px";
                            divInfo.style.fontFamily = "monospace";
                            divInfo.style.marginBottom = "15px";
                            divInfo.innerHTML = `
                                <b>Área GIS:</b> ${attr.area} m²<br/>
                                <b>Propietario:</b> ${attr.propietario}<br/>
                                <b>Bloques Const.:</b> ${attr.bloques}<br/>
                                <b>Dirección:</b> ${attr.direccion}
                            `;
                            divContenedor.appendChild(divInfo);

                            // 3. Botón de Bootstrap Nativo (ArcGIS no lo borrará porque es un nodo físico)
                            var btn = document.createElement("button");
                            btn.className = "btn btn-sm btn-primary w-100 fw-bold shadow-sm";
                            btn.innerHTML = `<i class="bi bi-box-arrow-up-right me-2"></i>Ir a Ficha Catastral`;

                            // Evento Clic conectado directamente a JSInterop
                            btn.onclick = function () {
                                window.visorGIS.abrirFicha(attr.id);
                            };

                            divContenedor.appendChild(btn);

                            // Le devolvemos el nodo HTML completo a ArcGIS
                            return divContenedor;
                        }
                    };

                    var graphic = new Graphic({
                        geometry: polygon,
                        symbol: fillSymbol,
                        attributes: {
                            id: p.id,
                            claveCatastral: p.claveCatastral,
                            area: p.areaTerrenoGrafica,
                            propietario: p.propietarioPrincipal,
                            bloques: p.totalBloques,
                            direccion: p.direccion
                        },
                        popupTemplate: popupTemplate
                    });

                    this.graphicsLayer.add(graphic);
                }
            });
        } catch (error) {
            console.error("Error renderizando polígonos WKT:", error);
        }
    }
};