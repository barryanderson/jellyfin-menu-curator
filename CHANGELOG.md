# Changelog

Versions follow Jellyfin's four-part plugin versioning (`major.minor.build.revision`). Each release section is used as the GitHub release notes.

## [Unreleased]

- Reset the menu with **Reset menu** on the settings page, or with the new **Reset dashboard menu** scheduled task.
- The menu now returns to normal as soon as Menu Curator is uninstalled, or at the next dashboard reload after it is disabled, instead of only after Jellyfin restarts.

## [1.0.0.0]

First release. Requires Jellyfin 12.2 or later.

- Show or hide plugin pages in the dashboard menu.
- Add pages that their plugins leave out of the menu.
- Rename menu entries, choose their icons from a list (defaulting to the plugin's own icon) and reorder them.
- The menu updates as soon as settings are saved.
- Rules for pages that are not loaded are kept, flagged on the settings page, and can be deleted.
- If anything goes wrong while adjusting the menu, the normal menu is served and a warning is logged.
- The release zip includes the plugin image, so manual installs show it on the Plugins page.
