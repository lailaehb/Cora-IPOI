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
  };

  const SONIDOS = {
    leido: [[1320, 0.09]],                 // pitido agudo corto: «leído»
    error: [[520, 0.12], [370, 0.18]],     // dos tonos que bajan: «algo ha fallado»
    guardado: [[880, 0.07], [1320, 0.1]],  // dos tonos que suben: «guardado»
    listo: [[660, 0.08], [990, 0.12]],
  };
  const VIBRACIONES = { leido: 40, error: [60, 80, 60], guardado: 25, listo: 20, seca: [30, 60, 30], disponible: [15, 40, 15, 40, 25] };

  // Reto 1: confirmación por sonido y vibración
  function aviso(tipo, conSonido, conVibracion) {
    if (conSonido && CAMPANAS[tipo]) campana.apply(null, CAMPANAS[tipo]);
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

  return { leer: leer, guardar: guardar, aviso: aviso, pedirPermiso: pedirPermiso, notificar: notificar, qr: { camaraDisponible: camaraDisponible, iniciar: iniciar, parar: parar, leerImagen: leerImagen, reducirImagen: reducirImagen } };
})();
