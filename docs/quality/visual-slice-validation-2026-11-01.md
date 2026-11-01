# Hollowwood visual/combat vertical-slice validation — 2026-11-01

## Evidence

- Unity 6000.3.27f2 Editor compilation: no errors or warnings.
- EditMode: 6/6 tests passed, including timing-window boundaries, deterministic item/damage coverage and the hero-menu stale-HUD regression.
- Windows x64 player: `Builds/Oathfall-VisualSlice-PBR3/Oathfall.exe`; build completed with 0 warnings, 0 errors; 117,844,657 bytes.
- Actual standalone player inspected at desktop resolution: the hero menu hides prior run HUD/message content; the Dawnwarden encounter renders without magenta materials; HUD content stays inside the viewport; the Dawnwarden and six Raiders are visible and distinguishable; enemy telegraphs and the combat message appear on active input.
- Pipeline build report confirms `Universal Render Pipeline/Lit`, `Universal Render Pipeline/Unlit`, and the imported 2K cobblestone base/normal textures are packaged.
- Gameplay audit build: `Builds/Oathfall-VisualSlice-Audit3/Oathfall.exe`; build completed with 0 warnings and 0 errors. Menu availability, run start, enemy approach, telegraphs, attack feedback, death-to-menu cleanup and dodge were reviewed in the actual Windows player. After four seconds with no player input, the hero remained at 141/150 health rather than immediately dying; six Raiders have unique first-attack slots.

## Fixed blockers

| Previous issue | Result |
| --- | --- |
| Magenta standalone environment | fixed by saved material assets + scene-owned references |
| Actor invisibility/indistinguishability | fixed at prototype readability level by separate Dawnwarden/Raider visual rigs |
| Left-clipped HUD | fixed through anchor-matching pivots |
| Stale run HUD under the post-death hero menu | fixed and regression-tested in `RuntimeHudTests` |
| Instant, unpresented player hit | replaced by windup/active/recovery timing with a one-hit target set |
| Raider alpha strike after all enemies reached melee | initial attack cadence now starts at melee entry, uses six unique staggered encounter slots, and the player dodge has zero incoming damage during its active window; covered by `EncounterRules_StaggersFirstEnemyAttacksAndMakesDodgesInvulnerable` |

## Honest remaining failures

This build does **not** meet the requested AA visual-quality gate. A CC0 2K Poly Haven cobblestone base-color/normal set is now assigned to the shrine stone as a real PBR import proof, but the environment composition and actor rigs remain Unity-primitive presentation proxies rather than final textured meshes. The current audit confirms the core prototype loop but does not replace a five-minute skilled player session, controller-only session, full loot-clear loop, production animation review, performance profile or independent tester score.

The next acceptance task is to choose and manifest approved free/owned art sources, import one real Dawnwarden/Raider pair with validated rigs/materials, and repeat the same standalone review without relaxing this record.
