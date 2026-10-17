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
            public int version = 2, owned;
            public int[] quantities;
            public string lastDrawTransaction;
            public int weapon = -1, armor = -1, support = -1;
        }
        SaveData data;
        readonly string key;
        public static int Count => catalog.Length;
        public const int RarityCount = 5, MergeCost = 3;
        public static int EntryCount => Count * RarityCount;
        public static string RarityName(int rarity) => new[] { "ノーマル", "グッド", "レア", "エピック", "レジェンド" }[Mathf.Clamp(rarity, 0, RarityCount-1)];
        public static int Value(int id, int rarity) => Item(id).Value * new[] { 1, 2, 3, 5, 8 }[Mathf.Clamp(rarity, 0, RarityCount-1)];
        public static string Effect(int id, int rarity) { var item=Item(id); return (item.Slot==EquipmentSlot.Weapon?"弾の威力 +":item.Slot==EquipmentSlot.Armor?"初期の仲間 +":"連射速度 +")+Value(id,rarity)+(item.Slot==EquipmentSlot.Support?"%":""); }
        public static EquipmentItem Item(int id) => catalog[id];
        public static EquipmentItem StageReward(int stage) => catalog[(Mathf.Max(1, stage) - 1) % catalog.Length];
        public static string StorageKey(string progressKey) => progressKey + ".Equipment.v1";
        public SquadRushEquipment(string progressKey = SquadRushProgress.DefaultKey, int completedStages = 0, bool saveMigration = true)
        {
            key = StorageKey(progressKey);
            bool existed = PlayerPrefs.HasKey(key);
            try { data = existed ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key)) : new SaveData(); }
            catch (ArgumentException) { data = new SaveData(); }
            if (data == null || (data.version != 1 && data.version != 2)) data = new SaveData();
            bool migrate=data.version==1;
            if(migrate || data.quantities==null || data.quantities.Length!=EntryCount)
            {
                data.quantities=new int[EntryCount];
                for(int id=0;id<Count;id++) if((data.owned&(1<<id))!=0) data.quantities[id]=1;
            }
            for(int i=0;i<EntryCount;i++) data.quantities[i]=Mathf.Clamp(data.quantities[i],0,999999);
            data.version=2; data.owned=0;
            data.weapon = Validate(data.weapon, EquipmentSlot.Weapon);
            data.armor = Validate(data.armor, EquipmentSlot.Armor);
            data.support = Validate(data.support, EquipmentSlot.Support);
            // Existing players keep progress and receive rewards for stages already cleared.
            if (!existed && completedStages > 0)
            {
                for (int stage = 1; stage <= Mathf.Min(Count, completedStages); stage++) data.quantities[StageReward(stage).Id]=1;
            }
            if(saveMigration && (migrate || (!existed && completedStages>0))) Save();
        }
        int Validate(int entry, EquipmentSlot slot) => entry>=0 && entry<EntryCount && Quantity(entry%Count,entry/Count)>0 && Item(entry%Count).Slot==slot ? entry : -1;
        public int Quantity(int id,int rarity=0) => id>=0&&id<Count&&rarity>=0&&rarity<RarityCount?data.quantities[id+rarity*Count]:0;
        public bool Owns(int id) { for(int r=0;r<RarityCount;r++) if(Quantity(id,r)>0)return true; return false; }
        int EquippedEntry(EquipmentSlot slot) => slot == EquipmentSlot.Weapon ? data.weapon : slot == EquipmentSlot.Armor ? data.armor : data.support;
        public int Equipped(EquipmentSlot slot) { int entry=EquippedEntry(slot);return entry<0?-1:entry%Count; }
        public int EquippedRarity(EquipmentSlot slot) { int entry=EquippedEntry(slot);return entry<0?0:entry/Count; }
        public bool IsEquipped(int id) => Owns(id) && Equipped(Item(id).Slot) == id;
        public bool IsEquipped(int id,int rarity) => IsEquipped(id)&&EquippedRarity(Item(id).Slot)==rarity;
        public int OwnedCount { get { int total=0;foreach(int quantity in data.quantities)total+=quantity;return total; } }
        public int WeaponPower => Bonus(EquipmentSlot.Weapon);
        public int StartingAllies => Bonus(EquipmentSlot.Armor);
        public float FireRateBonus => Bonus(EquipmentSlot.Support) / 100f;
        int Bonus(EquipmentSlot slot) { int id = Equipped(slot); return Owns(id) ? Value(id,EquippedRarity(slot)) : 0; }
        public bool Acquire(int id,int rarity=0)
        {
            if(id<0||id>=Count||rarity<0||rarity>=RarityCount||Quantity(id,rarity)>=999999)return false;
            data.quantities[id+rarity*Count]++; Save();return true;
        }
        public void GrantDraw(string transaction,int[] weapons)
        {
            if(string.IsNullOrEmpty(transaction)||data.lastDrawTransaction==transaction)return;
            foreach(int id in weapons)if(id>=0&&id<Count)data.quantities[id]=Mathf.Min(999999,data.quantities[id]+1);
            data.lastDrawTransaction=transaction;Save();
        }
        public bool Toggle(int id,int rarity=0)
        {
            if (Quantity(id,rarity)<1) return false;
            EquipmentSlot slot = Item(id).Slot;
            int value = IsEquipped(id,rarity) ? -1 : id+rarity*Count;
            SetEquipped(slot,value);Save();return true;
        }
        void SetEquipped(EquipmentSlot slot,int value)
        {
            if (slot == EquipmentSlot.Weapon) data.weapon = value;
            else if (slot == EquipmentSlot.Armor) data.armor = value;
            else data.support = value;
        }
        public bool CanMerge(int id,int rarity) => rarity>=0&&rarity<RarityCount-1&&Quantity(id,rarity)>=MergeCost&&Quantity(id,rarity+1)<999999;
        public bool Merge(int id,int rarity)
        {
            if(!CanMerge(id,rarity))return false;
            bool equipped=IsEquipped(id,rarity);
            data.quantities[id+rarity*Count]-=MergeCost;data.quantities[id+(rarity+1)*Count]++;
            if(equipped)SetEquipped(Item(id).Slot,id+(rarity+1)*Count);
            Save();return true;
        }
        void Save() { PlayerPrefs.SetString(key, JsonUtility.ToJson(data)); PlayerPrefs.Save(); }
    }
}
