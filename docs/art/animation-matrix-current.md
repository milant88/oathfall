# Hollowwood animation matrix — revision 5

Source locomotion/defense/reactions: Quaternius Universal Animation Library Standard 1 + 2 (CC0), original hashes in acquisition-2026-11-01.json. Model derivatives retain Humanoid rigs; gameplay owns root translation, with Animator on the visual child.

| State | Dawnwarden | Enemy roles | Timing owner |
|---|---|---|---|
| Idle | Sword_Idle | Sword_Idle | Gameplay state |
| Move | Sword_Idle / Walk_Loop / Jog_Fwd_Loop | same shared tree | Actual speed and travel-facing model |
| Attack | Dawnwarden_Combo1, lateral cut | Sword_A_Complete | Gameplay normalized action time |
| Attack2 | Dawnwarden_Combo2, reverse cut | Sword_B_Complete | Gameplay normalized action time |
| Attack3 | Dawnwarden_Combo3, overhead | Sword_Regular_C | Gameplay normalized action time |
| Bash | Shield_OneShot | shared binding, not a player ability | 5-second cooldown/stagger |
| Cleave | Sword_Attack | shared binding | 7-second cooldown, 1.8x sampled contact |
| Block | Idle_Shield_Loop | shared binding | Incoming-damage rule |
| Dodge | Roll | shared binding | .42-second action; first .25 protected |
| Hit / Stagger | Hit_Chest | Hit_Chest | Gameplay interruption |
| Death | Death01 | Death01 | Death state and delayed cleanup |

ComboMotionAuthoring writes three distinct humanoid clips with analytical arm IK, wrist/blade orientation and restrained chest rotation over the source stance. Rejected stock B/C candidates failed the forward-height check. These new curves are authored motion, not new mocap, and their body weight/footwork remains an art limitation.

WeaponMotionAuthoring resolves actual Animator state clips and bakes 121 actor-local grip/tip samples. It accounts for the imported 100x transform and 180-degree forward correction. PlayMode checks all three hero clips against the rendered hand-bound tip within 8 cm, forward contact and different paths. Animator never registers damage. Shared normalized progress prevents frame drops from skipping active intervals.

Rebuild order: ExpeditionAssets.BuildActors -> EnemyRoleAuthoring.Build -> WeaponMotionAuthoring.Bake -> AudioAssetAuthoring.Build. The last step restores the recorded audio references that baseline actor generation would replace. Do not rebake only the hero after changing enemy scale or weapon geometry.

Still planned: authored start/stop/turn and strafing, polished guard break, role-specific casting, full-body counterweight/footwork and close-up deformation approval. Common locomotion is intentionally retained; distinct meshes do not imply distinct animation sets.
