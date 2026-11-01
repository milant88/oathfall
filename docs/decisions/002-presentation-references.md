# ADR-002: Scene-owned presentation references and phased attack ownership

Status: accepted, 2026-11-01.

## Context

The first Windows player rendered magenta because runtime code relied on `Shader.Find` and generated materials that did not make the required shader/material inclusion unambiguous. Primitive actors also had no visible action phases.

## Decision

`HollowwoodSceneContext` owns serialized references to the palette and actor presentation definitions. `OathfallVisualAssetSeeder` creates the project-owned URP materials, definitions and controller shells through the Unity Editor; the saved scene references them directly. Gameplay never discovers a shader at runtime.

Attack timing is owned by the pure `AttackTiming` contract and `PlayerController` phase flow. Presentation receives phase changes for posing/feedback; it cannot independently apply damage.

## Consequences

- The player build includes the exact Lit/Unlit shaders and material assets used by the scene.
- Core remains pure. Presentation has no Gameplay dependency; Gameplay consumes presentation types.
- Generated assets are a transition baseline, not a substitute for source-controlled production model/texture work.
