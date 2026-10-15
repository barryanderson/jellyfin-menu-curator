using System.Collections.Generic;

namespace Jellyfin.Plugin.MenuCurator.Pages;

/// <summary>
/// Every page the config page can offer, plus the loaded plugins that have none.
/// </summary>
public sealed class PageListing
{
    /// <summary>
    /// Gets the HTML configuration pages of all loaded plugins.
    /// </summary>
    public required IReadOnlyList<MenuPage> Pages { get; init; }

    /// <summary>
    /// Gets the names of loaded plugins that register no HTML page, and so cannot be in the menu.
    /// </summary>
    public required IReadOnlyList<string> PluginsWithoutPages { get; init; }
}
