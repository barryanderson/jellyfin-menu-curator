using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.MenuCurator.Configuration;
using Jellyfin.Plugin.MenuCurator.Filters;
using Jellyfin.Plugin.MenuCurator.Pages;
using Jellyfin.Plugin.MenuCurator.Rules;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jellyfin.Plugin.MenuCurator.Tests;

public class ConfigurationPagesResultFilterTests
{
    private static readonly Guid ReportsId = Guid.NewGuid();
    private static readonly Guid NeptuneId = Guid.NewGuid();
    private static readonly MenuRules HideReports = new([new MenuEntryRule { Name = "reports", Visible = false }]);

    private static readonly MenuPage[] AllPages =
    [
        new() { Name = "NeptuneIndexers", DisplayName = "Neptune Indexers", PluginId = NeptuneId, PluginName = "Neptune Indexers", EnableInMainMenu = false, MenuIcon = "search" },
        new() { Name = "reports_custom", DisplayName = "Custom reports", PluginId = ReportsId, PluginName = "Reports", EnableInMainMenu = false },
    ];

    [Fact]
    public async Task SidebarRequest_HidesEntries_AndKeepsListType()
    {
        var (context, original) = CreateContext("?enableInMainMenu=true");

        await RunFilter(context, HideReports);

        var value = Assert.IsType<List<FakePageInfo>>(((ObjectResult)context.Result).Value);
        Assert.NotSame(original, value);
        Assert.Equal(["alpha", "beta"], value.Select(p => p.Name));
    }

    [Theory]
    [InlineData("?enableInMainMenu=True")]
    [InlineData("?EnableInMainMenu=true")]
    public async Task SidebarRequest_ParameterCaseVariants_AreRewritten(string query)
    {
        var (context, _) = CreateContext(query);

        await RunFilter(context, HideReports);

        Assert.Equal(2, ((List<FakePageInfo>)((ObjectResult)context.Result).Value!).Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?enableInMainMenu=false")]
    [InlineData("?enableInMainMenu=nonsense")]
    public async Task OtherRequestsToTheEndpoint_AreUntouched(string query)
    {
        var (context, original) = CreateContext(query);

        await RunFilter(context, HideReports);

        Assert.Same(original, ((ObjectResult)context.Result).Value);
        Assert.Equal(3, original.Count);
    }

    [Fact]
    public async Task OtherActions_AreUntouched()
    {
        var (context, original) = CreateContext("?enableInMainMenu=true", actionName: "GetDashboardConfigurationPage");

        await RunFilter(context, HideReports);

        Assert.Same(original, ((ObjectResult)context.Result).Value);
    }

    [Fact]
    public async Task NonSuccessStatus_IsUntouched()
    {
        var (context, original) = CreateContext("?enableInMainMenu=true");
        ((ObjectResult)context.Result).StatusCode = StatusCodes.Status404NotFound;

        await RunFilter(context, HideReports);

        Assert.Same(original, ((ObjectResult)context.Result).Value);
    }

    [Fact]
    public async Task RuleFailure_FailsOpen_AndStillRunsTheResult()
    {
        var (context, original) = CreateContext("?enableInMainMenu=true");
        var nextCalled = false;

        var filter = new ConfigurationPagesResultFilter(
            NullLogger<ConfigurationPagesResultFilter>.Instance,
            () => throw new InvalidOperationException("Simulated rule failure"),
            () => AllPages);
        await filter.OnResultExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ResultExecutedContext>(null!);
        });

        Assert.True(nextCalled);
        Assert.Same(original, ((ObjectResult)context.Result).Value);
        Assert.Equal(3, original.Count);
    }

    [Fact]
    public async Task EntriesWithoutNameProperty_FailOpen()
    {
        var (context, _) = CreateContext("?enableInMainMenu=true");
        var unexpected = new List<object> { 42 };
        ((ObjectResult)context.Result).Value = unexpected;

        await RunFilter(context, HideReports);

        Assert.Same(unexpected, ((ObjectResult)context.Result).Value);
    }

    [Theory]
    [InlineData(PluginStatus.Active, true, true)]
    [InlineData(PluginStatus.Restart, true, true)]
    [InlineData(PluginStatus.Disabled, true, false)]
    [InlineData(PluginStatus.Deleted, true, false)]
    [InlineData(PluginStatus.Malfunctioned, true, false)]
    [InlineData(PluginStatus.Active, false, false)]
    public void IsEnabled_FollowsJellyfinsPluginState(PluginStatus status, bool supported, bool expected)
    {
        var plugin = new LocalPlugin("/plugins/MenuCurator_1.0.0.0", supported, new PluginManifest { Status = status });

        Assert.Equal(expected, ConfigurationPagesResultFilter.IsEnabled(plugin));
    }

    [Fact]
    public void IsEnabled_UninstalledPlugin_IsNotEnabled()
    {
        // Uninstalling removes the plugin's record, though its code stays loaded until a restart.
        Assert.False(ConfigurationPagesResultFilter.IsEnabled(null));
    }

    [Fact]
    public async Task NoRules_LeavesTheServerListInPlace()
    {
        var (context, original) = CreateContext("?enableInMainMenu=true");

        await RunFilter(context, MenuRules.None);

        Assert.Same(original, ((ObjectResult)context.Result).Value);
    }

    [Fact]
    public async Task PinnedPage_IsAddedAsAServerEntry_WithOverridesAndOrder()
    {
        var (context, _) = CreateContext("?enableInMainMenu=true");
        var rules = new MenuRules(
        [
            new MenuEntryRule { Name = "NeptuneIndexers", Visible = true, Order = 0 },
            new MenuEntryRule { Name = "beta", DisplayName = "Second", MenuIcon = "looks_two", Order = 1 },
        ]);

        await RunFilter(context, rules);

        var value = Assert.IsType<List<FakePageInfo>>(((ObjectResult)context.Result).Value);
        Assert.Equal(["NeptuneIndexers", "beta", "alpha", "reports"], value.Select(p => p.Name));

        var pinned = value[0];
        Assert.Equal("Neptune Indexers", pinned.DisplayName);
        Assert.Equal("search", pinned.MenuIcon);
        Assert.Equal(NeptuneId, pinned.PluginId);
        Assert.True(pinned.EnableInMainMenu);
        Assert.Null(pinned.MenuSection);

        Assert.Equal("Second", value[1].DisplayName);
        Assert.Equal("looks_two", value[1].MenuIcon);
    }

    [Fact]
    public async Task SecondEntryFromOnePlugin_GetsItsOwnStableId()
    {
        var rules = new MenuRules([new MenuEntryRule { Name = "reports_custom", Visible = true }]);

        var (first, _) = CreateContext("?enableInMainMenu=true");
        await RunFilter(first, rules);
        var (second, _) = CreateContext("?enableInMainMenu=true");
        await RunFilter(second, rules);

        var value = (List<FakePageInfo>)((ObjectResult)first.Result).Value!;
        Assert.Equal(value.Count, value.Select(p => p.PluginId).Distinct().Count());
        Assert.Equal(ReportsId, value.Single(p => p.Name == "reports").PluginId);

        var derived = value.Single(p => p.Name == "reports_custom").PluginId;
        Assert.Equal(derived, ((List<FakePageInfo>)((ObjectResult)second.Result).Value!).Single(p => p.Name == "reports_custom").PluginId);
    }

    private static Task RunFilter(ResultExecutingContext context, MenuRules rules)
    {
        var filter = new ConfigurationPagesResultFilter(NullLogger<ConfigurationPagesResultFilter>.Instance, () => rules, () => AllPages);
        return filter.OnResultExecutionAsync(context, () => Task.FromResult<ResultExecutedContext>(null!));
    }

    private static (ResultExecutingContext Context, List<FakePageInfo> Original) CreateContext(
        string queryString,
        string actionName = "GetConfigurationPages")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/web/ConfigurationPages";
        httpContext.Request.QueryString = new QueryString(queryString.Length == 0 ? null : queryString);

        var action = new ControllerActionDescriptor { ControllerName = "Dashboard", ActionName = actionName };
        var actionContext = new ActionContext(httpContext, new RouteData(), action);

        var original = new List<FakePageInfo>
        {
            new() { Name = "alpha", DisplayName = "Alpha", PluginId = Guid.NewGuid() },
            new() { Name = "reports", DisplayName = "Reports", PluginId = ReportsId },
            new() { Name = "beta", DisplayName = "Beta", PluginId = Guid.NewGuid() },
        };
        var result = new ObjectResult(original) { DeclaredType = typeof(IEnumerable<FakePageInfo>) };

        return (new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, controller: new object()), original);
    }

    // Stands in for the server's Jellyfin.Api.Models.ConfigurationPageInfo, which is not in a published package.
    private sealed class FakePageInfo
    {
        public string Name { get; set; } = string.Empty;

        public bool EnableInMainMenu { get; set; } = true;

        public string? DisplayName { get; set; }

        public string? MenuSection { get; set; }

        public string? MenuIcon { get; set; }

        public Guid? PluginId { get; set; }
    }
}
