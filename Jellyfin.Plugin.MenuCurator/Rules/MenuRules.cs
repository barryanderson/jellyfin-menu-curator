using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.MenuCurator.Configuration;

namespace Jellyfin.Plugin.MenuCurator.Rules;

/// <summary>
/// The rules applied to the dashboard menu, keyed by page name.
/// </summary>
public sealed partial class MenuRules
{
    // Page names are matched case-insensitively, as the server does when serving a page.
    private readonly Dictionary<string, MenuEntryRule> _rules = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuRules"/> class.
    /// </summary>
    /// <param name="rules">The saved rules. Blank names are ignored; for a repeated name the first rule wins.</param>
    public MenuRules(IEnumerable<MenuEntryRule?> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        foreach (var rule in rules)
        {
            if (rule is not null && !string.IsNullOrWhiteSpace(rule.Name))
            {
                _rules.TryAdd(rule.Name.Trim(), rule);
            }
        }
    }

    /// <summary>
    /// Gets rules that leave the menu unchanged.
    /// </summary>
    public static MenuRules None { get; } = new([]);

    /// <summary>
    /// Gets a value indicating whether there are no rules.
    /// </summary>
    public bool IsEmpty => _rules.Count == 0;

    /// <summary>
    /// Gets the names of pages the rules show, which may include pages their plugin leaves out of the menu.
    /// </summary>
    public IEnumerable<string> ShownNames => _rules.Where(r => r.Value.Visible).Select(r => r.Key);

    /// <summary>
    /// Builds the rules from the saved configuration.
    /// </summary>
    /// <param name="configuration">The configuration, or <c>null</c> if the plugin is not initialised.</param>
    /// <returns>The rules to apply.</returns>
    public static MenuRules FromConfiguration(PluginConfiguration? configuration)
        => configuration?.Entries is { Length: > 0 } entries ? new MenuRules(entries) : None;

    /// <summary>
    /// Gets the rule for a page, if there is one.
    /// </summary>
    /// <param name="name">The page name.</param>
    /// <returns>The rule, or <c>null</c>.</returns>
    public MenuEntryRule? Find(string? name)
        => name is not null && _rules.TryGetValue(name.Trim(), out var rule) ? rule : null;

    /// <summary>
    /// Gets a label override, ignoring blank values.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <returns>The trimmed label, or <c>null</c>.</returns>
    internal static string? DisplayNameOf(MenuEntryRule rule)
        => string.IsNullOrWhiteSpace(rule.DisplayName) ? null : rule.DisplayName.Trim();

    /// <summary>
    /// Gets an icon override, ignoring anything that is not a Material icon name.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <returns>The icon name, or <c>null</c>.</returns>
    internal static string? MenuIconOf(MenuEntryRule rule)
    {
        var icon = rule.MenuIcon?.Trim();
        return icon is not null && IconName().IsMatch(icon) ? icon : null;
    }

    [GeneratedRegex("^[a-z0-9_]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex IconName();
}
