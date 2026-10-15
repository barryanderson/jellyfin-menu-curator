using System.Net.Mime;
using Jellyfin.Plugin.MenuCurator.Pages;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.MenuCurator.Api;

/// <summary>
/// Data for the Menu Curator config page.
/// </summary>
[ApiController]
[Route("MenuCurator")]
[Authorize(Policy = Policies.RequiresElevation)]
[Produces(MediaTypeNames.Application.Json)]
public class MenuCuratorController : ControllerBase
{
    private readonly PageCatalogue _catalogue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCuratorController"/> class.
    /// </summary>
    /// <param name="catalogue">The page catalogue.</param>
    public MenuCuratorController(PageCatalogue catalogue)
    {
        _catalogue = catalogue;
    }

    /// <summary>
    /// Gets every HTML configuration page of every loaded plugin, and the plugins that have none.
    /// </summary>
    /// <response code="200">Page listing returned.</response>
    /// <returns>A <see cref="PageListing"/> of pages and plugins without pages.</returns>
    [HttpGet("Pages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<PageListing> GetPages() => _catalogue.GetListing();
}
