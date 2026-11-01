# Asset manifest

Latest woodland additions and current source hashes: [woodland-manifest-current.json](woodland-manifest-current.json). Earlier imported/owned inventory and hashes: [acquisition-2026-11-01.json](acquisition-2026-11-01.json). Current implementation details: [materials and owned sources](materials-and-owned-assets-current.md), [animation bindings](animation-matrix-current.md). Entries below describe the earlier foundation and are not the complete current inventory.

Status: **current vertical-slice inventory**. This manifest is intentionally small; it does not claim final production art.

| Asset group | Local path | Source/license | Modifications/use | Status |
| --- | --- | --- | --- | --- |
| Hollowwood material palette | `Assets/Oathfall/Presentation/Generated/Materials/` | Oathfall-owned procedural URP Lit/Unlit material assets created by `OathfallVisualAssetSeeder` | calibrated base color, metallic, smoothness, selected emission; no external texture maps | prototype only |
| Cobblestone 01 — base color, normal DX, roughness (2K) | `Assets/Oathfall/ThirdParty/PolyHaven/Cobblestone01_2K/` | [Poly Haven Cobblestone 01](https://polyhaven.com/a/cobblestone_01), CC0; acquired 2026-11-01 | `diff` and `nor_dx` are assigned to the shrine floor/old stone/moss stone URP Lit materials; normal map imported as a Normal Map, 2× tiling, mipmaps, anisotropic level 8 and HQ Windows compression. Roughness is retained as source evidence pending proper smoothness-channel packing. | implemented texture proof; not final art |
| Actor presentation definitions | `Assets/Oathfall/Presentation/Generated/Definitions/` | Oathfall-owned | stable presentation IDs and explicit material/controller references | implemented |
| Generic Animator Controllers | `Assets/Oathfall/Presentation/Generated/Animation/` | Oathfall-owned | transition-ready controller shells; motion is currently presentation-code driven | prototype only |
| Hollowwood Shrine scene composition | `Assets/Scenes/SampleScene.unity` | Oathfall-owned | saved through Unity Editor from `OathfallVisualAssetSeeder.BuildHollowwoodShrine` | implemented baseline |

Poly Haven API MD5 values verified after acquisition: `diff` `50fecfc91458bd2eaffd6be223b118ec`; `nor_dx` `056109a8e2f4b54e0948b4e8358afaf9`; `rough` `615ee5ccb7c6a5568c4d91e08e0c386a`. Before importing further Poly Haven, ambientCG, Quaternius or Mixamo content, record the exact source URL, declared license, acquisition date, version/hash, import preset, model/texture modifications and reviewer. A successful import alone does not make an asset production-ready.

Revision 5 current outputs: [owned sources and hashes](revision5-manifest-current.json), [audio acquisition and hashes](audio-acquisition-revision5.json). The new gear/roles/foliage supersede the old prototype-only entries where applicable; production visual acceptance remains open.
