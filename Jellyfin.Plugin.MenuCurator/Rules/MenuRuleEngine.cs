using System;
using System.Collections.Generic;

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
    /// Applies the rules to the menu entries.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    /// <param name="entries">The entries, in the order the server returned them.</param>
    /// <param name="nameOf">Reads an entry's page name.</param>
    /// <param name="rules">The rules to apply.</param>
    /// <returns>The entries to show, in menu order.</returns>
    public static IReadOnlyList<T> Apply<T>(IEnumerable<T> entries, Func<T, string?> nameOf, MenuRules rules)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(nameOf);
        ArgumentNullException.ThrowIfNull(rules);

        var result = new List<T>();
        foreach (var entry in entries)
        {
            var name = nameOf(entry);
            if (name is not null && rules.HiddenNames.Contains(name))
            {
                continue;
            }

            result.Add(entry);
        }

        return result;
    }
}
