// modules/constants.js
export const CATEGORIAS_NO_OCULTABLES = ['features', 'features-campo', 'caracteristicas'];

export const PRECIO_RANGOS = {
    ARS: {
        'Menos de $250000': { min: 0, max: 250000 },
        'Entre $250000 - $750000': { min: 250000, max: 750000 },
        'Más de $750000': { min: 750000, max: null }
    },
    USD: {
        'Menos de USD$500': { min: 0, max: 500 },
        'USD$500 - USD$1500': { min: 500, max: 1500 },
        'Más de USD$1500': { min: 1500, max: null }
    }
};

export const SUPERFICIE_TOTAL_RANGOS = {
    'Menos de 50 m² totales': { min: 0, max: 50 },
    '50 - 100 m² totales': { min: 50, max: 100 },
    '100 - 200 m² totales': { min: 100, max: 200 },
    'Más de 200 m² totales': { min: 200, max: null }
};

export const SUPERFICIE_CUBIERTA_RANGOS = {
    'Menos de 30 m² cubiertos': { min: 0, max: 30 },
    '30 - 60 m² cubiertos': { min: 30, max: 60 },
    '60 - 100 m² cubiertos': { min: 60, max: 100 },
    'Más de 100 m² cubiertos': { min: 100, max: null }
};

export const ANTIGUEDAD_RANGOS = {
    'A estrenar': { min: 0, max: 0 },
    '1 a 10 años': { min: 1, max: 10 },
    '10 a 25 años': { min: 10, max: 25 },
    '25 a 50 años': { min: 25, max: 50 },
    '50 años o más': { min: 50, max: null }
};



export const ESTADO_INICIAL = {
    tipo: [],
    operacion: [],
    precio: { min: null, max: null, moneda: 'ARS' },
    ubicacion: { provincia: null, ciudad: null, barrio: null },
    ubicacionTexto: '', 
    ambientes: [],
    dormitorios: [],
    banos: [],
    superficieTotal: { min: null, max: null },
    superficieCubierta: { min: null, max: null },
    antiguedad: [],
    caracteristicas: []
};