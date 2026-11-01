# Oathfall

Oathfall is a single-player, offline, 3D isometric action roguelite for Windows. The long-term design includes Dawnwarden, Thornstrider and Runebinder, branching expeditions and persistent discoveries; the playable scope is listed below.

## Current status

The current Hollowwood expedition supports **Dawnwarden only**: twelve connected stops, mixed enemy roles, ward defenses, two sanctuaries and a final guardian. Three distinct authored Dawnwarden combo clips supply sampled sword contact; four enemy mesh variants and six gear display/drop models are implemented. Drops, the 24-slot inventory, six equipment slots and restorative draughts have gameplay effects. **This is not a finished AA game.** Full-run timing and validation are recorded in the [current expedition review](docs/quality/revision5-presentation-2026-11-01.md); earlier reports are historical snapshots.

## Quick start

1. Open this folder with Unity `6000.3.27f2`.
2. Open `Assets/Scenes/Hollowwood.unity`.
3. Enter Play mode and select Dawnwarden.
4. Use WASD or the left stick to move, left mouse/right trigger to attack, right mouse/left trigger to block, and Space/South button to dodge.
5. Q/LB: Shield Bash. E/RB: Cleaving Arc. I/Y: equipment; click/A inspects a bag item, then use EQUIP SELECTED. Select a worn slot to unequip. Drag/scroll the 3D preview to rotate/zoom; the selected bag item can be discarded. F/D-pad Up uses a restorative draught. Esc/Start pauses and opens UI scale, flash, shake and four audio-volume settings.
   Mouse wheel adjusts camera distance during gameplay; scrolling inventory/settings does not change the camera.
6. Clear all twelve stops, collect equipment and reach the northern waystone. The side path has a cache; the camp shortcut opens after its encounter.

Owned Blender sources live in `SourceArt/`; vendor originals remain in `Assets/Oathfall/ThirdParty/`. The App assembly connects Core, Gameplay and Presentation. Engine/package upgrades are outside this presentation revision.

Read [docs/INDEX.md](docs/INDEX.md) before changing gameplay systems. Current task state is in [working_knowledge/current/status.md](working_knowledge/current/status.md).

Current Windows player: `Builds/Oathfall-Hollowwood/Oathfall.exe`. The development audit build is separate at `Builds/Oathfall-Revision5-Development/Oathfall.exe`. [Build/source manifest](docs/evidence/2026-11-01-revision5/build-manifest.json) and [current presentation review](docs/quality/revision5-presentation-2026-11-01.md).

