// results.js - Versión corregida con integración de filtros desde Index
import { FiltrosEstado } from './modules/filtrosEstado.js';
import { UIHandlers } from './modules/uiHandlers.js';
import { FiltrosAplicadosUI } from './modules/filtrosAplicados.js';
import { APIComunicacion } from './modules/apiComunicacion.js';
import { CheckboxSync } from './modules/checkboxSync.js';
import { initAcordeon } from './modules/utils.js';

class FiltrosApp {
    constructor() {
        // Inicializar módulos
        this.estado = new FiltrosEstado();
        this.checkboxSync = new CheckboxSync(this.estado);
        this.uiHandlers = new UIHandlers(this.estado);
        this.filtrosAplicadosUI = new FiltrosAplicadosUI(this.estado);
        this.apiComunicacion = new APIComunicacion(this.estado);

        this._timeoutAplicarFiltros = null;
        this._setupEventListeners();

        // Integrar filtros desde Index con el sistema
        this._integrarFiltrosDesdeIndex();
    }

    init() {
        console.log('🚀 Iniciando sistema de filtros...');

        initAcordeon();
        this.uiHandlers.init();
        this.filtrosAplicadosUI.init();

        // PRIMERO cargar filtros desde URL (sin aplicar)
        this.apiComunicacion.cargarFiltrosDesdeURL();

        // DESPUÉS sincronizar checkboxes con el estado cargado
        setTimeout(() => {
            this.checkboxSync.sincronizarTodos();
            this.filtrosAplicadosUI.actualizarUI();
            this.filtrosAplicadosUI.actualizarContadorFiltros();
            console.log('✅ Sistema de filtros listo');
        }, 100);

        this._exportarParaDebug();
    }

    _setupEventListeners() {
        // ESCUCHAR CAMBIOS DE ESTADO PARA SINCRONIZAR
        document.addEventListener('estadoCambiado', (e) => {
            const detalle = e.detail || {};

            // Si es carga inicial silenciosa, solo sincronizar UI
            if (detalle.silencioso) {
                console.log('🔄 Sincronización silenciosa de carga inicial');
                this.checkboxSync.sincronizarTodos();
                this.filtrosAplicadosUI.actualizarUI();
                this.filtrosAplicadosUI.actualizarContadorFiltros();
                return; // NO aplicar filtros
            }
            // Sincronizar checkboxes con el estado actual
            this.checkboxSync.sincronizarTodos();

            // Actualizar UI de filtros aplicados
            this.filtrosAplicadosUI.actualizarUI();
            this.filtrosAplicadosUI.actualizarContadorFiltros();

            // Aplicar filtros con debounce SOLO si no es carga inicial
            this._aplicarFiltrosConDebounce();
        });

        // ESCUCHAR LIMPIEZA COMPLETA
        document.addEventListener('estadoLimpio', () => {
            console.log('🧹 Estado limpiado');
            this.checkboxSync.limpiarTodos();
            this.filtrosAplicadosUI.actualizarUI();
            this.filtrosAplicadosUI.actualizarContadorFiltros(0);
            this.apiComunicacion.limpiarURL();
        });
    }

    //  Integrar filtros desde Index con el sistema
    _integrarFiltrosDesdeIndex() {
        // Esperar a que el DOM esté listo
        setTimeout(() => {
            const params = new URLSearchParams(window.location.search);

            const tieneParametrosIndex = params.has('operacion') ||
                params.has('tipo') ||
                params.has('dormitorios') ||
                params.has('provincia') ||
                params.has('ciudad') ||
                params.has('barrio');

            // Solo si hay parámetros del formulario de Index y NO del sistema de filtros
            if (tieneParametrosIndex && !params.has('Operacion')) {
                console.log('🔗 Integrando filtros desde formulario de Index...');
                this._convertirParametrosIndex(params);

                // Forzar actualización de la UI después de integrar
                setTimeout(() => {
                    this.checkboxSync.sincronizarTodos();
                    this.filtrosAplicadosUI.actualizarUI();
                    this.filtrosAplicadosUI.actualizarContadorFiltros();
                }, 200);
            }
        }, 300); // Tiempo extra para asegurar que el sistema está inicializado
    }

    _convertirParametrosIndex(params) {
        const estado = this.estado.estado;

        // Operación (solo si tiene valor)
        if (params.has('operacion')) {
            const operacion = params.get('operacion');
            if (operacion && operacion.trim() !== '') {
                let operacionFormateada = operacion;
                if (operacion === 'AlquilerTemporal') {
                    operacionFormateada = 'Alquiler Temporal';
                }
                estado.operacion = [operacionFormateada];
            }
        }

        // Tipo (solo si tiene valor)
        if (params.has('tipo')) {
            const tipo = params.get('tipo');
            if (tipo && tipo.trim() !== '') {
                estado.tipo = [tipo];
            }
        }

        // Dormitorios (solo si tiene valor)
        if (params.has('dormitorios')) {
            const dormitoriosValue = params.get('dormitorios');
            if (dormitoriosValue && dormitoriosValue.trim() !== '') {
                const num = parseInt(dormitoriosValue);
                if (!isNaN(num) && num > 0) {
                    estado.dormitorios = [`${num}${num >= 5 ? '+' : ''} dormitorio${num > 1 ? 's' : ''}`];
                }
            }
        }

        // Ambientes (solo si tiene valor)
        if (params.has('ambientes')) {
            const ambientesValue = params.get('ambientes');
            if (ambientesValue && ambientesValue.trim() !== '') {
                const num = parseInt(ambientesValue);
                if (!isNaN(num) && num > 0) {
                    estado.ambientes = [`${num}${num >= 4 ? '+' : ''} ambiente${num > 1 ? 's' : ''}`];
                    console.log(`📥 Ambientes desde Index: ${num}`);
                }
            }
        }

        // Baños (solo si tiene valor)
        if (params.has('banos')) {
            const banosValue = params.get('banos');
            if (banosValue && banosValue.trim() !== '') {
                const num = parseInt(banosValue);
                if (!isNaN(num) && num > 0) {
                    estado.banos = [`${num}${num >= 5 ? '+' : ''} baño${num > 1 ? 's' : ''}`];
                    console.log(`📥 Baños desde Index: ${num}`);
                }
            }
        }

        // Ubicación
        if (params.has('provincia')) {
            const provinciaValue = params.get('provincia');
            if (provinciaValue && provinciaValue.trim() !== '') {
                const provinciaId = parseInt(provinciaValue);
                if (!isNaN(provinciaId) && provinciaId > 0) {
                    estado.ubicacion.provincia = provinciaId;
                }
            }
        }

        if (params.has('ciudad')) {
            const ciudadValue = params.get('ciudad');
            if (ciudadValue && ciudadValue.trim() !== '') {
                const ciudadId = parseInt(ciudadValue);
                if (!isNaN(ciudadId) && ciudadId > 0) {
                    estado.ubicacion.ciudad = ciudadId;
                }
            }
        }

        if (params.has('barrio')) {
            const barrioValue = params.get('barrio');
            if (barrioValue && barrioValue.trim() !== '') {
                const barrioId = parseInt(barrioValue);
                if (!isNaN(barrioId) && barrioId > 0) {
                    estado.ubicacion.barrio = barrioId;
                }
            }
        }

        // Precio (solo si tiene al menos un valor válido)
        if (params.has('PrecioMin') || params.has('PrecioMax')) {
            let precioMin = null;
            let precioMax = null;

            if (params.has('PrecioMin')) {
                const precioMinValue = params.get('PrecioMin');
                if (precioMinValue && precioMinValue.trim() !== '') {
                    const min = parseFloat(precioMinValue);
                    if (!isNaN(min) && min >= 0) {
                        precioMin = min;
                    }
                }
            }

            if (params.has('PrecioMax')) {
                const precioMaxValue = params.get('PrecioMax');
                if (precioMaxValue && precioMaxValue.trim() !== '') {
                    const max = parseFloat(precioMaxValue);
                    if (!isNaN(max) && max >= 0) {
                        precioMax = max;
                    }
                }
            }

            // Solo actualizar si hay al menos un valor válido
            if (precioMin !== null || precioMax !== null) {
                estado.precio.min = precioMin;
                estado.precio.max = precioMax;
                estado.precio.moneda = params.get('Moneda') || 'ARS';
            }
        }

        // Superficie Total (solo si tiene valor válido)
        if (params.has('SuperficieTotalMin') || params.has('SuperficieTotalMax')) {
            let superficieMin = null;
            let superficieMax = null;

            if (params.has('SuperficieTotalMin')) {
                const superficieMinValue = params.get('SuperficieTotalMin');
                if (superficieMinValue && superficieMinValue.trim() !== '') {
                    const min = parseFloat(superficieMinValue);
                    if (!isNaN(min) && min >= 0) {
                        superficieMin = min;
                    }
                }
            }

            if (params.has('SuperficieTotalMax')) {
                const superficieMaxValue = params.get('SuperficieTotalMax');
                if (superficieMaxValue && superficieMaxValue.trim() !== '') {
                    const max = parseFloat(superficieMaxValue);
                    if (!isNaN(max) && max >= 0) {
                        superficieMax = max;
                    }
                }
            }

            if (superficieMin !== null || superficieMax !== null) {
                estado.superficieTotal.min = superficieMin;
                estado.superficieTotal.max = superficieMax;
            }
        }

        // Superficie Cubierta (solo si tiene valor válido)
        if (params.has('SuperficieCubiertaMin') || params.has('SuperficieCubiertaMax')) {
            let superficieMin = null;
            let superficieMax = null;

            if (params.has('SuperficieCubiertaMin')) {
                const superficieMinValue = params.get('SuperficieCubiertaMin');
                if (superficieMinValue && superficieMinValue.trim() !== '') {
                    const min = parseFloat(superficieMinValue);
                    if (!isNaN(min) && min >= 0) {
                        superficieMin = min;
                    }
                }
            }

            if (params.has('SuperficieCubiertaMax')) {
                const superficieMaxValue = params.get('SuperficieCubiertaMax');
                if (superficieMaxValue && superficieMaxValue.trim() !== '') {
                    const max = parseFloat(superficieMaxValue);
                    if (!isNaN(max) && max >= 0) {
                        superficieMax = max;
                    }
                }
            }

            if (superficieMin !== null || superficieMax !== null) {
                estado.superficieCubierta.min = superficieMin;
                estado.superficieCubierta.max = superficieMax;
            }
        }

        // Antigüedad (solo si tiene valor)
        if (params.has('Antiguedad')) {
            const antiguedad = params.get('Antiguedad');
            if (antiguedad && antiguedad.trim() !== '') {
                estado.antiguedad = [antiguedad];

            }
        }

        // Características (solo si tiene valor)
        if (params.has('Caracteristicas')) {
            const caracteristicas = params.get('Caracteristicas');
            if (caracteristicas && caracteristicas.trim() !== '') {
                estado.caracteristicas = caracteristicas.split(',').map(c => c.trim()).filter(c => c);
            }
        }

    }

    _aplicarFiltrosConDebounce() {
        if (this._timeoutAplicarFiltros) {
            clearTimeout(this._timeoutAplicarFiltros);
        }

        this._timeoutAplicarFiltros = setTimeout(() => {
            console.log('📡 Aplicando filtros...');
            this.apiComunicacion.aplicarFiltros();
            this._timeoutAplicarFiltros = null;
        }, 400);
    }

    aplicarFiltros() {
        this.apiComunicacion.aplicarFiltros();
    }

    limpiarTodosFiltros() {
        this.estado.limpiarTodos();
    }

    _exportarParaDebug() {
        window.filtrosApp = {
            estado: this.estado,
            sincronizar: () => this.checkboxSync.sincronizarTodos(),
            limpiarTodos: () => this.limpiarTodosFiltros(),
            getEstado: () => this.estado.getEstado(),
            // NUEVO: Método para debug de integración con Index
            integrarDesdeIndex: () => this._integrarFiltrosDesdeIndex(),
            convertirParametrosIndex: (params) => this._convertirParametrosIndex(new URLSearchParams(params))
        };
    }
}

// Inicialización
let app;

document.addEventListener('DOMContentLoaded', () => {
    try {
        app = new FiltrosApp();
        app.init();
        window.AppFiltros = app;

        // NUEVO: Debug helper para ver parámetros de Index
        const params = new URLSearchParams(window.location.search);
        if (params.toString()) {
            console.log('🔍 Parámetros de URL disponibles:', Object.fromEntries(params));

            // Verificar si hay parámetros del formulario de Index
            const parametrosIndex = {};
            ['operacion', 'tipo', 'dormitorios', 'provincia', 'ciudad', 'barrio', 'ambientes', 'banos'].forEach(param => {
                if (params.has(param)) {
                    parametrosIndex[param] = params.get(param);
                }
            });

            if (Object.keys(parametrosIndex).length > 0) {
                console.log('📋 Parámetros detectados desde formulario de Index:', parametrosIndex);
            }
        }
    } catch (error) {
        console.error('💥 Error crítico:', error);
        mostrarErrorFallback();
    }
});

function mostrarErrorFallback() {
    const resultadosDiv = document.querySelector('.resultados');
    if (resultadosDiv) {
        resultadosDiv.innerHTML = `
            <div class="alert alert-danger">
                <h4>Error en el sistema de filtros</h4>
                <p>Por favor, recarga la página.</p>
                <button onclick="location.reload()" class="btn btn-primary">Recargar</button>
            </div>
        `;
    }
}