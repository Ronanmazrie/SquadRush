using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SquadRush
{
    public enum RunState { Ready, Playing, Won, Lost, Paused }
    public enum TargetKind { Enemy, Crate, Boss }

    public sealed class SquadRushGame : MonoBehaviour
    {
        public const int MaxSquad = 60;
        public int StageNumber { get; private set; }
        public SquadRushStage Stage => new SquadRushStage(StageNumber);
        public float StageDuration => Stage.Duration;
        public SquadRushScenery Scenery { get; private set; }
        public RunState State { get; private set; } = RunState.Ready;
        public int SquadCount { get; private set; }
        public int Kills { get; private set; }
        public float Elapsed { get; private set; }
        public float SquadX { get; private set; }
        public Camera View { get; private set; }
        public SquadRushHud Hud { get; private set; }
        public int ActiveTargets => targets.Count;
        public int ActiveBullets => shots.Count;

        sealed class Target
        {
            public GameObject Root; public TargetKind Kind; public int Hp, MaxHp;
            public float Radius, Speed; public TextMeshPro Label; public SquadRushTrooper Rig;
        }
        sealed class Shot { public Transform Body; public float Life; }
        sealed class Gate
        {
            public Transform Root; public int Left, Right; public bool LeftMultiply, RightMultiply;
        }
        sealed class Spark { public Transform Body; public float Life; }

        readonly List<Target> targets = new List<Target>();
        readonly List<Shot> shots = new List<Shot>();
        readonly List<Gate> gates = new List<Gate>();
        readonly List<Spark> sparks = new List<Spark>();
        readonly List<Transform> soldiers = new List<Transform>();
        readonly List<SquadRushTrooper> soldierRigs = new List<SquadRushTrooper>();
        readonly List<Transform> roadMarks = new List<Transform>();
        readonly Stack<Transform> shotPool = new Stack<Transform>();
        readonly Stack<Transform> sparkPool = new Stack<Transform>();
        readonly Stack<Target> enemyPool = new Stack<Target>();
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        Transform world, squadRoot, poolRoot;
        SquadRushArt art;
        Material outlinedText;
        Texture2D asphalt;
        SquadRushProgress progress = new SquadRushProgress();
        System.Random random;
        Target boss;
        float shotTimer, waveTimer, gateTimer, crateTimer, bossWaveTimer, targetX;
        bool dragging, initialized;
        Vector2 lastPointer;
        Rect previousSafeArea;
        int previousWidth, previousHeight;

        void Awake() { Initialize(); }
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            StageNumber = progress.ResumeStage;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            world = new GameObject("World").transform;
            world.SetParent(transform);
            poolRoot = new GameObject("ObjectPools").transform;
            poolRoot.SetParent(transform);
            squadRoot = new GameObject("BlueSquad").transform;
            squadRoot.SetParent(world);
            CreateEnvironment();
            art = gameObject.AddComponent<SquadRushArt>(); art.Initialize(View);
            Scenery = gameObject.AddComponent<SquadRushScenery>(); Scenery.Initialize(art, world);
            GenerateLandscape(NewSeed());
            for (int i = 0; i < MaxSquad; i++)
            {
                Transform soldier = CreateSoldier("Ally", squadRoot, false);
                soldier.gameObject.SetActive(false); soldiers.Add(soldier); soldierRigs.Add(soldier.GetComponent<SquadRushTrooper>());
            }
            for (int i = 0; i < 384; i++) shotPool.Push(CreateBullet());
            for (int i = 0; i < 64; i++) sparkPool.Push(CreateSpark());
            for (int i = 0; i < 96; i++) enemyPool.Push(CreateEnemy());
            Hud = gameObject.AddComponent<SquadRushHud>();
            Hud.Build(this);
            SetSquad(Stage.StartingAllies); Hud.UpdateStats();
            ShowOpening();
        }

        public void Begin()
        { StartRun(NewSeed()); }
        static int NewSeed() => Guid.NewGuid().GetHashCode();
        void StartRun(int seed)
        {
            ClearRun();
            random = new System.Random(seed ^ 0x51F15E);
            GenerateLandscape(seed);
            Elapsed = 0; Kills = 0; SquadX = targetX = 0;
            squadRoot.position = new Vector3(0, 0, 1.5f);
            shotTimer = 0; waveTimer = 1.5f; gateTimer = 0.5f; crateTimer = 4;
            bossWaveTimer = 3; dragging = false; boss = null;
            State = RunState.Playing; SetSquad(Stage.StartingAllies); Hud.HideMenu(); Hud.UpdateStats();
        }
        void ShowOpening() => Hud.ShowMenu("SQUAD\nRUSH", "STAGE " + StageNumber.ToString("D2") + "  ／  " + Stage.Name + "\n指で左右に移動して、仲間を増やそう", StageNumber == 1 ? "スタート" : "つづきから遊ぶ");
        void GenerateLandscape(int seed)
        {
            Scenery.Generate(Stage.Biome, seed);
            materials["Sand"].color = Stage.Ground;
            materials["Curb"].color = Stage.Biome == LandscapeBiome.Snow ? new Color(0.71f, 0.82f, 0.9f) : new Color(0.61f, 0.67f, 0.71f);
            RenderSettings.fogColor = Stage.Fog; View.backgroundColor = Stage.Fog;
        }
#if UNITY_EDITOR
        public void ConfigureVerification(string storageKey)
        {
            ClearRun(); progress = new SquadRushProgress(storageKey); StageNumber = progress.ResumeStage;
            State = RunState.Ready; GenerateLandscape(12345); SetSquad(Stage.StartingAllies); Hud.UpdateStats(); ShowOpening();
        }
        public void StartVerificationStage(int number, int seed) { StageNumber = Mathf.Max(1, number); StartRun(seed); }
#endif

        void Update()
        {
            if (Screen.width != previousWidth || Screen.height != previousHeight) ResizeView();
            if (State == RunState.Playing) Tick(Mathf.Min(Time.deltaTime, 0.05f), true);
            if (Screen.safeArea != previousSafeArea)
            { previousSafeArea = Screen.safeArea; Hud.ApplySafeArea(); }
        }
        void ResizeView()
        {
            previousWidth = Screen.width; previousHeight = Screen.height;
            float aspect = (float)Mathf.Max(1, Screen.width) / Mathf.Max(1, Screen.height);
            float width = Mathf.Min(1, (9f / 16) / aspect);
            View.rect = new Rect((1 - width) * 0.5f, 0, width, 1);
            View.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(15 * Mathf.Deg2Rad) / (aspect * width)) * Mathf.Rad2Deg;
            if (Hud != null) Hud.ApplySafeArea();
        }

        public void Tick(float dt, bool readInput)
        {
            if (State != RunState.Playing || dt <= 0) return;
            Elapsed += dt;
            if (readInput) ReadInput(dt);
            SquadX = Mathf.MoveTowards(SquadX, targetX, dt * 18);
            squadRoot.position = new Vector3(SquadX, 0, 1.5f);
            AnimateSquad(dt);
            ScrollRoad(dt);
            shotTimer -= dt;
            if (shotTimer <= 0) { FireVolley(); shotTimer += 0.34f; }
            if (Elapsed < StageDuration)
            {
                waveTimer -= dt; gateTimer -= dt; crateTimer -= dt;
                if (waveTimer <= 0) { SpawnWave(); waveTimer = Stage.WaveInterval(Elapsed / StageDuration); }
                if (gateTimer <= 0) { SpawnGate(); gateTimer = 9f; }
                if (crateTimer <= 0) { SpawnTarget(TargetKind.Crate, Next(-3.2f, 3.2f), 30, 8 + Stage.Difficulty + (int)(Elapsed / 5)); crateTimer = 10; }
            }
            else if (boss == null) SpawnBoss();
            else
            {
                bossWaveTimer -= dt;
                if (bossWaveTimer <= 0) { SpawnWave(6, 22); bossWaveTimer = 4.5f; }
            }
            MoveTargets(dt);
            if (State != RunState.Playing) return;
            MoveShots(dt);
            if (State != RunState.Playing) return;
            MoveGates(dt); MoveSparks(dt); Hud.UpdateStats();
        }

        void ReadInput(float dt)
        {
            bool pressed = false; Vector2 pointer = default;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            { pressed = true; pointer = Touchscreen.current.primaryTouch.position.ReadValue(); }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            { pressed = true; pointer = Mouse.current.position.ReadValue(); }
            if (pressed)
            {
                if (dragging) targetX += (pointer.x - lastPointer.x) / Mathf.Max(1, Screen.width) * 12;
                lastPointer = pointer; dragging = true;
            }
            else dragging = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) targetX -= dt * 7;
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) targetX += dt * 7;
                if (Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
            }
            targetX = Mathf.Clamp(targetX, -3.1f, 3.1f);
        }

        public void SetAim(float x) { targetX = Mathf.Clamp(x, -3.1f, 3.1f); }
        public void ChangeSquad(int amount) { SetSquad(SquadCount + amount); }
        public void ApplyGate(int value, bool multiply)
        {
            SetSquad(multiply ? SquadCount * value : SquadCount + value);
            Hud.Flash(multiply ? "仲間 ×" + value : "仲間 " + (value >= 0 ? "+" : "") + value);
        }
        void SetSquad(int count)
        {
            SquadCount = Mathf.Clamp(count, 0, MaxSquad);
            // Keep the back rows clear of the bottom control hint as the formation grows.
            View.transform.LookAt(new Vector3(0, 0, Mathf.Lerp(13, 10, Mathf.InverseLerp(30, 60, SquadCount))));
            int columns = Mathf.Min(6, Mathf.Max(1, SquadCount));
            for (int i = 0; i < soldiers.Count; i++)
            {
                if (i < SquadCount && !soldiers[i].gameObject.activeSelf) soldierRigs[i].ResetPose();
                soldiers[i].gameObject.SetActive(i < SquadCount);
                int row = i / columns;
                int rowCount = Mathf.Min(columns, SquadCount - row * columns);
                soldiers[i].localPosition = new Vector3((i % columns - (rowCount - 1) * 0.5f) * 0.48f, 0, -row * 0.4f);
            }
            if (SquadCount == 0 && State == RunState.Playing) Finish(false);
        }
        void AnimateSquad(float dt)
        {
            for (int i = 0; i < SquadCount; i++)
            {
                soldierRigs[i].Animate(Elapsed, dt, i * 1.8f, Mathf.Clamp((SquadX - targetX) * 5, -9, 9));
            }
        }

        void FireVolley()
        {
            // Every ally contributes one bullet; pools bound the visual workload.
            for (int i = 0; i < SquadCount; i++)
            {
                if (shotPool.Count == 0) break;
                Transform body = shotPool.Pop(); body.gameObject.SetActive(true);
                body.position = soldiers[i].position + new Vector3(0.12f, 0.65f, 0.5f);
                shots.Add(new Shot { Body = body, Life = 1.7f });
                soldierRigs[i].Fire();
            }
        }
        void MoveShots(float dt)
        {
            for (int i = shots.Count - 1; i >= 0; i--)
            {
                Shot shot = shots[i]; Vector3 old = shot.Body.position;
                Vector3 next = old + Vector3.forward * (29 * dt);
                shot.Body.position = next; shot.Life -= dt;
                Target hit = null; float nearest = float.MaxValue;
                foreach (Target target in targets)
                {
                    Vector3 p = target.Root.transform.position;
                    if (Mathf.Abs(p.x - old.x) < target.Radius && p.z + target.Radius >= old.z && p.z - target.Radius <= next.z && p.z < nearest)
                    { nearest = p.z; hit = target; }
                }
                if (hit != null)
                {
                    Burst(shot.Body.position); Damage(hit, 1); ReturnShot(i);
                    if (State != RunState.Playing) return;
                }
                else if (shot.Life <= 0 || next.z > 40) ReturnShot(i);
            }
        }
        void ReturnShot(int index)
        {
            Transform body = shots[index].Body; body.gameObject.SetActive(false);
            shotPool.Push(body); shots.RemoveAt(index);
        }

        void SpawnWave(int count = -1, float z = 32)
        {
            if (count < 0) count = 3 + Stage.Difficulty / 2 + (int)(Elapsed / 10);
            for (int i = 0; i < count; i++)
                SpawnTarget(TargetKind.Enemy, Next(-4f, 4f), z + Next(0, 3), Stage.EnemyHealth + (int)(Elapsed / 30));
        }
        float Next(float min, float max) { return min + (float)random.NextDouble() * (max - min); }
        public void SpawnTarget(TargetKind kind, float x, float z, int hp)
        {
            Target t;
            if (kind == TargetKind.Enemy)
            {
                if (enemyPool.Count == 0) return;
                t = enemyPool.Pop(); t.Root.SetActive(true); t.Radius = 0.32f; t.Rig.ResetPose();
            }
            else
            {
                t = new Target { Kind = kind, Root = new GameObject(kind.ToString()), Radius = kind == TargetKind.Boss ? 1.5f : 0.85f };
                t.Root.transform.SetParent(world);
                if (kind == TargetKind.Crate)
                {
                    art.Crate(t.Root.transform);
                    t.Label = WorldText(t.Root.transform, "", new Vector3(0, 1.3f, -1.5f), 10, Color.white);
                }
                else
                {
                    Transform model = art.Trooper("BossModel", t.Root.transform, true, true); model.localScale = Vector3.one * 3.4f;
                    model.localRotation = Quaternion.Euler(0, 180, 0); t.Rig = model.GetComponent<SquadRushTrooper>();
                    t.Label = WorldText(t.Root.transform, "", new Vector3(0, 4.8f, 0), 5, Color.white);
                    boss = t;
                }
            }
            t.Kind = kind; t.Hp = t.MaxHp = hp;
            t.Speed = kind == TargetKind.Enemy ? 3.2f + Stage.Difficulty * 0.055f + Elapsed / 120 : 3.5f;
            t.Root.transform.position = new Vector3(x, 0, z);
            if (t.Label != null) t.Label.text = hp.ToString();
            targets.Add(t);
        }
        void SpawnBoss()
        {
            SpawnTarget(TargetKind.Boss, 0, 32, Stage.BossHealth);
            Hud.Flash("ボス出現！ 集中攻撃！");
        }
        void MoveTargets(float dt)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                Target t = targets[i]; Transform body = t.Root.transform;
                if (t.Rig != null) t.Rig.Animate(Elapsed, dt, body.position.x * 2);
                if (t.Kind == TargetKind.Boss)
                {
                    Vector3 p = body.position; p.z = Mathf.Max(16, p.z - dt * 3.5f);
                    p.x = Mathf.Sin((Elapsed - StageDuration) * 0.6f) * 2.3f; body.position = p;
                }
                else
                {
                    Vector3 old = body.position; body.position += Vector3.back * (t.Speed * dt);
                    bool collision = false;
                    for (int s = 0; s < SquadCount; s++)
                    {
                        Vector3 p = soldiers[s].position;
                        if (Mathf.Abs(p.x - body.position.x) < t.Radius + 0.2f && old.z + t.Radius >= p.z && body.position.z - t.Radius <= p.z)
                        { collision = true; break; }
                    }
                    if (collision)
                    {
                        Burst(body.position + Vector3.up); RemoveTarget(t);
                        ChangeSquad(t.Kind == TargetKind.Crate ? -3 : -1);
                        Hud.Flash("仲間が減った！");
                        if (State != RunState.Playing) return;
                    }
                    else if (body.position.z < -7) RemoveTarget(t);
                }
            }
        }
        void Damage(Target t, int damage)
        {
            t.Hp -= damage;
            if (t.Rig != null) t.Rig.Hit();
            if (t.Label != null) t.Label.text = Mathf.Max(0, t.Hp).ToString();
            if (t.Hp > 0) return;
            Burst(t.Root.transform.position + Vector3.up); Burst(t.Root.transform.position + new Vector3(0.3f, 0.7f, 0.2f));
            TargetKind kind = t.Kind; RemoveTarget(t);
            if (kind == TargetKind.Crate) { ChangeSquad(4); Hud.Flash("箱を撃破！ 仲間 +4"); }
            else if (kind == TargetKind.Boss) Finish(true);
            else Kills++;
        }
        void RemoveTarget(Target t)
        {
            targets.Remove(t);
            if (t.Kind == TargetKind.Enemy) { t.Root.SetActive(false); enemyPool.Push(t); }
            else { if (Application.isPlaying) Destroy(t.Root); else DestroyImmediate(t.Root); }
        }

        void SpawnGate()
        {
            var gate = new Gate { Root = new GameObject("ChoiceGate").transform };
            gate.Root.SetParent(world); gate.Root.position = new Vector3(0, 0, 30);
            bool goodOnLeft = random.Next(2) == 0;
            int good = Elapsed > 18 && random.Next(3) == 0 ? 2 : 5 + (int)(Elapsed / 6);
            bool multiply = good == 2;
            gate.Left = goodOnLeft ? good : -4; gate.Right = goodOnLeft ? -4 : good;
            gate.LeftMultiply = goodOnLeft && multiply; gate.RightMultiply = !goodOnLeft && multiply;
            GatePanel(gate.Root, -2.5f, gate.Left, gate.LeftMultiply);
            GatePanel(gate.Root, 2.5f, gate.Right, gate.RightMultiply); gates.Add(gate);
        }
        void GatePanel(Transform parent, float x, int value, bool multiply)
        {
            art.Gate(parent, x, value > 0);
            WorldText(parent, multiply ? "×" + value : (value > 0 ? "+" : "") + value, new Vector3(x, 1.0f, -0.25f), 12, Color.white);
        }
        void MoveGates(float dt)
        {
            for (int i = gates.Count - 1; i >= 0; i--)
            {
                Gate gate = gates[i]; gate.Root.position += Vector3.back * (3.5f * dt);
                if (gate.Root.position.z > 1.5f) continue;
                ApplyGate(SquadX < 0 ? gate.Left : gate.Right, SquadX < 0 ? gate.LeftMultiply : gate.RightMultiply);
                if (Application.isPlaying) Destroy(gate.Root.gameObject); else DestroyImmediate(gate.Root.gameObject);
                gates.RemoveAt(i);
                if (State != RunState.Playing) return;
            }
        }

        public void TogglePause()
        {
            if (State == RunState.Playing)
            { State = RunState.Paused; dragging = false; Hud.ShowMenu("一時停止", "ひと息ついたら、続きを遊ぼう", "つづける"); }
            else if (State == RunState.Paused) { State = RunState.Playing; Hud.HideMenu(); }
        }
        void OnApplicationPause(bool paused) { if (paused && State == RunState.Playing) TogglePause(); }
        public void MenuAction()
        {
            if (State == RunState.Paused) TogglePause();
            else if (State == RunState.Won) { StageNumber++; Begin(); }
            else if (State == RunState.Ready || State == RunState.Lost) Begin();
        }
        void Finish(bool won)
        {
            State = won ? RunState.Won : RunState.Lost; dragging = false;
            if (won) progress.Complete(StageNumber);
            Hud.UpdateStats();
            Hud.ShowMenu(won ? "ステージクリア！" : "もう一度、挑戦！", "STAGE " + StageNumber.ToString("D2") + "  ／  " + Stage.Name + "\n撃破 " + Kills + "体   ／   仲間 " + SquadCount + "人", won ? "次のステージへ" : "もう一度遊ぶ");
        }
        void ClearRun()
        {
            for (int i = shots.Count - 1; i >= 0; i--) ReturnShot(i);
            for (int i = targets.Count - 1; i >= 0; i--) RemoveTarget(targets[i]);
            foreach (Gate gate in gates) { if (Application.isPlaying) Destroy(gate.Root.gameObject); else DestroyImmediate(gate.Root.gameObject); }
            gates.Clear();
            foreach (Spark s in sparks) { s.Body.gameObject.SetActive(false); sparkPool.Push(s.Body); }
            sparks.Clear();
        }

        void Burst(Vector3 position)
        {
            if (sparkPool.Count == 0) return;
            Transform body = sparkPool.Pop(); body.position = position; body.localScale = Vector3.one * 0.28f;
            body.rotation = View.transform.rotation; body.gameObject.SetActive(true); sparks.Add(new Spark { Body = body, Life = 0.22f });
        }
        void MoveSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                sparks[i].Life -= dt; sparks[i].Body.localScale += Vector3.one * (dt * 2.1f);
                if (sparks[i].Life > 0) continue;
                sparks[i].Body.gameObject.SetActive(false); sparkPool.Push(sparks[i].Body); sparks.RemoveAt(i);
            }
        }
        Transform CreateBullet()
        {
            return art.Projectile(poolRoot);
        }
        Transform CreateSpark()
        {
            return art.Impact(poolRoot);
        }
        Target CreateEnemy()
        {
            Transform root = CreateSoldier("Enemy", world, true); root.rotation = Quaternion.Euler(0, 180, 0);
            root.gameObject.SetActive(false); return new Target { Root = root.gameObject, Kind = TargetKind.Enemy, Rig = root.GetComponent<SquadRushTrooper>() };
        }
        Transform CreateSoldier(string name, Transform parent, bool red)
        {
            return art.Trooper(name, parent, red);
        }
        void CreateEnvironment()
        {
            View = new GameObject("Main Camera").AddComponent<Camera>(); View.tag = "MainCamera";
            View.transform.SetParent(transform); View.transform.position = new Vector3(0, 22, -12);
            View.transform.LookAt(new Vector3(0, 0, 13)); View.orthographic = false;
            View.nearClipPlane = 0.1f; View.farClipPlane = 150; View.allowHDR = false;
            View.clearFlags = CameraClearFlags.SolidColor; View.backgroundColor = new Color(0.83f, 0.72f, 0.54f);
            var light = new GameObject("Sun").AddComponent<Light>(); light.transform.SetParent(transform);
            light.type = LightType.Directional; light.intensity = 1.12f; light.color = new Color(1, 0.94f, 0.84f); light.transform.rotation = Quaternion.Euler(48, -42, 0);
            light.shadows = LightShadows.Soft; light.shadowStrength = 0.72f; light.shadowBias = 0.035f; light.shadowNormalBias = 0.16f;
            QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = 55; QualitySettings.shadowCascades = 0; QualitySettings.antiAliasing = 2;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.53f, 0.61f, 0.70f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.78f, 0.71f, 0.60f); RenderSettings.fogStartDistance = 42; RenderSettings.fogEndDistance = 95;
            Material road = Mat("Road", new Color(0.35f, 0.39f, 0.43f));
            asphalt = new Texture2D(256, 256, TextureFormat.RGB24, true) { name = "Asphalt grain", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear };
            var pixels = new Color[256 * 256];
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++)
            {
                uint hash = (uint)(x * 374761393 + y * 668265263); hash = (hash ^ (hash >> 13)) * 1274126177u;
                float grain = 0.94f + (hash & 255) / 4250f;
                grain -= Mathf.Pow(Mathf.Abs(Mathf.Sin(x * 0.035f)), 18) * 0.045f;
                pixels[y * 256 + x] = new Color(grain, grain, grain);
            }
            asphalt.SetPixels(pixels); asphalt.Apply(true, true); road.mainTexture = asphalt; road.mainTextureScale = new Vector2(3, 20); road.SetFloat("_Glossiness", 0.02f);
            Part(PrimitiveType.Cube, world, new Vector3(0, -0.28f, 14), new Vector3(11, 0.5f, 70), road);
            Material white = Mat("Marking", new Color(0.91f, 0.9f, 0.83f));
            for (int i = 0; i < 18; i++)
                roadMarks.Add(Part(PrimitiveType.Cube, world, new Vector3(0, 0.01f, -8 + i * 3.3f), new Vector3(0.12f, 0.025f, 1.4f), white));
            for (int side = -1; side <= 1; side += 2)
            {
                Part(PrimitiveType.Cube, world, new Vector3(side * 4.85f, 0.02f, 14), new Vector3(0.08f, 0.025f, 70), white);
                Part(PrimitiveType.Cube, world, new Vector3(side * 5.35f, 0.18f, 14), new Vector3(0.3f, 0.45f, 70), Mat("Curb", new Color(0.61f, 0.67f, 0.71f)));
                Part(PrimitiveType.Cube, world, new Vector3(side * 13, -0.34f, 14), new Vector3(15, 0.25f, 70), Mat("Sand", new Color(0.76f, 0.62f, 0.4f)));
                Part(PrimitiveType.Cube, world, new Vector3(side * 5.4f, 0.67f, 14), new Vector3(0.16f, 0.12f, 70), Mat("Rail", new Color(0.37f, 0.47f, 0.53f)));
                for (int i = 0; i < 18; i++)
                {
                    float z = -8 + i * 3.3f;
                    roadMarks.Add(Part(PrimitiveType.Cube, world, new Vector3(side * 5.4f, 0.40f, z), new Vector3(0.18f, 0.63f, 0.18f), Mat("Rail", new Color(0.37f, 0.47f, 0.53f))));
                    roadMarks.Add(Part(PrimitiveType.Cube, world, new Vector3(side * 5.4f, 0.67f, z), new Vector3(0.21f, 0.12f, 0.12f), Mat("Reflector", new Color(1, 0.67f, 0.18f))));
                }
            }
            ResizeView();
        }
        void ScrollRoad(float dt)
        {
            Scenery.Scroll(dt);
            foreach (Transform marking in roadMarks)
            { marking.position += Vector3.back * (dt * 3.5f); if (marking.position.z < -8) marking.position += Vector3.forward * 59.4f; }
        }
        Material Mat(string key, Color color, bool unlit = false)
        {
            if (materials.TryGetValue(key, out Material existing)) return existing;
            Material seed = Resources.Load<Material>(unlit ? "UnlitSeed" : "StandardSeed");
            Material mat = seed != null ? new Material(seed) : new Material(Shader.Find(unlit ? "Unlit/Color" : "Standard"));
            mat.name = key; mat.color = color; mat.enableInstancing = true;
            if (!unlit) { mat.SetFloat("_Glossiness", 0.18f); }
            materials.Add(key, mat); return mat;
        }
        Transform Part(PrimitiveType shape, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(shape); obj.name = material.name;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = obj.GetComponent<Collider>();
            if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider);
            return obj.transform;
        }
        TextMeshPro WorldText(Transform parent, string text, Vector3 position, float size, Color color)
        {
            var obj = new GameObject("Value"); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            obj.transform.rotation = View.transform.rotation;
            var label = obj.AddComponent<TextMeshPro>(); label.text = text;
            label.font = Resources.Load<TMP_FontAsset>("SquadFont"); label.fontSize = size; label.color = color;
            if (outlinedText == null)
            {
                outlinedText = new Material(Resources.Load<Material>("SquadValue") ?? label.font.material); outlinedText.SetColor("_OutlineColor", new Color(0.025f, 0.04f, 0.07f));
                outlinedText.SetFloat("_OutlineWidth", 0.22f); outlinedText.EnableKeyword("OUTLINE_ON");
            }
            label.fontSharedMaterial = outlinedText; label.extraPadding = true;
            label.alignment = TextAlignmentOptions.Center; label.enableAutoSizing = false;
            label.rectTransform.sizeDelta = new Vector2(5, 2); return label;
        }
        void OnDestroy()
        {
            foreach (Material mat in materials.Values)
                if (mat != null) { if (Application.isPlaying) Destroy(mat); else DestroyImmediate(mat); }
            if (outlinedText != null) { if (Application.isPlaying) Destroy(outlinedText); else DestroyImmediate(outlinedText); }
            if (asphalt != null) { if (Application.isPlaying) Destroy(asphalt); else DestroyImmediate(asphalt); }
        }
    }
}
