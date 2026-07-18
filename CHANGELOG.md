# Changelog

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
