using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Characters;
using WebHost.ClientApi.Accounts.Models;
using WebHost.ClientApi.Characters.Models;

namespace WebHost.ClientApi.Accounts;

[ApiController]
public class AccountsController : ControllerBase
{
    private static ConcurrentDictionary<uint, GarageSlots> _garageSlots;

    // Client ZoneSelection builds one plate per character_limit. Advertising 40
    // leaves a long row of empty "create" slots and scrolls the real character off-screen.
    private const int MinCharacterSlots = 5;

    [Route("api/v2/accounts")]
    [HttpPost]
    public object CreateAccount([FromBody] CreateAccountPost post)
    {
        return new { error = false };
    }

    [Route("api/v2/accounts/login")]
    [HttpPost]
    public AccountStatus Login()
    {
        var owned = CreatedCharacterStore.GetAll().Count;
        var characterLimit = Math.Max(MinCharacterSlots, owned + 1);

        // Any username/password (including blank) is accepted — PIN has no real accounts.
        return new AccountStatus
               {
                   AccountId = 1,
                   CanLogin = true,
                   IsDev = false,
                   SteamAuthPrompt = false,
                   SkipPrecursor = false,
                   CaisStatus = new CaisStatus { Duration = 0, ExpiresAt = 0, State = "disabled" },
                   CharacterLimit = characterLimit,
                   IsVip = true,
                   VipExpiration = 0,
                   // Fixed past timestamp — "now" can confuse client clock/timesync checks.
                   CreatedAt = new DateTimeOffset(2017, 1, 3, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()
               };
    }

    [Route("api/v2/accounts/current/status")]
    [HttpGet]
    public object CurrentStatus()
    {
        return new CurrentStatus { IsActive = true, CanLogin = true, IsDev = false, IsBanned = false };
    }

    [Route("api/v2/accounts/change_language")]
    [HttpPost]
    public void ChangeLanguage()
    {
        Ok();
    }

    [Route("api/v2/accounts/character_slots")]
    [HttpGet]
    public object CharacterSlots()
    {
        // Empty = no locked purchasable slots; client should not show unlock UI.
        return Array.Empty<object>();
    }

    [Route("api/v3/characters/{characterId}/titles")]
    [HttpGet]
    public object CharacterTitles(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return new { };
        }

        var characterTitles = new object[]
        {
            new Titles { Id = 117, Name = "Founder" },
            new Titles { Id = 128, Name = "Beta Commando" },
            new Titles { Id = 133, Name = "Beta Vanguard" },
            new Titles { Id = 135, Name = "Commander" },
            new Titles { Id = 136, Name = "Lieutenant" },
            new Titles { Id = 137, Name = "Ensign" },
            new Titles { Id = 144, Name = "Master Blaster" },
            new Titles { Id = 149, Name = "The Gun Show" },
            new Titles { Id = 150, Name = "Arc Runner" },
            new Titles { Id = 152, Name = "Pyromaniac" },
            new Titles { Id = 156, Name = "Barricade" },
            new Titles { Id = 158, Name = "Herald of Decay" },
            new Titles { Id = 171, Name = "Beta Trooper" }
        };

        return characterTitles;
    }

    [Route("api/v3/characters/{characterId}/garage_slots")]
    [HttpGet]
    [Produces("application/json")]
    public object GarageSlots(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return new { };
        }

        _garageSlots = new ConcurrentDictionary<uint, GarageSlots>();

        var craftingStation = new GarageSlots
                              {
                                  Id = 123987212,
                                  Name = "Crafting Station",
                                  CharacterGuid = ulong.Parse(characterId),
                                  GarageType = "crafting_station",
                                  ItemGuid = 9161555162510396669,
                                  EquippedSlots = Array.Empty<Array>(),
                                  Limits = new ItemLimits { Abilities = 4 },
                                  Decals = Array.Empty<Array>(),
                                  VisualLoadoutId = 0,
                                  WarpaintId = 0,
                                  Warpaintpatterns = Array.Empty<Array>(),
                                  VisualOverrides = Array.Empty<Array>(),
                                  Unlocked = true,
                                  ExpiresInSecs = 0
                              };
        _garageSlots.AddOrUpdate(craftingStation.Id, craftingStation, (k, nc) => nc);

        var firecat = new GarageSlots
                      {
                          Id = 184534131,
                          Name = "Astrek \"Firecat\"",
                          CharacterGuid = ulong.Parse(characterId),
                          GarageType = "battleframe",
                          ItemGuid = 9215052991608503805,
                          EquippedSlots = Array.Empty<Array>(),
                          Limits = new ItemLimits { Abilities = 4 },
                          Decals = new object[]
                                   {
                                       new Decals { SdbId = 10000, Color = 4294967295, Transform = new object[] { 0.05246, 0.019623, 0.0, 0.007484, -0.02002, -0.051758, 0.018127, -0.048492, 0.021362, 0.108154, -0.105469, 1.495117 } }
                                   },
                          VisualLoadoutId = 31240112,
                          WarpaintId = 77307,
                          Warpaintpatterns = new object[] { new Warpaintpatterns { SdbId = 10022, Transform = new object[] { 0.0, 16384.0, 418.0, 0.0 }, Usage = 0 } },
                          VisualOverrides = Array.Empty<Array>(),
                          Unlocked = true,
                          ExpiresInSecs = 0
                      };
        _garageSlots.AddOrUpdate(firecat.Id, firecat, (k, nc) => nc);

        return _garageSlots.Values;
    }

    [Route("api/v3/characters/{characterId}/garage_slots/{frameId}/perks")]
    [HttpGet]
    [Produces("application/json")]
    public object GarageSlotPerks(string characterId, string frameId)
    {
        if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(frameId))
        {
            return new { };
        }

        var framePerks = new GarageSlotPerks()
        {
            Perks = Array.Empty<Array>(),
            Respecs = 45,
            MaxPoints = 5
        };

        return framePerks;
    }

    // Temporary location
    [Route("api/v3/ui_actions")]
    [HttpPost]
    public void UIActions()
    {
        /*
         * POST is the following (example):
         *[
         *  {
         *    "screen_reference_id":48808253,
         *    "screen":"crafting",
         *    "action":"open"
         *  }
         *]
         *
         * What is this for? Logging?
         * Return seems to always have been empty.
         */

        Ok();
    }
}