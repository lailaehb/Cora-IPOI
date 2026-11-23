using System.Net.Http.Json;
using System.Text.Json;

namespace Cora.Servicios;

/// <summary>
/// Pide a la función de Vercel <c>/api/producto</c> la foto de un producto de cualquier tienda online.
/// Las tiendas no dejan que el navegador lea sus páginas, por eso lo hace el servidor.
/// </summary>
public class ServicioProducto
{
    readonly HttpClient _http;
    public ServicioProducto(HttpClient http) => _http = http;

    /// <summary>Motivo del último fallo ("sin_api", "tienda_bloquea", "sin_foto", "tarda"…).</summary>
    public string? UltimoError { get; private set; }

    public record Resultado(string? Foto, string? Titulo, string? Tienda);

    public async Task<Resultado?> BuscarAsync(string enlace)
    {
        UltimoError = null;
        try
        {
            using var cancelar = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using var resp = await _http.GetAsync("api/producto?url=" + Uri.EscapeDataString(enlace.Trim()), cancelar.Token);
            // En local (dotnet run) no existe /api/producto: devuelve la página HTML o un 404
            if (resp.Content.Headers.ContentType?.MediaType != "application/json") { UltimoError = "sin_api"; return null; }
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(cancelar.Token));
            var j = doc.RootElement;
            string? S(string k) => j.TryGetProperty(k, out var x) && x.ValueKind == JsonValueKind.String ? x.GetString() : null;
            var ok = j.TryGetProperty("ok", out var o) && o.ValueKind == JsonValueKind.True;
            if (!ok) { UltimoError = S("error") ?? "fallo"; return new Resultado(null, S("titulo"), S("tienda")); }
            return new Resultado(S("foto"), S("titulo"), S("tienda"));
        }
        catch (OperationCanceledException) { UltimoError = "tarda"; return null; }
        catch { UltimoError = "sin_conexion"; return null; }
    }

    /// <summary>Texto para el usuario cuando no se ha podido traer la foto.</summary>
    public string ExplicarError() => UltimoError switch
    {
        "sin_api" => "La foto de la tienda solo se descarga en la web publicada en Vercel.",
        "tienda_bloquea" => "La tienda no me ha dejado descargar la foto.",
        "sin_foto" => "No he encontrado la foto en la página de la tienda.",
        "tarda" => "La tienda ha tardado demasiado en responder.",
        _ => "No he podido descargar la foto de la tienda.",
    };
}
