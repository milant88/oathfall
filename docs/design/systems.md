# Game systems

Status labels: **planned** is approved design not yet in a build; **implemented** exists in the prototype; **verified** has matching validation evidence.

## Core loop — planned

Sanctuary → choose hero and starting option → traverse a seven-node act route → choose rewards/Oaths → defeat boss → bank discoveries or return after death. A run resets gold, equipment, temporary powers, and corruption. Profile progression is capped at 15% statistical advantage.

## Heroes — Dawnwarden implemented; others planned

| Hero | Resource | Positioning | Prototype status |
| --- | --- | --- | --- |
| Dawnwarden | Resolve from blocks and committed melee | Frontline, recovery-window pressure | **implemented** basic attack/block/dodge; abilities and Resolve are planned |
| Thornstrider | Focus from accurate marked-target shots | Mobile ranged flanking | **planned** |
| Runebinder | Mana and elemental marks | Spacing and spell sequencing | **planned** |

## Combat — implemented foundation

Heroes use a `CharacterController`. Enemies own explicit approach, telegraph, attack, recover, and death behavior; the current `EnemyBrain` is the smallest prototype of that state model. Damage uses a typed request/result contract and armor mitigation. Gameplay attack windows own hit timing; animation events may enhance presentation but never exclusively define damage.

## Items — implemented foundation; catalog planned

An item instance carries a stable generated ID, deterministic seed, slot, rarity, item level, base ID, affix IDs and rolls. Common/Enchanted/Rare/Relic rarity is generated from a seeded stream. Current bases and affixes are code-defined prototype content. The planned launch catalog is 48 bases, 90 affixes, 24 relics, and six equipment slots.

## Oaths, corruption, routes, and encounters — planned

Route branches reveal reward category. Oaths offer a meaningful current-run benefit and an explicit drawback. Corruption increases encounter danger and reward quality. Rooms are selected from authored, socket-validated spaces; arbitrary terrain generation is out of scope. Each first-release act contains three ordinary fights, one elite, one event, one sanctuary, and one boss.

## Accessibility — planned requirement

Provide remapping, UI scale, subtitles, color-independent telegraphs, reduced flashes/shake, optional assists, controller navigation, and encounter-boundary suspend/resume. Validate these before Wave 7 acceptance.
