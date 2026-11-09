using System.Text.RegularExpressions;
using Cora.Modelos;

namespace Cora.Servicios;

/// <summary>Planes de un día ya interpretados.</summary>
public record PlanResuelto(List<TipoPlan> Tipos, int Formalidad, string Etiqueta, string Frase);

/// <summary>
/// «IA» del planificador (reto 2): un sistema de reglas que elige un conjunto por día y explica por qué.
/// Tiene en cuenta el plan (formalidad), el tiempo, lo que está en la lavadora y los gustos del usuario,
/// y NO repite un conjunto hasta que pasa al menos una semana.
/// </summary>
public static class MotorIA
{
    static string Lista(IList<string> l) => l.Count > 1 ? string.Join(", ", l.Take(l.Count - 1)) + " y " + l[^1] : (l.Count == 1 ? l[0] : "");

    /// <summary>Junta los botones marcados y lo que se detecta en el texto libre.</summary>
    public static PlanResuelto? Resolver(PlanDia? p)
    {
        if (p == null) return null;
        var claves = new List<string>(p.Claves);
        if (!string.IsNullOrWhiteSpace(p.Texto))
            foreach (var t in DatosEjemplo.Planes)
                if (Regex.IsMatch(p.Texto, t.Patron, RegexOptions.IgnoreCase) && !claves.Contains(t.Clave)) claves.Add(t.Clave);
        var tipos = claves.Select(c => DatosEjemplo.Planes.FirstOrDefault(x => x.Clave == c)).Where(x => x != null).Select(x => x!).ToList();
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
    public static Outfit Recomendar(string dia, List<Prenda> prendas, PlanDia? planDia, Tiempo w, Dictionary<string, Outfit> outfits, List<string> gustos, int? inicio = null, string? evitar = null)
    {
        var plan = Resolver(planDia);
        int fPedida = plan?.Formalidad ?? 1;
        string frase = plan?.Frase ?? "un día tranquilo";
        var disponibles = prendas.Where(p => !p.Lavando).ToList();
        bool frio = w.T < 15, fresco = w.T < 19, llueve = w.Cielo == "lluvia";

        // Orden: con frío primero lo que más abriga; después lo que más le gusta al usuario
        int Gusto(Prenda p) => gustos.Contains(p.Sub) ? 0 : 1;
        List<Prenda> Ordenar(IEnumerable<Prenda> l) => frio
            ? l.OrderByDescending(p => p.Abrigo).ThenBy(Gusto).ToList()
            : l.OrderBy(Gusto).ToList();
        List<Prenda> Arriba(int f) => Ordenar(disponibles.Where(p => p.Cat == Categoria.Arriba && p.Encaja(f)));
        List<Prenda> Abajo(int f) => Ordenar(disponibles.Where(p => p.Cat == Categoria.Abajo && p.Encaja(f)));

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
                if (usadas.ContainsKey(k)) { choque ??= usadas[k]; continue; }
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
            repetido = true; rebajado = false; f = fPedida;
        }

        var zapatos = disponibles.Where(p => p.Cat == Categoria.Calzado && p.Encaja(f)).OrderBy(Gusto).ToList();
        if (llueve) { var secos = zapatos.Where(p => p.AptaLluvia).ToList(); if (secos.Count > 0) zapatos = secos; }
        var capas = new List<Prenda>();
        if (fresco)
        {
            capas = disponibles.Where(p => p.Cat == Categoria.Capa && p.Encaja(f)).ToList();
            if (capas.Count == 0) capas = disponibles.Where(p => p.Cat == Categoria.Capa).ToList();
            if (frio) capas = capas.OrderByDescending(p => p.Abrigo).ToList();
        }
        var zapato = zapatos.Count > 0 ? zapatos[semilla % zapatos.Count] : null;
        var capa = capas.Count > 0 ? capas[semilla % capas.Count] : null;
        var ids = new[] { capa, arriba, abajo, zapato }.Where(p => p != null).Select(p => p!.Id).ToList();

        // Explicación en lenguaje natural
        var porque = new List<string> { $"{w.T}° y {w.Cielo}, y toca {frase}." };
        if (plan != null && plan.Tipos.Count > 1) porque.Add("Día mixto: elijo un look que sirva para el plan más arreglado.");
        if (capa != null) porque.Add(frio ? $"Con frío, añado {capa.Nombre.ToLower()}." : $"Para la mañana fresca, {capa.Nombre.ToLower()}.");
        if (llueve && zapato != null) porque.Add($"Llueve: {zapato.Nombre.ToLower()} mejor que tacones.");
        if (choque != null && !repetido) porque.Add($"Cambio la combinación para no repetir la del {Fechas.Dias[(int)Fechas.De(choque).DayOfWeek]}.");
        if (rebajado) porque.Add("Elijo algo un poco menos arreglado para no repetir conjunto en la misma semana.");
        if (repetido) porque.Add("Con tu armario no puedo evitar repetir esta semana: añade alguna prenda más.");
        var fuera = prendas.Where(p => p.Lavando && p.Encaja(f) && p.Cat != Categoria.Calzado).Select(p => p.Nombre).ToList();
        if (fuera.Count > 0) porque.Add($"{string.Join(", ", fuera)} no entra: está en la lavadora.");
        if (ids.Count < 3) porque.Add("Te faltan prendas para este plan: añade alguna más.");

        return new Outfit { Ids = ids, Porque = string.Join(" ", porque), Clave = clave, Semilla = semilla };
    }
}
