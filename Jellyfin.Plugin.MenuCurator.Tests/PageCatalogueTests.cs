using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MenuCurator.Pages;
using MediaBrowser.Model.Plugins;
using Xunit;

namespace Jellyfin.Plugin.MenuCurator.Tests;

public class PageCatalogueTests
{
    private static readonly Guid IntroSkipperId = Guid.NewGuid();
    private static readonly Guid ReportsId = Guid.NewGuid();
    private static readonly Guid TrailersId = Guid.NewGuid();

    [Theory]
    [InlineData("Plugin.Configuration.configPage.html", true)]
    [InlineData("Plugin.Web.Page.HTM", true)]
    [InlineData("Plugin.Web.reports.js", false)]
    [InlineData("Plugin.Web.introskipper.css", false)]
    [InlineData("", false)]
    public void IsHtmlPage_UsesResourceExtension(string resourcePath, bool expected)
    {
        var page = new PluginPageInfo { Name = "page", EmbeddedResourcePath = resourcePath };

        Assert.Equal(expected, PageCatalogue.IsHtmlPage(page));
    }

    [Fact]
    public void Build_KeepsHtmlPagesOnly_AndListsPluginsWithoutPages()
    {
        var listing = PageCatalogue.Build(
        [
            (IntroSkipperId, "Intro Skipper", Pages(
                Page("Intro Skipper", "IntroSkipper.Configuration.configPage.html", menu: true),
                Page("introskipper.js", "IntroSkipper.Configuration.inject.js"),
                Page("introskipper.css", "IntroSkipper.Configuration.inject.css"))),
            // Name alone would not reveal this is a script.
            (ReportsId, "Reports", Pages(
                Page("reports", "Reports.Web.reports.html", menu: true, icon: "assessment"),
                Page("reportsjs", "Reports.Web.reports.js"))),
            (TrailersId, "Trailers", Pages(Page("trailers.js", "Trailers.inject.js"))),
            (Guid.NewGuid(), "AudioDB", Pages()),
        ]);

        Assert.Equal(["Intro Skipper", "reports"], listing.Pages.Select(p => p.Name));
        Assert.Equal(["AudioDB", "Trailers"], listing.PluginsWithoutPages);

        var reports = listing.Pages.Single(p => p.Name == "reports");
        Assert.Equal(ReportsId, reports.PluginId);
        Assert.Equal("Reports", reports.PluginName);
        Assert.True(reports.EnableInMainMenu);
        Assert.Equal("assessment", reports.MenuIcon);
    }

    [Fact]
    public void Build_DisplayName_FallsBackToPluginName()
    {
        var listing = PageCatalogue.Build(
        [
            (ReportsId, "Reports", Pages(
                Page("reports", "Reports.Web.reports.html"),
                Page("custom", "Reports.Web.custom.html", displayName: "Custom report"))),
        ]);

        Assert.Equal(["Reports", "Custom report"], listing.Pages.Select(p => p.DisplayName));
        Assert.All(listing.Pages, p => Assert.False(p.EnableInMainMenu));
    }

    private static List<PluginPageInfo> Pages(params PluginPageInfo[] pages) => [.. pages];

    private static PluginPageInfo Page(string name, string resourcePath, bool menu = false, string? icon = null, string? displayName = null)
        => new()
        {
            Name = name,
            EmbeddedResourcePath = resourcePath,
            EnableInMainMenu = menu,
            MenuIcon = icon,
            DisplayName = displayName
        };
}
