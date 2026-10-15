using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    /// Gets the hardcoded Phase 1 rules. Replaced by persisted configuration in Phase 2.
    /// </summary>
    public static MenuRules ProofOfConcept { get; } = new(["reports"]);

    /// <summary>
    /// Gets the page names to remove from the menu.
    /// </summary>
    public IReadOnlySet<string> HiddenNames { get; }
}
