<p align="center">
  <img src="assets/banner.png" alt="Menu Curator for Jellyfin" width="720">
</p>

# Menu Curator

A Jellyfin server plugin that lets administrators curate the **Plugins** section of the dashboard menu.

- **Hide** plugin pages you do not use.
- **Add** pages that their plugins leave out of the menu, such as a plugin's settings page.
- **Rename** entries, **change their icons** and **reorder** them.

Changes apply to the dashboard for every administrator, and the menu updates as soon as you save.

## Requirements

- Jellyfin **12.2 or later**.
- An administrator account.

Any Jellyfin installation works: the official Docker image, the linuxserver.io image, native Linux packages and Windows.

## Installation

### From the plugin repository (recommended)

1. In Jellyfin, open **Dashboard > Plugins** and select **Manage Repositories**. Add a repository:
   - **Name:** `Menu Curator`
   - **URL:** `https://raw.githubusercontent.com/OWNER/jellyfin-plugin-menu-curator/main/manifest.json`
2. Back on **Dashboard > Plugins**, show **Available** plugins (or search for "Menu Curator"), open **Menu Curator** and select **Install**.
3. Restart Jellyfin when prompted.

Updates then appear on the Plugins page like any other plugin.

### Manual installation

1. Download `menu-curator_<version>.zip` from the [releases page](https://github.com/OWNER/jellyfin-plugin-menu-curator/releases).
2. Find Jellyfin's data folder: on the dashboard home page, the **Paths** card lists it as **Program Data**. The server log also prints it at startup.
3. Create a folder called `MenuCurator_<version>` inside the `plugins` folder there, and extract the zip into it.
4. Make sure the Jellyfin user can read the new folder, then restart Jellyfin.

Common plugin folders, as examples only:

| Installation | Plugins folder |
|---|---|
| Official Docker image | `/config/plugins` |
| linuxserver.io Docker image | `/config/data/plugins` |
| Debian and Ubuntu packages | `/var/lib/jellyfin/plugins` |
| Windows | `%ProgramData%\Jellyfin\Server\plugins` |

For Docker, these paths are inside the container; copy the files to wherever your `/config` volume lives on the host.

The plugin's image only appears on the Plugins page when it is installed from the repository.

## Using Menu Curator

Open **Menu Curator** from the dashboard menu (or from **Dashboard > Plugins**). The page lists every plugin page that can appear in the menu:

- **In the menu:** the pages currently shown, in menu order. Untick one to hide it, set a **Menu name** or **Icon**, and use the arrows to reorder.
- **Other pages:** pages their plugins leave out of the menu. Tick one to add it.

Click **Save**. The dashboard reloads with the new menu.

The [user guide](docs/user-guide.md) covers every option in detail.

## How it works

The dashboard builds its Plugins menu from the server's list of plugin configuration pages. Menu Curator adjusts that list on the server, just before it is sent to the dashboard, using the rules you save. It does not inject any script into the web client and does not depend on other plugins.

Only the menu's own request is changed. The full page list that the Plugins pages use for their **Settings** links is left exactly as the server built it.

If anything unexpected happens while adjusting the menu, Menu Curator logs a warning and serves the normal menu. A problem with this plugin should never stop the dashboard from working.

## Limitations

- **Icons are Material icons.** The dashboard menu draws icons from the Material icon font by name, so plugin logos cannot be used as menu icons. The icon list offers a plugin's own icon first when it declares one.
- **Pages, not plugins.** Only plugins that register a settings page can appear in the menu. The settings page lists plugins that have none.
- **One menu for everyone.** The menu is the same for every administrator.

## Uninstalling and rolling back

- **Uninstall:** open **Dashboard > Plugins**, select **Menu Curator** and uninstall it, then restart Jellyfin. The dashboard menu returns to normal.
- **Manual removal:** stop Jellyfin, delete the `MenuCurator_<version>` folder from the plugins folder, and start Jellyfin again.
- **Roll back a version:** install the previous release's zip manually in place of the current one.

Your saved menu rules stay in `plugins/configurations/Jellyfin.Plugin.MenuCurator.xml` until you delete that file.

## Troubleshooting

- **The menu has not changed after saving.** Reload the dashboard. If it still shows the old menu, wait a minute and reload again: the dashboard caches the menu for up to a minute in some browsers.
- **I hid Menu Curator and cannot find it.** Open **Dashboard > Plugins**, select **Menu Curator**, then **Settings**.
- **Something looks wrong.** Check the server log for lines containing `MenuCurator`, and the browser console for lines starting with `[MenuCurator]`. Please include both when [reporting an issue](https://github.com/OWNER/jellyfin-plugin-menu-curator/issues).

## Building from source

Requires the .NET 10 SDK.

```bash
dotnet build -c Release
dotnet test
```

The plugin is built to `Jellyfin.Plugin.MenuCurator/bin/Release/net10.0/Jellyfin.Plugin.MenuCurator.dll`. A throwaway Jellyfin instance for testing is described in [dev/README.md](dev/README.md), and the release process in [docs/releasing.md](docs/releasing.md).

## Licence

[GPL-3.0](LICENSE), the same as the Jellyfin plugin template this project started from.
