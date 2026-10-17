# Illustrated equipment UI — 0.9.0

Date: 2026-10-17. Unity 6000.3.26f1, uGUI.

The equipment page now uses an original anime-style 2D hero and six separate item illustrations. The surrounding three slots display the currently equipped item or a clear empty-slot marker. A six-card inventory uses distinct images, short names, actual bonuses, Equipped/Owned/source-stage labels and highlighted equipped frames. The permanent navigation and existing Home/battle 3D models remain in place.

Item details show the same item's larger art, full name/type, actual effect, source stage and its difference against the currently equipped item of that slot. Positive differences are green, negative differences red. With no equipped item the current bonus is zero; an already equipped item is explicitly labelled. Unowned items can be inspected but cannot be equipped. The overlay blocks background input. No new rarity, leveling, gacha or currency system is introduced.

Art sources and exact prompts: equipment-art-notes.md and equipment-art-prompts.json. Seven original transparent PNGs were generated with the built-in image tool; reference-game artwork was not imported. Unity imports are configured through EquipmentArtImporter: single full-rect sprites, alpha preserved, bilinear filtering, no mipmaps or CPU-readable texture copies, maximum size 1024 for the hero and 512 for gear.

64 gameplay scenarios and 50 Play-mode scenarios pass. New checks cover the 2D hero with no render-texture child, six distinct item images, matching inventory/slot/detail artwork, source stages, detail text fit, actual stats after replacement, negative weapon comparisons and percentage-based support comparisons. Prior combat, save, 1000-ally and character checks continue to pass. The comparison preview uses an isolated test inventory with the advanced rifle granted for UI verification; no real player save is modified.

Browser and Android artifact checks are recorded separately. Physical Android/iPhone performance and Safari remain untested.
