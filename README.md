# Bulldozer

Bulldozer quickly paves an entire planet, adds painted guide lines, adjusts vein height, and can tear down existing factory machines.

Version 1.1.13 supports Dyson Sphere Program `0.10.34.28529` and BepInEx `5.4.17`. This is a maintained fork of the Bulldozer lineage — original by [Semar](https://github.com/mattsemar/dsp-bulldozer), continued by JClark, magnucha, and [Samox](https://github.com/samox73/dsp-bulldozer-updated); all credit for the mod itself to them. This fork exists because 1.1.12 crashes (`IndexOutOfRangeException` every frame, autosave disabled) on planets larger than the one you loaded in on — most commonly Galactic Scale resized worlds — and the [fix we submitted upstream](https://github.com/samox73/dsp-bulldozer-updated/pull/1) has gone unanswered. If a previous maintainer returns, we will gladly hand this back or deprecate in favor of upstream.

The original lineage was published without a license. We claim no copyright over prior authors' work.

If you have another Bulldozer package installed (Samox-Bulldozer, magnucha-Bulldozer, JClark-Bulldozer, or Semar-Bulldozer), remove or disable it before installing this one. They share the same plugin GUID and can create duplicate UI controls. This package is the same mod plus the crash and mapping fixes, and your existing config carries over.

Back up your saves before using factory teardown, vein movement, or whole-planet terrain changes. Fast Delete is an experimental teardown optimization; disable it in the config if you encounter any inconsistency.

## Features

- Cover a full planet or selected latitude range with foundation.
- Paint equators, tropics, meridians, poles, and configured regional markers.
- Preserve or remove vegetation, and raise or bury veins.
- Optionally consume foundation from inventory, local storage, and local logistics stations.
- Tear down factory machines, with options to retain stations or delete generated trash.

Non-cheaty soil-pile handling remains incomplete: foundation is deducted correctly, but soil-pile limits are not fully honored in every mode.

Open the **Environment Modification** build menu to find the action button.

![Foundation example](https://github.com/mattsemar/dsp-bulldozer/blob/master/Examples/example2.png?raw=true)

![Guide-line example](https://github.com/mattsemar/dsp-bulldozer/blob/master/Examples/example1.png?raw=true)

![Regional colors](https://github.com/mattsemar/dsp-bulldozer/blob/master/Examples/regions.png?raw=true)

## Installation

This mod requires [xiaoye97-BepInEx 5.4.17](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/BepInEx/).

### Manual

Extract the package and place `Bulldozer.dll` in its own folder under `BepInEx/plugins`.

### Mod manager

Install the package through a Thunderstore-compatible mod manager.

## Development

Build with a .NET SDK:

```powershell
dotnet restore .\Bulldozer.sln
dotnet build .\Bulldozer.sln -c Release --no-restore
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\package.ps1
```

The package script creates `artifacts/Bulldozer-<version>.zip` with exactly the files Thunderstore requires at the archive root.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## Acknowledgements

Original mod by Semar, later maintained by JClark. Thanks to runeranger, Madac, Veretragna, Narceen, and Bem on Discord for helpful suggestions and bug reports.

Icons made by [Freepik](https://www.freepik.com).
