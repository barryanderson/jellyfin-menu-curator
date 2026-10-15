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
    /// </summary>
    public bool Visible { get; set; } = true;
}
