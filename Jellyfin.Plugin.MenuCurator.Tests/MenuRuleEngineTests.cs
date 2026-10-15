using System.Linq;
using Jellyfin.Plugin.MenuCurator.Configuration;
using Jellyfin.Plugin.MenuCurator.Rules;
using Xunit;

namespace Jellyfin.Plugin.MenuCurator.Tests;

public class MenuRuleEngineTests
{
    private static readonly string?[] Pages = ["alpha", "reports", "beta", null, "gamma"];

    [Fact]
    public void Apply_NoRules_KeepsEverythingInOrder()
    {
        var result = MenuRuleEngine.Apply(Pages, p => p, new MenuRules([]));

        Assert.Equal(Pages, result);
    }

    [Fact]
    public void Apply_Hide_DropsMatchingEntriesAndKeepsOrder()
    {
        var result = MenuRuleEngine.Apply(Pages, p => p, new MenuRules(["reports", "beta"]));

        Assert.Equal(new string?[] { "alpha", null, "gamma" }, result);
    }

    [Fact]
    public void Apply_Hide_MatchesNamesCaseInsensitively()
    {
        var result = MenuRuleEngine.Apply(Pages, p => p, new MenuRules(["REPORTS"]));

        Assert.DoesNotContain("reports", result);
    }

    [Fact]
    public void Apply_StaleHideEntry_IsIgnored()
    {
        var result = MenuRuleEngine.Apply(Pages, p => p, new MenuRules(["removed_plugin_page"]));

        Assert.Equal(Pages, result);
    }

    [Fact]
    public void FromConfiguration_HidesOnlyEntriesMarkedNotVisible()
    {
        var config = new PluginConfiguration
        {
            Entries =
            [
                new MenuEntryRule { Name = "File Transformation", Visible = false },
                new MenuEntryRule { Name = "Intro Skipper", Visible = true },
                new MenuEntryRule { Name = " ", Visible = false },
                null!,
            ]
        };
        string?[] pages = ["File Transformation", "Intro Skipper", "Moonbase"];

        var result = MenuRuleEngine.Apply(pages, p => p, MenuRules.FromConfiguration(config));

        Assert.Equal(new string?[] { "Intro Skipper", "Moonbase" }, result.ToList());
    }

    [Fact]
    public void FromConfiguration_NullOrEmpty_ChangesNothing()
    {
        Assert.Empty(MenuRules.FromConfiguration(null).HiddenNames);
        Assert.Empty(MenuRules.FromConfiguration(new PluginConfiguration()).HiddenNames);
        Assert.Empty(MenuRules.FromConfiguration(new PluginConfiguration { Entries = null! }).HiddenNames);
    }
}
