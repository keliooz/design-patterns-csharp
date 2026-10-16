using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Decorator.Good;

/// <summary>
/// Decorator de cache: guarda produtos lidos por <paramref name="ttl"/> e só chama <c>inner</c> quando
/// o item não está no cache ou expirou. <see cref="SaveAsync"/> grava em <c>inner</c> e invalida a entrada.
/// Produtos não encontrados não vão para o cache, para que um produto recém-criado apareça na próxima leitura.
/// </summary>
/// <param name="inner">Repositório decorado.</param>
/// <param name="ttl">Tempo de vida de cada entrada. Precisa ser positivo.</param>
/// <param name="timeProvider">
/// Relógio usado para expirar entradas. Padrão: <see cref="TimeProvider.System"/>.
/// Nos testes, um relógio falso permite avançar o tempo sem <c>Thread.Sleep</c>.
/// </param>
public sealed class CachingProductRepository(
    IProductRepository inner,
    TimeSpan ttl,
    TimeProvider? timeProvider = null) : IProductRepository
{
    private readonly IProductRepository _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly TimeSpan _ttl = ttl > TimeSpan.Zero
        ? ttl
        : throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "O TTL do cache precisa ser positivo.");
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly ConcurrentDictionary<int, CacheEntry> _cache = new();

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(id, out var entry) && _timeProvider.GetUtcNow() < entry.ExpiresAt)
        {
            return entry.Product;
        }

        var product = await _inner.GetByIdAsync(id, cancellationToken);

        if (product is not null)
        {
            _cache[id] = new CacheEntry(product, _timeProvider.GetUtcNow() + _ttl);
        }

        return product;
    }

    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await _inner.SaveAsync(product, cancellationToken);

        // Invalida em vez de gravar no cache: a fonte de verdade é o repositório decorado.
        _cache.TryRemove(product.Id, out _);
    }

    private readonly record struct CacheEntry(Product Product, DateTimeOffset ExpiresAt);
}
