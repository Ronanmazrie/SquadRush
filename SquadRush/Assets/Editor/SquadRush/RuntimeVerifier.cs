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
            try
            {
                var checks = new System.Collections.Generic.List<string>();
                Require(game.State == RunState.Ready && game.SquadCount == 3, "Scene starts with a three-ally squad and menu", checks);
                var play = game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "Play");
                Capture(game, "menu.png");
                play.onClick.Invoke(); Require(game.State == RunState.Playing, "Start button starts the game", checks);
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
                game.Begin(); game.ChangeSquad(15); game.SetAim(1.5f);
                for (int i = 0; i < 325; i++) game.Tick(0.02f, false);
                Require(game.State == RunState.Playing && game.ActiveTargets > 0 && game.ActiveBullets > 0, "Enemies, squad and automatic bullets run in Play mode", checks);
                var progress = game.Hud.GetComponentsInChildren<RectTransform>().Single(r => r.name == "ProgressFill");
                Require(Mathf.Abs(progress.anchorMax.x - game.Elapsed / SquadRushGame.StageSeconds) < 0.01f, "Visible progress bar follows stage time", checks);
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
                game.ChangeSquad(57);
                for (int row = 0; row < 9; row++) for (int column = 0; column < 8; column++)
                    game.SpawnTarget(TargetKind.Enemy, -3.5f + column, 13 + row * 2.2f, 999);
                game.Tick(0.02f, false);
                Require(game.SquadCount == 60 && game.ActiveTargets == 72 && game.ActiveBullets == 60, "Crowded scene runs with 60 allies, 72 enemies and automatic fire", checks);
                yield return null;
                Capture(game, "crowd.png");
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification");
                Directory.CreateDirectory(output); File.WriteAllLines(Path.Combine(output, "runtime-checks.txt"), checks);
                Debug.Log("[SquadRush] " + checks.Count + " Play mode scenarios passed.");
            }
            finally
            {
                if (touch != null) InputSystem.RemoveDevice(touch);
                SessionState.SetBool(Pending, false);
            }
        }
        static void Require(bool condition, string name, System.Collections.Generic.List<string> checks)
        { if (!condition) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
        static void Capture(SquadRushGame game, string name)
        {
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
