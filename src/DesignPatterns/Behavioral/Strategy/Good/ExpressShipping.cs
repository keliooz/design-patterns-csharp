namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>Frete expresso: taxa base maior e valores por kg/km mais altos.</summary>
public sealed class ExpressShipping : IShippingStrategy
{
    public string Name => "express";

    public decimal Calculate(ShippingRequest request) =>
        25m + 3m * request.WeightKg + 0.10m * request.DistanceKm;
}
