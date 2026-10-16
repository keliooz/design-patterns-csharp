namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>Frete padrão: taxa base + valor por kg + valor por km.</summary>
public sealed class StandardShipping : IShippingStrategy
{
    public string Name => "standard";

    public decimal Calculate(ShippingRequest request) =>
        10m + 1.5m * request.WeightKg + 0.02m * request.DistanceKm;
}
