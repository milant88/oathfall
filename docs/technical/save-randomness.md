# Save, IDs, and deterministic randomness

## Stable identity — planned contract

Content definitions use human-reviewed stable string IDs. Persistent data stores IDs and values, never Unity object references or instance IDs. A content catalog resolves IDs and validates duplicates/references at authoring time.

## Random streams — implemented item stream; broader split planned

Use independent streams for route maps, encounters, loot, and events. The current `ItemGenerator` takes an explicit seed and produces repeatable item data; this behavior is **verified** by EditMode tests. Future systems must not share a stream with rendering, audio, or VFX.

## Save contract — planned

`ProfileSave` and `ActiveRunSave` are distinct, atomically written with a backup and independent schema version. Runs record content revision. On an incompatible update, preserve profile progress, explain that the run cannot resume, and return safely to sanctuary.

At encounter entry, record a snapshot and seed. An exit during combat reloads that boundary; an encounter completion records rewards and progression exactly once. Migration fixtures are mandatory for every schema increment.
