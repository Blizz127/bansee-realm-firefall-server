# Bansee Realm Firefall Server

A Firefall private server based on [The Melding Wars PIN](https://github.com/themeldingwars/PIN) (Pirate Intelligence Network). The goal is a playable **character create → character select → Enter World** path that drops you in **Copacabana**, New Eden’s retail first hub, with NPC and facility placements taken from TMW packet dumps first, then wiki maps.

Upstream PIN still applies: *Fight the Accord — Kill the Chosen*.

https://user-images.githubusercontent.com/920861/134824107-03e9f99c-b420-47c7-b742-efe68967161c.mp4

This fork is **[Blizz127/bansee-realm-firefall-server](https://github.com/Blizz127/bansee-realm-firefall-server)**.

## What works now

- Create a character (name, frame, appearance) and have it persist across WebHost and GameServer processes.
- Login, character select, and Enter World into New Eden (zone 448).
- Open World automatch resolves **Copacabana Beta** (the client looks up that zone name).
- New characters spawn at **Copacabana** (outpost 23), not Watchtower: Lagoa Rasa (outpost 17).
- Copacabana plaza is seeded from **2015/2016 TMW gameplay dumps**: named NPCs, SIN, garages, printers, pads, Larry terminals, Codex kiosks, and the rest of New Eden’s packet-captured world props.
- Starter stats match a first-hour freelancer (level 1, 2500 HP) instead of PIN’s old endgame L45 / 19192 HP loadout.
- Reconnect after a client crash or Tailscale blip instead of being kicked because the character was still zoned in.

## What changed and why

### Copacabana is the first hub

PIN’s New Eden default was **Watchtower: Lagoa Rasa (outpost 17)** at `(176.65, 250.13, 491.94)` — the metal hangar on the Praia Tropical minimap. Retail Firefall’s first hub is **Copacabana (outpost 23 / SDB loc 135929)**.

| | PIN default | Retail / this fork |
| --- | --- | --- |
| Outpost | 17 Watchtower: Lagoa Rasa | **23 Copacabana** |
| Player spawn | hangar deck | **`(-429.97, -383.74, 439.08)`** garage / BF area |
| SIN / outpost | n/a | **`(-312.56, -416.05, 417.76)`** — wiki SIN `(-315, -417)` |

If GRPC later restores a saved last-outpost of 17, login remaps it to 23 so you are not dumped back in the hangar.

Debug POIs (`watchtower`, `jacuzzi`) and PIN’s old Coral Forest test spawns (Aero, a fake Battleframe Station, a Thumper, a datapad piled on spawn 17) were removed from zone 448.

### Hub entities come from packets, not invented seeds

`npc.json` and `deployable.json` are treated as retail world data:

1. **TMW packet dumps first** (2015 StaticInfo names + XYZ, 2016 deployable ObserverView).
2. **Wiki maps / vendor lists** only when they match a capture (for example Supply Officer Cross at wiki `(-369, -455)`).
3. **Omit if unverified.** No guessed XYZ for Chai, Kaylee, Smithy, Tesla, Aero, Oilspill, Garland, or Claudia. Several of those are Cerrado / Thump Dump / Trans Hub characters in SDB, not Copa plaza.

Current New Eden custom spawn set (zone 448):

- **46 NPCs** — 41 Copacabana names from 2015 unique StaticInfo (plus Cross, Teobalde, Dean Winter, Shelby Gladwyn from packet + wiki) and **5 Lagoa Rasa watchtower guards** from the 2016 dump at the actual watchtower, not piled on the player.
- **339 deployables**, all `ref` prefixed `2016gameplay.*`. Copa plaza keeps packet hub props: Copacabana SIN Uplink (type **291**), battleframe garages/stations, molecular printers, army terminals, New You, Luau Larry Terminal (type **2231**, not PIN’s invented type 700), Marketplace, health/ammo pads, glider pads, Codex (type **3858**, same category as old SIN Imprint 735).

Stripped as non-retail:

- PIN-invented hub rows (Battleframe 395 / SIN 735 / Luau Larry 700 as handmade plaza seeds).
- 2016 capture-session leftovers: NPE/Chosen bodies and terminals, Fireworks Control Panel on the SIN, Proximity triggers, a stray Fire, player **Heavy Lift** calldowns.
- Exact dump duplicates (same type + XYZ scoped in twice).

Facings: Lagoa guards keep 2016 orientations. Copa NPCs use identity unless a 2015 dump quaternion was high-confidence (Rafaela Silva, Supply Officer Cross, Landing Pad Engineer, Jose Vargas / Wargrim who share a stand).

### Login, create, and network

- **File-backed character store** (`PIN_CHARACTER_STORE` or `~/firefall/run/created_characters.json`) so create and GameServer login share the same GUID, name, frame, and colors. Login never uses `GetLatest()` (that could load someone else’s visuals).
- **Mail claim POST** (`/api/v2/characters/{id}/mail/{id}/claim_attachments`) so Daily Login / Redeem All does not 405.
- **Server list** returns `Copacabana Beta` and `New Eden` with `PIN_PUBLIC_HOST:25000` so Open World automatch can JoinWorld.
- **UDP MTU 1200** (Tailscale/VPN path MTU) and SendTo no longer kills the process on `EMSGSIZE`.
- Matrix accepts post-HEHE KISS/ABRT on the assigned socket id and points the client at GameServer UDP **25001**.
- NPCs with **chassis 0** in SDB (Sergeant Dominick Atkinson, type 909) no longer NRE the shard: fallback collision, loadout collision assigned on the real entity, and a per-NPC try/catch so one bad type cannot take down New Eden.

## Known gaps (honest)

- Wiki vendors **Chai / Kaylee / Smithy & Tesla** have no Copa monster IDs + XYZ in these dumps.
- **Aero, Oilspill, Garland, Claudia** are not Copa plaza monsters in this SDB; they stay omitted.
- 1.6 moved jobs onto NPCs; there is **no Copacabana Job Board deployable type** in the 2016 dump (other hubs have typed boards).
- Wiki Armored Dropship / Melding Fragment Arcfolder coordinates do not match those deployable types in the 2016 plaza capture (supply crates sit near the old dropship pin).
- GRPC character fetch is still unused (GameServer falls back to the file store). Last-outpost persistence over GRPC is not live yet.
- Combat, AI, encounters, and most abilities remain PIN-level incomplete.

## Usage

**Note:** If you want to play around with the configuration, see the Development section below.

1. Install Firefall via Steam (`steam://install/227700`).
2. Edit `firefall.ini` in `steamapps/common/Firefall` (see below).
3. Use a PIN-patched `FirefallClient.exe` (keep a backup of the Steam original). Client **1962** (late 2016) is what this shard advertises.
4. Install the [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0).
5. Trust development certificates: `dotnet dev-certs https --trust`.
6. Start **WebHostManager**, **MatrixServer**, and **GameServer**.
7. Start Firefall, log in (blank fields are fine), create or select a character, **Enter World**.

Set `PIN_PUBLIC_HOST` to the address the client should JoinWorld (LAN IP, Tailscale, etc.). Default in this tree is `100.72.127.15`.

### firefall.ini

```ini
[Config]
OperatorHost = "localhost:4400"

[FilePaths]
AssetStreamPath = "http://localhost:4401/AssetStream/%ENVMNEMONIC%-%BUILDNUM%/"
VTRemotePath = "http://localhost:4401/vtex/%ENVMNEMONIC%-%BUILDNUM%/static.vtex"

[UI]
PlayIntroMovie = false
```

Point `OperatorHost` / asset URLs at your public host if the client is not on localhost.

### Features (PIN + this fork)

- Loading into New Eden / Copacabana
- Basic character movement, including jetpacks and gliders
- Character create with persisted appearance and starter frame
- Switch between battleframes with preconfigured loadouts
- Call down vehicles and some deployables
- Packet-placed Copacabana NPCs and hub terminals

### Limitations

- There is no full combat, projectile, or damage simulation
- Most of the UI does not work properly
- Most abilities are not fully working
- Vehicles only have physics if a player is driving (client-side)
- No AI / encounters / PvP yet

## Development

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).
2. Clone this repo (or upstream `git clone --recurse-submodules https://github.com/themeldingwars/PIN.git`).
3. Build the solution.
4. Edit `GameServer.dll.config` so `StaticDBPath`, `AssetDBPath`, and `MapsPath` point at a Firefall client tree (`clientdb.sd2`, `assetdb`, `maps`).
5. `dotnet dev-certs https --trust`
6. Start WebHostManager, GameServer, and MatrixServer together.
7. Point `firefall.ini` at Operator / asset hosts and start the client.

World seed files (do not hand-place PIN test junk in zone 448):

- `UdpHosts/GameServer/StaticDB/CustomData/npc.json`
- `UdpHosts/GameServer/StaticDB/CustomData/deployable.json`
- `UdpHosts/GameServer/Test/DataUtils.cs` — zone 448 default outpost **23**

### Web Hosts

CatchAll (4499 / 44399) is used for now, until the specific APIs are implemented.

| Host       | HTTP | HTTPS | Catch All |
|------------|------|-------|-----------|
| Operator   | 4400 | 44300 | ❌        |
| WebAsset   | 4401 | 44301 | ✔️        |
| ClientApi  | 4402 | 44302 | ❌        |
| InGame     | 4403 | 44303 | ❌        |
| WebAccount | 4404 | 44304 | ✔️        |
| Frontend   | 4405 | 44305 | ✔️        |
| Store      | 4406 | 44306 | ✔️        |
| Chat       | 4407 | 44307 | ❌        |
| Replay     | 4408 | 44308 | ✔️        |
| Web        | 4409 | 44309 | ✔️        |
| Market     | 4410 | 44310 | ✔️        |
| RedHanded  | 4411 | 44311 | ✔️        |

### UDP Servers

| Host          | UDP   |
|---------------|-------|
| Matrix Server | 25000 |
| Game Server   | 25001 |

## Credits

- [The Melding Wars / PIN](https://github.com/themeldingwars/PIN) — protocol, shard, and web hosts
- TMW 2015/2016 packet captures used for Copacabana / New Eden placements
- Firefall wiki (archive) for vendor pins and hub facility names, never as a substitute for dump XYZ
