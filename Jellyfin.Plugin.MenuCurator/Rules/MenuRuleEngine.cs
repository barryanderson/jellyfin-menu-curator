using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MenuCurator.Pages;

namespace Jellyfin.Plugin.MenuCurator.Rules;

/// <summary>
/// Applies <see cref="MenuRules"/> to a list of menu entries.
/// </summary>
/// <remarks>
/// Generic over the entry type because the server's ConfigurationPageInfo is not in a published package.
/// </remarks>
public static class MenuRuleEngine
{
    /// <summary>
    /// Applies the rules to the menu entries: hide, add, override, then order.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    /// <param name="entries">The entries, in the order the server returned them.</param>
    /// <param name="accessor">Reads, writes and creates entries.</param>
    /// <param name="rules">The rules to apply.</param>
    /// <param name="allPages">Supplies every page that could be added. Only called when a rule adds a page.</param>
    /// <returns>The entries to show, in menu order.</returns>
    public static IReadOnlyList<T> Apply<T>(
        IEnumerable<T> entries,
        IMenuEntryAccessor<T> accessor,
        MenuRules rules,
        Func<IReadOnlyList<MenuPage>> allPages)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(allPages);

        var result = new List<T>();
        var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Hide.
        foreach (var entry in entries)
        {
            var name = accessor.GetName(entry);
            if (name is not null)
            {
                present.Add(name);
            }

            if (rules.Find(name) is { Visible: false })
            {
                continue;
            }

            result.Add(entry);
        }

        // Add. Rules for pages that no longer exist are ignored.
        var toAdd = rules.ShownNames.Where(n => !present.Contains(n)).ToList();
        if (toAdd.Count > 0)
        {
            var pages = allPages();
            foreach (var name in toAdd)
            {
                var page = pages.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (page is not null)
                {
                    result.Add(accessor.Create(page));
                }
            }
        }

        // Override.
        foreach (var entry in result)
        {
            if (rules.Find(accessor.GetName(entry)) is not { } rule)
            {
                continue;
            }

            if (MenuRules.DisplayNameOf(rule) is { } displayName)
            {
                accessor.SetDisplayName(entry, displayName);
            }

            if (MenuRules.MenuIconOf(rule) is { } menuIcon)
            {
                accessor.SetMenuIcon(entry, menuIcon);
            }
        }

        // Order. OrderBy is stable, so unordered entries keep their relative order after the ordered ones.
        return result
            .OrderBy(e => rules.Find(accessor.GetName(e))?.Order is int order ? (0, order) : (1, 0))
            .ToList();
    }
}
