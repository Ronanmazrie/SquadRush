# Command tabs and loadout layout — 0.7.0

Date: 2026-10-16. Unity 6000.3.26f1, uGUI. Layout reference: [Survivor.io screenshots in Talk Android's illustrated guide](https://www.talkandroid.com/24372-survivor-io-ultimate-game-guide-with-tips-2023/). The reference was inspected for its permanent bottom navigation, central character/loadout slots, lower inventory grid and prominent battle action. No reference sprites, textures, characters or screenshots are included in the game.

Home now shows actual stage/inventory counts in a compact header, a chapter selector, the game's blue soldier, stage conditions and a large gold Sortie button. Three common tabs switch between Gacha, Home/Sortie and Equipment. Stage selection is accessible from the chapter banner. Gacha remains a placeholder without draws, consumption or purchases.

Equipment places the existing three slots around the central soldier. The lower inventory is a three-column, two-row icon grid. Tapping an inventory item opens its name, actual effect, acquisition stage and Equip/Remove action. Unowned items can be inspected but cannot be equipped. Tapping an equipped slot opens the corresponding item's detail. Tab highlights, slot labels, summary statistics and inventory states refresh after equipping.

The portrait is the game's original shared 3D soldier, rendered to a single 384x512 texture by an isolated layer-30 camera. The main camera excludes that layer. Rendering happens only when the permanent weapon state changes. The camera, portrait root and texture are released on HUD destruction. Equipment and navigation icons are original vector geometry rendered by uGUI CanvasRenderers.

60 gameplay scenarios and 36 Play-mode scenarios pass. Existing inventory/progress/terrain behavior remains covered. Updated UI checks cover locked-item inspection, close-without-change, Equip/Remove, equipped-slot inspection, persistent tab navigation, direct Home sortie, visible icon geometry and text overflow on stage/equipment pages. Screenshots include Home, inventory, owned/unowned item details and stage selection. Preview gear is obtained through an automated boss defeat.

Normal browser interaction and Android artifact checks are recorded separately. Physical Android/iPhone performance and Safari remain untested.
