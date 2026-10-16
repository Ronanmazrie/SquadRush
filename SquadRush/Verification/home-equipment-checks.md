# Scrolling ground, Home and equipment — 0.6.0

Date: 2026-10-16. Unity 6000.3.26f1. Built-in render pipeline, uGUI.

Ground had static textured cubes while scenery, gate frames and road markings moved backward. New shared top-surface geometry explicitly maps increasing UV V to increasing world Z. Road, soil and curb materials advance V by `distance * textureRepeat / 70`; features therefore move backward at the same 3.5 metres per second as props and markings. The phase wraps after 70 metres, stops outside combat and resets on a new sortie. One mesh is shared across five top surfaces; scrolling updates existing material offsets without creating per-frame arrays or materials.

Home exposes Stage, Equipment and Gacha. Stage selection offers only unlocked stages and permits replay without lowering saved progress. Pause/results include Return Home. Leaving battle clears targets/projectiles and stops simulation. Gacha has a Coming Soon page and navigation only: no draw logic, currency, consumption or purchase path.

Inventory and three equipped slots are saved as versioned JSON under `SquadRush.ResumeStage.v1.Equipment.v1`, independently of stage progression. Rewards are saved on boss defeat before leaving results; previously owned items are idempotent and never auto-equip. The shared `Acquire(id)` operation is available for future reward sources, but no gacha source calls it. Existing profiles receive equipment for already-completed stages on first migration, without changing stage progress. Verification uses isolated keys and does not persist migration to the real profile in batch mode.

| Reward stage | Item | Slot | Bonus |
|---|---|---|---|
| 1 | Scout rifle | Weapon | +1 bullet power |
| 2 | Ranger vest | Armor | +2 starting allies |
| 3 | Quick magazine | Support | +10% firing frequency |
| 4 | Assault rifle | Weapon | +2 bullet power |
| 5 | Guardian vest | Armor | +4 starting allies |
| 6 | Fast magazine | Support | +20% firing frequency |

Later stage rewards cycle over this catalog. One item per slot can be equipped; changing a weapon leaves armor/support intact. Unequip removes its bonus. Gear persists through defeat/reload and adds to the temporary supply-box bonuses; supply upgrades still reset each sortie. Equipment changes are blocked during battle.

Verification: 60 gameplay scenarios and 31 Play-mode scenarios pass. These cover terrain orientation/speed/pause/wrap/reset, Home/Stage/Gacha/Equipment navigation, locked stages and unowned gear, actual boss reward persistence, real damage/starting-ally/fire-rate bonuses, slot replacement, unequip, defeat/retry, legacy migration and reload. Active stage/equipment labels are checked for text overflow. The earlier 1000-ally, weighted-projectile, scenery and progress tests remain included.

Preview images include Home (`menu.png`), Stage, already-owned reward, Gacha, locked/earned equipment and before/after terrain scrolling. Equipment previews acquire the rifle through an automated boss defeat. Crowded scenes and reduced boss health remain synthetic verification fixtures. Browser normal-play and Android artifact checks are recorded separately. Physical Android/iPhone performance and Safari remain untested.
