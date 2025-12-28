using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Balance;

public static class HealthCalculator
{
    private const int BaseHealth = 100;
    private const int HealthPerDeathDeficit = 20;
    private const int HealthPerKillSurplus = 10;
    private const int MinHealth = 10;

    public static int CalculateHealth(int kills, int deaths)
    {
        var difference = deaths - kills;
        var calculatedHealth = difference switch
        {
            > 0 => BaseHealth + difference * HealthPerDeathDeficit,
            < 0 => BaseHealth + difference * HealthPerKillSurplus,
            _ => BaseHealth
        };

        return Math.Max(calculatedHealth, MinHealth);
    }
}

public class Balance : BasePlugin
{
    public override string ModuleName => "Balance";
    public override string ModuleVersion => "0.0.2";

    private readonly Dictionary<ulong, int> _playerKills = new();
    private readonly Dictionary<ulong, int> _playerDeaths = new();

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Balance plugin loaded!");
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventGameEnd>(OnGameEnd);
        RegisterEventHandler<EventBeginNewMatch>(OnBeginNewMatch);
        RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
    }
    
    private HookResult OnGameEnd(EventGameEnd @event, GameEventInfo info)
    {
        ClearPlayerStats("game end");
        return HookResult.Continue;
    }

    private HookResult OnBeginNewMatch(EventBeginNewMatch @event, GameEventInfo info)
    {
        ClearPlayerStats("new match");
        return HookResult.Continue;
    }

    private HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        ClearPlayerStats("warmup end");
        return HookResult.Continue;
    }

    private void ClearPlayerStats(string reason)
    {
        _playerKills.Clear();
        _playerDeaths.Clear();
        Console.WriteLine($"[Balance] Player stats cleared at {reason}.");
    }
    
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        var players = Utilities.GetPlayers();
        foreach (var player in players.Where(p => p is { IsValid: true, PawnIsAlive: true }))
        {
            _playerKills.TryGetValue(player.SteamID, out var kills);
            _playerDeaths.TryGetValue(player.SteamID, out var deaths);

            var calculatedHealth = HealthCalculator.CalculateHealth(kills, deaths);
            var difference = deaths - kills;

            var pawn = player.PlayerPawn.Value;
            if (pawn != null)
            {
                pawn.MaxHealth = calculatedHealth;
                pawn.Health = calculatedHealth;
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                player.PrintToChat($"[Balance] [{player.PlayerName}] health: {calculatedHealth}, kills: {kills}, deaths: {deaths}, difference: {difference}");
            }
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        if (victim != null && victim.IsValid)
        {
            IncrementStat(_playerDeaths, victim.SteamID);
        }

        var attacker = @event.Attacker;
        if (attacker != null && attacker.IsValid && attacker.PlayerPawn.IsValid && attacker != victim)
        {
            IncrementStat(_playerKills, attacker.SteamID);
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;
        _playerKills.Remove(player.SteamID);
        _playerDeaths.Remove(player.SteamID);

        return HookResult.Continue;
    }

    private static void IncrementStat(IDictionary<ulong, int> stats, ulong steamId)
    {
        stats.TryGetValue(steamId, out var current);
        stats[steamId] = current + 1;
    }
}