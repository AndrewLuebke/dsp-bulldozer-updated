# Revised implementation plan: Bulldozer UI compatibility

## Objective and scope

Repair Bulldozer's Environment Modification UI for Dyson Sphere Program (DSP)
`0.10.34.28529`, make UI initialization fail-safe and retryable, and produce a new
locally testable Thunderstore package. Preserve the already completed non-UI API,
dependency, build, and packaging work unless a new compiler or runtime failure proves
that work incorrect.

This plan is for an implementation agent. It was written after inspecting the current
dirty worktree, the installed game assembly, the actual Thunderstore profile and log,
the maintained `magnucha/dsp-bulldozer` fork, and the two relevant Thunderstore pages.
Do not replace the repository with a fork and do not run destructive Git commands.

## Current repository and environment

- Repository: `C:\Users\samue\Downloads\Modding\DSP Bulldozer 2`
- Target framework: `net48`
- Current local version: `1.1.11`
- Current game: DSP `0.10.34.28529`, Steam build `23109513`
- Installed assembly inspected:
  `C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll`
- Inspected assembly SHA-256:
  `AE0BA95F75BD879A62AA4CE253B2AB78EAA4FB3C7C595F5E1FEE75EBE0E0EF85`
- Build dependency: `DysonSphereProgram.GameLibs 0.10.34.28529-r.0`
- Runtime profile: Thunderstore `Default`, BepInEx `5.4.17.0`, Unity
  `2022.3.62.1451004`
- Live log:
  `%APPDATA%\Thunderstore Mod Manager\DataFolder\DysonSphereProgram\profiles\Default\BepInEx\LogOutput.log`
- No automated-test project exists. The prior Debug and Release builds completed with
  zero warnings and zero errors, and `scripts/package.ps1` produced a valid package.
- The worktree already contains the user's 1.1.11 compatibility changes and `plan.md`
  is untracked. Start by recording `git status --short`; preserve every existing change.
  Never use `git reset`, `git clean`, `git checkout --`, or bulk-copy another fork.

## Evidence and diagnosis

### Verified runtime evidence

1. The live BepInEx log loads `Bulldozer 1.1.11` successfully at line 86. There is no
   loader, dependency, or Harmony target failure for Bulldozer.
2. At line 228, Bulldozer logs:
   `Unable to initialize Bulldozer UI: the Environment Modification button template was not found.`
3. Immediately afterward, ErrorAnalyzer attributes the exception to
   `BulldozerPlugin.UIBuildMenu_OnCategoryButtonClick_Postfix`.
4. In the current source, `InitUi` returns with `_ui == null` when either the container
   or selected child button is absent. The postfix then unconditionally executes
   `instance._ui.Show(inittedThisTime)` at line 372. This is the direct cause of the
   reported `NullReferenceException`.
5. The active profile contains many other mods (26 BepInEx plugins were loaded), but
   this failure is deterministic in Bulldozer's own control flow. Other mods remain a
   separate integration-test risk.

### Verified current-game assembly evidence

Inspection of the installed `Assembly-CSharp.dll` with Mono.Cecil established:

1. `UIBuildMenu.childGroup` is a `UnityEngine.GameObject`.
2. `UIBuildMenu.childButtons` is a `UIButton[]`.
3. `UIBuildMenu.SetCurrentCategory(int)` treats categories `1` through `9` as child
   build categories, activates `childGroup`, loops over `childButtons`, and deliberately
   permits null array entries.
4. `UIBuildMenu.OnChildButtonClick(int)` contains explicit special handling for
   `currentCategory == 9 && childIndex == 1`. That is DSP's foundation/reform child
   slot. Consequently, the current `childButtons[0]` lookup is the wrong slot;
   `childButtons[1]` is the correct primary template for DSP `0.10.34.28529`.
5. `UIBuildMenu.OnCategoryButtonClick(int)` synchronously calls
   `SetCurrentCategory(int)` before returning. A Harmony postfix on
   `OnCategoryButtonClick` therefore runs after DSP has changed and populated the
   visible category. The observed failure is an index-selection bug, not proof of a
   timing bug.

### Corroborating external evidence

- The maintained fork's current `InitUi` locates
  `child-group/button-1`, consistent with `childButtons[1]`. Use this only as
  corroboration; its absolute `GameObject.Find` paths and unguarded dereferences are
  not robust enough to copy wholesale:
  [magnucha source](https://github.com/magnucha/dsp-bulldozer/blob/master/BulldozerPlugin.cs).
- The maintained fork's latest Thunderstore release is `1.1.10` from October 2025 and
  depends on BepInEx 5.4.17:
  [magnucha package](https://thunderstore.io/c/dyson-sphere-program/p/magnucha/Bulldozer/).
- The original JClark package remains `1.1.9` from December 2023:
  [JClark package](https://thunderstore.io/c/dyson-sphere-program/p/JClark/Bulldozer/).
- Harmony postfixes execute after the original method, matching the installed IL
  ordering described above:
  [Harmony postfix documentation](https://harmony.pardeike.net/articles/patching-postfix.html).
- The pinned GameLibs package is the build-time reference for the exact target game
  revision:
  [GameLibs 0.10.34.28529-r.0](https://www.nuget.org/packages/DysonSphereProgram.GameLibs/0.10.34.28529-r.0).

### Hypotheses and remaining unknowns

1. `childButtons[0]` is almost certainly the null value that triggered the generic
   warning. The installed IL proves index 1 is the correct semantic slot, but the
   current warning does not log the individual values. The revised diagnostics must
   record the array length, index-1 availability, fallback availability, and container
   component state if resolution ever fails again.
2. The build-menu objects may be recreated by a future game update or UI-overhauling
   mod. The implementation must detect an invalid/destroyed UI instance and retry
   without creating duplicates.
3. The configured profile's other mods may alter UI or player inventory behavior.
   Test first in a minimal cloned profile, then in the current 26-plugin profile.
4. Unity UI cannot be meaningfully exercised by this repository's normal `dotnet`
   build, and there is no existing test harness. Runtime smoke tests are mandatory.

## Files to modify

Modify only these files unless a new, reproducible compiler error proves another file
must change:

| File | Intended change |
| --- | --- |
| `BulldozerPlugin.cs` | Resolve the foundation template from `childButtons[1]`; add a relative `childGroup.transform.Find("button-1")` fallback; make initialization transactional; guard the Harmony postfix; add rate-limited retry and detailed failure logging; assign `_ui` only after successful construction. |
| `PluginUI.cs` | Change UI construction to report success/failure; validate required cloned controls; make partial instances inert and safely unloadable; make owned-object tracking per instance; null-guard update/state/show/hide paths. |
| `Bulldozer.csproj` | Bump package/assembly/file version to `1.1.12` after the fix passes compilation. Keep all current dependency pins and build portability changes. |
| `manifest.json` | Bump `version_number` to `1.1.12`. Keep the current dependency and website metadata. |
| `CHANGELOG.md` | Add a `1.1.12` entry describing the corrected foundation-button index and fail-safe/retry behavior. Do not rewrite historical entries. |

Do not modify `GridExplorer.cs`, `RaptorFastDelete.cs`, planet-painting files,
`NuGet.Config`, `README.md`, `.gitignore`, or `scripts/package.ps1` for this fix unless
a verified new failure requires it. In particular, preserve the completed
`ComputeFlattenTerrainReform`, `SetSandCount(..., ESandSource.Reform)`,
`landPercentDirtyFlag`, component-removal, dependency, and portable packaging changes.

Use `1.1.12` so the known-broken `1.1.11` test artifact is never mistaken for the
corrected binary. If the user explicitly requires retaining `1.1.11` because it was
never distributed, version retention is the only decision that may alter the metadata
steps below.

## Ordered implementation steps

### 1. Preserve the worktree and establish the baseline

1. Run the status and focused diffs listed under “Exact commands.”
2. Treat all existing modifications and untracked files as user work. Do not normalize
   line endings or reformat unrelated code.
3. Confirm the target assembly hash and game build still match the values above. If
   either differs, stop and re-inspect `UIBuildMenu` before applying the index change.

### 2. Introduce explicit current-game UI identifiers

In `BulldozerPlugin.cs`, introduce named constants rather than leaving magic numbers:

```csharp
private const int EnvironmentModificationCategory = 9;
private const int FoundationChildIndex = 1;
private const float UiInitRetrySeconds = 1f;
```

Replace UI-category comparisons in the affected UI lifecycle with the named category
constant. Do not change unrelated game category logic.

### 3. Add a validated UI resolver

Refactor `InitUi` into a boolean `TryInitUi(UIBuildMenu uiBuildMenu)` and add a helper
such as:

```csharp
private static bool TryResolveUiParts(
    UIBuildMenu menu,
    out RectTransform container,
    out GameObject foundationTemplate,
    out string failureReason)
```

The resolver must:

1. Reject a null/destroyed `UIBuildMenu` with a descriptive reason.
2. Resolve `container` from `menu.childGroup.GetComponent<RectTransform>()`.
3. Resolve the primary foundation button only when
   `menu.childButtons != null`, `Length > FoundationChildIndex`, and
   `menu.childButtons[FoundationChildIndex] != null`.
4. If the serialized array slot is unavailable, try the relative, hierarchy-scoped
   fallback `menu.childGroup.transform.Find("button-1")`. Do not use the old absolute
   `GameObject.Find("UI Root/...")` path and do not choose an arbitrary first button.
5. Validate that the selected object has a `RectTransform` and a usable `UIButton`
   with a non-null Unity `Button`. Treat the `count` child as optional because the
   current UI code already supports a null `countText`.
6. On failure, include diagnostic facts in `failureReason`: whether `childGroup`
   exists, whether it has a `RectTransform`, the child-button array length, whether
   index 1 exists, and whether relative `button-1` exists. Do not dump the entire scene.

The `reformAllButton` and `uiBuildMenu` parameters of
`UIElements.AddBulldozeComponents` are currently unused. Remove those parameters and
their call-site arguments rather than resolving an unused control. Preserve the
separate null-safe `LateUpdate` behavior that hides DSP's `reformAllButton` in sandbox
mode.

### 4. Make UI construction transactional

`TryInitUi` must not assign the plugin field `_ui` until the candidate UI is complete:

1. Resolve and validate all source controls before adding a component.
2. Create a local `UIElements candidate` on the child-group container.
3. Call a new boolean method such as
   `candidate.TryAddBulldozeComponents(container, foundationTemplate, action,
   out failureReason)`.
4. If construction fails or throws, call `candidate.Unload()`, destroy the candidate
   component, keep `_ui == null`, record the failure, and schedule a retry. Log the
   complete exception on the first occurrence.
5. Only after successful construction, assign `_ui = candidate`, initialize
   `TechUnlockedState` and `ReadyForAction`, clear the failure/backoff state, and return
   `true`.
6. The action callback must null-check `_ui?.countText` before writing the teardown
   count so a later UI teardown cannot cause another exception.

Add an `IsUsable`/`IsInitialized` property to `UIElements`. Before reusing `_ui`, verify
that its owner, action button, and Unity objects still exist. If not, unload it, clear
the field, and rebuild through the same transactional path.

### 5. Make the category postfix fail-safe

Retain the `OnCategoryButtonClick` postfix because the installed game IL verifies that
it runs after `SetCurrentCategory`. Move its body into an instance handler and wrap the
postfix boundary in `try/catch` so no future UI hierarchy mismatch escapes into DSP's
event loop.

The handler must follow this control flow:

1. If the menu is null or the plugin is unavailable, return.
2. If `currentCategory != EnvironmentModificationCategory`, hide an existing valid UI
   and return.
3. If no usable UI exists, call `TryInitUi`.
4. If initialization returns `false`, return without dereferencing `_ui`.
5. Refresh the tech/readiness state, then call `Show(initializedThisAttempt)` only on a
   validated, non-null UI.
6. On an unexpected exception, log it through the rate-limited UI failure path and
   leave the base game's build menu operational.

Do not set `initializedThisAttempt` merely because initialization was attempted; set it
only after `TryInitUi` succeeds. This avoids the exact current bug and preserves the
sandbox positioning behavior.

### 6. Add bounded automatic retry

Use `LateUpdate`, which already obtains the build menu null-safely, to retry only when:

- the game is running;
- the current category is Environment Modification;
- `_ui` is absent or unusable; and
- `Time.unscaledTime` has reached the next retry time.

Retry at most once per `UiInitRetrySeconds`. On success, call `Show(true)` once. On
failure, schedule the next attempt. Log the first distinct failure at warning level and
subsequent identical failures at debug level or suppress them until the failure reason
changes. Reset the timer when the user leaves and re-enters category 9 so a deliberate
new attempt is immediate.

This retry is a resilience fallback; the category-click postfix remains the primary
initialization point.

### 7. Harden `UIElements` against partial construction

In `PluginUI.cs`:

1. Change the static `gameObjectsToDestroy` collection to an instance-owned collection.
   Track only top-level objects created by this `UIElements` instance (the cloned action
   button, custom checkboxes/config objects, and separately parented hover labels).
   Never track or destroy the game's original foundation button or child group.
2. Make `InitOnOffSprites`, `InitActionButton`, and the checkbox/config creation path
   return success/failure or throw into the transaction boundary. Missing required
   sprites, `RectTransform`, `UIButton`, or underlying `Button` must fail construction
   with a clear reason.
3. Make `CopyButton` return null on an unusable clone and destroy that clone during
   rollback. Its icon and `count` children may be absent without crashing; the actual
   action `UIButton` and its `button` are required.
4. Set a private initialized flag only after every required control and event handler
   has been created. `Update`, `Show`, `Hide`, `TechUnlockedState`, and `ReadyForAction`
   must tolerate an uninitialized or unloaded object and must never dereference a null
   image or button.
5. Make `Unload` idempotent: detach/disable behavior, destroy only owned objects once,
   clear references, and mark the instance uninitialized. This must be safe after a
   half-completed initialization and from `OnDestroy`.
6. Retain the existing UI layout and semantics. Do not redesign the panel or add a new
   UI framework in this compatibility fix.

### 8. Update release metadata after the code builds

1. Set `Version` to `1.1.12`, `AssemblyVersion`/`FileVersion` to `1.1.12.0`, and
   `manifest.json.version_number` to `1.1.12`.
2. Add a changelog entry stating that the release selects DSP's foundation child slot
   1 and adds safe transactional initialization/retry behavior.
3. Keep `xiaoye97-BepInEx-5.4.17`, GameLibs `0.10.34.28529-r.0`, the current website
   URL, and the exact five-file package layout.

### 9. Compile and statically verify

1. Restore, build Debug, and build Release using the exact commands below.
2. Require zero compile errors. Investigate any warning introduced by this change;
   do not suppress it globally.
3. Run `git diff --check` and inspect the focused diff. Confirm there are no absolute
   developer/profile paths in source or build targets.
4. Run the packaging script and verify the archive root contains exactly
   `Bulldozer.dll`, `manifest.json`, `README.md`, `CHANGELOG.md`, and `icon.png`.
5. Verify the DLL version and manifest version both resolve to `1.1.12`.

### 10. Perform targeted in-game tests

Close DSP before replacing any test DLL. Preserve the working `1.1.11` DLL as a backup
outside the managed DLL filename.

#### Minimal cloned profile

Clone the Thunderstore `Default` profile in the UI. Disable every optional mod in the
clone except BepInEx; ErrorAnalyzer may remain enabled. Ensure only one Bulldozer DLL
with plugin GUID `semarware.dysonsphereprogram.bulldozer` is active.

Test in this order on a throwaway save or a backed-up save:

1. Launch modded and confirm `Loading [Bulldozer 1.1.12]` with no Harmony/load errors.
2. Load a normal, non-sandbox game. Open the build menu and select Environment
   Modification. Confirm the Bulldozer action button, checkboxes, and config icon are
   visible and usable.
3. Switch repeatedly between categories 1, 9, dismantle, and upgrade; close/reopen the
   build menu. Confirm there are no duplicate controls, drifting foundation-button
   positions, or exceptions.
4. Repeat in sandbox mode. Confirm DSP's `reformAllButton` is hidden only as intended
   and Bulldozer's controls retain their positions across category changes.
5. Test with the tech requirement both enabled and disabled; verify button interaction
   and tooltip state.
6. Open/close the config UI and toggle guideline, vein, and destroy-machine modes.
7. On a backed-up test planet, run a small functional operation before attempting a
   full-planet action. Confirm the confirmation dialog, progress/count state, and
   completion message work.
8. Save, reload, revisit category 9, and confirm the UI remains valid.

#### Existing configured profile

After the minimal profile passes, test the same DLL in the existing `Default` profile,
which currently loads 26 plugins. Repeat steps 1 through 4, then perform one small
foundation operation and one small factory-removal operation. Pay particular attention
to ErrorAnalyzer, UnlimitedFoundations, CommonAPI, DSPOptimizations, GigaStations, and
other UI-affecting mods. Do not attribute unrelated missing-dependency warnings in the
existing log to Bulldozer.

#### Forced graceful-failure test

Temporarily use a Debug-only resolver override or debugger to make template resolution
return false; do not ship that override. Verify that:

- category 9 remains usable by the base game;
- no `NullReferenceException` escapes;
- one actionable warning is logged rather than one per frame;
- retry occurs at the configured interval; and
- successful resolution later creates exactly one Bulldozer UI instance.

Remove the test override and rebuild before packaging Release.

## Acceptance criteria

1. `childButtons[1]` is the primary template and relative `button-1` is the only
   fallback; `childButtons[0]` and absolute `GameObject.Find("UI Root/...")` are absent.
2. No code path calls `_ui.Show`, `_ui.Hide`, state setters, or `countText` without
   validating the relevant instance/control.
3. A failed or partial UI initialization cannot escape the Harmony postfix, cannot
   leave `_ui` falsely initialized, and cannot leak duplicate controls.
4. Identical initialization failures are rate-limited and contain enough state to
   identify a future hierarchy mismatch.
5. Debug and Release builds succeed; the package script succeeds; versions and archive
   contents are consistent at `1.1.12`.
6. The minimal-profile normal and sandbox UI tests pass with no Bulldozer warning,
   exception, duplicate, or layout drift.
7. The configured 26-plugin profile passes category switching and small-operation
   smoke tests without a Bulldozer-attributed error.
8. Previously completed non-UI compatibility behavior remains intact.
9. The implementation diff contains no destructive Git operation, unrelated rewrite,
   user-specific build target, or modification outside the declared files without
   documented evidence.

## Exact commands

Run from the repository root in PowerShell.

### Safety and baseline

```powershell
git status --short
git diff -- BulldozerPlugin.cs PluginUI.cs Bulldozer.csproj manifest.json CHANGELOG.md

$gameAssembly = 'C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll'
Get-FileHash -LiteralPath $gameAssembly -Algorithm SHA256
Select-String -LiteralPath 'C:\Program Files (x86)\Steam\steamapps\appmanifest_1366540.acf' -Pattern 'buildid|LastUpdated'
```

### Restore and build

```powershell
$dotnetExe = 'C:\Program Files\dotnet\dotnet.exe'
& $dotnetExe restore .\Bulldozer.sln --force-evaluate
& $dotnetExe build .\Bulldozer.sln -c Debug --no-restore
& $dotnetExe build .\Bulldozer.sln -c Release --no-restore
& $dotnetExe list .\Bulldozer.csproj package --include-transitive
git diff --check
```

### Package and inspect

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\package.ps1 -Configuration Release

$manifest = Get-Content -LiteralPath .\manifest.json -Raw | ConvertFrom-Json
$dllVersion = [Reflection.AssemblyName]::GetAssemblyName((Resolve-Path .\bin\Release\net48\Bulldozer.dll)).Version
$manifest.version_number
$dllVersion

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packagePath = (Resolve-Path .\artifacts\Bulldozer-1.1.12.zip).Path
$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $archive.Entries | Select-Object FullName, Length
}
finally {
    $archive.Dispose()
}
```

Expected versions: manifest `1.1.12`, DLL `1.1.12.0`.

### Deploy to the existing test profile only after closing DSP

```powershell
$profileRoot = Join-Path $env:APPDATA 'Thunderstore Mod Manager\DataFolder\DysonSphereProgram\profiles\Default'
$pluginDirectory = Join-Path $profileRoot 'BepInEx\plugins\samox73-Bulldozer'
$installedDll = Join-Path $pluginDirectory 'Bulldozer.dll'
$backupDll = Join-Path $pluginDirectory 'Bulldozer.dll.1.1.11.bak'

if (Test-Path -LiteralPath $installedDll) {
    Copy-Item -LiteralPath $installedDll -Destination $backupDll -Force
}
Copy-Item -LiteralPath .\bin\Release\net48\Bulldozer.dll -Destination $installedDll -Force
```

Do not run that deployment block against a different profile without changing and
verifying `$profileRoot` first.

### Inspect the log after each test launch

```powershell
$logPath = Join-Path $profileRoot 'BepInEx\LogOutput.log'
Select-String -LiteralPath $logPath -Pattern 'Loading \[Bulldozer|Bulldozer\]|Bulldozer\.|NullReferenceException|OnCategoryButtonClick' -Context 2,8
```

The test passes only when Bulldozer `1.1.12` loads and no Bulldozer-attributed warning
or exception appears during the scenarios above.
