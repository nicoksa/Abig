// modules/utils.js
import { CATEGORIAS_NO_OCULTABLES } from './constants.js';

// ========== FUNCIONALIDAD ACORDEÓN ==========
export function initAcordeon() {
    const acordeones = document.querySelectorAll('.acordeon-titulo');

    acordeones.forEach(titulo => {
        titulo.addEventListener('click', function () {
            this.classList.toggle('active');
            const contenido = this.nextElementSibling;
            const isOpen = contenido.style.display === 'block';

            // Cerrar todos los demás acordeones
            if (!isOpen) {
                acordeones.forEach(otherTitulo => {
                    if (otherTitulo !== this) {
                        otherTitulo.classList.remove('active');
                        otherTitulo.nextElementSibling.style.display = 'none';
                    }
                });
            }

            // Toggle current
            contenido.style.display = isOpen ? 'none' : 'block';

            // Animar
            if (!isOpen) {
                contenido.style.animation = 'slideDown 0.3s ease';
            }
        });

        // Abrir el primer acordeón por defecto
        if (titulo.parentElement.dataset.tipo === 'tipo') {
            titulo.click();
        }
    });
}

// ========== HELPERS PARA MANIPULACIÓN DE UI ==========
export function toggleCategoriaVisibilidad(tipo, ocultar) {
    // No ocultar categorías de selección múltiple
    if (CATEGORIAS_NO_OCULTABLES.includes(tipo)) {
        return;
    }

    const grupoFiltro = document.querySelector(`.grupo-filtro[data-tipo="${tipo}"]`);
    if (grupoFiltro) {
        if (ocultar) {
            grupoFiltro.style.display = 'none';
        } else {
            grupoFiltro.style.display = 'block';
        }
    }
}

export function desmarcarCheckboxesPorTipo(tipo) {
    document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`).forEach(cb => {
        cb.checked = false;
    });
}

export function limpiarInputsPorId(ids) {
    ids.forEach(id => {
        const input = document.getElementById(id);
        if (input) input.value = '';
    });
}

export function obtenerArrayPorTipo(tipo, estado) {
    switch (tipo) {
        case 'tipo': return estado.tipo;
        case 'operacion': return estado.operacion;
        case 'ambientes': return estado.ambientes;
        case 'dormitorios': return estado.dormitorios;
        case 'banos': return estado.banos;
        case 'antiguedad': return estado.antiguedad;
        default: return null;
    }
}

export function extraerNumeroDeTexto(texto) {
    const match = texto.match(/\d+/);
    return match ? parseInt(match[0]) : null;
}

export function formatearPrecio(moneda, valor) {
    if (!valor && valor !== 0) return '∞';
    const simbolo = moneda === 'USD' ? 'USD$' : '$';
    return `${simbolo}${valor.toLocaleString()}`;
}

export function formatearSuperficie(valor, esCampo = false) {
    if (!valor && valor !== 0) return '∞';
    const unidad = esCampo ? 'ha' : 'm²';
    return `${valor.toLocaleString()} ${unidad}`;
}

// ========== VALIDACIONES ==========
export function validarRango(min, max) {
    if (min !== null && max !== null && min > max) {
        return { min: max, max: min }; // Intercambiar si min > max
    }
    return { min, max };
}

export function esValorNumerico(valor) {
    return !isNaN(parseFloat(valor)) && isFinite(valor);
}

// ========== MANEJO DE EVENTOS ==========
export function crearEventoPersonalizado(nombre, detalle) {
    return new CustomEvent(nombre, { detail: detalle });
}

export function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}