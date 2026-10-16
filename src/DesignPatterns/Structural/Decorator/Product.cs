namespace DesignPatterns.Structural.Decorator;

/// <summary>Produto do catálogo. Compartilhado pelas versões <c>Bad</c> e <c>Good</c>.</summary>
public sealed record Product(int Id, string Name, decimal Price);
