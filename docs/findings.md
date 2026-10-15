# Phase 0 findings

Source research for Menu Curator, done before any plugin code exists.

Each conclusion is labelled **Confirmed** (read in the source cited) or **Inferred** (reasoned from the source or from ASP.NET Core behaviour, still to be checked on a running server in Phase 1).

**Target changed to Jellyfin 12.x.** The research below was first done against 10.11.11. The section "Jellyfin 12.2" records what differs in 12.2 and takes precedence. Sections 1 to 9 remain the baseline wherever 12.2 is the same.

## Jellyfin 12.2

| Repository | Ref | Commit |
|---|---|---|
| `jellyfin/jellyfin` | `v12.2` (latest release, 5 October 2026) | `ca0f16eb7c195f720a1493ed469a27c4657db0c6` |
| `jellyfin/jellyfin-web` | `v12.2` | `61b1f890365ad15138e943e694af723c1d47df2a` |

Citations in this section refer to these commits (`server12`, `web12`).

### Unchanged from 10.11

* **The sidebar still uses `GET /web/ConfigurationPages?enableInMainMenu=true`.** `web12:src/apps/dashboard/components/drawer/sections/PluginDrawerSection.tsx#L18` is unchanged. The hook now calls `getPluginApi(api).getConfigurationPages` (`web12:src/apps/dashboard/features/plugins/api/useConfigurationPages.ts#L16`) instead of the dashboard API, but `@jellyfin/sdk` 1.0.0 (the version jellyfin-web 12.2 locks) still requests `/web/ConfigurationPages` (`lib/generated-client/api/plugin-api.js#L134`). This is only a regrouping. **Confirmed.**
* **The server endpoint is unchanged.** `server12:Jellyfin.Api/Controllers/DashboardController.cs` differs from 10.11.11 only by a `[Tags("Plugin")]` OpenAPI attribute (`#L23`). The route (`#L49`), the elevation policy (`#L50`) and the filtering are the same, so the filter's `Dashboard` / `GetConfigurationPages` match still applies. **Confirmed.**
* These files are identical to 10.11.11: `ConfigurationPageInfo.cs`, `PluginPageInfo.cs`, `IPluginServiceRegistrator.cs`, and jellyfin-web's `configurationPage.ts` and `routes.tsx`. **Confirmed.**
* **Plugin service registration is unchanged** (`server12:Emby.Server.Implementations/Plugins/PluginManager.cs#L206`).
* **The middleware pipeline order in `Configure` is unchanged** (`server12:Jellyfin.Server/Startup.cs#L173-191`). The only changes are additions in `ConfigureServices` (request localisation, a device access hosted service).
* **The MVC output formatters are unchanged** (`server12:Jellyfin.Server/Extensions/ApiServiceCollectionExtensions.cs#L125-137`). **Confirmed.**
* **Plugin config pages still run as classic pages.** jellyfin-web 12.2 still sets `window.ApiClient` (`web12:src/lib/jellyfin-apiclient/ServerConnections.js#L95`) and `window.Dashboard` (`web12:src/utils/dashboard.js#L267`), and `ApiClient` still offers `getPluginConfiguration` and `updatePluginConfiguration` (`web12:src/apiclient.d.ts#L184,L292`). So a template-style `configPage.html` still works. **Confirmed (globals present); end to end inferred.**

### Changed in 12.2

* **`MenuIcon` is now rendered** as a Material icon name, defaulting to `folder`: `<Icon>{pageInfo.MenuIcon ?? 'folder'}</Icon>` (`web12:.../PluginDrawerSection.tsx#L53`). Icon overrides are therefore useful on 12.2. **Confirmed.**
* **`MenuSection` is still ignored.** jellyfin-web 12.2 never reads it, and all entries remain in the one Plugins list. **Confirmed.**
* **Entries are still keyed by `PluginId`** (`#L49`), so the duplicate-key caveat from section 7.3 still applies. **Confirmed.**
* **The client now caches API responses for longer.** The query client now sets `staleTime` to 1 minute and `gcTime` to 24 hours (`web12:src/utils/query/queryClient.ts#L10-12,L48-50`). Responses are persisted to IndexedDB through `PersistQueryClientProvider`, cleared only when the web build version changes (`web12:src/RootApp.tsx#L18-22`). After rules change, the sidebar can show the old menu for up to a minute, even after a reload, before it refetches. The config page should say so. **Confirmed (settings); user-visible delay inferred.**
* **The server and the NuGet packages now target .NET 10.** `MediaBrowser.Controller` targets `net10.0`, and the `Jellyfin.Controller` 12.2.0 package lists only `net10.0`. A 10.11 build is not a valid 12.x plugin, even though one loaded on 12.2 in an informal test (the log showed the assembly loading, but nothing showed the filter running). **Confirmed (frameworks).**

### Plugin logo

* External plugins declare their image as **`imagePath` in `meta.json`**: a file inside the plugin folder (`server12:MediaBrowser.Common/Plugins/PluginManifest.cs#L103-108`). `GET /Plugins/{id}/{version}/Image` serves it (`server12:Jellyfin.Api/Controllers/PluginsController.cs#L222-243`). **Confirmed.**
* When a plugin is installed from a repository, the server downloads the package's `imageUrl` into the plugin folder and sets `imagePath` itself (`server12:Emby.Server.Implementations/Plugins/PluginManager.cs#L388-420`). **Confirmed.**
* The new `IHasEmbeddedImage` interface is documented as being for plugins built into the server only. External plugins should use `meta.json` (`server12:MediaBrowser.Controller/Plugins/IHasEmbeddedImage.cs`). It only takes effect where the server creates a placeholder record for a bundled plugin (`PluginManager.cs#L605-619`). **Confirmed.**
* Consequence for packaging: `manifest.json` sets `imageUrl`, which the server downloads into the plugin folder on install. **Inferred.** Decided in Phase 4: the release zip does not ship the image, because jprm writes an `image` key rather than `imagePath` to `meta.json`, so manual installs show no plugin image.

### Inputs for the 12.x build

* `TargetFramework` `net10.0`. `Jellyfin.Controller` and `Jellyfin.Model` **12.2.0** (the latest on nuget.org, matching the server). `build.yaml` `targetAbi: "12.2.0.0"`, `framework: "net10.0"`.
* Dev instances are pinned to `jellyfin/jellyfin:12.2` and `lscr.io/linuxserver/jellyfin:version-12.2ubu2604` (linuxserver.io has no plain `12.2` tag).

## Sources (10.11 baseline)

| Repository | Ref | Commit |
|---|---|---|
| `jellyfin/jellyfin` | `v10.11.11` (latest 10.11 tag) | `1fbd8739292cce610231be93daf43368733edf63` |
| `jellyfin/jellyfin-web` | `v10.11.11` | `35c0793ece3adbd247eab290ae1effab851f3d37` |
| `jellyfin/jellyfin-plugin-template` | `master` | `c93225a0a5a76d3843db05b4b5b77fcfc482fba3` |
| `ZL154/jellyfin-plugin-startrack` | `main` | `3b8c2b967ce244ace5ba382fc0276f189573c789` |
| `jellyfin/jellyfin-plugin-playbackreporting` | `master` | `814e507d06bf2f8186fabc182401333ae24d5394` |
| `jellyfin/jellyfin-plugin-reports` | `master` | `9a0b7776b094b36689252dc3a830b0de57f4d60e` |

Citations take the form `repo:path#Lx`. They refer to the commits above. `web` means jellyfin-web and `server` means jellyfin.

## 1. What populates the dashboard sidebar

**Confirmed: the core assumption holds.** The 10.11 React dashboard builds its plugin menu entries from `GET /web/ConfigurationPages?enableInMainMenu=true`.

* `web:src/apps/dashboard/components/drawer/sections/PluginDrawerSection.tsx#L15-18` calls `useConfigurationPages({ enableInMainMenu: true })`. This component is mounted by `web:src/apps/dashboard/components/drawer/AppDrawer.tsx#L32`.
* `web:src/apps/dashboard/features/plugins/api/useConfigurationPages.ts#L11-34` wraps the SDK's `DashboardApi.getConfigurationPages(params)`. The query key is `[ 'ConfigurationPages', enableInMainMenu ]`.
* The legacy `web:src/scripts/libraryMenu.js` no longer requests `ConfigurationPages` at all (no match in the file), so in 10.11 there is only one sidebar code path.

How the sidebar renders each entry (`PluginDrawerSection.tsx#L47-58`):

* Entries are rendered **in array order**, so reordering in the response reorders the menu. **Confirmed.**
* The label is `DisplayName`. **Confirmed.**
* The link is `/configurationpage?name=<Name>`, from `web:src/utils/dashboard.js#L115-117`. **Confirmed.**
* **`MenuIcon` is ignored.** Every entry gets a hardcoded `<Folder />` icon, with the comment `TODO: Support different icons?` (`#L53-54`). **Confirmed.**
* **`MenuSection` is ignored.** All entries are placed in a single "Plugins" list under the fixed "Plugins" link. **Confirmed.**
* The React `key` is `pageInfo.PluginId` (`#L49`). If two entries from the same plugin are in the menu, they share a key. React logs a duplicate key warning and may reconcile the list incorrectly when it changes. **Confirmed (the key); impact inferred.**

The unfiltered list is used elsewhere:

* `web:src/apps/dashboard/routes/plugins/plugin.tsx#L69` and `web:src/apps/dashboard/features/plugins/api/usePluginDetails.ts#L22` call `useConfigurationPages()` **without** the flag. They pass the result to `findBestConfigurationPage` (`web:src/apps/dashboard/features/plugins/api/configurationPage.ts#L3-19`), which picks each plugin's "Settings" page by `PluginId`, preferring the entry with `EnableInMainMenu = true`. **Confirmed.**
* Consequence: rewriting the unfiltered response (dropping entries or flipping `EnableInMainMenu`) would break or change the Settings link on the Plugins pages. **Inferred.**

Client caching:

* The global `QueryClient` (`web:src/utils/query/queryClient.ts#L3-11`) sets no `staleTime`, and the hook sets none either. React Query's defaults apply: stale immediately, refetch on mount and window focus. After saving rules, a page reload (or refocusing the tab) shows the new menu. **Confirmed (settings); refetch behaviour inferred from React Query defaults.**
* Installing or uninstalling a plugin invalidates the `ConfigurationPages` key (`useInstallPackage.ts#L19`, `useUninstallPlugin.ts#L22`). **Confirmed.**

Page routing: `web:src/apps/dashboard/routes/routes.tsx#L15,L49` maps `configurationpage` to a `ServerContentPage` that loads `/web/ConfigurationPage?name=...`. This serves any registered page, whether or not it is flagged for the menu, so added entries open correctly. **Confirmed (route); end to end inferred.**

## 2. Server endpoint and response shape

`server:Jellyfin.Api/Controllers/DashboardController.cs`

* Route `GET web/ConfigurationPages` (`#L48`), with `[Authorize(Policy = Policies.RequiresElevation)]` (`#L49`), so admins only. **Confirmed.**
* `enableInMainMenu` is a `bool?`. When present, the list is filtered to `p.EnableInMainMenu == value` (`#L57-60`). Absent means every page, and `false` means only pages not flagged. **Confirmed.**
* Pages come from `IPluginManager.Plugins`, keeping those whose `Instance` implements `IHasWebPages`, then `GetPages()` (`#L55`, `#L96-109`). **Confirmed.**
* `GET web/ConfigurationPage?name=` (`#L72-94`) matches `Name` case-insensitively and serves the embedded resource with a MIME type taken from `EmbeddedResourcePath`. It is not elevated. **Confirmed.**

`ConfigurationPageInfo` (`server:Jellyfin.Api/Models/ConfigurationPageInfo.cs#L17-65`):

| Property | Type | Source |
|---|---|---|
| `Name` | `string` | `PluginPageInfo.Name` |
| `EnableInMainMenu` | `bool` | `PluginPageInfo.EnableInMainMenu` |
| `MenuSection` | `string?` | `PluginPageInfo.MenuSection` |
| `MenuIcon` | `string?` | `PluginPageInfo.MenuIcon` |
| `DisplayName` | `string?` | page `DisplayName`, or the plugin's `Name` if blank (`#L23`) |
| `PluginId` | `Guid?` | `IPlugin.Id` |

* This type lives in `Jellyfin.Api`, which is **not** published as a NuGet package. The plugin cannot reference it at compile time. **Confirmed (location); package availability inferred from the template referencing only `Jellyfin.Controller` and `Jellyfin.Model`.**
* `EmbeddedResourcePath` is **not** in the response. It is only visible through `PluginPageInfo` in process. **Confirmed.**
* JSON casing depends on the request: MVC registers a PascalCase and a camelCase profile formatter (`server:Jellyfin.Server/Extensions/ApiServiceCollectionExtensions.cs#L123-129`), chosen by the `Accept` media type, and the default options are PascalCase (`#L140`). jellyfin-web receives PascalCase. A raw JSON rewriter would have to handle both. **Confirmed (formatters); client casing inferred from the SDK model property names.**

## 3. Non-HTML pages

`PluginPageInfo` (`server:MediaBrowser.Model/Plugins/PluginPageInfo.cs`) has no type field. Plugins register scripts and libraries as pages so `ConfigurationPage?name=` can serve them. Real examples:

* Playback Reporting (`Jellyfin.Plugin.PlaybackReporting/Plugin.cs#L46-118`) registers HTML pages `user_playback_report`, `playback_report_settings`, `hourly_usage_report`, `breakdown_report`, `duration_histogram_report`, `user_report` (the only one with `EnableInMainMenu = true`) and `custom_query`. It also registers `user_playback_report.js`, `Chart.bundle.min.js` and similar.
* Reports (`Jellyfin.Plugin.Reports/Plugin.cs#L32-39`) registers `reports` (`.html`, menu) and `reportsjs` (`.js`, no dot before the extension in `Name`).

Conclusion: `Name` is not a reliable signal (see `reportsjs`). The most reliable signal is **`EmbeddedResourcePath` ending in `.html` or `.htm` (case-insensitive)**. That is the same suffix the server's own `MimeTypes.GetMimeType(resourcePath)` uses to decide what it serves (`DashboardController.cs#L93`). The plugin can read this in process through `IHasWebPages.GetPages()`. **Confirmed (examples and server MIME logic); the "most reliable" judgement is inferred.**

Plugins with no pages are those whose `LocalPlugin.Instance` is not `IHasWebPages` (`DashboardController.cs#L103-106`). Disabled or malfunctioned plugins have no instance and so contribute nothing (`MediaBrowser.Common/Plugins/LocalPlugin.cs#L68,L73`). Rules for them become "stale". **Confirmed (instance check); stale behaviour inferred.**

Playback Reporting is a good dev fixture: it has several non-menu HTML pages to add, JS resources that must be filtered out, and one flagged menu page.

## 4. Enumerating pages from inside the plugin

The controller only needs `IPluginManager` (`MediaBrowser.Common.Plugins`, `IReadOnlyList<LocalPlugin> Plugins`, `server:MediaBrowser.Common/Plugins/IPluginManager.cs#L19`), which ships in the `Jellyfin.Common` package that `Jellyfin.Controller` depends on. The plugin can inject `IPluginManager` and repeat the same `Plugins` then `IHasWebPages` then `GetPages()` walk. That gives the full list including `EmbeddedResourcePath`, with no internal HTTP request and no auth token. **Confirmed (API surface); package dependency inferred.**

## 5. Registering middleware from a plugin

* `IPluginServiceRegistrator.RegisterServices(IServiceCollection, IServerApplicationHost)` exists in 10.11 (`server:MediaBrowser.Controller/Plugins/IPluginServiceRegistrator.cs`). **Confirmed.**
* `PluginManager.RegisterServices` (`server:Emby.Server.Implementations/Plugins/PluginManager.cs#L206-236`) creates each registrator for enabled, supported plugins and passes it the **host's own** service collection. A throwing registrator marks the plugin Malfunctioned instead of crashing the server. **Confirmed.**
* That collection is the generic host's (`server:Jellyfin.Server/Program.cs#L170`, `.ConfigureServices(services => appHost.Init(services))`, which calls `_pluginManager.RegisterServices` at `server:Emby.Server.Implementations/ApplicationHost.cs#L462`). So an `IStartupFilter` or `services.Configure<MvcOptions>(...)` registered by a plugin is seen by ASP.NET Core. **Confirmed (wiring); that ASP.NET Core then honours them is inferred from framework behaviour.**
* Reference plugin: StarTrack registers `services.AddSingleton<IStartupFilter, ScriptInjectionStartupFilter>()` (`InternalRatingSystem/WebInjectionStartup.cs#L31`). Its filter calls `app.UseMiddleware<...>()` and then `next(app)` (`InternalRatingSystem/ScriptInjectionStartupFilter.cs#L12-19`). **Confirmed.**

### Where an `IStartupFilter` middleware actually runs

`server:Jellyfin.Server/Startup.cs#L144-225`. An `IStartupFilter` that runs its middleware before `next(app)` sits **in front of the whole `Configure` method**, which means:

1. It runs before `UseBaseUrlRedirection` and outside `app.Map(config.BaseUrl, ...)` (`#L149-153`). The request path still includes any configured base URL (for example `/jellyfin/web/ConfigurationPages`), and `UsePathTrim` (`#L177`) has not run yet.
2. It runs **outside** `UseResponseCompression` (`#L167`). The response body it captures is already gzip or Brotli encoded when the client sends `Accept-Encoding`.
3. It runs outside Jellyfin's `ExceptionMiddleware` (`#L161`). Anything it throws is not caught by Jellyfin.

**Confirmed (pipeline order); the consequences follow directly from how ASP.NET Core middleware works.** All three can be handled (match on a path suffix or read `BaseUrl` from `IServerConfigurationManager`; strip `Accept-Encoding` on the matched request so compression never runs; catch everything). Each of them, however, is a way for a raw middleware to go wrong.

## 6. Recommendation: an MVC result filter instead of raw middleware

The spec's goal is server-side rewriting of this one endpoint's response. A cleaner way to do that than an `IStartupFilter` middleware is an **MVC `IAsyncResultFilter`**, registered from the plugin with `services.Configure<MvcOptions>(o => o.Filters.Add<...>())`.

* It runs inside MVC, after authorisation and the action, but **before output formatting and compression**. So there is no compression handling, no buffering, no base URL or path matching, and no PascalCase/camelCase JSON handling.
* It targets the action precisely: `ControllerActionDescriptor` with `ControllerName == "Dashboard"` and `ActionName == "GetConfigurationPages"`.
* It works on `ObjectResult.Value`, a `List<ConfigurationPageInfo>`. Because that type is not referenceable, the filter edits the existing objects by property name (reflection on the runtime type). Added entries are created with `Activator.CreateInstance` of the same runtime type, so serialisation is unchanged.
* Fail open is simple: on any exception, leave `context.Result` untouched and log a warning.
* It does not depend on Jellyfin's middleware order, which has changed between releases.

**Inferred.** This needs proving in Phase 1, in particular that a filter added through `Configure<MvcOptions>` from a plugin is applied to Jellyfin's controllers. If it is not, fall back to the `IStartupFilter` middleware with the three mitigations in section 5. The spec's rule engine, config model and fail-open behaviour are unaffected either way.

## 7. Changes to the spec this implies

1. **Rewrite only when `enableInMainMenu=true`.** All rules (hide, add, override, order) apply only to the sidebar request. The unfiltered request (and `enableInMainMenu=false`) must pass through untouched, or the Plugins pages' Settings links break (section 1).
2. **`MenuIcon` and `MenuSection` overrides have no visible effect in 10.11.** The sidebar ignores both. Options: drop them from the UI, or keep them hidden in the config model for a future dashboard. This needs a decision before Phase 3.
3. **Adding a second page from a plugin already in the menu** produces duplicate React keys (`PluginId`). The config UI should warn about this. It cannot be fixed server-side without changing `PluginId`, which would also affect `findBestConfigurationPage` on the main-menu list only. Consider in Phase 3.
4. **Mechanism:** an MVC result filter is preferred over `IStartupFilter` middleware (section 6). It still meets "server-side, no JS injection, no File Transformation dependency".

## 8. Open risks

* **Jellyfin 12 exists.** The latest release is `v12.2` (5 October 2026). StarTrack's csproj notes that 12 moved to .NET 10 with its own assembly versions, and that a 10.11 build does not load on 12 (`InternalRatingSystem/InternalRatingSystem.csproj#L6-8`). This project targets 10.11 as specified. Supporting 12 would need a second build target and its own Phase 0 check of the 12 dashboard. **Inferred for our plugin; not investigated.**
* The duplicate-key behaviour (section 7.3) has not been observed yet.
* The filter approach is unproven on a running server (section 6).

## 9. Inputs for Phase 1

* NuGet: `Jellyfin.Controller` and `Jellyfin.Model` **10.11.11** (latest 10.11 on nuget.org, matches the pinned server tag), `ExcludeAssets=runtime`, `PrivateAssets=all`, as in the template (which itself uses 10.11.5).
* `build.yaml`: `targetAbi: "10.11.0.0"`, `framework: "net9.0"` (template values).
* Analysers, from template `Jellyfin.Plugin.Template.csproj`: `Nullable` enable, `TreatWarningsAsErrors`, `AnalysisMode` `AllEnabledByDefault`, StyleCop, SerilogAnalyzer, MultithreadingAnalyzer.
* HTML page rule: `EmbeddedResourcePath` ends with `.html` or `.htm`, ordinal ignore case.
* Compression strategy: not needed with the result filter. With the middleware fallback, remove `Accept-Encoding` from matched requests.
* Dev instance: the image tag should be pinned to `10.11.11` for both `jellyfin/jellyfin` and `lscr.io/linuxserver/jellyfin`. The host is still to be decided, as no container runtime is installed on the development machine.
* Phase 1 verification items that source reading cannot settle:
  * the filter is applied to `GetConfigurationPages`
  * the rewritten JSON is identical in shape to stock, in both PascalCase and camelCase
  * `Accept-Encoding: gzip, br` responses are correct
  * the unfiltered request is unchanged
  * a forced exception in the rule engine serves the stock list
  * the sidebar updates on reload
