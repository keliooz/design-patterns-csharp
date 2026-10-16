namespace DesignPatterns.Behavioral.Strategy.Bad;

/// <summary>
/// ❌ ANTI-EXEMPLO: cálculo de frete com if/else sobre uma string.
///
/// Problemas:
/// - Todo novo método de frete exige MODIFICAR esta classe (viola Open/Closed).
/// - A classe cresce sem limite e mistura regras de preço de métodos diferentes.
/// - O método é identificado por "string mágica": um erro de digitação só aparece em runtime.
/// - Cada regra não pode ser testada isoladamente, só através desta classe inteira.
/// </summary>
public class LegacyShippingCalculator
{
    public static decimal Calculate(string method, decimal weightKg, decimal distanceKm)
    {
        if (method == "standard")
        {
            return 10m + 1.5m * weightKg + 0.02m * distanceKm;
        }
        else if (method == "express")
        {
            return 25m + 3m * weightKg + 0.10m * distanceKm;
        }
        else if (method == "pickup")
        {
            return 0m;
        }

        throw new ArgumentException($"Método de frete desconhecido: '{method}'.", nameof(method));
    }
}
