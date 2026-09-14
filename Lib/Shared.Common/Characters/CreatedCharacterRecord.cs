using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Common.Characters;

/// <summary>
/// Persisted character created via POST /api/v1/characters.
/// Shared between WebHosts and GameServer via JSON file.
/// </summary>
public class CreatedCharacterRecord
{
    public const uint NewEdenZoneId = 448;

    /// <summary>
    /// JS/Lua safe integer range. Low byte 0xFE = WebAPI character type (RIN/retail convention).
    /// </summary>
    public const ulong MaxSafeGuid = 9007199254740991UL; // 2^53 - 1

    public ulong CharacterGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = "male";
    public int StartClassId { get; set; }
    public int VoiceSet { get; set; }
    public int Head { get; set; }
    public int HeadAccessoryA { get; set; }
    public int HeadAccessoryB { get; set; }
    public int EyeColorId { get; set; }
    public int SkinColorId { get; set; }
    public int HairColorId { get; set; }
    public uint EyeColor { get; set; }
    public uint SkinColor { get; set; }
    public uint HairColor { get; set; }
    public uint LipColor { get; set; } = 0xffff0000u;
    public bool IsDev { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC time soft-delete was requested; null while active.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Retail-style grace period before permanent wipe (client shows countdown).</summary>
    public static readonly TimeSpan SoftDeleteGrace = TimeSpan.FromDays(7);

    [JsonIgnore]
    public int GenderByte => string.Equals(Gender, "female", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

    [JsonIgnore]
    public bool IsDeleted => DeletedAt.HasValue;

    [JsonIgnore]
    public long ExpiresInSeconds
    {
        get
        {
            if (!DeletedAt.HasValue)
            {
                return 0;
            }

            var remaining = (DeletedAt.Value + SoftDeleteGrace) - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? (long)remaining.TotalSeconds : 0;
        }
    }

    /// <summary>index 0 → 0x1FE (510), index 1 → 0x2FE (766), …</summary>
    public static ulong GuidForNewEden(int index = 0) => ((ulong)(index + 1) << 8) | 0xFEUL;

    public static bool IsClientSafeGuid(ulong guid) => guid != 0 && guid <= MaxSafeGuid;
}

public class CreatedCharacterStoreFile
{
    public List<CreatedCharacterRecord> Characters { get; set; } = [];
}
