# Hollowwood implementation review — 2026-11-01

Status: functional corrective slice implemented and tested; full approved plan remains incomplete, not AA accepted.

## Evidence so far

- 13 EditMode tests pass (inventory capacity/identity/stat recomputation, supported generation pool, timeline crossing, previous damage and HUD cases).
- 11 PlayMode tests pass: height-regression pickup with equipment stat change; full inventory retention; wall obstruction; restart cleanup; encounter transitions; slow-frame single hit and stagger interruption; stationary dodge immunity; shrine reward; drop under arch; ten restarts and simulated gamepad inventory focus; focus-loss pause. Exit-path/result regression added after standalone review.
- The encounter progression test uses direct lethal damage and repositioning. It proves state/reward transitions, not combat balance or complete physical-input gameplay.
- Actual Game view frames advanced (767 to 898); imported Animator time advanced. Visual review found and repaired FBX orientation overwrite, terrain blend misuse and presentation property-block initialization.
- Earlier generated actor definitions referenced the wrong MonoScript GUID (PresentationContracts instead of ActorPresentationDefinition). New valid definitions in Assets/Oathfall/Art are assigned through the Editor.
- Development and non-development Windows builds succeeded after the exit-layout and reward corrections. Final shortcut-label fix is included in both successful builds with zero reported build errors/warnings; identifiers and hashes are recorded in [build manifest](../evidence/2026-11-01/build-manifest.json). No minimum-spec, physical-controller, external tester, 10–15-minute pacing or final-AA evidence.
- Synthetic Gamepad audit fought all three encounters without health/damage/position overrides. Hero selection and equipment use API calls; do not describe this as manual/controller-only UI playtesting.
- Actual player review exposed an oversized shortcut obstacle (~203x FBX scale), inaccessible reward placement, under-arch drops on roofs, overlapping labels and a blocked north exit. Fixed through unit-scale obstacle composition, navigation projection, local downward ground ray, nearest-label presentation and an open northern passage. Full route validation is now part of scene authoring.

## Remaining acceptance

Side route/cache, shortcut, basic audio/trails, actor LODs, presentation settings and stat details are implemented. The route is much shorter than 10–15 minutes: the synthetic driver finishes a victory in roughly 35–45 seconds. This fails the requested pacing gate. The scene has recognizable landmarks and textured terrain but remains sparse, repetitive prototype art. Directional locomotion, fitted-armour deformation/weapon-contact polish, richer encounters, final environment art, recorded ambience/music and polished interaction feedback remain production work. Independent scoring and target-hardware measurement remain external acceptance. Compilation does not close these gaps.

Capture uses framebuffer readback and adds allocations. Exclude recorded runs from performance claims. The synthetic driver also allocates; its memory samples are diagnostic, not a zero-allocation gameplay certification.

## Latest standalone evidence

- Ten victorious expeditions completed in the development Windows player. One additional run died after focus loss; automatic pause was added afterward and is covered by the 11th PlayMode regression. This is not ten flawless manual runs.
- Actual normal-build menu and 720p HUD/inventory were inspected with native mouse input. A misencoded empty-slot label was found and corrected. Native 1080p mouse equip changed damage from 26 to 34.3; unequip restored 26. Screenshots preserve the before/equipped/unequipped states. 1440p fullscreen HUD and the full 3440x1440 framebuffer were inspected. Ultrawide exposed a wrapped equipment shortcut label; the label now uses the full button width with an explicit key-binding line. Verified in the final 3440x1440 development framebuffer and 1080p release player. The normal release is open at the Forest Approach.
- [62-second uninterrupted framebuffer video](../evidence/2026-11-01/hollowwood-gameplay-62s.mp4), captured at approximately 10 Hz and encoded at 30 fps, silent. Includes a completed expedition and restart. Runtime motion and a sequence of frames were inspected; it is not independent art approval.
- [Raw input/memory samples](../evidence/2026-11-01/ten-run-input-audit.csv), [player log](../evidence/2026-11-01/ten-run-player.txt), [local metrics](../evidence/2026-11-01/local-runtime-metrics.json), [PlayMode results](../evidence/2026-11-01/playmode-results.json).
- After excluding the first 65 seconds of capture, sampled frame interval median was 16.667 ms; allocated memory varied approximately 155–159 MiB and ended near 156 MiB. These are local diagnostic samples, not CPU/GPU frame timings, VRAM residency or minimum-spec certification.
