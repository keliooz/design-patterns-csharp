namespace DesignPatterns.Structural.Decorator.Good;

/// <summary>
/// Decorator de log: registra o resultado de cada chamada e repassa tudo para <c>inner</c>.
/// Funciona em volta de qualquer <see cref="IProductRepository"/>: o real, o de cache ou outro decorator.
/// </summary>
/// <param name="inner">Repositório decorado.</param>
/// <param name="log">
/// Destino das mensagens. Num projeto real seria um <c>ILogger&lt;T&gt;</c>;
/// aqui é um delegate para o exemplo não depender de pacotes.
/// </param>
public sealed class LoggingProductRepository(IProductRepository inner, Action<string> log) : IProductRepository
{
    private readonly IProductRepository _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Action<string> _log = log ?? throw new ArgumentNullException(nameof(log));

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _inner.GetByIdAsync(id, cancellationToken);
            _log(product is null ? $"GetById({id}): não encontrado" : $"GetById({id}): encontrado");
            return product;
        }
        catch (Exception ex)
        {
            _log($"GetById({id}): falhou - {ex.Message}");
            throw;
        }
    }

    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            await _inner.SaveAsync(product, cancellationToken);
            _log($"Save({product.Id}): salvo");
        }
        catch (Exception ex)
        {
            _log($"Save({product.Id}): falhou - {ex.Message}");
            throw;
        }
    }
}
