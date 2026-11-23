using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Cora.Modelos;

namespace Cora.Servicios;

/// <summary>
/// IA de verdad (Google Gemini) a través de la función de Vercel <c>/api/ia</c>.
/// Si no hay conexión, no hay clave configurada o se acaba la cuota, devuelve null
/// y la app usa las reglas de <see cref="MotorIA"/> como respaldo.
/// </summary>
public class ServicioIA
{
    readonly HttpClient _http;
    public ServicioIA(HttpClient http) => _http = http;

    /// <summary>Motivo del último fallo, para explicárselo al usuario ("sin_clave", "cuota", "sin_conexion"…).</summary>
    public string? UltimoError { get; private set; }

    public static readonly string[] Usos = { "Deporte", "Diario", "Arreglado", "Fiesta" };

    /// <summary>Usos de una prenda a partir de su formalidad (0 deporte · 1 diario · 2 arreglado · 3 fiesta).</summary>
    public static List<string> UsosDe(Prenda p) =>
        Enumerable.Range(0, 4).Where(f => p.Encaja(f)).Select(f => Usos[f]).ToList();

    /// <summary>
    /// Marca o desmarca un uso manteniendo el rango seguido (min..max).
    /// Devuelve null si se intenta quitar uno del medio, y (-1, -1) si se quita el único.
    /// </summary>
    public static (int Min, int Max)? AlternarUso(int min, int max, int nivel)
    {
        if (min < 0) return (nivel, nivel);
        if (nivel < min) return (nivel, max);
        if (nivel > max) return (min, nivel);
        if (min == max) return (-1, -1);
        if (nivel == min) return (min + 1, max);
        if (nivel == max) return (min, max - 1);
        return null;
    }

    public async Task<List<DiaRespuesta>?> PlanificarAsync(PeticionSemana peticion)
    {
        var r = await EnviarAsync(peticion, IAJson.Default.PeticionSemana, IAJson.Default.RespuestaSemana, 40);
        return r?.Dias;
    }

    public async Task<SugerenciaPrenda?> AnalizarFotoAsync(string imagen, string idioma = "es")
    {
        var tipos = DatosEjemplo.Subcategorias.Values.SelectMany(x => x).Distinct().ToList();
        var r = await EnviarAsync(new PeticionFoto { Imagen = imagen, Tipos = tipos, Idioma = idioma }, IAJson.Default.PeticionFoto, IAJson.Default.RespuestaFoto, 25);
        return r?.Prenda;
    }

    async Task<TRes?> EnviarAsync<TPet, TRes>(TPet peticion, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TPet> tipoPet,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRes> tipoRes, int segundos) where TRes : RespuestaIA
    {
        UltimoError = null;
        try
        {
            using var cancelar = new CancellationTokenSource(TimeSpan.FromSeconds(segundos));
            using var resp = await _http.PostAsJsonAsync("api/ia", peticion, tipoPet, cancelar.Token);
            // En local (dotnet run) no existe /api/ia: el servidor devuelve la página HTML o un 404
            if (resp.Content.Headers.ContentType?.MediaType != "application/json") { UltimoError = "sin_api"; return null; }
            var r = await resp.Content.ReadFromJsonAsync(tipoRes, cancelar.Token);
            if (r == null || !r.Ok) { UltimoError = r?.Error ?? "fallo_ia"; return null; }
            return r;
        }
        catch (OperationCanceledException) { UltimoError = "tarda"; return null; }
        catch { UltimoError = "sin_conexion"; return null; }
    }

    /// <summary>Texto para el usuario cuando la IA no está disponible.</summary>
    public string ExplicarError() => UltimoError switch
    {
        "sin_api" => "La IA solo funciona en la web publicada en Vercel; aquí uso mis reglas.",
        "sin_clave" => "Falta configurar la clave de la IA en Vercel; de momento uso mis reglas.",
        "cuota" => "La IA ha llegado a su límite gratuito de hoy; uso mis reglas.",
        "tarda" => "La IA ha tardado demasiado; uso mis reglas.",
        "pocas_prendas" => "Añade más prendas para que la IA pueda combinarlas.",
        _ => "No he podido hablar con la IA; uso mis reglas.",
    };
}

// ---------------------------------------------------------------- mensajes que se envían a /api/ia

public abstract class RespuestaIA
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
}

public class PeticionSemana
{
    public string Accion { get; set; } = "semana";
    public List<DiaPeticion> Dias { get; set; } = new();
    public List<PrendaIA> Armario { get; set; } = new();
    public List<UsadoIA> Usadas { get; set; } = new();
    public List<string> Gustos { get; set; } = new();
    public List<string> Estilos { get; set; } = new();
    /// <summary>Idioma en el que la IA escribe «momento» y «porque» (es, en, fr).</summary>
    public string Idioma { get; set; } = "es";
}

public class DiaPeticion
{
    public string Fecha { get; set; } = "";
    public string Dia { get; set; } = "";
    public int Temperatura { get; set; }
    public string Cielo { get; set; } = "";
    /// <summary>Botones marcados (Universidad, Deporte…).</summary>
    public List<string> Planes { get; set; } = new();
    /// <summary>Lo que el usuario ha escrito con sus palabras.</summary>
    public string Texto { get; set; } = "";
    /// <summary>Conjunto que el usuario acaba de descartar con «Otra opción».</summary>
    public List<string> Evitar { get; set; } = new();
}

public class PrendaIA
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Color { get; set; } = "";
    public List<string> Usos { get; set; } = new();
    public int Abrigo { get; set; }
    public bool Lluvia { get; set; }
}

public class UsadoIA
{
    public string Fecha { get; set; } = "";
    public List<string> Ids { get; set; } = new();
}

public class RespuestaSemana : RespuestaIA
{
    public List<DiaRespuesta>? Dias { get; set; }
}

public class DiaRespuesta
{
    public string Fecha { get; set; } = "";
    public List<LookIA> Looks { get; set; } = new();
}

public class LookIA
{
    public string Momento { get; set; } = "";
    public string Plan { get; set; } = "";
    public List<string> Ids { get; set; } = new();
    public string Porque { get; set; } = "";
}

public class PeticionFoto
{
    public string Accion { get; set; } = "foto";
    public string Imagen { get; set; } = "";
    public string Idioma { get; set; } = "es";
    public List<string> Tipos { get; set; } = new();
}

public class RespuestaFoto : RespuestaIA
{
    public SugerenciaPrenda? Prenda { get; set; }
}

/// <summary>Lo que la IA ve en la foto. El usuario lo revisa antes de guardar.</summary>
public class SugerenciaPrenda
{
    public bool EsPrenda { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Color { get; set; } = "";
    public List<string> Usos { get; set; } = new();
    public int Abrigo { get; set; }
    public bool Lluvia { get; set; } = true;
}

/// <summary>Serialización generada al compilar (en camelCase, como espera la función de Vercel).</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(PeticionSemana))]
[JsonSerializable(typeof(RespuestaSemana))]
[JsonSerializable(typeof(PeticionFoto))]
[JsonSerializable(typeof(RespuestaFoto))]
public partial class IAJson : JsonSerializerContext { }
