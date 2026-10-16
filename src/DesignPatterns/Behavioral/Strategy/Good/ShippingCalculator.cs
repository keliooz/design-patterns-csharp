namespace DesignPatterns.Behavioral.Strategy.Good;

/// <summary>
/// Contexto do padrão Strategy: escolhe a estratégia pelo nome e delega o cálculo.
/// Não conhece nenhuma regra de preço; adicionar um novo método de frete
/// é só registrar mais uma <see cref="IShippingStrategy"/>, sem alterar esta classe.
/// </summary>
public sealed class ShippingCalculator
{
    private readonly IReadOnlyDictionary<string, IShippingStrategy> _strategies;

    /// <param name="strategies">
    /// Estratégias disponíveis. Com injeção de dependência, basta registrar cada
    /// implementação como <c>IShippingStrategy</c> e o container injeta a coleção.
    /// Nomes duplicados lançam <see cref="ArgumentException"/>.
    /// </param>
    public ShippingCalculator(IEnumerable<IShippingStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        _strategies = strategies.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }

    public decimal Calculate(string method, ShippingRequest request)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategies.TryGetValue(method, out var strategy))
        {
            throw new ArgumentException($"Método de frete desconhecido: '{method}'.", nameof(method));
        }

        return strategy.Calculate(request);
    }
}
