# Bulldozer

Bulldozer quickly paves an entire planet, adds painted guide lines, adjusts vein height, and can tear down existing factory machines.

Version 1.1.11 supports Dyson Sphere Program `0.10.34.28529` and BepInEx `5.4.17`. It is an updated fork of the original work by Semar and JClark. Do not install it alongside another Bulldozer fork: they share the same plugin GUID and can create duplicate UI controls.

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
