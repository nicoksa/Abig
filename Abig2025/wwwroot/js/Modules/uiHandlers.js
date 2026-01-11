// modules/uiHandlers.js - Versión simplificada
import {
    toggleCategoriaVisibilidad,
    desmarcarCheckboxesPorTipo,
    limpiarInputsPorId,
    validarRango
} from './utils.js';

import {
    PRECIO_RANGOS,
    SUPERFICIE_TOTAL_RANGOS,
    SUPERFICIE_CUBIERTA_RANGOS,
    UBICACIONES_MOCK
} from './constants.js';

class UIHandlers {
    constructor(estadoManager) {
        this.estado = estadoManager;
    }

    init() {
        this._setupCheckboxListeners();
        this._setupPrecioListeners();
        this._setupUbicacionListener();
        this._setupRangoListeners();
    }

    // ========== CHECKBOXES SIMPLIFICADOS ==========
    _setupCheckboxListeners() {
        document.querySelectorAll('.acordeon-contenido input[type="checkbox"]').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => this._manejarCheckbox(e.target));
        });
    }

    _manejarCheckbox(checkbox) {
        const grupo = checkbox.closest('.grupo-filtro');
        const tipo = grupo.dataset.tipo;
        const valor = checkbox.value;
        const multiples = grupo.dataset.multiples === 'true';

        // Manejo especial para superficie
        if (tipo === 'superficie-total') {
            this._manejarSuperficieCheckbox(tipo, valor, checkbox.checked, SUPERFICIE_TOTAL_RANGOS);
            return;
        }

        if (tipo === 'superficie-cubierta') {
            this._manejarSuperficieCheckbox(tipo, valor, checkbox.checked, SUPERFICIE_CUBIERTA_RANGOS);
            return;
        }

        // Manejo normal
        if (checkbox.checked) {
            if (multiples || tipo === 'features' || tipo === 'features-campo') {
                this.estado.toggleCaracteristica(valor);
            } else {
                this._manejarSingleSelect(tipo, valor, checkbox);
            }
        } else {
            // Si se desmarca un checkbox
            if (multiples || tipo === 'features' || tipo === 'features-campo') {
                this.estado.toggleCaracteristica(valor);
            } else {
                this.estado.removerFiltro(tipo, valor);
            }
        }
    }

    _manejarSuperficieCheckbox(tipo, valor, checked, rangos) {
        if (checked) {
            desmarcarCheckboxesPorTipo(tipo);

            const inputIds = tipo === 'superficie-total'
                ? ['superficie-total-min', 'superficie-total-max']
                : ['superficie-cubierta-min', 'superficie-cubierta-max'];
            limpiarInputsPorId(inputIds);

            const rango = rangos[valor];
            if (rango) {
                if (tipo === 'superficie-total') {
                    this.estado.actualizarSuperficieTotal(rango.min, rango.max);
                } else {
                    this.estado.actualizarSuperficieCubierta(rango.min, rango.max);
                }
                toggleCategoriaVisibilidad(tipo, true);
            }
        } else {
            if (tipo === 'superficie-total') {
                this.estado.actualizarSuperficieTotal(null, null);
            } else {
                this.estado.actualizarSuperficieCubierta(null, null);
            }
        }
    }

    _manejarSingleSelect(tipo, valor, checkbox) {
        // Solo desmarcar visualmente, el estado se maneja en toggleFiltroLista
        document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`).forEach(cb => {
            if (cb !== checkbox) cb.checked = false;
        });

        this.estado.toggleFiltroLista(tipo, valor, false);
    }

    // ========== PRECIO SIMPLIFICADO ==========
    _setupPrecioListeners() {
        const selectorMoneda = document.getElementById('selector-moneda');
        const precioOpciones = document.querySelectorAll('.precio-opcion');
        const btnAplicarPrecio = document.getElementById('btn-aplicar-precio');

        if (selectorMoneda) {
            selectorMoneda.addEventListener('change', (e) => {
                this.estado.estado.precio.moneda = e.target.value;
                this._actualizarTextosPrecio(e.target.value);
            });
        }

        precioOpciones.forEach(opcion => {
            const checkbox = opcion.querySelector('input[type="checkbox"]');
            if (checkbox) {
                checkbox.addEventListener('change', () => this._manejarPrecioCheckbox(checkbox));
            }
        });

        if (btnAplicarPrecio) {
            btnAplicarPrecio.addEventListener('click', () => this._aplicarPrecioRango());
        }
    }

    _actualizarTextosPrecio(moneda) {
        document.querySelectorAll('.precio-opcion').forEach(opcion => {
            const span = opcion.querySelector('span');
            const input = opcion.querySelector('input');

            if (span && input) {
                const texto = moneda === 'USD' ? opcion.dataset.usd : opcion.dataset.ars;
                span.textContent = texto;
                input.value = texto;
            }
        });
    }

    _manejarPrecioCheckbox(checkbox) {
        if (checkbox.checked) {
            // Desmarcar otros
            document.querySelectorAll('.precio-opcion input[type="checkbox"]').forEach(cb => {
                if (cb !== checkbox) cb.checked = false;
            });

            limpiarInputsPorId(['precio-min', 'precio-max']);

            const texto = checkbox.value;
            const moneda = document.getElementById('selector-moneda')?.value || 'ARS';
            const rangosMoneda = PRECIO_RANGOS[moneda];
            const rango = rangosMoneda[texto];

            if (rango) {
                this.estado.actualizarPrecio(rango.min, rango.max, moneda);
                toggleCategoriaVisibilidad('precio', true);
            }
        } else {
            this.estado.actualizarPrecio(null, null);
        }
    }

    _aplicarPrecioRango() {
        const minInput = document.getElementById('precio-min');
        const maxInput = document.getElementById('precio-max');
        const selectorMoneda = document.getElementById('selector-moneda');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;
        const moneda = selectorMoneda?.value || 'ARS';

        const rangoValido = validarRango(min, max);

        document.querySelectorAll('.precio-opcion input[type="checkbox"]').forEach(cb => {
            cb.checked = false;
        });

        this.estado.actualizarPrecio(rangoValido.min, rangoValido.max, moneda);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('precio', true);
        }
    }

    // ========== UBICACIÓN SIMPLIFICADA ==========
    _setupUbicacionListener() {
        const inputUbicacion = document.getElementById('input-ubicacion');
        const sugerenciasDiv = document.getElementById('sugerencias-ubicacion');

        if (!inputUbicacion || !sugerenciasDiv) return;

        inputUbicacion.addEventListener('input', (e) => this._manejarBusquedaUbicacion(e.target.value, sugerenciasDiv));

        document.addEventListener('click', (e) => {
            if (!inputUbicacion.contains(e.target) && !sugerenciasDiv.contains(e.target)) {
                sugerenciasDiv.style.display = 'none';
            }
        });
    }

    _manejarBusquedaUbicacion(query, sugerenciasDiv) {
        sugerenciasDiv.innerHTML = '';

        if (query.length < 2) {
            sugerenciasDiv.style.display = 'none';
            return;
        }

        const resultados = UBICACIONES_MOCK.filter(ubicacion =>
            ubicacion.nombre.toLowerCase().includes(query.toLowerCase())
        );

        if (resultados.length > 0) {
            resultados.forEach(ubicacion => {
                const div = document.createElement('div');
                div.className = 'sugerencia-item';
                div.innerHTML = `
                    <strong>${ubicacion.nombre}</strong>
                    <span class="text-muted" style="float: right; font-size: 0.8rem;">${ubicacion.tipo}</span>
                `;
                div.addEventListener('click', () => {
                    document.getElementById('input-ubicacion').value = ubicacion.nombre;

                    if (ubicacion.tipoId === 'provincia') {
                        this.estado.actualizarUbicacion(ubicacion.id, null, null);
                    } else if (ubicacion.tipoId === 'ciudad') {
                        this.estado.actualizarUbicacion(ubicacion.provinciaId || null, ubicacion.id, null);
                    } else if (ubicacion.tipoId === 'barrio') {
                        this.estado.actualizarUbicacion(null, ubicacion.ciudadId || null, ubicacion.id);
                    }

                    sugerenciasDiv.style.display = 'none';
                    toggleCategoriaVisibilidad('ubicacion', true);
                });
                sugerenciasDiv.appendChild(div);
            });
            sugerenciasDiv.style.display = 'block';
        } else {
            sugerenciasDiv.style.display = 'none';
        }
    }

    // ========== RANGOS SIMPLIFICADOS ==========
    _setupRangoListeners() {
        const btnSuperficieTotal = document.getElementById('btn-aplicar-superficie-total');
        const btnSuperficieCubierta = document.getElementById('btn-aplicar-superficie-cubierta');

        if (btnSuperficieTotal) {
            btnSuperficieTotal.addEventListener('click', () => this._aplicarSuperficieTotal());
        }

        if (btnSuperficieCubierta) {
            btnSuperficieCubierta.addEventListener('click', () => this._aplicarSuperficieCubierta());
        }
    }

    _aplicarSuperficieTotal() {
        const minInput = document.getElementById('superficie-total-min');
        const maxInput = document.getElementById('superficie-total-max');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;

        desmarcarCheckboxesPorTipo('superficie-total');

        const rangoValido = validarRango(min, max);
        this.estado.actualizarSuperficieTotal(rangoValido.min, rangoValido.max);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('superficie-total', true);
        }
    }

    _aplicarSuperficieCubierta() {
        const minInput = document.getElementById('superficie-cubierta-min');
        const maxInput = document.getElementById('superficie-cubierta-max');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;

        desmarcarCheckboxesPorTipo('superficie-cubierta');

        const rangoValido = validarRango(min, max);
        this.estado.actualizarSuperficieCubierta(rangoValido.min, rangoValido.max);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('superficie-cubierta', true);
        }
    }

    // ========== LIMPIAR UI ==========
    limpiarUI() {
        // Esto se maneja ahora desde CheckboxSync
    }
}

export { UIHandlers };