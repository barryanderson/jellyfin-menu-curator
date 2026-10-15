using System;
using System.Reflection;
using Jellyfin.Plugin.MenuCurator.Pages;
using Jellyfin.Plugin.MenuCurator.Rules;

namespace Jellyfin.Plugin.MenuCurator.Filters;

/// <summary>
/// Accesses the server's ConfigurationPageInfo by property name, since the type is not in a published package.
/// </summary>
internal sealed class ReflectionMenuEntryAccessor : IMenuEntryAccessor<object>
{
    private readonly Type _entryType;
    private readonly PropertyInfo _name;
    private readonly PropertyInfo _displayName;
    private readonly PropertyInfo _menuIcon;
    private readonly PropertyInfo _enableInMainMenu;
    private readonly PropertyInfo _pluginId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionMenuEntryAccessor"/> class.
    /// </summary>
    /// <param name="entryType">The runtime type of the server's menu entries.</param>
    /// <exception cref="InvalidOperationException">The type does not have the expected shape.</exception>
    public ReflectionMenuEntryAccessor(Type entryType)
    {
        _entryType = entryType;
        _name = Property("Name", typeof(string));
        _displayName = Property("DisplayName", typeof(string));
        _menuIcon = Property("MenuIcon", typeof(string));
        _enableInMainMenu = Property("EnableInMainMenu", typeof(bool));
        _pluginId = Property("PluginId", typeof(Guid?));

        if (entryType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new InvalidOperationException($"{entryType.FullName} has no parameterless constructor.");
        }
    }

    /// <inheritdoc />
    public string? GetName(object entry) => _name.GetValue(entry) as string;

    /// <inheritdoc />
    public object Create(MenuPage page)
    {
        var entry = Activator.CreateInstance(_entryType)!;
        _name.SetValue(entry, page.Name);
        _displayName.SetValue(entry, page.DisplayName);
        _menuIcon.SetValue(entry, page.MenuIcon);
        _enableInMainMenu.SetValue(entry, true);
        _pluginId.SetValue(entry, (Guid?)page.PluginId);
        return entry;
    }

    /// <inheritdoc />
    public void SetDisplayName(object entry, string displayName) => _displayName.SetValue(entry, displayName);

    /// <inheritdoc />
    public void SetMenuIcon(object entry, string menuIcon) => _menuIcon.SetValue(entry, menuIcon);

    /// <summary>
    /// Gets an entry's plugin id.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The plugin id, or <c>null</c>.</returns>
    public Guid? GetPluginId(object entry) => (Guid?)_pluginId.GetValue(entry);

    /// <summary>
    /// Sets an entry's plugin id.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="pluginId">The plugin id.</param>
    public void SetPluginId(object entry, Guid pluginId) => _pluginId.SetValue(entry, (Guid?)pluginId);

    private PropertyInfo Property(string name, Type type)
    {
        var property = _entryType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property is null || property.PropertyType != type || !property.CanRead || !property.CanWrite)
        {
            throw new InvalidOperationException($"{_entryType.FullName} has no read/write {type.Name} property {name}.");
        }

        return property;
    }
}
