using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    private const string NameProperty = "Name";

    private readonly ILogger<ConfigurationPagesResultFilter> _logger;
    private readonly Func<MenuRules> _rulesProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPagesResultFilter"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ConfigurationPagesResultFilter}"/> interface.</param>
    public ConfigurationPagesResultFilter(ILogger<ConfigurationPagesResultFilter> logger)
        : this(logger, static () => MenuRules.FromConfiguration(Plugin.Instance?.Configuration))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPagesResultFilter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="rulesProvider">Supplies the rules to apply on each request.</param>
    internal ConfigurationPagesResultFilter(ILogger<ConfigurationPagesResultFilter> logger, Func<MenuRules> rulesProvider)
    {
        _logger = logger;
        _rulesProvider = rulesProvider;
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

    private void TryRewrite(ObjectResult result)
    {
        try
        {
            if (result.StatusCode is not (null or StatusCodes.Status200OK) || result.Value is not IList entries)
            {
                _logger.LogWarning("Unexpected ConfigurationPages result {ResultType}; serving it unchanged", result.Value?.GetType().FullName);
                return;
            }

            var kept = MenuRuleEngine.Apply(entries.Cast<object>(), ReadName, _rulesProvider());

            // Build the replacement as the same list type, so the server serialises it exactly as before.
            if (Activator.CreateInstance(entries.GetType()) is not IList rewritten)
            {
                _logger.LogWarning("Cannot create a {ListType} for the rewritten menu; serving it unchanged", entries.GetType().FullName);
                return;
            }

            foreach (var entry in kept)
            {
                rewritten.Add(entry);
            }

            result.Value = rewritten;
            _logger.LogDebug("Menu rewritten: {Kept} of {Total} entries kept", rewritten.Count, entries.Count);
        }
#pragma warning disable CA1031 // Fail open: the dashboard must never break because of this plugin.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogWarning(ex, "Failed to rewrite the dashboard menu; serving the original list");
        }
    }

    private static string? ReadName(object entry)
    {
        var property = entry.GetType().GetProperty(NameProperty, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{entry.GetType().FullName} has no {NameProperty} property.");
        return property.GetValue(entry) as string;
    }
}
