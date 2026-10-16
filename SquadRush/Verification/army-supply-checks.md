# Large squad, weapon supplies and weathered scenery — 0.5.0

Date: 2026-10-16. Unity 6000.3.26f1.

The logical squad limit is 1000. A maximum of 96 animated representatives stays inside the road and above the control hint. Each volley distributes all logical members across at most 40 tracers using integer partitioning; the sum of their damage equals squad size times weapon power, including non-divisible counts. Projectile collision applies that damage. The pool remains 384 projectiles, including the fastest weapon. Contact removes one logical ally (three for crates), independently of visual group size.

Supplies rotate through Power, Allies and Rapid Fire. Colored lids/stripes and Japanese labels identify rewards before opening. Power increases damage by one, up to six; rapid fire increases firing frequency by 16% per level, up to 1.8x. Each upgrade has five steps and applies to the whole squad. Excess upgrades grant allies instead. Recruitment grants max(4, ceil(squad / 10)), clamped at 1000. Weapons reset for retries and new stages; saved stage progression is unchanged. HUD shows power and fire rate; upgraded rifles have a gold rail and cyan muzzle attachment.

Props share a cached 128x128 texture containing broad and fine color variation. The shader samples this in object space, darkens bases slightly and reduces specular gloss for scenery and wooden crates. Surface marks stay attached during scrolling. Soldiers retain their original shading. Sandstone strata, bark ribs, log growth rings and moss patches add material cues; ground and curbs use a shared mipmapped mottling texture. Theme generation, cached meshes, fixed scenery slots and deterministic seeds are retained.

Automated verification covers 37 gameplay and 18 Play-mode scenarios, including 999/1000 changes, multiplication cap, full 1000-member damage, actual weighted hits, high-count collisions, maximum weapon power with 1000 allies, sustained fastest fire with bounded pools, acquisition/caps/reset of both upgrades, unopened crate contact, unclipped 1000-member and weapon HUD, visible rifle upgrades, stage transitions, persistence and scenery recycling. Player progress uses isolated temporary keys during tests.

Preview captures are synthetic verification scenes. `crowd.png` sets the logical count to 1000 and spawns 72 durable enemies; 96 blue representatives are rendered. `weapons.png` acquires both upgrades via shooting before displaying two sample weapon crates. Normal stage balance is checked separately in the browser. Physical Android/iPhone performance and Safari remain untested.
