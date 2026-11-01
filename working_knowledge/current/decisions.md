# Decisions

| Decision | Status | Rationale | Source |
| --- | --- | --- | --- |
| Use a clean Unity 6.3 LTS sibling project | confirmed | Existing projects use a newer Editor or unrelated starter content; downgrading would be unsafe. | ADR-001 |
| Use URP and uGUI/TextMeshPro | confirmed | Verified Universal 3D template includes compatible packages and supports the planned PC visual baseline. | ADR-001 |
| Build Dawnwarden first | confirmed | Establishes melee, defense, damage, enemy reaction, and loot feedback before ranged/magic complexity. | Product vision |
| Previous melee-entry staggering implementation | superseded | NavMesh Raider now staggers initial attack deadlines and locks facing during visible windup; do not treat older controller behavior as current. | EnemyBrain.cs |
| Separate App composition assembly | confirmed | Gameplay references Core; App connects Gameplay/Presentation without a cycle. | ADR-005 |
| Only implemented affixes may roll | confirmed | Health, armor, damage and move speed have real effects; planned relic IDs remain reserved in source. | ItemGenerator.cs |
| Authored asset/scene references | confirmed | Model, controller and material presence are build requirements; no primitive fallback. | ExpeditionBuildValidator.cs |
| Keep visual acceptance open | confirmed | Rigged assets and successful builds do not prove final art, 10–15-minute pacing, minimum-spec performance or independent approval. | Current quality report |

| Persist VolumeComponent sub-assets | confirmed | Transient profile overrides disappeared on reload; regression and build gate now enforce persistence. | WoodlandAtmosphereAuthoring / LightingPersistenceTests |
| Renderer-only camera cutaways | confirmed | Standalone arch and canopy occlusion hid the hero; static foliage uses cached render bounds beyond narrow trunk colliders; visibility changes must not alter collision or navigation. Hard cutaway is temporary art implementation. | CameraOccluder / PlayMode regression |
| Bound current trend-inspired scope | confirmed | Improve world coherence/readability and combat commitment; do not infer demand for multiplayer/live services or claim a sales ranking. | Presentation quality/research report |

| Bake blade samples from the actual retargeted actor | confirmed | Shared normalized time removes the independent oversized hit arc; rebake after model/clip changes. | ADR-006 |
| Active ward defense and mixed staged encounters | confirmed | Extend the expedition through combat, traversal and resource choices; measured full-run timing is still required. | ExpeditionContent / current quality report |
| Align navigation with the player collider | confirmed | Graph connectivity alone allowed a physical rock stall; lower climb and exclude boulder surfaces, retain collider regression. | ADR-006 |

| Require grounded, clear and connected enemy spawns | confirmed | Complete route-center paths did not prevent enemies on inaccessible rocks. Validate floor height, capsule clearance and path to nearby player; modifier root layer must match bake mask. | CombatSpatial / ADR-006 |
| Keep synthetic pacing separate from human acceptance | confirmed | Final run is 707.7 active seconds; automated gear decisions and known navigation differ from a first-time player. | Current quality report |

| Three owned humanoid combo curves | confirmed, revision 5 | Stock B/C contact candidates failed; authored lateral/reverse/overhead paths preserve gameplay timing and baked contact. Not claimed as mocap. | ComboMotionAuthoring / revision 5 report |
| Inspect first; explicit equip | confirmed | Item selection and the 3D preview are read-only; buttons perform mutation and refresh current indices/focus. | RuntimeHud.Equipment / EquipmentStats.PreviewSwap |
| Shared six-slot display catalog | confirmed for current slice | Reuse owned meshes for icons, drops and studio; a full unique item-art catalog and worn visual swaps remain planned. | GearCatalog / GearAssetAuthoring |
| Keep recorded sources/license hashes | confirmed | CC0 Kenney/Thimras audio, owned musical/event cues and import settings remain reproducible. | audio-acquisition-revision5.json |
| Clear ended hit overrides | confirmed | A black property block erased material rune emission; removing the override restores authored material response. | ActorPresentationController |
| Project spawn path endpoint within bounded clearance | confirmed | CharacterController can stand 0.901 m outside agent clearance; sample target within 1.5 m and 0.5 m height before complete-path validation. Keep floor/capsule checks and reject far endpoints. | CombatSpatial / spawn red-green evidence |
| Pulse audit healing input | confirmed | Real gameplay uses press edges. Release between synthetic requests so repeated available draughts work without changing health, cooldowns or difficulty. | RuntimeAuditDriver / post-repair audit |

| Initial source and evidence commits on main | confirmed by user | No previous HEAD exists; commit current verified production foundation, then final evidence/working knowledge. Push to user-supplied empty origin with provenance notes; no force or fabricated prior history. | Closeout request / plan 1.1–1.3 |
