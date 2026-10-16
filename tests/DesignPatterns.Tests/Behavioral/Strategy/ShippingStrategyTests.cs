using DesignPatterns.Behavioral.Strategy.Bad;
using DesignPatterns.Behavioral.Strategy.Good;

namespace DesignPatterns.Tests.Behavioral.Strategy;

public class ShippingStrategyTests
{
    // 2 kg a 100 km:
    //   standard = 10 + 1.5 * 2 + 0.02 * 100 = 15
    //   express  = 25 + 3   * 2 + 0.10 * 100 = 41
    //   pickup   = 0
    private static readonly ShippingRequest TwoKgHundredKm = new(weightKg: 2m, distanceKm: 100m);

    private static ShippingCalculator CreateCalculator(params IShippingStrategy[] extraStrategies)
    {
        IShippingStrategy[] defaults =
        [
            new StandardShipping(),
            new ExpressShipping(),
            new PickupShipping(),
        ];

        return new ShippingCalculator(defaults.Concat(extraStrategies));
    }

    [Theory]
    [InlineData("standard", 15.0)]
    [InlineData("express", 41.0)]
    [InlineData("pickup", 0.0)]
    public void Calculate_UsesTheRequestedStrategy(string method, double expected)
    {
        var calculator = CreateCalculator();

        var price = calculator.Calculate(method, TwoKgHundredKm);

        Assert.Equal((decimal)expected, price);
    }

    [Fact]
    public void Calculate_IgnoresCaseOfTheMethodName()
    {
        var calculator = CreateCalculator();

        var price = calculator.Calculate("EXPRESS", TwoKgHundredKm);

        Assert.Equal(41m, price);
    }

    [Fact]
    public void Calculate_UnknownMethod_Throws()
    {
        var calculator = CreateCalculator();

        Assert.Throws<ArgumentException>(() => calculator.Calculate("teleport", TwoKgHundredKm));
    }

    [Fact]
    public void NewStrategy_WorksWithoutChangingTheCalculator()
    {
        // Open/Closed na prática: um novo método de frete é só mais uma classe.
        var calculator = CreateCalculator(new SameDayShipping());

        var price = calculator.Calculate("same-day", TwoKgHundredKm);

        Assert.Equal(60m, price);
    }

    [Theory]
    [InlineData(-1.0, 10.0)]
    [InlineData(1.0, -10.0)]
    public void ShippingRequest_NegativeValues_Throws(double weightKg, double distanceKm)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ShippingRequest((decimal)weightKg, (decimal)distanceKm));
    }

    [Theory]
    [InlineData("standard")]
    [InlineData("express")]
    [InlineData("pickup")]
    public void Refactoring_KeepsTheLegacyBehavior(string method)
    {
        // Teste de caracterização: a versão com Strategy devolve o mesmo que a versão com if/else.
        var withStrategy = CreateCalculator();

        var expected = LegacyShippingCalculator.Calculate(method, weightKg: 2m, distanceKm: 100m);
        var actual = withStrategy.Calculate(method, TwoKgHundredKm);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("standard")]
    [InlineData("express")]
    [InlineData("pickup")]
    public void DelegateVersion_MatchesTheClassBasedVersion(string method)
    {
        var classBased = CreateCalculator();
        var delegateBased = new DelegateShippingCalculator();

        Assert.Equal(
            classBased.Calculate(method, TwoKgHundredKm),
            delegateBased.Calculate(method, TwoKgHundredKm));
    }

    [Fact]
    public void DelegateVersion_AllowsRegisteringNewMethods()
    {
        var calculator = new DelegateShippingCalculator();
        calculator.Register("same-day", _ => 60m);

        Assert.Equal(60m, calculator.Calculate("same-day", TwoKgHundredKm));
    }

    private sealed class SameDayShipping : IShippingStrategy
    {
        public string Name => "same-day";

        public decimal Calculate(ShippingRequest request) => 60m;
    }
}
