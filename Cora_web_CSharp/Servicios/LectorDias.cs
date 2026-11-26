using System.Text.RegularExpressions;

namespace Cora.Servicios;

/// <summary>Lo que se ha entendido: lo que vale para todos los días y lo de cada día concreto (fecha → texto).</summary>
public record Reparto(string General, Dictionary<string, string> PorDia);

/// <summary>
/// Reparte un texto entre varios días: «el lunes clase, el martes gimnasio y el miércoles cena con amigas»
/// → lunes: «clase», martes: «gimnasio», miércoles: «cena con amigas».
/// Entiende los días de la semana (también en inglés y francés), «hoy», «mañana», «pasado mañana» y «el finde».
/// «Por la mañana» no cuenta como «mañana». Lo que va antes del primer día vale para todos (p. ej. «viaje a la playa»).
/// Si se nombran varios días seguidos («el lunes y el martes clase»), el texto vale para todos ellos.
/// </summary>
public static class LectorDias
{
    // Se busca sobre el texto en minúsculas y sin tildes (MotorIA.Normalizar mantiene la misma longitud)
    static readonly Regex Dia = new(
        @"\b(pasado\s+ma[nñ]ana|(?<!(?:la|esta|cada|de|por)\s)ma[nñ]ana|hoy|lunes|martes|miercoles|jueves|viernes|sabados?|domingos?|fin(?:es)?\s+de\s+semana|finde|" +
        @"today|tomorrow|mondays?|tuesdays?|wednesdays?|thursdays?|fridays?|saturdays?|sundays?|weekend|" +
        @"aujourd'hui|demain|lundi|mardi|mercredi|jeudi|vendredi|samedi|dimanche|week-end)\b",
        RegexOptions.Compiled);

    /// <summary>Palabras que solo unen días («el lunes y el martes»): si entre dos días solo hay esto, van juntos.</summary>
    static readonly Regex SoloUnion = new(@"^[\s,/&]*((y|e|o|el|los|la|las|and|or|the|on|et|ou|le|les)[\s,/&]+)*(y|e|o|el|los|la|las|and|or|the|on|et|ou|le|les)?[\s,/&]*$", RegexOptions.Compiled);
    static readonly Regex Cola = new(@"(\s*[,;.:\-]*\s*\b(y|e|el|los|la|las|and|the|on|et|le|les|luego|despues|then|puis)\b)+\s*[,;.:\-]*\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static readonly Regex Cabeza = new(@"^\s*[,;.:\-]*\s*((y|e|de|luego|despues|después|then|and|puis|et)\b\s*)*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <param name="dias">Fechas (AAAA-MM-DD) entre las que se reparte.</param>
    public static Reparto Repartir(string texto, IList<string> dias)
    {
        var porDia = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(texto)) return new Reparto("", porDia);
        string norm = MotorIA.Normalizar(texto);
        if (norm.Length != texto.Length) norm = texto.ToLowerInvariant(); // por si acaso: los índices tienen que coincidir
        var menciones = Dia.Matches(norm).Cast<Match>().Select(m => (m.Index, Fin: m.Index + m.Length, Fechas: Resolver(m.Value, dias))).ToList();
        if (menciones.Count == 0) return new Reparto(Limpiar(texto), porDia);

        string general = Limpiar(texto[..menciones[0].Index]);
        int i = 0;
        while (i < menciones.Count)
        {
            // Grupo de días seguidos («el lunes y el martes»)
            var fechas = new List<string>(menciones[i].Fechas);
            int j = i;
            while (j + 1 < menciones.Count && SoloUnion.IsMatch(norm[menciones[j].Fin..menciones[j + 1].Index]))
            {
                j++;
                fechas.AddRange(menciones[j].Fechas);
            }
            int desde = menciones[j].Fin, hasta = j + 1 < menciones.Count ? menciones[j + 1].Index : texto.Length;
            string trozo = Limpiar(texto[desde..hasta]);
            foreach (var f in fechas.Distinct())
                porDia[f] = porDia.TryGetValue(f, out var ya) && ya.Length > 0 ? (trozo.Length > 0 ? ya + ", " + trozo : ya) : trozo;
            i = j + 1;
        }
        return new Reparto(general, porDia);
    }

    /// <summary>Qué fechas de la lista son «el lunes», «mañana», «el finde»…</summary>
    static List<string> Resolver(string palabra, IList<string> dias)
    {
        string hoy = Fechas.Hoy;
        palabra = Regex.Replace(palabra, @"\s+", " ").Replace('ñ', 'n');
        if (palabra is "hoy" or "today" or "aujourd'hui") return dias.Where(d => d == hoy).ToList();
        if (palabra is "manana" or "tomorrow" or "demain") return dias.Where(d => d == Fechas.Sumar(hoy, 1)).ToList();
        if (palabra == "pasado manana") return dias.Where(d => d == Fechas.Sumar(hoy, 2)).ToList();
        var semana = palabra switch
        {
            "lunes" or "monday" or "mondays" or "lundi" => new[] { DayOfWeek.Monday },
            "martes" or "tuesday" or "tuesdays" or "mardi" => new[] { DayOfWeek.Tuesday },
            "miercoles" or "wednesday" or "wednesdays" or "mercredi" => new[] { DayOfWeek.Wednesday },
            "jueves" or "thursday" or "thursdays" or "jeudi" => new[] { DayOfWeek.Thursday },
            "viernes" or "friday" or "fridays" or "vendredi" => new[] { DayOfWeek.Friday },
            "sabado" or "sabados" or "saturday" or "saturdays" or "samedi" => new[] { DayOfWeek.Saturday },
            "domingo" or "domingos" or "sunday" or "sundays" or "dimanche" => new[] { DayOfWeek.Sunday },
            _ => new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, // finde, fin de semana, weekend
        };
        return dias.Where(d => semana.Contains(Fechas.De(d).DayOfWeek)).ToList();
    }

    /// <summary>Quita comas, «y», «el»… sueltos al principio y al final del trozo.</summary>
    static string Limpiar(string t)
    {
        t = Cola.Replace(t, "");
        t = Cabeza.Replace(t, "");
        return t.Trim(' ', ',', ';', '.', ':', '-');
    }
}
