// initFiltrosFromIndex.js - Carga inicial de filtros desde el formulario de Index (VERSIÓN CORREGIDA)
document.addEventListener('DOMContentLoaded', function() {
    // Solo ejecutar si estamos en la página de resultados
    if (!document.querySelector('.resultados-wrapper')) return;
    
    console.log('🔍 Inicializando filtros desde formulario de Index...');
    
    // Función para crear etiqueta de filtro aplicado
    function crearEtiquetaFiltro(label, valor, tipo, valorOriginal = null) {
        const contenedor = document.getElementById('filtros-aplicados');
        if (!contenedor) return null;
        
        // NO crear etiqueta si el valor está vacío
        if (!valor || valor.trim() === '') return null;
        
        const etiqueta = document.createElement('div');
        etiqueta.className = 'etiqueta-filtro';
        etiqueta.innerHTML = `
            <span class="nombre"><strong>${label}:</strong> ${valor}</span>
            <span class="quitar" data-tipo="${tipo}" data-valor="${valorOriginal || valor}">×</span>
        `;
        
        // Listener para el botón quitar
        const botonQuitar = etiqueta.querySelector('.quitar');
        if (botonQuitar) {
            botonQuitar.addEventListener('click', (e) => {
                e.stopPropagation();
                console.log(`🗑️ Quitando filtro: ${tipo} = ${valorOriginal || valor}`);
                
                // Disparar evento para que el sistema principal maneje la remoción
                document.dispatchEvent(new CustomEvent('quitarFiltroIndex', {
                    detail: { tipo, valor: valorOriginal || valor }
                }));
            });
        }
        
        contenedor.appendChild(etiqueta);
        return etiqueta;
    }
    
    // Leer parámetros de la URL
    const params = new URLSearchParams(window.location.search);
    
    // Función helper para obtener display text de filtros
    function getDisplayText(paramName, value) {
        // Si el valor está vacío, retornar null
        if (!value || value.trim() === '') return null;
        
        const displayMap = {
            'operacion': {
                'Alquiler': 'Alquiler',
                'AlquilerTemporal': 'Alquiler Temporal'
            },
            'tipo': {
                'Departamento': 'Departamento',
                'Casa': 'Casa',
                'PH': 'PH',
                'Local': 'Local',
                'Terreno': 'Terreno',
                'Quinta': 'Quinta',
                'Cochera': 'Cochera',
                'Oficina': 'Oficina'
            },
            'dormitorios': {
                '1': '1 dormitorio',
                '2': '2 dormitorios',
                '3': '3 dormitorios',
                '4': '4 dormitorios',
                '5': '5+ dormitorios'
            },
            'ambientes': {
                '1': '1 ambiente',
                '2': '2 ambientes',
                '3': '3 ambientes',
                '4': '4+ ambientes'
            },
            'banos': {
                '1': '1 baño',
                '2': '2 baños',
                '3': '3 baños',
                '4': '4 baños',
                '5': '5+ baños'
            }
        };
        
        const paramLower = paramName.toLowerCase();
        if (displayMap[paramLower] && displayMap[paramLower][value]) {
            return displayMap[paramLower][value];
        }
        return value;
    }
    
    // Procesar cada parámetro
    let totalFiltros = 0;
    const etiquetasCreadas = [];
    
    // Operación (solo crear si tiene valor)
    if (params.has('operacion')) {
        const valor = params.get('operacion');
        const display = getDisplayText('operacion', valor);
        if (display) {
            const etiqueta = crearEtiquetaFiltro('Operación', display, 'operacion', valor);
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
            }
        }
    }
    
    // Tipo (solo crear si tiene valor)
    if (params.has('tipo')) {
        const valor = params.get('tipo');
        const display = getDisplayText('tipo', valor);
        if (display) {
            const etiqueta = crearEtiquetaFiltro('Tipo', display, 'tipo', valor);
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
            }
        }
    }
    
    // Dormitorios (solo crear si tiene valor)
    if (params.has('dormitorios')) {
        const valor = params.get('dormitorios');
        const display = getDisplayText('dormitorios', valor);
        if (display) {
            const etiqueta = crearEtiquetaFiltro('Dormitorios', display, 'dormitorios', display);
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
            }
        }
    }
    
    // Ambientes (solo crear si tiene valor)
    if (params.has('ambientes')) {
        const valor = params.get('ambientes');
        const display = getDisplayText('ambientes', valor);
        if (display) {
            const etiqueta = crearEtiquetaFiltro('Ambientes', display, 'ambientes', display);
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
            }
        }
    }
    
    // Baños (solo crear si tiene valor)
    if (params.has('banos')) {
        const valor = params.get('banos');
        const display = getDisplayText('banos', valor);
        if (display) {
            const etiqueta = crearEtiquetaFiltro('Baños', display, 'banos', display);
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
            }
        }
    }
    
    // Precio (solo crear si tiene al menos un valor)
    if (params.has('PrecioMin') || params.has('PrecioMax')) {
        const min = params.get('PrecioMin') || '';
        const max = params.get('PrecioMax') || '';
        const moneda = params.get('Moneda') || 'ARS';
        
        // Solo crear etiqueta si hay al menos un valor
        if (min || max) {
            const simbolo = moneda === 'USD' ? 'USD$' : '$';
            let minFormatted = '';
            let maxFormatted = '';
            
            if (min) {
                const minNum = parseFloat(min);
                if (!isNaN(minNum)) {
                    minFormatted = simbolo + minNum.toLocaleString();
                }
            }
            
            if (max) {
                const maxNum = parseFloat(max);
                if (!isNaN(maxNum)) {
                    maxFormatted = simbolo + maxNum.toLocaleString();
                }
            }
            
            const texto = `${minFormatted} ${min && max ? '-' : ''} ${maxFormatted}`.trim();
            
            if (texto) {
                const etiqueta = crearEtiquetaFiltro('Precio', texto, 'precio');
                if (etiqueta) {
                    etiquetasCreadas.push(etiqueta);
                    totalFiltros++;
                }
            }
        }
    }
    
    // Ubicación 
    if (params.has('provincia') || params.has('ciudad') || params.has('barrio')) {
        const provinciaId = params.get('provincia');
        const ciudadId = params.get('ciudad');
        const barrioId = params.get('barrio');
        
        // Intentar obtener el texto de ubicación del input si existe
        const inputUbicacion = document.getElementById('input-ubicacion');
        let textoUbicacion = '';
        
        if (inputUbicacion && inputUbicacion.value) {
            textoUbicacion = inputUbicacion.value;
        } else {
            // Si no hay texto en el input, usar IDs para mostrar algo descriptivo
            if (barrioId) {
                textoUbicacion = `Barrio ${barrioId}`;
            } else if (ciudadId) {
                textoUbicacion = `Ciudad ${ciudadId}`;
            } else if (provinciaId) {
                textoUbicacion = `Provincia ${provinciaId}`;
            } else {
                textoUbicacion = 'Ubicación seleccionada';
            }
        }
        
        if (textoUbicacion) {
            const etiqueta = crearEtiquetaFiltro('Ubicación', textoUbicacion, 'ubicacion');
            if (etiqueta) {
                etiquetasCreadas.push(etiqueta);
                totalFiltros++;
                
                // Agregar data attributes para referencia
                if (provinciaId) etiqueta.dataset.provinciaId = provinciaId;
                if (ciudadId) etiqueta.dataset.ciudadId = ciudadId;
                if (barrioId) etiqueta.dataset.barrioId = barrioId;
            }
        }
    }
    
    // Si no hay etiquetas creadas, mostrar mensaje
    const contenedor = document.getElementById('filtros-aplicados');
    if (contenedor && etiquetasCreadas.length === 0) {
        contenedor.innerHTML = '<p class="text-muted" style="margin: 0;">No hay filtros aplicados</p>';
    }
    
    // Mostrar contador en el título si hay filtros
    if (totalFiltros > 0) {
        const titulo = document.querySelector('.filtros-titulo');
        if (titulo) {
            let badge = titulo.querySelector('.badge');
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'badge bg-primary ms-2';
                titulo.appendChild(badge);
            }
            badge.textContent = totalFiltros;
        }
        
        // Agregar botón limpiar todos solo si hay filtros
        if (contenedor && etiquetasCreadas.length > 0) {
            const botonLimpiar = document.createElement('button');
            botonLimpiar.className = 'btn btn-limpiar';
            botonLimpiar.textContent = 'Limpiar todos';
            botonLimpiar.addEventListener('click', () => {
                window.location.href = '/Results2';
            });
            contenedor.appendChild(botonLimpiar);
        }
    }
    
    console.log(`✅ Filtros desde Index cargados: ${totalFiltros} filtros válidos`);
    
    // Escuchar eventos para quitar filtros específicos
    document.addEventListener('quitarFiltroIndex', (e) => {
        console.log('🔄 Quitando filtro desde Index', e.detail);
    });
    
    // MEJORA: Intentar obtener nombres reales de ubicación via API
    setTimeout(() => {
        mejorarTextoUbicacion(params);
    }, 500);
});

// Función para mejorar el texto de ubicación llamando a la API
async function mejorarTextoUbicacion(params) {
    const provinciaId = params.get('provincia');
    const ciudadId = params.get('ciudad');
    const barrioId = params.get('barrio');
    
    if (!provinciaId && !ciudadId && !barrioId) return;
    
    try {
        let locationText = '';
        
        // Si tenemos barrio, obtener información completa
        if (barrioId) {
            const response = await fetch(`/api/location/cities/${ciudadId}/neighborhoods`);
            const neighborhoods = await response.json();
            const barrio = neighborhoods.find(n => n.neighborhoodId == barrioId);
            
            if (barrio) {
                // Ahora obtener ciudad
                const cityResponse = await fetch(`/api/location/provinces/${provinciaId}/cities`);
                const cities = await cityResponse.json();
                const ciudad = cities.find(c => c.cityId == ciudadId);
                
                if (ciudad) {
                    // Finalmente obtener provincia
                    const provinceResponse = await fetch('/api/location/provinces');
                    const provinces = await provinceResponse.json();
                    const provincia = provinces.find(p => p.provinceId == provinciaId);
                    
                    if (provincia) {
                        locationText = `${barrio.name}, ${ciudad.name}, ${provincia.name}`;
                    }
                }
            }
        }
        // Si tenemos ciudad pero no barrio
        else if (ciudadId) {
            const response = await fetch(`/api/location/provinces/${provinciaId}/cities`);
            const cities = await response.json();
            const ciudad = cities.find(c => c.cityId == ciudadId);
            
            if (ciudad) {
                const provinceResponse = await fetch('/api/location/provinces');
                const provinces = await provinceResponse.json();
                const provincia = provinces.find(p => p.provinceId == provinciaId);
                
                if (provincia) {
                    locationText = `${ciudad.name}, ${provincia.name}`;
                }
            }
        }
        // Si solo tenemos provincia
        else if (provinciaId) {
            const response = await fetch('/api/location/provinces');
            const provinces = await response.json();
            const provincia = provinces.find(p => p.provinceId == provinciaId);
            
            if (provincia) {
                locationText = provincia.name;
            }
        }
        
        // Actualizar la etiqueta de ubicación si encontramos texto mejorado
        if (locationText) {
            const etiquetaUbicacion = document.querySelector('.etiqueta-filtro .nombre[data-tipo="ubicacion"]')?.closest('.etiqueta-filtro');
            if (etiquetaUbicacion) {
                const nombreSpan = etiquetaUbicacion.querySelector('.nombre');
                if (nombreSpan) {
                    nombreSpan.innerHTML = `<strong>Ubicación:</strong> ${locationText}`;
                    console.log(`📍 Texto de ubicación mejorado: ${locationText}`);
                }
            }
        }
    } catch (error) {
        console.log('⚠️ No se pudo mejorar el texto de ubicación, usando texto por defecto');
    }
}