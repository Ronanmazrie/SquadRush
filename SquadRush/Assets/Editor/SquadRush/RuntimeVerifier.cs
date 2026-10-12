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
            game.StartCoroutine(CheckRuntime(game));
        }
        static IEnumerator CheckRuntime(SquadRushGame game)
        {
            for (int i = 0; i < 5; i++) yield return null;
            Touchscreen touch = null;
            int code = 0;
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
                Capture(game, "gameplay.png");
                game.ChangeSquad(-100); Require(game.State == RunState.Lost, "Defeat menu appears in Play mode", checks);
                play.onClick.Invoke(); Require(game.State == RunState.Playing && game.SquadCount == 3 && game.ActiveTargets == 0, "Retry button resets the run", checks);
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification");
                Directory.CreateDirectory(output); File.WriteAllLines(Path.Combine(output, "runtime-checks.txt"), checks);
                Debug.Log("[SquadRush] " + checks.Count + " Play mode scenarios passed.");
            }
            catch (Exception error) { code = 1; Debug.LogException(error); }
            finally
            {
                if (touch != null) InputSystem.RemoveDevice(touch);
                SessionState.SetBool(Pending, false);
            }
            EditorApplication.Exit(code);
        }
        static void Require(bool condition, string name, System.Collections.Generic.List<string> checks)
        { if (!condition) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
        static void Capture(SquadRushGame game, string name)
        {
            var view = game.View;
            var canvas = game.Hud.GetComponentInChildren<Canvas>();
            var safe = (RectTransform)canvas.transform.Find("SafeArea");
            var texture = new RenderTexture(540, 960, 24);
            var previous = RenderTexture.active;
            var image = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                view.rect = new Rect(0, 0, 1, 1); view.targetTexture = texture;
                view.aspect = 540f / 960; view.orthographicSize = 5.7f / view.aspect;
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
