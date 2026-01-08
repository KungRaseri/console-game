using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Abstractions;

/// <summary>
/// Interface for calculating passive ability bonuses.
/// </summary>
public interface IPassiveBonusCalculator
{
    /// <summary>
    /// Calculate total physical damage bonus from passive abilities.
    /// </summary>
    int GetPhysicalDamageBonus(Character character);

    /// <summary>
    /// Calculate total magic damage bonus from passive abilities.
    /// </summary>
    int GetMagicDamageBonus(Character character);

    /// <summary>
    /// Calculate total critical chance bonus from passive abilities.
    /// </summary>
    double GetCriticalChanceBonus(Character character);

    /// <summary>
    /// Calculate total dodge chance bonus from passive abilities.
    /// </summary>
    double GetDodgeChanceBonus(Character character);

    /// <summary>
    /// Calculate total defense bonus from passive abilities.
    /// </summary>
    int GetDefenseBonus(Character character);
}
