using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cora.Modelos;
using Microsoft.JSInterop;

namespace Cora.Servicios;

/// <summary>
/// Estado de toda la app: armario, planes, outfits, guardados, perfil y ajustes.
/// Se guarda en el navegador (localStorage), así no se pierde al cerrar la página.
/// También gestiona la espera del planificador (reto 3) y los avisos de sonido y vibración (reto 1).
/// </summary>
public class Estado
{
    const string Clave = "cora-datos-v1";
    public const int DiasLavado = 2;
    static readonly string[] Mensajes = { "Mirando el tiempo de la semana…", "Descartando lo que está en la lavadora…", "Pidiendo ideas a la IA…", "Combinando tus prendas…" };
    const int DuracionEspera = 3600;
    /// <summary>Mensajes mientras se busca «Otra opción» para un día.</summary>
    static readonly string[] MensajesOtra = { "Buscando otra combinación…", "Probando con otras prendas…", "Teniendo en cuenta el tiempo…", "Casi lo tengo…" };
    const int DuracionOtra = 2400;

    private readonly IJSRuntime _js;
    private readonly ServicioIA _ia;
    public ServicioTiempo Tiempo { get; }

    public Estado(IJSRuntime js, ServicioTiempo tiempo, ServicioIA ia)
    {
        _js = js;
        Tiempo = tiempo;
        _ia = ia;
    }

    public Datos D { get; private set; } = new();
    public bool Listo { get; private set; }

    /// <summary>Se lanza cada vez que cambia algo, para que las pantallas se vuelvan a pintar.</summary>
    public event Action? Cambio;
    void Avisar() => Cambio?.Invoke();

    // ---------------------------------------------------------------- carga y guardado
    public async Task CargarAsync()
    {
        if (Listo) return;
        try
        {
            var json = await _js.InvokeAsync<string?>("cora.leer", Clave);
            if (!string.IsNullOrEmpty(json)) D = JsonSerializer.Deserialize(json, CoraJson.Default.Datos) ?? Iniciales();
            else D = Iniciales();
        }
        catch
        {
            D = Iniciales();
        }
        // Armarios guardados antes de la segunda tanda de fotos: se les añaden las prendas nuevas una vez
        if (D.VersionEjemplos < DatosEjemplo.VersionEjemplos)
        {
            foreach (var p in DatosEjemplo.Nuevas().Where(n => D.Prendas.All(x => x.Id != n.Id))) D.Prendas.Add(p);
            D.VersionEjemplos = DatosEjemplo.VersionEjemplos;
            await GuardarAsync();
        }
        PrepararLavadas();
        Listo = true;
        Avisar();
        _ = CargarTiempoAsync();
        _ = BucleLavadoraAsync();
    }

    public async Task GuardarAsync()
    {
        try { await _js.InvokeVoidAsync("cora.guardar", Clave, JsonSerializer.Serialize(D, CoraJson.Default.Datos)); } catch { }
    }

    async Task CambiadoAsync()
    {
        Avisar();
        await GuardarAsync();
    }

    static Datos Iniciales()
    {
        string hoy = Fechas.Hoy;
        var d = new Datos
        {
            Prendas = DatosEjemplo.Armario(),
            Favoritas = new() { "vaquero_azul" },
        };
        // La sudadera de ejemplo lleva 20 horas en la lavadora, para que se vea la barra a medias
        // Y el pantalón de chándal está en el cesto, esperando a la próxima lavadora
        foreach (var p in d.Prendas.Where(p => p.Id == "chandal_gris")) p.EnCesto = true;
        foreach (var p in d.Prendas.Where(p => p.Lavando))
        {
            var inicio = DateTime.UtcNow.AddHours(-20);
            p.LavadoHora = inicio.ToString("o", CultureInfo.InvariantCulture);
            p.LavadoDesde = Fechas.Iso(inicio.ToLocalTime());
        }
        // Hoy empieza sin planes: Cora pregunta en Inicio qué vas a hacer (no se inventa un outfit)
        d.Planes[Fechas.Sumar(hoy, 3)] = new PlanDia { Claves = { "cena" } };
        d.Planes[Fechas.Sumar(hoy, 5)] = new PlanDia { Claves = { "fiesta" } };
        // Historial de ejemplo de los últimos días, para que se note la regla de no repetir
        string[] pasados = { "uni", "trabajo", "casa", "uni", "cena", "uni" };
        for (int k = -6; k < 0; k++)
        {
            string dia = Fechas.Sumar(hoy, k);
            d.Planes[dia] = new PlanDia { Claves = { pasados[k + 6] } };
            d.Outfits[dia] = MotorIA.Recomendar(dia, d.Prendas, d.Planes[dia], ServicioTiempo.DeEjemplo(dia), d.Outfits, d.Perfil.Gustos);
        }
        return d;
    }

    // ---------------------------------------------------------------- lavadora en tiempo real
    // La ropa tarda DiasLavado días (48 h) en lavarse y secarse. La barra avanza con las horas reales y,
    // cuando se cumple el tiempo, Cora avisa (aviso en la app, sonido y notificación del navegador).
    // La prenda NO vuelve sola al armario: el usuario la marca como disponible cuando la tiene (control explícito).
    /// <summary>Lo que el usuario eligió al poner la lavadora (de 2 a 14 días; por defecto 2).</summary>
    public static TimeSpan DuracionDe(Prenda p) => TimeSpan.FromDays(Math.Clamp(p.LavadoDias ?? DiasLavado, DiasLavado, 14));

    public static DateTime InicioLavado(Prenda p)
    {
        if (p.LavadoHora != null && DateTime.TryParse(p.LavadoHora, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t))
            return t.ToUniversalTime();
        if (p.LavadoDesde != null) return Fechas.De(p.LavadoDesde).ToUniversalTime();
        return DateTime.UtcNow;
    }

    /// <summary>De 0 (recién echada a lavar) a 1 (seca).</summary>
    public static double FraccionLavado(Prenda p) =>
        Math.Clamp((DateTime.UtcNow - InicioLavado(p)).TotalMinutes / DuracionDe(p).TotalMinutes, 0, 1);

    public static bool Seca(Prenda p) => p.Lavando && FraccionLavado(p) >= 1;

    public static TimeSpan Falta(Prenda p)
    {
        var f = InicioLavado(p) + DuracionDe(p) - DateTime.UtcNow;
        return f < TimeSpan.Zero ? TimeSpan.Zero : f;
    }

    /// <summary>Fecha y hora (locales) en que estará seca.</summary>
    public static DateTime FinLavado(Prenda p) => (InicioLavado(p) + DuracionDe(p)).ToLocalTime();

    /// <summary>Ancho de la barra para el CSS, p. ej. "41.67%".</summary>
    public static string AnchoLavado(Prenda p) => (FraccionLavado(p) * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

    /// <summary>«1 día y 5 h», «5 h y 20 min» o «20 min».</summary>
    public static string TextoFalta(TimeSpan t)
    {
        int dias = (int)t.TotalDays;
        if (dias >= 1) return $"{dias} {(dias == 1 ? "día" : "días")} y {t.Hours} h";
        if (t.TotalHours >= 1) return $"{t.Hours} h y {t.Minutes} min";
        return $"{Math.Max(1, t.Minutes)} min";
    }

    public List<Prenda> Secas => D.Prendas.Where(Seca).ToList();

    /// <summary>Datos guardados con versiones anteriores: calcula la hora de inicio a partir del día.</summary>
    void PrepararLavadas()
    {
        foreach (var p in D.Prendas.Where(p => p.Lavando && p.LavadoHora == null))
            p.LavadoHora = InicioLavado(p).ToString("o", CultureInfo.InvariantCulture);
    }

    bool _bucleLavadora;

    /// <summary>Cada 30 segundos: mueve las barras y avisa de las prendas que ya están secas.</summary>
    async Task BucleLavadoraAsync()
    {
        if (_bucleLavadora) return;
        _bucleLavadora = true;
        while (true)
        {
            await ComprobarLavadoraAsync();
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    async Task ComprobarLavadoraAsync()
    {
        await RecordarLavadoraAsync();
        if (!D.Prendas.Any(p => p.Lavando)) return;
        var nuevas = D.Prendas.Where(p => p.Lavando && !p.AvisoSeca && Seca(p)).ToList();
        if (nuevas.Count > 0)
        {
            foreach (var p in nuevas) p.AvisoSeca = true;
            string titulo = nuevas.Count == 1 ? $"{nuevas[0].Nombre} ya está seca" : $"{nuevas.Count} prendas ya están secas";
            string texto = nuevas.Count == 1
                ? "Márcala como disponible en Cora para que vuelva a tu armario."
                : $"{string.Join(", ", nuevas.Select(p => p.Nombre))}. Márcalas como disponibles en Cora.";
            await SonarAsync("seca");
            MostrarAviso(titulo);
            try { await _js.InvokeVoidAsync("cora.notificar", titulo, texto); } catch { }
            await GuardarAsync();
        }
        Avisar(); // para que las barras avancen
    }

    // ---------------------------------------------------------------- cesto y programación de la lavadora
    /// <summary>Ropa sucia esperando a que se ponga la lavadora.</summary>
    public List<Prenda> EnCesto => D.Prendas.Where(p => p.EnCesto && !p.Lavando).ToList();

    /// <summary>Según la programación que ha elegido el usuario en Configuración.</summary>
    public bool TocaLavadoraHoy
    {
        get
        {
            var a = D.Ajustes;
            if (a.Programa == "semana") return (int)DateTime.Today.DayOfWeek == a.DiaSemana;
            if (a.Programa == "cada") return a.UltimaLavadora == null || -Fechas.Dif(a.UltimaLavadora) >= a.CadaDias;
            return false;
        }
    }

    /// <summary>«Sueles poner la lavadora los sábados», «Pones la lavadora cada 7 días · la próxima, el jueves 26» o "".</summary>
    public string TextoPrograma
    {
        get
        {
            var a = D.Ajustes;
            if (a.Programa == "semana")
            {
                string dia = Fechas.Dias[Math.Clamp(a.DiaSemana, 0, 6)];
                return $"Sueles poner la lavadora los {(dia.EndsWith("s") ? dia : dia + "s")}";
            }
            if (a.Programa == "cada")
            {
                string t = $"Pones la lavadora cada {a.CadaDias} días";
                if (a.UltimaLavadora != null)
                {
                    string prox = Fechas.Sumar(a.UltimaLavadora, a.CadaDias);
                    t += Fechas.Dif(prox) <= 0 ? " · toca hoy" : $" · la próxima, {Fechas.Etiqueta(prox).ToLower()}";
                }
                return t;
            }
            return "";
        }
    }

    public async Task EcharAlCestoAsync(IEnumerable<string> ids)
    {
        var lista = ids.ToList();
        foreach (var p in D.Prendas.Where(p => lista.Contains(p.Id) && !p.Lavando)) p.EnCesto = true;
        await SonarAsync("guardado");
        await CambiadoAsync();
    }

    public async Task SacarDelCestoAsync(IEnumerable<string> ids)
    {
        var lista = ids.ToList();
        foreach (var p in D.Prendas.Where(p => lista.Contains(p.Id))) p.EnCesto = false;
        await SonarAsync("disponible");
        await CambiadoAsync();
    }

    /// <summary>Todo lo del cesto va a la vez a la lavadora; estará lista en los días que elija el usuario.</summary>
    public async Task PonerLavadoraAsync(int dias)
    {
        // Se pide permiso para las notificaciones justo al pulsar (el navegador solo deja pedirlo tras un toque)
        try { await _js.InvokeVoidAsync("cora.pedirPermiso"); } catch { }
        dias = Math.Clamp(dias, DiasLavado, 14);
        string ahora = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        foreach (var p in EnCesto)
        {
            p.EnCesto = false;
            p.Lavando = true;
            p.LavadoHora = ahora;
            p.LavadoDesde = Fechas.Hoy;
            p.LavadoDias = dias;
            p.AvisoSeca = false;
        }
        D.Ajustes.UltimaLavadora = Fechas.Hoy;
        await SonarAsync("guardado");
        await CambiadoAsync();
    }

    /// <summary>Si hoy toca lavadora (según la programación) y hay ropa en el cesto, avisa una vez al día.</summary>
    async Task RecordarLavadoraAsync()
    {
        var cesto = EnCesto;
        if (!TocaLavadoraHoy || cesto.Count == 0 || D.Ajustes.UltimoRecordatorio == Fechas.Hoy) return;
        D.Ajustes.UltimoRecordatorio = Fechas.Hoy;
        string titulo = "Hoy toca poner la lavadora";
        string texto = $"Tienes {cesto.Count} {(cesto.Count == 1 ? "prenda" : "prendas")} en el cesto. Entra en Cora para poner la lavadora.";
        await SonarAsync("seca");
        MostrarAviso(titulo);
        try { await _js.InvokeVoidAsync("cora.notificar", titulo, texto); } catch { }
        await GuardarAsync();
    }

    /// <summary>Solo para probar: hace como si hubieran pasado unas horas en la lavadora.</summary>
    public async Task AdelantarLavadoraAsync(int horas)
    {
        try { await _js.InvokeVoidAsync("cora.pedirPermiso"); } catch { }
        foreach (var p in D.Prendas.Where(p => p.Lavando))
        {
            var inicio = InicioLavado(p).AddHours(-horas);
            p.LavadoHora = inicio.ToString("o", CultureInfo.InvariantCulture);
            p.LavadoDesde = Fechas.Iso(inicio.ToLocalTime());
        }
        await ComprobarLavadoraAsync();
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- tiempo
    public async Task CargarTiempoAsync()
    {
        if (await Tiempo.CargarAsync(D.Perfil.Ciudad))
        {
            // Se recalculan los outfits de hoy en adelante que se hicieron con otro tiempo
            // (los de la IA se respetan: los ha elegido con los planes y el usuario los ha visto)
            for (int i = 0; i < 16; i++)
            {
                string dia = Fechas.Sumar(Fechas.Hoy, i);
                var w = Tiempo.De(dia);
                if (D.Outfits.TryGetValue(dia, out var o) && !o.DeIA && !o.Fijo && !o.Puesto && !o.Porque.StartsWith($"{w.T}° y {w.Cielo}"))
                    D.Outfits[dia] = Recomendar(dia, o.Semilla);
            }
            await GuardarAsync();
        }
        Avisar();
    }

    // ---------------------------------------------------------------- consultas
    public Prenda? PrendaPorId(string id) => D.Prendas.FirstOrDefault(p => p.Id == id);
    public PlanResuelto? PlanDe(string dia) => D.Planes.TryGetValue(dia, out var p) ? MotorIA.Resolver(p) : null;
    public Outfit? OutfitDe(string dia) => D.Outfits.TryGetValue(dia, out var o) ? o : null;
    public bool EsFavorita(string id) => D.Favoritas.Contains(id);
    public bool DiaGuardado(string dia) => D.Guardados.Any(g => g.Fecha == dia);
    public List<Prenda> EnLavadora => D.Prendas.Where(p => p.Lavando).ToList();

    Outfit Recomendar(string dia, int? inicio = null, string? evitar = null, Dictionary<string, Outfit>? outfits = null) =>
        MotorIA.Recomendar(dia, D.Prendas, D.Planes.GetValueOrDefault(dia), Tiempo.De(dia), outfits ?? D.Outfits, D.Perfil.Gustos, inicio, evitar);

    // ---------------------------------------------------------------- IA (Gemini) con las reglas de respaldo
    /// <summary>Lo que se le manda a la IA: los días con sus planes y tiempo, el armario real y lo ya usado esa semana.</summary>
    PeticionSemana Peticion(List<string> dias, Dictionary<string, List<string>>? evitar = null)
    {
        var p = new PeticionSemana { Gustos = new(D.Perfil.Gustos), Estilos = new(D.Perfil.Estilos), Idioma = D.Ajustes.Idioma };
        foreach (var d in dias)
        {
            var plan = D.Planes.GetValueOrDefault(d);
            var w = Tiempo.De(d);
            p.Dias.Add(new DiaPeticion
            {
                Fecha = d,
                Dia = Fechas.Etiqueta(d),
                Temperatura = w.T,
                Cielo = w.Cielo,
                Planes = plan == null ? new() : plan.Claves
                    .Select(c => DatosEjemplo.Planes.FirstOrDefault(x => x.Clave == c)?.Nombre)
                    .Where(n => n != null).Select(n => n!).ToList(),
                Texto = plan?.Texto ?? "",
                Evitar = evitar != null && evitar.TryGetValue(d, out var ev) ? ev : new(),
            });
        }
        // Solo lo que no está en la lavadora: la IA no puede elegir lo que no está disponible
        p.Armario = D.Prendas.Where(x => !x.NoDisponible).Select(x => new PrendaIA
        {
            Id = x.Id, Nombre = x.Nombre, Categoria = x.Cat.ToString(), Tipo = x.Sub, Color = x.Color,
            Usos = ServicioIA.UsosDe(x), Abrigo = x.Abrigo, Lluvia = x.AptaLluvia,
        }).ToList();
        // Conjuntos de los días cercanos que no se piden ahora, para no repetir en 7 días
        var difs = dias.Select(Fechas.Dif).ToList();
        foreach (var (fecha, o) in D.Outfits)
        {
            if (dias.Contains(fecha) || o.Ids.Count == 0) continue;
            int df = Fechas.Dif(fecha);
            if (difs.Any(x => Math.Abs(x - df) <= 6)) p.Usadas.Add(new UsadoIA { Fecha = fecha, Ids = new(o.Ids) });
        }
        return p;
    }

    static readonly Categoria[] OrdenLook = { Categoria.Capa, Categoria.Arriba, Categoria.Abajo, Categoria.Calzado };

    /// <summary>Convierte la respuesta de la IA en un Outfit, comprobando que las prendas existen y no están en la lavadora.</summary>
    Outfit? OutfitDesdeIA(DiaRespuesta r)
    {
        var looks = new List<(Look Look, bool Deporte)>();
        foreach (var l in r.Looks)
        {
            var prendas = l.Ids.Distinct().Select(PrendaPorId).Where(p => p != null && !p.NoDisponible).Select(p => p!).ToList();
            // Un conjunto válido necesita, como mínimo, algo de arriba y algo de abajo
            if (!prendas.Any(p => p.Cat == Categoria.Arriba) || !prendas.Any(p => p.Cat == Categoria.Abajo)) continue;
            var ids = OrdenLook.Select(c => prendas.FirstOrDefault(p => p.Cat == c)).Where(p => p != null).Select(p => p!.Id).ToList();
            looks.Add((new Look { Momento = l.Momento.Trim(), Ids = ids, Porque = l.Porque.Trim() }, l.Plan == "deporte"));
        }
        if (looks.Count == 0) return null;
        // El conjunto principal es el de los planes «normales»; el de deporte va como cambio de ropa
        var orden = looks.OrderBy(x => x.Deporte).Select(x => x.Look).Take(3).ToList();
        var principal = orden[0];
        var arribaAbajo = principal.Ids.Select(PrendaPorId).Where(p => p != null && (p.Cat == Categoria.Arriba || p.Cat == Categoria.Abajo));
        return new Outfit
        {
            Ids = principal.Ids,
            Porque = principal.Porque,
            Momento = orden.Count > 1 ? principal.Momento : "",
            Extra = orden.Skip(1).ToList(),
            Clave = MotorIA.ClaveDe(arribaAbajo),
            DeIA = true,
        };
    }

    public async Task AsegurarOutfitAsync(string dia)
    {
        if (D.Outfits.ContainsKey(dia)) return;
        D.Outfits[dia] = Recomendar(dia);
        await CambiadoAsync();
    }

    /// <summary>Día para el que se está pidiendo «Otra opción» a la IA (para mostrar «Pensando…»).</summary>
    public string? PensandoDia { get; private set; }

    /// <summary>Mensaje que se ve mientras se busca «Otra opción» (reto 3: la espera se explica).</summary>
    public string MensajeOtra { get; private set; } = "";

    public async Task OtraOpcionAsync(string dia)
    {
        if (PensandoDia != null) return;
        var prev = OutfitDe(dia);
        Outfit? o = null;
        PensandoDia = dia;
        MensajeOtra = MensajesOtra[0];
        Avisar();
        // Si el outfit era de la IA, se le pide otro; si no, salen de las reglas. En los dos casos hay una espera
        // corta con mensajes, para que se note que Cora está trabajando y no parezca que no ha pasado nada.
        Task<List<DiaRespuesta>?>? tareaIA = null;
        if (prev?.DeIA == true)
        {
            var evitar = new Dictionary<string, List<string>> { [dia] = new(prev.Ids) };
            tareaIA = _ia.PlanificarAsync(Peticion(new List<string> { dia }, evitar));
        }
        var inicio = DateTime.Now;
        double pasado;
        while ((pasado = (DateTime.Now - inicio).TotalMilliseconds) < DuracionOtra || (tareaIA != null && !tareaIA.IsCompleted))
        {
            await Task.Delay(150);
            MensajeOtra = MensajesOtra[Math.Min(MensajesOtra.Length - 1, (int)(pasado / 650))];
            Avisar();
        }
        if (tareaIA != null)
        {
            var r = await tareaIA;
            var rd = r?.FirstOrDefault(x => x.Fecha == dia);
            if (rd != null) o = OutfitDesdeIA(rd);
            if (o != null && prev != null && o.Clave == prev.Clave) o = null; // ha repetido: mejor las reglas
        }
        PensandoDia = null;
        o ??= Recomendar(dia, (prev?.Semilla ?? 0) + 1, prev?.Clave);
        D.Outfits[dia] = o;
        foreach (var g in D.Guardados.Where(g => g.Fecha == dia)) g.Ids = new List<string>(o.Ids);
        await SonarAsync("listo");
        await CambiadoAsync();
    }

    // Estado de la espera (reto 3)
    public bool Generando { get; private set; }
    public List<string> DiasGenerando { get; private set; } = new();
    /// <summary>Cuántos días se han rellenado ya tras la espera (-1 = ninguno).</summary>
    public int Rellenos { get; private set; } = -1;
    public string MensajeEspera { get; private set; } = "";
    public int Progreso { get; private set; }

    public bool EsperandoDia(string dia)
    {
        int i = DiasGenerando.IndexOf(dia);
        return i >= 0 && (Generando || (Rellenos >= 0 && i > Rellenos));
    }
    public bool RecienRelleno(string dia)
    {
        int i = DiasGenerando.IndexOf(dia);
        return i >= 0 && !Generando && i == Rellenos;
    }

    /// <summary>
    /// Retos 2 y 3: genera la semana con una espera que se ve (mensajes, huecos grises y relleno día a día).
    /// Primero lo intenta con la IA; los días que la IA no resuelva (o si no hay IA) salen de las reglas.
    /// </summary>
    public async Task GenerarSemanaAsync(List<string> dias)
    {
        if (Generando || dias.Count == 0) return;
        var peticion = Peticion(dias);
        foreach (var d in dias) D.Outfits.Remove(d);
        Generando = true; DiasGenerando = dias; Rellenos = -1; Progreso = 0; MensajeEspera = Mensajes[0];
        Avisar();

        var tareaIA = _ia.PlanificarAsync(peticion);
        var inicio = DateTime.Now;
        double pasado;
        // La espera dura como mínimo DuracionEspera y, si la IA tarda más, hasta que responda
        while ((pasado = (DateTime.Now - inicio).TotalMilliseconds) < DuracionEspera || !tareaIA.IsCompleted)
        {
            await Task.Delay(150);
            Progreso = (int)Math.Min(tareaIA.IsCompleted ? 100 : 95, pasado / DuracionEspera * 100);
            MensajeEspera = Mensajes[Math.Min(Mensajes.Length - 1, (int)(pasado / 900))];
            Avisar();
        }
        var respuesta = await tareaIA;

        // En orden, para que las reglas de respaldo tengan en cuenta los días anteriores (no repetir)
        int deIA = 0;
        foreach (var d in dias)
        {
            var rd = respuesta?.FirstOrDefault(x => x.Fecha == d);
            var o = rd != null ? OutfitDesdeIA(rd) : null;
            if (o != null) deIA++;
            D.Outfits[d] = o ?? Recomendar(d);
        }
        Generando = false; Rellenos = 0; Progreso = 100;
        Avisar();
        for (int i = 1; i < dias.Count; i++)
        {
            await Task.Delay(200);
            Rellenos = i;
            Avisar();
        }
        await Task.Delay(250);
        DiasGenerando = new(); Rellenos = -1;
        await SonarAsync("listo");
        string listo = dias.Count == 1 ? "Tu outfit está listo" : "Tu semana está lista";
        MostrarAviso(respuesta == null ? _ia.ExplicarError() : deIA == dias.Count ? listo : listo + " (algún día con mis reglas)");
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- «Me lo pongo», uso de las prendas y olvidadas
    /// <summary>Marca (o desmarca) que hoy llevas el outfit propuesto. Con confeti, sonido y vibración.</summary>
    public async Task MeLoPongoAsync(string dia)
    {
        var o = OutfitDe(dia);
        if (o == null) return;
        o.Puesto = !o.Puesto;
        if (o.Puesto)
        {
            await SonarAsync("puesto");
            await ConfetiAsync();
            MostrarAviso(T("¡Apuntado! Hoy llevas este outfit", "¡Hecho! Hoy llevas esta ropa"));
        }
        await CambiadoAsync();
    }

    public async Task ConfetiAsync()
    {
        try { await _js.InvokeVoidAsync("cora.confeti"); } catch { }
    }

    /// <summary>Ya se ha usado el gesto de deslizar: deja de salir la pista.</summary>
    public async Task GestoVistoAsync()
    {
        if (D.Ajustes.VistoGesto) return;
        D.Ajustes.VistoGesto = true;
        await GuardarAsync();
    }

    /// <summary>
    /// Cuántas veces has llevado cada prenda y cuándo fue la última. Cuentan los días pasados con outfit
    /// y hoy, solo si has dicho «Me lo pongo».
    /// </summary>
    public List<UsoPrenda> Usos()
    {
        var veces = new Dictionary<string, int>();
        var ultima = new Dictionary<string, string>();
        foreach (var (fecha, o) in DiasPuestos())
            foreach (var id in o.Ids.Distinct())
            {
                veces[id] = veces.GetValueOrDefault(id) + 1;
                if (!ultima.TryGetValue(id, out var u) || string.CompareOrdinal(fecha, u) > 0) ultima[id] = fecha;
            }
        return D.Prendas.Select(p => new UsoPrenda(p, veces.GetValueOrDefault(p.Id), ultima.GetValueOrDefault(p.Id))).ToList();
    }

    /// <summary>Días que cuentan como «llevado»: los pasados con outfit y hoy si lo has marcado.</summary>
    public List<(string Fecha, Outfit O)> DiasPuestos() => D.Outfits
        .Where(x => x.Value.Ids.Count > 0 && (Fechas.Dif(x.Key) < 0 || (Fechas.Dif(x.Key) == 0 && x.Value.Puesto)))
        .Select(x => (x.Key, x.Value)).OrderBy(x => x.Key, StringComparer.Ordinal).ToList();

    public const int DiasOlvido = 14;

    /// <summary>Prendas disponibles que no usas desde hace 2 semanas (o nunca), de la más olvidada a la menos.</summary>
    public List<UsoPrenda> Olvidadas() => Usos()
        .Where(u => !u.P.NoDisponible && (u.Ultima == null || -Fechas.Dif(u.Ultima) >= DiasOlvido))
        .OrderBy(u => u.Ultima ?? "", StringComparer.Ordinal).ThenBy(u => u.P.Nombre).ToList();

    /// <summary>«Aún no la has usado» o «Hace 18 días que no la usas».</summary>
    public static string TextoOlvido(UsoPrenda u) =>
        u.Ultima == null ? "Aún no la has usado" : $"Hace {-Fechas.Dif(u.Ultima)} días que no la usas";

    /// <summary>Prepara el outfit de mañana alrededor de una prenda olvidada.</summary>
    public async Task LlevarMananaAsync(string id)
    {
        string dia = Fechas.Sumar(Fechas.Hoy, 1);
        var prev = OutfitDe(dia);
        var o = MotorIA.Recomendar(dia, D.Prendas, D.Planes.GetValueOrDefault(dia), Tiempo.De(dia), D.Outfits, D.Perfil.Gustos, (prev?.Semilla ?? 0) + 1, null, id);
        o.Fijo = true;
        D.Outfits[dia] = o;
        await SonarAsync("listo");
        MostrarAviso("Listo: mañana la llevas");
        await CambiadoAsync();
    }

    /// <summary>El look montado en el probador pasa a ser el de hoy (y ya cuenta como puesto).</summary>
    public async Task PonerLookHoyAsync(List<string> ids, string porque)
    {
        var prendas = ids.Select(PrendaPorId).Where(p => p != null).Select(p => p!).ToList();
        D.Outfits[Fechas.Hoy] = new Outfit
        {
            Ids = ids, Porque = porque, Fijo = true, Puesto = true,
            Clave = MotorIA.ClaveDe(prendas.Where(p => p.Cat == Categoria.Arriba || p.Cat == Categoria.Abajo)),
        };
        foreach (var g in D.Guardados.Where(g => g.Fecha == Fechas.Hoy)) g.Ids = new List<string>(ids);
        await SonarAsync("puesto");
        await ConfetiAsync();
        MostrarAviso("¡Tu look es el de hoy!");
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- planes
    public async Task AlternarPlanAsync(string dia, string clave)
    {
        if (!D.Planes.TryGetValue(dia, out var p)) D.Planes[dia] = p = new PlanDia();
        if (!p.Claves.Remove(clave)) p.Claves.Add(clave);
        await CambiadoAsync();
    }

    public async Task TextoPlanAsync(string dia, string texto)
    {
        if (!D.Planes.TryGetValue(dia, out var p)) D.Planes[dia] = p = new PlanDia();
        p.Texto = texto;
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- armario y lavadora
    public async Task CambiarLavadoAsync(IEnumerable<string> ids, bool lavando)
    {
        // Se pide permiso para las notificaciones justo al pulsar (el navegador solo deja pedirlo tras un toque)
        if (lavando) { try { await _js.InvokeVoidAsync("cora.pedirPermiso"); } catch { } }
        var lista = ids.ToList();
        foreach (var p in D.Prendas.Where(p => lista.Contains(p.Id)))
        {
            p.Lavando = lavando;
            p.LavadoDesde = lavando ? Fechas.Hoy : null;
            p.LavadoHora = lavando ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) : null;
            p.AvisoSeca = false;
            if (!lavando) p.EnCesto = false;
        }
        // Al echar a lavar, el sonido de «guardado»; al volver al armario, una campanita alegre
        await SonarAsync(lavando ? "guardado" : "disponible");
        await CambiadoAsync();
    }

    public async Task AnadirPrendaAsync(Prenda p)
    {
        D.Prendas.Add(p);
        await SonarAsync("guardado");
        MostrarAviso($"{p.Nombre} guardada en tu armario");
        await CambiadoAsync();
    }

    /// <summary>Cambia para qué planes sirve una prenda (de lo más cómodo a lo más arreglado).</summary>
    public async Task CambiarUsosAsync(string id, int min, int max)
    {
        var p = PrendaPorId(id);
        if (p == null) return;
        p.FormMin = min;
        p.FormMax = max;
        await CambiadoAsync();
    }

    public async Task BorrarPrendaAsync(string id)
    {
        D.Prendas.RemoveAll(p => p.Id == id);
        D.Favoritas.Remove(id);
        await CambiadoAsync();
    }

    public async Task AlternarFavoritaAsync(string id)
    {
        if (!D.Favoritas.Remove(id))
        {
            D.Favoritas.Add(id);
            await SonarAsync("guardado");
            MostrarAviso("Añadida a favoritas");
        }
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- guardados
    public async Task AlternarGuardadoDiaAsync(string dia)
    {
        if (D.Guardados.RemoveAll(g => g.Fecha == dia) == 0)
        {
            var o = OutfitDe(dia);
            if (o == null) return;
            var pl = PlanDe(dia);
            D.Guardados.Add(new Guardado { Id = "o" + dia, Fecha = dia, Ids = new List<string>(o.Ids), Titulo = Fechas.Cap(Fechas.Larga(dia)) + (pl != null ? " · " + pl.Etiqueta : "") });
            await SonarAsync("guardado");
            MostrarAviso("Guardado en tu perfil, en Outfits guardados");
        }
        await CambiadoAsync();
    }

    public bool EstaGuardado(string id) => D.Guardados.Any(g => g.Id == id);

    public async Task AlternarGuardadoAsync(Guardado g)
    {
        if (D.Guardados.RemoveAll(x => x.Id == g.Id) == 0)
        {
            D.Guardados.Add(g);
            await SonarAsync("guardado");
            MostrarAviso("Guardado en tu perfil");
        }
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- perfil y ajustes
    public async Task CambiarFotoPerfilAsync(string? foto)
    {
        D.Perfil.Foto = foto;
        await SonarAsync("guardado");
        await CambiadoAsync();
    }

    public async Task GuardarPerfilAsync(Perfil p)
    {
        bool cambiaCiudad = p.Ciudad != D.Perfil.Ciudad;
        if (string.IsNullOrWhiteSpace(p.Usuario)) p.Usuario = p.Nombre.Trim().ToLower().Replace(" ", "");
        p.Foto = D.Perfil.Foto; // la foto se cambia desde «Mi perfil»
        D.Perfil = p;
        await CambiadoAsync();
        if (cambiaCiudad) _ = CargarTiempoAsync();
    }

    // ---------------------------------------------------------------- accesibilidad
    /// <summary>Texto normal o, con «Palabras más sencillas», su versión fácil.</summary>
    public string T(string normal, string sencillo) => D.Ajustes.TextosSencillos ? sencillo : normal;

    /// <summary>Clases que se ponen en la app entera según los ajustes de accesibilidad.</summary>
    public string ClasesAccesibilidad
    {
        get
        {
            var a = D.Ajustes;
            var l = new List<string>();
            if (a.Letra == "grande") l.Add("letra-grande");
            else if (a.Letra == "muy-grande") l.Add("letra-muy-grande");
            if (a.AltoContraste) l.Add("contraste");
            if (a.Tema == "oscuro") l.Add("oscuro");
            else if (a.Tema == "auto") l.Add("tema-auto");
            if (a.BotonesGrandes) l.Add("botones-grandes");
            if (a.TextosSencillos) l.Add("sencillo");
            if (a.MenosOpciones) l.Add("menos-opciones");
            return string.Join(" ", l);
        }
    }

    /// <summary>Un solo interruptor que pone (o quita) todos los ajustes del modo fácil.</summary>
    public async Task ModoFacilAsync(bool activar)
    {
        var a = D.Ajustes;
        if (activar && a.Letra == "normal") a.Letra = "grande";
        if (!activar) a.Letra = "normal";
        a.BotonesGrandes = a.TextosSencillos = a.MenosOpciones = a.AvisosLargos = activar;
        await SonarAsync("guardado");
        MostrarAviso(activar ? "Modo fácil activado" : "Modo fácil desactivado");
        await CambiadoAsync();
    }

    public async Task CambiarAjustesAsync(Action<Ajustes> cambio)
    {
        cambio(D.Ajustes);
        await CambiadoAsync();
    }

    public async Task RestablecerAsync()
    {
        var perfil = D.Perfil;
        D = Iniciales();
        D.Perfil = perfil;
        MostrarAviso("Datos de ejemplo restablecidos");
        await CambiadoAsync();
    }

    // ---------------------------------------------------------------- avisos (reto 1)
    /// <summary>Sonido + vibración: "leido", "error", "guardado", "listo", "seca" o "disponible".</summary>
    // ---------------------------------------------------------------- Explorar: me gusta, seguir y comentarios
    public bool LeGusta(string pubId) => D.Social.MeGusta.Contains(pubId);
    public bool Sigo(string usuario) => D.Social.Siguiendo.Contains(usuario);
    public List<Comentario> MisComentarios(string pubId) => D.Social.Comentarios.Where(c => c.PubId == pubId).ToList();
    public int TotalComentarios(string pubId) => DatosEjemplo.ComentariosDe(pubId).Length + D.Social.Comentarios.Count(c => c.PubId == pubId);

    public async Task AlternarMeGustaAsync(string pubId)
    {
        if (!D.Social.MeGusta.Remove(pubId)) { D.Social.MeGusta.Add(pubId); await SonarAsync("guardado"); }
        await CambiadoAsync();
    }

    public async Task AlternarSeguirAsync(string usuario)
    {
        if (!D.Social.Siguiendo.Remove(usuario)) D.Social.Siguiendo.Add(usuario);
        await CambiadoAsync();
    }

    public async Task ComentarAsync(string pubId, string texto)
    {
        texto = texto.Trim();
        if (texto.Length == 0) return;
        if (texto.Length > 300) texto = texto[..300];
        D.Social.Comentarios.Add(new Comentario
        {
            Id = Guid.NewGuid().ToString("N")[..10], PubId = pubId, Texto = texto,
            Hora = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
        });
        await SonarAsync("guardado");
        await CambiadoAsync();
    }

    public async Task BorrarComentarioAsync(string id)
    {
        D.Social.Comentarios.RemoveAll(c => c.Id == id);
        await CambiadoAsync();
    }

    public async Task SonarAsync(string tipo)
    {
        try { await _js.InvokeVoidAsync("cora.aviso", tipo, D.Ajustes.Sonido, D.Ajustes.Vibracion); } catch { }
    }

    public string? TextoAviso { get; private set; }
    int _avisoId;

    /// <summary>Mensaje breve abajo de la pantalla (toast).</summary>
    public async void MostrarAviso(string texto)
    {
        int id = ++_avisoId;
        TextoAviso = texto;
        Avisar();
        await Task.Delay(D.Ajustes.AvisosLargos ? 6000 : 2600);
        if (id == _avisoId) { TextoAviso = null; Avisar(); }
    }

    public ValueTask Atras() => _js.InvokeVoidAsync("history.back");
}

/// <summary>Cuántas veces has llevado una prenda y el último día (AAAA-MM-DD), o null si nunca.</summary>
public record UsoPrenda(Prenda P, int Veces, string? Ultima);

/// <summary>Serialización de los datos generada al compilar (funciona también al publicar la web).</summary>
[JsonSerializable(typeof(Datos))]
public partial class CoraJson : JsonSerializerContext { }
