using Jellyfin.Plugin.MenuCurator.Filters;
using Jellyfin.Plugin.MenuCurator.Pages;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MenuCurator;

/// <summary>
/// Registers the plugin's services with the server.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<PageCatalogue>();

        // A global MVC result filter runs after the action and before output formatting,
        // so it sees the page list as objects, ahead of JSON casing and response compression.
        serviceCollection.Configure<MvcOptions>(options => options.Filters.Add<ConfigurationPagesResultFilter>());
    }
}
