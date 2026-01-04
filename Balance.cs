using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Balance;

public static class HealthCalculator
{
    private const int BaseHealth = 100;
    private const int PlusHealthPerDeath = 20;

    public static int CalculateHealth(int kills, int deaths)
    {
        var difference = deaths - kills;
        return difference switch
        {
            > 0 => BaseHealth + difference * PlusHealthPerDeath,
            _ => BaseHealth
        };
    }
}

public static class PlayerColorHash
{
    private static readonly char[] Colors =
    [
        ChatColors.Purple,
        ChatColors.Gold,
        ChatColors.LightBlue,
        ChatColors.Red,
        ChatColors.Olive,
        ChatColors.Magenta,
        ChatColors.Yellow,
        ChatColors.Blue,
        ChatColors.LightRed,
        ChatColors.Green,
        ChatColors.BlueGrey,
        ChatColors.Orange,
        ChatColors.Lime,
        ChatColors.LightPurple
    ];

    public static char GetColor(string playerName)
    {
        var hash = playerName.Aggregate(0, (current, c) => current * 31 + c);
        var index = Math.Abs(hash) % Colors.Length;
        return Colors[index];
    }
}

public class Balance : BasePlugin
{
    public override string ModuleName => "Balance";
    public override string ModuleVersion => "0.0.3";

    private readonly Dictionary<ulong, int> _playerKills = new();
    private readonly Dictionary<ulong, int> _playerDeaths = new();

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Balance plugin loaded!");
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventGameEnd>(OnGameEnd);
        RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
        RegisterEventHandler<EventCsWinPanelMatch>(OnCsWinPanelMatch);
        AddCommandListener("mp_warmup_end", OnWarmupEndCommand);
        AddCommandListener("mp_restartgame", OnRestartGameCommand);
    }
    
    private HookResult OnGameEnd(EventGameEnd @event, GameEventInfo info)
    {
        ClearPlayerStats("game end");
        return HookResult.Continue;
    }

    private HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        ClearPlayerStats("warmup end");
        return HookResult.Continue;
    }

    private HookResult OnWarmupEndCommand(CCSPlayerController? player, CommandInfo info)
    {
        ClearPlayerStats("warmup end (manual)");
        return HookResult.Continue;
    }

    private HookResult OnRestartGameCommand(CCSPlayerController? player, CommandInfo info)
    {
        ClearPlayerStats("restart game");
        return HookResult.Continue;
    }

    private HookResult OnCsWinPanelMatch(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        ClearPlayerStats("win panel match");
        return HookResult.Continue;
    }

    private void ClearPlayerStats(string reason)
    {
        _playerKills.Clear();
        _playerDeaths.Clear();
        PrintToAllChat($"Player stats cleared at {reason}.");
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
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                var color = player.Team == CsTeam.CounterTerrorist ? ChatColors.Blue : ChatColors.Yellow;
                var healthColor = calculatedHealth > 100 ? $"{ChatColors.Green}" : "";
                var healthReset = calculatedHealth > 100 ? $"{ChatColors.Default}" : "";
                PrintToAllChat($"[{color}{player.PlayerName}{ChatColors.Default}] health: {healthColor}{calculatedHealth}{healthReset}, kills: {kills}, deaths: {deaths}, difference: {-difference}");
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

    private static void PrintToAllChat(string message)
    {
        Server.PrintToChatAll($" {ChatColors.Silver}[Balance]{ChatColors.Default} {message}");
    }
}