// Funciones del navegador que usa la app desde C# (con IJSRuntime).
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

  const SONIDOS = {
    leido: [[1320, 0.09]],                 // pitido agudo corto: «leído»
    error: [[520, 0.12], [370, 0.18]],     // dos tonos que bajan: «algo ha fallado»
    guardado: [[880, 0.07], [1320, 0.1]],  // dos tonos que suben: «guardado»
    listo: [[660, 0.08], [990, 0.12]],
  };
  const VIBRACIONES = { leido: 40, error: [60, 80, 60], guardado: 25, listo: 20 };

  // Reto 1: confirmación por sonido y vibración
  function aviso(tipo, conSonido, conVibracion) {
    if (conSonido && SONIDOS[tipo]) tonos(SONIDOS[tipo]);
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
    await video.play().catch(function () { });
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

  return { leer: leer, guardar: guardar, aviso: aviso, qr: { camaraDisponible: camaraDisponible, iniciar: iniciar, parar: parar, leerImagen: leerImagen } };
})();
