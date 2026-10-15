# Menu Curator user guide

Menu Curator controls the **Plugins** section of the Jellyfin dashboard menu: which plugin pages appear there, what they are called, which icons they show and in what order.

## Opening the settings page

After installation, **Menu Curator** appears in the dashboard menu with a slider icon. You can also reach it from **Dashboard > Plugins > Menu Curator > Settings**, which always works, even if you hide Menu Curator's own menu entry.

## The settings page

### In the menu

This section lists the pages currently in the menu, in the order the menu shows them. Each row has:

- **The tick box:** untick it to hide the page from the menu. Hidden pages move to **Other pages**, and their settings stay available from **Dashboard > Plugins**.
- **Move up and Move down:** change the page's position in the menu.
- **Menu name:** the label shown in the menu. Leave it blank to keep the plugin's own name.
- **Icon:** the icon shown next to the label. The preview beside the list shows the current choice.
- **The description:** the plugin the page belongs to and the page's internal name.

### Choosing an icon

The first entry in the icon list is the default:

- **The plugin's own icon**, when the plugin declares one for this page or for another of its pages. For example, Moonbase declares a dark mode icon, so its first entry is "Dark mode (plugin's icon)".
- **Folder (default)**, when the plugin declares no icon. This is the icon the dashboard shows for such plugins.

Below that are suggested icons grouped by theme (Media, Playback, Discovery, Tools and settings, Data and reports, People and security, Network and system, Look and feel, Other).

To use an icon that is not listed, choose **Custom icon name...** and type the icon's name, for example `rocket_launch`. Names are in lower case with underscores; browse them at [Google Fonts icons](https://fonts.google.com/icons). A name that is not a valid icon name is ignored, and the menu keeps the default icon.

The dashboard can only show icons from the Material icon font, so plugin logos cannot be used as menu icons.

### Other pages

Pages that are not in the menu: pages their plugins leave out of the menu, and pages you have hidden. Tick a page to add it to the bottom of the menu, then move it, rename it or change its icon above.

Some plugins register several pages. Each can be added separately; the menu shows them as separate entries.

### Rules for pages that are not loaded

Shown only when needed. Lists saved rules for pages that are not currently available, usually because their plugin is disabled, uninstalled or has renamed the page. These rules have no effect while the page is missing, and apply again if it returns. Untick a rule and save to delete it.

### Plugins with no settings page

Lists loaded plugins that register no settings page. These cannot appear in the menu, because there is no page to open.

## Saving

Click **Save**. Menu Curator saves your rules and reloads the dashboard, so the menu updates straight away.

The dashboard normally keeps a copy of the menu for up to a minute. Saving clears that copy before reloading. If a future Jellyfin release stores it differently, saving still works, but the menu may take up to a minute and a reload to update.

## Who sees the changes

The menu is the same for every administrator. Changes apply to every browser and device that opens the dashboard, after its next reload.

## Plugin updates and removals

- **A plugin adds a new menu page:** it appears at the bottom of the menu, below your ordered pages, until you place it.
- **A plugin is updated:** your rules still apply, as long as the plugin keeps its page names.
- **A plugin is removed or disabled:** its rules move to **Rules for pages that are not loaded**.

## Resetting the menu

To start again, select **Reset menu** at the bottom of the settings page and confirm. This deletes all your Menu Curator settings: hidden and added pages, names, icons, order, and rules for pages that are not loaded. The dashboard reloads with the menu the plugins themselves ask for.

If the settings page cannot be opened, open **Dashboard > Scheduled Tasks** and run **Reset dashboard menu**, in the **Menu Curator** category. It does the same, and has no schedule, so it only runs when you start it. Reload the dashboard afterwards; the menu can take up to a minute to update.

## Removing Menu Curator

Uninstall it from **Dashboard > Plugins**. The dashboard menu returns to normal straight away, and the plugin is removed completely when Jellyfin restarts. Disabling the plugin also returns the menu to normal; reload the dashboard to see it, as the dashboard can keep the old menu for up to a minute. See the [README](../README.md#uninstalling-and-rolling-back) for manual removal.
