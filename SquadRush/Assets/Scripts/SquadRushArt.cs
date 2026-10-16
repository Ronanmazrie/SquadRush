using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SquadRush
{
    // Colored parts are baked into shared meshes: each soldier uses three solid renderers.
    public sealed class SquadRushArt : MonoBehaviour
    {
        readonly Dictionary<string, Mesh> models = new Dictionary<string, Mesh>();
        readonly Dictionary<PrimitiveType, Mesh> primitives = new Dictionary<PrimitiveType, Mesh>();
        readonly List<Object> owned = new List<Object>();
        Material toon, glow, weathered;
        Camera view;
        static readonly Color Gear = new Color(0.085f, 0.13f, 0.19f);
        static readonly Color Steel = new Color(0.4f, 0.54f, 0.63f);
        static readonly Color Skin = new Color(1f, 0.75f, 0.49f);

        public void Initialize(Camera camera)
        {
            view = camera;
            toon = new Material(Shader.Find("SquadRush/Toon")) { name = "Toy soldier shading", enableInstancing = true };
            glow = new Material(Shader.Find("SquadRush/Glow")) { name = "Soft glows and contact shadows", enableInstancing = true };
            weathered = new Material(toon) { name = "Weathered stone wood and metal" };
            weathered.SetFloat("_Weathering", 1);
            var surfaceGrain = new Texture2D(128, 128, TextureFormat.RGB24, true) { name = "Shared prop wear", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear };
            var grainPixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                grainPixels[y * 128 + x] = new Color(Mathf.PerlinNoise(x / 23f, y / 23f), Mathf.PerlinNoise(x / 3f, y / 3f), 0);
            surfaceGrain.SetPixels(grainPixels); surfaceGrain.Apply(true, true);
            weathered.SetTexture("_SurfaceGrain", surfaceGrain); owned.Add(surfaceGrain);
            owned.Add(toon); owned.Add(glow); owned.Add(weathered);
        }
        public Transform Trooper(string name, Transform parent, bool red, bool boss = false)
        {
            var root = new GameObject(name).transform; root.SetParent(parent, false);
            var rig = root.gameObject.AddComponent<SquadRushTrooper>();
            string key = boss ? "warden" : red ? "red" : "blue";
            Color uniform = boss ? new Color(0.32f, 0.38f, 0.43f) : red ? new Color(0.91f, 0.15f, 0.22f) : new Color(0.035f, 0.55f, 0.98f);
            if (!models.TryGetValue(key, out Mesh body))
            {
                var b = new Recipe(this);
                b.Add(PrimitiveType.Sphere, new Vector3(0, 0.70f, 0), new Vector3(boss ? 0.68f : 0.37f, 0.55f, 0.30f), uniform);
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.73f, 0.135f), new Vector3(0.29f, 0.28f, 0.085f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.67f, -0.19f), new Vector3(0.30f, 0.35f, 0.16f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.72f, -0.285f), new Vector3(0.25f, 0.17f, 0.045f), uniform * 0.7f);
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.58f, -0.285f), new Vector3(0.23f, 0.07f, 0.04f), new Color(0.83f, 0.66f, 0.33f));
                for (int stripe = -1; stripe <= 1; stripe += 2)
                    b.Add(PrimitiveType.Cube, new Vector3(stripe * 0.085f, 0.72f, -0.313f), new Vector3(0.035f, 0.12f, 0.018f), Steel);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.10f, 0.78f, 0.19f), new Vector3(0.09f, 0.105f, 0.045f), Steel);
                    b.Add(PrimitiveType.Sphere, new Vector3(s * (boss ? 0.38f : 0.23f), 0.80f, 0.005f), new Vector3(boss ? 0.29f : 0.17f, 0.27f, 0.19f), uniform);
                    b.Add(PrimitiveType.Sphere, new Vector3(s * (boss ? 0.39f : 0.215f), 0.64f, 0.13f), new Vector3(0.13f, 0.26f, 0.14f), Skin, new Vector3(-32, 0, s * 8));
                    b.Add(PrimitiveType.Sphere, new Vector3(s * 0.18f, 0.57f, 0.26f), new Vector3(0.14f, 0.13f, 0.13f), Gear);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.135f, 0.50f, 0.10f), new Vector3(0.095f, 0.10f, 0.08f), Gear);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.24f, 0.83f, -0.025f), new Vector3(0.035f, 0.055f, 0.12f), new Color(1, 0.78f, 0.20f));
                }
                b.Add(PrimitiveType.Sphere, new Vector3(0, 1.03f, 0.015f), new Vector3(0.34f, 0.32f, 0.32f), boss ? new Color(0.71f, 0.77f, 0.66f) : Skin);
                b.Add(PrimitiveType.Sphere, new Vector3(0, 1.15f, -0.018f), new Vector3(0.45f, 0.25f, 0.43f), uniform);
                b.Add(PrimitiveType.Cylinder, new Vector3(0, 1.095f, 0.015f), new Vector3(0.46f, 0.024f, 0.44f), uniform);
                b.Add(PrimitiveType.Sphere, new Vector3(0, 1.23f, -0.035f), new Vector3(0.27f, 0.085f, 0.31f), uniform * 1.25f);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.16f, -0.221f), new Vector3(0.20f, 0.038f, 0.015f), new Color(0.78f, 0.94f, 1));
                b.Add(PrimitiveType.Cube, new Vector3(0.17f, 0.91f, -0.19f), new Vector3(0.026f, 0.30f, 0.026f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.055f, 0.169f), new Vector3(0.235f, 0.09f, 0.055f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.065f, 0.200f), new Vector3(0.17f, 0.036f, 0.018f), boss ? new Color(1, 0.56f, 0.05f) : new Color(0.23f, 0.81f, 0.95f));
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.96f, 0.188f), new Vector3(0.07f, 0.025f, 0.016f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0.15f, 0.61f, 0.34f), new Vector3(0.11f, 0.13f, 0.46f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0.15f, 0.68f, 0.28f), new Vector3(0.06f, 0.05f, 0.18f), Steel);
                b.Add(PrimitiveType.Cylinder, new Vector3(0.15f, 0.61f, 0.59f), new Vector3(0.075f, 0.045f, 0.075f), Steel, new Vector3(90, 0, 0));
                for (int rail = 0; rail < 3; rail++)
                    b.Add(PrimitiveType.Cube, new Vector3(0.15f, 0.69f, 0.39f + rail * 0.065f), new Vector3(0.14f, 0.035f, 0.025f), Steel);
                if (boss)
                {
                    b.Add(PrimitiveType.Cube, new Vector3(0, 0.74f, 0.20f), new Vector3(0.52f, 0.19f, 0.13f), new Color(0.91f, 0.23f, 0.12f));
                    for (int n = -1; n <= 1; n++) b.Add(PrimitiveType.Cube, new Vector3(n * 0.11f, 0.77f, 0.277f), new Vector3(0.05f, 0.12f, 0.03f), new Color(1, 0.67f, 0.08f));
                    for (int side = -1; side <= 1; side += 2)
                    {
                        b.Add(PrimitiveType.Cube, new Vector3(side * 0.42f, 0.87f, 0), new Vector3(0.28f, 0.16f, 0.34f), Gear, new Vector3(0, 0, side * 12));
                        b.Add(PrimitiveType.Cube, new Vector3(side * 0.42f, 0.96f, 0.02f), new Vector3(0.18f, 0.035f, 0.26f), new Color(1, 0.60f, 0.05f));
                    }
                }
                body = b.Bake(key); models.Add(key, body);
            }
            string legsKey = key + " legs";
            if (!models.TryGetValue(legsKey, out Mesh leg))
            {
                var b = new Recipe(this);
                b.Add(PrimitiveType.Sphere, new Vector3(0, -0.13f, 0), new Vector3(0.14f, 0.34f, 0.15f), uniform);
                b.Add(PrimitiveType.Cube, new Vector3(0, -0.30f, 0.045f), new Vector3(0.15f, 0.14f, 0.23f), Gear);
                b.Add(PrimitiveType.Cube, new Vector3(0, -0.12f, 0.085f), new Vector3(0.11f, 0.11f, 0.035f), Steel * 0.8f);
                leg = b.Bake(legsKey); models.Add(legsKey, leg);
            }
            rig.Body = ObjectMesh("Armored body", root, body, toon);
            rig.LeftLeg = ObjectMesh("Left boot", root, leg, toon); rig.LeftLeg.localPosition = new Vector3(-0.095f, 0.38f, 0);
            rig.RightLeg = ObjectMesh("Right boot", root, leg, toon); rig.RightLeg.localPosition = new Vector3(0.095f, 0.38f, 0);
            rig.ColoredRenderers = new[] { rig.Body.GetComponent<MeshRenderer>(), rig.LeftLeg.GetComponent<MeshRenderer>(), rig.RightLeg.GetComponent<MeshRenderer>() };
            var shadow = ObjectMesh("Contact shadow", root, Disc("shadow", new Color(0.025f, 0.035f, 0.065f, 0.35f)), glow);
            shadow.localPosition = new Vector3(0.05f, 0.035f, 0); shadow.localRotation = Quaternion.Euler(90, 0, 0); shadow.localScale = new Vector3(boss ? 1.2f : 0.62f, 0.43f, 1);
            rig.Muzzle = ObjectMesh("Muzzle flash", root, Star("muzzle", new Color(1, 0.68f, 0.12f, 0.7f)), glow);
            rig.Muzzle.localPosition = new Vector3(0.15f, 0.64f, 0.70f); rig.Muzzle.rotation = view.transform.rotation; rig.Muzzle.localScale = Vector3.one * 0.22f;
            if (!red)
            {
                const string moduleKey = "Upgraded rifle module";
                if (!models.TryGetValue(moduleKey, out Mesh module))
                {
                    var b = new Recipe(this);
                    b.Add(PrimitiveType.Cube, new Vector3(0.15f, 0.70f, 0.41f), new Vector3(0.17f, 0.055f, 0.35f), new Color(1, 0.67f, 0.12f));
                    b.Add(PrimitiveType.Cylinder, new Vector3(0.15f, 0.61f, 0.63f), new Vector3(0.12f, 0.065f, 0.12f), new Color(0.26f, 0.85f, 1), new Vector3(90, 0, 0));
                    module = b.Bake(moduleKey); models.Add(moduleKey, module);
                }
                rig.WeaponModule = ObjectMesh(moduleKey, rig.Body, module, toon); rig.WeaponModule.gameObject.SetActive(false);
            }
            rig.ResetPose(); return root;
        }
        public void Crate(Transform parent, SupplyReward reward)
        {
            string key = "supply crate " + reward;
            if (!models.TryGetValue(key, out Mesh mesh))
            {
                var b = new Recipe(this);
                Color wood = new Color(0.72f, 0.36f, 0.12f);
                Color accent = reward == SupplyReward.Power ? new Color(1, 0.68f, 0.08f) : reward == SupplyReward.RapidFire ? new Color(0.24f, 0.87f, 0.36f) : new Color(0.08f, 0.65f, 1);
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.76f, 0), new Vector3(1.62f, 1.5f, 1.25f), new Color(0.27f, 0.14f, 0.07f));
                for (int p = 0; p < 5; p++)
                {
                    b.Add(PrimitiveType.Cube, new Vector3(-0.64f + p * 0.32f, 0.76f, -0.65f), new Vector3(0.305f, 1.46f, 0.11f), wood * (p % 2 == 0 ? 1.1f : 0.92f));
                    b.Add(PrimitiveType.Cube, new Vector3(-0.64f + p * 0.32f, 1.53f, 0), new Vector3(0.305f, 0.10f, 1.30f), wood * 1.12f);
                }
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.55f, 0.76f, -0.728f), new Vector3(0.16f, 1.56f, 0.055f), Steel * 0.65f);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.55f, 1.59f, 0), new Vector3(0.16f, 0.04f, 1.38f), Steel);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 0.82f, 0.76f, 0), new Vector3(0.08f, 1.50f, 1.34f), wood * 0.7f);
                    for (int y = 0; y < 3; y++) b.Add(PrimitiveType.Sphere, new Vector3(s * 0.55f, 0.17f + y * 0.57f, -0.767f), Vector3.one * 0.08f, Steel);
                }
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.6f, 0), new Vector3(0.28f, 0.07f, 0.7f), accent);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.66f, 0), new Vector3(0.7f, 0.07f, 0.28f), accent);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.12f, -0.73f), new Vector3(0.8f, 0.17f, 0.045f), accent);
                mesh = b.Bake(key); models.Add(key, mesh);
            }
            ObjectMesh("Wooden planks and metal straps", parent, mesh, weathered);
        }
        public void Gate(Transform parent, float x, bool gain)
        {
            string key = gain ? "blue gate" : "red gate";
            Color color = gain ? new Color(0.04f, 0.64f, 1) : new Color(1, 0.12f, 0.20f);
            if (!models.TryGetValue(key, out Mesh mesh))
            {
                var b = new Recipe(this);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Add(PrimitiveType.Cube, new Vector3(s * 2.20f, 0.82f, 0), new Vector3(0.17f, 1.68f, 0.27f), color * 0.85f);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 2.20f, 0.12f, 0), new Vector3(0.40f, 0.22f, 0.55f), Gear);
                    b.Add(PrimitiveType.Cube, new Vector3(s * 2.20f, 1.63f, 0), new Vector3(0.25f, 0.09f, 0.34f), Color.white);
                    for (int lamp = 0; lamp < 3; lamp++)
                        b.Add(PrimitiveType.Cube, new Vector3(s * 2.2f, 0.57f + lamp * 0.3f, -0.15f), new Vector3(0.08f, 0.13f, 0.04f), Color.white);
                }
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.16f, 0), new Vector3(4.55f, 0.12f, 0.28f), color);
                b.Add(PrimitiveType.Cube, new Vector3(0, 1.60f, 0), new Vector3(4.55f, 0.075f, 0.20f), color * 1.2f);
                mesh = b.Bake(key); models.Add(key, mesh);
            }
            var frame = ObjectMesh(key, parent, mesh, toon); frame.localPosition = new Vector3(x, 0, 0);
            string panelKey = key + " light";
            if (!models.TryGetValue(panelKey, out Mesh panel))
            {
                panel = Quad(panelKey, 4.34f, 1.35f, new Color(color.r, color.g, color.b, 0.70f), new Color(color.r, color.g, color.b, 0.28f));
                models.Add(panelKey, panel);
            }
            var sheet = ObjectMesh("Energy panel", parent, panel, glow); sheet.localPosition = new Vector3(x, 0.88f, -0.03f);
            string gleamKey = key + " reflection";
            if (!models.TryGetValue(gleamKey, out Mesh gleam))
            {
                gleam = Quad(gleamKey, 0.32f, 1.26f, new Color(1, 1, 1, 0.02f), new Color(1, 1, 1, 0.32f)); models.Add(gleamKey, gleam);
            }
            var reflection = ObjectMesh("Glass reflection", parent, gleam, glow); reflection.localPosition = new Vector3(x - 1.2f, 0.9f, -0.06f); reflection.localRotation = Quaternion.Euler(0, 0, -22);
            var halo = ObjectMesh("Gate floor glow", parent, Disc("gate halo " + gain, new Color(color.r, color.g, color.b, 0.21f)), glow);
            halo.localPosition = new Vector3(x, 0.04f, 0); halo.localRotation = Quaternion.Euler(90, 0, 0); halo.localScale = new Vector3(4.5f, 1.25f, 1);
        }
        public Transform SceneryObject(Transform parent) => ObjectMesh("Landscape prop", parent, null, weathered);
        public Mesh LandscapeModel(LandscapeBiome biome, int kind)
        {
            string key = "Landscape " + biome + " " + kind;
            if (models.TryGetValue(key, out Mesh existing)) return existing;
            if (biome != LandscapeBiome.Desert) return WoodlandModel(biome, kind, key);
            // Each variant is baked once; the generator changes only transforms and mesh references.
            var rock = new Recipe(this);
            if (kind == 0)
            {
            rock.Add(PrimitiveType.Cube, new Vector3(0, 0.35f, 0), new Vector3(1.3f, 0.7f, 0.95f), new Color(0.66f, 0.38f, 0.22f), new Vector3(0, 22, 9));
            rock.Add(PrimitiveType.Cube, new Vector3(0.1f, 0.84f, 0.07f), new Vector3(0.92f, 0.44f, 0.72f), new Color(0.84f, 0.56f, 0.32f), new Vector3(0, -8, -5));
            rock.Add(PrimitiveType.Cube, new Vector3(-0.6f, 0.2f, 0.4f), new Vector3(0.58f, 0.4f, 0.64f), new Color(0.71f, 0.43f, 0.24f), new Vector3(5, 35, -9));
                for (int layer = 0; layer < 3; layer++)
                    rock.Add(PrimitiveType.Cube, new Vector3(0, 0.18f + layer * 0.17f, -0.465f), new Vector3(1.08f - layer * 0.14f, 0.025f, 0.025f), new Color(0.50f + layer * 0.06f, 0.29f + layer * 0.04f, 0.18f), new Vector3(0, 22, 9));
                var mesh = rock.Bake(key); models.Add(key, mesh); return mesh;
            }
            var cactus = new Recipe(this);
            if (kind == 1)
            {
            Color green = new Color(0.19f, 0.42f, 0.28f);
            cactus.Add(PrimitiveType.Cylinder, new Vector3(0, 0.85f, 0), new Vector3(0.34f, 0.85f, 0.34f), green);
            cactus.Add(PrimitiveType.Sphere, new Vector3(0, 1.68f, 0), new Vector3(0.34f, 0.34f, 0.34f), green);
            for (int side = -1; side <= 1; side += 2)
            {
                float height = side < 0 ? 0.9f : 1.2f;
                cactus.Add(PrimitiveType.Cube, new Vector3(side * 0.26f, height - 0.24f, 0), new Vector3(0.53f, 0.22f, 0.24f), green);
                cactus.Add(PrimitiveType.Cylinder, new Vector3(side * 0.47f, height, 0), new Vector3(0.24f, 0.32f, 0.24f), green);
                cactus.Add(PrimitiveType.Sphere, new Vector3(side * 0.47f, height + 0.3f, 0), Vector3.one * 0.24f, green);
            }
            for (int rib = -1; rib <= 1; rib++)
                cactus.Add(PrimitiveType.Cube, new Vector3(rib * 0.09f, 0.85f, -0.16f), new Vector3(0.018f, 1.4f, 0.025f), new Color(0.42f, 0.61f, 0.33f));
                var mesh = cactus.Bake(key); models.Add(key, mesh); return mesh;
            }
            var marker = new Recipe(this);
            if (kind == 2)
            {
            marker.Add(PrimitiveType.Cube, new Vector3(0, 0.23f, 0), new Vector3(0.30f, 0.46f, 1.65f), new Color(0.18f, 0.24f, 0.28f));
            for (int stripe = 0; stripe < 4; stripe++)
                marker.Add(PrimitiveType.Cube, new Vector3(0, 0.48f, -0.62f + stripe * 0.41f), new Vector3(0.32f, 0.04f, 0.19f), new Color(1, 0.70f, 0.20f));
                var mesh = marker.Bake(key); models.Add(key, mesh); return mesh;
            }
            var light = new Recipe(this);
            if (kind == 3)
            {
            light.Add(PrimitiveType.Cylinder, new Vector3(0, 1.4f, 0), new Vector3(0.075f, 1.4f, 0.075f), Steel * 0.65f);
            light.Add(PrimitiveType.Cube, new Vector3(-0.3f, 2.75f, 0), new Vector3(0.65f, 0.07f, 0.12f), Steel);
            light.Add(PrimitiveType.Cube, new Vector3(-0.53f, 2.66f, 0), new Vector3(0.32f, 0.11f, 0.28f), Gear);
            light.Add(PrimitiveType.Cube, new Vector3(-0.53f, 2.59f, 0), new Vector3(0.24f, 0.025f, 0.21f), new Color(1, 0.89f, 0.56f));
                var mesh = light.Bake(key); models.Add(key, mesh); return mesh;
            }
            var ruins = new Recipe(this);
            for (int i = 0; i < 3; i++)
            {
                ruins.Add(PrimitiveType.Cube, new Vector3(i * 0.45f - 0.45f, 0.28f + i % 2 * 0.15f, 0), new Vector3(0.42f, 0.55f + i % 2 * 0.3f, 0.65f), new Color(0.75f, 0.55f, 0.35f), new Vector3(0, i * 13, i * 3));
            }
            Mesh ruinMesh = ruins.Bake(key); models.Add(key, ruinMesh); return ruinMesh;
        }
        Mesh WoodlandModel(LandscapeBiome biome, int kind, string key)
        {
            bool snow = biome == LandscapeBiome.Snow;
            var b = new Recipe(this);
            Color stone = snow ? new Color(0.49f, 0.64f, 0.76f) : new Color(0.42f, 0.49f, 0.40f);
            Color foliage = snow ? new Color(0.19f, 0.40f, 0.38f) : new Color(0.17f, 0.40f, 0.19f);
            if (kind == 0 || kind == 1)
            {
                b.Add(PrimitiveType.Cylinder, new Vector3(0, 0.9f, 0), new Vector3(0.22f, 0.9f, 0.22f), new Color(0.37f, 0.24f, 0.14f));
                for (int rib = 0; rib < 5; rib++)
                {
                    float a = rib * Mathf.PI * 0.4f;
                    b.Add(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 0.104f, 0.65f, Mathf.Sin(a) * 0.104f), new Vector3(0.025f, 1.1f - rib * 0.08f, 0.027f), new Color(0.23f, 0.16f, 0.10f));
                }
                b.Add(PrimitiveType.Sphere, new Vector3(0, 0.05f, 0), new Vector3(0.65f, 0.12f, 0.6f), snow ? new Color(0.70f, 0.82f, 0.87f) : new Color(0.27f, 0.34f, 0.16f));
                if (kind == 0)
                {
                    for (int layer = 0; layer < 3; layer++)
                    {
                        float size = 1.5f - layer * 0.35f;
                        // Flattened overlapping foliage gives a distinct conifer silhouette at mobile scale.
                        b.Add(PrimitiveType.Sphere, new Vector3(0, 1.1f + layer * 0.5f, 0), new Vector3(size, 1.05f, size), foliage * (1 + layer * 0.1f));
                        if (snow) b.Add(PrimitiveType.Sphere, new Vector3(0, 1.42f + layer * 0.5f, 0), new Vector3(size * 0.83f, 0.42f, size * 0.83f), new Color(0.9f, 0.96f, 1));
                    }
                }
                else
                {
                    for (int lobe = 0; lobe < 3; lobe++)
                        b.Add(PrimitiveType.Sphere, new Vector3((lobe - 1) * 0.45f, 1.65f + lobe % 2 * 0.35f, 0), new Vector3(1.1f, 1.3f, 1.15f), snow ? new Color(0.84f, 0.93f, 0.97f) : foliage * (1 + lobe * 0.12f));
                }
            }
            else if (kind == 2)
            {
                b.Add(PrimitiveType.Cube, new Vector3(0, 0.5f, 0), new Vector3(1.25f, 0.95f, 1), stone, new Vector3(7, 22, 9));
                b.Add(PrimitiveType.Cube, new Vector3(-0.6f, 0.23f, 0.3f), new Vector3(0.65f, 0.46f, 0.64f), stone * 0.85f, new Vector3(-4, 38, -8));
                if (snow) b.Add(PrimitiveType.Sphere, new Vector3(0, 0.96f, 0), new Vector3(1.3f, 0.25f, 1.1f), Color.white);
                else
                {
                    b.Add(PrimitiveType.Sphere, new Vector3(-0.26f, 0.94f, 0.1f), new Vector3(0.75f, 0.065f, 0.66f), new Color(0.31f, 0.42f, 0.19f));
                    b.Add(PrimitiveType.Sphere, new Vector3(-0.6f, 0.43f, 0.3f), new Vector3(0.44f, 0.07f, 0.38f), new Color(0.40f, 0.49f, 0.23f));
                }
            }
            else if (kind == 3)
            {
                b.Add(PrimitiveType.Cylinder, new Vector3(0, 0.25f, 0), new Vector3(0.45f, 0.9f, 0.45f), new Color(0.42f, 0.29f, 0.18f), new Vector3(0, 0, 90));
                b.Add(PrimitiveType.Cylinder, new Vector3(-0.91f, 0.25f, 0), new Vector3(0.34f, 0.012f, 0.34f), new Color(0.74f, 0.59f, 0.35f), new Vector3(0, 0, 90));
                b.Add(PrimitiveType.Cylinder, new Vector3(-0.926f, 0.25f, 0), new Vector3(0.22f, 0.014f, 0.22f), new Color(0.57f, 0.43f, 0.26f), new Vector3(0, 0, 90));
                b.Add(PrimitiveType.Cylinder, new Vector3(-0.943f, 0.25f, 0), new Vector3(0.16f, 0.014f, 0.16f), new Color(0.74f, 0.59f, 0.35f), new Vector3(0, 0, 90));
                if (snow) b.Add(PrimitiveType.Cube, new Vector3(0, 0.46f, 0), new Vector3(1.65f, 0.08f, 0.3f), Color.white);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                    b.Add(PrimitiveType.Sphere, new Vector3((i - 1) * 0.35f, 0.22f, i % 2 * 0.18f), new Vector3(0.7f, 0.6f, 0.7f), snow ? new Color(0.82f, 0.92f, 1) : new Color(0.33f, 0.52f, 0.20f));
            }
            Mesh mesh = b.Bake(key); models.Add(key, mesh); return mesh;
        }
        public Transform Projectile(Transform parent)
        {
            if (!models.TryGetValue("tracer", out Mesh mesh))
            {
                mesh = Quad("Tracer", 0.15f, 1.1f, new Color(0.3f, 0.85f, 1, 0), new Color(0.3f, 0.85f, 1, 0.28f));
                // A white core remains legible against asphalt; the outer ribbon supplies a cyan halo.
                var vertices = new List<Vector3>(mesh.vertices); var colors = new List<Color>(mesh.colors); var triangles = new List<int>(mesh.triangles);
                AddQuad(vertices, colors, triangles, 0.045f, 0.75f, new Color(1, 1, 0.88f, 1), new Color(0.48f, 0.88f, 1, 0.80f));
                mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); models.Add("tracer", mesh);
            }
            var body = ObjectMesh("Tracer", parent, mesh, glow); body.localRotation = Quaternion.Euler(90, 0, 0); body.gameObject.SetActive(false); return body;
        }
        public Transform Impact(Transform parent)
        {
            var body = ObjectMesh("Impact burst", parent, Star("impact", new Color(1, 0.70f, 0.16f, 0.85f)), glow);
            body.rotation = view.transform.rotation; body.gameObject.SetActive(false); return body;
        }
        Mesh Star(string key, Color color)
        {
            if (models.TryGetValue(key, out Mesh existing)) return existing;
            var vertices = new List<Vector3> { Vector3.zero }; var colors = new List<Color> { new Color(1, 1, 0.93f, 1) }; var triangles = new List<int>();
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI / 8; float radius = i % 2 == 0 ? 1.1f : 0.38f;
                vertices.Add(new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius); colors.Add(i % 2 == 0 ? new Color(color.r, color.g, color.b, 0) : color);
            }
            for (int i = 0; i < 16; i++) { triangles.Add(0); triangles.Add(i + 1); triangles.Add((i + 1) % 16 + 1); }
            var mesh = NewMesh(key, vertices, colors, triangles); models.Add(key, mesh); return mesh;
        }
        Mesh Disc(string key, Color color)
        {
            if (models.TryGetValue(key, out Mesh existing)) return existing;
            var vertices = new List<Vector3> { Vector3.zero }; var colors = new List<Color> { color }; var triangles = new List<int>();
            for (int i = 0; i < 24; i++) { float a = i * Mathf.PI / 12; vertices.Add(new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * 0.5f); colors.Add(new Color(color.r, color.g, color.b, 0)); }
            for (int i = 0; i < 24; i++) { triangles.Add(0); triangles.Add(i + 1); triangles.Add((i + 1) % 24 + 1); }
            var mesh = NewMesh(key, vertices, colors, triangles); models.Add(key, mesh); return mesh;
        }
        Mesh Quad(string name, float width, float height, Color bottom, Color top)
        {
            var v = new List<Vector3>(); var c = new List<Color>(); var t = new List<int>(); AddQuad(v,c,t,width,height,bottom,top); return NewMesh(name,v,c,t);
        }
        static void AddQuad(List<Vector3> v, List<Color> c, List<int> t, float width, float height, Color bottom, Color top)
        {
            int start=v.Count;
            v.Add(new Vector3(-width/2,-height/2,0)); v.Add(new Vector3(width/2,-height/2,0)); v.Add(new Vector3(width/2,height/2,0)); v.Add(new Vector3(-width/2,height/2,0));
            c.Add(bottom); c.Add(bottom); c.Add(top); c.Add(top);
            t.Add(start); t.Add(start+1); t.Add(start+2); t.Add(start); t.Add(start+2); t.Add(start+3);
        }
        Mesh NewMesh(string name, List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            var mesh = new Mesh { name=name }; mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles,0); mesh.RecalculateBounds(); owned.Add(mesh); return mesh;
        }
        Transform ObjectMesh(string name, Transform parent, Mesh mesh, Material material)
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(parent, false);
            obj.GetComponent<MeshFilter>().sharedMesh=mesh; var renderer=obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial=material;
            bool solid = material == toon || material == weathered;
            renderer.shadowCastingMode=solid ? ShadowCastingMode.On : ShadowCastingMode.Off; renderer.receiveShadows=solid; return obj.transform;
        }
        Mesh Primitive(PrimitiveType kind)
        {
            if (primitives.TryGetValue(kind,out Mesh mesh)) return mesh;
            if (kind == PrimitiveType.Sphere)
            {
                // Smooth normals retain the toy-like highlights with fewer vertices than built-in spheres.
                const int rings = 12, slices = 16;
                var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
                for (int ring = 0; ring <= rings; ring++) for (int slice = 0; slice <= slices; slice++)
                {
                    float latitude = ring * Mathf.PI / rings, longitude = slice * Mathf.PI * 2 / slices;
                    Vector3 normal = new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude));
                    vertices.Add(normal * 0.5f); normals.Add(normal); colors.Add(Color.white);
                    if (ring == rings || slice == slices) continue;
                    int a = ring * (slices + 1) + slice, b = a + slices + 1;
                    triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                    triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
                }
                mesh = NewMesh("Shared smooth sphere", vertices, colors, triangles); mesh.SetNormals(normals); primitives.Add(kind, mesh); return mesh;
            }
            var obj=GameObject.CreatePrimitive(kind); mesh=obj.GetComponent<MeshFilter>().sharedMesh; obj.SetActive(false); Release(obj); primitives.Add(kind,mesh); return mesh;
        }
        sealed class Recipe
        {
            readonly SquadRushArt art; readonly List<CombineInstance> parts = new List<CombineInstance>(); readonly List<Mesh> temporary = new List<Mesh>();
            public Recipe(SquadRushArt owner) { art=owner; }
            public void Add(PrimitiveType kind, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
            {
                Mesh mesh=Instantiate(art.Primitive(kind)); var colors=new Color[mesh.vertexCount]; for(int i=0;i<colors.Length;i++) colors[i]=color;
                mesh.colors=colors; temporary.Add(mesh); parts.Add(new CombineInstance { mesh=mesh,transform=Matrix4x4.TRS(position,Quaternion.Euler(rotation),scale) });
            }
            public Mesh Bake(string name)
            {
                var mesh=new Mesh { name=name }; mesh.CombineMeshes(parts.ToArray(),true,true); mesh.RecalculateBounds(); art.owned.Add(mesh);
                foreach(var part in temporary) Release(part); return mesh;
            }
        }
        static void Release(Object obj) { if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj); }
        void OnDestroy() { foreach(var asset in owned) if(asset!=null) Release(asset); }
    }
}
