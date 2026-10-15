namespace Jellyfin.Plugin.MenuCurator.Configuration;

/// <summary>
/// The user's rule for one plugin page in the dashboard menu.
/// </summary>
public class MenuEntryRule
{
    /// <summary>
    /// Gets or sets the page name, as registered by its plugin.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the page is shown in the menu.
    /// Showing a page its plugin leaves out of the menu adds it.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the label to show instead of the plugin's own, or <c>null</c> to keep it.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the Material icon name to show instead of the plugin's own, or <c>null</c> to keep it.
    /// </summary>
    public string? MenuIcon { get; set; }

    /// <summary>
    /// Gets or sets the position in the menu, or <c>null</c> to place the page after all ordered pages.
    /// </summary>
    public int? Order { get; set; }
}
