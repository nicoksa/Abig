// modules/apiComunicacion.js
class APIComunicacion {
    constructor(estadoManager) {
        this.estado = estadoManager;
        this.cargaInicial = true; // NUEVA BANDERA
    }

    // ========== CARGAR FILTROS DESDE URL ==========
    cargarFiltrosDesdeURL() {
        const params = new URLSearchParams(window.location.search);
        if (params.toString()) {
            console.log('📥 Cargando filtros desde URL (carga inicial)');
            // SILENCIOSAMENTE cargar el estado sin disparar eventos
            this._cargarSinEventos(params);
            this.cargaInicial = false; // Marcar que ya pasó la carga inicial
        } else {
            // Si no hay parámetros, marcar carga inicial como completa
            this.cargaInicial = false;
        }
    }

    // Cargar parámetros sin disparar eventos de aplicación
    _cargarSinEventos(params) {
        const estado = this.estado.estado; // Acceso directo al estado interno

        // Operación
        if (params.has('Operacion')) {
            estado.operacion = [params.get('Operacion')];
        }

        // Tipo
        if (params.has('Tipo')) {
            estado.tipo = [params.get('Tipo')];
        }

        // Dormitorios
        if (params.has('Dormitorios')) {
            const num = parseInt(params.get('Dormitorios'));
            estado.dormitorios = [`${num}${num >= 5 ? '+' : ''} dormitorio${num > 1 ? 's' : ''}`];
        }

        // Ambientes
        if (params.has('Ambientes')) {
            const num = parseInt(params.get('Ambientes'));
            estado.ambientes = [`${num}${num >= 4 ? '+' : ''} ambiente${num > 1 ? 's' : ''}`];
        }

        // Baños
        if (params.has('Banos')) {
            const num = parseInt(params.get('Banos'));
            estado.banos = [`${num}${num >= 5 ? '+' : ''} baño${num > 1 ? 's' : ''}`];
        }

        // Precio
        if (params.has('PrecioMin') || params.has('PrecioMax')) {
            estado.precio.min = params.has('PrecioMin') ? parseFloat(params.get('PrecioMin')) : null;
            estado.precio.max = params.has('PrecioMax') ? parseFloat(params.get('PrecioMax')) : null;
            estado.precio.moneda = params.get('Moneda') || 'ARS';
        }

        // Ubicación
        if (params.has('Provincia') || params.has('Ciudad') || params.has('Barrio')) {
            estado.ubicacion.provincia = params.has('Provincia') ? parseInt(params.get('Provincia')) : null;
            estado.ubicacion.ciudad = params.has('Ciudad') ? parseInt(params.get('Ciudad')) : null;
            estado.ubicacion.barrio = params.has('Barrio') ? parseInt(params.get('Barrio')) : null;
        }

        // Superficie Total
        if (params.has('SuperficieTotalMin') || params.has('SuperficieTotalMax')) {
            estado.superficieTotal.min = params.has('SuperficieTotalMin') ? parseFloat(params.get('SuperficieTotalMin')) : null;
            estado.superficieTotal.max = params.has('SuperficieTotalMax') ? parseFloat(params.get('SuperficieTotalMax')) : null;
        }

        // Superficie Cubierta
        if (params.has('SuperficieCubiertaMin') || params.has('SuperficieCubiertaMax')) {
            estado.superficieCubierta.min = params.has('SuperficieCubiertaMin') ? parseFloat(params.get('SuperficieCubiertaMin')) : null;
            estado.superficieCubierta.max = params.has('SuperficieCubiertaMax') ? parseFloat(params.get('SuperficieCubiertaMax')) : null;
        }

        // Antigüedad
        if (params.has('Antiguedad')) {
            estado.antiguedad = [params.get('Antiguedad')];
        }

        // Características
        if (params.has('Caracteristicas')) {
            const caracteristicas = params.get('Caracteristicas');
            estado.caracteristicas = caracteristicas.split(',').map(c => c.trim()).filter(c => c);
        }

        console.log('✅ Estado cargado desde URL:', this.estado.getEstado());

        // DESPUÉS de cargar silenciosamente, disparar UN SOLO evento para sincronizar UI
        document.dispatchEvent(new CustomEvent('estadoCambiado', {
            detail: { tipo: 'cargaInicial', silencioso: true }
        }));
    }

    // ========== APLICAR FILTROS (AJAX) ==========
    aplicarFiltros() {
        // PREVENIR aplicación en carga inicial
        if (this.cargaInicial) {
            console.log('⏭️ Saltando aplicación de filtros en carga inicial');
            return;
        }

        const params = this._construirParams();
        const url = `${window.location.pathname}?${params}`;

        console.log('📡 Aplicando filtros vía AJAX:', url);

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
                console.error('❌ Error al aplicar filtros:', error);
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

                console.log('✅ Resultados actualizados vía AJAX');
            }
        } else {
            console.warn('⚠️ No se encontró la sección de resultados en la respuesta AJAX');
            this._fallbackRecarga(url);
        }
    }

    // ========== FALLBACK PARA RECARGA COMPLETA ==========
    _fallbackRecarga(url) {
        console.log('🔄 Recargando página completa...');
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
            console.log('⬅️ Navegación con botón atrás/adelante');
            this.cargaInicial = false; // Ya no es carga inicial
            this.cargarFiltrosDesdeURL();
            this.aplicarFiltros();
        });
    }
}

export { APIComunicacion };