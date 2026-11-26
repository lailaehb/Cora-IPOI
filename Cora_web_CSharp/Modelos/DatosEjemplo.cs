namespace Cora.Modelos;

/// <summary>Datos de ejemplo: el armario de Pilar, las etiquetas de prueba, los planes y Explorar.</summary>
public static class DatosEjemplo
{
    static Prenda P(string id, string n, string m, Categoria c, string sub, string color, string talla, int fmin, int fmax, int abrigo = 1, bool lluvia = true, bool lavando = false) =>
        new() { Id = id, Nombre = n, Marca = m, Cat = c, Sub = sub, Color = color, Talla = talla, FormMin = fmin, FormMax = fmax, Abrigo = abrigo, AptaLluvia = lluvia, Lavando = lavando };

    public static List<Prenda> Armario() => new List<Prenda>
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
    }.Concat(Nuevas()).ToList();

    /// <summary>
    /// Prendas añadidas en la segunda tanda de fotos (más variedad). Van aparte para poder añadirlas
    /// también a los armarios que ya estaban guardados en el navegador (ver Estado.CargarAsync).
    /// </summary>
    public const int VersionEjemplos = 3;
    public static List<Prenda> Nuevas() => new()
    {
        P("top_halter_negro", "Top halter satinado negro", "Zara", Categoria.Arriba, "Tops", "negro", "S", 2, 3, abrigo: 0),
        P("top_halter_caqui", "Top halter satinado caqui", "Zara", Categoria.Arriba, "Tops", "caqui", "S", 2, 3, abrigo: 0),
        P("body_burdeos", "Body plisado burdeos", "Zara", Categoria.Arriba, "Tops", "burdeos", "S", 2, 3, abrigo: 0),
        P("top_peplum_rojo", "Top palabra de honor rojo", "Zara", Categoria.Arriba, "Tops", "rojo", "S", 2, 3, abrigo: 0),
        P("top_asimetrico_negro", "Top asimétrico drapeado", "Zara", Categoria.Arriba, "Tops", "negro", "M", 1, 3, abrigo: 1),
        P("camiseta_volantes_burdeos", "Camiseta de punto con volantes", "Zara", Categoria.Arriba, "Camisetas", "burdeos", "M", 1, 2, abrigo: 1),
        P("vaquero_negro", "Vaquero recto negro", "Zara", Categoria.Abajo, "Vaqueros", "negro", "38", 1, 2),
        P("vaquero_ancho_azul", "Vaquero ancho azul", "Zara", Categoria.Abajo, "Vaqueros", "azul", "38", 1, 2),
        P("vaquero_crudo", "Vaquero recto crudo", "Zara", Categoria.Abajo, "Vaqueros", "crudo", "38", 1, 2),
        P("chaqueta_borrego_marron", "Chaqueta de ante con borrego", "Zara", Categoria.Capa, "Chaquetas", "marrón", "M", 1, 2, abrigo: 2),
        P("chaqueta_peplum_caqui", "Chaqueta peplum caqui", "Zara", Categoria.Capa, "Chaquetas", "caqui", "M", 1, 3, abrigo: 2),
        P("chaqueta_piel_negra", "Chaqueta de piel con cinturón", "Zara", Categoria.Capa, "Chaquetas", "negro", "M", 1, 3, abrigo: 1),
        // Tercera tanda: ropa cómoda y de punto
        P("jersey_amarillo", "Jersey amarillo pastel", "Zara", Categoria.Arriba, "Jerséis", "amarillo", "M", 1, 2, abrigo: 2),
        P("jersey_pico_marino", "Jersey de pico azul marino", "Zara", Categoria.Arriba, "Jerséis", "azul marino", "M", 1, 2, abrigo: 2),
        P("polo_rayas_burdeos", "Jersey polo de rayas", "Zara", Categoria.Arriba, "Jerséis", "burdeos", "M", 1, 2, abrigo: 2),
        P("camiseta_larga_gris", "Camiseta de manga larga gris", "Zara", Categoria.Arriba, "Camisetas", "gris", "M", 0, 2, abrigo: 1),
        P("sudadera_cremallera_coral", "Sudadera con cremallera coral", "Zara", Categoria.Arriba, "Sudaderas", "coral", "M", 0, 1, abrigo: 1),
        P("jogger_caqui", "Jogger caqui", "Zara", Categoria.Abajo, "Pantalones", "caqui", "M", 0, 1),
        P("jogger_coral", "Jogger coral", "Zara", Categoria.Abajo, "Pantalones", "coral", "M", 0, 1),
        P("pantalon_ancho_gris", "Pantalón ancho de felpa gris", "Zara", Categoria.Abajo, "Pantalones", "gris", "M", 0, 1),
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

    /// <summary>
    /// Tipos de plan que entienden las reglas de Cora en lo que escribes o dices (con tus palabras, como se lo contarías a una amiga).
    /// Formalidad: 0 deporte · 1 diario · 2 arreglado · 3 fiesta. «Deporte» y «Baile» llevan un conjunto aparte (hay que cambiarse).
    /// Los patrones se comparan con el texto en minúsculas y sin tildes (MotorIA.Normalizar; la ñ se queda como ñ).
    /// </summary>
    public static readonly List<TipoPlan> Planes = new()
    {
        new("uni", "Universidad", 1, @"\buni\b|universidad|\bclases?\b(?! de (?:baile|flamenco|sevillanas|zumba|yoga|pilates|danza|spinning|natacion|canto|musica|guitarra|piano))|facultad|examen|biblio|estudi|tutoria|insti|\bclass\b|universit|college|lecture|exam|study|library|\bcours\b|\bfac\b|bibliotheque|etudi", "ir a clase"),
        new("trabajo", "Trabajo", 2, @"trabaj|oficina|reunion|entrevista|curro|practicas en|\bwork|office|meeting|interview|\bjob\b|travail|bureau|boulot|entretien", "trabajar"),
        new("cena", "Cena", 2, @"\bcena|\bcenar\b|comida|almuerzo|comer fuera|comer en un|restaurante|\bcita\b|tapas|tapeo|brunch|dinner|lunch|restaurant|\bdate\b|diner|dejeuner|rendez-vous", "una comida o cena"),
        new("fiesta", "Fiesta", 3, @"fiesta|discoteca|disco\b|cumple|boda|festival|feria|copas|de marcha|de fiesta|salir de noche|salir por la noche|salir a bailar|ir a bailar|nochevieja|party|club|birthday|wedding|drinks|night out|fete|soiree|anniversaire|mariage|boite", "una fiesta"),
        new("evento", "Evento", 2, @"evento|actuacion|actuar|espectaculo|teatro|musical|concierto|opera|ballet|gala|graduacion|comunion|bautizo|presentacion|inauguracion|show\b|concert|theatre|theater|spectacle", "un evento o espectáculo"),
        new("deporte", "Deporte", 0, @"gimnasio|\bgym\b|deporte|correr|running|entren|padel|tenis|(?<!\b(?:ver|veo|vemos|mirar) (?:el |un |al |una |los )?(?:partido de )?)(?<!\bir al )\b(?:futbol|partido|baloncesto|basket|voley)\b|natacion|nadar|yoga|pilates|spinning|crossfit|\bbici|ciclismo|senderismo|\bruta\b|caminar|pesas|boxeo|escalada|workout|\brun\b|training|swim|football|soccer|hike|hiking|entrainement|natation|randonnee|velo|musculation", "hacer deporte"),
        new("baile", "Baile", 0, @"flamenco|sevillanas|(?<!salir a |ir a )bailar|\bbaile\b|danza|zumba|clase de baile|ensayo|dance class|danse", "ir a baile"),
        new("partido", "Ver un partido", 1, @"\b(?:ver|veo|vemos|mirar) (?:el |un |al |una |los )?(?:partido|futbol|baloncesto|basket|carrera|formula)|\bir al (?:futbol|estadio|campo)|estadio|campo de futbol|watch (?:the |a )?(?:game|match)", "ver un partido"),
        new("campo", "Campo", 1, @"\bcampo\b(?! de futbol)|parcela|finca|chalet|casa de campo|barbacoa|piscina|huerto|\bmonte\b|picnic|petunias|countryside|campagne", "un día en el campo"),
        new("viaje", "Viaje", 1, @"viaje|viajar|playa|turismo|excursion|escapada|vacaciones|monta[nñ]a|camping|apartamento|hotel|cordoba|sevilla|granada|malaga|cadiz|madrid|valencia|barcelona|\bpueblo\b|crucero|avion|\btren\b|\btrip\b|travel|beach|holiday|vacation|sightseeing|voyage|plage|vacances", "un día de viaje"),
        new("familia", "Familia y pareja", 1, @"familia|novio|novia|pareja|suegr|abuel|\bprimos?\b|\bprimas?\b|\btios\b|\btias?\b|sobrin|cu[nñ]ad|boyfriend|girlfriend|family|copain|copine|famille", "estar con la familia o la pareja"),
        new("amigos", "Amigos", 1, @"amig[oa]s?|quedada|quedar con|colegas|friends|amis\b", "quedar con amigos"),
        new("paseo", "Paseo y parque", 1, @"pasear|paseo|parque|\bbanco\b|pipas|charlar|hablar con|cotillear|jugar con|ni[nñ]os|ni[nñ]as|peques|columpio|\bplaza\b|terraza|heladeria|walk|park|balade", "un rato por la calle"),
        new("ocio", "Ocio", 1, @"cantar|\bcoro\b|karaoke|\bcine\b|peli|bolos|museo|exposicion|juegos de mesa|escape room|videojuegos|leer|libreria|cinema|movie|karaoke", "un plan de ocio"),
        new("recados", "Recados", 1, @"recados|compras|comprar|de tiendas|centro comercial|supermercado|\bsuper\b|mercado|u[nñ]as|manicura|pedicura|peluqueria|\bpelu\b|estetica|depil|dentista|medico|farmacia|gestion|errands|shopping|groceries|courses|coiffeur", "hacer recados"),
        new("casa", "Sin planes", 1, @"en casa|quedarme en casa|\bnada\b|descans|sin planes|tranquil|sofa|at home|nothing|\brest\b|maison|\brien\b|repos", "un día tranquilo"),
    };

    /// <summary>
    /// Looks que se eligen al registrarse (y en Configuración). Cada uno es un conjunto real hecho con prendas del armario
    /// de ejemplo, y trae los tipos de prenda que más le pegan: así lo que eliges sirve de verdad para que Cora acierte.
    /// </summary>
    public static readonly List<Estilo> Looks = new()
    {
        new("Casual", "Vaqueros, camisetas y zapatillas", new[] { "camiseta_blanca", "vaquero_azul", "zapatillas", "chaqueta_borrego_marron" }, new[] { "Vaqueros", "Camisetas", "Zapatillas" }),
        new("Minimalista", "Básicos en negro, blanco y gris", new[] { "top_punto_gris", "vaquero_negro", "zapatillas", "chaqueta_piel_negra" }, new[] { "Tops", "Vaqueros", "Chaquetas" }),
        new("Elegante", "Blazer, falda y tacones", new[] { "top_asimetrico_negro", "falda_negra", "tacones_negros", "blazer_negro" }, new[] { "Blazers", "Faldas", "Tacones" }),
        new("Cómodo", "Punto y pantalones anchos", new[] { "jersey_amarillo", "pantalon_ancho_gris", "zapatillas", "jersey_negro" }, new[] { "Jerséis", "Pantalones" }),
        new("Deportivo", "Sudaderas, joggers y deportivas", new[] { "sudadera_cremallera_coral", "jogger_caqui", "zapatillas", "camiseta_larga_gris" }, new[] { "Sudaderas", "Pantalones", "Zapatillas" }),
        new("De noche", "Tops satinados y tacones", new[] { "top_halter_negro", "vaquero_negro", "tacones_negros", "chaqueta_piel_negra" }, new[] { "Tops", "Tacones" }),
    };
    public static readonly string[] Estilos = Looks.Select(l => l.Nombre).ToArray();

    /// <summary>Lo que te gusta llevar (tipos de prenda), sacado de los looks que has elegido.</summary>
    public static List<string> GustosDe(IEnumerable<string> estilos) =>
        Looks.Where(l => estilos.Contains(l.Nombre)).SelectMany(l => l.Tipos).Distinct().ToList();

    // ---------------------------------------------------------------- Explorar
    public static readonly List<Persona> Personas = new()
    {
        new("lucia", "Lucía", "Estudiante de Bellas Artes. Vaqueros, botas y prendas con aire setentero.", new[] { "Casual", "Clásico" }, 1240, 310),
        new("marta", "Marta", "Trabajo en una oficina. Looks sencillos para el día y algo especial para salir de noche.", new[] { "Minimalista", "Clásico" }, 860, 152),
        new("andrea", "Andrea", "Estudiante. Clase por la mañana, biblioteca por la tarde y zapatillas siempre.", new[] { "Casual", "Deportivo" }, 2030, 420),
        new("carla", "Carla", "Entreno casi todos los días y me gusta ir cómoda también fuera del gimnasio.", new[] { "Deportivo" }, 540, 98),
        new("pablo", "Pablo", "Estudio Ingeniería. Básicos, jerséis de punto y zapatillas cómodas para todo el día.", new[] { "Casual", "Minimalista" }, 980, 205),
        new("javi", "Javi", "Me encantan las rayas y los colores tierra. Entre semana, sencillo; el finde, al monte.", new[] { "Clásico", "Casual" }, 1410, 260),
        new("dani", "Dani", "Gimnasio por la mañana y clase por la tarde. Si es cómodo, me lo pongo, haga el frío que haga.", new[] { "Deportivo", "Casual" }, 720, 180),
    };

    public static readonly List<Publicacion> Publicaciones = new()
    {
        new("dani3", "dani", "hace 1 h", "Para correr con frío", "Chaqueta de running, manga larga y pantalón corto. Esta mañana hacía 8 grados.", null, new[] { "chaqueta_running_negra", "camiseta_larga_gris_chico", "short_deporte_malva", "zapatillas_retro_blancas" }, 54),
        new("lucia", "lucia", "hace 2 h", "Outfit para una tarde de otoño", "Jersey gris, camisa azul y vaqueros claros.", null, new[] { "jersey_gris", "camisa_azul", "vaquero_claro", "botas_negras" }, 124),
        new("pablo1", "pablo", "hace 3 h", "Punto y tonos tierra", "Jersey marrón, pantalón recto y zapatillas de ante. Para clase y para después.", null, new[] { "jersey_marron_chico", "camiseta_blanca_chico", "pantalon_recto_gris", "zapatillas_ante_caqui" }, 143),
        new("marta", "marta", "hace 5 h", "Look de oficina", "Blazer con top de punto y falda. Fácil y elegante.", null, new[] { "blazer_negro", "top_punto_gris", "falda_negra", "tacones_negros" }, 87),
        new("dani1", "dani", "hace 8 h", "Comodidad total", "Jersey de cremallera y jogger. Del gimnasio a la biblioteca sin cambiarme.", null, new[] { "jersey_cremallera_amarillo", "jogger_gris_oscuro", "zapatillas_retro_blancas" }, 88),
        new("andrea", "andrea", "ayer", "Para ir a clase", "Cómodo para todo el día: chaqueta de punto, cargo y zapatillas.", null, new[] { "jersey_negro", "camiseta_blanca", "cargo_verde", "zapatillas" }, 203),
        new("carla1", "carla", "ayer", "Del gimnasio a tomar algo", "Sudadera y chándal grises con zapatillas blancas. Cómodo y apañado.", null, new[] { "sudadera_gris", "chandal_gris", "zapatillas" }, 64),
        new("javi1", "javi", "ayer", "Polo de rayas para cenar", "Un polo de punto con pantalón recto. Arreglado sin pasarse.", null, new[] { "polo_rayas_marino", "pantalon_recto_gris", "zapatillas_retro_blancas" }, 176),
        new("marta2", "marta", "hace 2 días", "Cena de viernes", "Top rojo con falda negra y tacones. Con el blazer por si refresca.", null, new[] { "top_rojo", "falda_negra", "tacones_negros", "blazer_negro" }, 152),
        new("pablo2", "pablo", "hace 2 días", "Rayas de domingo", "Camiseta de rayas y cargo para un día tranquilo.", null, new[] { "camiseta_rayas_verde", "cargo_caqui_chico", "zapatillas_ante_caqui" }, 92),
        new("lucia2", "lucia", "hace 3 días", "Básicos que siempre funcionan", "Camiseta blanca, vaquero azul y botas. No falla.", null, new[] { "camiseta_blanca", "vaquero_azul", "botas_negras" }, 98),
        new("pablo3", "pablo", "hace 3 días", "Día de lluvia", "Chaqueta impermeable y jogger. Que llueva lo que quiera.", null, new[] { "chaqueta_impermeable_lima", "camiseta_blanca_chico", "jogger_marino", "zapatillas_retro_blancas" }, 118),
        new("andrea2", "andrea", "hace 4 días", "Día de biblioteca", "Jersey gordito y vaquero claro para estar a gusto muchas horas.", null, new[] { "jersey_gris", "vaquero_claro", "zapatillas" }, 77),
        new("dani2", "dani", "hace 4 días", "Oversize", "Camiseta oversize granate con jogger gris. Mi uniforme de entreno.", null, new[] { "camiseta_oversize_granate", "jogger_gris_oscuro", "zapatillas_retro_blancas" }, 61),
        new("lucia3", "lucia", "hace 5 días", "Otoño de los 70", "Chaqueta de ante con borrego, vaquero ancho y botas. Mi favorito del año.", null, new[] { "chaqueta_borrego_marron", "camiseta_volantes_burdeos", "vaquero_ancho_azul", "botas_negras" }, 311),
        new("carla2", "carla", "hace 5 días", "Conjunto coral", "Sudadera y jogger a juego. Para ir al gimnasio y luego a por un café.", null, new[] { "sudadera_cremallera_coral", "camiseta_larga_gris", "jogger_coral", "zapatillas" }, 96),
        new("javi3", "javi", "hace 5 días", "Invierno de verdad", "Plumífero encima de rayas y pantalón recto. Abrigado y sin perder estilo.", null, new[] { "plumifero_negro", "camiseta_rayas_azul", "pantalon_recto_gris", "zapatillas_ante_caqui" }, 207),
        new("marta3", "marta", "hace 6 días", "Noche de cumpleaños", "Top halter negro de satén con vaquero negro y tacones. Sencillo y elegante.", null, new[] { "top_halter_negro", "vaquero_negro", "tacones_negros", "chaqueta_piel_negra" }, 189),
        new("javi2", "javi", "hace 6 días", "Básicos de rayas", "Camiseta de rayas azul, cargo y zapatillas. Fácil.", null, new[] { "camiseta_rayas_azul", "cargo_caqui_chico", "zapatillas_ante_caqui" }, 104),
        new("andrea3", "andrea", "hace 1 semana", "Crudo y caqui", "La chaqueta peplum con el vaquero crudo queda genial para ir a clase.", null, new[] { "chaqueta_peplum_caqui", "top_asimetrico_negro", "vaquero_crudo", "zapatillas" }, 142),
        new("dani4", "dani", "hace 1 semana", "Entreno en el parque", "Polar gris y pantalón técnico. Calentito y cómodo para moverse.", null, new[] { "chaqueta_polar_gris", "camiseta_oversize_granate", "pantalon_tecnico_negro", "zapatillas_retro_blancas" }, 73),
        new("javi4", "javi", "hace 1 semana", "Monte el sábado", "Chaqueta técnica para la ruta. Ligera y no se moja.", null, new[] { "chaqueta_tecnica_negra", "camiseta_larga_gris_chico", "jogger_gris_oscuro", "zapatillas_ante_caqui" }, 95),
    };

    public static readonly Dictionary<string, ComentarioEjemplo[]> ComentariosEjemplo = new()
    {
        ["lucia"] = new ComentarioEjemplo[] { new("marta", "Me encanta la camisa debajo del jersey", "hace 1 h"), new("andrea", "¿De dónde son las botas?", "hace 40 min") },
        ["marta"] = new ComentarioEjemplo[] { new("lucia", "Elegantísima y nada incómodo", "hace 3 h") },
        ["andrea"] = new ComentarioEjemplo[] { new("carla", "Ese cargo combina con todo", "ayer"), new("lucia", "Copio este para el lunes", "ayer") },
        ["carla1"] = new ComentarioEjemplo[] { new("andrea", "Uniforme oficial de los domingos", "ayer") },
        ["pablo1"] = new ComentarioEjemplo[] { new("javi", "Esas zapatillas de ante son top", "hace 2 h"), new("lucia", "Muy buen combo de colores", "hace 1 h") },
        ["javi1"] = new ComentarioEjemplo[] { new("dani", "Ese polo lo quiero", "ayer") },
        ["dani1"] = new ComentarioEjemplo[] { new("carla", "El amarillo te queda genial", "hace 5 h") },
        ["dani3"] = new ComentarioEjemplo[] { new("carla", "¡Qué valor con 8 grados!", "hace 30 min") },
        ["javi3"] = new ComentarioEjemplo[] { new("pablo", "Ese plumífero tiene pinta de abrigar muchísimo", "hace 4 días"), new("marta", "Muy buena combinación", "hace 4 días") },
        ["marta2"] = new ComentarioEjemplo[] { new("lucia", "El rojo te queda genial", "hace 2 días") },
    };

    public static Persona? PersonaDe(string usuario) => Personas.FirstOrDefault(p => p.Usuario == usuario);

    /// <summary>A quién sigue cada persona de ejemplo (dentro de Cora). Los seguidores salen de aquí al revés.</summary>
    static readonly Dictionary<string, string[]> SigueA = new()
    {
        ["lucia"] = new[] { "marta", "andrea", "javi" },
        ["marta"] = new[] { "lucia", "carla", "pablo" },
        ["andrea"] = new[] { "lucia", "carla", "dani", "pablo" },
        ["carla"] = new[] { "andrea", "dani" },
        ["pablo"] = new[] { "javi", "dani", "lucia" },
        ["javi"] = new[] { "pablo", "marta" },
        ["dani"] = new[] { "carla", "pablo", "andrea" },
    };
    public static List<Persona> SiguiendoDe(string usuario) =>
        (SigueA.TryGetValue(usuario, out var l) ? l : Array.Empty<string>()).Select(PersonaDe).Where(p => p != null).Select(p => p!).ToList();
    public static List<Persona> SeguidoresDe(string usuario) =>
        Personas.Where(p => SigueA.TryGetValue(p.Usuario, out var l) && l.Contains(usuario)).ToList();
    public static Publicacion? PublicacionDe(string id) => Publicaciones.FirstOrDefault(p => p.Id == id);
    public static ComentarioEjemplo[] ComentariosDe(string id) => ComentariosEjemplo.TryGetValue(id, out var c) ? c : Array.Empty<ComentarioEjemplo>();

    /// <summary>Las prendas de una publicación (aunque no estén en tu armario).</summary>
    public static List<Prenda> PrendasDe(Publicacion p)
    {
        var catalogo = Catalogo();
        return p.Ids.Select(id => catalogo.FirstOrDefault(x => x.Id == id)).Where(x => x != null).Select(x => x!).ToList();
    }

    /// <summary>Todas las prendas conocidas (para pintar los looks de Explorar aunque no estén en tu armario).</summary>
    public static List<Prenda> Catalogo() => Armario().Concat(Etiquetas()).Concat(Chicos()).ToList();

    /// <summary>Ropa de chico de las publicaciones de Explorar (Pablo, Javi y Dani). No va a tu armario.</summary>
    public static List<Prenda> Chicos() => new()
    {
        P("jersey_marron_chico", "Jersey de punto marrón", "Zara", Categoria.Arriba, "Jerséis", "marrón", "L", 1, 2, abrigo: 2),
        P("jersey_cremallera_amarillo", "Jersey con cremallera amarillo", "Zara", Categoria.Arriba, "Jerséis", "amarillo", "L", 1, 2, abrigo: 2),
        P("polo_rayas_marino", "Polo de punto de rayas", "Zara", Categoria.Arriba, "Jerséis", "azul marino", "L", 1, 2, abrigo: 1),
        P("camiseta_blanca_chico", "Camiseta blanca básica", "Zara", Categoria.Arriba, "Camisetas", "blanco", "L", 0, 1, abrigo: 0),
        P("camiseta_rayas_azul", "Camiseta de rayas azul", "Zara", Categoria.Arriba, "Camisetas", "azul", "L", 0, 1, abrigo: 1),
        P("camiseta_rayas_verde", "Camiseta de rayas verde", "Zara", Categoria.Arriba, "Camisetas", "verde", "L", 0, 1, abrigo: 1),
        P("camiseta_oversize_granate", "Camiseta oversize granate", "Zara", Categoria.Arriba, "Camisetas", "granate", "L", 0, 1, abrigo: 0),
        P("jogger_gris_oscuro", "Jogger gris oscuro", "Zara", Categoria.Abajo, "Pantalones", "gris", "L", 0, 1),
        P("cargo_caqui_chico", "Pantalón cargo jogger", "Zara", Categoria.Abajo, "Pantalones", "caqui", "L", 0, 1),
        P("pantalon_recto_gris", "Pantalón recto gris azulado", "Zara", Categoria.Abajo, "Pantalones", "gris", "L", 1, 2),
        P("zapatillas_retro_blancas", "Zapatillas retro blancas", "Zara", Categoria.Calzado, "Zapatillas", "blanco", "43", 0, 1, abrigo: 0),
        P("zapatillas_ante_caqui", "Zapatillas de ante caqui", "Zara", Categoria.Calzado, "Zapatillas", "caqui", "43", 0, 2, abrigo: 0, lluvia: false),
        // Deporte y abrigo
        P("camiseta_larga_gris_chico", "Camiseta de manga larga gris lavado", "Zara", Categoria.Arriba, "Camisetas", "gris", "L", 0, 1, abrigo: 1),
        P("short_deporte_malva", "Pantalón corto de deporte", "Zara", Categoria.Abajo, "Shorts", "malva", "L", 0, 0, abrigo: 0),
        P("jogger_marino", "Jogger azul marino", "Zara", Categoria.Abajo, "Pantalones", "azul marino", "L", 0, 1),
        P("pantalon_tecnico_negro", "Pantalón técnico negro", "Zara", Categoria.Abajo, "Pantalones", "negro", "L", 0, 1),
        P("plumifero_negro", "Plumífero negro", "Zara", Categoria.Capa, "Abrigos", "negro", "L", 0, 2, abrigo: 2),
        P("chaqueta_polar_gris", "Chaqueta polar gris", "Zara", Categoria.Capa, "Chaquetas", "gris", "L", 0, 1, abrigo: 1),
        P("chaqueta_tecnica_negra", "Chaqueta técnica con capucha", "Zara", Categoria.Capa, "Chaquetas", "negro", "L", 0, 1, abrigo: 2),
        P("chaqueta_running_negra", "Chaqueta de running negra", "Zara", Categoria.Capa, "Chaquetas", "negro", "L", 0, 0, abrigo: 1),
        P("chaqueta_impermeable_lima", "Chaqueta impermeable lima", "Zara", Categoria.Capa, "Chaquetas", "lima", "L", 0, 1, abrigo: 1),
    };
}

/// <summary>Un look para elegir al registrarse: nombre, frase corta, sus 4 prendas (fotos) y los tipos de prenda que le van.</summary>
public record Estilo(string Nombre, string Frase, string[] Ids, string[] Tipos);
