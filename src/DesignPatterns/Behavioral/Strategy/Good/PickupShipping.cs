namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>Retirada na loja: sem custo de frete.</summary>
public sealed class PickupShipping : IShippingStrategy
{
    public string Name => "pickup";

    public decimal Calculate(ShippingRequest request) => 0m;
}
