# ADR 005 — gameplay and presentation composition

Status: implemented, 2026-11-01.

Core owns pure inventory/equipment/stat rules, attack timing and the engine-independent actor-view contract. Gameplay owns CharacterController/NavMeshAgent actions, damage and collection. Presentation owns rigged models, Animator, materials, sound and visual state. App connects the implementations and owns scene flow, HUD and camera wiring.

Gameplay does not reference Presentation or App. IGameSession/GameplaySession exposes only the runtime service needed by actors and pickup. App installs and clears that session with its lifetime. Animator clips present gameplay state; they never apply damage. Interval crossing and a per-swing hit set prevent skipped or duplicate hits.

Scene contexts and actor definitions reference build-included prefabs/materials/controllers. Shader.Find is limited to Editor authoring. NavMesh data is an authored asset, and scene generation validates a complete path from spawn through all encounters to the exit.

Rejected: runtime primitive fallback, animation-event-exclusive damage, silent unsupported affixes, and claiming an imported model is final production art.
