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
}
