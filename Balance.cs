using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
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

public class Balance : BasePlugin
{
    public override string ModuleName => "Balance";
    public override string ModuleVersion => "0.0.4";

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
        if (IsPistolRound())
        {
            return HookResult.Continue;
        }

        var players = Utilities.GetPlayers();
        foreach (var player in players.Where(p => p is { IsValid: true, PawnIsAlive: true }))
        {
            _playerKills.TryGetValue(player.SteamID, out var kills);
            _playerDeaths.TryGetValue(player.SteamID, out var deaths);

            var calculatedHealth = HealthCalculator.CalculateHealth(kills, deaths);

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;

            pawn.MaxHealth = calculatedHealth;
            pawn.Health = calculatedHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

            if (calculatedHealth > 100)
            {
                var teamColor = player.Team switch
                {
                    CsTeam.CounterTerrorist => ChatColors.Blue,
                    CsTeam.Terrorist => ChatColors.Yellow,
                    _ => ChatColors.White
                };
                PrintToAllChat(
                    $"[{teamColor}{player.PlayerName}{ChatColors.Default}] " +
                    $"health: {ChatColors.Green}{calculatedHealth}{ChatColors.Default}, " +
                    $"kills: {kills}, deaths: {deaths}, diff: {kills - deaths}");
            }
        }
        return HookResult.Continue;
    }

    private static bool IsPistolRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .FirstOrDefault()?.GameRules;

        if (gameRules == null) return false;

        var totalRoundsPlayed = gameRules.TotalRoundsPlayed;
        var maxRounds = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 24;
        var halfTimeRound = maxRounds / 2;

        return totalRoundsPlayed == 0 || totalRoundsPlayed == halfTimeRound;
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