using System.Diagnostics.CodeAnalysis;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.MenuCurator.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the menu rules. Pages without a rule keep their plugin's own setting.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Settable array round-trips through both the XML store and the JSON sent by the config page.")]
    public MenuEntryRule[] Entries { get; set; } = [];
}
