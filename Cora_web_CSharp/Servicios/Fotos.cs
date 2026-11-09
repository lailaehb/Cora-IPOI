using Microsoft.AspNetCore.Components.Forms;

namespace Cora.Servicios;

public static class Fotos
{
    /// <summary>Reduce la foto elegida (máx. 480 px) y la convierte en texto para guardarla en el navegador.</summary>
    public static async Task<string?> LeerAsync(IBrowserFile archivo, int lado = 480)
    {
        try
        {
            var reducida = await archivo.RequestImageFileAsync("image/jpeg", lado, lado);
            await using var s = reducida.OpenReadStream(10_000_000);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            return "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return null;
        }
    }
}
