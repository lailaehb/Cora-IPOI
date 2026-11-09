using Microsoft.AspNetCore.Components;

namespace Cora.Shared;

/// <summary>Iconos de la app (SVG de trazo, como en Figma).</summary>
public static class Iconos
{
    static MarkupString S(string trazo, int tam = 24, double grosor = 1.8, string relleno = "none") =>
        new($"<svg class=\"ico\" width=\"{tam}\" height=\"{tam}\" viewBox=\"0 0 24 24\" fill=\"{relleno}\" stroke=\"currentColor\" stroke-width=\"{grosor.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{trazo}</svg>");

    public static MarkupString Casa => S("<path d='M3 11l9-7 9 7'/><path d='M5 10v10h14V10'/><path d='M10 20v-5h4v5'/>");
    public static MarkupString Percha => S("<path d='M12 8a2 2 0 1 0-2-2'/><path d='M12 8v1L3 16a1.4 1.4 0 0 0 .9 2.5h16.2A1.4 1.4 0 0 0 21 16l-9-7'/>");
    public static MarkupString PerchaGrande => S("<path d='M12 8a2 2 0 1 0-2-2'/><path d='M12 8v1L3 16a1.4 1.4 0 0 0 .9 2.5h16.2A1.4 1.4 0 0 0 21 16l-9-7'/>", 64, 1.4);
    public static MarkupString Mas => S("<path d='M12 5v14M5 12h14'/>", 28, 2.4);
    public static MarkupString Calendario => S("<rect x='3.5' y='5' width='17' height='15' rx='2'/><path d='M3.5 10h17M8 3v4M16 3v4'/>");
    public static MarkupString Globo => S("<circle cx='12' cy='12' r='9'/><path d='M3 12h18M12 3c2.6 2.6 3.8 5.6 3.8 9s-1.2 6.4-3.8 9c-2.6-2.6-3.8-5.6-3.8-9S9.4 5.6 12 3z'/>");
    public static MarkupString Atras => S("<path d='M15 5l-7 7 7 7'/>", 26, 2);
    public static MarkupString Siguiente => S("<path d='M9 6l6 6-6 6'/>", 20, 2);
    public static MarkupString Anterior => S("<path d='M15 6l-6 6 6 6'/>", 22, 2);
    public static MarkupString Qr => S("<rect x='3' y='3' width='7' height='7' rx='1'/><rect x='14' y='3' width='7' height='7' rx='1'/><rect x='3' y='14' width='7' height='7' rx='1'/><path d='M14 14h3v3h-3zM20 14v.01M14 20h.01M17 20h4v-3'/>", 28);
    public static MarkupString Camara => S("<path d='M4 8h3l2-3h6l2 3h3v11H4z'/><circle cx='12' cy='13' r='3.5'/>", 28);
    public static MarkupString Lapiz => S("<path d='M4 20h4L19 9l-4-4L4 16z'/>", 28);
    public static MarkupString Lavadora => S("<rect x='4' y='3' width='16' height='18' rx='2'/><circle cx='12' cy='13' r='4'/><path d='M8 6.5h.01M11 6.5h.01'/>", 22, 2);
    public static MarkupString Recargar => S("<path d='M20 11a8 8 0 0 0-14.6-4.5M4 4v4h4M4 13a8 8 0 0 0 14.6 4.5M20 20v-4h-4'/>", 18);
    public static MarkupString Marcador(bool lleno) => S("<path d='M6 3h12v18l-6-4-6 4z'/>", 20, 1.8, lleno ? "currentColor" : "none");
    public static MarkupString Corazon(bool lleno) => S("<path d='M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z'/>", 22, 1.8, lleno ? "currentColor" : "none");
    public static MarkupString Comentario(bool lleno) => S("<path d='M4 5h16v11H9l-5 4z'/>", 22, 1.8, lleno ? "currentColor" : "none");
    public static MarkupString Engranaje => S("<circle cx='12' cy='12' r='3'/><path d='M12 2v3M12 19v3M4.2 4.2l2.1 2.1M17.7 17.7l2.1 2.1M2 12h3M19 12h3M4.2 19.8l2.1-2.1M17.7 6.3l2.1-2.1'/>", 22);
    public static MarkupString Chispa => S("<path d='M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8z'/><path d='M19 15l.8 2.2L22 18l-2.2.8L19 21l-.8-2.2L16 18l2.2-.8z'/>", 20);
    public static MarkupString Info => S("<circle cx='12' cy='12' r='9'/><path d='M12 11v5M12 8h.01'/>", 20);
    public static MarkupString Check => S("<path d='M5 12l5 5L20 7'/>", 16, 2.6);
    public static MarkupString Imagen => S("<rect x='3' y='4' width='18' height='16' rx='2'/><circle cx='9' cy='10' r='2'/><path d='M21 16l-5-5-9 9'/>", 20);
    public static MarkupString Altavoz => S("<path d='M4 9v6h4l5 4V5L8 9z'/><path d='M16 9a4 4 0 0 1 0 6M18.5 6.5a8 8 0 0 1 0 11'/>", 20);
}
