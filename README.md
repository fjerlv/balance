# Balance

A Counter-Strike 2 plugin that dynamically adjusts player health based on kill/death difference.

## How It Works

At the start of each round, player health is adjusted:
- Players with more deaths than kills get bonus health (+20 HP per death deficit)
- Players with more kills than deaths keep normal health (100 HP)
- Health bonuses are skipped on pistol rounds (first round and first round after halftime)

Health changes are broadcast to all players in chat with a green [Balance] prefix.

Stats are automatically cleared on:
- Game end
- Warmup end (event and manual `mp_warmup_end` command)
- Win panel match
- Game restart (`mp_restartgame` command)

Player stats are also cleaned up when they disconnect.

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

## Build

```bash
dotnet build
```

The compiled plugin will be in `bin/Debug/net8.0/`.

## Installation

1. Build the plugin
2. Copy the following files to your CounterStrikeSharp plugins folder:
   ```
   csgo/addons/counterstrikesharp/plugins/Balance/
   ├── Balance.dll
   ├── Balance.deps.json
   └── Balance.pdb
   ```
3. Restart the server or load the plugin
