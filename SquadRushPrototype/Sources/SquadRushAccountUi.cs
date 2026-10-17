using System;
using TMPro;
using UnityEngine;
namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        RectTransform characterPage,loginPage,missionPage,drawOverlay;
        TextMeshProUGUI[] walletLabels,characterStates,loginRows,missionStates;
        UnityEngine.UI.Button[] characterCards,missionClaims;
        Sprite[] characterPortraits;
        UnityEngine.UI.Image characterArt,equipmentHeroImage;
        TextMeshProUGUI characterName,characterDescription,characterAction,loginStatus,loginAction,gachaPity,gachaCost,gachaNotice;
        UnityEngine.UI.Button characterSelect,loginClaim,drawOne,drawTen,ticketMode,gemMode,lastDrawReceipt;
        TextMeshProUGUI skillAction;
        UnityEngine.UI.Button skillButton;
        SquadRushUiIcon skillIcon,characterSkillIcon;
        SquadRushUiIcon[] characterBadges,loginBadges;
        UnityEngine.UI.Image[] loginSurfaces,missionProgress;
        int inspectingCharacter;
        bool useTickets=true;
        void RefreshWallet()
        {
            if(walletLabels==null)return;
            walletLabels[0].text=game.Account.Gems.ToString("N0");walletLabels[1].text=game.Account.Tickets.ToString("N0");walletLabels[2].text=game.Account.Coins.ToString("N0");
            foreach(var label in walletLabels){label.enableAutoSizing=true;label.fontSizeMin=9;label.fontSizeMax=15;}
            RefreshNavigation(true);
        }
        void BuildAccountPages()
        {
            characterPortraits=new[]{heroIllustration,RequiredIllustration("akane"),RequiredIllustration("mio")};
            characterPage=Page("CharactersPage");loginPage=Page("LoginPage");missionPage=Page("MissionsPage");
            BuildCharactersPage();BuildLoginPage();BuildMissionsPage();BuildMailPage();BuildMissionTabs();
        }
        void ShowAccountPage(FrontEndPage page)
        {
            mailPage.gameObject.SetActive(page==FrontEndPage.Mail);
            characterPage.gameObject.SetActive(page==FrontEndPage.Characters);loginPage.gameObject.SetActive(page==FrontEndPage.Login);missionPage.gameObject.SetActive(page==FrontEndPage.Missions);
        }
        void BuildCharactersPage()
        {
            PageText("CharactersTitle",characterPage,"キャラクター",28,Color.white,.855f,50);
            characterCards=new UnityEngine.UI.Button[3];characterStates=new TextMeshProUGUI[3];characterBadges=new SquadRushUiIcon[3];
            for(int i=0;i<3;i++)
            {
                int id=i;float x=.055f+i*.30f;
                var card=PageButton("Character"+i,characterPage,"",new Vector2(x,.65f),new Vector2(x+.285f,.825f),slate,()=>InspectCharacter(id));characterCards[i]=card.GetComponent<UnityEngine.UI.Button>();
                Illustration("CharacterPortrait"+i,card.transform,characterPortraits[i],new Vector2(.08f,.25f),new Vector2(.92f,.97f));
                characterBadges[i]=DrawIcon("CharacterBadge"+i,card.transform,SquadIcon.Lock,new Vector2(.73f,.75f),new Vector2(.98f,.99f));
                characterStates[i]=BoxText("CharacterState"+i,card.transform,"",15,Color.white,new Vector2(.025f,.02f),new Vector2(.975f,.27f));
            }
            characterArt=Illustration("CharacterPreview",characterPage,characterPortraits[0],new Vector2(.28f,.36f),new Vector2(.72f,.645f));
            characterName=PageText("CharacterName",characterPage,"",25,gold,.345f,45);
            var skillPanel=Panel("CharacterSkillCard",characterPage,slate,new Vector2(.055f,.225f),new Vector2(.945f,.32f));
            characterSkillIcon=DrawIcon("CharacterSkillIcon",skillPanel,SquadIcon.Heal,new Vector2(.02f,.13f),new Vector2(.19f,.87f));
            characterDescription=BoxText("CharacterSkillDescription",skillPanel,"",18,new Color(.78f,.88f,.94f),new Vector2(.22f,.06f),new Vector2(.97f,.94f));
            var button=PageButton("SelectCharacter",characterPage,"",new Vector2(.20f,.13f),new Vector2(.80f,.21f),blue,()=>game.SelectCharacter(inspectingCharacter));
            characterSelect=button.GetComponent<UnityEngine.UI.Button>();characterAction=button.GetComponentInChildren<TextMeshProUGUI>();characterAction.fontSize=22;
        }
        public void ShowCharacters(){inspectingCharacter=game.Account.SelectedCharacter;OpenPage(FrontEndPage.Characters);InspectCharacter(inspectingCharacter);}
        void InspectCharacter(int id)
        {
            inspectingCharacter=id;characterArt.sprite=characterPortraits[id];characterName.text=SquadRushCharacters.Name(id)+" / "+SquadRushCharacters.Skill(id);
            characterSkillIcon.Kind=CharacterSkillKind(id);
            characterDescription.text=SquadRushCharacters.Description(id);
            for(int i=0;i<3;i++){bool owned=game.Account.OwnsCharacter(i);characterStates[i].text=SquadRushCharacters.Name(i)+" / "+(game.Account.SelectedCharacter==i?"出撃中":owned?"所持":"未入手");characterStates[i].enableAutoSizing=true;characterStates[i].fontSizeMin=11;characterStates[i].fontSizeMax=15;characterCards[i].image.color=i==id?new Color(.38f,.43f,.55f):slate;characterBadges[i].Kind=owned?SquadIcon.Check:SquadIcon.Lock;}
            bool available=game.Account.OwnsCharacter(id);characterSelect.interactable=available&&game.Account.SelectedCharacter!=id;characterAction.text=game.Account.SelectedCharacter==id?"選択中":available?"このキャラで出撃":"ガチャで入手";
        }
        void BuildLoginPage()
        {
            PageText("LoginTitle",loginPage,"ログインボーナス",28,Color.white,.855f,50);
            loginStatus=PageText("LoginStatus",loginPage,"",18,new Color(.70f,.84f,.94f),.755f,38);
            loginRows=new TextMeshProUGUI[7];loginBadges=new SquadRushUiIcon[7];loginSurfaces=new UnityEngine.UI.Image[7];
            for(int i=0;i<7;i++)
            {
                float y=.68f-i*.065f;var row=Panel("LoginDay"+i,loginPage,slate,new Vector2(.08f,y),new Vector2(.92f,y+.057f));
                loginSurfaces[i]=row.GetComponent<UnityEngine.UI.Image>();
                loginRows[i]=BoxText("LoginReward"+i,row,(i+1)+"日目",17,Color.white,new Vector2(.025f,.03f),new Vector2(.25f,.97f));
                DrawIcon("LoginGem"+i,row,SquadIcon.Gem,new Vector2(.28f,.12f),new Vector2(.39f,.88f));
                BoxText("LoginGems"+i,row,i==6?"300":"100",18,Color.white,new Vector2(.39f,.03f),new Vector2(.57f,.97f));
                DrawIcon("LoginTicket"+i,row,SquadIcon.Ticket,new Vector2(.60f,.12f),new Vector2(.71f,.88f));
                BoxText("LoginTickets"+i,row,i==6?"×3":"×1",18,Color.white,new Vector2(.72f,.03f),new Vector2(.87f,.97f));
                loginBadges[i]=DrawIcon("LoginBadge"+i,row,SquadIcon.Gift,new Vector2(.88f,.18f),new Vector2(.98f,.82f));
            }
            var button=PageButton("ClaimLogin",loginPage,"",new Vector2(.18f,.16f),new Vector2(.82f,.24f),gold,game.ClaimLogin);loginClaim=button.GetComponent<UnityEngine.UI.Button>();loginAction=button.GetComponentInChildren<TextMeshProUGUI>();loginAction.color=ink;loginAction.fontSize=23;
            PageText("LoginHint",loginPage,"日本時間0時に更新 / 7回で一巡",14,new Color(.61f,.77f,.86f),.125f,28);
        }
        public void ShowLogin()
        {
            bool ready=game.Account.CanClaimLogin;int day=ready?game.Account.LoginDay:(game.Account.LoginDay+5)%7+1;
            loginStatus.text=ready?"今日のログイン報酬を受け取れます":"今日の報酬は受取済みです";
            for(int i=0;i<7;i++){loginRows[i].color=i+1==day?gold:new Color(.78f,.87f,.94f);loginSurfaces[i].color=i+1==day?new Color(.21f,.29f,.37f):slate;loginBadges[i].Kind=i+1<day||i+1==day&&!ready?SquadIcon.Check:SquadIcon.Gift;}
            loginClaim.interactable=ready;loginAction.text=ready?"本日の報酬を受け取る":"受取済み";OpenPage(FrontEndPage.Login);
        }
        void BuildMissionsPage()
        {
            PageText("MissionsTitle",missionPage,"ミッション",28,Color.white,.855f,50);
            PageText("MissionsHint",missionPage,"デイリーは日本時間0時に更新",17,new Color(.67f,.81f,.91f),.14f,28);
            missionStates=new TextMeshProUGUI[4];missionClaims=new UnityEngine.UI.Button[4];missionProgress=new UnityEngine.UI.Image[4];
            SquadIcon[] icons={SquadIcon.Battle,SquadIcon.Target,SquadIcon.Ticket,SquadIcon.Trophy};
            for(int i=0;i<4;i++)
            {
                int id=i;float y=.64f-i*.142f;var row=Panel("Mission"+i,missionPage,slate,new Vector2(.055f,y),new Vector2(.945f,y+.126f));
                DrawIcon("MissionIcon"+i,row,icons[i],new Vector2(.02f,.43f),new Vector2(.13f,.95f));
                BoxText("MissionName"+i,row,SquadRushAccount.MissionName(i),19,Color.white,new Vector2(.14f,.52f),new Vector2(.975f,.94f));
                missionStates[i]=BoxText("MissionState"+i,row,"",16,new Color(.72f,.84f,.92f),new Vector2(.14f,.12f),new Vector2(.36f,.49f));
                DrawIcon("MissionGem"+i,row,SquadIcon.Gem,new Vector2(.38f,.12f),new Vector2(.45f,.49f));
                BoxText("MissionReward"+i,row,"+"+SquadRushAccount.MissionGems(i),16,gold,new Vector2(.46f,.12f),new Vector2(.63f,.49f));
                var track=Panel("MissionTrack"+i,row,ink,new Vector2(.14f,.055f),new Vector2(.61f,.085f));missionProgress[i]=Panel("MissionProgress"+i,track,new Color(.25f,.72f,.75f),Vector2.zero,Vector2.one).GetComponent<UnityEngine.UI.Image>();
                var claim=PageButton("ClaimMission"+i,row,"",new Vector2(.65f,.06f),new Vector2(.97f,.48f),blue,()=>game.ClaimMission(id));missionClaims[i]=claim.GetComponent<UnityEngine.UI.Button>();claim.GetComponentInChildren<TextMeshProUGUI>().fontSize=16;
            }
        }
        public void ShowMissions()
        {
            for(int i=0;i<4;i++)
            {
                missionStates[i].text=Mathf.Min(game.Account.MissionProgress(i),SquadRushAccount.MissionGoal(i))+" / "+SquadRushAccount.MissionGoal(i);
                missionProgress[i].rectTransform.anchorMax=new Vector2(Mathf.Clamp01((float)game.Account.MissionProgress(i)/SquadRushAccount.MissionGoal(i)),1);
                missionClaims[i].interactable=game.Account.CanClaimMission(i);missionClaims[i].GetComponentInChildren<TextMeshProUGUI>().text=game.Account.MissionClaimed(i)?"受取済み":"受け取る";
            }
            OpenPage(FrontEndPage.Missions);
        }
        void BuildRecruitmentPage()
        {
            BuildGachaSections();
            var banner=Panel("RecruitmentBanner",permanentGacha,new Color(.16f,.12f,.26f),new Vector2(.05f,.58f),new Vector2(.95f,.775f));
            // Character sprites are loaded again here because the recruitment page is built before account pages.
            Illustration("RecruitAkane",banner,RequiredIllustration("akane"),new Vector2(.02f,.04f),new Vector2(.43f,.96f));
            Illustration("RecruitMio",banner,RequiredIllustration("mio"),new Vector2(.57f,.04f),new Vector2(.98f,.96f));
            BoxText("RecruitmentCaption",banner,"新キャラ\nアカネ / ミオ",19,gold,new Vector2(.28f,.20f),new Vector2(.72f,.75f));
            gachaPity=BoxText("GachaPity",permanentGacha,"",19,gold,new Vector2(.04f,.505f),new Vector2(.96f,.58f));
            ticketMode=PageButton("UseTickets",permanentGacha,"チケット",new Vector2(.09f,.44f),new Vector2(.48f,.50f),slate,()=>{useTickets=true;RefreshRecruitment();}).GetComponent<UnityEngine.UI.Button>();
            gemMode=PageButton("UseGems",permanentGacha,"ジェム",new Vector2(.52f,.44f),new Vector2(.91f,.50f),slate,()=>{useTickets=false;RefreshRecruitment();}).GetComponent<UnityEngine.UI.Button>();
            ButtonIcon(ticketMode.gameObject,SquadIcon.Ticket);ButtonIcon(gemMode.gameObject,SquadIcon.Gem);
            var one=PageButton("DrawOne",permanentGacha,"1回引く",new Vector2(.08f,.33f),new Vector2(.47f,.42f),blue,()=>game.DrawGacha(1,useTickets));drawOne=one.GetComponent<UnityEngine.UI.Button>();
            var ten=PageButton("DrawTen",permanentGacha,"10回引く",new Vector2(.53f,.33f),new Vector2(.92f,.42f),gold,()=>game.DrawGacha(10,useTickets));drawTen=ten.GetComponent<UnityEngine.UI.Button>();ten.GetComponentInChildren<TextMeshProUGUI>().color=ink;
            gachaCost=PageText("GachaCost",permanentGacha,"",16,new Color(.77f,.86f,.94f),.302f,30);
            gachaNotice=BoxText("GachaNotice",permanentGacha,"通常は基本武器またはコイン\nチケット・ジェムはゲーム内報酬で獲得",16,new Color(.66f,.79f,.89f),new Vector2(.04f,.16f),new Vector2(.96f,.215f));
            lastDrawReceipt=PageButton("LastDrawReceipt",permanentGacha,"前回の結果",new Vector2(.27f,.112f),new Vector2(.73f,.155f),slate,()=>ShowDrawResults(game.Account.LastResults)).GetComponent<UnityEngine.UI.Button>();lastDrawReceipt.GetComponentInChildren<TextMeshProUGUI>().fontSize=15;
            BuildReleaseCampaign();
        }
        void RefreshRecruitment()
        {
            gachaPity.text="キャラ確定まであと "+(90-game.Account.Pity)+"回";
            lastDrawReceipt.interactable=game.Account.LastResults.Length>0;
            ticketMode.image.color=useTickets?new Color(.37f,.30f,.57f):slate;gemMode.image.color=!useTickets?new Color(.15f,.40f,.54f):slate;
            drawOne.interactable=game.Account.CanDraw(1,useTickets);drawTen.interactable=game.Account.CanDraw(10,useTickets);
            gachaCost.text=useTickets?"1回：チケット1枚 / 10回：10枚":"1回：ジェム100個 / 10回：1000個";
        }
        void BuildAccountOverlays()
        {
            drawOverlay=Panel("DrawResults",frontRoot,new Color(.01f,.02f,.05f,.96f),Vector2.zero,Vector2.one);drawOverlay.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;drawOverlay.gameObject.SetActive(false);
        }
        void HideAccountOverlays(){if(drawOverlay!=null)drawOverlay.gameObject.SetActive(false);}
        public void ShowDrawResults(SquadRushDrawResult[] results)
        {
            // Results have a fixed upper bound of ten; rebuild only this receipt, never the menu hierarchy.
            for(int i=drawOverlay.childCount-1;i>=0;i--){var child=drawOverlay.GetChild(i).gameObject;child.SetActive(false);if(Application.isPlaying)Destroy(child);else DestroyImmediate(child);}
            PageText("DrawResultsTitle",drawOverlay,(game.Account.LastBanner==DrawBanner.AkaneEvent?"イベント ":"恒常 ")+results.Length+"回ガチャ結果",28,gold,.88f,50);
            for(int i=0;i<results.Length;i++)
            {
                var result=results[i];float x=.055f+(i%2)*.46f,y=.725f-(i/2)*.115f;
                var card=Panel("DrawResult"+i,drawOverlay,result.kind==DrawKind.Character?new Color(.35f,.26f,.12f):slate,new Vector2(x,y),new Vector2(x+.43f,y+.103f));
                if(result.kind==DrawKind.Character||result.kind==DrawKind.Weapon)Illustration("ResultArt"+i,card,result.kind==DrawKind.Character?characterPortraits[result.id]:itemIllustrations[result.id],new Vector2(.025f,.08f),new Vector2(.35f,.93f));
                else DrawIcon("CoinArt"+i,card,SquadIcon.Coin,new Vector2(.025f,.08f),new Vector2(.35f,.93f));
                var label=BoxText("ResultLabel"+i,card,result.Label,16,Color.white,new Vector2(.36f,.06f),new Vector2(.98f,.95f));label.enableAutoSizing=true;label.fontSizeMin=11;label.fontSizeMax=16;
            }
            PageButton("CloseDrawResults",drawOverlay,"結果を確認",new Vector2(.20f,.10f),new Vector2(.80f,.18f),blue,()=>drawOverlay.gameObject.SetActive(false));
            drawOverlay.gameObject.SetActive(true);drawOverlay.SetAsLastSibling();RefreshWallet();
        }
        void BuildSkillHud()
        {
            var button=PageButton("CharacterSkill",battleRoot,"",new Vector2(.61f,.105f),new Vector2(.96f,.195f),new Color(.20f,.40f,.50f),()=>game.ActivateCharacterSkill());
            skillButton=button.GetComponent<UnityEngine.UI.Button>();skillAction=button.GetComponentInChildren<TextMeshProUGUI>();skillAction.fontSize=18;
            ButtonIcon(button,SquadIcon.Heal,.30f);skillIcon=button.GetComponentInChildren<SquadRushUiIcon>();
        }
        void RefreshSkillHud()
        {
            if(skillButton==null||game.Account==null)return;
            skillIcon.Kind=CharacterSkillKind(game.Account.SelectedCharacter);
            skillButton.interactable=game.State==RunState.Playing&&game.SkillCooldownRemaining<=0;
            skillAction.text=SquadRushCharacters.Skill(game.Account.SelectedCharacter)+"\n"+(game.SkillCooldownRemaining>0?Mathf.CeilToInt(game.SkillCooldownRemaining)+"秒":"発動する");
        }
        static SquadIcon CharacterSkillKind(int id) => id==1?SquadIcon.Blast:id==2?SquadIcon.Lightning:SquadIcon.Heal;
    }
}
