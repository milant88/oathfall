# Architecture

## Assemblies

- `Oathfall.Core`: deterministic rules, contracts, IDs, inventory/equipment, stat recomputation and item generation.
- `Oathfall.Gameplay`: actors, combat, loot pickup, NavMesh enemies and input-facing control. References Core and Input System, not App/Presentation.
- `Oathfall.Presentation`: model/Animator, VFX/audio and scene/actor reference definitions.
- `Oathfall.App`: runtime composition, run orchestration, HUD and camera wiring. Connects Gameplay and Presentation implementations.
- `Oathfall.Editor`: content validation and authoring tools.
- `Oathfall.Tests.EditMode`: deterministic rule coverage.
- `Oathfall.Tests.PlayMode`: collision, pickup, action and run/UI transition regressions.

Core must not depend on Unity scene objects, UI, or editor-only APIs. Scene objects are presentation and runtime adapters around Core data.

## Persistence

PLANNED: Profiles and active runs will use separate schema-versioned saves. Active runs will be saved at encounter boundaries. Persistent profile/run saves are not implemented by the corrective slice; it currently resets run inventory on a new expedition.

## Randomness

IMPLEMENTED: item rolls use a seeded stream and retain the seed; instance identity is an independent GUID. An item stores base ID, affixes, rolled values, level and rarity. Unsupported relic/effect IDs do not enter active drops. Separate map/encounter/event streams remain part of the wider roadmap.
