using System.Text.Json;
using Cora.Modelos;

namespace Cora.Servicios;

/// <summary>
/// El tiempo de cada día. Usa la API gratuita de Open-Meteo (no necesita clave) con la ciudad del perfil.
/// Si no hay conexión, usa una previsión de ejemplo para que la app siga funcionando.
/// </summary>
public class ServicioTiempo
{
    private readonly HttpClient _http;
    private Dictionary<string, Tiempo>? _prevision;

    public ServicioTiempo(HttpClient http) => _http = http;

    public string Fuente { get; private set; } = "Previsión de ejemplo";
    public bool EsReal => _prevision != null;

    static readonly Tiempo[] Ejemplo =
    {
        new(13, "nublado"), new(16, "sol"), new(19, "sol"), new(11, "lluvia"), new(17, "nubes"), new(22, "sol"), new(15, "lluvia"),
    };

    public static Tiempo DeEjemplo(string dia)
    {
        int n = Fechas.Dif(dia);
        if (n >= 0 && n < 7) return Ejemplo[n];
        uint h = Fechas.Hash(dia);
        string[] cielos = { "sol", "nubes", "lluvia", "sol", "nublado" };
        return new Tiempo(10 + (int)(h % 11), cielos[h % 5]);
    }

    public Tiempo De(string dia) => _prevision != null && _prevision.TryGetValue(dia, out var t) ? t : DeEjemplo(dia);

    public Dictionary<string, Tiempo>? Prevision => _prevision;

    /// <summary>Busca la ciudad y descarga la previsión de 16 días. Devuelve true si lo consigue.</summary>
    public async Task<bool> CargarAsync(string ciudad)
    {
        if (string.IsNullOrWhiteSpace(ciudad)) return false;
        try
        {
            using var docGeo = JsonDocument.Parse(await _http.GetStringAsync($"https://geocoding-api.open-meteo.com/v1/search?count=1&language=es&name={Uri.EscapeDataString(ciudad)}"));
            var geo = docGeo.RootElement;
            if (!geo.TryGetProperty("results", out var res) || res.GetArrayLength() == 0) { Fuente = $"Previsión de ejemplo (no encuentro «{ciudad}»)"; return false; }
            double lat = res[0].GetProperty("latitude").GetDouble();
            double lon = res[0].GetProperty("longitude").GetDouble();
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&daily=temperature_2m_max,weathercode&timezone=auto&forecast_days=16";
            using var doc = JsonDocument.Parse(await _http.GetStringAsync(url));
            var j = doc.RootElement;
            var daily = j.GetProperty("daily");
            var dias = daily.GetProperty("time");
            var temps = daily.GetProperty("temperature_2m_max");
            var codigos = daily.GetProperty("weathercode");
            var p = new Dictionary<string, Tiempo>();
            for (int i = 0; i < dias.GetArrayLength(); i++)
            {
                if (temps[i].ValueKind != JsonValueKind.Number || codigos[i].ValueKind != JsonValueKind.Number) continue;
                p[dias[i].GetString()!] = new Tiempo((int)Math.Round(temps[i].GetDouble()), Cielo(codigos[i].GetInt32()));
            }
            _prevision = p;
            Fuente = $"Previsión real de {ciudad}";
            return true;
        }
        catch
        {
            Fuente = "Previsión de ejemplo (sin conexión)";
            return false;
        }
    }

    /// <summary>Traduce el código de tiempo de Open-Meteo (WMO) a nuestras cuatro categorías.</summary>
    static string Cielo(int codigo) => codigo switch
    {
        <= 1 => "sol",
        2 => "nubes",
        3 or 45 or 48 => "nublado",
        _ => "lluvia",
    };

    public static string Icono(string cielo) => cielo switch { "lluvia" => "🌧️", "sol" => "☀️", _ => "☁️" };
}
