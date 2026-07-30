Include ..\AGENTS.md

# Infinite Stockpiles — Mod-Specific Agent Instructions

## Identity
- **Assembly:** `infinitestockpiles`
- **Namespace:** `Calloatti.InfiniteStockpiles`
- **ModId:** `Calloatti.InfiniteStockpiles`
- **Framework:** Bindito DI
- **Publicizer:** removes `Timberborn.InventorySystem`, `Timberborn.Stockpiles`
- **Min Game Version:** 1.0.12.5 — uses `timberborn-decompiled-1.0.*`

## What This Mod Does
Makes stockpiles automatically replenish to 50% capacity. Decorates `StockpileSpec` with `InfiniteStockpileBehavior` that listens to inventory stock changes and disallowed goods changes, resetting stock to half capacity.

## Source Architecture (`Version-1.0/Source/`)

| File | Role |
|---|---|
| `InfiniteStockpiles.cs` | `IModStarter` entry point (via configurator), `InfiniteStockpileConfigurator`, `InfiniteStockpileBehavior` |

## Version Folders
- `Version-1.0` — targets game 1.0.x.x
- `Version-1.1` — targets game 1.1.x.x
