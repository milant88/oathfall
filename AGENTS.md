# Oathfall agent instructions

## Working agreement

Read `README.md`, `docs/INDEX.md`, `working_knowledge/current/*`, and all applicable nested instructions before changing Oathfall. Research existing patterns and write a numbered task plan before implementation. Do not change unrelated user work or commit unless asked.

## Architecture rules

- Keep pure rules, randomization, identifiers, and save models in `Assets/Oathfall/Core` without Unity scene or UI dependencies.
- Keep gameplay orchestration in `Assets/Oathfall/Gameplay`; keep UI, camera, VFX, and audio presentation outside Core.
- Use ScriptableObject definitions and stable string IDs for authored content. Save IDs and primitive values, never object references.
- Use independent random streams for route, encounter, loot, and event generation.
- Use the Input System and uGUI/TextMeshPro for runtime UI. Do not add `OnGUI`.
- Do not hand-edit Unity scene, prefab, or asset YAML while a connected Editor can make the change.

## Quality gates

Add meaningful EditMode tests for deterministic rules and focused PlayMode tests for state transitions. Run the narrowest relevant checks during iteration, then compile, inspect the console, and verify the changed gameplay in Play mode. Update working knowledge and the matching design or technical document at meaningful milestones.

## Version control

Use Git LFS for binary assets. Never stage `Library`, `Temp`, `Logs`, `UserSettings`, generated IDE files, or build output. Keep source assets with their `.meta` files. Branches require a ticket key unless the user explicitly authorizes a ticketless branch. Commit only when requested.
