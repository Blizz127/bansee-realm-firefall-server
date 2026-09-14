using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Common.Characters;
using WebHost.ClientApi.Characters.Models;
using WebHost.ClientApi.Models.Base;

namespace WebHost.ClientApi.Characters;

public class CharactersRepository : ICharactersRepository
{
    public CharactersList GetCharacters()
    {
        var created = CreatedCharacterStore.GetAll();
        return new CharactersList
               {
                   Characters = created.Select(ToListCharacter).ToList(),
                   IsDev = false,
                   RbBalance = 0,
                   NameChangeCost = 100
               };
    }

    public CreatedCharacterRecord CreateCharacter(CharacterCreate data)
    {
        var record = new CreatedCharacterRecord
                     {
                         Name = data.Name?.Trim() ?? string.Empty,
                         Gender = string.IsNullOrWhiteSpace(data.Gender) ? "male" : data.Gender.ToLowerInvariant(),
                         StartClassId = data.StartClassId,
                         VoiceSet = data.VoiceSet,
                         Head = data.Head,
                         HeadAccessoryA = data.HeadAccessoryA,
                         HeadAccessoryB = data.HeadAccessoryB,
                         EyeColorId = data.EyeColorId,
                         SkinColorId = data.SkinColorId,
                         HairColorId = data.HairColorId,
                         IsDev = data.IsDev,
                     };

        return CreatedCharacterStore.Create(record);
    }

    public CreatedCharacterRecord SoftDelete(ulong characterGuid) => CreatedCharacterStore.SoftDelete(characterGuid);

    public CreatedCharacterRecord Undelete(ulong characterGuid) => CreatedCharacterStore.Undelete(characterGuid);

    private static Character ToListCharacter(CreatedCharacterRecord record)
    {
        CharCreateColors.Resolve(record);
        var gender = record.GenderByte;
        var hairId = record.HeadAccessoryA;
        var accessories = new List<ColoredItem>
                          {
                              new() { Id = hairId, Value = new ColorValue { Color = record.HairColor } }
                          };
        if (record.HeadAccessoryB != 0)
        {
            accessories.Add(new ColoredItem { Id = record.HeadAccessoryB, Value = new ColorValue { Color = record.HairColor } });
        }

        return new Character
               {
                   CharacterGuid = record.CharacterGuid,
                   Name = record.Name,
                   UniqueName = record.Name,
                   IsDev = record.IsDev,
                   IsActive = true,
                   CreatedAt = TruncateUtc(record.CreatedAt),
                   TitleId = 0,
                   TimePlayedSecs = 0,
                   NeedsNameChange = false,
                   MaxFrameLevel = 10,
                   FrameSdbId = record.StartClassId,
                   CurrentLevel = 1,
                   Gender = gender,
                   CurrentGender = record.Gender,
                   EliteRank = 0,
                   LastSeenAt = TruncateUtc(DateTime.UtcNow),
                   Visuals = new Visuals
                             {
                                 Id = 0,
                                 Race = 0,
                                 Gender = gender,
                                 SkinColor = new ColoredItem { Id = record.SkinColorId, Value = new ColorValue { Color = record.SkinColor } },
                                 VoiceSet = new Item { Id = record.VoiceSet },
                                 Head = new Item { Id = record.Head },
                                 EyeColor = new ColoredItem { Id = record.EyeColorId, Value = new ColorValue { Color = record.EyeColor } },
                                 LipColor = new ColoredItem { Id = 1, Value = new ColorValue { Color = record.LipColor } },
                                 HairColor = new ColoredItem { Id = record.HairColorId, Value = new ColorValue { Color = record.HairColor } },
                                 FacialHairColor = new ColoredItem { Id = record.HairColorId, Value = new ColorValue { Color = record.HairColor } },
                                 HeadAccessories = accessories,
                                 Ornaments = new List<ColoredItem>(),
                                 Eyes = new Item { Id = 10001 },
                                 Hair = new HairItem { Id = hairId, Color = new ColorItem { Id = record.HairColorId, Value = record.HairColor } },
                                 FacialHair = new HairItem
                                              {
                                                  Id = record.HeadAccessoryB,
                                                  Color = new ColorItem { Id = record.HairColorId, Value = record.HairColor }
                                              },
                                 Glider = new Item { Id = 0 },
                                 Vehicle = new Item { Id = 0 },
                                 Decals = new List<ColoredTransformableSdbItem>(),
                                 WarpaintId = 143225,
                                 Warpaint = CharCreateColors.AccordChassisWarpaint.ToList(),
                                 Decalgradients = new List<long>(),
                                 WarpaintPatterns = new List<WarpaintPattern>(),
                                 VisualOverrides = new List<long>()
                             },
                   Gear = CharCreateStarterGear.ForChassis(record.StartClassId)
                                              .Select(slot => new Gear
                                                              {
                                                                  SlotTypeId = slot.SlotTypeId,
                                                                  SdbId = slot.SdbId,
                                                                  ItemGuid = CharCreateStarterGear.ItemGuid(record.CharacterGuid, slot.SlotTypeId)
                                                              })
                                              .ToList(),
                   ExpiresIn = record.ExpiresInSeconds,
                   DeletedAt = record.DeletedAt.HasValue
                                   ? new DateTimeOffset(DateTime.SpecifyKind(record.DeletedAt.Value, DateTimeKind.Utc)).ToUnixTimeSeconds()
                                   : null,
                   Race = "human",
                   Migrations = new List<int>()
               };
    }

    private static DateTime TruncateUtc(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, DateTimeKind.Utc);
    }
}
