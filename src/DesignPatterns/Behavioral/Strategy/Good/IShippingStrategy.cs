namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>
/// Contrato comum a todos os algoritmos de cálculo de frete.
/// </summary>
public interface IShippingStrategy
{
    /// <summary>Identificador do método de frete (ex.: "standard").</summary>
    string Name { get; }

    decimal Calculate(ShippingRequest request);
}
