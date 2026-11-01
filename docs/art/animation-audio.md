# Art, animation, and audio matrix

## Style target — planned

Stylized realism: grounded medieval-fantasy materials, warm/cool regional contrast, readable hero and enemy silhouettes, and atmospheric but gameplay-safe lighting. Prototype primitives are **implemented only for validation**, never an art-quality claim.

## Animation matrix — implemented baseline / planned production

| Actor | Required clips | Gate |
| --- | --- | --- |
| Dawnwarden | locomotion, attack chain, block, hit, stagger, dodge, death | Weapon alignment, foot placement, clear recovery |
| Thornstrider | locomotion, draw/release, roll, trap, hit, death | Aim readability and projectile timing |
| Runebinder | locomotion, cast, blink, hit, death | Spell silhouettes and cancel readability |
| Enemies | idle, acquire, approach, telegraph, attack, recover, stagger, death | Telegraph precedes damage and is visible from isometric camera |

The current vertical slice implements presentation phase calls for Dawnwarden and Raider (`Idle`, move, windup, active, recover, block, dodge, hit and death) and ships controller shells. It does **not** yet ship imported/retargeted authored clips; its transform-driven proxy posing is not production-animation acceptance.

## Audio priorities — planned

Prioritize attack commitment, block, dodge, hit confirmation, enemy telegraph, elite/boss cue, loot rarity, and sanctuary safety. Route sources through named mixer groups and test at low volume with subtitles enabled.
