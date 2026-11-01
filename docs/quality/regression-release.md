# Regression and release checks

## Per-change

Compile; run the narrowest relevant EditMode or PlayMode tests; invoke content validation when definitions change; inspect new Console errors; review contract boundaries and cleanup paths.

## High-risk regression list

- Damage stacking, mitigation, caps, healing, and repeated collision suppression.
- Seeded item equality, affix eligibility, roll bounds, and proc recursion limits.
- Enemy interruption, unreachable navigation, simultaneous deaths, and pooled-object reuse.
- Route sockets, spawn safety, boss progression, and reward idempotency.
- Atomic saves, corrupt data recovery, migrations, focus loss, and encounter-boundary resume.
- Controller-only navigation, visual telegraph contrast, UI scaling, and reduced-flash mode.

## Release gate — planned

Clean import and Windows build; full automated suite; migration fixtures; two-hour soak; offline/controller-only run; performance capture on the stated minimum-spec hardware; license/credit inventory; and external beta evidence.
