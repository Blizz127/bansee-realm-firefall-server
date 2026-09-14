using Microsoft.AspNetCore.Mvc;
using Shared.Common.Characters;

namespace WebHost.InGameApi.Controllers;

[ApiController]
public class CharactersData : ControllerBase
{
    [Route("character/data")]
    [Route("api/v1/character/data")]
    [HttpGet]
    [Produces("application/json")]
    public object Data([FromQuery] ulong? character_guid = null, [FromQuery] ulong? characterGuid = null)
    {
        var created = ResolveCreated(character_guid ?? characterGuid);
        var data = new Data
                   {
                       CharacterGuid = created?.CharacterGuid ?? CreatedCharacterRecord.GuidForNewEden(),
                       Name = created?.Name ?? "Freelancer",
                       Redbux = 1094,
                       Crystite = 4104594,
                       Gender = (uint)(created?.GenderByte ?? 0),
                       UniqueName = (created?.Name ?? "Freelancer").ToUpperInvariant(),
                       Race = 0
                   };

        return data;
    }

    [Route("api/v1/character_sheet.json")]
    [HttpGet]
    [Produces("application/json")]
    public object CharacterSheet([FromQuery] ulong? character_guid = null, [FromQuery] ulong? characterGuid = null)
    {
        var created = ResolveCreated(character_guid ?? characterGuid);
        var frameId = (uint)(created?.StartClassId ?? 75774);
        var (name, webIcon) = CharCreateStarterGear.FrameLabel((int)frameId);

        var sheet = new CharacterSheet
                    {
                        Battleframe = new Battleframe
                                      {
                                          ItemSdbId = frameId,
                                          Name = name,
                                          WebIcon = webIcon,
                                          Constraints = new Constraints
                                                        {
                                                            Mass = new MassPowerCpu
                                                                   {
                                                                       Level = new LevelValue
                                                                               {
                                                                                   Total = 10, Current = 1
                                                                               },
                                                                       Value = new LevelValue
                                                                               {
                                                                                   Total = 1000, Current = 0
                                                                               }
                                                                   },
                                                            Power = new MassPowerCpu
                                                                    {
                                                                        Level = new LevelValue
                                                                                {
                                                                                    Total = 10, Current = 1
                                                                                },
                                                                        Value = new LevelValue
                                                                                {
                                                                                    Total = 500, Current = 0
                                                                                }
                                                                    },
                                                            Cpu = new MassPowerCpu
                                                                  {
                                                                      Level = new LevelValue
                                                                              {
                                                                                  Total = 10, Current = 1
                                                                              },
                                                                      Value = new LevelValue
                                                                              {
                                                                                  Total = 500, Current = 0
                                                                              }
                                                                  }
                                                        },
                                          Xp = new Xp
                                               {
                                                   CurrentXp = 0, LifetimeXp = 0
                                               }
                                      }
                    };

        return sheet;
    }

    [Route("api/v1/character_sheet/equipped_items.json")]
    [HttpGet]
    [Produces("application/json")]
    public object EquippedItems([FromQuery] ulong? character_guid = null, [FromQuery] ulong? characterGuid = null)
    {
        var created = ResolveCreated(character_guid ?? characterGuid);
        var frameId = created?.StartClassId ?? 75774;
        uint Slot(int slotTypeId, uint fallback) =>
            (uint)(CharCreateStarterGear.SdbIdForSlot(frameId, slotTypeId) ?? (int)fallback);

        var equipped = new Equipped
                       {
                           Primary = new Item
                                     {
                                         ItemId = string.Empty, DefaultItemSdbId = Slot(1, 87056), IsUnlocked = true
                                     },
                           Secondary = new Item
                                       {
                                           ItemId = string.Empty, DefaultItemSdbId = Slot(2, 87769), IsUnlocked = true
                                       },
                           Ability1 = new Item
                                      {
                                          ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                      },
                           Ability2 = new Item
                                      {
                                          ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                      },
                           Ability3 = new Item
                                      {
                                          ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                      },
                           Hkm = new Item
                                 {
                                     ItemId = string.Empty, DefaultItemSdbId = Slot(6, 89124), IsUnlocked = true
                                 },
                           Passive = new Item
                                     {
                                         ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                     },
                           Jumpjets = new Item
                                      {
                                          ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                      },
                           Servos = new Item
                                    {
                                        ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                    },
                           Backpack = new Item
                                      {
                                          ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                      },
                           Plating = new Item
                                     {
                                         ItemId = string.Empty, DefaultItemSdbId = 0, IsUnlocked = true
                                     }
                       };

        return equipped;
    }

    private static CreatedCharacterRecord ResolveCreated(ulong? guid)
    {
        var created = guid is > 0
                          ? CreatedCharacterStore.GetByGuid(guid.Value)
                          : CreatedCharacterStore.GetLatest();
        if (created is { IsDeleted: true })
        {
            return null;
        }

        return created;
    }
}

public class Data
{
    public ulong CharacterGuid { get; set; }
    public string Name { get; set; }
    public uint Redbux { get; set; }
    public uint Crystite { get; set; }
    public uint Gender { get; set; }
    public string UniqueName { get; set; }
    public uint Race { get; set; }
}

public class Equipped
{
    public Item Primary { get; set; }
    public Item Secondary { get; set; }
    public Item Ability1 { get; set; }
    public Item Ability2 { get; set; }
    public Item Ability3 { get; set; }
    public Item Hkm { get; set; }
    public Item Passive { get; set; }
    public Item Jumpjets { get; set; }
    public Item Servos { get; set; }
    public Item Backpack { get; set; }
    public Item Plating { get; set; }
}

public class Item
{
    public string ItemId { get; set; }
    public uint DefaultItemSdbId { get; set; }
    public bool IsUnlocked { get; set; }
}

public class CharacterSheet
{
    public Battleframe Battleframe { get; set; }
}

public class Battleframe
{
    public uint ItemSdbId { get; set; }
    public string Name { get; set; }
    public string WebIcon { get; set; }
    public Constraints Constraints { get; set; }
    public Xp Xp { get; set; }
}

public class Constraints
{
    public MassPowerCpu Mass { get; set; }
    public MassPowerCpu Power { get; set; }
    public MassPowerCpu Cpu { get; set; }
}

public class Xp
{
    public uint CurrentXp { get; set; }
    public uint LifetimeXp { get; set; }
}

public class MassPowerCpu
{
    public LevelValue Level { get; set; }
    public LevelValue Value { get; set; }
}

public class LevelValue
{
    public uint Total { get; set; }
    public uint Current { get; set; }
}