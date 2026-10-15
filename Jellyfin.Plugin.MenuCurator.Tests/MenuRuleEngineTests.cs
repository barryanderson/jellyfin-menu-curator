using System.Linq;
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
    public void Apply_ProofOfConceptRules_HideReportsAndFileTransformation()
    {
        string?[] pages = ["File Transformation", "Intro Skipper", "reports", "Moonbase"];

        var result = MenuRuleEngine.Apply(pages, p => p, MenuRules.ProofOfConcept);

        Assert.Equal(new string?[] { "Intro Skipper", "Moonbase" }, result.ToList());
    }
}
