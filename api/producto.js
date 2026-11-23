// Función de Vercel: saca la foto (y el nombre) de la página de un producto de tienda (Zara, Mango, H&M…).
// El navegador no puede leer esas páginas directamente (las tiendas no lo permiten por CORS),
// así que la web llama a /api/producto?url=… y esta función, desde el servidor, abre la página,
// busca la foto principal (og:image, JSON-LD…) y la devuelve ya descargada como data URL.
//
//   GET /api/producto?url=https://www.zara.com/es/es/…-p05755125.html
//   → { ok: true, foto: "data:image/jpeg;base64,…", titulo: "Jersey punto básico" }
//   → { ok: false, error: "no_permitido" | "tienda_bloquea" | "sin_foto" | "fallo" }

// Solo se abren páginas de estas tiendas (para que nadie use la función para leer cualquier web)
const TIENDAS = ['zara.', 'mango.', 'bershka.', 'pullandbear.', 'stradivarius.', 'massimodutti.', 'oysho.', 'lefties.',
  'hm.com', 'primark.', 'uniqlo.', 'shein.', 'nike.', 'adidas.', 'elcorteingles.', 'springfield.'];

const CABECERAS = {
  'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36',
  'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8',
  'Accept-Language': 'es-ES,es;q=0.9,en;q=0.8',
};

const MAX_FOTO = 6 * 1024 * 1024; // 6 MB

function decodificar(s) {
  return s.replace(/&amp;/g, '&').replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&lt;/g, '<').replace(/&gt;/g, '>')
    .replace(/\\u002F/gi, '/').replace(/\\\//g, '/');
}

function meta(html, nombre) {
  // <meta property="og:image" content="…"> en cualquier orden de atributos
  const re = new RegExp(`<meta[^>]+(?:property|name)=["']${nombre}["'][^>]*>`, 'i');
  const etiqueta = (html.match(re) || [])[0];
  if (!etiqueta) return null;
  const c = etiqueta.match(/content=["']([^"']+)["']/i);
  return c ? decodificar(c[1]).trim() : null;
}

function imagenJsonLd(html) {
  const bloques = html.match(/<script[^>]+application\/ld\+json[^>]*>[\s\S]*?<\/script>/gi) || [];
  for (const b of bloques) {
    try {
      const j = JSON.parse(b.replace(/^<script[^>]*>/i, '').replace(/<\/script>$/i, ''));
      const lista = Array.isArray(j) ? j : (j['@graph'] || [j]);
      for (const o of lista) {
        let img = o && o.image;
        if (Array.isArray(img)) img = img[0];
        if (img && typeof img === 'object') img = img.url || img.contentUrl;
        if (typeof img === 'string' && img) return img;
      }
    } catch { /* bloque mal formado: se ignora */ }
  }
  return null;
}

// Último recurso: una URL de imagen que lleve la referencia del producto (Zara la mete en el JSON de la página)
function imagenConReferencia(html, referencia) {
  if (!referencia) return null;
  const re = /https?:(?:\/\/|\\u002F\\u002F|\\\/\\\/)[^"'\s<>()]+?\.(?:jpe?g|png|webp)[^"'\s<>()]*/gi;
  for (const m of html.match(re) || []) {
    const u = decodificar(m);
    if (u.includes(referencia)) return u;
  }
  return null;
}

function buscarFoto(html, referencia) {
  return meta(html, 'og:image:secure_url') || meta(html, 'og:image') || meta(html, 'twitter:image')
    || imagenJsonLd(html) || imagenConReferencia(html, referencia)
    || ((html.match(/<link[^>]+rel=["']image_src["'][^>]+href=["']([^"']+)["']/i) || [])[1] ?? null);
}

async function descargarFoto(url, pagina) {
  const r = await fetch(url, { headers: { ...CABECERAS, Accept: 'image/avif,image/webp,image/jpeg,image/*,*/*;q=0.8', Referer: pagina } });
  if (!r.ok) return null;
  const tipo = (r.headers.get('content-type') || '').split(';')[0].trim();
  if (!tipo.startsWith('image/')) return null;
  const datos = Buffer.from(await r.arrayBuffer());
  if (datos.length === 0 || datos.length > MAX_FOTO) return null;
  return `data:${tipo};base64,${datos.toString('base64')}`;
}

module.exports = async (req, res) => {
  res.setHeader('Cache-Control', 'no-store');
  let pagina;
  try { pagina = new URL(String(req.query.url || '')); } catch { return res.status(400).json({ ok: false, error: 'no_permitido' }); }
  const host = pagina.hostname.toLowerCase();
  if (!/^https?:$/.test(pagina.protocol) || !TIENDAS.some(t => host.includes(t))) {
    return res.status(400).json({ ok: false, error: 'no_permitido' });
  }
  try {
    const r = await fetch(pagina.href, { headers: CABECERAS, redirect: 'follow' });
    if (r.status === 403 || r.status === 429) return res.status(200).json({ ok: false, error: 'tienda_bloquea' });
    if (!r.ok) return res.status(200).json({ ok: false, error: 'fallo', detalle: `La tienda respondió ${r.status}` });
    const html = await r.text();
    const referencia = ((pagina.pathname + pagina.search).match(/\d{6,}/) || [])[0] || '';
    const titulo = (meta(html, 'og:title') || '').replace(/\s*[|·–-]\s*(ZARA|MANGO|H&M|Bershka|Pull&Bear|Stradivarius|Massimo Dutti|Oysho|Lefties|Primark|UNIQLO|SHEIN|Nike|adidas|El Corte Inglés|Springfield).*$/i, '').trim();

    let foto = buscarFoto(html, referencia);
    if (!foto) return res.status(200).json({ ok: false, error: 'sin_foto', titulo });
    if (foto.startsWith('//')) foto = 'https:' + foto;
    foto = new URL(foto, pagina.href).href;
    foto = foto.replace(/\{width\}|\{w\}/gi, '750');

    const datos = await descargarFoto(foto, pagina.href);
    if (!datos) return res.status(200).json({ ok: false, error: 'sin_foto', titulo });
    return res.status(200).json({ ok: true, foto: datos, titulo });
  } catch (e) {
    console.error('Error leyendo el producto:', e.message);
    return res.status(200).json({ ok: false, error: 'fallo', detalle: String(e.message || e).slice(0, 200) });
  }
};
