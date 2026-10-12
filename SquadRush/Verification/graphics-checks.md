# Graphics verification — 0.2.0

Date: 2026-10-12. Unity 6000.3.26f1, Built-in pipeline.

Reference ads were viewed in the browser:
- https://www.youtube.com/shorts/RPpcQ-B43zw
- https://www.youtube.com/shorts/Q2_2he4NHvA

Original procedural geometry replaces the primitive soldier silhouette with a helmet, face/visor, vest, backpack, arms, gloves, boots and rifle. Body/leg meshes and materials are shared across soldiers; animation, flash and lean are simulation-driven. Bosses use a distinct armored mesh. Crates include separate planks, straps and rivets. Gate frames include transparent panels and floor glow. The camera uses perspective while keeping the road's horizontal framing consistent across portrait sizes.

Visual inspection:
- Unity renders at 540 × 960: menu, combat with 22 allies, supply-crate number 22, plus/minus gates, tracers and armored boss. PNGs in `Preview/` are deterministic Editor captures; the foreground crate and boss are spawned for inspection.
- WebGL renders in Chromium at 390 × 844: original meshes, shading, translucent gates, contact shadows and tracers appear; no missing/pink shaders. Japanese menu, pause, defeat and retry layouts remain usable.
- Start, pointer drag, automatic fire, Pause/Continue and Retry checked in the Web build. Console warning/error entries: 0.
- Existing 11 simulation and 10 Play-mode scenarios pass. The batch Editor logged a Unity Search indexing exception outside game code; Play-mode verification still completed successfully with exit code 0.

Android APK signature, manifest, version code and alignment checked separately. Physical Android/iPhone devices and Safari performance remain untested.
