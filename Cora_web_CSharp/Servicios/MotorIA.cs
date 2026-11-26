using System.Text;
using System.Text.RegularExpressions;
using Cora.Modelos;

namespace Cora.Servicios;

/// <summary>Planes de un día ya interpretados.</summary>
public record PlanResuelto(List<TipoPlan> Tipos, int Formalidad, string Etiqueta, string Frase);

/// <summary>
/// Reglas de respaldo del planificador: se usan cuando la IA (Gemini, <see cref="ServicioIA"/>) no está disponible
/// (en local, sin clave o sin cuota). Eligen un conjunto por día y explican por qué.
/// Tienen en cuenta el plan (formalidad), el tiempo, lo que está en la lavadora y los gustos del usuario,
/// NO repiten un conjunto hasta que pasa al menos una semana y, si hay deporte con otros planes,
/// preparan un segundo conjunto para el deporte.
/// </summary>
public static class MotorIA
{
    static string Lista(IList<string> l) => l.Count > 1 ? string.Join(", ", l.Take(l.Count - 1)) + " y " + l[^1] : (l.Count == 1 ? l[0] : "");

    /// <summary>Minúsculas y sin tildes, para que «Pádel», «padel» y «PADEL» se entiendan igual.</summary>
    public static string Normalizar(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (char c in s.ToLowerInvariant())
            sb.Append(c switch
            {
                'á' or 'à' or 'ä' or 'â' => 'a',
                'é' or 'è' or 'ë' or 'ê' => 'e',
                'í' or 'ì' or 'ï' or 'î' => 'i',
                'ó' or 'ò' or 'ö' or 'ô' => 'o',
                'ú' or 'ù' or 'ü' or 'û' => 'u',
                _ => c,
            });
        return sb.ToString();
    }

    /// <summary>Junta los botones marcados y lo que se detecta en el texto libre.</summary>
    public static PlanResuelto? Resolver(PlanDia? p)
    {
        if (p == null) return null;
        var claves = new List<string>(p.Claves);
        if (!string.IsNullOrWhiteSpace(p.Texto))
        {
            string texto = Normalizar(p.Texto);
            // Se ordenan por dónde aparecen en el texto («gimnasio y luego cena» → deporte, cena)
            var encontrados = DatosEjemplo.Planes
                .Select(t => (t, m: Regex.Match(texto, t.Patron, RegexOptions.IgnoreCase)))
                .Where(x => x.m.Success && !claves.Contains(x.t.Clave))
                .OrderBy(x => x.m.Index)
                .Select(x => x.t.Clave)
                .ToList();
            claves.AddRange(encontrados);
        }
        var tipos = claves.Select(c => DatosEjemplo.Planes.FirstOrDefault(x => x.Clave == c)).Where(x => x != null).Select(x => x!).ToList();
        // «Sin planes» no pinta nada si hay otros planes ese día
        if (tipos.Count > 1) tipos.RemoveAll(t => t.Clave == "casa");
        if (tipos.Count == 0) return null;
        return new PlanResuelto(tipos, tipos.Max(x => x.Formalidad), Lista(tipos.Select(x => x.Nombre.ToLower()).ToList()), Lista(tipos.Select(x => x.Frase).ToList()));
    }

    public static string ClaveDe(IEnumerable<Prenda?> prendas) => string.Join("|", prendas.Where(p => p != null).Select(p => p!.Id).OrderBy(x => x, StringComparer.Ordinal));

    /// <summary>Combinaciones usadas (o ya planificadas) a menos de 7 días del día pedido.</summary>
    static Dictionary<string, string> Usadas(string dia, Dictionary<string, Outfit> outfits)
    {
        var m = new Dictionary<string, string>();
        for (int k = -6; k <= 6; k++)
        {
            if (k == 0) continue;
            string d = Fechas.Sumar(dia, k);
            if (outfits.TryGetValue(d, out var o) && !string.IsNullOrEmpty(o.Clave)) m[o.Clave] = d;
        }
        return m;
    }

    /// <param name="inicio">Semilla para empezar a buscar (se usa en «Otra opción»).</param>
    /// <param name="evitar">Combinación que el usuario acaba de descartar.</param>
    /// <param name="forzar">Prenda que tiene que salir sí o sí (p. ej. una que hace tiempo que no se usa).</param>
    public static Outfit Recomendar(string dia, List<Prenda> prendas, PlanDia? planDia, Tiempo w, Dictionary<string, Outfit> outfits, List<string> gustos, int? inicio = null, string? evitar = null, string? forzar = null)
    {
        var plan = Resolver(planDia);
        // El deporte va en un conjunto aparte (hay que cambiarse); el principal se elige para el resto de planes
        var otros = plan?.Tipos.Where(t => t.Clave != "deporte").ToList() ?? new();
        bool conDeporte = plan?.Tipos.Any(t => t.Clave == "deporte") ?? false;
        bool deporteAparte = conDeporte && otros.Count > 0;
        int fPedida = plan == null ? 1 : (otros.Count > 0 ? otros.Max(x => x.Formalidad) : 0);
        string frase = plan == null ? "un día tranquilo" : (deporteAparte ? Lista(otros.Select(x => x.Frase).ToList()) : plan.Frase);

        var disponibles = prendas.Where(p => !p.NoDisponible).ToList();
        // Prenda obligatoria: el conjunto se arma a su alrededor (con el nivel de arreglo más cercano que le encaje)
        var forzada = forzar == null ? null : disponibles.FirstOrDefault(p => p.Id == forzar);
        if (forzada != null && !forzada.Encaja(fPedida)) fPedida = Math.Clamp(fPedida, forzada.FormMin, forzada.FormMax);
        List<Prenda>? Solo(Categoria c) => forzada != null && forzada.Cat == c ? new List<Prenda> { forzada } : null;
        bool frio = w.T < 15, fresco = w.T < 19, llueve = w.Cielo == "lluvia";

        // Orden: con frío primero lo que más abriga; después lo que más le gusta al usuario
        int Gusto(Prenda p) => gustos.Contains(p.Sub) ? 0 : 1;
        List<Prenda> Ordenar(IEnumerable<Prenda> l) => frio
            ? l.OrderByDescending(p => p.Abrigo).ThenBy(Gusto).ToList()
            : l.OrderBy(Gusto).ToList();
        List<Prenda> Arriba(int f) => Solo(Categoria.Arriba) ?? Ordenar(disponibles.Where(p => p.Cat == Categoria.Arriba && p.Encaja(f)));
        List<Prenda> Abajo(int f) => Solo(Categoria.Abajo) ?? Ordenar(disponibles.Where(p => p.Cat == Categoria.Abajo && p.Encaja(f)));

        var usadas = Usadas(dia, outfits);
        int b = inicio ?? (int)(Fechas.Hash(dia) % 5);
        // Si con la formalidad pedida no hay combinación nueva, prueba un punto menos arreglado (nunca baja a «deporte»)
        var niveles = fPedida >= 2 ? new[] { fPedida, fPedida - 1 } : new[] { fPedida };

        Prenda? arriba = null, abajo = null;
        string clave = "";
        int semilla = b, f = fPedida;
        string? choque = null;
        bool repetido = false, rebajado = false;

        foreach (int nivel in niveles)
        {
            var ups = Arriba(nivel); var downs = Abajo(nivel);
            int nu = Math.Max(1, ups.Count), nd = Math.Max(1, downs.Count);
            for (int i = 0; i < nu * nd; i++)
            {
                int s = b + i;
                var u = ups.Count > 0 ? ups[s % nu] : null;
                var d = downs.Count > 0 ? downs[(s / nu) % nd] : null;
                string k = ClaveDe(new[] { u, d });
                if (k == evitar) continue;
                if (usadas.ContainsKey(k) && fPedida > 0) { choque ??= usadas[k]; continue; }
                arriba = u; abajo = d; clave = k; semilla = s; f = nivel; rebajado = nivel < fPedida;
                break;
            }
            if (clave != "") break;
        }
        if (clave == "")
        {
            // Con este armario no se puede evitar repetir: al menos que sea distinta de la descartada
            var ups = Arriba(fPedida); var downs = Abajo(fPedida);
            int nu = Math.Max(1, ups.Count), nd = Math.Max(1, downs.Count);
            for (int i = 0; i < nu * nd; i++)
            {
                int s = b + i;
                var u = ups.Count > 0 ? ups[s % nu] : null;
                var d = downs.Count > 0 ? downs[(s / nu) % nd] : null;
                string k = ClaveDe(new[] { u, d });
                if (k != evitar || i == nu * nd - 1) { arriba = u; abajo = d; clave = k; semilla = s; break; }
            }
            repetido = fPedida > 0; rebajado = false; f = fPedida;
        }

        var zapatos = disponibles.Where(p => p.Cat == Categoria.Calzado && p.Encaja(f)).OrderBy(Gusto).ToList();
        if (llueve) { var secos = zapatos.Where(p => p.AptaLluvia).ToList(); if (secos.Count > 0) zapatos = secos; }
        zapatos = Solo(Categoria.Calzado) ?? zapatos;
        var capas = new List<Prenda>();
        if (fresco)
        {
            capas = disponibles.Where(p => p.Cat == Categoria.Capa && p.Encaja(f)).ToList();
            if (capas.Count == 0 && f > 0) capas = disponibles.Where(p => p.Cat == Categoria.Capa).ToList();
            if (frio) capas = capas.OrderByDescending(p => p.Abrigo).ToList();
        }
        capas = Solo(Categoria.Capa) ?? capas;
        var zapato = zapatos.Count > 0 ? zapatos[semilla % zapatos.Count] : null;
        var capa = capas.Count > 0 ? capas[semilla % capas.Count] : null;
        var ids = new[] { capa, arriba, abajo, zapato }.Where(p => p != null).Select(p => p!.Id).ToList();

        // Explicación en lenguaje natural
        var porque = new List<string> { $"{w.T}° y {w.Cielo}, y toca {frase}." };
        if (forzada != null) porque.Add($"Rescato {forzada.Nombre.ToLower()}, que hace tiempo que no usas.");
        if (otros.Count > 1) porque.Add("Día mixto: elijo un look que sirva para el plan más arreglado.");
        if (capa != null) porque.Add(frio ? $"Con frío, añado {capa.Nombre.ToLower()}." : $"Para la mañana fresca, {capa.Nombre.ToLower()}.");
        if (llueve && zapato != null) porque.Add($"Llueve: {zapato.Nombre.ToLower()} mejor que tacones.");
        if (choque != null && !repetido) porque.Add($"Cambio la combinación para no repetir la del {Fechas.Dias[(int)Fechas.De(choque).DayOfWeek]}.");
        if (rebajado) porque.Add("Elijo algo un poco menos arreglado para no repetir conjunto en la misma semana.");
        if (repetido) porque.Add("Con tu armario no puedo evitar repetir esta semana: añade alguna prenda más.");
        var fuera = prendas.Where(p => p.NoDisponible && p.Encaja(f) && p.Cat != Categoria.Calzado).Select(p => p.Nombre).ToList();
        if (fuera.Count > 0) porque.Add($"{string.Join(", ", fuera)} no entra: está para lavar.");
        if (ids.Count < 3) porque.Add(f == 0
            ? "Te falta ropa de deporte: añade alguna prenda y márcala como «Deporte»."
            : "Te faltan prendas para este plan: añade alguna más.");

        var outfit = new Outfit { Ids = ids, Porque = string.Join(" ", porque), Clave = clave, Semilla = semilla };
        if (deporteAparte)
        {
            outfit.Momento = Fechas.Cap(Lista(otros.Select(x => x.Nombre.ToLower()).ToList()));
            outfit.Extra.Add(LookDeporte(disponibles, ids, semilla, fresco));
        }
        return outfit;
    }

    /// <summary>Segundo conjunto del día, solo con prendas marcadas para «Deporte».</summary>
    static Look LookDeporte(List<Prenda> disponibles, List<string> yaElegidas, int semilla, bool fresco)
    {
        Prenda? Elegir(Categoria c)
        {
            var l = disponibles.Where(p => p.Cat == c && p.Encaja(0)).ToList();
            var libres = l.Where(p => !yaElegidas.Contains(p.Id)).ToList();
            if (libres.Count > 0) l = libres;
            return l.Count == 0 ? null : l[semilla % l.Count];
        }
        var arriba = Elegir(Categoria.Arriba);
        var abajo = Elegir(Categoria.Abajo);
        var zapato = Elegir(Categoria.Calzado);
        var capa = fresco ? Elegir(Categoria.Capa) : null;
        var ids = new[] { capa, arriba, abajo, zapato }.Where(p => p != null).Select(p => p!.Id).ToList();
        string porque = arriba != null && abajo != null
            ? $"Para el deporte, ropa cómoda: {arriba.Nombre.ToLower()} y {abajo.Nombre.ToLower()}."
            : "No tienes suficiente ropa de deporte: añade alguna prenda y márcala como «Deporte».";
        return new Look { Momento = "Para hacer deporte", Ids = ids, Porque = porque };
    }
}
