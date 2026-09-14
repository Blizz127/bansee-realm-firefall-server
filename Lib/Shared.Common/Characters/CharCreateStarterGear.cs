using System.Collections.Generic;

namespace Shared.Common.Characters;

/// <summary>
/// Char-select list gear for Accord starter frames, from dbcharacter::CharCreateLoadoutSlots
/// (create loadouts 286–290). Slot type ids match the retail characters/list gear payload.
/// </summary>
public static class CharCreateStarterGear
{
    public readonly record struct GearSlot(int SlotTypeId, int SdbId);

    // Assault 76164 (loadout 286), Dreadnaught 75772 (287), Biotech 75774 (288),
    // Engineer 75775 (289), Recon 75773 (290).
    private static readonly Dictionary<int, GearSlot[]> ByChassis = new()
    {
        [76164] =
        [
            new(1, 86770),
            new(2, 87769),
            new(6, 88491),
            new(116, 126107),
            new(122, 129240),
            new(126, 127568),
            new(127, 128298),
            new(128, 126838),
            new(129, 129094),
        ],
        [75772] =
        [
            new(1, 86879),
            new(2, 87769),
            new(6, 125199),
            new(116, 126107),
            new(122, 129240),
            new(126, 127568),
            new(127, 128298),
            new(128, 126838),
            new(129, 129094),
        ],
        [75774] =
        [
            new(1, 87056),
            new(2, 87769),
            new(6, 89124),
            new(116, 126107),
            new(122, 129240),
            new(126, 127568),
            new(127, 128298),
            new(128, 126838),
            new(129, 129094),
        ],
        [75775] =
        [
            new(1, 87414),
            new(2, 87769),
            new(6, 91559),
            new(116, 126107),
            new(122, 129240),
            new(126, 127568),
            new(127, 128298),
            new(128, 126838),
            new(129, 129094),
        ],
        [75773] =
        [
            new(1, 86997),
            new(2, 87769),
            new(6, 91770),
            new(116, 126107),
            new(122, 129240),
            new(126, 127568),
            new(127, 128298),
            new(128, 126838),
            new(129, 129094),
        ],
    };

    public static GearSlot[] ForChassis(int startClassId)
    {
        return ByChassis.TryGetValue(startClassId, out var slots) ? slots : ByChassis[75774];
    }

    public static (string Name, string WebIcon) FrameLabel(int startClassId) => startClassId switch
    {
        76164 => ("Accord Assault", "Assault"),
        75772 => ("Accord Dreadnaught", "Dreadnaught"),
        75774 => ("Accord Biotech", "Biotech"),
        75775 => ("Accord Engineer", "Engineer"),
        75773 => ("Accord Recon", "Recon"),
        _ => ("Accord Biotech", "Biotech"),
    };

    public static int? SdbIdForSlot(int startClassId, int slotTypeId)
    {
        foreach (var slot in ForChassis(startClassId))
        {
            if (slot.SlotTypeId == slotTypeId)
            {
                return slot.SdbId;
            }
        }

        return null;
    }

    /// <summary>Deterministic JS-safe item GUID for list payloads.</summary>
    public static long ItemGuid(ulong characterGuid, int slotTypeId)
    {
        return (long)((characterGuid << 16) | (uint)(slotTypeId & 0xFFFF));
    }
}
