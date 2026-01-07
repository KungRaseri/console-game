using MediatR;
using RealmEngine.Shared.Models;
using RealmEngine.Shared.Services;
using RealmEngine.Core.Abstractions;
using Serilog;

namespace RealmEngine.Core.Features.Equipment.Commands;

/// <summary>
/// Handler for equipping items.
/// </summary>
public class EquipItemHandler : IRequestHandler<EquipItemCommand, EquipItemResult>
{
    private readonly ICharacterRepository _characterRepository;

    public EquipItemHandler(ICharacterRepository characterRepository)
    {
        _characterRepository = characterRepository;
    }

    public async Task<EquipItemResult> Handle(EquipItemCommand request, CancellationToken cancellationToken)
    {
        var character = await _characterRepository.GetByIdAsync(request.CharacterId);
        if (character == null)
        {
            return new EquipItemResult
            {
                Success = false,
                Message = "Character not found"
            };
        }

        // Find item in inventory
        var item = character.Inventory.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null)
        {
            return new EquipItemResult
            {
                Success = false,
                Message = "Item not found in inventory"
            };
        }

        // Track previous item and its abilities
        Item? previousItem = null;
        var revokedAbilities = new List<string>();
        
        // Get current equipment in slot
        previousItem = GetItemInSlot(character, request.Slot);
        
        // Revoke abilities from previous item
        if (previousItem != null)
        {
            var previousAbilities = character.EquipmentGrantedAbilities
                .Where(kvp => kvp.Value == previousItem.Id)
                .Select(kvp => kvp.Key)
                .ToList();
            revokedAbilities.AddRange(previousAbilities);
            
            EquipmentAbilityService.RevokeAbilitiesFromItem(character, previousItem);
        }

        // Equip new item
        SetItemInSlot(character, request.Slot, item);

        // Grant abilities from new item
        var grantedAbilities = new List<string>();
        EquipmentAbilityService.GrantAbilitiesFromItem(character, item);
        
        // Track newly granted abilities
        grantedAbilities = character.EquipmentGrantedAbilities
            .Where(kvp => kvp.Value == item.Id)
            .Select(kvp => kvp.Key)
            .ToList();

        // Save character
        await _characterRepository.UpdateAsync(character);

        Log.Information("Character {CharacterId} equipped {ItemName} in {Slot} (granted {AbilityCount} abilities)",
            character.Id, item.Name, request.Slot, grantedAbilities.Count);

        return new EquipItemResult
        {
            Success = true,
            Message = $"Equipped {item.Name}",
            PreviousItem = previousItem,
            GrantedAbilities = grantedAbilities,
            RevokedAbilities = revokedAbilities
        };
    }

    private Item? GetItemInSlot(Character character, EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.MainHand => character.EquippedMainHand,
            EquipmentSlot.OffHand => character.EquippedOffHand,
            EquipmentSlot.Head => character.EquippedHelmet,
            EquipmentSlot.Shoulders => character.EquippedShoulders,
            EquipmentSlot.Chest => character.EquippedChest,
            EquipmentSlot.Bracers => character.EquippedBracers,
            EquipmentSlot.Gloves => character.EquippedGloves,
            EquipmentSlot.Belt => character.EquippedBelt,
            EquipmentSlot.Legs => character.EquippedLegs,
            EquipmentSlot.Boots => character.EquippedBoots,
            EquipmentSlot.Necklace => character.EquippedNecklace,
            EquipmentSlot.Ring1 => character.EquippedRing1,
            EquipmentSlot.Ring2 => character.EquippedRing2,
            _ => null
        };
    }

    private void SetItemInSlot(Character character, EquipmentSlot slot, Item? item)
    {
        switch (slot)
        {
            case EquipmentSlot.MainHand:
                character.EquippedMainHand = item;
                break;
            case EquipmentSlot.OffHand:
                character.EquippedOffHand = item;
                break;
            case EquipmentSlot.Head:
                character.EquippedHelmet = item;
                break;
            case EquipmentSlot.Shoulders:
                character.EquippedShoulders = item;
                break;
            case EquipmentSlot.Chest:
                character.EquippedChest = item;
                break;
            case EquipmentSlot.Bracers:
                character.EquippedBracers = item;
                break;
            case EquipmentSlot.Gloves:
                character.EquippedGloves = item;
                break;
            case EquipmentSlot.Belt:
                character.EquippedBelt = item;
                break;
            case EquipmentSlot.Legs:
                character.EquippedLegs = item;
                break;
            case EquipmentSlot.Boots:
                character.EquippedBoots = item;
                break;
            case EquipmentSlot.Necklace:
                character.EquippedNecklace = item;
                break;
            case EquipmentSlot.Ring1:
                character.EquippedRing1 = item;
                break;
            case EquipmentSlot.Ring2:
                character.EquippedRing2 = item;
                break;
        }
    }
}
