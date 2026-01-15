// modules/filtrosEstado.js
import { ESTADO_INICIAL } from './constants.js';
import { toggleCategoriaVisibilidad } from './utils.js';

class FiltrosEstado {
    constructor() {
        this.estado = JSON.parse(JSON.stringify(ESTADO_INICIAL));
        this.suscriptores = [];
    }

    suscribir(evento, callback) {
        this.suscriptores.push({ evento, callback });
    }

    notificar(evento, datos = null) {
        document.dispatchEvent(new CustomEvent(evento, { detail: datos }));
        this.suscriptores.forEach(sub => {
            if (sub.evento === evento) sub.callback(datos);
        });
    }

    getEstado() {
        return JSON.parse(JSON.stringify(this.estado));
    }

    // ========== SETTERS SIMPLIFICADOS ==========
    actualizarPrecio(min, max, moneda = 'ARS') {
        this.estado.precio = { min, max, moneda };
        this.notificar('estadoCambiado', { tipo: 'precio' });
    }

    actualizarUbicacion(provincia, ciudad, barrio, ubicacionTexto = '') {
        this.estado.ubicacion = { provincia, ciudad, barrio };

        // Si se proporciona texto, guardarlo
        if (ubicacionTexto) {
            this.estado.ubicacionTexto = ubicacionTexto;
        }

        this.notificar('estadoCambiado', { tipo: 'ubicacion' });
    }

    actualizarSuperficieTotal(min, max) {
        this.estado.superficieTotal = { min, max };
        this.notificar('estadoCambiado', { tipo: 'superficie-total' });
    }

    actualizarSuperficieCubierta(min, max) {
        this.estado.superficieCubierta = { min, max };
        this.notificar('estadoCambiado', { tipo: 'superficie-cubierta' });
    }

    // ========== TOGGLE SIMPLIFICADO ==========
    toggleFiltroLista(tipo, valor, multiples = false) {
        const arrayTipo = this._obtenerArrayPorTipo(tipo);
        if (!arrayTipo) return;

        const index = arrayTipo.indexOf(valor);

        if (index === -1) {
            // Agregar
            if (multiples) {
                arrayTipo.push(valor);
            } else {
                // Single select - reemplazar
                arrayTipo.length = 0;
                arrayTipo.push(valor);
                toggleCategoriaVisibilidad(tipo, true);
            }
        } else {
            // Remover
            arrayTipo.splice(index, 1);
            if (!multiples) {
                toggleCategoriaVisibilidad(tipo, false);
            }
        }

        this.notificar('estadoCambiado', { tipo });
    }

    toggleCaracteristica(valor) {
        const index = this.estado.caracteristicas.indexOf(valor);

        if (index === -1) {
            this.estado.caracteristicas.push(valor);
        } else {
            this.estado.caracteristicas.splice(index, 1);
        }

        this.notificar('estadoCambiado', { tipo: 'caracteristicas' });
    }

    // ========== REMOVER FILTRO SIMPLIFICADO ==========
    removerFiltro(tipo, valor = null) {
        switch (tipo) {
            case 'tipo':
            case 'operacion':
            case 'ambientes':
            case 'dormitorios':
            case 'banos':
            case 'antiguedad':
                const array = this._obtenerArrayPorTipo(tipo);
                if (array && valor) {
                    const index = array.indexOf(valor);
                    if (index > -1) array.splice(index, 1);
                } else if (array) {
                    array.length = 0;
                }
                toggleCategoriaVisibilidad(tipo, false);
                break;

            case 'caracteristicas':
                if (valor) {
                    const idx = this.estado.caracteristicas.indexOf(valor);
                    if (idx > -1) this.estado.caracteristicas.splice(idx, 1);
                } else {
                    this.estado.caracteristicas = [];
                }
                break;

            case 'precio':
                this.estado.precio = { min: null, max: null, moneda: 'ARS' };
                toggleCategoriaVisibilidad('precio', false);
                break;

            case 'ubicacion':
                this.estado.ubicacion = { provincia: null, ciudad: null, barrio: null };
                this.estado.ubicacionTexto = ''; // Limpiar texto también
                toggleCategoriaVisibilidad('ubicacion', false);
                break;

            case 'superficie-total':
                this.estado.superficieTotal = { min: null, max: null };
                toggleCategoriaVisibilidad('superficie-total', false);
                break;

            case 'superficie-cubierta':
                this.estado.superficieCubierta = { min: null, max: null };
                toggleCategoriaVisibilidad('superficie-cubierta', false);
                break;
        }

        // Notificar cambio y sincronizar checkboxes
        this.notificar('estadoCambiado', { tipo, accion: 'removido', valor });
    }

    // ========== LIMPIAR TODO ==========
    limpiarTodos() {
        this.estado = JSON.parse(JSON.stringify(ESTADO_INICIAL));

        // Mostrar todas las categorías
        document.querySelectorAll('.grupo-filtro').forEach(grupo => {
            grupo.style.display = 'block';
        });

        this.notificar('estadoLimpio');
        this.notificar('estadoCambiado', { tipo: 'todos' });
    }

    // ========== HELPERS PRIVADOS ==========
    _obtenerArrayPorTipo(tipo) {
        switch (tipo) {
            case 'tipo': return this.estado.tipo;
            case 'operacion': return this.estado.operacion;
            case 'ambientes': return this.estado.ambientes;
            case 'dormitorios': return this.estado.dormitorios;
            case 'banos': return this.estado.banos;
            case 'antiguedad': return this.estado.antiguedad;
            default: return null;
        }
    }
}

export { FiltrosEstado };