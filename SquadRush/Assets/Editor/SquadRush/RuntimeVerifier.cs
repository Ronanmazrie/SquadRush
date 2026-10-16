using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SquadRush.Editor
{
    [InitializeOnLoad]
    public static class RuntimeVerifier
    {
        const string Pending = "SquadRush.RuntimeVerification.Pending";
        static RuntimeVerifier() { EditorApplication.playModeStateChanged += PlayStateChanged; }
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Runtime verification runs in batch mode.");
            PrototypeBuilder.PrepareVisuals();
            EditorSceneManager.OpenScene("Assets/Scenes/SquadRush.unity");
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void PlayStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
            var game = UnityEngine.Object.FindFirstObjectByType<SquadRushGame>();
            if (game == null) { Debug.LogError("Runtime game was not created."); EditorApplication.Exit(1); return; }
            game.enabled = false;
            game.StartCoroutine(Guard(CheckRuntime(game)));
        }
        static IEnumerator Guard(IEnumerator steps)
        {
            while (true)
            {
                bool more = false, failed = false;
                object next = null;
                try { more = steps.MoveNext(); if (more) next = steps.Current; }
                catch (Exception error) { failed = true; Debug.LogException(error); }
                if (failed || !more) { EditorApplication.Exit(failed ? 1 : 0); yield break; }
                yield return next;
            }
        }
        static IEnumerator CheckRuntime(SquadRushGame game)
        {
            for (int i = 0; i < 5; i++) yield return null;
            Touchscreen touch = null;
            string storageKey = "SquadRush.RuntimeVerification." + Guid.NewGuid();
            try
            {
                game.ConfigureVerification(storageKey);
                var checks = new System.Collections.Generic.List<string>();
                Require(game.State == RunState.Ready && game.SquadCount == 3, "Scene starts with a three-ally squad and menu", checks);
                var play = game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "Play");
                Capture(game, "menu.png");
                Require(game.Hud.GetComponentsInChildren<SquadRushUiIcon>().All(icon => icon.canvasRenderer != null && icon.GeneratedVertices > 0), "Visible navigation icons have renderers and generated geometry", checks);
                Require(game.Hud.CurrentPage == FrontEndPage.Home && !play.gameObject.activeInHierarchy, "Home replaces the old opening dialog", checks);
                Require(Button(game, "HomeSortie").gameObject.activeInHierarchy && Button(game, "HomeCommand").gameObject.activeInHierarchy && Button(game, "HomeEquipment").gameObject.activeInHierarchy, "Home exposes a direct sortie and persistent navigation", checks);
                Click(game, "HomeGacha");
                Require(game.Hud.CurrentPage == FrontEndPage.Gacha && game.Equipment.OwnedCount == 0, "Gacha navigation shows a placeholder without granting items", checks);
                Capture(game, "gacha.png");
                Click(game, "GachaEquipment");
                Click(game, "InventoryItem0"); Require(game.Hud.CurrentPage == FrontEndPage.Equipment && !Button(game, "EquipmentAction").interactable, "Equipment screen prevents equipping unearned items", checks);
                Capture(game, "equipment-locked.png"); Click(game, "CloseEquipmentDetail");
                Require(game.Hud.CurrentPage == FrontEndPage.Equipment && !Button(game, "EquipmentAction").gameObject.activeInHierarchy, "Closing an item inspection returns to inventory without changing gear", checks);
                Click(game, "HomeCommand"); Click(game, "HomeStages");
                Require(game.Hud.CurrentPage == FrontEndPage.Stages && !Button(game, "NextStage").interactable, "Home opens stage selection with locked progression", checks);
                Capture(game, "stages.png");
                Click(game, "StageStart"); Require(game.State == RunState.Playing && game.Hud.CurrentPage == FrontEndPage.Hidden, "Stage sortie button starts combat and hides Home", checks);
                Capture(game, "ground-before.png");
                game.Tick(0.25f, false);
                Require(Mathf.Abs(game.GroundTravel - 0.875f) < 0.001f && game.transform.Find("World/SoilSurface").GetComponent<Renderer>().sharedMaterial.mainTextureOffset.y > 0, "Play-mode ground textures advance with terrain distance", checks);
                Capture(game, "ground-after.png");
                Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.RightArrow)); InputSystem.Update();
                game.Tick(0.05f, true);
                Require(game.SquadX > 0, "Keyboard input moves the squad", checks);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                game.Begin();
                touch = InputSystem.AddDevice<Touchscreen>();
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = new Vector2(100, 400) });
                InputSystem.Update(); game.Tick(0.02f, true);
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Moved, position = new Vector2(240, 400) });
                InputSystem.Update(); game.Tick(0.02f, true);
                Require(game.SquadX > 0, "Touch drag moves the squad", checks);
                InputSystem.RemoveDevice(touch); touch = null;
                var pause = game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "Pause");
                pause.onClick.Invoke(); Require(game.State == RunState.Paused, "Pause button freezes the run", checks);
                play.onClick.Invoke(); Require(game.State == RunState.Playing, "Continue button resumes the run", checks);
                game.StartVerificationStage(1, 5104); game.ChangeSquad(15); game.SetAim(1.5f);
                for (int i = 0; i < 325; i++) game.Tick(0.02f, false);
                Require(game.State == RunState.Playing && game.ActiveTargets > 0 && game.ActiveBullets > 0, "Enemies, squad and automatic bullets run in Play mode", checks);
                var progress = game.Hud.GetComponentsInChildren<RectTransform>().Single(r => r.name == "ProgressFill");
                Require(Mathf.Abs(progress.anchorMax.x - game.Elapsed / game.StageDuration) < 0.01f, "Visible progress bar follows the current stage duration", checks);
                game.SpawnTarget(TargetKind.Crate, -2.5f, 16, 22);
                game.Hud.Flash("");
                yield return null; // Flush Unity's deferred Destroy queue before inspecting rendered objects.
                Capture(game, "gameplay.png");
                game.SpawnTarget(TargetKind.Boss, 0, 15, 1000);
                game.Tick(0.02f, false);
                yield return null;
                Capture(game, "boss.png");
                game.ChangeSquad(-100); Require(game.State == RunState.Lost, "Defeat menu appears in Play mode", checks);
                play.onClick.Invoke(); Require(game.State == RunState.Playing && game.SquadCount == 3 && game.ActiveTargets == 0, "Retry button resets the run", checks);
                game.ChangeSquad(997);
                for (int row = 0; row < 9; row++) for (int column = 0; column < 8; column++)
                    game.SpawnTarget(TargetKind.Enemy, -3.5f + column, 13 + row * 2.2f, 999);
                game.Tick(0.02f, false);
                Require(game.SquadCount == 1000 && game.VisibleAllies == 96 && game.ActiveTargets == 72 && game.ActiveBullets == 40 && game.LastVolleyDamage == 1000, "1000 allies, 96 displayed troops, 72 enemies and weighted automatic fire run in Play mode", checks);
                yield return null;
                Capture(game, "crowd.png");
                var countLabel = game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "SquadCount");
                Require(countLabel.text == "仲間 1000" && !countLabel.isTextOverflowing, "1000-ally HUD is fully visible without clipping", checks);
                game.StartVerificationStage(1, 5104);
                game.SpawnTarget(TargetKind.Crate, 0, 5, 1, SupplyReward.Power);
                for (int i = 0; i < 30; i++) game.Tick(0.02f, false);
                game.SpawnTarget(TargetKind.Crate, 0, 5, 1, SupplyReward.RapidFire);
                for (int i = 0; i < 30; i++) game.Tick(0.02f, false);
                Require(game.WeaponDamage == 2 && game.RapidFireLevel == 1 && game.GetComponentsInChildren<SquadRushTrooper>().Where(t => t.WeaponModule != null).All(t => t.WeaponModule.gameObject.activeSelf), "Weapon crates upgrade damage, firing rate and visible rifles", checks);
                game.ChangeSquad(27);
                game.SpawnTarget(TargetKind.Crate, -2.8f, 14, 18, SupplyReward.Power);
                game.SpawnTarget(TargetKind.Crate, 2.8f, 17, 22, SupplyReward.RapidFire);
                game.Hud.Flash(""); yield return null; Capture(game, "weapons.png");
                var weaponLabel = game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "WeaponStats");
                Require(weaponLabel.text.Contains("威力 2") && !weaponLabel.isTextOverflowing, "Weapon HUD shows acquired upgrades without clipping", checks);
                game.StartVerificationStage(1, 5104); game.ChangeSquad(57); game.SpawnTarget(TargetKind.Boss, 0, 20, 1);
                for (int i = 0; i < 180 && game.State == RunState.Playing; i++) game.Tick(0.02f, false);
                Require(game.State == RunState.Won && new SquadRushProgress(storageKey).ResumeStage == 2, "A real boss defeat saves stage two", checks);
                Require(game.Equipment.Owns(0) && new SquadRushEquipment(storageKey).Owns(0), "Boss clear permanently saves the rifle reward", checks);
                yield return null;
                Capture(game, "clear.png");
                play.onClick.Invoke();
                Require(game.StageNumber == 2 && game.State == RunState.Playing && game.ActiveTargets == 0, "The clear screen Next button advances to stage two", checks);
                game.ChangeSquad(15); game.SetAim(-1.5f);
                for (int i = 0; i < 300; i++) game.Tick(0.02f, false);
                game.Hud.Flash(""); yield return null; Capture(game, "forest.png");
                game.StartVerificationStage(3, 8128); game.ChangeSquad(15); game.SetAim(1.5f);
                for (int i = 0; i < 300; i++) game.Tick(0.02f, false);
                game.Hud.Flash(""); yield return null; Capture(game, "snow.png");
                game.ConfigureVerification(storageKey);
                Require(game.StageNumber == 2 && game.State == RunState.Ready, "Reloading the saved progress offers stage two in the menu", checks);
                Click(game, "HomeEquipment"); Click(game, "InventoryItem0"); Click(game, "EquipmentAction");
                Require(game.Equipment.IsEquipped(0) && game.WeaponDamage == 2 && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Equipment button equips the earned rifle and updates its state", checks);
                yield return null; Capture(game, "equipment.png");
                Require(game.Hud.GetComponentsInChildren<TMP_Text>().Where(t => t.gameObject.activeInHierarchy).All(t => !t.isTextOverflowing), "Equipment page labels fit their visible controls", checks);
                Click(game, "EquippedSlotWeapon"); Capture(game, "equipment-detail.png");
                Require(Button(game, "EquipmentAction").interactable && Button(game, "EquipmentAction").GetComponentInChildren<TMP_Text>().text == "装備を外す", "The equipped weapon slot opens its own removable item detail", checks);
                Click(game, "CloseEquipmentDetail");
                Click(game, "HomeCommand"); Click(game, "HomeStages"); Click(game, "PreviousStage"); Click(game, "StageStart");
                Require(game.StageNumber == 1 && game.WeaponDamage == 2 && game.Equipment.IsEquipped(0), "An earlier unlocked stage starts with permanent equipped bonuses", checks);
                game.TogglePause(); Click(game, "ReturnHome");
                Require(game.State == RunState.Ready && game.Hud.CurrentPage == FrontEndPage.Home && game.ActiveTargets == 0 && game.ActiveBullets == 0, "Pause screen returns cleanly to Home", checks);
                game.ConfigureVerification(storageKey); Click(game, "HomeEquipment");
                Require(game.Equipment.IsEquipped(0) && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Reload restores both the equipped rifle and equipment UI", checks);
                Click(game, "EquippedSlotWeapon"); Click(game, "EquipmentAction");
                Require(game.WeaponDamage == 1 && !game.Equipment.IsEquipped(0), "Equipment button also unequips the rifle", checks);
                Click(game, "HomeCommand");
                Click(game, "HomeStages"); Click(game, "PreviousStage");
                Capture(game, "stages-owned.png");
                Require(game.Hud.GetComponentsInChildren<TMP_Text>().Where(t => t.gameObject.activeInHierarchy).All(t => !t.isTextOverflowing), "Stage page fits already-owned reward and navigation labels", checks);
                Click(game, "HomeCommand");
                Click(game, "HomeSortie");
                Require(game.State == RunState.Playing && game.StageNumber == 2 && game.Hud.CurrentPage == FrontEndPage.Hidden, "Home's central sortie starts the current unlocked stage directly", checks);
                game.ShowHome();
                Require(game.Hud.GetComponentsInChildren<TMP_Text>(true).All(t => t.font == null || t.text.All(c => c < 32 || t.font.HasCharacter(c))), "Menu, stage names and Next button have baked Japanese glyphs", checks);
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification");
                Directory.CreateDirectory(output); File.WriteAllLines(Path.Combine(output, "runtime-checks.txt"), checks);
                Debug.Log("[SquadRush] " + checks.Count + " Play mode scenarios passed.");
            }
            finally
            {
                if (touch != null) InputSystem.RemoveDevice(touch);
                PlayerPrefs.DeleteKey(storageKey); PlayerPrefs.DeleteKey(SquadRushEquipment.StorageKey(storageKey)); PlayerPrefs.Save();
                SessionState.SetBool(Pending, false);
            }
        }
        static void Require(bool condition, string name, System.Collections.Generic.List<string> checks)
        { if (!condition) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
        static UnityEngine.UI.Button Button(SquadRushGame game, string name) => game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == name);
        static void Click(SquadRushGame game, string name)
        {
            var button = Button(game, name);
            if (!button.gameObject.activeInHierarchy || !button.interactable) throw new InvalidOperationException("Button unavailable: " + name);
            button.onClick.Invoke();
        }
        static void Capture(SquadRushGame game, string name)
        {
            game.Hud.UpdateStats();
            var view = game.View;
            var canvas = game.Hud.GetComponentInChildren<Canvas>();
            var safe = (RectTransform)canvas.transform.Find("SafeArea");
            var texture = new RenderTexture(540, 960, 24) { antiAliasing = 2 };
            var previous = RenderTexture.active;
            var image = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                view.rect = new Rect(0, 0, 1, 1); view.targetTexture = texture;
                view.aspect = 540f / 960;
                view.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(15 * Mathf.Deg2Rad) / view.aspect) * Mathf.Rad2Deg;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = view; canvas.planeDistance = 1;
                safe.anchorMin = Vector2.zero; safe.anchorMax = Vector2.one; safe.offsetMin = safe.offsetMax = Vector2.zero;
                Canvas.ForceUpdateCanvases();
                foreach (TMP_Text text in game.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases(); view.Render();
                RenderTexture.active = texture; image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification", "Preview");
                Directory.CreateDirectory(output); File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; view.targetTexture = null; RenderTexture.active = previous;
                UnityEngine.Object.Destroy(image); texture.Release(); UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
