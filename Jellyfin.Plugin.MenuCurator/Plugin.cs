using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.MenuCurator.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MenuCurator;

/// <summary>
/// The Menu Curator plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// The name of the plugin's config page.
    /// </summary>
    public const string ConfigPageName = "MenuCurator";

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Menu Curator";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("c86ff527-927b-4653-abc1-4b7d6fbf1a09");

    /// <inheritdoc />
    public override string Description => "Choose which plugin pages appear in the dashboard menu.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                // In the menu by default so the plugin cannot hide itself out of reach.
                // If the user hides it anyway, it stays reachable from Dashboard > Plugins.
                Name = ConfigPageName,
                DisplayName = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace),
                EnableInMainMenu = true,
                MenuIcon = "tune"
            }
        ];
    }
}
