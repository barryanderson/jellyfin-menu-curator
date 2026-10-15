using System;

namespace Jellyfin.Plugin.MenuCurator.Pages;

/// <summary>
/// A plugin configuration page that can appear in the dashboard menu.
/// </summary>
public sealed class MenuPage
{
    /// <summary>
    /// Gets the page name. Used in the page link and as the rule key.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the label the menu shows by default.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the owning plugin's id.
    /// </summary>
    public required Guid PluginId { get; init; }

    /// <summary>
    /// Gets the owning plugin's name.
    /// </summary>
    public required string PluginName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the plugin itself puts this page in the menu.
    /// </summary>
    public required bool EnableInMainMenu { get; init; }

    /// <summary>
    /// Gets the plugin's own Material icon name for the page, if any.
    /// </summary>
    public string? MenuIcon { get; init; }
}
