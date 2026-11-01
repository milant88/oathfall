# Hollowwood Shrine art bible — vertical slice

Status: **implemented revision 5 presentation baseline; not AA accepted**. Earlier claims that this slice still uses unrigged primitive actors and has no PBR maps are superseded by the current manifests and player evidence.

## Intent

The Fallen Oath Shrine is a cold, old forest sanctuary punctuated by warm Dawnwarden metal and a small ember at the collapsed oath monument. The playable center remains clear. Trees, broken columns and the damaged entrance establish depth at the perimeter.

## Palette and material families

| Family | Use | Current status |
| --- | --- | --- |
| Cool old/moss stone | foundation, tiles, columns, monument | manifest-tracked PBR surfaces on owned modular meshes; repetition and sculpted damage still need work |
| Bark and muted leaf | perimeter trees, wagon | branching oak variants, layered leaves and understory; source winding repaired; production foliage/wind still pending |
| Blue-grey metal, brown cloth, restrained gold | Dawnwarden silhouette and gear | rigged humanoid derivatives, separate material families and six gear display models; close-up armour and wear remain provisional |
| Brown leather, dark blue cloth, limited rune glow | Raider, Hexblade, Breaker and Guardian | four distinct mesh silhouettes, common humanoid base; bespoke anatomy and role animation remain pending |

No Tolkien-specific character, heraldic, or architectural designs are used.

## Camera/readability standard

- Isometric camera: FOV 45°, nominal offset `(0, 14, -12)` from hero, 49° pitch; wheel zoom factor .84–1.16. Current implementation is TopDownCamera, not a completed Cinemachine migration.
- Dawnwarden must remain identifiable by shield silhouette and warm accent against the neutral ground.
- Raider windup uses a red floor telegraph; it supplements, never replaces, attack timing.
- Post processing: ACES, exposure +.05, contrast +12, saturation -8, Bloom .16 and Vignette .16. Warm directional key 1.55 with shadow strength .85, cool unshadowed fill .4, trilight ambient and exponential-squared fog density .018. No gameplay depth of field, motion blur or chromatic aberration.

## Acceptance still required

1. Improve material breakup, consistent wear and sculpted forms; PBR map import alone is not art acceptance. Retain source/hash records and linear roughness-to-smoothness conversion.
2. Refine hands, shoulders, armour fit, cloth and UV density on the imported/owned rigs. Current enemy LOD0 geometry is roughly 16–18K triangles, with two reduced LODs but many renderers that need consolidation.
3. Polish full-body footwork and weight transfer for the three distinct authored combo clips; add start/stop/strafe and role-specific casting. Actual sampled blade paths and rendered tips are regression-tested, not a substitute for human animation review.
4. Review captured audio by listening and complete the surface-specific/mixer pass. Finish physical-controller, minimum-spec and independent art scoring. Multi-resolution local UI review is complete; it does not certify AA quality.

Current evidence and open findings: [revision 5 review](../quality/revision5-presentation-2026-11-01.md), [materials and source policy](materials-and-owned-assets-current.md), [animation matrix](animation-matrix-current.md), [owned asset hashes](revision5-manifest-current.json). The accepted local standard is technical readability; no current model has independent AA art approval.
