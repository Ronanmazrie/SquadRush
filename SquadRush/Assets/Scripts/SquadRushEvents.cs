using TMPro;
using UnityEngine;
namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        RectTransform permanentGacha,eventGacha,mailPage;
        UnityEngine.UI.Button permanentTab,eventTab,eventTicketMode,eventGemMode,eventDrawOne,eventDrawTen,eventReceipt,mailClaim;
        TextMeshProUGUI eventPity,eventCost,mailState,mailClaimLabel;
        int lastNav=-1;RunState lastNavState=(RunState)(-1);float nextNavRefresh;
        public bool ShowingGachaEvents {get;private set;}
        void BuildGachaSections()
        {
            permanentGacha=Panel("PermanentGacha",gachaPage,Color.clear,Vector2.zero,Vector2.one);
            eventGacha=Panel("EventGacha",gachaPage,Color.clear,Vector2.zero,Vector2.one);
            PageText("GachaTitle",gachaPage,"ガチャ",28,Color.white,.86f,45);
            permanentTab=PageButton("PermanentGachaTab",gachaPage,"恒常ガチャ",new Vector2(.055f,.785f),new Vector2(.485f,.84f),slate,game.OpenGacha).GetComponent<UnityEngine.UI.Button>();
            eventTab=PageButton("EventGachaTab",gachaPage,"イベント",new Vector2(.515f,.785f),new Vector2(.945f,.84f),slate,game.OpenEvents).GetComponent<UnityEngine.UI.Button>();
            permanentTab.GetComponentInChildren<TextMeshProUGUI>().fontSize=20;eventTab.GetComponentInChildren<TextMeshProUGUI>().fontSize=20;
            ButtonIcon(permanentTab.gameObject,SquadIcon.Supply);ButtonIcon(eventTab.gameObject,SquadIcon.Blast);
        }
        void BuildReleaseCampaign()
        {
            var banner=Panel("EventRecruitmentBanner",eventGacha,new Color(.24f,.08f,.12f),new Vector2(.05f,.58f),new Vector2(.95f,.775f));
            Illustration("EventAkane",banner,RequiredIllustration("akane"),new Vector2(.025f,.02f),new Vector2(.43f,.98f));
            BoxText("EventCaption",banner,"紅蓮作戦\nアカネピックアップ",24,gold,new Vector2(.42f,.27f),new Vector2(.98f,.88f));
            BoxText("EventSkill",banner,"固有スキル：爆裂掃射",14,new Color(.95f,.77f,.77f),new Vector2(.42f,.06f),new Vector2(.98f,.27f));
            eventPity=BoxText("EventPity",eventGacha,"",19,gold,new Vector2(.04f,.505f),new Vector2(.96f,.58f));
            eventTicketMode=PageButton("EventUseTickets",eventGacha,"チケット",new Vector2(.09f,.44f),new Vector2(.48f,.50f),slate,()=>{useTickets=true;RefreshEventRecruitment();}).GetComponent<UnityEngine.UI.Button>();
            eventGemMode=PageButton("EventUseGems",eventGacha,"ジェム",new Vector2(.52f,.44f),new Vector2(.91f,.50f),slate,()=>{useTickets=false;RefreshEventRecruitment();}).GetComponent<UnityEngine.UI.Button>();
            ButtonIcon(eventTicketMode.gameObject,SquadIcon.Ticket);ButtonIcon(eventGemMode.gameObject,SquadIcon.Gem);
            eventDrawOne=PageButton("EventDrawOne",eventGacha,"1回引く",new Vector2(.08f,.33f),new Vector2(.47f,.42f),blue,()=>game.DrawGacha(1,useTickets,DrawBanner.AkaneEvent)).GetComponent<UnityEngine.UI.Button>();
            eventDrawTen=PageButton("EventDrawTen",eventGacha,"10回引く",new Vector2(.53f,.33f),new Vector2(.92f,.42f),gold,()=>game.DrawGacha(10,useTickets,DrawBanner.AkaneEvent)).GetComponent<UnityEngine.UI.Button>();eventDrawTen.GetComponentInChildren<TextMeshProUGUI>().color=ink;
            eventCost=PageText("EventCost",eventGacha,"",16,new Color(.77f,.86f,.94f),.302f,30);
            BoxText("EventRules",eventGacha,"キャラはアカネ / 通常は武器・コイン\n天井は恒常ガチャと別に保存\n所持済みキャラはコイン1000個に変換\n開催期間：終了日未定",16,new Color(.72f,.82f,.91f),new Vector2(.04f,.175f),new Vector2(.96f,.285f));
            eventReceipt=PageButton("EventLastReceipt",eventGacha,"前回の結果",new Vector2(.27f,.112f),new Vector2(.73f,.16f),slate,()=>ShowDrawResults(game.Account.LastResults)).GetComponent<UnityEngine.UI.Button>();eventReceipt.GetComponentInChildren<TextMeshProUGUI>().fontSize=15;
            SetGachaSection(false);
        }
        void RefreshEventRecruitment()
        {
            eventPity.text="キャラ確定まであと "+(90-game.Account.EventPity)+"回";
            eventTicketMode.image.color=useTickets?new Color(.37f,.30f,.57f):slate;eventGemMode.image.color=!useTickets?new Color(.15f,.40f,.54f):slate;
            eventDrawOne.interactable=game.Account.CanDraw(1,useTickets);eventDrawTen.interactable=game.Account.CanDraw(10,useTickets);eventReceipt.interactable=game.Account.LastResults.Length>0;
            eventCost.text=useTickets?"1回：チケット1枚 / 10回：10枚":"1回：ジェム100個 / 10回：1000個";
        }
        void SetGachaSection(bool events)
        {
            ShowingGachaEvents=events;permanentGacha.gameObject.SetActive(!events);eventGacha.gameObject.SetActive(events);
            permanentTab.image.color=events?slate:new Color(.16f,.38f,.49f);eventTab.image.color=events?new Color(.43f,.19f,.25f):slate;
            permanentTab.GetComponentInChildren<TextMeshProUGUI>().color=events?Color.white:gold;eventTab.GetComponentInChildren<TextMeshProUGUI>().color=events?gold:Color.white;
        }
        public void ShowEvents(){RefreshEventRecruitment();SetGachaSection(true);OpenPage(FrontEndPage.Gacha);}
        void BuildMailPage()
        {
            mailPage=Page("MailPage");PageText("MailTitle",mailPage,"メール",28,Color.white,.855f,50);
            PageText("MailHint",mailPage,"運営からのお知らせ・プレゼント",17,new Color(.68f,.81f,.91f),.795f,38);
            var card=Panel("ReleaseMail",mailPage,slate,new Vector2(.055f,.40f),new Vector2(.945f,.755f));
            DrawIcon("MailRewardIcon",card,SquadIcon.Mail,new Vector2(.035f,.68f),new Vector2(.19f,.94f));
            BoxText("MailSubject",card,"リリース記念プレゼント",23,gold,new Vector2(.21f,.69f),new Vector2(.97f,.95f));
            BoxText("MailBody",card,"遊んでいただきありがとうございます！\n無料10連分のチケットをお届けします。\n恒常・イベントの両ガチャで使えます。",18,Color.white,new Vector2(.045f,.32f),new Vector2(.955f,.70f));
            DrawIcon("MailTicket",card,SquadIcon.Ticket,new Vector2(.25f,.08f),new Vector2(.39f,.31f));
            BoxText("MailReward",card,"チケット ×10",23,Color.white,new Vector2(.40f,.08f),new Vector2(.87f,.31f));
            mailState=PageText("MailState",mailPage,"",18,new Color(.72f,.86f,.94f),.355f,45);
            var claim=PageButton("MailClaimRelease",mailPage,"",new Vector2(.12f,.23f),new Vector2(.88f,.32f),gold,()=>game.ClaimReleaseCampaign());mailClaim=claim.GetComponent<UnityEngine.UI.Button>();mailClaimLabel=claim.GetComponentInChildren<TextMeshProUGUI>();mailClaimLabel.fontSize=23;mailClaimLabel.color=ink;ButtonIcon(claim,SquadIcon.Gift,.22f);
            BoxText("MailRules",mailPage,"1セーブデータにつき1回 / 受取期限なし\n受取済みのプレゼントも一覧に残ります。",15,new Color(.65f,.79f,.88f),new Vector2(.055f,.14f),new Vector2(.945f,.22f));
        }
        public void ShowMail()
        {
            bool claimed=game.Account.ReleaseCampaignClaimed,ready=game.Account.CanClaimReleaseCampaign;
            mailState.text=claimed?"受取済み":"未受取 1件";mailClaim.interactable=ready;mailClaimLabel.text=claimed?"受取済み":ready?"プレゼントを受け取る":"チケット所持上限です";OpenPage(FrontEndPage.Mail);
        }
        void BuildMissionTabs()
        {
            foreach(var page in new[]{missionPage,loginPage})
            {
                bool login=page==loginPage;
                var left=PageButton(login?"MissionListFromLogin":"MissionsListTab",page,"ミッション",new Vector2(.055f,.785f),new Vector2(.485f,.835f),login?slate:new Color(.16f,.38f,.49f),game.OpenMissions);
                var right=PageButton(login?"LoginSelectedTab":"LoginRewardsTab",page,"ログイン",new Vector2(.515f,.785f),new Vector2(.945f,.835f),login?new Color(.16f,.38f,.49f):slate,game.OpenLogin);
                left.GetComponentInChildren<TextMeshProUGUI>().fontSize=19;right.GetComponentInChildren<TextMeshProUGUI>().fontSize=19;ButtonIcon(left,SquadIcon.Missions);ButtonIcon(right,SquadIcon.Gift);
            }
        }
        void RefreshNavigation(bool force=false)
        {
            if(tabs==null||game.Account==null)return;
            navigationRoot.gameObject.SetActive(game.State!=RunState.Playing&&game.State!=RunState.Paused);
            int active=CurrentPage==FrontEndPage.Gacha?3:CurrentPage==FrontEndPage.Equipment?1:CurrentPage==FrontEndPage.Characters?2:CurrentPage==FrontEndPage.Missions||CurrentPage==FrontEndPage.Login?4:CurrentPage==FrontEndPage.Mail?5:0;
            if(!force&&lastNav==active&&lastNavState==game.State&&Time.unscaledTime<nextNavRefresh)return;
            lastNav=active;lastNavState=game.State;nextNavRefresh=Time.unscaledTime+.5f;
            bool usable=game.State!=RunState.Playing;
            bool mission=game.Account.CanClaimLogin;for(int i=0;i<4;i++)mission|=game.Account.CanClaimMission(i);
            for(int i=0;i<tabs.Length;i++)
            {
                tabs[i].interactable=usable;tabs[i].image.color=i==active?new Color(.13f,.35f,.46f):slate;tabLabels[i].color=i==active?gold:Color.white;tabAccents[i].gameObject.SetActive(i==active);
                navNotices[i].gameObject.SetActive(i==4?mission:i==5&&game.Account.CanClaimReleaseCampaign);
            }
        }
    }
    public sealed partial class SquadRushGame
    {
        public void NavigateMenu(int target)
        {
            if(State==RunState.Playing)return;
            if(State!=RunState.Ready&&target!=0)ShowHome();
            switch(target){case 0:ShowHome();break;case 1:OpenEquipment();break;case 2:OpenCharacters();break;case 3:OpenGacha();break;case 4:OpenMissions();break;case 5:OpenMail();break;}
        }
    }
}
