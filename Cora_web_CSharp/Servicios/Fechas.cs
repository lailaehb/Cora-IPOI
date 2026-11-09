using System.Globalization;

namespace Cora.Servicios;

/// <summary>Utilidades de fechas. Guardamos los días como texto «AAAA-MM-DD».</summary>
public static class Fechas
{
    public static readonly string[] Dias = { "domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado" };
    public static readonly string[] Meses = { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };

    public static string Iso(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static DateTime De(string s) => DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static string Hoy => Iso(DateTime.Today);
    public static string Sumar(string s, int dias) => Iso(De(s).AddDays(dias));
    /// <summary>Días desde hoy hasta s (negativo si ya pasó).</summary>
    public static int Dif(string s) => (int)Math.Round((De(s) - DateTime.Today).TotalDays);

    /// <summary>Lunes de la semana del día s (en España la semana empieza en lunes).</summary>
    public static string Lunes(string s)
    {
        var d = De(s);
        int w = ((int)d.DayOfWeek + 6) % 7;
        return Iso(d.AddDays(-w));
    }

    public static List<string> Semana(string s)
    {
        var l = Lunes(s);
        return Enumerable.Range(0, 7).Select(i => Sumar(l, i)).ToList();
    }

    public static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    public static string DiaCorto(string s) => Dias[(int)De(s).DayOfWeek][..3];

    public static string Etiqueta(string s)
    {
        int n = Dif(s);
        if (n == 0) return "Hoy";
        if (n == 1) return "Mañana";
        if (n == -1) return "Ayer";
        var d = De(s);
        return $"{Cap(Dias[(int)d.DayOfWeek])} {d.Day}";
    }

    public static string Larga(string s)
    {
        var d = De(s);
        return $"{Dias[(int)d.DayOfWeek]}, {d.Day} de {Meses[d.Month - 1]}";
    }

    /// <summary>Número fijo para cada texto (sirve para que cada día tenga su propia propuesta).</summary>
    public static uint Hash(string s)
    {
        uint h = 7;
        unchecked { foreach (char c in s) h = h * 31 + c; }
        return h;
    }
}
