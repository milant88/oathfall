# Changelog

## Unreleased — presentation and equipment, content revision 5

- Distinct Raider, Hexblade, Breaker and Guardian geometry, valid Humanoid rigs and reduced LODs.
- Three authored Dawnwarden combo trajectories with matching baked blade contact.
- Six gear meshes/icons/world drops, a lit draggable 3D item preview, explicit inspect/equip actions, comparisons and a rebuilt HUD.
- Branching tree/understory variants and corrected leaf normals; coherent shaded foliage and reduced paving uniformity.
- Recorded CC0 combat/UI/forest sounds, owned music/magic cues and persistent Master/Music/Ambience/SFX controls.
- Fixed erased rune emission, overlit gear, unsuitable metal normal and modal HUD clutter.
- Fixed rejected enemy spawns when the player stands just outside agent clearance; the observed-position regression passes. Modal HUD visibility now updates in the opening frame.
- Corrected development-only audit healing input to release between presses; gameplay difficulty is unchanged.
- Validation and remaining AA limitations: [revision 5 review](../quality/revision5-presentation-2026-11-01.md).

## Unreleased — extended expedition, content revision 4

- Twelve connected stops with mixed melee, targeted magic, heavy strikes, three active ward defenses, sanctuaries and a guardian.
- Sampled animated blade contact and matching normalized presentation time; completed first swing/recovery and corrected model forward/socket scale.
- Limited restorative draughts, inventory disposal with stale-selection protection, encounter names and active run duration.
- Dry terrain masks, shared muted palette, extended landmarks/vegetation; navigation climb matches the player and boulders are excluded from walking surfaces.
- Full validation and remaining production limitations: [current review](../quality/extended-expedition-2026-11-01.md).

## Unreleased — Hollowwood presentation pass, content revision 3

- Persistent ACES/bloom/color/vignette overrides; fixed profile effects disappearing on reload/build. Added persistence regression and build validation.
- Clustered ferns, roots and scanned-rock surfaces; denser tree perimeter, cooler canopy fill, improved steel readability and opaque terrain albedo import.
- Speed-based locomotion blending, travel-facing models, duration-matched attacks and single-swing Cleave. Dodge recovery now 0.42 s with unchanged 0.25 s protection.
- Reusable per-actor impact emitter, temporary enemy health bars, mouse-wheel camera zoom and suppression of false impact feedback on immune/dead targets.
- Visibility-only arch/wall and canopy cutaways preserve collision/navigation; collider-free tree crowns use cached render bounds.
- Production art, exact weapon contact, minimum-spec/controller acceptance and 10–15-minute pacing remain open.

## Unreleased — Hollowwood corrective slice, content revision 2

- Actual item ownership, capacity, equipment comparison/icons and stat recomputation; unsupported affixes removed from active rolls.
- Grounded controls, aimed attacks, immediate dodge protection, interval-safe single-hit sweeps, abilities and NavMesh Raiders.
- Connected forest/camp/shrine, optional cache/shortcut, owned stone kit, imported CC0 rigs/animations, fitted armour, actor LODs, PBR surfaces and presentation settings.
- Basic audio/trails, nearest-loot labels and explicit build validators. App assembly removes Gameplay/Presentation coupling.
- Added pickup/collision/equipment/input/transition regressions. Standalone review remains authoritative; 10–15-minute pacing and final-AA acceptance are not achieved.

## 0.1.0 — Foundation prototype

- Created the Unity 6.3.27f2 URP project and local Git/LFS foundation.
- Added Core, Gameplay, Presentation, Editor, and EditMode test assembly boundaries.
- Added hero selection, Dawnwarden arena movement/attack/block/dodge, hostile prototypes, deterministic drops, HUD, and camera.
- Added deterministic item-generation and damage-rule coverage.
- Validated a Windows x64 development build with zero warnings or errors.

## 0.1.0 — unreleased

- Created the Unity 6.3 LTS URP foundation.
- Added local Git/LFS configuration, durable documentation, working-knowledge templates, and the initial Dawnwarden combat-and-loot prototype.

Revision 4 validation (2026-11-01): 42 tests passed, development/release builds successful; full synthetic expedition victory in 11:47 with 232 defeated. Navigation/spawn and inventory selection findings fixed. Production art and independent acceptance remain open.


