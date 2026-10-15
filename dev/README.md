# Dev instance

Throwaway Jellyfin servers for testing Menu Curator. Never test on a real server first.

## First run

1. `cp dev/.env.example dev/.env` and adjust the port if 8097 is taken.
2. `docker compose -f dev/compose.yaml --profile official up -d` (or `--profile lsio`).
3. Open `http://localhost:<port>`, complete the setup wizard, and create an admin user.
4. Dashboard > Plugins: show **Available** plugins, install **Reports** and **Playback Reporting**, then restart.
   * Reports has one menu page (`reports`), a good one to hide from Dashboard > Menu Curator.
   * Playback Reporting has one menu page (`user_report`), several HTML pages that are not in the menu, and JavaScript resources.
5. Dashboard > API Keys: create a key and put it in `dev/.env` as `JELLYFIN_API_KEY`.

## Install a build

```
dotnet build -c Release
dev/install.sh official   # or lsio
```

The script copies the DLL into a `MenuCurator_<version>` folder in the instance's plugins directory (`/config/plugins` for the official image, `/config/data/plugins` for linuxserver.io) and restarts the container.

## Verify

```
dev/verify.sh
```

Then check the dashboard sidebar in a browser.

## Reset

Stop the container and delete `dev/config-*` and `dev/cache-*`.
