# Normal enemy health adjustment — 0.7.1

Date: 2026-10-16. Unity 6000.3.26f1.

Normal enemies now start at 3 HP instead of 1. The existing difficulty increment remains +1 per four stages, capped at +3; the existing time increment remains +1 after 30 seconds. For example, stage one starts at 3 HP, stage five at 4 HP, and stage thirteen or later at 6 HP before time increments. Boss and supply-box tuning is unchanged.

64 gameplay scenarios and 36 Play-mode scenarios pass. The updated checks inspect the first normally spawned wave, verify increasing stage health, and use real projectile collisions with a single ally: an opening enemy survives two basic shots and dies from the third, while a power upgrade defeats it with two shots. Existing equipment, supply, 1000-ally damage, boss rewards, stage progress and menu flows remain covered.

No save format or key changed. Physical mobile device balance and performance remain untested. Browser and Android distribution checks are recorded separately.
