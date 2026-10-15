# Changelog

## 1.1.14 - 2026-10-15

- Updated for Dyson Sphere Program 0.10.35: `PlanetAuxData.ReformSnap` gained a reform-mode argument; Bulldozer now passes the normal paving mode.
- Fixed the Bulldozer button overlapping the game's Environment Modification controls in 0.10.35 (terrain restoration, vegetation transplant and vein raise/bury now follow the foundation button). The Bulldozer button and its option checkboxes now sit to the left of the foundation button, and the game's foundation button is no longer moved in sandbox mode. To fit inside the build menu, the Bulldozer button is drawn at 75% size and the separate config icon is gone: right-click the Bulldozer button to open the settings window.
- The foundation button's original position is now captured each session instead of being restored from a value saved in the config by an older game version, which could force the button back into a pre-0.10.35 layout.

## 1.1.13 - 2026-09-08

First release under IlldariFairlight (maintained fork of Samox 1.1.12; fix
submitted upstream as samox73/dsp-bulldozer-updated#1, unanswered as of
2026-09-08).

- Fixed `IndexOutOfRangeException` crash in `ReformIndexInfoProvider` on any planet
  larger than the one the game loaded in on (e.g. Galactic Scale resized worlds):
  the mod-index lookup was sized once at construction from the starting planet and
  never reallocated. It is now (re)allocated per planet change, and reads are
  bounds-guarded.
- Fixed stale latitude/longitude lookup data leaking across planet changes (the
  clear ran against an already-emptied list, clearing nothing), which could silently
  pave a smaller planet using the previous planet's coordinate mapping.

## 1.1.12 - 2026-07-18

- Fixed Environment Modification UI initialization by using DSP's foundation button slot.
- Added safe UI initialization, diagnostics, and bounded retry behavior for future UI mismatches.

## 1.1.11 - 2026-07-18

- Updated game references for Dyson Sphere Program 0.10.34.28529.
- Updated terrain dirty-state, soil-cost, and soil-source APIs.
- Updated optimized teardown to use the game's component-removal methods.
- Removed machine-specific post-build deployment and added portable package creation.
- Updated the package dependency to BepInEx 5.4.17.

## Prior history

This fork is based on JClark's Bulldozer 1.1.9. The following is the preserved
release history from the prior README.

### 1.1.9

- Fix for a DSP release.

### 1.1.8

- Fixed the action-button position outside sandbox mode and in other tool categories.

### 1.1.7 through 1.1.0

- Restored and hid sandbox UI as appropriate; added the early guide-lines-only mode.
- Made guideline foundation decoration match the configured decoration mode.
- Improved Galactic Scale 2 compatibility and regional-painting behavior.
- Reduced first-load pre-calculation work and added selected-latitude limits.

### 1.0.32 through 1.0.25

- Fixed and accelerated factory teardown, including belt deletion and time-budget controls.
- Updated for game-code changes and made Honest foundation consumption the default.
- Added options to preserve vegetation, skip logistics stations, and customize regional colors.
- Fixed vein-alteration behavior on several planet states.

### 1.0.24 through 1.0.11

- Added landing-capsule protection, guide-mark customization, pole painting, minor meridians,
  a configuration window, soil-pile estimation, tropical-line fixes, and confirmation prompts.
- Added early machine-destruction support.

### 1.0.10 and earlier

- Added BepInEx configuration, vein/foundation overrides, equator/meridian controls,
  frame-update controls, large-planet support, repaving controls, cancellation, and follow-up
  passes to catch missed veins.
