// Función de Vercel: saca la foto (y el nombre) de la página de un producto de cualquier tienda online.
// El navegador no puede leer esas páginas directamente (las tiendas no lo permiten por CORS),
// así que la web llama a /api/producto?url=… y esta función, desde el servidor, abre la página,
// busca la foto principal (og:image, JSON-LD…) y la devuelve ya descargada como data URL.
//
//   GET /api/producto?url=https://www.zara.com/es/es/…-p05755125.html
//   → { ok: true, foto: "data:image/jpeg;base64,…", titulo: "Jersey punto básico" }
//   → { ok: true, foto: "data:image/jpeg;base64,…", titulo: "Jersey punto básico", tienda: "ZARA" }
//   → { ok: false, error: "no_permitido" | "tienda_bloquea" | "sin_foto" | "fallo" }

const dns = require('dns').promises;
const net = require('net');

// Vale cualquier tienda, pero nunca direcciones internas (localhost, 192.168…, la red de Vercel…),
// para que nadie pueda usar la función para colarse en servidores privados.
function ipPrivada(ip) {
  if (net.isIPv4(ip)) {
    const [a, b] = ip.split('.').map(Number);
    return a === 0 || a === 10 || a === 127 || (a === 169 && b === 254) || (a === 172 && b >= 16 && b <= 31)
      || (a === 192 && b === 168) || (a === 100 && b >= 64 && b <= 127) || a >= 224;
  }
  const x = ip.toLowerCase();
  if (x.startsWith('::ffff:')) return ipPrivada(x.slice(7));
  return x === '::' || x === '::1' || x.startsWith('fc') || x.startsWith('fd') || x.startsWith('fe8') || x.startsWith('fe9')
    || x.startsWith('fea') || x.startsWith('feb') || x.startsWith('ff');
}

async function direccionPermitida(u) {
  if (!/^https?:$/.test(u.protocol) || (u.port && u.port !== '80' && u.port !== '443')) return false;
  const host = u.hostname.replace(/^\[|\]$/g, '');
  if (host === 'localhost' || host.endsWith('.localhost') || host.endsWith('.internal') || host.endsWith('.local')) return false;
  if (net.isIP(host)) return !ipPrivada(host);
  try {
    const ips = await dns.lookup(host, { all: true });
    return ips.length > 0 && ips.every(i => !ipPrivada(i.address));
  } catch { return false; }
}

// fetch que sigue las redirecciones a mano, comprobando cada salto
async function abrir(url, opciones, saltos = 5) {
  let actual = new URL(url);
  for (let i = 0; i <= saltos; i++) {
    if (!(await direccionPermitida(actual))) { const e = new Error('no_permitido'); e.codigo = 'no_permitido'; throw e; }
    const r = await fetch(actual.href, { ...opciones, redirect: 'manual' });
    const destino = r.headers.get('location');
    if (r.status >= 300 && r.status < 400 && destino) { actual = new URL(destino, actual); continue; }
    return r;
  }
  throw new Error('Demasiadas redirecciones');
}

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
  const r = await abrir(url, { headers: { ...CABECERAS, Accept: 'image/avif,image/webp,image/jpeg,image/*,*/*;q=0.8', Referer: pagina } });
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
  try {
    const r = await abrir(pagina.href, { headers: CABECERAS });
    if (r.status === 403 || r.status === 429) return res.status(200).json({ ok: false, error: 'tienda_bloquea' });
    if (!r.ok) return res.status(200).json({ ok: false, error: 'fallo', detalle: `La tienda respondió ${r.status}` });
    const html = await r.text();
    const referencia = ((pagina.pathname + pagina.search).match(/\d{6,}/) || [])[0] || '';
    const tienda = (meta(html, 'og:site_name') || '').trim();
    // «Jersey punto básico - ZARA España» → «Jersey punto básico» (quita el nombre de la tienda del final)
    let titulo = (meta(html, 'og:title') || (html.match(/<title[^>]*>([^<]*)<\/title>/i) || [])[1] || '').trim();
    titulo = decodificar(titulo).replace(/\s+[|·–—-]\s+[^|·–—-]*$/, '').trim();
    if (tienda && titulo.toLowerCase().endsWith(tienda.toLowerCase())) titulo = titulo.slice(0, -tienda.length).replace(/[\s|·–—-]+$/, '').trim();

    let foto = buscarFoto(html, referencia);
    if (!foto) return res.status(200).json({ ok: false, error: 'sin_foto', titulo, tienda });
    if (foto.startsWith('//')) foto = 'https:' + foto;
    foto = new URL(foto, pagina.href).href;
    foto = foto.replace(/\{width\}|\{w\}/gi, '750');

    const datos = await descargarFoto(foto, pagina.href);
    if (!datos) return res.status(200).json({ ok: false, error: 'sin_foto', titulo, tienda });
    return res.status(200).json({ ok: true, foto: datos, titulo, tienda });
  } catch (e) {
    if (e.codigo === 'no_permitido') return res.status(400).json({ ok: false, error: 'no_permitido' });
    console.error('Error leyendo el producto:', e.message);
    return res.status(200).json({ ok: false, error: 'fallo', detalle: String(e.message || e).slice(0, 200) });
  }
};
