using Cora.Servicios;
using Microsoft.AspNetCore.Components;

namespace Cora.Shared;

/// <summary>
/// Base de todas las pantallas: da acceso al estado (E) y a la navegación (Nav),
/// y vuelve a pintar la pantalla cada vez que cambia algo.
/// </summary>
public abstract class PaginaCora : ComponentBase, IDisposable
{
    [Inject] protected Estado E { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected string Hoy => Fechas.Hoy;

    bool _suscrita;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (!_suscrita) { E.Cambio += AlCambiar; _suscrita = true; }
        return base.SetParametersAsync(parameters);
    }

    void AlCambiar() => InvokeAsync(StateHasChanged);

    protected void Ir(string ruta) => Nav.NavigateTo(ruta);

    public virtual void Dispose() => E.Cambio -= AlCambiar;
}
