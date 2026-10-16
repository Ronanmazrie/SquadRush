# Procedural scenery and stage progression — 0.4.0

Date: 2026-10-16. Unity 6000.3.26f1.

The fixed scenery loop is replaced with 48 reusable slots. Each attempt selects new props, lateral positions, rotations and scales from an independent random stream. Slots leaving view are rerolled rather than repeating the same layout. Shared meshes are cached by theme and prop kind; restarting does not accumulate scene objects.

Stages cycle through desert, forest and snow. Stage one has a 40-second build-up before the boss; duration increases to a maximum of 58 seconds. Boss health, wave frequency and initial squad size increase gradually, with difficulty capped after stage 21. Defeating the boss shows Next; losing retries the same stage with a new seed.

Progress is the next stage, stored under `SquadRush.ResumeStage.v1` with PlayerPrefs.Save at clear time. Saving happens before the Next button, so closing at the clear menu resumes at the next stage. A lower/replayed stage cannot reduce the saved high-water mark. [Unity documents browser PlayerPrefs storage as IndexedDB](https://docs.unity3d.com/ja/current/ScriptReference/PlayerPrefs.html).

Verification:

- 21 gameplay scenarios pass: original controls/collisions/rewards plus clear/save/advance, retry without advancing, progress reload, reproducible debug seeds, distinct layouts, recycling without object growth, pause, increasing difficulty and snow-stage settings.
- 15 Play-mode scenarios pass: actual Next button and saved-menu transition, 60 allies/72 enemies, keyboard/touch/pause/retry, progress bar and Japanese font coverage.
- Verification uses unique temporary PlayerPrefs keys, removed afterward. It never clears the player's real progress.
- Preview images include desert combat, forest, snow and the clear menu. The boss health is reduced and additional allies are added for automated menu verification; `crowd.png` is a synthetic stress scene.
- Existing Unity Search indexing exception is confined to Editor startup; verification exits successfully.
- Browser normal play cleared stages one and two, advanced to snow, and survived a page reload at stage three. The template enables autoSyncPersistentDataPath to remove Unity's deprecated manual-sync warning and retain progress.

Browser and APK checks are recorded separately. Physical mobile devices remain untested.
