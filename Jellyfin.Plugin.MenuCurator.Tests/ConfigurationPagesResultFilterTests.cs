using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.MenuCurator.Filters;
using Jellyfin.Plugin.MenuCurator.Rules;
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
    private static readonly MenuRules HideReports = new(["reports"]);

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
            () => throw new InvalidOperationException("Simulated rule failure"));
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

    private static Task RunFilter(ResultExecutingContext context, MenuRules rules)
    {
        var filter = new ConfigurationPagesResultFilter(NullLogger<ConfigurationPagesResultFilter>.Instance, () => rules);
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
            new() { Name = "alpha" },
            new() { Name = "reports" },
            new() { Name = "beta" },
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

        public Guid? PluginId { get; set; }
    }
}
