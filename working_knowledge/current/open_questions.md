# Open questions

## Blocking

No blocking questions. User supplied the remote; project/evidence and provenance notes were pushed and verified.

## Non-blocking

- Independent AA scoring, physical-controller play and minimum-spec CPU/GPU/VRAM validation remain open.
- The new enemy geometry retains a shared humanoid base and most animation states. Full anatomy/cloth/deformation, casting and true strafe/start/stop need art work.
- Three combo clips are now distinct; full-body weight transfer and contact polish still need human animation review.
- One display model per active gear slot/base family is implemented. Worn-character gear swaps and a broader weapon/base catalog are planned.
- Audio output is captured and level-checked; subjective listening, mastering and surface-specific steps remain open.
- Native helper-injected I/Escape did not visibly toggle the player UI. Synthetic Input System tests pass, native mouse buttons work; physical-keyboard verification remains separate.
- Final post-repair synthetic victory: 630.5 seconds. Human first-play timing, balance and ten-run soak are separate gates.

## Resolved

- Target D:/Projects/Unity/Oathfall only, existing engine/packages, no purchases. Commits/push are now explicitly authorized.
- Distinct enemy meshes, three authored swings, six gear models, recorded audio and native 3D equipment inspection are implemented; older duplicate-swing/silent-audio statements describe revision 4.
- Leaf winding, steel preview overexposure/industrial normal, hidden material emission and modal HUD clutter were found and repaired during review.
- Off-mesh player positions no longer falsely reject every enemy spawn: bounded endpoint projection passes the observed regression and full PlayMode gate.
- The audit must release between draught button presses. Continuous hold lost repeated healing requests; the corrected driver is development-only and does not alter gameplay rules.
