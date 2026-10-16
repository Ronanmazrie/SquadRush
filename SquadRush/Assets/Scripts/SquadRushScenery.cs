using System.Collections.Generic;
using UnityEngine;

namespace SquadRush
{
    public sealed class SquadRushScenery : MonoBehaviour
    {
        const float Length = 72;
        sealed class Slot { public Transform Root; public int Side; public bool Distant; }
        readonly List<Slot> slots = new List<Slot>();
        SquadRushArt art;
        System.Random random;
        LandscapeBiome biome;
        public int Seed { get; private set; }
        public int RecycledCount { get; private set; }
        public int Capacity => slots.Count;

        public void Initialize(SquadRushArt owner, Transform parent)
        {
            art = owner;
            for (int row = 0; row < 12; row++) for (int side = -1; side <= 1; side += 2) for (int band = 0; band < 2; band++)
                slots.Add(new Slot { Root = art.SceneryObject(parent), Side = side, Distant = band == 1 });
        }
        public void Generate(LandscapeBiome theme, int seed)
        {
            Seed = seed; biome = theme; random = new System.Random(seed); RecycledCount = 0;
            for (int i = 0; i < slots.Count; i++) Reroll(slots[i], -10 + i / 4 * 6 + Next(0, 2));
        }
        public void Scroll(float dt)
        {
            foreach (var slot in slots)
            {
                slot.Root.localPosition += Vector3.back * (dt * 3.5f);
                if (slot.Root.localPosition.z >= -12) continue;
                float z = slot.Root.localPosition.z;
                while (z < -12) z += Length;
                Reroll(slot, z); RecycledCount++;
            }
        }
        void Reroll(Slot slot, float z)
        {
            int kind = random.Next(5);
            slot.Root.GetComponent<MeshFilter>().sharedMesh = art.LandscapeModel(biome, kind);
            slot.Root.name = biome + " " + kind;
            slot.Root.localPosition = new Vector3(slot.Side * (slot.Distant ? Next(9, 13) : Next(6.2f, 8.1f)), 0, z);
            slot.Root.localRotation = Quaternion.Euler(0, Next(0, 360), 0);
            float scale = Next(0.7f, 1.35f) * (slot.Distant ? 1.6f : 1);
            slot.Root.localScale = new Vector3(scale, scale * Next(0.85f, 1.4f), scale);
            slot.Root.gameObject.SetActive(!slot.Distant || random.NextDouble() > 0.30);
        }
        float Next(float min, float max) => min + (float)random.NextDouble() * (max - min);
        public int LayoutFingerprint()
        {
            unchecked
            {
                int hash = 17;
                foreach (var slot in slots)
                {
                    hash = hash * 31 + slot.Root.localPosition.GetHashCode();
                    hash = hash * 31 + slot.Root.localScale.GetHashCode();
                    hash = hash * 31 + slot.Root.localRotation.GetHashCode();
                    hash = hash * 31 + slot.Root.GetComponent<MeshFilter>().sharedMesh.name.GetHashCode();
                    hash = hash * 31 + (slot.Root.gameObject.activeSelf ? 1 : 0);
                }
                return hash;
            }
        }
    }
}
