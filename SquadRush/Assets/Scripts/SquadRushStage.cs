using UnityEngine;

namespace SquadRush
{
    public enum LandscapeBiome { Desert, Forest, Snow }

    public readonly struct SquadRushStage
    {
        public readonly int Number;
        public SquadRushStage(int number) { Number = Mathf.Max(1, number); }
        public int Difficulty => Mathf.Min(Number - 1, 20);
        public LandscapeBiome Biome => (LandscapeBiome)((Number - 1) % 3);
        public string Name => Biome == LandscapeBiome.Desert ? "砂漠" : Biome == LandscapeBiome.Forest ? "森林" : "雪原";
        public float Duration => 40 + Mathf.Min(Difficulty, 6) * 3;
        public int StartingAllies => 3 + Mathf.Min(Difficulty / 3, 5);
        public int BossHealth => 180 + Difficulty * 50;
        public int EnemyHealth => 3 + Mathf.Min(Difficulty / 4, 3);
        public float WaveInterval(float progress) => Mathf.Max(0.9f, Mathf.Lerp(2.6f, 1.5f, progress) / (1 + Difficulty * 0.045f));
        public Color Ground => Biome == LandscapeBiome.Desert ? new Color(0.76f, 0.62f, 0.40f) : Biome == LandscapeBiome.Forest ? new Color(0.28f, 0.43f, 0.25f) : new Color(0.83f, 0.9f, 0.96f);
        public Color Fog => Biome == LandscapeBiome.Desert ? new Color(0.78f, 0.71f, 0.60f) : Biome == LandscapeBiome.Forest ? new Color(0.57f, 0.69f, 0.59f) : new Color(0.78f, 0.87f, 0.96f);
    }

    // Store only the next stage, independently of any particular attempt or random landscape.
    public sealed class SquadRushProgress
    {
        public const string DefaultKey = "SquadRush.ResumeStage.v1";
        readonly string key;
        public SquadRushProgress(string storageKey = DefaultKey) { key = storageKey; }
        public int ResumeStage => Mathf.Clamp(PlayerPrefs.GetInt(key, 1), 1, 999999);
        public void Complete(int stage)
        {
            int next = Mathf.Min(999999, stage + 1);
            if (next <= ResumeStage) return;
            PlayerPrefs.SetInt(key, next);
            PlayerPrefs.Save();
        }
    }
}
