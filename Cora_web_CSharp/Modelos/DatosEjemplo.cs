namespace Cora.Modelos;

/// <summary>Datos de ejemplo: el armario de Pilar, las etiquetas de prueba, los planes y Explorar.</summary>
public static class DatosEjemplo
{
    static Prenda P(string id, string n, string m, Categoria c, string sub, string color, string talla, int fmin, int fmax, int abrigo = 1, bool lluvia = true, bool lavando = false) =>
        new() { Id = id, Nombre = n, Marca = m, Cat = c, Sub = sub, Color = color, Talla = talla, FormMin = fmin, FormMax = fmax, Abrigo = abrigo, AptaLluvia = lluvia, Lavando = lavando };

    public static List<Prenda> Armario() => new()
    {
        P("camiseta_blanca", "Camiseta blanca", "Zara", Categoria.Arriba, "Camisetas", "blanco", "M", 0, 1, abrigo: 0),
        P("top_punto_gris", "Top de punto gris", "Zara", Categoria.Arriba, "Tops", "gris", "S", 1, 2, abrigo: 0),
        P("top_rojo", "Top rojo satinado", "Zara", Categoria.Arriba, "Tops", "rojo", "S", 2, 3, abrigo: 0),
        P("sudadera_gris", "Sudadera gris", "Zara", Categoria.Arriba, "Sudaderas", "gris", "M", 0, 1, abrigo: 1, lavando: true),
        P("jersey_negro", "Chaqueta de punto negra", "Mango", Categoria.Capa, "Chaquetas", "negro", "M", 1, 2, abrigo: 2),
        P("blazer_negro", "Blazer negro", "Mango", Categoria.Capa, "Blazers", "negro", "M", 2, 3, abrigo: 1),
        P("vaquero_azul", "Vaquero azul", "Levi's", Categoria.Abajo, "Vaqueros", "azul", "38", 1, 2),
        P("vaquero_claro", "Vaquero claro", "Levi's", Categoria.Abajo, "Vaqueros", "azul claro", "38", 1, 1),
        P("falda_negra", "Falda negra", "Zara", Categoria.Abajo, "Faldas", "negro", "S", 2, 3, abrigo: 0),
        P("chandal_gris", "Pantalón de chándal", "Zara", Categoria.Abajo, "Pantalones", "gris", "M", 0, 1),
        P("cargo_verde", "Pantalón cargo", "Pull&Bear", Categoria.Abajo, "Pantalones", "verde oliva", "38", 1, 1),
        P("zapatillas", "Zapatillas blancas", "Nike", Categoria.Calzado, "Zapatillas", "blanco", "38", 0, 1, abrigo: 0, lluvia: true),
        P("tacones_negros", "Tacones negros", "Zara", Categoria.Calzado, "Tacones", "negro", "38", 2, 3, abrigo: 0, lluvia: false),
        P("botas_negras", "Botas altas", "Mango", Categoria.Calzado, "Botas", "marrón", "38", 1, 3, abrigo: 0, lluvia: true),
    };

    /// <summary>Prendas que llegan por QR (las «etiquetas de prueba» del prototipo).</summary>
    public static List<Prenda> Etiquetas() => new()
    {
        P("jersey_gris", "Jersey gris oversize", "Massimo Dutti", Categoria.Arriba, "Jerséis", "gris", "M", 1, 2, abrigo: 2),
        P("camisa_azul", "Camisa azul cielo", "Mango", Categoria.Arriba, "Camisas", "azul claro", "S", 1, 2, abrigo: 0),
    };

    public static readonly Dictionary<Categoria, string> NombreCat = new()
    {
        [Categoria.Arriba] = "Parte de arriba", [Categoria.Abajo] = "Parte de abajo", [Categoria.Calzado] = "Calzado", [Categoria.Capa] = "Abrigos y chaquetas",
    };

    public static readonly Dictionary<Categoria, string> NombreCorto = new()
    {
        [Categoria.Arriba] = "Arriba", [Categoria.Abajo] = "Abajo", [Categoria.Calzado] = "Calzado", [Categoria.Capa] = "Abrigos",
    };

    public static readonly Dictionary<Categoria, string[]> Subcategorias = new()
    {
        [Categoria.Arriba] = new[] { "Camisetas", "Tops", "Camisas", "Sudaderas", "Jerséis", "Deportiva" },
        [Categoria.Abajo] = new[] { "Vaqueros", "Pantalones", "Faldas", "Mallas", "Shorts" },
        [Categoria.Calzado] = new[] { "Zapatillas", "Botas", "Tacones" },
        [Categoria.Capa] = new[] { "Chaquetas", "Blazers", "Abrigos" },
    };

    /// <summary>Valores por defecto para prendas nuevas: (formalidad mínima, máxima, abrigo, apta para lluvia).</summary>
    public static readonly Dictionary<string, (int Min, int Max, int Abrigo, bool Lluvia)> PorDefecto = new()
    {
        ["Camisetas"] = (0, 1, 0, true), ["Tops"] = (1, 2, 0, true), ["Camisas"] = (1, 2, 0, true), ["Sudaderas"] = (0, 1, 1, true), ["Jerséis"] = (1, 2, 2, true), ["Deportiva"] = (0, 0, 0, true),
        ["Vaqueros"] = (1, 2, 1, true), ["Pantalones"] = (1, 2, 1, true), ["Faldas"] = (2, 3, 0, true), ["Mallas"] = (0, 0, 1, true), ["Shorts"] = (0, 1, 0, true),
        ["Zapatillas"] = (0, 1, 0, true), ["Botas"] = (1, 3, 0, true), ["Tacones"] = (2, 3, 0, false),
        ["Chaquetas"] = (1, 2, 2, true), ["Blazers"] = (2, 3, 1, true), ["Abrigos"] = (1, 3, 2, true),
    };

    public static readonly List<TipoPlan> Planes = new()
    {
        // Los patrones se comparan con el texto en minúsculas y sin tildes (MotorIA.Normalizar)
        new("uni", "Universidad", 1, @"\buni\b|universidad|clase|facultad|examen|biblio|estudi|tutoria|insti", "ir a clase"),
        new("trabajo", "Trabajo", 2, @"trabaj|oficina|reunion|entrevista|curro|practicas en", "trabajar"),
        new("cena", "Cena", 2, @"cena|comida|comer fuera|restaurante|\bcita\b|tapas|brunch", "una cena"),
        new("fiesta", "Fiesta", 3, @"fiesta|discoteca|cumple|boda|concierto|festival|copas|de marcha|salir de noche|salir por la noche", "una fiesta"),
        new("deporte", "Deporte", 0, @"gimnasio|\bgym\b|deporte|correr|running|entren|padel|tenis|futbol|baloncesto|basket|voley|natacion|nadar|piscina|yoga|pilates|spinning|crossfit|zumba|\bbici|ciclismo|senderismo|\bruta\b|caminar|pesas|boxeo|escalada|partido|baile|danza", "hacer deporte"),
        new("casa", "Sin planes", 1, @"en casa|\bnada\b|descans|recados|sin planes|compras", "un día tranquilo"),
    };

    public static readonly string[] Estilos = { "Casual", "Minimalista", "Clásico", "Deportivo" };
    public static readonly string[] Gustos = { "Vaqueros", "Vestidos", "Chándal", "Zapatillas" };

    // ---------------------------------------------------------------- Explorar
    public static readonly List<Persona> Personas = new()
    {
        new("lucia", "Lucía", "Estudiante de Bellas Artes. Capas, tonos neutros y botas todo el otoño.", new[] { "Casual", "Clásico" }, 1240, 310),
        new("marta", "Marta", "Trabajo en una oficina y busco looks cómodos que no parezcan cómodos.", new[] { "Minimalista", "Clásico" }, 860, 152),
        new("andrea", "Andrea", "Clase por la mañana, biblioteca por la tarde. Zapatillas siempre.", new[] { "Casual", "Deportivo" }, 2030, 420),
        new("carla", "Carla", "Entreno casi todos los días y me gusta ir cómoda también fuera del gimnasio.", new[] { "Deportivo" }, 540, 98),
    };

    public static readonly List<Publicacion> Publicaciones = new()
    {
        new("lucia", "lucia", "hace 2 h", "Outfit para una tarde de otoño", "Jersey gris, camisa azul y vaqueros claros.", "look_lucia", new[] { "jersey_gris", "camisa_azul", "vaquero_claro", "botas_negras" }, 124),
        new("marta", "marta", "hace 5 h", "Look de oficina", "Blazer con top de punto y falda. Fácil y elegante.", null, new[] { "blazer_negro", "top_punto_gris", "falda_negra", "tacones_negros" }, 87),
        new("andrea", "andrea", "ayer", "Para ir a clase", "Cómodo para todo el día: chaqueta de punto, cargo y zapatillas.", null, new[] { "jersey_negro", "camiseta_blanca", "cargo_verde", "zapatillas" }, 203),
        new("carla1", "carla", "ayer", "Del gimnasio a tomar algo", "Sudadera y chándal grises con zapatillas blancas. Cómodo y apañado.", null, new[] { "sudadera_gris", "chandal_gris", "zapatillas" }, 64),
        new("marta2", "marta", "hace 2 días", "Cena de viernes", "Top rojo con falda negra y tacones. Con el blazer por si refresca.", null, new[] { "top_rojo", "falda_negra", "tacones_negros", "blazer_negro" }, 152),
        new("lucia2", "lucia", "hace 3 días", "Básicos que siempre funcionan", "Camiseta blanca, vaquero azul y botas. No falla.", null, new[] { "camiseta_blanca", "vaquero_azul", "botas_negras" }, 98),
        new("andrea2", "andrea", "hace 4 días", "Día de biblioteca", "Jersey gordito y vaquero claro para estar a gusto muchas horas.", null, new[] { "jersey_gris", "vaquero_claro", "zapatillas" }, 77),
    };

    public static readonly Dictionary<string, ComentarioEjemplo[]> ComentariosEjemplo = new()
    {
        ["lucia"] = new ComentarioEjemplo[] { new("marta", "Me encanta la camisa debajo del jersey", "hace 1 h"), new("andrea", "¿De dónde son las botas?", "hace 40 min") },
        ["marta"] = new ComentarioEjemplo[] { new("lucia", "Elegantísima y nada incómodo", "hace 3 h") },
        ["andrea"] = new ComentarioEjemplo[] { new("carla", "Ese cargo combina con todo", "ayer"), new("lucia", "Copio este para el lunes", "ayer") },
        ["carla1"] = new ComentarioEjemplo[] { new("andrea", "Uniforme oficial de los domingos", "ayer") },
        ["marta2"] = new ComentarioEjemplo[] { new("lucia", "El rojo te queda genial", "hace 2 días") },
    };

    public static Persona? PersonaDe(string usuario) => Personas.FirstOrDefault(p => p.Usuario == usuario);
    public static Publicacion? PublicacionDe(string id) => Publicaciones.FirstOrDefault(p => p.Id == id);
    public static ComentarioEjemplo[] ComentariosDe(string id) => ComentariosEjemplo.TryGetValue(id, out var c) ? c : Array.Empty<ComentarioEjemplo>();

    /// <summary>Las prendas de una publicación (aunque no estén en tu armario).</summary>
    public static List<Prenda> PrendasDe(Publicacion p)
    {
        var catalogo = Catalogo();
        return p.Ids.Select(id => catalogo.FirstOrDefault(x => x.Id == id)).Where(x => x != null).Select(x => x!).ToList();
    }

    /// <summary>Todas las prendas conocidas (para pintar los looks de Explorar aunque no estén en tu armario).</summary>
    public static List<Prenda> Catalogo() => Armario().Concat(Etiquetas()).ToList();
}
