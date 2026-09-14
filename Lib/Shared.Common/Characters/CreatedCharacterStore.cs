using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Common.Characters;

/// <summary>
/// File-backed character store so WebHost create and GameServer login share state
/// across processes. Path: PIN_CHARACTER_STORE or ~/firefall/run/created_characters.json.
/// </summary>
public static class CreatedCharacterStore
{
    private static readonly object Gate = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string StorePath
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("PIN_CHARACTER_STORE");
            if (!string.IsNullOrWhiteSpace(env))
            {
                return env;
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "firefall", "run", "created_characters.json");
        }
    }

    public static IReadOnlyList<CreatedCharacterRecord> GetAll()
    {
        lock (Gate)
        {
            return LoadUnlocked().Characters.ToList();
        }
    }

    public static CreatedCharacterRecord GetByGuid(ulong characterGuid)
    {
        lock (Gate)
        {
            var file = LoadUnlocked();
            var exact = file.Characters.FirstOrDefault(c => c.CharacterGuid == characterGuid);
            if (exact != null)
            {
                return exact;
            }

            var masked = characterGuid & ~0xffUL;
            return file.Characters.FirstOrDefault(c => (c.CharacterGuid & ~0xffUL) == masked);
        }
    }

    public static CreatedCharacterRecord GetLatest()
    {
        lock (Gate)
        {
            return LoadUnlocked().Characters
                                 .Where(c => !c.IsDeleted)
                                 .OrderByDescending(c => c.CreatedAt)
                                 .FirstOrDefault();
        }
    }

    public static CreatedCharacterRecord Create(CreatedCharacterRecord record)
    {
        lock (Gate)
        {
            var file = LoadUnlocked();
            if (file.Characters.Any(c => !c.IsDeleted &&
                                         string.Equals(c.Name, record.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("ERR_NAME_IN_USE");
            }

            // Reuse index slots so GUIDs stay dense after deletes.
            var nextIndex = 0;
            var used = file.Characters.Select(c => c.CharacterGuid).ToHashSet();
            while (used.Contains(CreatedCharacterRecord.GuidForNewEden(nextIndex)))
            {
                nextIndex++;
            }

            record.CharacterGuid = CreatedCharacterRecord.GuidForNewEden(nextIndex);
            record.CreatedAt = DateTime.UtcNow;
            record.DeletedAt = null;
            CharCreateColors.Resolve(record);
            file.Characters.Add(record);
            SaveUnlocked(file);
            return record;
        }
    }

    public static CreatedCharacterRecord SoftDelete(ulong characterGuid)
    {
        lock (Gate)
        {
            var file = LoadUnlocked();
            var record = FindUnlocked(file, characterGuid);
            if (record == null)
            {
                throw new InvalidOperationException("ERR_CHAR_NOT_FOUND");
            }

            if (record.IsDeleted)
            {
                throw new InvalidOperationException("ERR_CHAR_DELETED");
            }

            record.DeletedAt = DateTime.UtcNow;
            SaveUnlocked(file);
            return record;
        }
    }

    public static CreatedCharacterRecord Undelete(ulong characterGuid)
    {
        lock (Gate)
        {
            var file = LoadUnlocked();
            var record = FindUnlocked(file, characterGuid);
            if (record == null)
            {
                throw new InvalidOperationException("ERR_CHAR_NOT_FOUND");
            }

            record.DeletedAt = null;
            SaveUnlocked(file);
            return record;
        }
    }

    public static bool IsNameAvailable(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 4 || name.Length > 20)
        {
            return false;
        }

        lock (Gate)
        {
            return !LoadUnlocked().Characters.Any(c => !c.IsDeleted &&
                                                       string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static CreatedCharacterRecord FindUnlocked(CreatedCharacterStoreFile file, ulong characterGuid)
    {
        var exact = file.Characters.FirstOrDefault(c => c.CharacterGuid == characterGuid);
        if (exact != null)
        {
            return exact;
        }

        var masked = characterGuid & ~0xffUL;
        return file.Characters.FirstOrDefault(c => (c.CharacterGuid & ~0xffUL) == masked);
    }

    private static CreatedCharacterStoreFile LoadUnlocked()
    {
        try
        {
            var path = StorePath;
            if (!File.Exists(path))
            {
                return new CreatedCharacterStoreFile();
            }

            var json = File.ReadAllText(path);
            var file = JsonSerializer.Deserialize<CreatedCharacterStoreFile>(json, JsonOptions) ?? new CreatedCharacterStoreFile();
            file = MigrateGuidsIfNeeded(file);
            // Refresh packed colors from SDB-derived palette when IDs are present.
            var colorDirty = false;
            foreach (var c in file.Characters)
            {
                var beforeSkin = c.SkinColor;
                var beforeEye = c.EyeColor;
                var beforeHair = c.HairColor;
                CharCreateColors.Resolve(c);
                if (c.SkinColor != beforeSkin || c.EyeColor != beforeEye || c.HairColor != beforeHair)
                {
                    colorDirty = true;
                }
            }

            if (colorDirty)
            {
                SaveUnlocked(file);
            }

            return file;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CreatedCharacterStore] Failed to load {StorePath}: {ex.Message}");
            return new CreatedCharacterStoreFile();
        }
    }

    /// <summary>
    /// Older PIN builds used 0x99AABBCCDDEE… GUIDs that exceed JS safe integers and
    /// break the client character list. Remap them to small RIN-style IDs.
    /// </summary>
    private static CreatedCharacterStoreFile MigrateGuidsIfNeeded(CreatedCharacterStoreFile file)
    {
        var changed = false;
        for (var i = 0; i < file.Characters.Count; i++)
        {
            if (!CreatedCharacterRecord.IsClientSafeGuid(file.Characters[i].CharacterGuid))
            {
                file.Characters[i].CharacterGuid = CreatedCharacterRecord.GuidForNewEden(i);
                changed = true;
            }
        }

        if (changed)
        {
            SaveUnlocked(file);
        }

        return file;
    }

    private static void SaveUnlocked(CreatedCharacterStoreFile file)
    {
        var path = StorePath;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(file, JsonOptions));
    }
}
