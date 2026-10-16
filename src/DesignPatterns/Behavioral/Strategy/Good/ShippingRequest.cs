namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>
/// Dados de entrada do cálculo de frete. Imutável e validado na criação.
/// </summary>
public sealed record ShippingRequest
{
    public decimal WeightKg { get; }
    public decimal DistanceKm { get; }

    public ShippingRequest(decimal weightKg, decimal distanceKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(weightKg);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceKm);

        WeightKg = weightKg;
        DistanceKm = distanceKm;
    }
}
