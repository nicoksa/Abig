// modules/apiComunicacion.js
class APIComunicacion {
    constructor(estadoManager) {
        this.estado = estadoManager;
    }

    // ========== CARGAR FILTROS DESDE URL ==========
    cargarFiltrosDesdeURL() {
        const params = new URLSearchParams(window.location.search);
        if (params.toString()) {
            this.estado.cargarDesdeParams(params);
        }
    }

    // ========== APLICAR FILTROS (AJAX) ==========
    aplicarFiltros() {
        const params = this._construirParams();
        const url = `${window.location.pathname}?${params}`;

        //  AJAX para evitar recargar toda la página
        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.text();
            })
            .then(html => this._manejarRespuestaAJAX(html, url))
            .catch(error => {
                console.error('Error al aplicar filtros:', error);
                // Fallback: recargar la página normalmente
                this._fallbackRecarga(url);
            });
    }

    // ========== CONSTRUIR PARÁMETROS ==========
    _construirParams() {
        const params = new URLSearchParams();
        const estado = this.estado.getEstado();

        // Operación
        if (estado.operacion.length > 0) {
            params.append('Operacion', estado.operacion[0]);
        }

        // Tipo
        if (estado.tipo.length > 0) {
            params.append('Tipo', estado.tipo[0]);
        }

        // Dormitorios
        if (estado.dormitorios.length > 0) {
            const texto = estado.dormitorios[0];
            const numMatch = texto.match(/\d+/);
            if (numMatch) {
                params.append('Dormitorios', numMatch[0]);
            }
        }

        // Ambientes
        if (estado.ambientes.length > 0) {
            const texto = estado.ambientes[0];
            const numMatch = texto.match(/\d+/);
            if (numMatch) {
                params.append('Ambientes', numMatch[0]);
            }
        }

        // Baños
        if (estado.banos.length > 0) {
            const texto = estado.banos[0];
            const numMatch = texto.match(/\d+/);
            if (numMatch) {
                params.append('Banos', numMatch[0]);
            }
        }

        // Precio
        if (estado.precio.min !== null) {
            params.append('PrecioMin', estado.precio.min);
        }
        if (estado.precio.max !== null) {
            params.append('PrecioMax', estado.precio.max);
        }
        if (estado.precio.min !== null || estado.precio.max !== null) {
            params.append('Moneda', estado.precio.moneda);
        }

        // Ubicación
        if (estado.ubicacion.provincia) {
            params.append('Provincia', estado.ubicacion.provincia);
        }
        if (estado.ubicacion.ciudad) {
            params.append('Ciudad', estado.ubicacion.ciudad);
        }
        if (estado.ubicacion.barrio) {
            params.append('Barrio', estado.ubicacion.barrio);
        }

        // Superficie Total
        if (estado.superficieTotal.min !== null) {
            params.append('SuperficieTotalMin', estado.superficieTotal.min);
        }
        if (estado.superficieTotal.max !== null) {
            params.append('SuperficieTotalMax', estado.superficieTotal.max);
        }

        // Superficie Cubierta
        if (estado.superficieCubierta.min !== null) {
            params.append('SuperficieCubiertaMin', estado.superficieCubierta.min);
        }
        if (estado.superficieCubierta.max !== null) {
            params.append('SuperficieCubiertaMax', estado.superficieCubierta.max);
        }

        // Antigüedad
        if (estado.antiguedad.length > 0) {
            params.append('Antiguedad', estado.antiguedad[0]);
        }

        // Características
        if (estado.caracteristicas.length > 0) {
            params.append('Caracteristicas', estado.caracteristicas.join(','));
        }

        return params.toString();
    }

    // ========== MANEJO DE RESPUESTA AJAX ==========
    _manejarRespuestaAJAX(html, url) {
        // Extraer solo la parte de resultados del HTML
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const nuevosResultados = doc.querySelector('.resultados');

        if (nuevosResultados) {
            const resultadosActuales = document.querySelector('.resultados');
            if (resultadosActuales) {
                resultadosActuales.innerHTML = nuevosResultados.innerHTML;

                // Actualizar URL sin recargar la página
                window.history.pushState({}, '', url);

                // Disparar evento para notificar que los resultados se actualizaron
                document.dispatchEvent(new CustomEvent('resultadosActualizados'));
            }
        } else {
            console.warn('No se encontró la sección de resultados en la respuesta AJAX');
            this._fallbackRecarga(url);
        }
    }

    // ========== FALLBACK PARA RECARGA COMPLETA ==========
    _fallbackRecarga(url) {
        window.location.href = url;
    }

    // ========== LIMPIAR URL ==========
    limpiarURL() {
        window.location.href = window.location.pathname;
    }

    // ========== MANEJO DE HISTORY API ==========
    configurarHistoryAPI() {
        // Manejar el botón atrás/adelante del navegador
        window.addEventListener('popstate', () => {
            this.cargarFiltrosDesdeURL();
            this.aplicarFiltros();
        });
    }
}

export { APIComunicacion };