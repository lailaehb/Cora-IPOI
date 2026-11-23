using System.Text.Json.Serialization;

namespace Cora.Modelos;

/// <summary>Categorías del armario.</summary>
public enum Categoria { Arriba, Abajo, Calzado, Capa }

/// <summary>Una prenda del armario.</summary>
public class Prenda
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Marca { get; set; } = "";
    public Categoria Cat { get; set; }
    /// <summary>Subcategoría: Camisetas, Vaqueros, Botas…</summary>
    public string Sub { get; set; } = "";
    public string Color { get; set; } = "";
    public string Talla { get; set; } = "";
    /// <summary>Formalidad que cubre la prenda: 0 deporte · 1 informal · 2 arreglado · 3 fiesta.</summary>
    public int FormMin { get; set; }
    public int FormMax { get; set; }
    /// <summary>0 fresco · 1 medio · 2 abriga.</summary>
    public int Abrigo { get; set; }
    /// <summary>Calzado apto para lluvia.</summary>
    public bool AptaLluvia { get; set; } = true;
    public bool Lavando { get; set; }
    /// <summary>Fecha (AAAA-MM-DD) en que se echó a lavar.</summary>
    public string? LavadoDesde { get; set; }
    /// <summary>Momento exacto (UTC, formato ISO) en que se echó a lavar: la barra avanza con las horas reales.</summary>
    public string? LavadoHora { get; set; }
    /// <summary>Ya se avisó de que está seca (para no avisar dos veces).</summary>
    public bool AvisoSeca { get; set; }
    /// <summary>Está sucia en el cesto, esperando a que el usuario ponga la lavadora.</summary>
    public bool EnCesto { get; set; }
    /// <summary>Días que el usuario eligió al poner la lavadora (mínimo 2).</summary>
    public int? LavadoDias { get; set; }
    /// <summary>En el cesto o en la lavadora: Cora no la usa en los conjuntos.</summary>
    [JsonIgnore] public bool NoDisponible => Lavando || EnCesto;
    /// <summary>Foto hecha por el usuario (data URL). Si es null se usa la foto de ejemplo.</summary>
    public string? Foto { get; set; }

    public bool Encaja(int formalidad) => FormMin <= formalidad && formalidad <= FormMax;
    [JsonIgnore] public string Imagen => Foto ?? $"img/{Id}.png";
    public Prenda Copia() => (Prenda)MemberwiseClone();
}

/// <summary>Planes de un día: los botones marcados y el texto libre.</summary>
public class PlanDia
{
    public List<string> Claves { get; set; } = new();
    public string Texto { get; set; } = "";
}

/// <summary>Conjunto propuesto por la IA para un día.</summary>
public class Outfit
{
    /// <summary>Capa, arriba, abajo y calzado.</summary>
    public List<string> Ids { get; set; } = new();
    /// <summary>Explicación en lenguaje natural.</summary>
    public string Porque { get; set; } = "";
    /// <summary>Combinación principal (arriba + abajo) para no repetir.</summary>
    public string Clave { get; set; } = "";
    public int Semilla { get; set; }
    /// <summary>Para qué momento es el conjunto principal, si hay más de uno ese día (p. ej. «Clase y cena»).</summary>
    public string Momento { get; set; } = "";
    /// <summary>Otros conjuntos del mismo día (p. ej. el de deporte, porque hay que cambiarse).</summary>
    public List<Look> Extra { get; set; } = new();
    /// <summary>true si lo ha elegido la IA (Gemini); false si sale de las reglas de respaldo.</summary>
    public bool DeIA { get; set; }
}

/// <summary>Un conjunto para un momento del día.</summary>
public class Look
{
    public string Momento { get; set; } = "";
    public List<string> Ids { get; set; } = new();
    public string Porque { get; set; } = "";
}

public class Guardado
{
    public string Id { get; set; } = "";
    public string? Fecha { get; set; }
    public List<string> Ids { get; set; } = new();
    public string Titulo { get; set; } = "";
    public string? Look { get; set; }
}

public class Perfil
{
    public string Nombre { get; set; } = "";
    public string Usuario { get; set; } = "";
    public string Email { get; set; } = "";
    public string Edad { get; set; } = "";
    public string Ciudad { get; set; } = "Ciudad Real";
    public string Cuenta { get; set; } = "Correo";
    public List<string> Estilos { get; set; } = new();
    public List<string> Gustos { get; set; } = new();
    public bool Registrada { get; set; }
    /// <summary>Foto de perfil (data URL). Si es null, se ve la inicial.</summary>
    public string? Foto { get; set; }
    /// <summary>Frase sobre ti que sale en tu perfil, como en los de Explorar.</summary>
    public string Bio { get; set; } = "";

    public Perfil Copia() => new()
    {
        Nombre = Nombre, Usuario = Usuario, Email = Email, Edad = Edad, Ciudad = Ciudad, Cuenta = Cuenta, Foto = Foto, Bio = Bio,
        Estilos = new List<string>(Estilos), Gustos = new List<string>(Gustos), Registrada = Registrada,
    };
}

public class Ajustes
{
    public bool Sonido { get; set; } = true;
    public bool Vibracion { get; set; } = true;
    /// <summary>Reto 3: "mensajes" (espera con mensajes y huecos grises) o "clasica".</summary>
    public string Espera { get; set; } = "mensajes";
    /// <summary>Días que suele tardar la ropa en estar lista (sale marcado al poner la lavadora). Mínimo 2.</summary>
    public int DiasLavadora { get; set; } = 2;
    /// <summary>Cuándo pone la lavadora el usuario: "manual" (cuando quiera), "cada" (cada X días) o "semana" (un día fijo).</summary>
    public string Programa { get; set; } = "manual";
    public int CadaDias { get; set; } = 7;
    /// <summary>Día fijo de la semana (0 domingo … 6 sábado).</summary>
    public int DiaSemana { get; set; } = 6;
    /// <summary>Último día que se puso la lavadora (AAAA-MM-DD).</summary>
    public string? UltimaLavadora { get; set; }
    /// <summary>Último día que se avisó de que tocaba lavadora (para avisar una vez al día).</summary>
    public string? UltimoRecordatorio { get; set; }

    // ---- Accesibilidad (modo fácil y ajustes sueltos) ----
    /// <summary>Tamaño de letra: "normal", "grande" o "muy-grande".</summary>
    public string Letra { get; set; } = "normal";
    public bool AltoContraste { get; set; }
    public bool BotonesGrandes { get; set; }
    /// <summary>Menos frases y palabras más fáciles.</summary>
    public bool TextosSencillos { get; set; }
    /// <summary>Oculta lo que no es básico (Explorar, «Otra opción»…).</summary>
    public bool MenosOpciones { get; set; }
    /// <summary>Los mensajes de abajo se quedan más tiempo en pantalla.</summary>
    public bool AvisosLargos { get; set; }
    /// <summary>El modo fácil está activo cuando están puestos todos sus ajustes.</summary>
    [JsonIgnore] public bool ModoFacil => Letra != "normal" && BotonesGrandes && TextosSencillos && MenosOpciones && AvisosLargos;
}

/// <summary>Todo lo que se guarda en el navegador.</summary>
public class Datos
{
    public List<Prenda> Prendas { get; set; } = new();
    public Dictionary<string, PlanDia> Planes { get; set; } = new();
    public Dictionary<string, Outfit> Outfits { get; set; } = new();
    public List<Guardado> Guardados { get; set; } = new();
    public List<string> Favoritas { get; set; } = new();
    public Perfil Perfil { get; set; } = new();
    public Ajustes Ajustes { get; set; } = new();
    /// <summary>Lo que haces en Explorar: me gusta, a quién sigues y tus comentarios.</summary>
    public Social Social { get; set; } = new();
}

public class Social
{
    public List<string> MeGusta { get; set; } = new();
    public List<string> Siguiendo { get; set; } = new();
    public List<Comentario> Comentarios { get; set; } = new();
}

/// <summary>Un comentario que has escrito en una publicación de Explorar.</summary>
public class Comentario
{
    public string Id { get; set; } = "";
    public string PubId { get; set; } = "";
    public string Texto { get; set; } = "";
    /// <summary>Momento en que se escribió (UTC, formato ISO).</summary>
    public string Hora { get; set; } = "";
}

/// <summary>Tiempo de un día: temperatura máxima y cielo ("sol", "nubes", "nublado", "lluvia").</summary>
public record Tiempo(int T, string Cielo);

/// <summary>Tipo de plan: Universidad, Trabajo, Cena…</summary>
public record TipoPlan(string Clave, string Nombre, int Formalidad, string Patron, string Frase);

/// <summary>Un outfit publicado en Explorar. Usuario sin «@» (lucia).</summary>
public record Publicacion(string Id, string Usuario, string Hace, string Titulo, string Texto, string? Look, string[] Ids, int MeGusta);

/// <summary>Una persona de Explorar (datos de ejemplo).</summary>
public record Persona(string Usuario, string Nombre, string Bio, string[] Estilos, int Seguidores, int Siguiendo);

/// <summary>Comentarios de ejemplo que ya traen las publicaciones.</summary>
public record ComentarioEjemplo(string Usuario, string Texto, string Hace);
