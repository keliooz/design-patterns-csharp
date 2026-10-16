using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Decorator.Bad;

/// <summary>
/// ❌ ANTI-EXEMPLO: repositório que acessa os dados, faz log e faz cache, tudo na mesma classe.
///
/// Problemas:
/// - Três responsabilidades (acesso a dados, log, cache) misturadas em cada método (viola SRP).
/// - Ligar/desligar comportamento exige flags no construtor e <c>if</c>s espalhados.
/// - Um novo comportamento (métricas, retry, autorização) obriga a MODIFICAR esta classe (viola Open/Closed).
/// - A ordem entre log e cache está fixa no código: não dá para escolher logar só as idas ao banco.
/// - Outro repositório (pedidos, clientes) precisa copiar e colar toda essa lógica.
/// - Não dá para testar o cache sem o acesso a dados, nem o acesso a dados sem o log.
/// </summary>
public class ProductRepository
{
    // "Banco de dados".
    private readonly ConcurrentDictionary<int, Product> _table = new();

    // Cache.
    private readonly bool _enableCache;
    private readonly TimeSpan _cacheTtl;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<int, (Product Product, DateTimeOffset ExpiresAt)> _cache = new();

    // Log.
    private readonly bool _enableLogging;
    private readonly Action<string>? _log;

    public ProductRepository(
        IEnumerable<Product> seed,
        bool enableCache,
        TimeSpan cacheTtl,
        bool enableLogging,
        Action<string>? log,
        TimeProvider? timeProvider = null)
    {
        foreach (var product in seed)
        {
            _table[product.Id] = product;
        }

        _enableCache = enableCache;
        _cacheTtl = cacheTtl;
        _enableLogging = enableLogging;
        _log = log;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Product? product;

        if (_enableCache
            && _cache.TryGetValue(id, out var entry)
            && _timeProvider.GetUtcNow() < entry.ExpiresAt)
        {
            product = entry.Product;
        }
        else
        {
            _table.TryGetValue(id, out product);

            if (_enableCache && product is not null)
            {
                _cache[id] = (product, _timeProvider.GetUtcNow() + _cacheTtl);
            }
        }

        if (_enableLogging)
        {
            _log?.Invoke(product is null ? $"GetById({id}): não encontrado" : $"GetById({id}): encontrado");
        }

        return Task.FromResult(product);
    }

    public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        _table[product.Id] = product;

        if (_enableCache)
        {
            _cache.TryRemove(product.Id, out _);
        }

        if (_enableLogging)
        {
            _log?.Invoke($"Save({product.Id}): salvo");
        }

        return Task.CompletedTask;
    }
}
