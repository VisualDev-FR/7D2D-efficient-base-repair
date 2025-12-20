# 7D2D - Efficient Base Repair System

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Language](https://img.shields.io/badge/language-C%23-blue)

> **An automated maintenance agent for 3D voxel structures, featuring BFS-driven structural analysis, data-driven configuration, and a custom load-balancing algorithm.**

## Overview

This project implements a sophisticated automated repair and upgrade utility for the *7 Days to Die* engine. It introduces a custom functional block capable of scanning, analyzing, and maintaining large-scale structures using advanced pathfinding logic.

Key technical pillars:
* **Structural Analysis:** Uses a specialized BFS algorithm to map contiguous voxel structures.
* **Resource Management:** Real-time inventory tracking to match repair requirements with available materials.
* **Extensible Configuration:** Fully decoupled logic via XML-based parameter injection.

## Technical Implementation

### Structural Graph Traversal (BFS)

To identify damaged components within a contiguous structure, the system implements a **Breadth-First Search (BFS)** algorithm.

* **Logic:** Starting from the source block, the system recursively probes direct neighbors to build a map of the connected structure.
* **Termination Predicate:** The search automatically terminates upon hitting "boundary" voxels (Air, Water, Terrain, or non-structural decorations).
* **Optimization:** To prevent redundant processing, a predicate-based filtering system ignores non-relevant blocks early in the traversal.

### Dynamic Resource Load Balancing

Beyond simple repairs, the system manages automated refueling and reloading for defense systems (turrets, generators, etc.) using a **custom load-balancing algorithm**.

* **Equitable Distribution:** Rather than filling targets sequentially (which could leave the last containers empty), the algorithm calculates the aggregate demand across all connected systems and distributes the available buffer proportionally.
* **Starvation Prevention:** This approach prevents "resource starvation," ensuring that all defensive components maintain operational uptime even when the central supply is limited.
* **Optimized Logistics:** The distribution logic is decoupled from the structural scan, allowing it to process refueling cycles (e.g., Gasoline, Ammo) efficiently within the maintenance tick.

### Performance & CPU Budgeting

Scanning structures exceeding **100,000+ blocks** on a single frame can lead to "micro-stutters" or frame drops. This project implements several mitigation strategies:

* **On-Demand Processing:** Structural analysis is triggered by specific events (UI Interaction, Radial Menu) rather than every frame.
* **Tick-Based Logic:** Automated maintenance tasks are distributed over `n` game ticks to prevent CPU spikes.
* **Configurable Complexity:** The `MaxBfsIteration` parameter allows users to cap the computational cost based on their hardware constraints.

## Data-Driven Configuration

The system's behavior is entirely configurable via [ModConfig.xml](./ModConfig.xml), allowing a clean separation between the core engine logic and gameplay balancing values.

| Property | Description |
| :--- | :--- |
| `maxBfsIterations` | Caps the traversal depth (BFS) to manage CPU resource allocation and performance impact. |
| `needsMaterialsForRepair` | Determines if resource consumption is mandatory for executing repair operations. |
| `needsMaterialsForUpgrade` | Determines if resource consumption is mandatory for executing block upgrades. |
| `refreshRate` | Frequency (in game ticks) of automated structural scans (set to 0 to disable). |
| `repairRate` | Amount of structural integrity restored per tick (0 for instant repair). |
| `upgradeRate` | Number of blocks upgraded per tick (0 for instant, -1 to disable the feature). |
| `playRepairSound` | Toggles spatial audio feedback during the active repair process. |
| `activeDuringBloodMoon` | Enables or suspends system operations during Blood Moon events. |
| `autoTurnOff` | Logic-gate: automatically shuts down the system when no maintenance tasks remain. |
| `keepPaintAfterUpgrade` | Ensures texture/paint data persistence during block state transitions. |
| `lootSize` | Defines the dimensions (columns, rows) of the UI inventory grid. |
| `upgradeSound` | Audio asset identifier triggered upon successful block upgrade cycles. |
| `turnOnAfterReload` | Turn on blocks after auto reloading if they were turned off. |
| `turnOnAfterRefuel` | Turn on blocks after auto refueling if they were turned off. |

## Installation & Integration

### Manual Deployment

1.  Download the latest release from [NexusMods](https://www.nexusmods.com/7daystodie/mods/4861).
2.  Deploy the package to the game's `Mods/` directory.

### Requirements & Compatibility

  * **EAC:** Must be disabled (due to IL injection/Harmony requirements).
  * **Architecture:** Required on both Client and Server (Syncing voxel states and UI data).

## Development & Tooling

  * **Changelog:** Track technical iterations in [CHANGELOG.md](https://www.google.com/search?q=./CHANGELOG.md).
  * **Environment:** Developed for 7D2D v1.3+ using .NET Framework 4.8.
