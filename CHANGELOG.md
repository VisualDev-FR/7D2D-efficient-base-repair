# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.3] - 2025-11-08

### Fixed
- Fixed OutOfBoundsException when there is more required materials than anticipated in xui
- Fixed compatibility issues for V2.4b7

## Changed
- Changed testing prefab to test configuration with lot of repair materials

## Added
- Added new testing block 'ebrTestManyRepairItems'

## [2.0.2] - 2025-09-10

### Fixed
- Fixed OutOfBoundsException when there is more required materials than anticipated in xui
- Fixed compatibility issues for V2.3b9

## [2.0.1] - 2025-08-27

### Fixed
- Fixed NullReferenceException happening when a ranged trap has a null AmmoItem. EBR Will takes the first ammo type if several are specified in blocks.xml

## [2.0.0] - 2025-06-23

### Added
- Added missing documentation for ingame command-line tool

### Fixed
- Fixed compatibility issues for v2.0b289

## [1.1.1] - 2025-05-04

### Fixed

- Fixed NullReferenceExceptions when a solar pannel is connected to the structure

## [1.1.0] - 2025-04-20

### Fixed
- Fixed multiblocks being ignored when no connection exists on the parent block.

### Changed
- Moved EfficientBaseRepair parameters from `Config/blocks.xml` to [`ModConfig.xml`](./ModConfig.xml).

### Added
- Added automatic refueling for power source blocks (generators).
- Added automatic reloading for ranged blocks (turrets, dart traps, etc.).
- Added a new console command entry for debugging. Use `help ebr` to view documentation.
- Added a new testing prefab: `efficientbaserepair_testing_01` — useful for testing or understanding how the mod works.
- Added a new [Logging](./Scripts/Utils/Logging.cs) system.
- Added a new [ModConfig](./Scripts/Utils/ModConfig.cs) system.
- Added a new parameter `turnOnAfterReload` in ModConfig.xml
- Added a new parameter `turnOnAfterRefuel` in ModConfig.xml

## [1.0.6] - 2024-11-21

### Fixed
- fix keepPaintAfterUpgradeOption
- fix NullArgumentException raised from TileEntity.GetUpgradeMaterialsForPos()

## [1.0.5] - 2024-11-19

### Fixed
- Fix KeyNotFoundException raised when modifying the structure while the repair / upgrade processes
- Fix crate emptying after update
- Fix windows blocks reparing / upgrading

### Changed
- (internal) change the way to repair repair blocks, by uning vanilla Block.DamageBlock()

## [1.0.4] - 2024-09-17

### Added
- Add pdb file into the release to make easier the debugging process
- Add skill book progress infos

## [1.0.3] - 2024-08-13

### Fixed
- Fix progression.xml: hammer and nailgun was not craftable after level 50.
- Impossible to enable auto repair during blood moon.

### Changed
- Dynamic properties from xml are now stored in static field instead of loading them with the method 'Init'

## [1.0.2] - 2024-07-13

### Fixed
- Fix Xui Method not found error by rebuilding binaries for b317.

## [1.0.1] - 2024-06-27

### Fixed
- Prevents crates wiping by ignoring the upgrade of crates. (hotfix)

## [1.0.0] - 2024-06-24

### Fixed
- The `upgrade on` button was not persistant, and was set to off at each game restart

### Changed
- The mod is now compatible with version 1.0 of the game
- A21 is not supported anymore

## [0.1.1] - 2024-06-09

### Fixed
- Fix performances issues due to useless stats refresh

## [0.1.0] - 2024-06-09

### Fixed
- On dedicated servers, the structure was analysed three times at each refresh.
- Spike blocks at stage dmg=0 or dmg=1 were upgraded for free

### Changed
- The xml parameter `NeedsMaterials` was renamed to `NeedsMaterialsForRepair`
- The default xml parameter `AutoTurnOff` was set to true

### Added
- new batch script to start a local dedicated server
- Add possibility to upgrade structures with a dedicated button
- Add the number of upgradable blocks in the stats pannel of the UI
- Add the total time (repairing + upgrading) in the stats pannel of the UI
- New xml parameter `UpgradeRate`
- New xml parameter `NeedsMaterialsForUpgrade`
- New xml parameter `KeepPaintAfterUpgrade`


## [0.0.3] - 2024-06-04

### Fixed
- Fix translation of the refresh button
- Fix repair timer for dedicated servers
- Fix the UI header text

## [0.0.2] - 2024-06-03

### Fixed
- The block will not generate corrupted chunk anymore

## [0.0.1] - 2024-06-02

### Added
- Implement the repairation blocks algorithm
- Create a dedicated UI for the block
- Set the repairation algorithm parametrizable from xml files


[unreleased]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/master...unreleased
[2.0.2]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/2.0.1...2.0.2
[2.0.1]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/2.0.0...2.0.1
[2.0.0]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.1.0...2.0.0
[1.1.1]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.1.0...1.1.1
[1.1.0]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.6...1.1.0
[1.0.6]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.5...1.0.6
[1.0.5]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.4...1.0.5
[1.0.4]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.3...1.0.4
[1.0.3]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.2...1.0.3
[1.0.2]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.1...1.0.2
[1.0.1]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/0.1.1...1.0.0
[0.1.1]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/0.1.0...0.1.1
[0.1.0]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/0.0.3...0.1.0
[0.0.3]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/0.0.2...0.0.3
[0.0.2]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/compare/0.0.1...0.0.2
[0.0.1]: https://github.com/VisualDev-FR/7D2D-efficient-base-repair/tree/0.0.1
