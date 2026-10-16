# Graphics verification — 0.3.0

Date: 2026-10-16. Unity 6000.3.26f1, Built-in pipeline.

This records the 0.3.0 graphics work. For 0.4.0 progression and the updated shared Preview captures, see `stage-checks.md`.

Reference ads were viewed in the browser:
- https://www.youtube.com/shorts/RPpcQ-B43zw
- https://www.youtube.com/shorts/Q2_2he4NHvA

Changes inspected:
- Thin silhouette outlines and stronger warm/cool lighting. Helmet highlights, shoulder patches, backpack straps, knee pads, rifle rails and boss shoulder armor.
- Directional shadows cast by the actual meshes plus the existing contact shadow. One 2048 px shadow map, no cascades, 55 world-unit shadow distance, 2× MSAA.
- Shared meshes for cactus plants, layered sandstone, streetlights and yellow safety markings. Props follow the existing road loop and add no collisions or rewards.
- Mipmapped asphalt grain replaces the diagonal pattern. Distant haze adds separation.
- Gate reflections and indicator lights; cyan supply marks on crates. A saved TMP outline material preserves the numeral outline variant in player builds.
- Camera tilt changes between 30 and 60 allies to keep the last row above the control hint. Gameplay positions and collision rules remain unchanged.
- Sphere meshes use 221 vertices. Models and materials are shared; actual device performance is unmeasured.

Shadow support follows [Unity's Built-in shader example](https://docs.unity3d.com/ja/current/Manual/built-in-shader-examples-receive-shadows.html), with ForwardBase shadow macros and an instanced ShadowCaster pass.

Visual inspection:
- Unity renders at 540 × 960: menu, combat with 22 allies, crate number 22, plus/minus gates, tracers, armored boss, and a crowded scene. PNGs in `Preview/` are Editor captures; the foreground crate and boss are spawned for inspection. `crowd.png` is a synthetic stress scene, not normal wave density.
- Captures wait for Unity's deferred destruction before rendering so removed targets no longer remain visible.
- 11 simulation and 11 Play-mode scenarios pass, including 60 allies, 72 enemies and 60 bullets. The Editor logged its existing Unity Search indexing exception outside game code; verification completed with exit code 0.

WebGL and Android results are recorded in `web-checks.txt` and `android-checks.txt`. Physical Android/iPhone devices and Safari remain untested.
