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

    public Perfil Copia() => new()
    {
        Nombre = Nombre, Usuario = Usuario, Email = Email, Edad = Edad, Ciudad = Ciudad, Cuenta = Cuenta,
        Estilos = new List<string>(Estilos), Gustos = new List<string>(Gustos), Registrada = Registrada,
    };
}

public class Ajustes
{
    public bool Sonido { get; set; } = true;
    public bool Vibracion { get; set; } = true;
    /// <summary>Reto 3: "mensajes" (espera con mensajes y huecos grises) o "clasica".</summary>
    public string Espera { get; set; } = "mensajes";
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
}

/// <summary>Tiempo de un día: temperatura máxima y cielo ("sol", "nubes", "nublado", "lluvia").</summary>
public record Tiempo(int T, string Cielo);

/// <summary>Tipo de plan: Universidad, Trabajo, Cena…</summary>
public record TipoPlan(string Clave, string Nombre, int Formalidad, string Patron, string Frase);

public record Publicacion(string Id, string Usuario, string Hace, string Titulo, string Texto, string? Look, string[] Ids, int MeGusta, int Comentarios);
