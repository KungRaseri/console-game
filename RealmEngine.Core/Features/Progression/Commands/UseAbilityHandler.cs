using RealmEngine.Core.Features.Progression.Services;
using RealmEngine.Shared.Models;
using RealmEngine.Shared.Utilities;
using RealmEngine.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Handles using an ability.
/// </summary>
public class UseAbilityHandler : IRequestHandler<UseAbilityCommand, UseAbilityResult>
{
    private readonly AbilityCatalogService _abilityCatalog;
    private readonly ILogger<UseAbilityHandler> _logger;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UseAbilityHandler"/> class.
    /// </summary>
    /// <param name="abilityCatalog">The ability catalog service.</param>
    /// <param name="logger">The logger.</param>
    public UseAbilityHandler(
        AbilityCatalogService abilityCatalog,
        ILogger<UseAbilityHandler>? logger = null)
    {
        _abilityCatalog = abilityCatalog ?? throw new ArgumentNullException(nameof(abilityCatalog));
        _logger = logger ?? NullLogger<UseAbilityHandler>.Instance;
    }

    /// <summary>
    /// Handles using an ability.
    /// </summary>
    /// <param name="request">The use ability command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The use ability result.</returns>
    public Task<UseAbilityResult> Handle(UseAbilityCommand request, CancellationToken cancellationToken)
    {
        // Verify ability is known
        if (!request.User.LearnedAbilities.TryGetValue(request.AbilityId, out var learnedAbility))
        {
            return Task.FromResult(new UseAbilityResult
            {
                Success = false,
                Message = "You don't know that ability!"
            });
        }

        var ability = _abilityCatalog.GetAbility(request.AbilityId);
        if (ability == null)
        {
            return Task.FromResult(new UseAbilityResult
            {
                Success = false,
                Message = "Ability not found!"
            });
        }

        // Check cooldown
        if (request.User.AbilityCooldowns.TryGetValue(request.AbilityId, out var cooldownRemaining) && cooldownRemaining > 0)
        {
            return Task.FromResult(new UseAbilityResult
            {
                Success = false,
                Message = $"{ability.DisplayName} is still cooling down ({cooldownRemaining} turns)."
            });
        }

        // Check mana cost
        if (request.User.Mana < ability.ManaCost)
        {
            return Task.FromResult(new UseAbilityResult
            {
                Success = false,
                Message = $"Not enough mana! {ability.DisplayName} requires {ability.ManaCost} mana."
            });
        }

        // Deduct mana
        request.User.Mana -= ability.ManaCost;

        // Calculate damage/healing based on ability type
        int damageDealt = 0;
        int healingDone = 0;

        if (!string.IsNullOrEmpty(ability.BaseDamage))
        {
            damageDealt = DiceRoller.RollDiceString(ability.BaseDamage);
            
            // Apply character's magic damage bonus if it's a magical ability
            if (ability.Traits.ContainsKey("damageType") && 
                ability.Traits["damageType"]?.ToString()?.Contains("magic", StringComparison.OrdinalIgnoreCase) == true)
            {
                damageDealt += request.User.GetMagicDamageBonus();
            }
            else
            {
                damageDealt += request.User.GetPhysicalDamageBonus();
            }

            // Apply damage to target
            if (request.TargetEnemy != null)
            {
                request.TargetEnemy.Health = Math.Max(0, request.TargetEnemy.Health - damageDealt);
            }
            else if (request.TargetCharacter != null)
            {
                request.TargetCharacter.Health = Math.Max(0, request.TargetCharacter.Health - damageDealt);
            }
        }

        // Check for healing abilities
        if (ability.Type == AbilityTypeEnum.Healing || 
            (ability.Traits.ContainsKey("effect") && 
             ability.Traits["effect"]?.ToString()?.Contains("heal", StringComparison.OrdinalIgnoreCase) == true))
        {
            if (!string.IsNullOrEmpty(ability.BaseDamage))
            {
                healingDone = DiceRoller.RollDiceString(ability.BaseDamage);
                
                // Apply to target or self
                var healTarget = request.TargetCharacter ?? request.User;
                healTarget.Health = Math.Min(healTarget.MaxHealth, healTarget.Health + healingDone);
            }
        }

        // Apply cooldown
        if (ability.Cooldown > 0)
        {
            request.User.AbilityCooldowns[request.AbilityId] = ability.Cooldown;
        }

        // Update usage statistics
        learnedAbility.TimesUsed++;

        _logger.LogInformation(
            "Character {Character} used ability {Ability} (Damage: {Damage}, Healing: {Healing})",
            request.User.Name, ability.DisplayName, damageDealt, healingDone);

        // Build result message
        string message = $"Used {ability.DisplayName}!";
        if (damageDealt > 0)
            message += $" Dealt {damageDealt} damage.";
        if (healingDone > 0)
            message += $" Healed {healingDone} HP.";

        return Task.FromResult(new UseAbilityResult
        {
            Success = true,
            Message = message,
            DamageDealt = damageDealt,
            HealingDone = healingDone,
            ManaCost = ability.ManaCost,
            AbilityUsed = ability
        });
    }
}
