using System;
using UnityEngine;

namespace SquadRush
{
    public enum EquipmentSlot { Weapon, Armor, Support }

    public readonly struct EquipmentItem
    {
        public readonly int Id, Value, RewardStage;
        public readonly string Name;
        public readonly EquipmentSlot Slot;
        public EquipmentItem(int id, string name, EquipmentSlot slot, int value, int rewardStage)
        { Id = id; Name = name; Slot = slot; Value = value; RewardStage = rewardStage; }
        public string Effect => Slot == EquipmentSlot.Weapon ? "弾の威力 +" + Value : Slot == EquipmentSlot.Armor ? "初期の仲間 +" + Value : "連射速度 +" + Value + "%";
        public string SlotName => Slot == EquipmentSlot.Weapon ? "武器" : Slot == EquipmentSlot.Armor ? "防具" : "サポート";
    }

    // Acquisition is shared by stage rewards and future reward sources; no gacha is implemented.
    public sealed class SquadRushEquipment
    {
        static readonly EquipmentItem[] catalog =
        {
            new EquipmentItem(0, "スカウトライフル", EquipmentSlot.Weapon, 1, 1),
            new EquipmentItem(1, "レンジャーベスト", EquipmentSlot.Armor, 2, 2),
            new EquipmentItem(2, "クイックマガジン", EquipmentSlot.Support, 10, 3),
            new EquipmentItem(3, "アサルトライフル", EquipmentSlot.Weapon, 2, 4),
            new EquipmentItem(4, "ガーディアンベスト", EquipmentSlot.Armor, 4, 5),
            new EquipmentItem(5, "高速マガジン", EquipmentSlot.Support, 20, 6)
        };
        [Serializable] sealed class SaveData
        {
            public int version = 1, owned;
            public int weapon = -1, armor = -1, support = -1;
        }
        SaveData data;
        readonly string key;
        public static int Count => catalog.Length;
        public static EquipmentItem Item(int id) => catalog[id];
        public static EquipmentItem StageReward(int stage) => catalog[(Mathf.Max(1, stage) - 1) % catalog.Length];
        public static string StorageKey(string progressKey) => progressKey + ".Equipment.v1";
        public SquadRushEquipment(string progressKey = SquadRushProgress.DefaultKey, int completedStages = 0, bool saveMigration = true)
        {
            key = StorageKey(progressKey);
            bool existed = PlayerPrefs.HasKey(key);
            try { data = existed ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key)) : new SaveData(); }
            catch (ArgumentException) { data = new SaveData(); }
            if (data == null || data.version != 1) data = new SaveData();
            data.owned &= (1 << Count) - 1;
            data.weapon = Validate(data.weapon, EquipmentSlot.Weapon);
            data.armor = Validate(data.armor, EquipmentSlot.Armor);
            data.support = Validate(data.support, EquipmentSlot.Support);
            // Existing players keep progress and receive rewards for stages already cleared.
            if (!existed && completedStages > 0)
            {
                for (int stage = 1; stage <= Mathf.Min(Count, completedStages); stage++) data.owned |= 1 << StageReward(stage).Id;
                if (saveMigration) Save();
            }
        }
        int Validate(int id, EquipmentSlot slot) => Owns(id) && Item(id).Slot == slot ? id : -1;
        public bool Owns(int id) => id >= 0 && id < Count && (data.owned & (1 << id)) != 0;
        public int Equipped(EquipmentSlot slot) => slot == EquipmentSlot.Weapon ? data.weapon : slot == EquipmentSlot.Armor ? data.armor : data.support;
        public bool IsEquipped(int id) => Owns(id) && Equipped(Item(id).Slot) == id;
        public int OwnedCount { get { int total = 0; for (int i = 0; i < Count; i++) if (Owns(i)) total++; return total; } }
        public int WeaponPower => Bonus(EquipmentSlot.Weapon);
        public int StartingAllies => Bonus(EquipmentSlot.Armor);
        public float FireRateBonus => Bonus(EquipmentSlot.Support) / 100f;
        int Bonus(EquipmentSlot slot) { int id = Equipped(slot); return Owns(id) ? Item(id).Value : 0; }
        public bool Acquire(int id)
        {
            if (id < 0 || id >= Count || Owns(id)) return false;
            data.owned |= 1 << id; Save(); return true;
        }
        public bool Toggle(int id)
        {
            if (!Owns(id)) return false;
            EquipmentSlot slot = Item(id).Slot;
            int value = IsEquipped(id) ? -1 : id;
            if (slot == EquipmentSlot.Weapon) data.weapon = value;
            else if (slot == EquipmentSlot.Armor) data.armor = value;
            else data.support = value;
            Save(); return true;
        }
        void Save() { PlayerPrefs.SetString(key, JsonUtility.ToJson(data)); PlayerPrefs.Save(); }
    }
}
