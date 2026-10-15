using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.MenuCurator.Pages;
using Jellyfin.Plugin.MenuCurator.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MenuCurator.Filters;

/// <summary>
/// Rewrites the page list returned to the dashboard sidebar by <c>GET /web/ConfigurationPages?enableInMainMenu=true</c>.
/// </summary>
/// <remarks>
/// Fails open: if anything is unexpected, the server's own response is served unchanged.
/// </remarks>
public sealed class ConfigurationPagesResultFilter : IAsyncResultFilter
{
    private const string ControllerName = "Dashboard";
    private const string ActionName = "GetConfigurationPages";
    private const string MenuQueryKey = "enableInMainMenu";

    private readonly ILogger<ConfigurationPagesResultFilter> _logger;
    private readonly Func<MenuRules> _rulesProvider;
    private readonly Func<IReadOnlyList<MenuPage>> _pagesProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPagesResultFilter"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ConfigurationPagesResultFilter}"/> interface.</param>
    /// <param name="catalogue">The page catalogue, used to add pages their plugins leave out of the menu.</param>
    public ConfigurationPagesResultFilter(ILogger<ConfigurationPagesResultFilter> logger, PageCatalogue catalogue)
        : this(
            logger,
            static () => MenuRules.FromConfiguration(Plugin.Instance?.Configuration),
            () => catalogue.GetListing().Pages)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPagesResultFilter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="rulesProvider">Supplies the rules to apply on each request.</param>
    /// <param name="pagesProvider">Supplies every page that could be added.</param>
    internal ConfigurationPagesResultFilter(
        ILogger<ConfigurationPagesResultFilter> logger,
        Func<MenuRules> rulesProvider,
        Func<IReadOnlyList<MenuPage>> pagesProvider)
    {
        _logger = logger;
        _rulesProvider = rulesProvider;
        _pagesProvider = pagesProvider;
    }

    /// <inheritdoc />
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (IsSidebarRequest(context) && context.Result is ObjectResult result)
        {
            TryRewrite(result);
        }

        return next();
    }

    /// <summary>
    /// Derives a stable stand-in plugin id for a second menu entry from the same plugin.
    /// </summary>
    /// <param name="pluginId">The real plugin id.</param>
    /// <param name="pageName">The page name.</param>
    /// <returns>An id that is unique to the plugin and page.</returns>
    internal static Guid DeriveEntryId(Guid pluginId, string? pageName)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(pluginId.ToString("N") + "/" + pageName?.ToUpperInvariant()));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static bool IsSidebarRequest(ResultExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor action
            || !string.Equals(action.ControllerName, ControllerName, StringComparison.Ordinal)
            || !string.Equals(action.ActionName, ActionName, StringComparison.Ordinal))
        {
            return false;
        }

        // Only the sidebar's request is rewritten. The unfiltered list feeds the Settings links
        // on the Plugins pages and must stay as the server built it.
        var query = context.HttpContext.Request.Query;
        return query.TryGetValue(MenuQueryKey, out var values)
            && bool.TryParse(values.ToString(), out var enabled)
            && enabled;
    }

    // The sidebar keys its entries by plugin id, so two entries from one plugin would collide.
    // Only the sidebar reads this list, so later entries from the same plugin get a derived id.
    private static void EnsureUniquePluginIds(IEnumerable<object> entries, ReflectionMenuEntryAccessor accessor)
    {
        var seen = new HashSet<Guid>();
        foreach (var entry in entries)
        {
            if (accessor.GetPluginId(entry) is { } id && !seen.Add(id))
            {
                accessor.SetPluginId(entry, DeriveEntryId(id, accessor.GetName(entry)));
            }
        }
    }

    private void TryRewrite(ObjectResult result)
    {
        try
        {
            if (result.StatusCode is not (null or StatusCodes.Status200OK)
                || result.Value is not IList entries
                || entries.GetType() is not { IsGenericType: true } listType
                || listType.GetGenericArguments() is not [var entryType])
            {
                _logger.LogWarning("Unexpected ConfigurationPages result {ResultType}; serving it unchanged", result.Value?.GetType().FullName);
                return;
            }

            var rules = _rulesProvider();
            if (rules.IsEmpty)
            {
                return;
            }

            var accessor = new ReflectionMenuEntryAccessor(entryType);
            var curated = MenuRuleEngine.Apply(entries.Cast<object>(), accessor, rules, _pagesProvider);
            EnsureUniquePluginIds(curated, accessor);

            // Build the replacement as the same list type, so the server serialises it exactly as before.
            var rewritten = (IList)Activator.CreateInstance(listType)!;
            foreach (var entry in curated)
            {
                rewritten.Add(entry);
            }

            result.Value = rewritten;
            _logger.LogDebug("Menu rewritten: {Before} entries in, {After} out", entries.Count, rewritten.Count);
        }
#pragma warning disable CA1031 // Fail open: the dashboard must never break because of this plugin.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogWarning(ex, "Failed to rewrite the dashboard menu; serving the original list");
        }
    }
}
