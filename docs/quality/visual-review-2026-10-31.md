# Standalone visual review — 2026-10-31

Result: FAIL. This is an early technical prototype and does not approach the intended AA visual quality. A successful compile/build is not visual acceptance.

## Procedure and evidence

Launched the latest Windows executable through Computer Use and inspected its actual 2560×1440 window. Selected Dawnwarden with the mouse, entered the arena, sent dodge and attack inputs, and observed the return to hero selection after defeat. Dodge input changed the camera framing. The rendering defects prevented reliable visual assessment of actors, hit registration, block timing, or combat feel. No full combat or controller test is claimed.

## Findings

1. **Blocking:** arena floor and pillars render magenta in the standalone build. The previous material-asset addition did not fix this. `PrototypeLit.mat` exists outside Resources and is not referenced by the runtime, so its mere presence does not guarantee shader inclusion. Confirm the cause through player shader/material diagnostics before fixing it.
2. **Blocking:** hero/enemies are not visually distinguishable in the captured arena. Revalidate actor visibility after fixing rendering.
3. **High:** run status and message text extend off the left edge. `RuntimeHud.SetRect` forces a centered pivot even for left-edge anchors; correct the layout and test supported resolutions.
4. **High:** the environment is a flat primitive plane with cylindrical pillars; no authored fantasy environment, curated textures, or art composition is present.
5. **High:** hero choices are plain colored rectangles; descriptions advertise resources/abilities that remain planned. Actor models, weapon animation, readable combat telegraphs, and effects are not production-ready.

## Required next gates

Fix standalone materials, actor visibility, and HUD layout first. Then create one representative Hollowwood room with a rigged Dawnwarden, one enemy, authored lighting/materials, attack/block/dodge animations, and readable impact/telegraph effects. Review the standalone executable again before claiming visual acceptance or expanding content.
