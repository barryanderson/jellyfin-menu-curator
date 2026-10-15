using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MenuCurator.Pages;

/// <summary>
/// Lists the configuration pages registered by loaded plugins, the same way the server's ConfigurationPages endpoint does.
/// </summary>
public sealed class PageCatalogue
{
    private readonly IPluginManager _pluginManager;
    private readonly ILogger<PageCatalogue> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCatalogue"/> class.
    /// </summary>
    /// <param name="pluginManager">Instance of the <see cref="IPluginManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{PageCatalogue}"/> interface.</param>
    public PageCatalogue(IPluginManager pluginManager, ILogger<PageCatalogue> logger)
    {
        _pluginManager = pluginManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets the pages of all loaded plugins.
    /// </summary>
    /// <returns>The listing.</returns>
    public PageListing GetListing()
    {
        // Plugins with no instance are disabled or failed to load; the server lists no pages for them either.
        var sources = _pluginManager.Plugins
            .Select(p => p.Instance)
            .OfType<IPlugin>()
            .Select(ReadPages);

        return Build(sources);
    }

    /// <summary>
    /// Builds the listing from each plugin's registered pages.
    /// </summary>
    /// <param name="plugins">Each loaded plugin with its pages (empty if it has none).</param>
    /// <returns>The listing.</returns>
    internal static PageListing Build(IEnumerable<(Guid Id, string Name, IReadOnlyList<PluginPageInfo> Pages)> plugins)
    {
        var pages = new List<MenuPage>();
        var withoutPages = new List<string>();

        foreach (var (id, name, registered) in plugins)
        {
            var htmlPages = registered.Where(IsHtmlPage).ToList();
            if (htmlPages.Count == 0)
            {
                withoutPages.Add(name);
                continue;
            }

            pages.AddRange(htmlPages.Select(p => new MenuPage
            {
                Name = p.Name,
                DisplayName = string.IsNullOrWhiteSpace(p.DisplayName) ? name : p.DisplayName,
                PluginId = id,
                PluginName = name,
                EnableInMainMenu = p.EnableInMainMenu,
                MenuIcon = p.MenuIcon
            }));
        }

        withoutPages.Sort(StringComparer.OrdinalIgnoreCase);
        return new PageListing { Pages = pages, PluginsWithoutPages = withoutPages };
    }

    /// <summary>
    /// Gets a value indicating whether a registered page is an HTML page rather than a script or stylesheet.
    /// </summary>
    /// <param name="page">The registered page.</param>
    /// <returns><c>true</c> for HTML pages.</returns>
    /// <remarks>
    /// Page names are unreliable (for example "reportsjs"), so this uses the resource extension,
    /// which is also what the server uses to choose the response type.
    /// </remarks>
    internal static bool IsHtmlPage(PluginPageInfo page)
        => !string.IsNullOrWhiteSpace(page.Name)
            && (page.EmbeddedResourcePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                || page.EmbeddedResourcePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase));

    private (Guid Id, string Name, IReadOnlyList<PluginPageInfo> Pages) ReadPages(IPlugin plugin)
    {
        if (plugin is not IHasWebPages hasWebPages)
        {
            return (plugin.Id, plugin.Name, []);
        }

        try
        {
            return (plugin.Id, plugin.Name, hasWebPages.GetPages().ToList());
        }
#pragma warning disable CA1031 // One misbehaving plugin must not hide every other plugin's pages.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogWarning(ex, "Could not read the pages of plugin {PluginName}", plugin.Name);
            return (plugin.Id, plugin.Name, []);
        }
    }
}
