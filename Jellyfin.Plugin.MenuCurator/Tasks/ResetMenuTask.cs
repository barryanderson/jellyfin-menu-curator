using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MenuCurator.Tasks;

/// <summary>
/// Deletes every menu rule. Run by hand from Dashboard > Scheduled Tasks, for example when the settings page cannot be opened.
/// </summary>
public class ResetMenuTask : IScheduledTask
{
    private readonly ILogger<ResetMenuTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetMenuTask"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{ResetMenuTask}"/> interface.</param>
    public ResetMenuTask(ILogger<ResetMenuTask> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Reset dashboard menu";

    /// <inheritdoc />
    public string Key => "MenuCuratorResetMenu";

    /// <inheritdoc />
    public string Description => "Deletes every Menu Curator rule, returning the dashboard menu to what the plugins themselves ask for.";

    /// <inheritdoc />
    public string Category => "Menu Curator";

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var plugin = Plugin.Instance ?? throw new InvalidOperationException("Menu Curator is not loaded.");
        plugin.ResetMenu();
        _logger.LogInformation("Menu rules deleted; the dashboard menu is back to its default");

        progress.Report(100);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => [];
}
