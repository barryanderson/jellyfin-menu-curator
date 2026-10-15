using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jellyfin.Plugin.MenuCurator.Configuration;

namespace Jellyfin.Plugin.MenuCurator.Rules;

/// <summary>
/// The rules applied to the dashboard menu.
/// </summary>
public sealed class MenuRules
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuRules"/> class.
    /// </summary>
    /// <param name="hiddenNames">Page names to remove from the menu.</param>
    public MenuRules(IEnumerable<string> hiddenNames)
    {
        // Page names are matched case-insensitively, as the server does when serving a page.
        HiddenNames = new ReadOnlySet<string>(new HashSet<string>(hiddenNames, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets rules that leave the menu unchanged.
    /// </summary>
    public static MenuRules None { get; } = new([]);

    /// <summary>
    /// Gets the page names to remove from the menu.
    /// </summary>
    public IReadOnlySet<string> HiddenNames { get; }

    /// <summary>
    /// Builds the rules from the saved configuration.
    /// </summary>
    /// <param name="configuration">The configuration, or <c>null</c> if the plugin is not initialised.</param>
    /// <returns>The rules to apply.</returns>
    public static MenuRules FromConfiguration(PluginConfiguration? configuration)
    {
        if (configuration?.Entries is not { Length: > 0 } entries)
        {
            return None;
        }

        return new MenuRules(entries
            .Where(e => e is not null && !e.Visible && !string.IsNullOrWhiteSpace(e.Name))
            .Select(e => e.Name));
    }
}
