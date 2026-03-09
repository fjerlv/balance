# Balance

A Counter Strike 2 plugin for balancing health based on kills and deaths.

## How It Works

At the start of each round, player health is adjusted based on two factors:

**Team size bonus:** If your team has fewer players than the opposition, 100 HP is distributed per missing player divided among your team.
- Example: 1v2 (1 missing) → +100 HP each
- Example: 2v3 (1 missing) → +50 HP each
- Example: 3v5 (2 missing) → +66 HP each

**Performance bonus:** Players with more deaths than kills receive +20 HP per death deficit.
- Example: 2 kills, 5 deaths → +60 HP

Both bonuses stack. Players with more kills than deaths and on the larger (or equal) team keep normal health (100 HP).

Health bonuses are skipped on pistol rounds (first round and first round after halftime).

Health changes are broadcast to all players in chat with a green [Balance] prefix.

Stats are automatically cleared on:
- Game end
- Warmup end (event and manual `mp_warmup_end` command)
- Win panel match
- Game restart (`mp_restartgame` command)

Player stats are also cleaned up when they disconnect.

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

## Build

```bash
dotnet build
```

The compiled plugin will be in `bin/Debug/net10.0/`.

## Running Tests

Run all tests:

```bash
dotnet test
```

Run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

The test suite includes tests for the `HealthCalculator` class which handles the health bonus calculations.

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
