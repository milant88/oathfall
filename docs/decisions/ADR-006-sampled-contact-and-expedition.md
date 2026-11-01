# ADR 006 — sampled weapon contact and encounter-owned progression

Status: implemented; visual acceptance remains separate.

## Decision

Bake 121 actor-local grip/tip samples for each action from the owned Humanoid prefab in the connected Editor. Gameplay owns normalized action time and uses narrow capsule sweeps with interval subdivision, obstruction checks and a per-action target set. Presentation receives the same time and evaluates its Animator at that point. The Raider ground signal projects the active samples. Animation events never apply damage.

Imported skinned mesh BakeMesh positions already include imported scale; applying the renderer scale again produced incorrect paths. Authoring transforms them with renderer position/rotation and stores actor-root-local coordinates. Owned actor forward is corrected by 180 degrees. The regression compares rendered hand-bound and sampled tips within 8 cm; another rejects phantom 2.7 m reach. Models, clip bindings, orientation or scale changes require rebaking.

A shared Core expedition catalog describes twelve stable stops. App tracks only registered live encounter members, advances waves and grants checkpoint rewards once. Ward occupancy is pure Core state, with combat and remaining-threat clearance required. Sanctuaries replenish limited healing resources. The audit supplies synthetic input and ordinary equipment calls; it never changes player health/damage/position.

Navigation baking uses 0.3 m step height below the CharacterController's 0.35 m limit and excludes boulders. A previously complete route graph still stalled physically; actual collider traversal and standalone input playback are required in addition to graph connectivity.

Spawn points must also be on nearby floor height, clear of world colliders and connected to the nearby player. Boulder modifier roots share world layer 8 so the world-only bake actually includes their exclusion. Route-center graph checks alone do not establish safe spawn locations.

## Alternatives and limits

An independent wide analytic sword arc was rejected because hits did not match visible contact. Animation events were rejected as exclusive damage authority because low frame rates/interruption could skip or duplicate damage. Runtime mesh baking every swing was rejected as unnecessary CPU/allocation work. Sample libraries remain content-dependent and must be regenerated after authoring changes.

Only the first combo uses a composed attack/recovery clip; the next two reuse a stable single swing. Enemy roles share the humanoid base. These changes establish functional contact and content variety, not final AA animation/art acceptance.
