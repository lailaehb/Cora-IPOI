// Función de Vercel: el puente entre Cora y la IA (Google Gemini).
// La web (Blazor) no puede guardar la clave de la IA porque cualquiera la vería en el navegador,
// así que la web llama a /api/ia y esta función, que se ejecuta en el servidor, llama a Gemini.
//
// Configuración en Vercel → Settings → Environment Variables:
//   GEMINI_API_KEY  (obligatoria) la clave gratuita de https://aistudio.google.com/apikey
//   GEMINI_MODEL    (opcional)    por defecto «gemini-3.5-flash-lite»
//
// Acciones:
//   { accion: "semana", dias, armario, usadas, gustos, estilos }  → { ok, dias: [{ fecha, looks: [{ momento, plan, ids, porque }] }] }
//   { accion: "foto", imagen, tipos }                              → { ok, prenda: { esPrenda, nombre, categoria, tipo, color, usos, abrigo, lluvia } }

const API = 'https://generativelanguage.googleapis.com/v1beta';
const MODELO_DEFECTO = 'gemini-3.5-flash-lite';
const PLANES = ['uni', 'trabajo', 'cena', 'fiesta', 'evento', 'deporte', 'baile', 'casa', 'otro'];
const USOS = ['Deporte', 'Diario', 'Arreglado', 'Fiesta'];
const CATEGORIAS = ['Arriba', 'Abajo', 'Calzado', 'Capa'];

// Idioma de la app: la IA escribe sus textos (momento, porque, nombre, color) en ese idioma.
// Las categorías, tipos y planes siguen en español porque son los valores internos de la app.
const IDIOMAS = { es: 'español', en: 'inglés (English)', fr: 'francés (français)' };
function enIdioma(idioma) {
  if (!idioma || idioma === 'es' || !IDIOMAS[idioma]) return '';
  return `\n- IMPORTANTE: escribe todos los textos libres para el usuario en ${IDIOMAS[idioma]}. Los valores de las listas cerradas (categorías, tipos, planes, usos e ids) déjalos exactamente como están.`;
}

let modeloEncontrado = null; // si el modelo por defecto deja de existir, se busca otro y se recuerda

// ------------------------------------------------------------------ llamada a Gemini
async function llamarGemini(clave, modelo, instrucciones, partes, esquema) {
  const r = await fetch(`${API}/models/${modelo}:generateContent`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'x-goog-api-key': clave },
    body: JSON.stringify({
      systemInstruction: { parts: [{ text: instrucciones }] },
      contents: [{ role: 'user', parts: partes }],
      generationConfig: { responseMimeType: 'application/json', responseSchema: esquema },
    }),
  });
  const cuerpo = await r.json().catch(() => ({}));
  if (!r.ok) {
    const e = new Error(cuerpo?.error?.message || `Gemini respondió ${r.status}`);
    e.status = r.status;
    throw e;
  }
  const texto = (cuerpo?.candidates?.[0]?.content?.parts || []).map(p => p.text || '').join('');
  if (!texto) throw new Error('La IA no ha devuelto respuesta');
  // Por si acaso viene envuelto en ```json … ```
  return JSON.parse(texto.replace(/^```(json)?/i, '').replace(/```$/, '').trim());
}

// Si el modelo configurado no existe (404), elige el Flash-Lite más nuevo de la lista de modelos.
async function buscarModelo(clave) {
  const r = await fetch(`${API}/models?pageSize=200`, { headers: { 'x-goog-api-key': clave } });
  const j = await r.json().catch(() => ({}));
  const nombres = (j.models || [])
    .filter(m => (m.supportedGenerationMethods || []).includes('generateContent'))
    .map(m => m.name.replace(/^models\//, ''))
    .filter(n => /^gemini-[\d.]+-flash/.test(n) && !/preview|exp|image|tts|live|audio/.test(n));
  const version = n => parseFloat((n.match(/gemini-([\d.]+)/) || [])[1] || '0');
  nombres.sort((a, b) => (b.includes('lite') - a.includes('lite')) || version(b) - version(a));
  return nombres[0] || null;
}

async function preguntar(clave, instrucciones, partes, esquema) {
  const modelo = modeloEncontrado || process.env.GEMINI_MODEL || MODELO_DEFECTO;
  try {
    return await llamarGemini(clave, modelo, instrucciones, partes, esquema);
  } catch (e) {
    if (e.status !== 404) throw e;
    const otro = await buscarModelo(clave);
    if (!otro || otro === modelo) throw e;
    modeloEncontrado = otro;
    return await llamarGemini(clave, otro, instrucciones, partes, esquema);
  }
}

// ------------------------------------------------------------------ planificar la semana
const INSTRUCCIONES_SEMANA = `Eres Cora, una estilista que prepara los conjuntos de la semana con la ropa que el usuario TIENE en su armario.
Reglas:
- Usa SOLO prendas de la lista "armario" y escribe sus "id" exactamente. No inventes prendas.
- Cada conjunto lleva una prenda de arriba y una de abajo, calzado y, si hace fresco (menos de 19 °C) o llueve, una prenda de la categoría Capa. Respeta la categoría de cada prenda.
- Elige prendas cuyo campo "usos" encaje con el plan: deporte o baile → "Deporte"; universidad, recados, campo, parque, familia, amigos, viajes o día tranquilo → "Diario"; trabajo, comida, cena, concierto, teatro o un evento → "Arreglado" (o "Diario" si no hay nada mejor); fiesta → "Fiesta" o "Arreglado". La persona cuenta su vida con sus palabras ("comer pipas en un banco", "ir a la parcela", "hacerme las uñas"): entiéndelo como lo entendería una amiga.
- Cada día trae los botones marcados ("planes") y lo que el usuario ha escrito ("texto"). Lee el texto con atención: puede haber varios planes en un día ("clase por la mañana, gimnasio por la tarde y cena"). Si un día no tiene planes ni texto, es un día tranquilo (ropa de "Diario").
- Si un plan obliga a cambiarse de ropa (el deporte o una clase de baile como flamenco, sevillanas o zumba), haz un conjunto aparte y cómodo para ese momento (plan "deporte" o "baile"). Ver un partido NO es hacer deporte: es ropa de calle. Si varios planes se pueden hacer con la misma ropa, usa un solo conjunto. Máximo 3 conjuntos por día.
- Con frío prioriza prendas de "abrigo" alto; con lluvia evita el calzado con "lluvia": false.
- No repitas la misma combinación de arriba + abajo que aparezca en "usadas" ni entre los días que planificas (salvo la ropa de deporte, que sí se puede repetir). Si un día trae "evitar", no propongas esas prendas juntas.
- Ten en cuenta los gustos y estilos del usuario cuando haya varias opciones.
- "momento": 2-5 palabras, por ejemplo "Clase por la mañana", "Gimnasio por la tarde", "Cena con amigas".
- "porque": 1 o 2 frases cortas, en español, tuteando, explicando por qué ese conjunto (plan, tiempo…). Sin emojis.
- Devuelve un elemento en "dias" por cada día recibido, con la misma "fecha".`;

function esquemaSemana(ids) {
  return {
    type: 'OBJECT',
    properties: {
      dias: {
        type: 'ARRAY',
        items: {
          type: 'OBJECT',
          properties: {
            fecha: { type: 'STRING' },
            looks: {
              type: 'ARRAY',
              items: {
                type: 'OBJECT',
                properties: {
                  momento: { type: 'STRING' },
                  plan: { type: 'STRING', enum: PLANES },
                  ids: { type: 'ARRAY', items: { type: 'STRING', enum: ids } },
                  porque: { type: 'STRING' },
                },
                required: ['momento', 'plan', 'ids', 'porque'],
              },
            },
          },
          required: ['fecha', 'looks'],
        },
      },
    },
    required: ['dias'],
  };
}

async function semana(clave, b) {
  const disponibles = (b.armario || []).filter(p => p && p.id);
  if (disponibles.length < 3) return { ok: false, error: 'pocas_prendas' };
  if (!Array.isArray(b.dias) || b.dias.length === 0) return { ok: false, error: 'sin_dias' };
  const datos = {
    dias: b.dias.slice(0, 14),
    armario: disponibles.slice(0, 300),
    usadas: (b.usadas || []).slice(0, 40),
    gustos: b.gustos || [],
    estilos: b.estilos || [],
  };
  const res = await preguntar(clave, INSTRUCCIONES_SEMANA + enIdioma(b.idioma),
    [{ text: 'Prepara los conjuntos con estos datos (JSON):\n' + JSON.stringify(datos) }],
    esquemaSemana(disponibles.map(p => String(p.id))));
  return { ok: true, dias: Array.isArray(res.dias) ? res.dias : [] };
}

// ------------------------------------------------------------------ reconocer una prenda en una foto
const INSTRUCCIONES_FOTO = `Eres Cora, una app de armario virtual. Te pasan la foto de UNA prenda de ropa o calzado.
Rellena sus datos en español:
- "esPrenda": false si en la foto no hay ropa ni calzado claramente visible.
- "nombre": corto y natural, con tipo y color, por ejemplo "Mallas negras", "Top lencero de satén", "Sudadera gris".
- "categoria": Arriba (camisetas, tops, camisas, sudaderas, jerséis, ropa deportiva de arriba), Abajo (pantalones, vaqueros, faldas, mallas, shorts), Calzado o Capa (chaquetas, abrigos, blazers).
- "tipo": el más parecido de la lista "tipos" para esa categoría.
- "usos": para qué se usa normalmente: "Deporte" (ropa técnica, mallas, chándal, deportivas de correr), "Diario" (clase, recados), "Arreglado" (trabajo, cena) y/o "Fiesta". Puede haber varios, pero sé realista: unas mallas son de Deporte, un top de lentejuelas es de Fiesta.
- "color": el color principal en minúsculas.
- "abrigo": 0 si es fresca, 1 si es media, 2 si abriga mucho.
- "lluvia": false solo para calzado que se estropea con agua (tacones, ante, sandalias).`;

function esquemaFoto(tipos) {
  return {
    type: 'OBJECT',
    properties: {
      esPrenda: { type: 'BOOLEAN' },
      nombre: { type: 'STRING' },
      categoria: { type: 'STRING', enum: CATEGORIAS },
      tipo: tipos.length ? { type: 'STRING', enum: tipos } : { type: 'STRING' },
      color: { type: 'STRING' },
      usos: { type: 'ARRAY', items: { type: 'STRING', enum: USOS } },
      abrigo: { type: 'INTEGER' },
      lluvia: { type: 'BOOLEAN' },
    },
    required: ['esPrenda', 'nombre', 'categoria', 'tipo', 'color', 'usos', 'abrigo', 'lluvia'],
  };
}

async function foto(clave, b) {
  const m = /^data:(image\/[a-z+]+);base64,(.+)$/i.exec(b.imagen || '');
  if (!m) return { ok: false, error: 'sin_imagen' };
  const tipos = (b.tipos || []).map(String).slice(0, 60);
  const res = await preguntar(clave, INSTRUCCIONES_FOTO + enIdioma(b.idioma), [
    { inlineData: { mimeType: m[1], data: m[2] } },
    { text: 'Tipos posibles: ' + tipos.join(', ') },
  ], esquemaFoto(tipos));
  return { ok: true, prenda: res };
}

// ------------------------------------------------------------------ punto de entrada
module.exports = async (req, res) => {
  res.setHeader('Cache-Control', 'no-store');
  if (req.method !== 'POST') return res.status(405).json({ ok: false, error: 'solo_post' });
  const clave = process.env.GEMINI_API_KEY;
  if (!clave) return res.status(503).json({ ok: false, error: 'sin_clave' });
  let b = req.body;
  if (typeof b === 'string') { try { b = JSON.parse(b); } catch { b = {}; } }
  b = b || {};
  try {
    if (b.accion === 'semana') return res.status(200).json(await semana(clave, b));
    if (b.accion === 'foto') return res.status(200).json(await foto(clave, b));
    return res.status(400).json({ ok: false, error: 'accion_desconocida' });
  } catch (e) {
    const cuota = e.status === 429;
    console.error('Error de la IA:', e.status, e.message);
    return res.status(cuota ? 429 : 502).json({ ok: false, error: cuota ? 'cuota' : 'fallo_ia', detalle: String(e.message || e).slice(0, 300) });
  }
};
