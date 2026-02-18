using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace FiveStack;

public partial class FiveStackPlugin
{
    [ConsoleCommand("css_menu", "Open warmup main menu")]
    [ConsoleCommand("css_warmup", "Open warmup main menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnWarmupMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowMainMenu(player);
    }

    [ConsoleCommand("css_bots", "Open bots menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnBotsMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowBotsMenu(player);
    }

    [ConsoleCommand("css_kickbots", "Kick all bots")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnKickBots(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.KickAllBots(player);
    }

    [ConsoleCommand("css_addbots", "Add bots to server")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnAddBots(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.AddBots(player);
    }

    [ConsoleCommand("css_map", "Open map selection menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowMapMenu(player);
    }

    [ConsoleCommand("css_mode", "Open game mode menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnModeMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowModeMenu(player);
    }

    [ConsoleCommand("css_settings", "Open settings menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnSettingsMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowSettingsMenu(player);
    }

    [ConsoleCommand("css_give", "Open give weapons menu")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnGiveMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ShowGiveMenu(player);
    }

    [ConsoleCommand("css_whelp", "Show warmup commands help")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnWarmupHelp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        _warmupSystem.ShowHelp(player);
    }

    // Direct mode commands (no menu needed)
    [ConsoleCommand("css_dm", "Change to Deathmatch")]
    [ConsoleCommand("css_deathmatch", "Change to Deathmatch")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDeathmatch(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeToMode(player, "Deathmatch", 1, 2);
    }

    [ConsoleCommand("css_ar", "Change to Arms Race")]
    [ConsoleCommand("css_armsrace", "Change to Arms Race")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnArmsRace(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeToMode(player, "Arms Race", 1, 0);
    }

    [ConsoleCommand("css_retake", "Change to Retake")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnRetake(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeToMode(player, "Retake", 0, 1);  // Competitive mode with RetakesPlugin
    }

    // Voting commands
    [ConsoleCommand("css_yes", "Vote yes for mode change")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnVoteYes(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.VoteYes(player);
    }

    [ConsoleCommand("css_no", "Vote no for mode change")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnVoteNo(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.VoteNo(player);
    }

    // Direct map commands (bypass menu issues)
    [ConsoleCommand("css_dust2", "Change to de_dust2")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapDust2(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_dust2");
    }

    [ConsoleCommand("css_mirage", "Change to de_mirage")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapMirage(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_mirage");
    }

    [ConsoleCommand("css_inferno", "Change to de_inferno")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapInferno(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_inferno");
    }

    [ConsoleCommand("css_nuke", "Change to de_nuke")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapNuke(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_nuke");
    }

    [ConsoleCommand("css_ancient", "Change to de_ancient")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapAncient(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_ancient");
    }

    [ConsoleCommand("css_anubis", "Change to de_anubis")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapAnubis(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_anubis");
    }

    [ConsoleCommand("css_vertigo", "Change to de_vertigo")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapVertigo(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_vertigo");
    }

    [ConsoleCommand("css_overpass", "Change to de_overpass")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapOverpass(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "de_overpass");
    }

    [ConsoleCommand("css_office", "Change to cs_office")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapOffice(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "cs_office");
    }

    [ConsoleCommand("css_shoots", "Change to ar_shoots")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapShoots(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "ar_shoots");
    }

    [ConsoleCommand("css_baggage", "Change to ar_baggage")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapBaggage(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "ar_baggage");
    }

    [ConsoleCommand("css_poolday", "Change to ar_pool_day")]
    [ConsoleCommand("css_pool_day", "Change to ar_pool_day")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapPoolDay(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeMapDirect(player, "ar_pool_day");
    }

    [ConsoleCommand("css_rio", "Change to cs_rio")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnMapRio(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !_warmupSystem.IsWarmupMode()) return;
        _warmupSystem.ChangeWorkshopMap(player, "cs_rio", "3071179917");
    }
}
