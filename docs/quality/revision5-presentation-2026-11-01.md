# Revision 5 — actors, combat motion, equipment and woodland

Status: implementation and local standalone review complete, 2026-11-01; AA acceptance remains open. This report supersedes revision 4 for the changed presentation below. It does not certify AA art, minimum hardware or physical-controller play.

## Implemented

- Four distinct owned enemy mesh derivatives: scavenger Raider, robed/staff Hexblade, lamellar/hammer Breaker and crowned Guardian. Each uses the licensed humanoid base, named bone weights, valid avatar and two reduced LODs. Their silhouette additions are actual geometry, not only recoloring. They still share a base body and most locomotion/attack clips; sculpted production characters remain open.
- Three separate Dawnwarden humanoid clips: lateral cut, reverse cut and overhead finisher. An Editor authoring tool solves the arm trajectory and writes humanoid muscle curves over the source idle pose. These are authored IK-assisted clips, not motion capture. Gameplay owns timing, interrupt/cancel windows and damage; 121 baked grip/tip samples per action follow the actual retargeted model.
- Six separate gear models, lit icons and matching world drops. Equipment has a 24-card bag, worn slots, actual properties/stat deltas, explicit equip/unequip/discard and a rotating, draggable, zoomable 3D item studio. Inspecting an item never equips it. The preview is the selected gear asset, not a dressed character. There is one model per active slot/base family, not unique art for every roll.
- HUD action cards expose keys, ability readiness and draught count. Equipment/settings hide underlying combat HUD, with inventory-operation feedback inside the panel. Safe-margin scaling retains the full panel at 720p.
- Branching tree meshes with layered leaves/LODs, grass blades, shrubs, flowers, scattered masonry and less uniform paving. Existing route/collision clearance remains authoritative. The space still repeats modular landmarks; no claim of a finished AA environment.
- Forest field ambience, a restrained owned musical bed, recorded swing/step/metal/loot/UI cues and owned rune/ward/victory cues. Master/Music/Ambience/SFX settings persist. Eight UI/event voices are reused; actor sources use distance rolloff except the local hero. This is source-volume routing, not a finished AudioMixer asset or final mastering pass.

## Repairs found during review

1. The first weapon icon omitted a blade surface because of inward mesh normals. Recalculated closed mesh normals in the Blender source and regenerated models/icons.
2. Stock second/third swing trajectories did not pass forward-contact review. Replaced them with separately authored trajectories and rebaked all five actor contact libraries.
3. Tree leaves appeared black in the player: leaf triangles faced downward. Corrected winding in the source, rebuilt LODs, and applied restrained shaded-leaf transmission compensation. A subsequent ordinary-player view confirms green, readable canopies.
4. Equipment metal was overlit and carried an unsuitable industrial plate normal. Removed that normal from display gear and reduced studio/icon light intensity. Review the corrected final player evidence below.
5. Combat HUD clutter remained visible behind Equipment. Hide it while modal panels are open; retain inventory errors in a dedicated panel footer.
6. Hit flash property blocks erased authored rune emission after the flash ended. Clear the override to restore the original material.
7. Opt-in audio recording could race destruction with its audio callback. Lock only the buffer handoff/copy; file serialization remains outside the audio callback.

## Validation and evidence

The final product gate passed **20 EditMode and 29 PlayMode tests**, zero failed/skipped. Coverage includes unique build-required gear references, missing-content rejection, read-only stat comparison, all three distinct visible-blade paths within 8 cm, preview suspension/disposal, one hit per target, low-frame intervals, interruptions, pickup capacity/deduplication, equipment replacement, route traversal and accessible enemy spawns. Final modal changes passed the affected Equipment/keyboard checks; the observed off-mesh spawn defect has a red/green regression.

Native inspection covered menu, fresh run, all six catalog models, drag rotation and equip/unequip/discard. Clean panel views were inspected at 720p, 1080p, 1440p and ultrawide. The final 1440p opening-frame capture confirms synchronous HUD hiding. Synthetic combat uses actual Input System Gamepad events and direct equipment APIs without position/health/damage overrides; it is not a human playtest.

The first 65-second Unity output-mix recording contains audio (mean -30.1 dBFS, peak -8.4 dBFS, no full-scale clipping). Waveform/level checks are not a subjective listening review. Short/full videos must state their measured capture cadence; screenshot readback excludes those sessions from performance certification.

## Open production work

Distinct silhouettes and three clips are implemented, but close-up anatomy, armour fit, cloth, footwork/weight transfer, start/stop/strafe transitions, role-specific casting and final weapon contact polish require further art iteration. The actor lineup makes these limitations visible. No independent 4/5 scoring, physical-controller session, minimum-spec GPU test or ten-run revision-5 soak has been completed. Audio still needs listening/mix and surface-specific step review. Equipment needs more base-model variety and actual worn-gear appearance changes before a full item catalog expands.

## Native Equipment review (final development build)

At 1280x720, a real collected Common Lv.1 boot item showed +3 armour. Native mouse EQUIP SELECTED changed22 -> 25 and removed it from the bag; selecting the worn boot slot and UNEQUIP returned25 -> 22 with one bag item. DISCARD removed that item, kept22 armour and disabled stale actions. The footer displayed the operation result inside the panel. 3D models and inventory text stayed within the screen; underlying combat HUD is now hidden. Source inventory was obtained by synthetic combat, while these UI operations used native window clicks.

The pre-spawn-repair short video is retained as `gameplay-before-spawn-repair-60s.mp4`: 1653 source frames in the first 60 seconds, 27.56 Hz average, 33.91 ms median interval, 547.78 ms maximum startup/capture stall. Output 30 fps resamples/repeats frames; it is not a 60 fps performance proof. After 65 seconds the full capture targets 5 Hz. Its 65-second audio mix measured mean -30.4 dBFS, peak -8.4 dBFS. Native controls muted and restored the UI/event volume.

A focused Input System keyboard check opens/closes Equipment with I and pause with Escape at zero timeScale and is included in the 49-test final gate. Native helper-injected I/Escape did not visibly change the Windows player's UI, although native clicks worked and Alt+F4 closed a test window. This remains an unverified native keyboard-input path, not evidence of a physical keyboard test. Avoid claiming complete hardware coverage.

## Standalone spawn defect found at result review

The second full run reached victory in 632.4 active seconds / 241 kills, but its development console exposed four `No accessible encounter spawn` errors in Whispering Grove. This is **diagnostic evidence, not a clean final pass**. The first643.4-second run did not log these errors. Retain the video/log and victory-before-spawn-repair.png rather than hiding the failure.

Trace reproduction at 332.30 seconds: player (-14.60427,0,174.7434), nearest NavMesh point (-15.45,.05,175.05), distance 0.901 m. The smaller CharacterController could stand outside the agent's baked clearance. CalculatePath to the exact off-mesh endpoint rejected every otherwise valid spawn candidate. A focused test using all four failed origins fails before the repair and passes afterward, including rejection of a distant invalid endpoint. CombatSpatial now projects only the target endpoint within 1.5 m and 0.5 m vertical tolerance before checking complete connectivity. It never teleports the player or bypasses capsule/floor checks.

The UI opening-frame HUD flash was also reproduced in framebuffer evidence; visibility now updates synchronously when a panel opens. Affected Equipment/keyboard tests pass4/4 afterward. Steady720p/1080p and3440x1440 views show the clean modal; the1440p opening-frame image is retained as pre-fix diagnostic evidence.

Post-repair full gate: **29/29 PlayMode passed**, including the observed spawn-edge regression and paused-keyboard path; combined with20/20 EditMode this is49unique passing tests. The first attempted rerun was cancelled because a native focus change invoked the game's intentional pause during a route test. It is excluded. The uninterrupted rerun completed in75.64test seconds. No physical-controller or minimum-spec claim follows.

## Audit harness repair and evidence boundaries

The first post-spawn-repair player audit exercised three deaths/restarts at the first ward (123.3, 185.1 and 152.0 seconds), without application errors. Preserve `player-draught-held.txt`, `input-audit-draught-held.csv` and `death-postrepair.png`. The driver held D-pad up continuously below half health; the game correctly consumes draughts on press edges, so further healing requests were lost while health stayed low. The development-only driver now releases between requests. Health, cooldown, damage, movement and encounter difficulty are unchanged. This is a test-harness correction, not a claimed gameplay balance fix. The subsequent player pass completed, as recorded below.

The final 1440p native inspection confirms both the boot model and shield catalog preview. `equipment-opening-postrepair-1440.png` verifies that the old combat HUD is hidden in the same opening frame. Empty equipment slots explicitly say that the model is a catalog preview; disabled actions do not imply item ownership.

Actor LOD0 skinned-mesh totals are 16,735 triangles for Dawnwarden and 15,966–18,344 for enemy variants. Each has two reduced LODs. Their 13–36 separate renderers remain a draw-call consolidation opportunity; triangle counts alone are not a performance pass. See `actor-mesh-budget.json` for per-LOD data.

Both builds succeeded with zero reported build warnings/errors: development `build_7e45cf03ba68`, ordinary `build_4d1b18de3c45`. The development build includes the audit-input correction; ordinary builds exclude that driver. The native D3D12 startup diagnostic `failed to query info queue interface (0x80004002)` is present and does not stop rendering; it is not silently counted as a clean log.

### Evidence index

- [Build/source manifest](../evidence/2026-11-01-revision5/build-manifest.json)
- [20 EditMode results](../evidence/2026-11-01-revision5/editmode-results-final.json), [29 PlayMode results](../evidence/2026-11-01-revision5/playmode-postrepair.json)
- [Spawn regression before](../evidence/2026-11-01-revision5/spawn-edge-red.json) and [after repair](../evidence/2026-11-01-revision5/spawn-edge-green.json)
- [60-second gameplay with captured game audio](../evidence/2026-11-01-revision5/gameplay-60s.mp4)
- [Actor lineup](../evidence/2026-11-01-revision5/actor-lineup.png), [1440p opening frame](../evidence/2026-11-01-revision5/equipment-opening-postrepair-1440.png), [shield studio](../evidence/2026-11-01-revision5/equipment-shield-final-1440.png)
- [720p equipped item](../evidence/2026-11-01-revision5/equipment-equipped-720.png), [1080p panel](../evidence/2026-11-01-revision5/equipment-stable-1080.png), [3440x1440 panel](../evidence/2026-11-01-revision5/equipment-ultrawide-stable.jpg)

## Final closeout

The corrected audit recorded a death after 348.8 active seconds / 132 defeated, then a full victory after **630.5 active seconds / 240 defeated**, passing every stop and the northern exit. The entire trace is retained; this is not a claim of two successful runs. No application exceptions, missing authored assets or inaccessible spawn/reward errors occurred in this final trace. The D3D12 info-queue diagnostic noted above remains present. No health, damage or position overrides were used.

The complete [post-repair audit video](../evidence/2026-11-01-revision5/full-audit-postrepair.mp4) is 1028.36 seconds, 1280x720, silent and preserves the death/restart plus victory. It uses variable capture cadence: about 29.54 Hz during the first minute, then a 5 Hz target after 65 seconds. The [60-second excerpt](../evidence/2026-11-01-revision5/gameplay-60s.mp4) is 1080p with Unity's captured audio (mean -30.0 dBFS, peak -8.2 dBFS). Its 30 fps output resamples 1772 source frames; median capture interval 31.95 ms, maximum startup/readback interval 543.63 ms. These are evidence captures, not minimum-spec or 60 fps performance results.

Sources: [final summary](../evidence/2026-11-01-revision5/player-summary-final.json), [full trace](../evidence/2026-11-01-revision5/input-audit-postrepair-final.csv), [player log](../evidence/2026-11-01-revision5/player-postrepair-final.txt), [victory screen](../evidence/2026-11-01-revision5/victory-postrepair.png), [sampled full-video review](../evidence/2026-11-01-revision5/full-audit-contact-sheet.jpg).

The ordinary non-development player was reopened at 1920x1080 and visually reached the hero menu with no magenta, missing scene objects or clipped menu text; [launch capture](../evidence/2026-11-01-revision5/ordinary-closeout-menu.png). It is left open for the user's own play. Physical keyboard/controller acceptance remains separate from helper-generated input. All 73 owned-manifest hashes match; documentation links and owned Unity meta files validate. The existing 49-test results are reused because this closeout changes documentation/Git configuration, not product behavior.

Adversarial review outcome: local implementation checks pass; explicit open production gates are close-up art/animation quality, subjective audio review, physical input, minimum-spec profiling and repeated-run human difficulty. Nothing in this report certifies AA production quality.
