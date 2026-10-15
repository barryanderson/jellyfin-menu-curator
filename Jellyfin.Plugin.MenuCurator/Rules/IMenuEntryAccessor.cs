using Jellyfin.Plugin.MenuCurator.Pages;

namespace Jellyfin.Plugin.MenuCurator.Rules;

/// <summary>
/// Reads, writes and creates menu entries of a type the rule engine does not know.
/// </summary>
/// <typeparam name="T">The entry type.</typeparam>
public interface IMenuEntryAccessor<T>
{
    /// <summary>
    /// Gets an entry's page name.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The page name.</returns>
    string? GetName(T entry);

    /// <summary>
    /// Creates a menu entry for a page its plugin leaves out of the menu.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>The new entry.</returns>
    T Create(MenuPage page);

    /// <summary>
    /// Sets the entry's label.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="displayName">The label.</param>
    void SetDisplayName(T entry, string displayName);

    /// <summary>
    /// Sets the entry's Material icon name.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="menuIcon">The icon name.</param>
    void SetMenuIcon(T entry, string menuIcon);
}
