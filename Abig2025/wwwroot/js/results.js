// results.js - Versión simplificada
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
    }

    init() {
        console.log('🚀 Iniciando sistema de filtros...');

        initAcordeon();
        this.uiHandlers.init();
        this.filtrosAplicadosUI.init();
        this.apiComunicacion.cargarFiltrosDesdeURL();

        // Sincronizar checkboxes inicialmente
        setTimeout(() => {
            this.checkboxSync.sincronizarTodos();
        }, 100);

        this._exportarParaDebug();
        console.log('✅ Sistema de filtros listo');
    }

    _setupEventListeners() {
        // ESCUCHAR CAMBIOS DE ESTADO PARA SINCRONIZAR
        document.addEventListener('estadoCambiado', (e) => {
            console.log('🔄 Estado cambiado, sincronizando...', e.detail);

            // Sincronizar checkboxes con el estado actual
            this.checkboxSync.sincronizarTodos();

            // Actualizar UI de filtros aplicados
            this.filtrosAplicadosUI.actualizarUI();
            this.filtrosAplicadosUI.actualizarContadorFiltros();

            // Aplicar filtros con debounce
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
            getEstado: () => this.estado.getEstado()
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