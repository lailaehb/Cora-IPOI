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
        [Categoria.Arriba] = new[] { "Camisetas", "Tops", "Camisas", "Sudaderas", "Jerséis" },
        [Categoria.Abajo] = new[] { "Vaqueros", "Pantalones", "Faldas" },
        [Categoria.Calzado] = new[] { "Zapatillas", "Botas", "Tacones" },
        [Categoria.Capa] = new[] { "Chaquetas", "Blazers", "Abrigos" },
    };

    /// <summary>Valores por defecto para prendas nuevas: (formalidad mínima, máxima, abrigo, apta para lluvia).</summary>
    public static readonly Dictionary<string, (int Min, int Max, int Abrigo, bool Lluvia)> PorDefecto = new()
    {
        ["Camisetas"] = (0, 1, 0, true), ["Tops"] = (1, 2, 0, true), ["Camisas"] = (1, 2, 0, true), ["Sudaderas"] = (0, 1, 1, true), ["Jerséis"] = (1, 2, 2, true),
        ["Vaqueros"] = (1, 2, 1, true), ["Pantalones"] = (1, 2, 1, true), ["Faldas"] = (2, 3, 0, true),
        ["Zapatillas"] = (0, 1, 0, true), ["Botas"] = (1, 3, 0, true), ["Tacones"] = (2, 3, 0, false),
        ["Chaquetas"] = (1, 2, 2, true), ["Blazers"] = (2, 3, 1, true), ["Abrigos"] = (1, 3, 2, true),
    };

    public static readonly List<TipoPlan> Planes = new()
    {
        new("uni", "Universidad", 1, "uni|clase|facultad|examen|biblio", "ir a clase"),
        new("trabajo", "Trabajo", 2, "trabaj|oficina|reuni|entrevista|práctica|practica", "trabajar"),
        new("cena", "Cena", 2, "cena|comida|restaurante|cita", "una cena"),
        new("fiesta", "Fiesta", 3, "fiesta|discoteca|cumple|boda|concierto|salir", "una fiesta"),
        new("deporte", "Deporte", 0, "gimnasio|gym|deporte|correr|entren|pádel|padel|yoga", "hacer deporte"),
        new("casa", "Sin planes", 1, "casa|nada|descanso|recados|sin planes", "un día tranquilo"),
    };

    public static readonly string[] Estilos = { "Casual", "Minimalista", "Clásico", "Deportivo" };
    public static readonly string[] Gustos = { "Vaqueros", "Vestidos", "Chándal", "Zapatillas" };

    public static readonly List<Publicacion> Publicaciones = new()
    {
        new("lucia", "@lucia", "hace 2 h", "Outfit para una tarde de otoño", "Jersey gris, camisa azul y vaqueros claros.", "look_lucia", new[] { "jersey_gris", "camisa_azul", "vaquero_claro", "botas_negras" }, 124, 12),
        new("marta", "@marta", "hace 5 h", "Look de oficina", "Blazer con top de punto y falda. Fácil y elegante.", null, new[] { "blazer_negro", "top_punto_gris", "falda_negra", "tacones_negros" }, 87, 6),
        new("andrea", "@andrea", "ayer", "Para ir a clase", "Cómodo para todo el día: chaqueta de punto, cargo y zapatillas.", null, new[] { "jersey_negro", "camiseta_blanca", "cargo_verde", "zapatillas" }, 203, 18),
    };

    /// <summary>Todas las prendas conocidas (para pintar los looks de Explorar aunque no estén en tu armario).</summary>
    public static List<Prenda> Catalogo() => Armario().Concat(Etiquetas()).ToList();
}
