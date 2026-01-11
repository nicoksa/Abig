// modules/checkboxSync.js
import { SUPERFICIE_TOTAL_RANGOS, SUPERFICIE_CUBIERTA_RANGOS, PRECIO_RANGOS } from './constants.js';

class CheckboxSync {
    constructor(estadoManager) {
        this.estado = estadoManager;
    }

    // ========== SINCRONIZAR TODOS LOS CHECKBOXES ==========
    sincronizarTodos() {
        const estado = this.estado.getEstado();

        // Sincronizar checkboxes de tipo simple
        this._sincronizarGrupoSimple('tipo', estado.tipo);
        this._sincronizarGrupoSimple('operacion', estado.operacion);
        this._sincronizarGrupoSimple('ambientes', estado.ambientes);
        this._sincronizarGrupoSimple('dormitorios', estado.dormitorios);
        this._sincronizarGrupoSimple('banos', estado.banos);
        this._sincronizarGrupoSimple('antiguedad', estado.antiguedad);

        // Sincronizar características
        this._sincronizarCaracteristicas(estado.caracteristicas);

        // Sincronizar rangos especiales
        this._sincronizarSuperficie('superficie-total', estado.superficieTotal, SUPERFICIE_TOTAL_RANGOS);
        this._sincronizarSuperficie('superficie-cubierta', estado.superficieCubierta, SUPERFICIE_CUBIERTA_RANGOS);
        this._sincronizarPrecio(estado.precio);
    }

    // ========== SINCRONIZAR GRUPO SIMPLE ==========
    _sincronizarGrupoSimple(tipo, valoresEstado) {
        const checkboxes = document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`);

        checkboxes.forEach(checkbox => {
            const estaEnEstado = valoresEstado.includes(checkbox.value);
            const estaMarcado = checkbox.checked;

            // Solo cambiar si hay discrepancia
            if (estaMarcado !== estaEnEstado) {
                checkbox.checked = estaEnEstado;
            }
        });
    }

    // ========== SINCRONIZAR CARACTERÍSTICAS ==========
    _sincronizarCaracteristicas(valoresEstado) {
        // Buscar en todos los grupos posibles
        const selectores = [
            '[data-tipo="features"] input[type="checkbox"]',
            '[data-tipo="features-campo"] input[type="checkbox"]',
            '[data-tipo="caracteristicas"] input[type="checkbox"]'
        ].join(', ');

        const checkboxes = document.querySelectorAll(selectores);

        checkboxes.forEach(checkbox => {
            const estaEnEstado = valoresEstado.includes(checkbox.value);
            const estaMarcado = checkbox.checked;

            if (estaMarcado !== estaEnEstado) {
                checkbox.checked = estaEnEstado;
            }
        });
    }

    // ========== SINCRONIZAR SUPERFICIE ==========
    _sincronizarSuperficie(tipo, superficieEstado, rangos) {
        const checkboxes = document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`);

        // Si no hay filtro de superficie, desmarcar todo
        if (superficieEstado.min === null && superficieEstado.max === null) {
            checkboxes.forEach(cb => {
                if (cb.checked) cb.checked = false;
            });
            return;
        }

        // Encontrar el checkbox que corresponde al rango actual
        let checkboxValorCorrespondiente = null;

        for (const [texto, rango] of Object.entries(rangos)) {
            if (rango.min === superficieEstado.min && rango.max === superficieEstado.max) {
                checkboxValorCorrespondiente = texto;
                break;
            }
        }

        // Sincronizar todos los checkboxes del grupo
        checkboxes.forEach(cb => {
            const deberiaEstarMarcado = (cb.value === checkboxValorCorrespondiente);
            if (cb.checked !== deberiaEstarMarcado) {
                cb.checked = deberiaEstarMarcado;
            }
        });
    }

    // ========== SINCRONIZAR PRECIO ==========
    _sincronizarPrecio(precioEstado) {
        const checkboxes = document.querySelectorAll('[data-tipo="precio"] input[type="checkbox"]');

        // Si no hay filtro de precio, desmarcar todo
        if (precioEstado.min === null && precioEstado.max === null) {
            checkboxes.forEach(cb => {
                if (cb.checked) cb.checked = false;
            });
            return;
        }

        // Encontrar el checkbox que corresponde al rango actual
        const rangos = PRECIO_RANGOS[precioEstado.moneda];
        let checkboxValorCorrespondiente = null;

        for (const [texto, rango] of Object.entries(rangos)) {
            if (rango.min === precioEstado.min && rango.max === precioEstado.max) {
                checkboxValorCorrespondiente = texto;
                break;
            }
        }

        // Sincronizar todos los checkboxes de precio
        checkboxes.forEach(cb => {
            const deberiaEstarMarcado = (cb.value === checkboxValorCorrespondiente);
            if (cb.checked !== deberiaEstarMarcado) {
                cb.checked = deberiaEstarMarcado;
            }
        });
    }

    // ========== LIMPIAR TODOS LOS CHECKBOXES ==========
    limpiarTodos() {
        // Desmarcar todos los checkboxes
        document.querySelectorAll('.acordeon-contenido input[type="checkbox"]').forEach(cb => {
            cb.checked = false;
        });
    }
}

export { CheckboxSync };