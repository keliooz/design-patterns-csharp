using DesignPatterns.Structural.Decorator;
using DesignPatterns.Structural.Decorator.Good;
using LegacyProductRepository = DesignPatterns.Structural.Decorator.Bad.ProductRepository;

namespace DesignPatterns.Tests.Structural.Decorator;

public class ProductRepositoryDecoratorTests
{
    private static readonly Product Keyboard = new(1, "Teclado", 199.90m);
    private static readonly Product Mouse = new(2, "Mouse", 89.90m);
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private static InMemoryProductRepository CreateDatabase() => new([Keyboard, Mouse]);

    // ---------- Cache ----------

    [Fact]
    public async Task Caching_SecondRead_DoesNotHitTheInnerRepository()
    {
        var database = new CountingProductRepository(CreateDatabase());
        var repository = new CachingProductRepository(database, Ttl, new ManualTimeProvider());

        var first = await repository.GetByIdAsync(1);
        var second = await repository.GetByIdAsync(1);

        Assert.Equal(Keyboard, first);
        Assert.Equal(Keyboard, second);
        Assert.Equal(1, database.GetByIdCalls);
    }

    [Fact]
    public async Task Caching_ExpiredEntry_ReadsFromTheInnerRepositoryAgain()
    {
        var clock = new ManualTimeProvider();
        var database = new CountingProductRepository(CreateDatabase());
        var repository = new CachingProductRepository(database, Ttl, clock);

        await repository.GetByIdAsync(1);
        clock.Advance(Ttl - TimeSpan.FromSeconds(1));
        await repository.GetByIdAsync(1);
        Assert.Equal(1, database.GetByIdCalls);

        clock.Advance(TimeSpan.FromSeconds(1));
        await repository.GetByIdAsync(1);
        Assert.Equal(2, database.GetByIdCalls);
    }

    [Fact]
    public async Task Caching_Save_InvalidatesTheEntry()
    {
        var database = new CountingProductRepository(CreateDatabase());
        var repository = new CachingProductRepository(database, Ttl, new ManualTimeProvider());
        var cheaperKeyboard = Keyboard with { Price = 149.90m };

        await repository.GetByIdAsync(1);
        await repository.SaveAsync(cheaperKeyboard);
        var afterSave = await repository.GetByIdAsync(1);

        Assert.Equal(cheaperKeyboard, afterSave);
        Assert.Equal(2, database.GetByIdCalls);
        Assert.Equal(1, database.SaveCalls);
    }

    [Fact]
    public async Task Caching_MissingProduct_IsNotCached()
    {
        var database = new CountingProductRepository(CreateDatabase());
        var repository = new CachingProductRepository(database, Ttl, new ManualTimeProvider());
        var monitor = new Product(3, "Monitor", 999m);

        Assert.Null(await repository.GetByIdAsync(3));

        // Alguém cadastra o produto direto no banco, sem passar pelo cache.
        await database.SaveAsync(monitor);

        Assert.Equal(monitor, await repository.GetByIdAsync(3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Caching_NonPositiveTtl_Throws(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CachingProductRepository(CreateDatabase(), TimeSpan.FromSeconds(seconds)));
    }

    // ---------- Log ----------

    [Fact]
    public async Task Logging_RecordsEachCallAndReturnsTheInnerResult()
    {
        var log = new List<string>();
        var repository = new LoggingProductRepository(CreateDatabase(), log.Add);

        var found = await repository.GetByIdAsync(1);
        var missing = await repository.GetByIdAsync(42);
        await repository.SaveAsync(Mouse);

        Assert.Equal(Keyboard, found);
        Assert.Null(missing);
        Assert.Equal(
            ["GetById(1): encontrado", "GetById(42): não encontrado", "Save(2): salvo"],
            log);
    }

    [Fact]
    public async Task Logging_InnerFailure_IsLoggedAndRethrown()
    {
        var log = new List<string>();
        var repository = new LoggingProductRepository(new FailingProductRepository(), log.Add);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetByIdAsync(1));

        Assert.Equal("banco fora do ar", ex.Message);
        Assert.Equal(["GetById(1): falhou - banco fora do ar"], log);
    }

    [Fact]
    public void Decorators_NullInner_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LoggingProductRepository(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new CachingProductRepository(null!, Ttl));
    }

    // ---------- Composição ----------

    [Fact]
    public async Task Order_LoggingOutsideCache_LogsEveryCall()
    {
        var log = new List<string>();
        IProductRepository repository =
            new LoggingProductRepository(
                new CachingProductRepository(CreateDatabase(), Ttl, new ManualTimeProvider()),
                log.Add);

        await repository.GetByIdAsync(1);
        await repository.GetByIdAsync(1);

        Assert.Equal(2, log.Count);
    }

    [Fact]
    public async Task Order_LoggingInsideCache_LogsOnlyTripsToTheDatabase()
    {
        var log = new List<string>();
        IProductRepository repository =
            new CachingProductRepository(
                new LoggingProductRepository(CreateDatabase(), log.Add),
                Ttl,
                new ManualTimeProvider());

        await repository.GetByIdAsync(1);
        await repository.GetByIdAsync(1);

        Assert.Single(log);
    }

    [Fact]
    public async Task Decorators_AreTransparentToTheClient()
    {
        // O cliente só conhece IProductRepository: com ou sem decorators, o resultado é o mesmo.
        IProductRepository plain = CreateDatabase();
        IProductRepository decorated =
            new LoggingProductRepository(
                new CachingProductRepository(CreateDatabase(), Ttl, new ManualTimeProvider()),
                _ => { });

        foreach (var id in new[] { 1, 2, 42, 1 })
        {
            Assert.Equal(await plain.GetByIdAsync(id), await decorated.GetByIdAsync(id));
        }
    }

    [Fact]
    public async Task Refactoring_KeepsTheLegacyBehavior()
    {
        // Teste de caracterização: Logging(Caching(InMemory)) faz o mesmo que a classe "faz-tudo".
        var clock = new ManualTimeProvider();
        var legacyLog = new List<string>();
        var decoratedLog = new List<string>();

        var legacy = new LegacyProductRepository(
            [Keyboard, Mouse], enableCache: true, Ttl, enableLogging: true, legacyLog.Add, clock);
        IProductRepository decorated =
            new LoggingProductRepository(
                new CachingProductRepository(CreateDatabase(), Ttl, clock),
                decoratedLog.Add);

        var cheaperMouse = Mouse with { Price = 59.90m };

        foreach (var id in new[] { 1, 1, 2, 42 })
        {
            Assert.Equal(await legacy.GetByIdAsync(id), await decorated.GetByIdAsync(id));
        }

        await legacy.SaveAsync(cheaperMouse);
        await decorated.SaveAsync(cheaperMouse);
        Assert.Equal(await legacy.GetByIdAsync(2), await decorated.GetByIdAsync(2));

        Assert.Equal(legacyLog, decoratedLog);
    }

    // ---------- Dublês de teste ----------

    /// <summary>Também é um decorator: conta as chamadas e repassa para o repositório real.</summary>
    private sealed class CountingProductRepository(IProductRepository inner) : IProductRepository
    {
        public int GetByIdCalls { get; private set; }

        public int SaveCalls { get; private set; }

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            GetByIdCalls++;
            return inner.GetByIdAsync(id, cancellationToken);
        }

        public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return inner.SaveAsync(product, cancellationToken);
        }
    }

    private sealed class FailingProductRepository : IProductRepository
    {
        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("banco fora do ar");

        public Task SaveAsync(Product product, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("banco fora do ar");
    }

    /// <summary>Relógio controlado pelo teste (o pacote Microsoft.Extensions.TimeProvider.Testing traz um pronto).</summary>
    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
