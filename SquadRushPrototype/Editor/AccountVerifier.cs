using System;
using System.Collections.Generic;
using UnityEngine;
namespace SquadRush.Editor
{
    public static class AccountVerifier
    {
        static void Check(bool value,string label,List<string> checks){if(!value)throw new InvalidOperationException(label);checks.Add("PASS "+label);}
        public static void Verify(SquadRushGame game,string key,List<string> checks)
        {
            string[] names={key+".clock",key+".draw",key+".pity",key+".journal",key+".legacy",key+".campaign",key+".cap"};
            try
            {
                var now=new DateTimeOffset(2026,10,17,14,59,0,TimeSpan.Zero);
                var gear=new SquadRushEquipment(names[0]);var account=new SquadRushAccount(gear,names[0],utcNow:()=>now);
                Check(account.OwnsCharacter(0)&&!account.OwnsCharacter(1)&&account.SelectedCharacter==0&&account.Gems==0&&account.Tickets==0,"New account starts with Ren and no paid or unearned currency",checks);
                Check(!account.SelectCharacter(1)&&!account.SelectCharacter(-1)&&!account.CanDraw(1,true)&&account.Draw(1,false).Length==0,"Locked characters and insufficient draw balances cannot be used",checks);
                Check(account.ClaimLogin()&&account.Gems==100&&account.Tickets==1&&!account.ClaimLogin(),"Login grants exactly 100 gems and one ticket once per Japanese calendar day",checks);
                account=new SquadRushAccount(gear,names[0],utcNow:()=>now);Check(!account.CanClaimLogin&&account.Gems==100,"Reload does not reset the claimed login reward",checks);
                now=now.AddMinutes(1);Check(account.CanClaimLogin&&account.ClaimLogin()&&account.Gems==200,"Login becomes claimable exactly at midnight in Japan",checks);
                now=now.AddDays(-1);Check(!account.ClaimLogin(),"Moving the clock backwards never grants another login bonus",checks);now=now.AddDays(1);
                for(int day=3;day<=7;day++){now=now.AddDays(1);Check(account.ClaimLogin(),"Login day "+day+" grants its scheduled reward",checks);}
                Check(account.Gems==900&&account.Tickets==9,"Seventh login gives 300 gems and three tickets",checks);
                now=now.AddDays(1);Check(account.LoginDay==1&&account.ClaimLogin()&&account.Gems==1000,"Seven-day login reward cycle restarts without requiring consecutive days",checks);
                Check(account.RecordBattle(1,20,true)==200&&account.RecordBattle(1,30,true)==100,"First clear grants 200 gems and a replay grants 100",checks);
                Check(account.CanClaimMission(0)&&account.CanClaimMission(1)&&!account.CanClaimMission(2)&&!account.CanClaimMission(3),"Daily missions track stage clears and defeated enemies independently",checks);
                int gems=account.Gems;Check(account.ClaimMission(0)&&account.Gems==gems+100&&!account.ClaimMission(0),"Mission gem rewards are claimed only once",checks);
                account.RecordBattle(3,0,true);Check(account.ClaimMission(3)&&!account.ClaimMission(3),"Stage-three permanent mission unlocks and cannot be claimed twice",checks);
                now=now.AddDays(1);Check(account.MissionProgress(0)==0&&!account.MissionClaimed(0)&&account.MissionClaimed(3),"Japanese midnight resets daily missions while preserving permanent claims",checks);
                var replay=new SquadRushAccount(gear,names[0],utcNow:()=>now);Check(replay.RecordBattle(3,0,true)==100,"Previously cleared stage does not grant another first-clear bonus after reload",checks);

                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[1]),"{\"version\":1,\"gems\":2000,\"tickets\":11,\"characters\":1}");
                gear=new SquadRushEquipment(names[1]);account=new SquadRushAccount(gear,names[1]);
                int step=0;double[] weaponRoll={.9,.1,.1};Func<double> weapon=()=>weaponRoll[step++%3];
                var result=account.Draw(1,true,weapon);Check(result.Length==1&&result[0].kind==DrawKind.Weapon&&gear.Quantity(0)==1&&account.Tickets==10&&account.Gems==2000,"Ticket draw consumes exactly one ticket and saves a normal rifle reward",checks);
                step=0;double[] coinRoll={.9,.9,.9};Func<double> coins=()=>coinRoll[step++%3];
                result=account.Draw(1,false,coins);Check(result[0].kind==DrawKind.Coins&&account.Coins==200&&account.Gems==1900&&account.Tickets==10,"Gem draw spends 100 gems without using tickets and grants 200 coins",checks);
                Check(account.CanClaimMission(2)&&account.ClaimMission(2),"A successful draw advances the daily draw mission",checks);
                int before=account.Gems;Check(account.Draw(2,false).Length==0&&account.Gems==before,"Unsupported draw amounts are rejected without spending",checks);
                step=0;result=account.Draw(10,true,weapon);Check(result.Length==10&&account.Tickets==0&&gear.Quantity(0)==11,"Ten-pull grants all ten weapons and spends exactly ten tickets",checks);
                before=account.Gems;Check(account.Draw(10,true).Length==0&&account.Gems==before,"Insufficient tickets cannot silently spend gems",checks);
                Check(new SquadRushAccount(gear,names[1]).LastResults.Length==10&&new SquadRushEquipment(names[1]).Quantity(0)==11,"Draw receipt, pity, wallet and weapon quantities survive reload",checks);

                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[2]),"{\"version\":1,\"tickets\":100,\"characters\":1,\"pity\":78}");
                gear=new SquadRushEquipment(names[2]);account=new SquadRushAccount(gear,names[2]);
                step=0;double[] near={.04,.9,.1};Func<double> nearRoll=()=>near[step++%3];
                result=account.Draw(1,true,nearRoll);Check(result[0].kind==DrawKind.Coins&&account.Pity==79,"Pull 79 retains the base 0.5 percent character chance",checks);
                result=account.Draw(1,true,nearRoll);Check(result[0].kind==DrawKind.Character&&result[0].isNew&&account.Pity==0,"Pull 80 raises the character chance to 5.5 percent and a hit resets pity",checks);
                Check(Math.Abs(SquadRushAccount.CharacterChance(1)-.005)<1e-9&&Math.Abs(SquadRushAccount.CharacterChance(80)-.055)<1e-9&&Math.Abs(SquadRushAccount.CharacterChance(89)-.505)<1e-9&&SquadRushAccount.CharacterChance(90)==1,"Published probability boundaries are exact",checks);
                bool increasing=true;for(int pull=80;pull<=90;pull++)increasing&=SquadRushAccount.CharacterChance(pull)>SquadRushAccount.CharacterChance(pull-1);
                Check(increasing,"Every soft-pity step from 80 through 90 strictly increases the rate",checks);
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[2]),"{\"version\":1,\"tickets\":100,\"characters\":1,\"pity\":88}");account=new SquadRushAccount(gear,names[2]);
                result=account.Draw(10,true,()=>.999999);int characters=0,index=-1;for(int i=0;i<result.Length;i++)if(result[i].kind==DrawKind.Character){characters++;index=i;}
                Check(characters==1&&index==1&&account.Pity==8&&account.Tickets==90,"A ten-pull crossing 90 guarantees the second result and resets pity for the remaining eight pulls",checks);
                account.Draw(1,true,()=>0);Check(account.OwnsCharacter(1)&&account.OwnsCharacter(2),"Character results prioritize the remaining unowned character",checks);
                before=account.Coins;result=account.Draw(1,true,()=>0);Check(!result[0].isNew&&account.Coins==before+1000,"After both new characters are owned, duplicate characters convert to 1000 coins",checks);
                Check(account.SelectCharacter(2)&&new SquadRushAccount(gear,names[2]).SelectedCharacter==2,"Acquired character selection persists",checks);

                gear=new SquadRushEquipment(names[3]);gear.GrantDraw("recover-once",new[]{0,3});
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[3]),"{\"version\":1,\"gems\":900,\"pity\":1,\"totalPulls\":1,\"characters\":1,\"pendingTransaction\":\"recover-once\",\"pendingWeapons\":[0,3]}");
                account=new SquadRushAccount(gear,names[3]);Check(gear.Quantity(0)==1&&gear.Quantity(3)==1&&account.Gems==900&&account.Pity==1,"Recovering a draw already granted before interruption never duplicates items or charges again",checks);
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[3]),"{\"version\":1,\"gems\":800,\"characters\":1,\"pendingTransaction\":\"recover-new\",\"pendingWeapons\":[0,0]}");
                account=new SquadRushAccount(gear,names[3]);account=new SquadRushAccount(gear,names[3]);Check(gear.Quantity(0)==3&&account.Gems==800,"Pending committed draw rewards finish once after reload",checks);
                PlayerPrefs.SetString(SquadRushEquipment.StorageKey(names[4]),"{\"version\":1,\"owned\":7,\"weapon\":0,\"armor\":1,\"support\":2}");
                gear=new SquadRushEquipment(names[4]);account=new SquadRushAccount(gear,names[4],3);Check(gear.OwnedCount==3&&gear.WeaponPower==1&&account.SelectedCharacter==0&&account.Gems==0&&account.MissionProgress(3)==3,"Existing equipment migrates without free currency or lost progress, and unlocks the permanent stage mission",checks);

                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[5]),"{\"version\":1,\"gems\":321,\"tickets\":2,\"coins\":400,\"characters\":3,\"selectedCharacter\":1,\"pity\":79}");
                gear=new SquadRushEquipment(names[5]);account=new SquadRushAccount(gear,names[5],utcNow:()=>now);
                Check(account.CanClaimReleaseCampaign&&!account.ReleaseCampaignClaimed&&account.Tickets==2,"Existing v1 profiles become eligible without an automatic campaign grant",checks);
                Check(account.ClaimReleaseCampaign()&&account.Tickets==12&&account.ReleaseCampaignClaimed&&account.Gems==321&&account.Coins==400&&account.Pity==79&&account.TotalPulls==0&&account.SelectedCharacter==1,"Release gift adds exactly ten tickets without spending, drawing or changing character and pity",checks);
                Check(!account.ClaimReleaseCampaign()&&account.Tickets==12,"Repeated release-campaign claims cannot duplicate tickets",checks);
                now=now.AddDays(20);account=new SquadRushAccount(gear,names[5],utcNow:()=>now);
                Check(account.ReleaseCampaignClaimed&&!account.CanClaimReleaseCampaign&&!account.ClaimReleaseCampaign()&&account.Tickets==12,"Campaign claim survives reload and calendar rollover",checks);
                result=account.Draw(10,true,()=>.999999);
                Check(result.Length==10&&account.Tickets==2&&account.Gems==321&&account.TotalPulls==10&&account.Pity==89,"Gift tickets fund a full permanent ten-pull and advance its existing pity",checks);
                Check(account.CanClaimLogin&&account.ClaimLogin()&&account.Tickets==3&&!account.CanClaimReleaseCampaign,"Login rewards and campaign receipts remain independent",checks);
                var freshGift=new SquadRushAccount(new SquadRushEquipment(names[6]),names[6]);Check(freshGift.ClaimReleaseCampaign()&&freshGift.Tickets==10&&freshGift.Gems==0,"New profiles also receive one free ten-pull without a gem balance",checks);
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[6]),"{\"version\":1,\"tickets\":9999990}");account=new SquadRushAccount(new SquadRushEquipment(names[6]),names[6]);
                Check(!account.CanClaimReleaseCampaign&&!account.ClaimReleaseCampaign()&&!account.ReleaseCampaignClaimed&&account.Tickets==9999990,"Ticket cap defers the gift without discarding tickets or marking it received",checks);
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[6]),"{\"version\":1,\"tickets\":9999989}");account=new SquadRushAccount(new SquadRushEquipment(names[6]),names[6]);
                Check(account.ClaimReleaseCampaign()&&account.Tickets==9999999,"Exactly ten available wallet slots allow the complete campaign gift",checks);

                PlayerPrefs.SetString(SquadRushAccount.StorageKey(names[6]),"{\"version\":1,\"tickets\":20,\"gems\":500,\"pity\":50,\"eventPity\":89,\"characters\":1}");
                gear=new SquadRushEquipment(names[6]);account=new SquadRushAccount(gear,names[6]);
                result=account.Draw(1,true,()=>.999999,DrawBanner.AkaneEvent);
                Check(result[0].kind==DrawKind.Character&&result[0].id==1&&result[0].isNew&&account.Pity==50&&account.EventPity==0&&account.Tickets==19,"Event pity grants featured Akane and leaves permanent pity untouched",checks);
                account.Draw(1,true,()=>.999999);Check(account.Pity==51&&account.EventPity==0,"Permanent pulls cannot advance the event pity",checks);
                before=account.Coins;result=account.Draw(1,true,()=>0,DrawBanner.AkaneEvent);
                Check(result[0].id==1&&!result[0].isNew&&account.Coins==before+1000&&!account.OwnsCharacter(2),"Event characters remain Akane and duplicates grant coins even while Mio is unowned",checks);
                account=new SquadRushAccount(new SquadRushEquipment(names[6]),names[6]);
                Check(account.Pity==51&&account.EventPity==0&&account.LastBanner==DrawBanner.AkaneEvent&&account.Tickets==17,"Both banner counters and event receipt survive reload",checks);
                Check(account.Draw(1,true,()=>0,(DrawBanner)99).Length==0&&account.Tickets==17,"Unknown banners are rejected without spending",checks);
                var locations=new HashSet<string>();var drops=new System.Random(20261017);
                for(int stage=1;stage<=SquadRushStage.AuthoredCount;stage++)
                {
                    locations.Add(new SquadRushStage(stage).Location);bool valid=true;var seen=new HashSet<int>();var rarities=new HashSet<int>();
                    for(int i=0;i<120;i++){var drop=SquadRushEquipment.RollStageDrop(stage,drops);valid&=drop.Id>=0&&drop.Id<SquadRushEquipment.DropItemCount(stage)&&drop.Rarity>=SquadRushEquipment.DropMinRarity(stage)&&drop.Rarity<=SquadRushEquipment.DropMaxRarity(stage);seen.Add(drop.Id);rarities.Add(drop.Rarity);}
                    Check(valid&&seen.Count==SquadRushEquipment.DropItemCount(stage)&&rarities.Count==SquadRushEquipment.DropMaxRarity(stage)-SquadRushEquipment.DropMinRarity(stage)+1,"Stage "+stage+" random drops cover eligible item types and its rarity band",checks);
                }
                Check(locations.Count==12&&new SquadRushStage(12).BossHealth>new SquadRushStage(6).BossHealth&&SquadRushEquipment.DropMinRarity(12)==2&&SquadRushEquipment.DropMaxRarity(12)==3,"Twelve named stages scale combat and reach rare-to-epic drops",checks);

                game.ShowHome();int allies=game.SquadCount;game.Begin();allies=game.SquadCount;
                Check(game.ActivateCharacterSkill()&&game.SquadCount==Mathf.Min(1000,allies+3)&&!game.ActivateCharacterSkill(),"Ren's actual skill supplies three allies and enforces cooldown",checks);
                game.TogglePause();float cooldown=game.SkillCooldownRemaining;game.Tick(5,false);Check(game.SkillCooldownRemaining==cooldown,"Pausing freezes character skill cooldown",checks);game.TogglePause();
                Check(!game.SelectCharacter(1)&&game.DrawGacha(1,true).Length==0,"Character switching and gacha spending are blocked during combat",checks);
                game.OpenEvents();Check(!game.ClaimReleaseCampaign()&&!game.Account.ReleaseCampaignClaimed&&game.Hud.CurrentPage==FrontEndPage.Hidden,"Campaign opening and claiming are blocked during combat",checks);
                game.Begin();Check(game.SkillCooldownRemaining==0&&game.SkillBuffRemaining==0,"Starting a new attempt resets skill cooldown and buffs",checks);
                // Give the isolated verification profile the two earned character results.
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(key),"{\"version\":1,\"characters\":7}");game.ConfigureVerification(key);game.SelectCharacter(1);game.Begin();
                game.SpawnTarget(TargetKind.Enemy,-3,25,10);game.SpawnTarget(TargetKind.Enemy,3,25,10);
                Check(game.ActivateCharacterSkill()&&game.Kills==2&&game.ActiveTargets==0,"Akane's area skill damages and defeats real enemies across both lanes",checks);
                game.ShowHome();game.SelectCharacter(2);game.Begin();float interval=game.FireInterval;
                Check(game.ActivateCharacterSkill()&&game.SkillBuffRemaining==6&&game.FireInterval<interval&&Mathf.Abs(game.FireRateMultiplier-1.5f)<.001f,"Mio's active skill increases real firing speed for six seconds",checks);
                game.ChangeSquad(997);for(int i=0;i<121;i++)game.Tick(.05f,false);
                Check(game.SkillBuffRemaining==0&&Mathf.Abs(game.FireRateMultiplier-1)<.001f,"Mio's temporary firing bonus expires without accumulating permanently",checks);
                game.ShowHome();game.SelectCharacter(0);
            }
            finally
            {
                foreach(string name in names){PlayerPrefs.DeleteKey(SquadRushEquipment.StorageKey(name));PlayerPrefs.DeleteKey(SquadRushAccount.StorageKey(name));}
                PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey(SquadRushEquipment.StorageKey(key));PlayerPrefs.DeleteKey(SquadRushAccount.StorageKey(key));PlayerPrefs.Save();
            }
        }
    }
}
