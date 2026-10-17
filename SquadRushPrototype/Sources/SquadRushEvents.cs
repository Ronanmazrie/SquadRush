using TMPro;
using UnityEngine;

namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        RectTransform permanentGacha,eventGacha;
        UnityEngine.UI.Button permanentTab,eventTab,campaignClaim;
        UnityEngine.UI.Image homeCampaignNotice,eventCampaignNotice;
        TextMeshProUGUI campaignStatus,campaignAction,homeCampaignLabel;
        public bool ShowingGachaEvents {get;private set;}

        void BuildHomeCampaign()
        {
            var button=PageButton("HomeEvents",homePage,"リリース記念\n無料10連プレゼント",new Vector2(.10f,.685f),new Vector2(.90f,.775f),new Color(.26f,.18f,.40f),game.OpenEvents);
            homeCampaignLabel=button.GetComponentInChildren<TextMeshProUGUI>();homeCampaignLabel.fontSize=22;homeCampaignLabel.color=gold;ButtonIcon(button,SquadIcon.Gift,.24f);
            homeCampaignNotice=Panel("HomeCampaignNotice",button.transform,gold,new Vector2(.94f,.78f),new Vector2(.97f,.91f)).GetComponent<UnityEngine.UI.Image>();
        }
        void BuildGachaSections()
        {
            permanentGacha=Panel("PermanentGacha",gachaPage,Color.clear,Vector2.zero,Vector2.one);
            eventGacha=Panel("EventGacha",gachaPage,Color.clear,Vector2.zero,Vector2.one);
            PageText("GachaTitle",gachaPage,"ガチャ",28,Color.white,.86f,45);
            permanentTab=PageButton("PermanentGachaTab",gachaPage,"恒常ガチャ",new Vector2(.055f,.785f),new Vector2(.485f,.84f),slate,ShowGacha).GetComponent<UnityEngine.UI.Button>();
            eventTab=PageButton("EventGachaTab",gachaPage,"イベント",new Vector2(.515f,.785f),new Vector2(.945f,.84f),slate,game.OpenEvents).GetComponent<UnityEngine.UI.Button>();
            permanentTab.GetComponentInChildren<TextMeshProUGUI>().fontSize=20;eventTab.GetComponentInChildren<TextMeshProUGUI>().fontSize=20;
            ButtonIcon(permanentTab.gameObject,SquadIcon.Supply);ButtonIcon(eventTab.gameObject,SquadIcon.Gift);
            eventCampaignNotice=Panel("EventCampaignNotice",eventTab.transform,gold,new Vector2(.93f,.74f),new Vector2(.97f,.90f)).GetComponent<UnityEngine.UI.Image>();
        }
        void BuildReleaseCampaign()
        {
            var banner=Panel("ReleaseCampaignBanner",eventGacha,new Color(.17f,.12f,.28f),new Vector2(.055f,.455f),new Vector2(.945f,.765f));
            Panel("CampaignAccent",banner,gold,new Vector2(.025f,.965f),new Vector2(.975f,.98f));
            Illustration("CampaignRen",banner,heroIllustration,new Vector2(.025f,.05f),new Vector2(.38f,.96f));
            BoxText("CampaignTag",banner,"RELEASE CELEBRATION",13,new Color(.76f,.70f,.94f),new Vector2(.38f,.77f),new Vector2(.98f,.94f));
            BoxText("CampaignTitle",banner,"リリース記念\n無料10連",29,gold,new Vector2(.38f,.40f),new Vector2(.98f,.78f));
            DrawIcon("CampaignTicket",banner,SquadIcon.Ticket,new Vector2(.40f,.12f),new Vector2(.55f,.34f));
            BoxText("CampaignReward",banner,"チケット ×10",21,Color.white,new Vector2(.55f,.12f),new Vector2(.97f,.34f));
            campaignStatus=BoxText("CampaignStatus",eventGacha,"",18,new Color(.73f,.85f,.94f),new Vector2(.065f,.385f),new Vector2(.935f,.45f));
            var claim=PageButton("ClaimReleaseCampaign",eventGacha,"",new Vector2(.09f,.29f),new Vector2(.91f,.375f),gold,()=>game.ClaimReleaseCampaign());
            campaignClaim=claim.GetComponent<UnityEngine.UI.Button>();campaignAction=claim.GetComponentInChildren<TextMeshProUGUI>();campaignAction.fontSize=22;campaignAction.color=ink;ButtonIcon(claim,SquadIcon.Gift,.21f);
            var go=PageButton("CampaignToPermanent",eventGacha,"恒常ガチャで使う",new Vector2(.16f,.20f),new Vector2(.84f,.27f),blue,()=>{useTickets=true;ShowGacha();});go.GetComponentInChildren<TextMeshProUGUI>().fontSize=22;ButtonIcon(go,SquadIcon.Ticket,.22f);
            BoxText("CampaignRules",eventGacha,"このセーブデータで1回限定 / 終了日未定\n受け取ったチケットは恒常ガチャで使用できます。\n受取だけではガチャを引きません。",14,new Color(.65f,.77f,.88f),new Vector2(.055f,.115f),new Vector2(.945f,.19f));
            SetGachaSection(false);
        }
        void RefreshCampaignNotices()
        {
            bool ready=game.Account.CanClaimReleaseCampaign;
            if(homeCampaignLabel!=null)homeCampaignLabel.text=game.Account.ReleaseCampaignClaimed?"リリース記念\n10連チケット受取済み":"リリース記念\n無料10連プレゼント";
            if(homeCampaignNotice!=null)homeCampaignNotice.gameObject.SetActive(ready);
            if(eventCampaignNotice!=null)eventCampaignNotice.gameObject.SetActive(ready);
        }
        void SetGachaSection(bool events)
        {
            ShowingGachaEvents=events;permanentGacha.gameObject.SetActive(!events);eventGacha.gameObject.SetActive(events);
            permanentTab.image.color=events?slate:new Color(.16f,.38f,.49f);eventTab.image.color=events?new Color(.35f,.25f,.48f):slate;
            permanentTab.GetComponentInChildren<TextMeshProUGUI>().color=events?Color.white:gold;eventTab.GetComponentInChildren<TextMeshProUGUI>().color=events?gold:Color.white;
            RefreshCampaignNotices();
        }
        public void ShowEvents()
        {
            bool claimed=game.Account.ReleaseCampaignClaimed,ready=game.Account.CanClaimReleaseCampaign;
            campaignClaim.interactable=ready;campaignAction.text=claimed?"受取済み":ready?"チケット10枚を受け取る":"チケットの所持上限です";
            campaignStatus.text=claimed?"チケット10枚を受け取りました！":ready?"リリースを記念して、10連分をプレゼント！":"チケットを使用してから受け取れます。";
            SetGachaSection(true);OpenPage(FrontEndPage.Gacha);
        }
    }
}
