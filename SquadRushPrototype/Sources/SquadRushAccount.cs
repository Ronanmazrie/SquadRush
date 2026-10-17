using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadRush
{
    public enum DrawKind { Weapon, Coins, Character }
    public enum DrawBanner { Permanent, AkaneEvent }
    [Serializable] public sealed class SquadRushDrawResult
    {
        public DrawKind kind;
        public int id,amount;
        public bool isNew;
        public string Label => kind==DrawKind.Character?SquadRushCharacters.Name(id)+(isNew?" / NEW":" / コイン1000"):kind==DrawKind.Weapon?SquadRushEquipment.Item(id).Name+" ×1":"コイン ×"+amount;
    }
    public static class SquadRushCharacters
    {
        public const int Count=3;
        static readonly string[] names={"レン","アカネ","ミオ"};
        static readonly string[] skills={"応急補給","爆裂掃射","電光連射"};
        static readonly string[] descriptions={"仲間を3人補給する\n再使用まで12秒","敵全体に威力依存の範囲攻撃\n再使用まで16秒","6秒間、連射速度を50%強化\n再使用まで18秒"};
        public static string Name(int id)=>names[id];
        public static string Skill(int id)=>skills[id];
        public static string Description(int id)=>descriptions[id];
        public static float Cooldown(int id)=>id==0?12:id==1?16:18;
        public static Color Uniform(int id)=>id==1?new Color(1,.24f,.16f,1):id==2?new Color(.22f,.92f,.74f,1):new Color(1,1,1,0);
    }

    // Offline prototype economy. All acquisition, spending, pity and reward claims share one save.
    public sealed class SquadRushAccount
    {
        public const int GemCost=100,WeaponCoinValue=200,DuplicateCoins=1000,ReleaseCampaignTickets=10;
        const int WalletLimit=9999999;
        [Serializable] sealed class SaveData
        {
            public int version=1,gems,tickets,coins,pity,totalPulls,characters=1,selectedCharacter,bestClear;
            public int observedDay=-1,lastLoginDay=-1,loginDays,dailyClears,dailyKills,dailyPulls,dailyClaims,permanentClaims;
            public string pendingTransaction;
            public int[] pendingWeapons;
            public int pendingUpgradeItem,pendingUpgradeLevel;
            public SquadRushDrawResult[] lastResults;
            public bool releaseCampaignClaimed;
            public int eventPity;
            public DrawBanner lastBanner;
        }
        readonly string key;
        readonly SquadRushEquipment equipment;
        readonly Func<DateTimeOffset> clock;
        readonly System.Random random=new System.Random(Guid.NewGuid().GetHashCode());
        SaveData data;
        public static string StorageKey(string progressKey)=>progressKey+".Account.v1";
        public SquadRushAccount(SquadRushEquipment gear,string progressKey=SquadRushProgress.DefaultKey,int completedStages=0,bool saveMigration=true,Func<DateTimeOffset> utcNow=null)
        {
            key=StorageKey(progressKey);equipment=gear;clock=utcNow??(()=>DateTimeOffset.UtcNow);
            bool existed=PlayerPrefs.HasKey(key);
            try{data=existed?JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key)):new SaveData();}catch(ArgumentException){data=new SaveData();}
            if(data==null||data.version!=1)data=new SaveData();
            data.gems=Mathf.Clamp(data.gems,0,WalletLimit);data.tickets=Mathf.Clamp(data.tickets,0,WalletLimit);data.coins=Mathf.Clamp(data.coins,0,WalletLimit);
            data.characters=(data.characters&7)|1;data.pity=Mathf.Clamp(data.pity,0,89);
            data.eventPity=Mathf.Clamp(data.eventPity,0,89);
            if(!OwnsCharacter(data.selectedCharacter))data.selectedCharacter=0;
            if(!existed)data.bestClear=Mathf.Max(0,completedStages);
            if(saveMigration){RecoverDraw();RecoverUpgrade();}ObserveDay();if(!existed&&saveMigration)Save();
        }
        int Today=>(int)(clock().ToOffset(TimeSpan.FromHours(9)).Date-new DateTime(1970,1,1)).TotalDays;
        void ObserveDay()
        {
            int day=Today;if(day<=data.observedDay)return;
            data.observedDay=day;data.dailyClears=data.dailyKills=data.dailyPulls=data.dailyClaims=0;
        }
        public int Gems=>data.gems;
        public int Tickets=>data.tickets;
        public int Coins=>data.coins;
        public int Pity=>data.pity;
        public int EventPity=>data.eventPity;
        public DrawBanner LastBanner=>data.lastBanner;
        public int BannerPity(DrawBanner banner)=>banner==DrawBanner.AkaneEvent?data.eventPity:data.pity;
        public int TotalPulls=>data.totalPulls;
        public int SelectedCharacter=>data.selectedCharacter;
        public int LoginDay=>(data.loginDays%7)+1;
        public bool CanClaimLogin {get{ObserveDay();return Today==data.observedDay&&data.lastLoginDay<data.observedDay;}}
        public int LoginGems=>LoginDay==7?300:100;
        public int LoginTickets=>LoginDay==7?3:1;
        public bool ReleaseCampaignClaimed=>data.releaseCampaignClaimed;
        public bool CanClaimReleaseCampaign=>!data.releaseCampaignClaimed&&data.tickets<=WalletLimit-ReleaseCampaignTickets;
        public bool ClaimReleaseCampaign()
        {
            if(!CanClaimReleaseCampaign)return false;
            // Reward and receipt share one persisted record, including for existing v1 saves.
            data.tickets+=ReleaseCampaignTickets;data.releaseCampaignClaimed=true;Save();return true;
        }
        public bool OwnsCharacter(int id)=>id>=0&&id<SquadRushCharacters.Count&&(data.characters&(1<<id))!=0;
        public bool SelectCharacter(int id){if(!OwnsCharacter(id))return false;data.selectedCharacter=id;Save();return true;}
        public static double CharacterChance(int pull)
        {
            if(pull>=90)return 1;
            return pull<80?.005:.005+.05*(pull-79);
        }
        public bool CanDraw(int count,bool tickets)=> (count==1||count==10)&&(tickets?Tickets>=count:Gems>=count*GemCost);
        public SquadRushDrawResult[] LastResults=>data.lastResults??Array.Empty<SquadRushDrawResult>();
        public SquadRushDrawResult[] Draw(int count,bool tickets,Func<double> sample=null,DrawBanner banner=DrawBanner.Permanent)
        {
            if(banner!=DrawBanner.Permanent&&banner!=DrawBanner.AkaneEvent)return Array.Empty<SquadRushDrawResult>();
            RecoverDraw();RecoverUpgrade();ObserveDay();if(!CanDraw(count,tickets))return Array.Empty<SquadRushDrawResult>();
            // Sample before mutating balances; injected samples are used only by deterministic Editor checks.
            var rolls=new double[count*3];for(int i=0;i<rolls.Length;i++){rolls[i]=(sample??random.NextDouble)();if(double.IsNaN(rolls[i])||rolls[i]<0||rolls[i]>=1)throw new ArgumentOutOfRangeException(nameof(sample));}
            if(tickets)data.tickets-=count;else data.gems-=count*GemCost;
            var results=new SquadRushDrawResult[count];var weapons=new List<int>();
            for(int i=0;i<count;i++)
            {
                var result=new SquadRushDrawResult();int pull=BannerPity(banner)+1;
                if(rolls[i*3]<CharacterChance(pull))
                {
                    var candidates=new List<int>();for(int id=1;id<3;id++)if(!OwnsCharacter(id))candidates.Add(id);
                    if(candidates.Count==0){candidates.Add(1);candidates.Add(2);}
                    result.kind=DrawKind.Character;result.id=banner==DrawBanner.AkaneEvent?1:candidates[Math.Min(candidates.Count-1,(int)(rolls[i*3+2]*candidates.Count))];result.amount=1;
                    result.isNew=!OwnsCharacter(result.id);data.characters|=1<<result.id;
                    if(!result.isNew)data.coins=Add(data.coins,DuplicateCoins);
                    if(banner==DrawBanner.AkaneEvent)data.eventPity=0;else data.pity=0;
                }
                else
                {
                    if(banner==DrawBanner.AkaneEvent)data.eventPity++;else data.pity++;
                    if(rolls[i*3+1]<.5){result.kind=DrawKind.Weapon;result.id=rolls[i*3+2]<.5?0:3;result.amount=1;weapons.Add(result.id);}
                    else{result.kind=DrawKind.Coins;result.amount=WeaponCoinValue;data.coins=Add(data.coins,result.amount);}
                }
                results[i]=result;
            }
            data.totalPulls=Add(data.totalPulls,count);data.dailyPulls=Add(data.dailyPulls,count);data.lastResults=results;data.lastBanner=banner;
            data.pendingTransaction=Guid.NewGuid().ToString("N");data.pendingWeapons=weapons.ToArray();
            Save();RecoverDraw();return results;
        }
        void RecoverDraw()
        {
            if(string.IsNullOrEmpty(data.pendingTransaction))return;
            equipment.GrantDraw(data.pendingTransaction,data.pendingWeapons??Array.Empty<int>());
            data.pendingTransaction=null;data.pendingWeapons=null;Save();
        }
        public bool CanUpgradeEquipment(int id)=>equipment.Owns(id)&&equipment.Level(id)<SquadRushEquipment.MaxLevel&&Coins>=equipment.UpgradeCost(id);
        public bool UpgradeEquipment(int id)
        {
            RecoverUpgrade();
            if(!CanUpgradeEquipment(id))return false;
            data.coins-=equipment.UpgradeCost(id);
            data.pendingUpgradeItem=id;data.pendingUpgradeLevel=equipment.Level(id)+1;
            // Persist payment and its target together before touching the inventory record.
            Save();RecoverUpgrade();return true;
        }
        void RecoverUpgrade()
        {
            if(data.pendingUpgradeLevel==0)return;
            // Applying an absolute target is idempotent if interrupted after the inventory save.
            equipment.ApplyPaidLevel(data.pendingUpgradeItem,data.pendingUpgradeLevel);
            data.pendingUpgradeItem=0;data.pendingUpgradeLevel=0;Save();
        }
        public bool ClaimLogin()
        {
            if(!CanClaimLogin)return false;
            data.gems=Add(data.gems,LoginGems);data.tickets=Add(data.tickets,LoginTickets);data.lastLoginDay=data.observedDay;data.loginDays++;Save();return true;
        }
        public int RecordBattle(int stage,int kills,bool won)
        {
            ObserveDay();data.dailyKills=Add(data.dailyKills,Mathf.Max(0,kills));int reward=0;
            if(won)
            {
                reward=100+(stage>data.bestClear?100:0);data.bestClear=Mathf.Max(data.bestClear,stage);
                data.gems=Add(data.gems,reward);data.coins=Add(data.coins,50);data.dailyClears=Add(data.dailyClears,1);
            }
            Save();return reward;
        }
        public static string MissionName(int id)=>new[]{"今日ステージを1回クリア","今日敵を50体撃破","今日ガチャを1回引く","STAGE 03をクリア"}[id];
        public static int MissionGoal(int id)=>id==1?50:id==3?3:1;
        public static int MissionGems(int id)=>id==2?50:id==3?300:100;
        public int MissionProgress(int id){ObserveDay();return id==0?data.dailyClears:id==1?data.dailyKills:id==2?data.dailyPulls:data.bestClear;}
        public bool MissionClaimed(int id){ObserveDay();return id==3?(data.permanentClaims&1)!=0:(data.dailyClaims&(1<<id))!=0;}
        public bool CanClaimMission(int id)=>id>=0&&id<4&&!MissionClaimed(id)&&MissionProgress(id)>=MissionGoal(id);
        public bool ClaimMission(int id)
        {
            if(!CanClaimMission(id))return false;
            data.gems=Add(data.gems,MissionGems(id));if(id==3)data.permanentClaims|=1;else data.dailyClaims|=1<<id;Save();return true;
        }
        static int Add(int value,int delta)=>(int)Math.Min(WalletLimit,(long)value+delta);
        void Save(){PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();}
    }
}
