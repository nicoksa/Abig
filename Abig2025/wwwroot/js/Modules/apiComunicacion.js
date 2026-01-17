// modules/apiComunicacion.js
import { toggleCategoriaVisibilidad } from './utils.js';
class APIComunicacion {
    constructor(estadoManager) {
        this.estado = estadoManager;
        this.cargaInicial = true;
    }

    // ========== CARGAR FILTROS DESDE URL ==========
    cargarFiltrosDesdeURL() {
        const params = new URLSearchParams(window.location.search);
        if (params.toString()) {
            console.log('📥 Cargando filtros desde URL (carga inicial)');
            //  cargar el estado sin disparar eventos
            this._cargarSinEventos(params);
            this.cargaInicial = false; // Marcar que ya pasó la carga inicial
        } else {
            // Si no hay parámetros, marcar carga inicial como completa
            this.cargaInicial = false;
        }
    }

    _cargarSinEventos(params) {
        const estado = this.estado.estado;

        // Operación
        if (params.has('Operacion')) {
            estado.operacion = [params.get('Operacion')];
            toggleCategoriaVisibilidad('operacion', true);
        }

        // Tipo
        if (params.has('Tipo')) {
            estado.tipo = [params.get('Tipo')];
            toggleCategoriaVisibilidad('tipo', true);
        }

        // Dormitorios
        if (params.has('Dormitorios')) {
            const num = parseInt(params.get('Dormitorios'));
            estado.dormitorios = [`${num}${num >= 5 ? '+' : ''} dormitorio${num > 1 ? 's' : ''}`];
            toggleCategoriaVisibilidad('dormitorios', true);
        }

        // Ambientes
        if (params.has('Ambientes')) {
            const num = parseInt(params.get('Ambientes'));
            estado.ambientes = [`${num}${num >= 4 ? '+' : ''} ambiente${num > 1 ? 's' : ''}`];
            toggleCategoriaVisibilidad('ambientes', true);
        }

        // Baños
        if (params.has('Banos')) {
            const num = parseInt(params.get('Banos'));
            estado.banos = [`${num}${num >= 5 ? '+' : ''} baño${num > 1 ? 's' : ''}`];
            toggleCategoriaVisibilidad('banos', true);
        }

        // Precio
        if (params.has('PrecioMin') || params.has('PrecioMax')) {
            estado.precio.min = params.has('PrecioMin') ? parseFloat(params.get('PrecioMin')) : null;
            estado.precio.max = params.has('PrecioMax') ? parseFloat(params.get('PrecioMax')) : null;
            estado.precio.moneda = params.get('Moneda') || 'ARS';
            toggleCategoriaVisibilidad('precio', true);
        }

        // Ubicación
        if (params.has('Provincia') || params.has('Ciudad') || params.has('Barrio')) {
            estado.ubicacion.provincia = params.has('Provincia') ? parseInt(params.get('Provincia')) : null;
            estado.ubicacion.ciudad = params.has('Ciudad') ? parseInt(params.get('Ciudad')) : null;
            estado.ubicacion.barrio = params.has('Barrio') ? parseInt(params.get('Barrio')) : null;
            toggleCategoriaVisibilidad('ubicacion', true);
        }

        // Superficie Total
        if (params.has('SuperficieTotalMin') || params.has('SuperficieTotalMax')) {
            estado.superficieTotal.min = params.has('SuperficieTotalMin') ? parseFloat(params.get('SuperficieTotalMin')) : null;
            estado.superficieTotal.max = params.has('SuperficieTotalMax') ? parseFloat(params.get('SuperficieTotalMax')) : null;
            toggleCategoriaVisibilidad('superficie-total', true);
        }

        // Superficie Cubierta
        if (params.has('SuperficieCubiertaMin') || params.has('SuperficieCubiertaMax')) {
            estado.superficieCubierta.min = params.has('SuperficieCubiertaMin') ? parseFloat(params.get('SuperficieCubiertaMin')) : null;
            estado.superficieCubierta.max = params.has('SuperficieCubiertaMax') ? parseFloat(params.get('SuperficieCubiertaMax')) : null;
            toggleCategoriaVisibilidad('superficie-cubierta', true);
        }

        // Antigüedad
        if (params.has('Antiguedad')) {
            estado.antiguedad = [params.get('Antiguedad')];
            toggleCategoriaVisibilidad('antiguedad', true);
        }

        // Características
        if (params.has('Caracteristicas')) {
            const caracteristicas = params.get('Caracteristicas');
            estado.caracteristicas = caracteristicas.split(',').map(c => c.trim()).filter(c => c);
            // Las características NO se ocultan (son multiples)
        }

        // ORDENAMIENTO (nuevo) - se maneja en el backend, no en el estado
        // No necesitamos guardar el ordenamiento en el estado del cliente

        // Disparar evento para sincronizar UI
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

        // Obtener parámetros de la URL actual
        const urlParams = new URLSearchParams(window.location.search);
        const ordenActual = urlParams.get('OrdenarPor');
        const paginaActual = urlParams.get('PaginaActual');

        // ========== FILTROS ==========
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

        // ========== ORDENAMIENTO ==========
        // Si hay ordenamiento en la URL, mantenerlo
        if (ordenActual) {
            params.append('OrdenarPor', ordenActual);
        }

        // ========== PAGINACIÓN ==========
        // Si estamos aplicando filtros nuevos, volver a la página 1
        // Pero si solo estamos cambiando de página (no cambiaron filtros), mantener la página actual
        const cambioDeFiltros = Object.keys(estado).some(key => {
            if (key === 'ubicacionTexto') return false; // Ignorar este campo
            const value = estado[key];

            if (Array.isArray(value)) {
                return value.length > 0;
            } else if (typeof value === 'object' && value !== null) {
                return Object.values(value).some(v => v !== null && v !== '');
            }
            return value !== null && value !== '';
        });

        if (cambioDeFiltros) {
            // Si cambian los filtros, ir a página 1
            params.append('PaginaActual', '1');
        } else if (paginaActual && paginaActual !== "1") {
            // Si no cambian los filtros, mantener la página actual (si no es 1)
            params.append('PaginaActual', paginaActual);
        }
        // Si no hay página o es página 1, no agregamos el parámetro (queda por defecto)

        return params.toString();
    }

    // ========== MANEJO DE RESPUESTA AJAX ==========
    _manejarRespuestaAJAX(html, url) {
        // Extraer solo la parte de resultados del HTML
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');

        // Buscar la sección de resultados Y la sección de ordenamiento Y la paginación
        const nuevosResultados = doc.querySelector('.resultados');
        const nuevoOrdenamiento = doc.querySelector('.ordenamiento-container');
        const nuevaPaginacion = doc.querySelector('.paginacion-container');

        if (nuevosResultados) {
            const contenedorPadre = document.querySelector('.busqueda-contenedor');
            const resultadosActuales = document.querySelector('.resultados');

            if (resultadosActuales && contenedorPadre) {
                // Si hay sección de ordenamiento en la respuesta, actualizarla también
                if (nuevoOrdenamiento) {
                    const ordenamientoActual = contenedorPadre.querySelector('.ordenamiento-container');
                    if (ordenamientoActual) {
                        ordenamientoActual.replaceWith(nuevoOrdenamiento.cloneNode(true));
                    } else {
                        // Insertar antes de los resultados
                        resultadosActuales.parentNode.insertBefore(nuevoOrdenamiento.cloneNode(true), resultadosActuales);
                    }
                }

                // Si hay paginación en la respuesta, actualizarla también
                if (nuevaPaginacion) {
                    const paginacionActual = contenedorPadre.querySelector('.paginacion-container');
                    if (paginacionActual) {
                        paginacionActual.replaceWith(nuevaPaginacion.cloneNode(true));
                    } else {
                        // Insertar después de los resultados
                        resultadosActuales.parentNode.insertBefore(nuevaPaginacion.cloneNode(true), resultadosActuales.nextSibling);
                    }
                }

                // Actualizar resultados
                resultadosActuales.innerHTML = nuevosResultados.innerHTML;

                // Actualizar URL sin recargar la página
                window.history.pushState({}, '', url);

                // Disparar evento para notificar que los resultados se actualizaron
                document.dispatchEvent(new CustomEvent('resultadosActualizados'));

                // Agregar listeners para la nueva paginación
                this._agregarListenersPaginacion();

                console.log('✅ Resultados actualizados vía AJAX');
            }
        } else {
            console.warn('⚠️ No se encontró la sección de resultados en la respuesta AJAX');
            this._fallbackRecarga(url);
        }
    }

    // ========== AGREGAR LISTENERS PARA PAGINACIÓN ==========
    _agregarListenersPaginacion() {
        // Agregar listeners para los botones de paginación que respetan AJAX
        document.querySelectorAll('.pagina-btn:not(.disabled):not(.active)').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const url = btn.getAttribute('href');

                if (url) {
                    // Usar AJAX para cambiar de página
                    window.history.pushState({}, '', url);
                    this.cargarFiltrosDesdeURL();
                    this.aplicarFiltros();
                }
            });
        });
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