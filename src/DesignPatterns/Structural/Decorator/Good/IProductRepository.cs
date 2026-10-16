namespace DesignPatterns.Structural.Decorator.Good;

/// <summary>
/// Componente do padrão Decorator: o contrato que o cliente conhece.
/// Tanto o repositório real quanto os decorators implementam esta interface,
/// por isso o cliente não sabe (nem precisa saber) quantas camadas existem.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task SaveAsync(Product product, CancellationToken cancellationToken = default);
}
