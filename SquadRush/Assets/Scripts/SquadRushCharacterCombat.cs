using UnityEngine;
namespace SquadRush
{
    public sealed partial class SquadRushGame
    {
        public SquadRushAccount Account {get;private set;}
        public float SkillCooldownRemaining {get;private set;}
        public float SkillBuffRemaining {get;private set;}
        public int SkillActivations {get;private set;}
        void ResetCharacterSkill(){SkillCooldownRemaining=SkillBuffRemaining=0;SkillActivations=0;}
        void TickCharacterSkill(float dt)
        {
            SkillCooldownRemaining=Mathf.Max(0,SkillCooldownRemaining-dt);SkillBuffRemaining=Mathf.Max(0,SkillBuffRemaining-dt);
        }
        public bool SelectCharacter(int id)
        {
            if(State!=RunState.Ready||!Account.SelectCharacter(id))return false;
            ResetCharacterSkill();RefreshCommander();Hud.ShowCharacters();return true;
        }
        public bool ActivateCharacterSkill()
        {
            if(State!=RunState.Playing||SkillCooldownRemaining>0)return false;
            int id=Account.SelectedCharacter;SkillCooldownRemaining=SquadRushCharacters.Cooldown(id);SkillActivations++;
            if(id==0)ChangeSquad(3);
            else if(id==1)
            {
                int damage=10+WeaponDamage*5;
                for(int i=targets.Count-1;i>=0&&State==RunState.Playing;i--)
                {var target=targets[i];if(target.Kind!=TargetKind.Crate)Damage(target,damage);}
            }
            else SkillBuffRemaining=6;
            Burst(squadRoot.position+Vector3.up);Hud.Flash(SquadRushCharacters.Name(id)+"："+SquadRushCharacters.Skill(id));Hud.UpdateStats();return true;
        }
        void RefreshCommander()
        {
            if(soldierRigs.Count==0||Account==null)return;
            soldierRigs[0].SetUniform(SquadRushCharacters.Uniform(Account.SelectedCharacter));
        }
        public void OpenCharacters(){if(State==RunState.Ready)Hud.ShowCharacters();}
        public void OpenLogin(){if(State==RunState.Ready)Hud.ShowLogin();}
        public void OpenMissions(){if(State==RunState.Ready)Hud.ShowMissions();}
        public void OpenEvents(){if(State==RunState.Ready)Hud.ShowEvents();}
        public bool ClaimReleaseCampaign()
        {
            if(State!=RunState.Ready)return false;
            bool claimed=Account.ClaimReleaseCampaign();Hud.ShowEvents();return claimed;
        }
        public SquadRushDrawResult[] DrawGacha(int count,bool tickets)
        {
            if(State!=RunState.Ready)return System.Array.Empty<SquadRushDrawResult>();
            var result=Account.Draw(count,tickets);Hud.ShowGacha();if(result.Length>0)Hud.ShowDrawResults(result);return result;
        }
        public void ClaimLogin(){if(State==RunState.Ready){Account.ClaimLogin();Hud.ShowLogin();}}
        public void ClaimMission(int id){if(State==RunState.Ready){Account.ClaimMission(id);Hud.ShowMissions();}}
    }
}
