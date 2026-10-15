# Releasing

Releases are built by GitHub Actions when a version tag is pushed. The workflow builds and tests the plugin, packages it with [jprm](https://github.com/oddstr13/jellyfin-plugin-repository-manager), attaches the zip to a GitHub release and adds the version to `manifest.json`, which is the plugin repository that Jellyfin servers read.

## One-time setup

1. Create the GitHub repository `jellyfin-plugin-menu-curator` and push `main` to it.
2. Replace `OWNER` in `README.md` with the GitHub account or organisation name (it appears in the repository URL and the links). `build.yaml` needs no change: the release workflow fills in the owner and repository itself.
3. Under **Settings > Actions > General > Workflow permissions**, choose **Read and write permissions**, so the release workflow can create releases and commit `manifest.json`.
4. If `main` is protected, allow GitHub Actions to push to it, or the manifest update step will fail after the release has been created.

## Version numbers

The version is set once, in `build.yaml` (`version`). `Directory.Build.props` reads it, so the DLL, `meta.json` and the version Jellyfin offers all agree. Use four parts, as Jellyfin does: `1.0.0.0`, `1.0.1.0`, `1.1.0.0`.

- **Revision or build** (`1.0.0.1`, `1.0.1.0`): fixes.
- **Minor** (`1.1.0.0`): new features.
- **Major** (`2.0.0.0`): changes that need users to act, such as a new minimum Jellyfin version.

## Making a release

1. Update `build.yaml`:
   - `version`: the new version.
   - `changelog`: a short summary. Jellyfin shows it on the plugin's page.
   - `targetAbi`: the minimum Jellyfin version, if it changed.
2. Add a section for the version at the top of `CHANGELOG.md`, headed `## [<version>]`. It becomes the release notes.
3. Commit to `main` and push.
4. Tag the commit and push the tag:

   ```bash
   git tag v1.0.0.0
   git push origin v1.0.0.0
   ```

5. Watch the **Release** workflow. It stops before publishing if the tag does not match `build.yaml`, the tagged commit is not on `main`, or a test fails.
6. Once it finishes, check that the release has the zip attached and that `manifest.json` on `main` lists the new version.

## After a Jellyfin release

1. Check whether the new Jellyfin version changes the plugin ABI: read the release notes, and whether `Jellyfin.Controller` and `Jellyfin.Model` have a new major or minor version on NuGet.
2. If it does, update the package references in `Jellyfin.Plugin.MenuCurator.csproj` (and the tests project), `targetAbi` in `build.yaml`, and `TargetFramework` and `framework` if the .NET version changed. Rebuild, run the tests, and test in a dev instance against both Docker images (see `dev/README.md`).
3. Check that the dashboard still builds its plugin menu from `/web/ConfigurationPages?enableInMainMenu=true`, as recorded in `docs/findings.md`. If that has changed, the plugin falls back to the normal menu rather than breaking, but needs updating.
4. Release a new version with the new `targetAbi`.

## Before the first public release

Confirm the example plugin folders listed in the README on each installation type. Only the linuxserver.io image (`/config/data/plugins`) has been checked so far.
