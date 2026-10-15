using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MenuCurator.Configuration;
using Jellyfin.Plugin.MenuCurator.Pages;
using Jellyfin.Plugin.MenuCurator.Rules;
using Xunit;

namespace Jellyfin.Plugin.MenuCurator.Tests;

public class MenuRuleEngineTests
{
    private static readonly Guid NeptuneId = Guid.NewGuid();

    // Pages a plugin registers but leaves out of the menu, available to add.
    private static readonly MenuPage[] AllPages =
    [
        new() { Name = "NeptuneIndexers", DisplayName = "Neptune Indexers", PluginId = NeptuneId, PluginName = "Neptune Indexers", EnableInMainMenu = false },
        new() { Name = "JellyTrailers", DisplayName = "JellyTrailers", PluginId = Guid.NewGuid(), PluginName = "JellyTrailers", EnableInMainMenu = false, MenuIcon = "movie" },
    ];

    [Fact]
    public void Apply_NoRules_KeepsEverythingInOrder()
    {
        var result = Apply(MenuRules.None);

        Assert.Equal(["File Transformation", "Intro Skipper", "Moonbase"], Names(result));
    }

    [Fact]
    public void Apply_Hide_DropsMatchingEntries_CaseInsensitively()
    {
        var result = Apply(Rules(new MenuEntryRule { Name = "intro skipper", Visible = false }));

        Assert.Equal(["File Transformation", "Moonbase"], Names(result));
    }

    [Fact]
    public void Apply_Add_AppendsPagesTheirPluginLeavesOut()
    {
        var result = Apply(Rules(new MenuEntryRule { Name = "NeptuneIndexers", Visible = true }));

        Assert.Equal(["File Transformation", "Intro Skipper", "Moonbase", "NeptuneIndexers"], Names(result));
        var added = result[^1];
        Assert.Equal("Neptune Indexers", added.DisplayName);
        Assert.Equal(NeptuneId, added.PluginId);
        Assert.True(added.EnableInMainMenu);
    }

    [Fact]
    public void Apply_Add_DoesNotDuplicatePagesAlreadyInTheMenu()
    {
        var result = Apply(Rules(new MenuEntryRule { Name = "Moonbase", Visible = true }));

        Assert.Single(result, e => e.Name == "Moonbase");
    }

    [Fact]
    public void Apply_NoAdditions_DoesNotReadThePageList()
    {
        var read = false;

        MenuRuleEngine.Apply(Menu(), new TestAccessor(), Rules(new MenuEntryRule { Name = "Moonbase", Visible = false }), () =>
        {
            read = true;
            return AllPages;
        });

        Assert.False(read);
    }

    [Fact]
    public void Apply_StaleRules_AreIgnored()
    {
        var result = Apply(Rules(
            new MenuEntryRule { Name = "removed_plugin_page", Visible = false },
            new MenuEntryRule { Name = "another_removed_page", Visible = true, Order = 0 }));

        Assert.Equal(["File Transformation", "Intro Skipper", "Moonbase"], Names(result));
    }

    [Fact]
    public void Apply_Override_SetsNameAndIcon_IgnoringBlankAndInvalidValues()
    {
        var result = Apply(Rules(
            new MenuEntryRule { Name = "Moonbase", DisplayName = "  Night mode  ", MenuIcon = "bedtime" },
            new MenuEntryRule { Name = "Intro Skipper", DisplayName = " ", MenuIcon = "<b>bad</b>" },
            new MenuEntryRule { Name = "JellyTrailers", Visible = true, DisplayName = "Trailers" }));

        var moonbase = result.Single(e => e.Name == "Moonbase");
        Assert.Equal("Night mode", moonbase.DisplayName);
        Assert.Equal("bedtime", moonbase.MenuIcon);

        var intro = result.Single(e => e.Name == "Intro Skipper");
        Assert.Equal("Intro Skipper", intro.DisplayName);
        Assert.Null(intro.MenuIcon);

        var trailers = result.Single(e => e.Name == "JellyTrailers");
        Assert.Equal("Trailers", trailers.DisplayName);
        Assert.Equal("movie", trailers.MenuIcon);
    }

    [Fact]
    public void Apply_Order_SortsOrderedEntriesFirst_ThenTheRestInOriginalOrder()
    {
        var result = Apply(Rules(
            new MenuEntryRule { Name = "Moonbase", Order = 0 },
            new MenuEntryRule { Name = "NeptuneIndexers", Visible = true, Order = 1 }));

        Assert.Equal(["Moonbase", "NeptuneIndexers", "File Transformation", "Intro Skipper"], Names(result));
    }

    [Fact]
    public void FromConfiguration_NullOrEmpty_IsEmpty()
    {
        Assert.True(MenuRules.FromConfiguration(null).IsEmpty);
        Assert.True(MenuRules.FromConfiguration(new PluginConfiguration()).IsEmpty);
        Assert.True(MenuRules.FromConfiguration(new PluginConfiguration { Entries = null! }).IsEmpty);
    }

    [Fact]
    public void FromConfiguration_SkipsNullAndBlankNames_FirstRuleForANameWins()
    {
        var rules = MenuRules.FromConfiguration(new PluginConfiguration
        {
            Entries =
            [
                null!,
                new MenuEntryRule { Name = " ", Visible = false },
                new MenuEntryRule { Name = "Moonbase", Visible = false },
                new MenuEntryRule { Name = "MOONBASE", Visible = true },
            ]
        });

        Assert.False(rules.Find("moonbase")!.Visible);
        Assert.Empty(rules.ShownNames);
    }

    private static List<TestEntry> Menu() =>
    [
        new() { Name = "File Transformation", DisplayName = "File Transformation" },
        new() { Name = "Intro Skipper", DisplayName = "Intro Skipper" },
        new() { Name = "Moonbase", DisplayName = "Moonbase" },
    ];

    private static IReadOnlyList<TestEntry> Apply(MenuRules rules)
        => MenuRuleEngine.Apply(Menu(), new TestAccessor(), rules, () => AllPages);

    private static MenuRules Rules(params MenuEntryRule[] rules) => new(rules);

    private static IEnumerable<string> Names(IEnumerable<TestEntry> entries) => entries.Select(e => e.Name);

    private sealed class TestEntry
    {
        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? MenuIcon { get; set; }

        public bool EnableInMainMenu { get; set; } = true;

        public Guid? PluginId { get; set; }
    }

    private sealed class TestAccessor : IMenuEntryAccessor<TestEntry>
    {
        public string? GetName(TestEntry entry) => entry.Name;

        public TestEntry Create(MenuPage page) => new()
        {
            Name = page.Name,
            DisplayName = page.DisplayName,
            MenuIcon = page.MenuIcon,
            PluginId = page.PluginId,
            EnableInMainMenu = true
        };

        public void SetDisplayName(TestEntry entry, string displayName) => entry.DisplayName = displayName;

        public void SetMenuIcon(TestEntry entry, string menuIcon) => entry.MenuIcon = menuIcon;
    }
}
