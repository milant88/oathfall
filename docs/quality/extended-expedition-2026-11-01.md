# Hollowwood extended expedition — content revision 4

Status: revision 4 local implementation and gameplay gate complete, 2026-11-01. This report supersedes revision 3 only where verified below. No AA acceptance, physical-controller or minimum-spec certification is implied.

## Implemented changes

- Blade trajectories sampled from the authored retargeted model in the connected Editor. Gameplay uses actor-local grip/tip samples with narrow contact capsules and interval substeps. Presentation receives the same normalized action time. Blade-tip trail uses the actual hand-local tip. Imported transform scale and model forward orientation are handled explicitly.
- First swing combines attack and its recovery; the other two combo entries currently reuse the stable single-swing clip with different timing. This is an honest limitation, not three new bespoke animations. The previous downward finisher was rejected. Bash retains shield-specific proximity contact.
- Twelve connected stops extend the route from z=-43 to z=480. Six multi-group skirmishes, three ward defenses (75/90/105 seconds of active occupancy), two sanctuaries and a final Fallen Oathkeeper. Ward progress stops outside its boundary; combat continues and all remaining threats must be cleared.
- Raider melee, Hexblade targeted ground spells, Breaker close ground strikes and a guardian with additional patterns below 55% health. Signals appear before damage; death/stagger cancels pending spells. Roles share the approved humanoid rig, with owned palette/scale variants, and are not final bespoke enemy art.
- Three restorative draughts (65 health, 8-second reuse limit), checkpoint replenishment, sanctuary recovery, reduced drop frequency and explicit inventory disposal. No automatic equipping for normal players.
- Shared muted forest/masonry/armor palette, removed spike-like roots, explicit dry terrain mask, extended forest dressing and authored stop compositions. Player-camera review shows a more consistent palette and dry ground, but visible repeated humanoids, simple foliage silhouettes and modular masonry remain below the final production-art gate.

## Validation

Final automated gate: **17/17 EditMode + 25/25 PlayMode, 42 passed, zero failed/skipped**. Includes blade-tip agreement within 8 cm, phantom-reach rejection, low-frame hits/interruption, death/result/restart, ward progression, stale inventory selection, whole-route physical traversal and floor-accessible enemy spawns.

Development build `build_bef0efdc9af6` and ordinary build `build_51da7791f0da` both succeeded with zero build errors/warnings. [Build manifest](../evidence/2026-11-01-expedition/build-manifest.json), [EditMode results](../evidence/2026-11-01-expedition/editmode-results.json), [PlayMode results](../evidence/2026-11-01-expedition/playmode-results.json).

Final standalone audit completed all twelve stops and the north exit in **707.7 active seconds (11:47), 232 enemies defeated, no deaths**. Log ends with `EXPEDITION result=victory`, `AUDIT ended: victory` and successful capture completion; no exception/error entries were found. Earlier three interrupted audits are retained and excluded from this completion claim. Four resolution layouts inspected: 1280x720, 1920x1080, 2560x1440 and 3440x1440. Native mouse inventory exchange/discard and pause were checked separately from automated combat. The captured full-frame ultrawide image, rather than a cropped desktop window, confirms both HUD margins.

## Evidence interpretation

The development audit uses synthetic Gamepad movement/combat and direct selection/equipment APIs, with no damage/health/position overrides. Its elapsed gameplay time is measured, not treated as a human first-play session. A 600–900 second target must be supported by a completed run; real player pacing still needs external playtests. Capture readback invalidates performance measurements.

## Remaining production gates

Independent visual scoring, minimum-spec CPU/GPU/VRAM, physical controller, authored strafing/start/stop/turns, three distinct polished combo clips, bespoke enemy silhouettes and full production audio remain open. Timed ward defenses are combat objectives, not evidence of content variety by themselves; repeated compositions must be assessed during play.

## Review findings during first player run

- Native mouse equipment exchange increased damage to 42.9 and max health to 175; discard removed the replaced bag item. Recorded screenshot. Follow-up fix clears stale discard selection after inventory mutation and names the selected slot on the discard button; regression added.
- Ward boundary looked like a hostile spell circle: changed to a thin gold ring with eight inward notches, while hostile pulse circles retain thick orange/red lines.
- Audit pursued old drops across already-completed areas, causing unrepresentative backtracking. Limited automated pickup pursuit to nearby items (10 m); this changes only the test driver, not normal player loot rules. Repeat timing after this repair.

- First extended run stalled outside Whispering Grove at a rock. Navigation allowed 0.75 m climbing while the player allows 0.35 m. Rebuilt with 0.3 m climb, 0.6 m clearance and non-walkable boulders; added actual collider traversal regression at the observed coordinates. The interrupted run is retained as iteration evidence, not a completion or timing pass. Unity defines this parameter as the [maximum vertical step](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/ai/navmeshbuildsettings/agentclimb).

- Second interrupted audit exposed a driver-only threshold mismatch: corner advancement required 0.15 m while movement stopped at 0.4 m. Movement now continues to 0.1 m; actual player-collider traversal across every route segment passes.
- Third interrupted audit found enemies on boulders: the modifier root's layer 0 excluded it from the world-only bake, despite child colliders using layer 8. Set modifier roots to layer 8. Spawn selection now rejects elevated surfaces, obstructed capsules and incomplete paths to the player; regression samples all combat stops and representative wave positions. This is a separate spawn validation gap beyond route-center connectivity.
- Raider warning now projects actual blade samples; focused contact regression passes afterward. Full-frame 2560×1440 and 3440×1440 HUD views and ordinary 720p menu/pause/gameplay inspected without clipping. Capture images are in the evidence folder. No minimum-spec performance conclusion follows from these views.


## Final evidence and review

- [60-second gameplay excerpt](../evidence/2026-11-01-expedition/gameplay-60s.mp4), [complete expedition recording](../evidence/2026-11-01-expedition/full-expedition.mp4), [victory screen](../evidence/2026-11-01-expedition/victory-1080.jpg), [input trace](../evidence/2026-11-01-expedition/input-audit.csv), [player log](../evidence/2026-11-01-expedition/player-log.txt).
- Both videos originate from uninterrupted **approximately 5 Hz framebuffer capture, without audio**. The short excerpt is encoded at 30 fps using repeated frames; it is not a 30/60 fps animation or performance measurement. Full recording preserves capture timing. Live player observations and contact regression supplement the low-rate video.
- Reviewed combat contact, terrain/foliage consistency, sanctuary traversal, last gauntlet, guardian and result in the actual player. No pink materials, T-pose, missing actor, HUD clipping or progression blocker observed in the final run. Earlier render/layout frames and native inventory evidence remain in this folder.
- Scope review: damage remains Gameplay-owned, Core has no scene/UI dependency, source assets remain separate from owned derivatives, Editor-authored references remain build validated. No engine/package upgrades, paid acquisitions, commits or unrelated repository changes.
- Visual limitations are material: simple tree canopies, repetitive road/ruin compositions, shared humanoid enemies, duplicate combo clips and basic UI/audio. Ward objectives occupy 270 seconds of the target duration; human tests must assess repetition and challenge. The guardian was defeated without exhausting recovery resources, so difficulty tuning remains open.
- Ten-run soak was **not repeated for revision 4**. Earlier revision soak results cannot certify this longer expedition. Capture data is unsuitable for CPU/GPU/memory budgeting; no minimum-spec performance claim is made.

- Final ordinary release smoke: launched the final release binary at 1920x1080, selected Dawnwarden with a native mouse click and verified fresh stage 1/12, 150/150 health and three draughts. Left open at Forest Approach for the user. [Final release frame](../evidence/2026-11-01-expedition/release-final-1080.jpg). Full recording duration is 708.32 seconds; active gameplay result is 707.7 seconds. [Full-route review frames](../evidence/2026-11-01-expedition/full-run-review.jpg).
