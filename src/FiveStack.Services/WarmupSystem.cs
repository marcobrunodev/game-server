using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace FiveStack;

// Vote types
public enum VoteType
{
    ChangeMap,
    ChangeMode
}

// Active vote state
public class VoteState
{
    public VoteType Type { get; set; }
    public string Description { get; set; } = "";
    public string InitiatorName { get; set; } = "";
    public HashSet<ulong> YesVotes { get; set; } = new();
    public HashSet<ulong> NoVotes { get; set; } = new();
    public Action? OnSuccess { get; set; }
    public CounterStrikeSharp.API.Modules.Timers.Timer? Timer { get; set; }
}

public class WarmupSystem
{
    private readonly ILogger<WarmupSystem> _logger;
    private readonly EnvironmentService _environmentService;
    private readonly GameServer _gameServer;

    // Current active vote (only one at a time)
    private VoteState? _activeVote;

    // Vote duration in seconds
    private const float VOTE_DURATION = 20f;

    // Available maps for warmup
    private static readonly string[] WarmupMaps = new[]
    {
        "ar_shoots",
        "ar_pool_day",
        "ar_baggage",
        "de_dust2",
        "de_mirage",
        "de_inferno",
        "de_anubis",
        "de_ancient",
        "de_nuke",
        "de_vertigo",
        "de_overpass",
        "cs_office",
        "cs_italy"
    };

    // Game modes
    private static readonly (string Name, int GameType, int GameMode)[] GameModes = new[]
    {
        ("Deathmatch", 1, 2),
        ("Arms Race", 1, 0),
        ("Retake", 3, 0),
        ("Competitive", 0, 1)
    };

    public WarmupSystem(
        ILogger<WarmupSystem> logger,
        EnvironmentService environmentService,
        GameServer gameServer)
    {
        _logger = logger;
        _environmentService = environmentService;
        _gameServer = gameServer;
    }

    public bool IsWarmupMode()
    {
        return _environmentService.IsWarmupMode();
    }

    // ===== BOT COMMANDS =====

    public void ShowBotsMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("🤖 Bot Settings");
        menu.AddMenuOption("Kick All Bots", (p, opt) => KickAllBots(p));
        menu.AddMenuOption("Add Bots (Fill)", (p, opt) => AddBots(p));
        menu.AddMenuOption("Set Bot Count...", (p, opt) => ShowBotCountMenu(p));
        menu.AddMenuOption("Bot Difficulty...", (p, opt) => ShowBotDifficultyMenu(p));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowBotCountMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🤖 Bot Count");
        menu.AddMenuOption("0 (No Bots)", (p, opt) => SetBotCount(p, 0));
        menu.AddMenuOption("2 Bots", (p, opt) => SetBotCount(p, 2));
        menu.AddMenuOption("4 Bots", (p, opt) => SetBotCount(p, 4));
        menu.AddMenuOption("6 Bots", (p, opt) => SetBotCount(p, 6));
        menu.AddMenuOption("8 Bots", (p, opt) => SetBotCount(p, 8));
        menu.AddMenuOption("10 Bots", (p, opt) => SetBotCount(p, 10));
        menu.AddMenuOption("16 Bots (Max)", (p, opt) => SetBotCount(p, 16));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowBotDifficultyMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🎯 Bot Difficulty");
        menu.AddMenuOption("Easy", (p, opt) => SetBotDifficulty(p, 0));
        menu.AddMenuOption("Normal", (p, opt) => SetBotDifficulty(p, 1));
        menu.AddMenuOption("Hard", (p, opt) => SetBotDifficulty(p, 2));
        menu.AddMenuOption("Expert", (p, opt) => SetBotDifficulty(p, 3));

        MenuManager.OpenChatMenu(player, menu);
    }

    public void KickAllBots(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        Server.ExecuteCommand("bot_kick");
        Server.ExecuteCommand("bot_quota 0");
        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} kicked all bots");
        _logger.LogInformation($"[Warmup] {player.PlayerName} kicked all bots");
    }

    public void AddBots(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        Server.ExecuteCommand("bot_quota 8");
        Server.ExecuteCommand("bot_quota_mode fill");
        Server.ExecuteCommand("bot_add_ct");
        Server.ExecuteCommand("bot_add_t");
        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} added bots to server");
        _logger.LogInformation($"[Warmup] {player.PlayerName} added bots");
    }

    private void SetBotCount(CCSPlayerController player, int count)
    {
        if (!IsWarmupMode()) return;

        if (count == 0)
        {
            Server.ExecuteCommand("bot_kick");
            Server.ExecuteCommand("bot_quota 0");
        }
        else
        {
            Server.ExecuteCommand($"bot_quota {count}");
            Server.ExecuteCommand("bot_quota_mode fill");
        }

        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} set bot count to {ChatColors.Yellow}{count}");
        _logger.LogInformation($"[Warmup] {player.PlayerName} set bot count to {count}");
    }

    private void SetBotDifficulty(CCSPlayerController player, int difficulty)
    {
        if (!IsWarmupMode()) return;

        string[] difficultyNames = { "Easy", "Normal", "Hard", "Expert" };
        Server.ExecuteCommand($"bot_difficulty {difficulty}");

        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} set bot difficulty to {ChatColors.Yellow}{difficultyNames[difficulty]}");
        _logger.LogInformation($"[Warmup] {player.PlayerName} set bot difficulty to {difficultyNames[difficulty]}");
    }

    // ===== MAP COMMANDS =====

    public void ShowMapMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("🗺️ Select Map");

        foreach (var map in WarmupMaps)
        {
            var mapName = map;
            menu.AddMenuOption(FormatMapName(map), (p, opt) => ChangeMap(p, mapName));
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ChangeMap(CCSPlayerController player, string mapName)
    {
        if (!IsWarmupMode()) return;

        // Start vote for map change
        StartVote(
            player,
            VoteType.ChangeMap,
            $"Change map to {ChatColors.Yellow}{FormatMapName(mapName)}",
            () =>
            {
                BroadcastMessage($"Map changing to {ChatColors.Yellow}{FormatMapName(mapName)}");
                _logger.LogInformation($"[Warmup] Vote passed: changing map to {mapName}");
                Server.NextFrame(() =>
                {
                    Server.ExecuteCommand($"changelevel {mapName}");
                });
            }
        );
    }

    private string FormatMapName(string mapName)
    {
        // Remove prefix and capitalize
        var name = mapName.Replace("de_", "").Replace("cs_", "").Replace("ar_", "");
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    // ===== MODE COMMANDS =====

    public void ShowModeMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("🎮 Game Mode");

        foreach (var mode in GameModes)
        {
            var modeCopy = mode;
            menu.AddMenuOption(mode.Name, (p, opt) => ChangeMode(p, modeCopy.Name, modeCopy.GameType, modeCopy.GameMode));
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ChangeMode(CCSPlayerController player, string modeName, int gameType, int gameMode)
    {
        if (!IsWarmupMode()) return;

        // Start vote for mode change
        StartVote(
            player,
            VoteType.ChangeMode,
            $"Change mode to {ChatColors.Yellow}{modeName}",
            () =>
            {
                Server.ExecuteCommand($"game_type {gameType}");
                Server.ExecuteCommand($"game_mode {gameMode}");

                BroadcastMessage($"Mode changing to {ChatColors.Yellow}{modeName}");
                BroadcastMessage($"{ChatColors.Grey}Map will restart to apply changes...");
                _logger.LogInformation($"[Warmup] Vote passed: changing mode to {modeName}");

                Server.NextFrame(() =>
                {
                    Server.ExecuteCommand("mp_restartgame 1");
                });
            }
        );
    }

    // ===== SETTINGS MENU =====

    public void ShowSettingsMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("⚙️ Settings");
        menu.AddMenuOption("Toggle Infinite Respawn", (p, opt) => ToggleRespawn(p));
        menu.AddMenuOption("Toggle God Mode (for you)", (p, opt) => ToggleGodMode(p));
        menu.AddMenuOption("Toggle Noclip (for you)", (p, opt) => ToggleNoclip(p));
        menu.AddMenuOption("Give Weapons...", (p, opt) => ShowGiveMenu(p));
        menu.AddMenuOption("Restart Round", (p, opt) => RestartRound(p));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ToggleRespawn(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        // Toggle respawn setting
        var currentValue = ConVar.Find("mp_respawn_on_death_ct")?.GetPrimitiveValue<int>() ?? 0;
        var newValue = currentValue == 1 ? 0 : 1;

        Server.ExecuteCommand($"mp_respawn_on_death_ct {newValue}");
        Server.ExecuteCommand($"mp_respawn_on_death_t {newValue}");

        var status = newValue == 1 ? "ENABLED" : "DISABLED";
        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} {status} infinite respawn");
        _logger.LogInformation($"[Warmup] {player.PlayerName} toggled respawn to {status}");
    }

    private void ToggleGodMode(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        // Toggle god mode for this player
        var isGod = pawn.TakesDamage;
        pawn.TakesDamage = !isGod;

        var status = !isGod ? "DISABLED" : "ENABLED";
        player.PrintToChat($" {ChatColors.Green}God Mode {status} for you");
        _logger.LogInformation($"[Warmup] {player.PlayerName} toggled god mode: {status}");
    }

    private void ToggleNoclip(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        // Toggle noclip
        var moveType = pawn.MoveType;
        if (moveType == MoveType_t.MOVETYPE_NOCLIP)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            player.PrintToChat($" {ChatColors.Green}Noclip DISABLED");
        }
        else
        {
            pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            player.PrintToChat($" {ChatColors.Green}Noclip ENABLED");
        }

        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        _logger.LogInformation($"[Warmup] {player.PlayerName} toggled noclip");
    }

    private void RestartRound(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        Server.ExecuteCommand("mp_restartgame 1");
        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} restarted the round");
        _logger.LogInformation($"[Warmup] {player.PlayerName} restarted round");
    }

    // ===== GIVE WEAPONS MENU =====

    public void ShowGiveMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("🔫 Give Weapon");
        menu.AddMenuOption("Rifles...", (p, opt) => ShowRiflesMenu(p));
        menu.AddMenuOption("SMGs...", (p, opt) => ShowSmgsMenu(p));
        menu.AddMenuOption("Pistols...", (p, opt) => ShowPistolsMenu(p));
        menu.AddMenuOption("Heavy...", (p, opt) => ShowHeavyMenu(p));
        menu.AddMenuOption("Grenades...", (p, opt) => ShowGrenadesMenu(p));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowRiflesMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🔫 Rifles");
        menu.AddMenuOption("AK-47", (p, opt) => GiveWeapon(p, "weapon_ak47"));
        menu.AddMenuOption("M4A4", (p, opt) => GiveWeapon(p, "weapon_m4a1"));
        menu.AddMenuOption("M4A1-S", (p, opt) => GiveWeapon(p, "weapon_m4a1_silencer"));
        menu.AddMenuOption("AWP", (p, opt) => GiveWeapon(p, "weapon_awp"));
        menu.AddMenuOption("SG 553", (p, opt) => GiveWeapon(p, "weapon_sg556"));
        menu.AddMenuOption("AUG", (p, opt) => GiveWeapon(p, "weapon_aug"));
        menu.AddMenuOption("FAMAS", (p, opt) => GiveWeapon(p, "weapon_famas"));
        menu.AddMenuOption("Galil AR", (p, opt) => GiveWeapon(p, "weapon_galilar"));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowSmgsMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🔫 SMGs");
        menu.AddMenuOption("MP9", (p, opt) => GiveWeapon(p, "weapon_mp9"));
        menu.AddMenuOption("MAC-10", (p, opt) => GiveWeapon(p, "weapon_mac10"));
        menu.AddMenuOption("MP7", (p, opt) => GiveWeapon(p, "weapon_mp7"));
        menu.AddMenuOption("UMP-45", (p, opt) => GiveWeapon(p, "weapon_ump45"));
        menu.AddMenuOption("P90", (p, opt) => GiveWeapon(p, "weapon_p90"));
        menu.AddMenuOption("PP-Bizon", (p, opt) => GiveWeapon(p, "weapon_bizon"));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowPistolsMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🔫 Pistols");
        menu.AddMenuOption("Desert Eagle", (p, opt) => GiveWeapon(p, "weapon_deagle"));
        menu.AddMenuOption("USP-S", (p, opt) => GiveWeapon(p, "weapon_usp_silencer"));
        menu.AddMenuOption("Glock-18", (p, opt) => GiveWeapon(p, "weapon_glock"));
        menu.AddMenuOption("P250", (p, opt) => GiveWeapon(p, "weapon_p250"));
        menu.AddMenuOption("Five-SeveN", (p, opt) => GiveWeapon(p, "weapon_fiveseven"));
        menu.AddMenuOption("Tec-9", (p, opt) => GiveWeapon(p, "weapon_tec9"));
        menu.AddMenuOption("CZ75-Auto", (p, opt) => GiveWeapon(p, "weapon_cz75a"));
        menu.AddMenuOption("Dual Berettas", (p, opt) => GiveWeapon(p, "weapon_elite"));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowHeavyMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("🔫 Heavy");
        menu.AddMenuOption("Nova", (p, opt) => GiveWeapon(p, "weapon_nova"));
        menu.AddMenuOption("XM1014", (p, opt) => GiveWeapon(p, "weapon_xm1014"));
        menu.AddMenuOption("MAG-7", (p, opt) => GiveWeapon(p, "weapon_mag7"));
        menu.AddMenuOption("Sawed-Off", (p, opt) => GiveWeapon(p, "weapon_sawedoff"));
        menu.AddMenuOption("M249", (p, opt) => GiveWeapon(p, "weapon_m249"));
        menu.AddMenuOption("Negev", (p, opt) => GiveWeapon(p, "weapon_negev"));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void ShowGrenadesMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("💣 Grenades");
        menu.AddMenuOption("HE Grenade", (p, opt) => GiveWeapon(p, "weapon_hegrenade"));
        menu.AddMenuOption("Flashbang", (p, opt) => GiveWeapon(p, "weapon_flashbang"));
        menu.AddMenuOption("Smoke Grenade", (p, opt) => GiveWeapon(p, "weapon_smokegrenade"));
        menu.AddMenuOption("Molotov", (p, opt) => GiveWeapon(p, "weapon_molotov"));
        menu.AddMenuOption("Incendiary", (p, opt) => GiveWeapon(p, "weapon_incgrenade"));
        menu.AddMenuOption("Decoy", (p, opt) => GiveWeapon(p, "weapon_decoy"));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void GiveWeapon(CCSPlayerController player, string weapon)
    {
        if (!IsWarmupMode()) return;

        player.GiveNamedItem(weapon);
        player.PrintToChat($" {ChatColors.Green}Received {weapon.Replace("weapon_", "")}");
    }

    // ===== HELP COMMAND =====

    public void ShowHelp(CCSPlayerController player)
    {
        if (!IsWarmupMode())
        {
            player.PrintToChat($" {ChatColors.Red}Warmup commands are only available in warmup mode");
            return;
        }

        player.PrintToChat($" {ChatColors.Yellow}═══ Warmup Commands ═══");
        player.PrintToChat($" {ChatColors.Green}.menu{ChatColors.White} - Open main menu");
        player.PrintToChat($" {ChatColors.Green}.bots{ChatColors.White} - Bot settings");
        player.PrintToChat($" {ChatColors.Green}.map{ChatColors.White} - Vote to change map");
        player.PrintToChat($" {ChatColors.Green}.mode{ChatColors.White} - Vote to change mode");
        player.PrintToChat($" {ChatColors.Green}.yes{ChatColors.White} / {ChatColors.Red}.no{ChatColors.White} - Vote");
        player.PrintToChat($" {ChatColors.Green}.settings{ChatColors.White} - Server settings");
        player.PrintToChat($" {ChatColors.Green}.give{ChatColors.White} - Give weapons");
        player.PrintToChat($" {ChatColors.Green}.help{ChatColors.White} - Show this help");
    }

    // ===== MAIN MENU =====

    public void ShowMainMenu(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        var menu = new ChatMenu("🎮 Warmup Menu");
        menu.AddMenuOption("🤖 Bots", (p, opt) => ShowBotsMenu(p));
        menu.AddMenuOption("🗺️ Change Map", (p, opt) => ShowMapMenu(p));
        menu.AddMenuOption("🎮 Game Mode", (p, opt) => ShowModeMenu(p));
        menu.AddMenuOption("⚙️ Settings", (p, opt) => ShowSettingsMenu(p));
        menu.AddMenuOption("🔫 Give Weapons", (p, opt) => ShowGiveMenu(p));
        menu.AddMenuOption("❓ Help", (p, opt) => ShowHelp(p));

        MenuManager.OpenChatMenu(player, menu);
    }

    // ===== VOTING SYSTEM =====

    private void StartVote(CCSPlayerController initiator, VoteType type, string description, Action onSuccess)
    {
        if (!IsWarmupMode()) return;

        // Check if vote already active
        if (_activeVote != null)
        {
            initiator.PrintToChat($" {ChatColors.Red}A vote is already in progress! Type .yes or .no to vote.");
            return;
        }

        // Create new vote
        _activeVote = new VoteState
        {
            Type = type,
            Description = description,
            InitiatorName = initiator.PlayerName,
            OnSuccess = onSuccess
        };

        // Initiator automatically votes yes
        _activeVote.YesVotes.Add(initiator.SteamID);

        // Broadcast vote started
        BroadcastMessage($"{ChatColors.Green}{initiator.PlayerName}{ChatColors.White} started a vote:");
        BroadcastMessage($"{description}");
        BroadcastMessage($"Type {ChatColors.Green}.yes{ChatColors.White} or {ChatColors.Red}.no{ChatColors.White} to vote (20 seconds)");

        _logger.LogInformation($"[Warmup] {initiator.PlayerName} started vote: {description}");

        // Start timer to end vote after 20 seconds
        _activeVote.Timer = new CounterStrikeSharp.API.Modules.Timers.Timer(VOTE_DURATION, EndVote, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void VoteYes(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        if (_activeVote == null)
        {
            player.PrintToChat($" {ChatColors.Red}No active vote. Use .mode or .map to start one.");
            return;
        }

        // Remove from no votes if they changed their mind
        _activeVote.NoVotes.Remove(player.SteamID);
        _activeVote.YesVotes.Add(player.SteamID);

        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} voted {ChatColors.Green}YES{ChatColors.White} ({_activeVote.YesVotes.Count} yes / {_activeVote.NoVotes.Count} no)");
        _logger.LogInformation($"[Warmup] {player.PlayerName} voted YES");
    }

    public void VoteNo(CCSPlayerController player)
    {
        if (!IsWarmupMode()) return;

        if (_activeVote == null)
        {
            player.PrintToChat($" {ChatColors.Red}No active vote. Use .mode or .map to start one.");
            return;
        }

        // Remove from yes votes if they changed their mind
        _activeVote.YesVotes.Remove(player.SteamID);
        _activeVote.NoVotes.Add(player.SteamID);

        BroadcastMessage($"{ChatColors.Green}{player.PlayerName}{ChatColors.White} voted {ChatColors.Red}NO{ChatColors.White} ({_activeVote.YesVotes.Count} yes / {_activeVote.NoVotes.Count} no)");
        _logger.LogInformation($"[Warmup] {player.PlayerName} voted NO");
    }

    private void EndVote()
    {
        if (_activeVote == null) return;

        var yesCount = _activeVote.YesVotes.Count;
        var noCount = _activeVote.NoVotes.Count;

        _logger.LogInformation($"[Warmup] Vote ended: {yesCount} yes, {noCount} no");

        if (yesCount > noCount)
        {
            // Vote passed
            BroadcastMessage($"Vote {ChatColors.Green}PASSED{ChatColors.White}! ({yesCount} yes / {noCount} no)");
            _activeVote.OnSuccess?.Invoke();
        }
        else
        {
            // Vote failed
            BroadcastMessage($"Vote {ChatColors.Red}FAILED{ChatColors.White}. ({yesCount} yes / {noCount} no)");
        }

        // Clean up
        _activeVote.Timer?.Kill();
        _activeVote = null;
    }

    public bool HasActiveVote()
    {
        return _activeVote != null;
    }

    // ===== UTILITY =====

    private void BroadcastMessage(string message)
    {
        _gameServer.Message(HudDestination.Chat, $" {ChatColors.Yellow}[Warmup]{ChatColors.White} {message}");
    }
}
