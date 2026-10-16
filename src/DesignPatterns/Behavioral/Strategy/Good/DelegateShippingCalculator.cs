namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>
/// Strategy no estilo C# idiomático: em vez de uma interface + uma classe por algoritmo,
/// cada estratégia é um delegate <c>Func&lt;ShippingRequest, decimal&gt;</c>.
///
/// Boa escolha quando as estratégias são curtas e sem estado.
/// Prefira classes (versão <see cref="ShippingCalculator"/>) quando cada estratégia
/// tem dependências próprias, estado ou precisa ser testada e registrada isoladamente.
/// </summary>
public sealed class DelegateShippingCalculator
{
    private readonly Dictionary<string, Func<ShippingRequest, decimal>> _strategies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["standard"] = r => 10m + 1.5m * r.WeightKg + 0.02m * r.DistanceKm,
            ["express"] = r => 25m + 3m * r.WeightKg + 0.10m * r.DistanceKm,
            ["pickup"] = _ => 0m,
        };

    /// <summary>Registra (ou substitui) um método de frete.</summary>
    public void Register(string method, Func<ShippingRequest, decimal> strategy)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(strategy);

        _strategies[method] = strategy;
    }

    public decimal Calculate(string method, ShippingRequest request)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategies.TryGetValue(method, out var strategy))
        {
            throw new ArgumentException($"Método de frete desconhecido: '{method}'.", nameof(method));
        }

        return strategy(request);
    }
}
