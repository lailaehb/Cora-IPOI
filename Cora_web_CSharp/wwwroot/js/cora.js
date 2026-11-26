// Funciones del navegador que usa la app desde C# (con IJSRuntime).
// ---------- Actualizarse sola ----------
// Las apps añadidas a la pantalla de inicio (sobre todo en iPhone) no se recargan al abrirlas: siguen con la versión vieja.
// Al volver a la app se mira version.txt (lo crea Vercel al publicar); si ha cambiado, se recarga con la versión nueva.
(function () {
  let actual = null;
  async function mirar() {
    try {
      const r = await fetch('version.txt?t=' + Date.now(), { cache: 'no-store' });
      if (!r.ok) return;
      const v = (await r.text()).trim();
      if (!v) return;
      if (actual === null) actual = v;
      else if (v !== actual) location.reload();
    } catch (e) { }
  }
  mirar();
  document.addEventListener('visibilitychange', function () { if (document.visibilityState === 'visible') mirar(); });
  window.addEventListener('pageshow', function (e) { if (e.persisted) mirar(); });
})();

window.cora = (function () {
  // ---------- Guardar datos en el navegador ----------
  function leer(clave) {
    try { return localStorage.getItem(clave); } catch (e) { return null; }
  }
  function guardar(clave, valor) {
    try { localStorage.setItem(clave, valor); } catch (e) { }
  }

  // ---------- Sonido (Web Audio) ----------
  // El iPhone solo deja sonar audio después de que la persona toque la pantalla,
  // así que «despertamos» el audio con el primer toque.
  let ctx = null;
  function despertar() {
    try {
      ctx = ctx || new (window.AudioContext || window.webkitAudioContext)();
      if (ctx.state === 'suspended') ctx.resume();
    } catch (e) { }
  }
  document.addEventListener('pointerdown', despertar, { passive: true });

  function tonos(lista) {
    despertar();
    if (!ctx) return;
    let t = ctx.currentTime;
    lista.forEach(function (par) {
      const f = par[0], d = par[1];
      const o = ctx.createOscillator(), g = ctx.createGain();
      o.type = 'sine'; o.frequency.value = f;
      g.gain.setValueAtTime(0.0001, t);
      g.gain.exponentialRampToValueAtTime(0.3, t + 0.01);
      g.gain.exponentialRampToValueAtTime(0.0001, t + d);
      o.connect(g).connect(ctx.destination);
      o.start(t); o.stop(t + d + 0.02);
      t += d + 0.04;
    });
  }

  // Campanita: notas que se solapan y se apagan despacio (onda triangular + un armónico suave).
  // Suena más agradable que los pitidos cortos; se usa para las buenas noticias de la lavadora.
  function campana(notas, separacion, duracion) {
    despertar();
    if (!ctx) return;
    const t0 = ctx.currentTime + 0.02;
    notas.forEach(function (f, i) {
      const t = t0 + i * separacion;
      [[f, 'triangle', 0.22], [f * 2, 'sine', 0.05]].forEach(function (capa) {
        const o = ctx.createOscillator(), g = ctx.createGain();
        o.type = capa[1]; o.frequency.value = capa[0];
        g.gain.setValueAtTime(0.0001, t);
        g.gain.exponentialRampToValueAtTime(capa[2], t + 0.015);
        g.gain.exponentialRampToValueAtTime(0.0001, t + duracion);
        o.connect(g).connect(ctx.destination);
        o.start(t); o.stop(t + duracion + 0.05);
      });
    });
  }

  // Sonidos tipo campana (notas en Hz)
  const CAMPANAS = {
    seca: [[784, 988, 1175], 0.16, 0.7],              // sol-si-re: «tu ropa está lista», suave y tranquilo
    disponible: [[1047, 1568], 0.08, 0.3],           // do-sol agudos y cortitos: un «ding-ding» suave, «de vuelta al armario»
    puesto: [[784, 988, 1175, 1568], 0.07, 0.45],    // arpegio que sube: «¡me lo pongo!»
  };

  const SONIDOS = {
    leido: [[1320, 0.09]],                 // pitido agudo corto: «leído»
    error: [[520, 0.12], [370, 0.18]],     // dos tonos que bajan: «algo ha fallado»
    guardado: [[880, 0.07], [1320, 0.1]],  // dos tonos que suben: «guardado»
    listo: [[660, 0.08], [990, 0.12]],
  };
  const VIBRACIONES = { leido: 40, error: [60, 80, 60], guardado: 25, listo: 20, seca: [30, 60, 30], disponible: [15, 40, 15, 40, 25], puesto: [20, 40, 20, 40, 60], tic: 6 };

  // «Tic» muy suave y corto, como la rueda de un selector, al pasar de prenda en el probador
  function clic() {
    despertar();
    if (!ctx) return;
    const t = ctx.currentTime, o = ctx.createOscillator(), g = ctx.createGain();
    o.type = 'triangle'; o.frequency.value = 1900;
    g.gain.setValueAtTime(0.0001, t);
    g.gain.exponentialRampToValueAtTime(0.06, t + 0.004);
    g.gain.exponentialRampToValueAtTime(0.0001, t + 0.035);
    o.connect(g).connect(ctx.destination);
    o.start(t); o.stop(t + 0.05);
  }

  // Reto 1: confirmación por sonido y vibración
  function aviso(tipo, conSonido, conVibracion) {
    if (tipo === 'tic') { if (conSonido) clic(); }
    else if (conSonido && CAMPANAS[tipo]) campana.apply(null, CAMPANAS[tipo]);
    else if (conSonido && SONIDOS[tipo]) tonos(SONIDOS[tipo]);
    // La vibración funciona en Android. Safari en iPhone no deja vibrar a las páginas web.
    if (conVibracion && navigator.vibrate) { try { navigator.vibrate(VIBRACIONES[tipo] || 30); } catch (e) { } }
  }

  // ---------- Lector de QR ----------
  let stream = null, timer = null;

  function decodificar(canvas, ctx2d) {
    if (!window.jsQR) return null;
    const img = ctx2d.getImageData(0, 0, canvas.width, canvas.height);
    const r = window.jsQR(img.data, canvas.width, canvas.height, { inversionAttempts: 'attemptBoth' });
    return r ? r.data : null;
  }

  // La cámara en directo solo funciona en páginas seguras (https o localhost)
  function camaraDisponible() {
    return !!(window.isSecureContext && navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
  }

  async function iniciar(idVideo, dotnet) {
    parar();
    const video = document.getElementById(idVideo);
    if (!video) return 'sin-video';
    try {
      stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' }, audio: false });
    } catch (e) {
      return 'sin-permiso';
    }
    video.srcObject = stream;
    video.setAttribute('playsinline', '');
    video.setAttribute('muted', '');
    video.muted = true;
    await video.play().catch(function () { });
    // En algunas apps añadidas a la pantalla de inicio (sobre todo en iPhone) la cámara «arranca» pero no da imagen.
    // Si en 4 segundos no llega ningún fotograma, se apaga y la app ofrece hacer una foto a la etiqueta.
    const conImagen = await new Promise(function (ok) {
      if (video.videoWidth > 0) return ok(true);
      const t = setTimeout(function () { ok(video.videoWidth > 0); }, 4000);
      video.addEventListener('loadeddata', function () { clearTimeout(t); ok(true); }, { once: true });
    });
    if (!conImagen) { parar(); return 'sin-imagen'; }
    const canvas = document.createElement('canvas');
    const c2 = canvas.getContext('2d', { willReadFrequently: true });
    timer = setInterval(function () {
      if (!video.videoWidth) return;
      const k = Math.min(1, 640 / video.videoWidth);
      canvas.width = Math.round(video.videoWidth * k);
      canvas.height = Math.round(video.videoHeight * k);
      c2.drawImage(video, 0, 0, canvas.width, canvas.height);
      const texto = decodificar(canvas, c2);
      if (texto) {
        parar();
        dotnet.invokeMethodAsync('QrLeido', texto);
      }
    }, 200);
    return 'ok';
  }

  function parar() {
    if (timer) { clearInterval(timer); timer = null; }
    if (stream) { stream.getTracks().forEach(function (t) { t.stop(); }); stream = null; }
  }

  // Lee el QR de una foto (para cuando no hay cámara en directo)
  function leerImagen(dataUrl) {
    return new Promise(function (resolve) {
      const im = new Image();
      im.onload = function () {
        const k = Math.min(1, 1400 / Math.max(im.width, im.height));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(im.width * k); canvas.height = Math.round(im.height * k);
        const c2 = canvas.getContext('2d', { willReadFrequently: true });
        c2.drawImage(im, 0, 0, canvas.width, canvas.height);
        resolve(decodificar(canvas, c2));
      };
      im.onerror = function () { resolve(null); };
      im.src = dataUrl;
    });
  }

  // Reduce una foto (data URL) a un lado máximo y la pasa a JPEG, para que quepa en el navegador.
  // Se pinta sobre fondo blanco por si la foto de la tienda es PNG con transparencia.
  function reducirImagen(dataUrl, lado) {
    return new Promise(function (resolve) {
      const im = new Image();
      im.onload = function () {
        const k = Math.min(1, (lado || 480) / Math.max(im.width, im.height));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(im.width * k); canvas.height = Math.round(im.height * k);
        const c2 = canvas.getContext('2d');
        c2.fillStyle = '#fff'; c2.fillRect(0, 0, canvas.width, canvas.height);
        c2.drawImage(im, 0, 0, canvas.width, canvas.height);
        try { resolve(canvas.toDataURL('image/jpeg', 0.85)); } catch (e) { resolve(null); }
      };
      im.onerror = function () { resolve(null); };
      im.src = dataUrl;
    });
  }

  // ---------- Notificaciones del navegador (la ropa ya está seca) ----------
  // Funcionan con la web abierta (aunque esté en otra pestaña) en ordenador y Android.
  // En iPhone solo si la web se añade a la pantalla de inicio (iOS 16.4 o posterior).
  function pedirPermiso() {
    try {
      if ('Notification' in window && Notification.permission === 'default') Notification.requestPermission();
    } catch (e) { }
  }
  function notificar(titulo, texto) {
    try {
      if (!('Notification' in window) || Notification.permission !== 'granted') return false;
      const n = new Notification(titulo, { body: texto, icon: 'icono.png', tag: 'cora-lavadora' });
      n.onclick = function () {
        window.focus();
        if (window.Blazor && Blazor.navigateTo) Blazor.navigateTo('categoria/lavadora');
        n.close();
      };
      return true;
    } catch (e) { return false; }
  }

  // ---------- Menos movimiento (ajuste del móvil) ----------
  function quieto() { try { return matchMedia('(prefers-reduced-motion: reduce)').matches; } catch (e) { return false; } }

  // ---------- Confeti al decir «Me lo pongo» ----------
  // Un lienzo encima de la app, 1,6 segundos, con los colores de Cora. Con «Reducir movimiento» no sale.
  function confeti() {
    if (quieto()) return;
    const marco = document.querySelector('.movil') || document.body;
    const r = marco.getBoundingClientRect();
    const c = document.createElement('canvas');
    const k = window.devicePixelRatio || 1;
    c.className = 'confeti'; c.width = r.width * k; c.height = r.height * k;
    marco.appendChild(c);
    const g = c.getContext('2d'); g.scale(k, k);
    const colores = ['#8F3F5E', '#D795AB', '#F3DBE3', '#E8C33A', '#2F7A52', '#F27B63', '#FFFFFF'];
    const piezas = Array.from({ length: 90 }, function () {
      return {
        x: r.width / 2 + (Math.random() - .5) * 60, y: r.height * .42,
        vx: (Math.random() - .5) * 9, vy: -Math.random() * 10 - 4,
        w: 5 + Math.random() * 6, h: 8 + Math.random() * 8, giro: Math.random() * 6, vg: (Math.random() - .5) * .35,
        color: colores[(Math.random() * colores.length) | 0],
      };
    });
    const inicio = performance.now();
    (function paso(ahora) {
      const t = ahora - inicio;
      g.clearRect(0, 0, r.width, r.height);
      g.globalAlpha = Math.max(0, 1 - Math.max(0, t - 1000) / 600);
      piezas.forEach(function (p) {
        p.vy += .32; p.vx *= .99; p.x += p.vx; p.y += p.vy; p.giro += p.vg;
        g.save(); g.translate(p.x, p.y); g.rotate(p.giro); g.fillStyle = p.color;
        g.fillRect(-p.w / 2, -p.h / 2, p.w, p.h * Math.abs(Math.cos(p.giro * 1.7)) + 1); g.restore();
      });
      if (t < 1600) requestAnimationFrame(paso); else c.remove();
    })(inicio);
  }

  // ---------- Deslizar la tarjeta del outfit de hoy ----------
  // Derecha → «Me lo pongo»; izquierda → «Otra opción». La tarjeta sigue al dedo, se inclina y enseña un sello.
  // El scroll vertical sigue funcionando (touch-action: pan-y en el CSS). Los botones hacen lo mismo, para quien no deslice.
  let deslizarRef = null;
  const UMBRAL = 90;
  function registrarDeslizar(ref) { deslizarRef = ref; }
  function olvidarDeslizar() { deslizarRef = null; }

  document.addEventListener('pointerdown', function (e) {
    const carta = e.target.closest && e.target.closest('[data-deslizar]');
    if (!carta || !deslizarRef || e.button > 0) return;
    const izquierda = carta.getAttribute('data-deslizar') !== 'derecha';
    const x0 = e.clientX, y0 = e.clientY, id = e.pointerId;
    let dx = 0, arrastrando = false;

    function mover(ev) {
      if (ev.pointerId !== id) return;
      dx = ev.clientX - x0;
      if (!izquierda && dx < 0) dx = dx / 4; // sin «Otra opción» (modo fácil): solo cede un poquito
      if (!arrastrando && Math.abs(dx) > 8 && Math.abs(dx) > Math.abs(ev.clientY - y0)) {
        arrastrando = true;
        carta.classList.add('arrastrando');
        try { carta.setPointerCapture(id); } catch (x) { }
      }
      if (!arrastrando) return;
      carta.style.transform = 'translateX(' + dx + 'px) rotate(' + (dx / 22) + 'deg)';
      carta.style.setProperty('--si', Math.max(0, Math.min(1, dx / UMBRAL)));
      carta.style.setProperty('--no', izquierda ? Math.max(0, Math.min(1, -dx / UMBRAL)) : 0);
    }
    function soltar(ev) {
      if (ev.pointerId !== id) return;
      document.removeEventListener('pointermove', mover);
      document.removeEventListener('pointerup', soltar);
      document.removeEventListener('pointercancel', soltar);
      carta.classList.remove('arrastrando');
      if (!arrastrando) return;
      // Que el toque de soltar no cuente como clic dentro de la tarjeta
      carta.addEventListener('click', function (c) { c.stopPropagation(); c.preventDefault(); }, { capture: true, once: true });
      const ref = deslizarRef;
      if (ev.type !== 'pointercancel' && dx > UMBRAL && ref) {
        // «Me lo pongo»: vuelve a su sitio con un saltito y Cora lo apunta
        carta.style.transform = ''; carta.style.setProperty('--si', 0);
        carta.classList.add('aprobada');
        setTimeout(function () { carta.classList.remove('aprobada'); }, 600);
        ref.invokeMethodAsync('Deslizado', 'derecha');
      } else if (ev.type !== 'pointercancel' && izquierda && dx < -UMBRAL && ref) {
        // «Otra opción»: sale volando hacia la izquierda y llega otra
        carta.classList.add('fuera');
        carta.style.transform = 'translateX(-130%) rotate(-16deg)';
        setTimeout(function () { ref.invokeMethodAsync('Deslizado', 'izquierda'); }, 220);
        setTimeout(function () { carta.classList.remove('fuera'); carta.style.transform = ''; carta.style.setProperty('--no', 0); }, 1500);
      } else {
        carta.style.transform = ''; carta.style.setProperty('--si', 0); carta.style.setProperty('--no', 0);
      }
    }
    document.addEventListener('pointermove', mover);
    document.addEventListener('pointerup', soltar);
    document.addEventListener('pointercancel', soltar);
  });

  // ---------- Carruseles del probador ----------
  // Cada fila se desliza de lado y se para en una prenda; la del centro es la elegida y se avisa a C#.
  function centro(fila) {
    const r = fila.getBoundingClientRect(), mitad = r.left + r.width / 2;
    let mejor = 0, dist = Infinity;
    Array.prototype.forEach.call(fila.children, function (h, i) {
      const b = h.getBoundingClientRect(), d = Math.abs(b.left + b.width / 2 - mitad);
      if (d < dist) { dist = d; mejor = i; }
    });
    return mejor;
  }
  function observarCarrusel(idFila, ref) {
    const fila = document.getElementById(idFila);
    if (!fila || fila._cora) return;
    let espera = null, ultimo = centro(fila);
    fila._cora = true;
    fila.addEventListener('scroll', function () {
      // Mientras se desliza, en cuanto otra prenda llega al centro, se avisa (así la opinión cambia en directo)
      const i = centro(fila);
      if (i !== ultimo) { ultimo = i; ref.invokeMethodAsync('Elegir', idFila, i); }
      clearTimeout(espera);
      espera = setTimeout(function () { const j = centro(fila); if (j !== ultimo) { ultimo = j; ref.invokeMethodAsync('Elegir', idFila, j); } }, 140);
    }, { passive: true });
  }
  function irCarrusel(idFila, i, suave) {
    const fila = document.getElementById(idFila);
    const h = fila && fila.children[i];
    if (!h) return;
    // Se usa el centro de la prenda (no cambia aunque esté encogida con scale)
    const rf = fila.getBoundingClientRect(), rh = h.getBoundingClientRect();
    const izquierda = fila.scrollLeft + (rh.left + rh.width / 2) - (rf.left + rf.width / 2);
    fila.scrollTo({ left: izquierda, behavior: suave && !quieto() ? 'smooth' : 'auto' });
  }

  // ---------- Dictar los planes con la voz ----------
  // Usa el reconocimiento de voz del navegador (Chrome, Edge, Safari en iPhone). Si no lo hay, el botón no sale.
  let oyente = null;
  function vozDisponible() { return !!(window.SpeechRecognition || window.webkitSpeechRecognition); }
  function escuchar(idioma, ref) {
    pararVoz();
    const R = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!R) { ref.invokeMethodAsync('VozFin', 'sin_voz'); return false; }
    const r = new R();
    r.lang = idioma === 'en' ? 'en-US' : idioma === 'fr' ? 'fr-FR' : 'es-ES';
    r.interimResults = true; r.continuous = false; r.maxAlternatives = 1;
    let fallo = null;
    r.onresult = function (e) {
      let texto = '', final = false;
      for (let k = 0; k < e.results.length; k++) { texto += e.results[k][0].transcript; if (e.results[k].isFinal) final = true; }
      ref.invokeMethodAsync('VozTexto', texto, final);
    };
    r.onerror = function (e) { fallo = e.error || 'error'; };
    r.onend = function () { if (oyente === r) oyente = null; ref.invokeMethodAsync('VozFin', fallo); };
    oyente = r;
    try { r.start(); } catch (e) { oyente = null; ref.invokeMethodAsync('VozFin', 'error'); return false; }
    return true;
  }
  function pararVoz() { if (oyente) { try { oyente.stop(); } catch (e) { } } }

  // ---------- Onda al tocar (botones, tarjetas, cuadrados) ----------
  // Un círculo suave sale desde el dedo: se nota que la app responde a cada toque.
  document.addEventListener('pointerdown', function (e) {
    const el = e.target.closest && e.target.closest('.btn, .chip, .cuadro, .look-elegir, .fila-lista, .tira-dias button, .segmentos button');
    if (!el || el.disabled || quieto()) return;
    const r = el.getBoundingClientRect(), lado = Math.max(r.width, r.height) * 2.2;
    const o = document.createElement('span');
    o.className = 'onda';
    o.style.width = o.style.height = lado + 'px';
    o.style.left = (e.clientX - r.left - lado / 2) + 'px';
    o.style.top = (e.clientY - r.top - lado / 2) + 'px';
    el.appendChild(o);
    setTimeout(function () { o.remove(); }, 650);
  }, { passive: true });

  // ---------- Números que cuentan hacia arriba (Tu armario en números) ----------
  function contar() {
    document.querySelectorAll('[data-contar]:not([data-contado])').forEach(function (el) {
      el.setAttribute('data-contado', '');
      const fin = parseInt(el.getAttribute('data-contar'), 10) || 0, extra = el.getAttribute('data-sufijo') || '';
      if (quieto() || fin === 0) { el.textContent = fin + extra; return; }
      const t0 = performance.now(), dur = 900;
      (function paso(t) {
        const k = Math.min(1, (t - t0) / dur), v = Math.round(fin * (1 - Math.pow(1 - k, 3)));
        el.textContent = v + extra;
        if (k < 1) requestAnimationFrame(paso);
      })(t0);
    });
  }

  // ---------- Agitar el móvil en el probador = «Sorpréndeme» ----------
  // En iPhone hay que pedir permiso al tocar un botón (lo hace el primer toque en «Sorpréndeme»).
  let agitarRef = null, ultimoAgite = 0;
  function alMoverse(e) {
    const a = e.accelerationIncludingGravity || e.acceleration;
    if (!a || !agitarRef) return;
    const fuerza = Math.sqrt((a.x || 0) ** 2 + (a.y || 0) ** 2 + (a.z || 0) ** 2);
    const ahora = Date.now();
    if (fuerza > 24 && ahora - ultimoAgite > 1500) { ultimoAgite = ahora; agitarRef.invokeMethodAsync('Agitado'); }
  }
  function activarAgitar(ref, idBoton) {
    agitarRef = ref;
    const pedir = window.DeviceMotionEvent && DeviceMotionEvent.requestPermission;
    if (pedir) {
      const b = document.getElementById(idBoton);
      if (b) b.addEventListener('click', function () {
        DeviceMotionEvent.requestPermission().then(function (r) { if (r === 'granted') window.addEventListener('devicemotion', alMoverse); }).catch(function () { });
      }, { once: true });
    } else if (window.DeviceMotionEvent) {
      window.addEventListener('devicemotion', alMoverse);
    }
    return ('ontouchstart' in window) && !!window.DeviceMotionEvent;
  }
  function pararAgitar() { agitarRef = null; window.removeEventListener('devicemotion', alMoverse); }

  return { confeti: confeti, quieto: quieto, contar: contar,
    agitar: { activar: activarAgitar, parar: pararAgitar },
    deslizar: { registrar: registrarDeslizar, olvidar: olvidarDeslizar },
    carrusel: { observar: observarCarrusel, ir: irCarrusel },
    voz: { disponible: vozDisponible, escuchar: escuchar, parar: pararVoz },
    leer: leer, guardar: guardar, aviso: aviso, pedirPermiso: pedirPermiso, notificar: notificar, qr: { camaraDisponible: camaraDisponible, iniciar: iniciar, parar: parar, leerImagen: leerImagen, reducirImagen: reducirImagen } };
})();
