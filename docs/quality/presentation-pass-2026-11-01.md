# Hollowwood presentation improvement pass

Status: implemented and locally validated, content revision 3. This focused presentation pass is complete; full roadmap, production-art and 10–15-minute pacing gates remain open. Previous functional evidence remains in hollowwood-implementation-2026-11-01.md.

## Research and direction

Primary sources checked 2026-11-01:
- https://www.supergiantgames.com/games/hades-ii/ — active sequel/reference for action roguelite presentation; not a market-share ranking.
- https://norestforthewicked.com/ — October 2026 class-system and armour previews, precision-ARPG positioning.
- https://store.steampowered.com/app/2071280/Ravenswatch/ — dark-fantasy action roguelike with hero/build diversity; recent reviews are mixed, so popularity is not blanket design validation.

Our inference: invest in distinct silhouettes, readable attack commitment/recovery, coherent authored environments and meaningful equipment feedback before adding content breadth or expensive rendering features. Defer co-op/live services; preserve offline scope. Existing source-library clips are limited: no dedicated strafe clips in the approved free import. Do not label generic forward jogging as finished directional locomotion.

## Baseline defects

Sparse uniform plant scattering, capsule-like bush appearance, broad tan floor and flat midday-like illumination. Animator Move parameter was not driving a blend tree; all attack clips were fitted to the same 0.75 s despite different gameplay windows, and Cleave compressed a 4.33 s multi-swing combo into one attack. Existing melee timing and pickup regression tests remain essential.

## Acceptance

Results and limitations are recorded below. No claim of AA acceptance, physical-controller verification or minimum-spec performance from the local RTX-class machine.

## Implemented and reviewed

- Owned Blender woodland kit: three fern and three boulder variants plus roots, clustered at road margins; 2K Poly Haven rock maps and source/license hashes preserved. Forest tree perimeter is denser; combat lanes remain clear and NavMesh route validation passes.
- Global Volume now contains persistent ACES, color adjustments (exposure +0.05, contrast 12, saturation -8), bloom (0.16, threshold 1.15) and vignette (0.16). Main camera HDR/post-processing enabled; Volume layer 0 is included. No gameplay DOF/motion blur/chromatic aberration.
- Warm main directional light, cool shadow fill and shrine amber accents; reflection probe has a nonblack sky fill. Steel retains scanned normals/roughness but uses a readable authored albedo. Terrain albedo alpha is explicitly ignored; DirectX cobblestone normal flips green on import.
- Animator locomotion blend tree, actual-speed parameters and travel-facing models. Hero attacks use individual gameplay durations; Raider swings use 1.35 s. Cleave uses one swing instead of a compressed multi-swing animation. Dodge travel/recovery 0.42 s, protection remains 0.25 s.
- Reused per-actor impact emitter, temporary enemy health bars and no impact event on immune/dead targets. Mouse-wheel zoom is bounded and ignores input over UI or during pause/inventory. Camera cutaway hides only marked arch/wall renderers, preserving physics/navigation and restoring prior renderer states.

## Findings repaired during review

1. Volume overrides disappeared after asset reload because they were transient objects. Fixed by storing VolumeComponent sub-assets and checking persistence in build validation; reimport/repeat-authoring regression passes.
2. Initial fern normals faced down and plants were too large. Corrected owned mesh winding and reduced placement scale; green upper leaf surfaces inspected.
3. Initial contrast/shadows obscured the characters. Added controlled fill/reflection and revised actor steel/cloth albedo; verified visible actor silhouette.
4. Arch hid the player in actual standalone combat. Added visibility-only cutaway, a ray aimed at the lower body, and collision/restoration plus actual-scene regressions.
5. Final shrine approach exposed canopy occlusion beyond the narrow trunk collider. Static tree render bounds now participate in visibility-only cutaways; no extra gameplay collider or navigation obstacle is introduced. Bounds are cached once and tested without per-frame allocation. Collider-free canopy restoration/registry cleanup has a separate regression.

## Final validation

- 14 EditMode and 17 PlayMode tests pass, including new lighting persistence, Cleave playback timing, dodge recovery vulnerability, immune/dead impact suppression and camera-cutaway restoration.
- Editor frames advanced 508 → 1031 and Animator normalized time advanced; this was a live scene, not a frozen screenshot.
- Intermediate standalone builds completed eight and then three synthetic-input victories while visual iteration continued. Those runs predate the final camera cutaway; final build/video evidence follows. Synthetic input is not physical-controller verification. Hero selection/equip use direct audit APIs; health/damage/position are not overridden.
- Final Windows development `build_9f94190e3885` and release `build_7ecbe363da39` succeeded with zero build errors/warnings. Build manifest includes Editor/package/source/scene hashes and owned assembly hashes.
- Final synthetic-input audit completed one victory, then a death and successful restart during approximately 92 seconds before manual closure. The 62.4-second continuous silent framebuffer video includes the complete first victory and part of the second run. Movement/combat use synthetic Gamepad; selection/equipment use direct APIs. No health/damage/position override. This is not a ten-run soak or physical-controller acceptance.
- Final standalone 1080p scene, shrine approach canopy fix and gameplay contact sheet inspected. Native release mouse hero selection works; the release is left open at Forest Approach. Earlier native zoom, inventory, pause and 3440×1440 HUD review were on the same presentation revision before the final canopy-only fix; no new 720p/1440p validation is claimed this pass.
- No application exceptions/assertions observed in final player logs. D3D12 reported an info-queue query warning during initialization; rendering continued. Capture readback and concurrent Editor/build work invalidate performance comparisons; no CPU/GPU/minimum-spec certification is claimed.
- Adversarial review repaired transient Volume components, fern winding/scale, dark actor materials, arch lower-body and canopy occlusion. Narrow tests and affected player scenarios were repeated. Static foliage bounds assume authored trees remain stationary; future wind/deformation needs conservative bounds. Hard visibility cuts and strong wet-stone highlights remain art polish, not blockers for this prototype.

## Evidence

- [Build manifest](../evidence/2026-11-01-presentation/build-manifest.json)
- [EditMode results](../evidence/2026-11-01-presentation/editmode-results.json), [PlayMode results](../evidence/2026-11-01-presentation/playmode-results.json)
- [Before](../evidence/2026-11-01-presentation/before-approach.jpg), [final release](../evidence/2026-11-01-presentation/after-approach.jpg), [canopy combat fix](../evidence/2026-11-01-presentation/canopy-combat-fixed.jpg)
- [62.4-second gameplay](../evidence/2026-11-01-presentation/gameplay-62s.mp4), [review sheet](../evidence/2026-11-01-presentation/gameplay-review-sheet.jpg), [ultrawide review](../evidence/2026-11-01-presentation/ultrawide.jpg)
- [Final player audit log](../evidence/2026-11-01-presentation/player-audit.txt), [input audit](../evidence/2026-11-01-presentation/input-audit.csv)


## Remaining limits

Still a prototype, not AA accepted. Exact blade contact, authored strafing/turn/start/stop clips, model deformation and production animation/audio need further work. Environment composition and content/pacing remain shallow; no 10–15-minute acceptance. Cutaway is a hard visibility change, not a dither fade. Physical controller, independent art reviews and minimum-spec hardware remain external gates.
