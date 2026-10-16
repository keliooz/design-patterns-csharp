using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Decorator.Good;

/// <summary>
/// Componente concreto: só acessa os dados. Num projeto real seria a versão com EF Core ou Dapper.
/// Não sabe nada de log nem de cache.
/// </summary>
public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<int, Product> _table = new();

    public InMemoryProductRepository(IEnumerable<Product>? seed = null)
    {
        foreach (var product in seed ?? [])
        {
            _table[product.Id] = product;
        }
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _table.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        _table[product.Id] = product;
        return Task.CompletedTask;
    }
}
