# Quality strategy

Core rules require EditMode tests for deterministic generation, affix eligibility, value bounds, damage mitigation, and bounded proc chains. Gameplay state changes require focused PlayMode tests for hit registration, interruption, death cleanup, pickups, and save boundaries.

Every wave also requires an Editor console review and an actual Play-mode verification. Release gates add clean import/build, save migration fixtures, soak testing, controller-only runs, and representative performance captures.
