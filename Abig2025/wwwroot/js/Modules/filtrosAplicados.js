// modules/filtrosAplicados.js - Versión simplificada
import { formatearPrecio, formatearSuperficie } from './utils.js';

class FiltrosAplicadosUI {
    constructor(estadoManager) {
        this.estado = estadoManager;
        this.contenedor = document.getElementById('filtros-aplicados');
    }

    init() {
        this.actualizarUI();
    }

    // ========== ACTUALIZAR UI ==========
    actualizarUI() {
        if (!this.contenedor) return;

        const estado = this.estado.getEstado();
        this.contenedor.innerHTML = '';

        let totalFiltros = 0;

        // Tipo
        if (estado.tipo.length > 0) {
            estado.tipo.forEach(valor => {
                this._agregarEtiquetaFiltro('Tipo', valor, 'tipo', valor);
                totalFiltros++;
            });
        }

        // Operación
        if (estado.operacion.length > 0) {
            estado.operacion.forEach(valor => {
                this._agregarEtiquetaFiltro('Operación', valor, 'operacion', valor);
                totalFiltros++;
            });
        }

        // Dormitorios
        if (estado.dormitorios.length > 0) {
            estado.dormitorios.forEach(valor => {
                this._agregarEtiquetaFiltro('Dormitorios', valor, 'dormitorios', valor);
                totalFiltros++;
            });
        }

        // Ambientes
        if (estado.ambientes.length > 0) {
            estado.ambientes.forEach(valor => {
                this._agregarEtiquetaFiltro('Ambientes', valor, 'ambientes', valor);
                totalFiltros++;
            });
        }

        // Baños
        if (estado.banos.length > 0) {
            estado.banos.forEach(valor => {
                this._agregarEtiquetaFiltro('Baños', valor, 'banos', valor);
                totalFiltros++;
            });
        }

        // Precio
        if (estado.precio.min !== null || estado.precio.max !== null) {
            const texto = `${formatearPrecio(estado.precio.moneda, estado.precio.min || 0)} - ${formatearPrecio(estado.precio.moneda, estado.precio.max || '∞')}`;
            this._agregarEtiquetaFiltro('Precio', texto, 'precio');
            totalFiltros++;
        }

        // Ubicación
        if (estado.ubicacion.provincia || estado.ubicacion.ciudad || estado.ubicacion.barrio) {

            let ubicTexto = '';

            if (estado.ubicacionTexto && estado.ubicacionTexto.trim() !== '') {
                ubicTexto = estado.ubicacionTexto;
            } else {
                const ubicacionInput = document.getElementById('input-ubicacion');
                ubicTexto = ubicacionInput?.value || 'Ubicación seleccionada';
            }

            this._agregarEtiquetaFiltro('Ubicación', ubicTexto, 'ubicacion');
            totalFiltros++;
        }

        // Superficie Total
        if (estado.superficieTotal.min !== null || estado.superficieTotal.max !== null) {
            const texto = `${formatearSuperficie(estado.superficieTotal.min || 0)} - ${formatearSuperficie(estado.superficieTotal.max || '∞')}`;
            this._agregarEtiquetaFiltro('Sup. Total', texto, 'superficie-total');
            totalFiltros++;
        }

        // Superficie Cubierta
        if (estado.superficieCubierta.min !== null || estado.superficieCubierta.max !== null) {
            const texto = `${formatearSuperficie(estado.superficieCubierta.min || 0)} - ${formatearSuperficie(estado.superficieCubierta.max || '∞')}`;
            this._agregarEtiquetaFiltro('Sup. Cubierta', texto, 'superficie-cubierta');
            totalFiltros++;
        }

        // Antigüedad
        if (estado.antiguedad.length > 0) {
            estado.antiguedad.forEach(valor => {
                this._agregarEtiquetaFiltro('Antigüedad', valor, 'antiguedad', valor);
                totalFiltros++;
            });
        }

        // Características
        if (estado.caracteristicas.length > 0) {
            estado.caracteristicas.forEach(valor => {
                const displayValue = valor.replace('Campo_', '').replace(/([A-Z])/g, ' $1').trim();
                this._agregarEtiquetaFiltro('Característica', displayValue, 'caracteristicas', valor);
                totalFiltros++;
            });
        }

        // Botón limpiar todos
        if (totalFiltros > 0) {
            this._agregarBotonLimpiarTodos();
        } else {
            this._mostrarMensajeSinFiltros();
        }
    }

    // ========== AGREGAR ETIQUETA CON LISTENER SIMPLE ==========
    _agregarEtiquetaFiltro(label, valor, tipo, valorOriginal = null) {
        const etiqueta = document.createElement('div');
        etiqueta.className = 'etiqueta-filtro';
        etiqueta.innerHTML = `
            <span class="nombre"><strong>${label}:</strong> ${valor}</span>
            <span class="quitar" data-tipo="${tipo}" data-valor="${valorOriginal || valor}">×</span>
        `;

        // AGREGAR LISTENER DIRECTO Y SIMPLE
        const botonQuitar = etiqueta.querySelector('.quitar');
        botonQuitar.addEventListener('click', (e) => {
            e.stopPropagation();
            const tipo = botonQuitar.dataset.tipo;
            const valor = botonQuitar.dataset.valor;

            console.log(`🗑️ Quitando filtro directamente: ${tipo} = ${valor}`);

            // Remover del estado - esto disparará estadoCambiado que sincronizará checkboxes
            this.estado.removerFiltro(tipo, valor);
        });

        this.contenedor.appendChild(etiqueta);
    }

    _agregarBotonLimpiarTodos() {
        const botonLimpiar = document.createElement('button');
        botonLimpiar.className = 'btn btn-limpiar';
        botonLimpiar.textContent = 'Limpiar todos';
        botonLimpiar.addEventListener('click', () => {
            console.log('🧹 Limpiando todos los filtros');
            this.estado.limpiarTodos();
        });
        this.contenedor.appendChild(botonLimpiar);
    }

    _mostrarMensajeSinFiltros() {
        this.contenedor.innerHTML = '<p class="text-muted" style="margin: 0;">No hay filtros aplicados</p>';
    }

    // ========== CONTADOR SIMPLIFICADO ==========
    actualizarContadorFiltros(total = null) {
        if (total === null) {
            const estado = this.estado.getEstado();
            total = this._calcularTotalFiltros(estado);
        }

        const titulo = document.querySelector('.filtros-titulo');
        if (titulo) {
            let badge = titulo.querySelector('.badge');
            if (total > 0) {
                if (!badge) {
                    badge = document.createElement('span');
                    badge.className = 'badge bg-primary ms-2';
                    titulo.appendChild(badge);
                }
                badge.textContent = total;
            } else if (badge) {
                badge.remove();
            }
        }
    }

    _calcularTotalFiltros(estado) {
        let total = 0;
        total += estado.tipo.length;
        total += estado.operacion.length;
        total += estado.ambientes.length;
        total += estado.dormitorios.length;
        total += estado.banos.length;
        total += estado.antiguedad.length;
        total += estado.caracteristicas.length;

        if (estado.precio.min !== null || estado.precio.max !== null) total++;
        if (estado.ubicacion.provincia || estado.ubicacion.ciudad || estado.ubicacion.barrio) total++;
        if (estado.superficieTotal.min !== null || estado.superficieTotal.max !== null) total++;
        if (estado.superficieCubierta.min !== null || estado.superficieCubierta.max !== null) total++;

        return total;
    }
}

export { FiltrosAplicadosUI };