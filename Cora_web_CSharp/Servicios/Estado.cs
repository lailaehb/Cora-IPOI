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
    static readonly string[] Mensajes = { "Mirando el tiempo de la semana…", "Descartando lo que está en la lavadora…", "Evitando repetir conjuntos…", "Combinando tus prendas…" };
    const int DuracionEspera = 3600;

    private readonly IJSRuntime _js;
    public ServicioTiempo Tiempo { get; }

    public Estado(IJSRuntime js, ServicioTiempo tiempo)
    {
        _js = js;
        Tiempo = tiempo;
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
        LiberarLavadas();
        Listo = true;
        Avisar();
        _ = CargarTiempoAsync();
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
        foreach (var p in d.Prendas.Where(p => p.Lavando)) p.LavadoDesde = hoy;
        d.Planes[hoy] = new PlanDia { Claves = { "uni" } };
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

    /// <summary>Las prendas que llevan 2 días en la lavadora vuelven a estar disponibles solas.</summary>
    void LiberarLavadas()
    {
        foreach (var p in D.Prendas.Where(p => p.Lavando && p.LavadoDesde != null && -Fechas.Dif(p.LavadoDesde) >= DiasLavado))
        {
            p.Lavando = false;
            p.LavadoDesde = null;
        }
    }

    // ---------------------------------------------------------------- tiempo
    public async Task CargarTiempoAsync()
    {
        if (await Tiempo.CargarAsync(D.Perfil.Ciudad))
        {
            // Se recalculan los outfits de hoy en adelante que se hicieron con otro tiempo
            for (int i = 0; i < 16; i++)
            {
                string dia = Fechas.Sumar(Fechas.Hoy, i);
                var w = Tiempo.De(dia);
                if (D.Outfits.TryGetValue(dia, out var o) && !o.Porque.StartsWith($"{w.T}° y {w.Cielo}"))
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

    // ---------------------------------------------------------------- IA
    public async Task AsegurarOutfitAsync(string dia)
    {
        if (D.Outfits.ContainsKey(dia)) return;
        D.Outfits[dia] = Recomendar(dia);
        await CambiadoAsync();
    }

    public async Task OtraOpcionAsync(string dia)
    {
        var prev = OutfitDe(dia);
        var o = Recomendar(dia, (prev?.Semilla ?? 0) + 1, prev?.Clave);
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

    /// <summary>Retos 2 y 3: genera la semana con una espera que se ve (mensajes, huecos grises y relleno día a día).</summary>
    public async Task GenerarSemanaAsync(List<string> dias)
    {
        if (Generando || dias.Count == 0) return;
        foreach (var d in dias) D.Outfits.Remove(d);
        Generando = true; DiasGenerando = dias; Rellenos = -1; Progreso = 0; MensajeEspera = Mensajes[0];
        Avisar();

        var inicio = DateTime.Now;
        double pasado;
        while ((pasado = (DateTime.Now - inicio).TotalMilliseconds) < DuracionEspera)
        {
            await Task.Delay(150);
            Progreso = (int)Math.Min(100, pasado / DuracionEspera * 100);
            MensajeEspera = Mensajes[Math.Min(Mensajes.Length - 1, (int)(pasado / 900))];
            Avisar();
        }

        // Se calculan en orden, para que cada día tenga en cuenta los anteriores (no repetir)
        foreach (var d in dias) D.Outfits[d] = Recomendar(d);
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
        MostrarAviso("Tu semana está lista");
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
        var lista = ids.ToList();
        foreach (var p in D.Prendas.Where(p => lista.Contains(p.Id)))
        {
            p.Lavando = lavando;
            p.LavadoDesde = lavando ? Fechas.Hoy : null;
        }
        await SonarAsync("guardado");
        await CambiadoAsync();
    }

    public async Task AnadirPrendaAsync(Prenda p)
    {
        D.Prendas.Add(p);
        await SonarAsync("guardado");
        MostrarAviso($"{p.Nombre} guardada en tu armario");
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
    public async Task GuardarPerfilAsync(Perfil p)
    {
        bool cambiaCiudad = p.Ciudad != D.Perfil.Ciudad;
        if (string.IsNullOrWhiteSpace(p.Usuario)) p.Usuario = p.Nombre.Trim().ToLower().Replace(" ", "");
        D.Perfil = p;
        await CambiadoAsync();
        if (cambiaCiudad) _ = CargarTiempoAsync();
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
    /// <summary>Sonido + vibración: "leido", "error", "guardado" o "listo".</summary>
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
        await Task.Delay(2600);
        if (id == _avisoId) { TextoAviso = null; Avisar(); }
    }

    public ValueTask Atras() => _js.InvokeVoidAsync("history.back");
}

/// <summary>Serialización de los datos generada al compilar (funciona también al publicar la web).</summary>
[JsonSerializable(typeof(Datos))]
public partial class CoraJson : JsonSerializerContext { }
