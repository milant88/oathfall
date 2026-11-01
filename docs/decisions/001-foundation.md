# ADR-001: clean LTS project and data-first gameplay

Status: accepted, 2026-10-31.

Oathfall uses a new Unity `6000.3.27f2` URP project rather than downgrading either existing ARPG project. It uses local Git with LFS for binary assets, uGUI/TextMeshPro for runtime UI, and Core/GamePlay/Presentation assembly separation. Core uses stable content IDs and seeded data rather than asset references for persisted game state.

This preserves existing user projects, makes generation reproducible, and keeps future content authoring isolated from save compatibility.
