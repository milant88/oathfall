# Current materials, owned assets and sound — revision 5

Vendor originals remain separate from owned prefabs. Original Quaternius and Poly Haven acquisitions and license/hash records remain in acquisition-2026-11-01.json and woodland manifests. New audio sources are listed in audio-acquisition-revision5.json; current owned outputs are listed in revision5-manifest-current.json.

SourceArt/author_revision5.py derives Raider/Hexblade/Breaker/Guardian geometry from the existing licensed humanoid rig and authors six standalone gear meshes. It exports UVs, bone weights and two actor LODs. Source .blend files are retained. EnemyRoleAuthoring creates Unity-owned materials, controllers/avatar references and bone-remapped LOD prefabs. GearAssetAuthoring creates six display/drop prefabs and 256px icons through the Editor; GearCatalog makes them build-required. Steel/brass/leather/moonstone have separate responses. Display steel deliberately excludes the industrial metal-plate normal found unsuitable in review.

SourceArt/author_forest_detail.py authors three branching oak variants with folded leaves, grass, hazel and flowers. Leaf normals face upward; foliage uses double-sided Lit materials with restrained emission as a transmission approximation. The builder adds bank dressing without colliders and retains existing world collision/navigation. This is not a custom subsurface foliage shader, wind system or finished sculpted forest kit.

2K scan inputs, linear data maps, correctly imported normals, mipmaps and streaming remain in place. Terrain has explicit dry mask data. Scene assets, prefabs, controllers and materials are generated through the connected Unity Editor, not hand-written YAML.

Sound: selected Kenney RPG CC0 clips (author's OpenGameArt mirror) provide swing/contact/step/loot/UI; Thimras CC0 Park ambiences provides the forest excerpt. Original source and licenses are retained. SourceArt/author_soundscape.py creates the original modal theme and rune/ward/victory cues. Long loops stream compressed; short cues decompress on load. Soundscape owns music/ambient sources and eight event voices. Actor sources present gameplay signals, never cause damage. Master/Music/Ambience/SFX controls persist via PresentationSettings. Full AudioMixer routing, professionally recorded weapon layers, surface-specific footsteps and subjective mix approval remain planned.

Game version/engine/packages remain unchanged by this presentation task. Content revision 5 identifies this art/binding/UI snapshot; it is not a new save schema or a claim of production acceptance.
