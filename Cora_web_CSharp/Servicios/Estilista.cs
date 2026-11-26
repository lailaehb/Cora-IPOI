using Cora.Modelos;

namespace Cora.Servicios;

/// <summary>Un comentario de Cora sobre el look: bien (✓) o un consejo (!).</summary>
public record Punto(bool Bien, string Texto);

/// <summary>Lo que opina Cora de un look: nota de 0 a 100, una frase corta, los detalles y para qué planes sirve.</summary>
public record Opinion(int Nota, string Titulo, List<Punto> Puntos, List<string> Planes);

/// <summary>
/// Opinión al momento para el probador: mira si las prendas son del mismo estilo (deporte, diario, arreglado, fiesta),
/// si van bien con el tiempo de hoy, si los colores combinan y si ya llevaste esa combinación esta semana.
/// Son reglas sencillas, así responde enseguida mientras deslizas.
/// </summary>
public static class Estilista
{
    /// <summary>Colores que combinan con todo.</summary>
    static readonly string[] Neutros = { "negro", "blanco", "gris", "beige", "crudo", "marrón", "azul marino", "marino", "azul", "azul claro", "camel", "caqui", "denim" };

    /// <summary>Color en CSS para pintar el puntito de cada color (si no se conoce, uno suave).</summary>
    public static string Css(string color) => Normal(color) switch
    {
        "negro" => "#232023", "blanco" => "#F7F5F2", "gris" => "#A3A0A2", "beige" => "#D9C6A5", "crudo" => "#EDE3CC",
        "marrón" => "#7A5232", "camel" => "#C19A6B", "azul" => "#3F6FB0", "azul claro" => "#9DBBDD", "azul marino" or "marino" => "#1F2F55",
        "rojo" => "#C0392B", "burdeos" => "#7B1E3A", "granate" => "#7A2433", "verde" => "#3F7D4E", "verde oliva" => "#6B6B3A",
        "caqui" => "#8A8455", "amarillo" => "#E8C33A", "coral" => "#F27B63", "malva" => "#B48BB5", "lima" => "#B8D94A",
        "rosa" => "#E8A1B5", "naranja" => "#E67E22", "morado" => "#7D4FA0", "lila" => "#C3A6D8", "plateado" => "#C9CCD1", "dorado" => "#D4AF37",
        _ => "#D8C7C2",
    };

    /// <summary>Para qué es cada nivel de arreglo, en una frase (0 deporte … 3 fiesta).</summary>
    static readonly string[] Para = { "hacer deporte", "el día a día", "planes arreglados", "ir de fiesta" };

    public static string Normal(string color) => (color ?? "").Trim().ToLowerInvariant();
    public static bool EsNeutro(string color) => string.IsNullOrWhiteSpace(color) || Neutros.Contains(Normal(color));

    public static Opinion Opinar(List<Prenda> look, Tiempo w, Dictionary<string, Outfit> outfits, string dia)
    {
        var puntos = new List<Punto>();
        int nota = 100;
        var arriba = look.FirstOrDefault(p => p.Cat == Categoria.Arriba);
        var abajo = look.FirstOrDefault(p => p.Cat == Categoria.Abajo);
        var zapato = look.FirstOrDefault(p => p.Cat == Categoria.Calzado);
        var capa = look.FirstOrDefault(p => p.Cat == Categoria.Capa);

        // 1. Estilo: ¿hay algún plan para el que sirvan todas las prendas a la vez?
        int min = look.Count == 0 ? 0 : look.Max(p => p.FormMin), max = look.Count == 0 ? 3 : look.Min(p => p.FormMax);
        var planes = new List<string>();
        if (min <= max)
        {
            planes = Enumerable.Range(min, max - min + 1).Select(f => ServicioIA.Usos[f]).ToList();
            puntos.Add(new(true, $"Todo va a juego de estilo: sirve para {Lista(planes.Select(x => x.ToLower()).ToList())}."));
        }
        else
        {
            var comoda = look.OrderBy(p => p.FormMax).First();
            var arreglada = look.OrderByDescending(p => p.FormMin).First();
            nota -= 35;
            puntos.Add(new(false, $"{comoda.Nombre} es para {Para[comoda.FormMax]} y {arreglada.Nombre.ToLower()}, para {Para[arreglada.FormMin]}. Prueba a cambiar una de las dos."));
        }

        // 2. Tiempo de hoy
        bool llueve = w.Cielo == "lluvia";
        var calurosas = look.Where(p => p.Abrigo >= 2).ToList();
        int fallosTiempo = 0;
        if (w.T < 15 && capa == null) { nota -= 20; fallosTiempo++; puntos.Add(new(false, $"Hoy hace {w.T}°: ponte una chaqueta o un abrigo.")); }
        else if (w.T < 15 && capa != null) puntos.Add(new(true, $"Con {w.T}°, {capa.Nombre.ToLower()} te abrigará."));
        if (w.T >= 24 && calurosas.Count > 0) { nota -= 15; fallosTiempo++; puntos.Add(new(false, $"Con {w.T}° vas a pasar calor con {calurosas[0].Nombre.ToLower()}.")); }
        if (llueve && zapato != null && !zapato.AptaLluvia) { nota -= 20; fallosTiempo++; puntos.Add(new(false, $"Va a llover y {zapato.Nombre.ToLower()} se pueden estropear.")); }
        if (fallosTiempo == 0 && !(w.T < 15 && capa != null)) puntos.Add(new(true, $"Perfecto para hoy: {w.T}° y {w.Cielo}."));

        // 3. Colores: los neutros combinan con todo; muchos colores fuertes a la vez, mejor no
        var fuertes = look.Where(p => !EsNeutro(p.Color)).Select(p => Normal(p.Color)).Distinct().ToList();
        if (fuertes.Count == 0) puntos.Add(new(true, "Colores neutros: combinan seguro."));
        else if (fuertes.Count == 1) puntos.Add(new(true, $"El {fuertes[0]} da el toque de color y el resto lo acompaña."));
        else if (fuertes.Count == 2) { nota -= 5; puntos.Add(new(true, $"{Cap(fuertes[0])} y {fuertes[1]}: atrevido, ¡me gusta!")); }
        else { nota -= 15; puntos.Add(new(false, $"Muchos colores fuertes a la vez ({Lista(fuertes)}). Cambia uno por algo negro, blanco o gris.")); }

        // 4. ¿Ya llevaste esta combinación esta semana?
        string clave = MotorIA.ClaveDe(new[] { arriba, abajo });
        for (int k = -6; k <= 6; k++)
        {
            if (k == 0) continue;
            string d = Fechas.Sumar(dia, k);
            if (outfits.TryGetValue(d, out var o) && o.Clave == clave && clave != "")
            {
                nota -= 10;
                puntos.Add(new(false, k < 0 ? $"Ya llevaste esta combinación {Cuando(d)}." : $"Ya tienes esta combinación para {Cuando(d)}."));
                break;
            }
        }
        if (zapato == null) { nota -= 10; puntos.Add(new(false, "Te falta el calzado: añade unos zapatos a tu armario.")); }

        nota = Math.Clamp(nota, 10, 100);
        string titulo = nota >= 85 ? "¡Combinan genial!" : nota >= 65 ? "Va bien, con algún detalle" : nota >= 45 ? "Yo cambiaría algo" : "Mejor prueba otra cosa";
        // Primero lo que hay que cambiar, después lo que está bien
        return new Opinion(nota, titulo, puntos.OrderBy(p => p.Bien).ToList(), planes);
    }

    /// <summary>«ayer», «mañana» o «el lunes 16».</summary>
    static string Cuando(string d) => Math.Abs(Fechas.Dif(d)) <= 1 ? Fechas.Etiqueta(d).ToLower() : "el " + Fechas.Etiqueta(d).ToLower();
    static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
    static string Lista(IList<string> l) => l.Count > 1 ? string.Join(", ", l.Take(l.Count - 1)) + " y " + l[^1] : (l.Count == 1 ? l[0] : "");
}
