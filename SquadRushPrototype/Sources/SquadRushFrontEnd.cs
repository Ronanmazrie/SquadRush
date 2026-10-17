using TMPro;
using UnityEngine;

namespace SquadRush
{
    public enum FrontEndPage { Hidden, Home, Stages, Equipment, Gacha, Characters, Login, Missions, Mail }

    public sealed partial class SquadRushHud
    {
        public FrontEndPage CurrentPage { get; private set; }
        RectTransform navigationRoot;
        UnityEngine.UI.Image[] navNotices;
        UnityEngine.UI.Button previousHomeStage,nextHomeStage;
        TextMeshProUGUI homeDrop;
        RectTransform frontRoot, homePage, stagesPage, equipmentPage, gachaPage, itemDetail;
        TextMeshProUGUI profileStage, profileBag, homeStage, homeStatus, selectedStage, stageBiome, stageInfo, stageReward, rewardCaption, unlockedInfo, bagCount;
        TextMeshProUGUI detailTitle, detailEffect, detailSource, detailAction;
        UnityEngine.UI.Button previousStage, nextStage, applyEquipment;
        UnityEngine.UI.Button[] tabs, equippedButtons;
        UnityEngine.UI.Image[] tabAccents, inventoryCards;
        TextMeshProUGUI[] inventoryStates, equippedLabels, equippedEmptyLabels, tabLabels;
        UnityEngine.UI.Image[] equippedIcons;
        int selectedItem, selectedRarity;
        readonly Color slate = new Color(0.12f, 0.20f, 0.31f);
        readonly Color gold = new Color(1, 0.78f, 0.32f);

        void BuildFrontEnd()
        {
            frontRoot = Panel("FrontEnd", safeRoot, new Color(0.035f, 0.065f, 0.11f, 0.98f), Vector2.zero, Vector2.one);
            frontRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            homePage = Page("HomePage"); stagesPage = Page("StagesPage"); equipmentPage = Page("EquipmentPage"); gachaPage = Page("GachaPage");
            LoadEquipmentIllustrations(); BuildHomePage(); BuildStagesPage(); BuildEquipmentPage(); BuildGachaPage(); BuildAccountPages();
            BuildCommandHeader(); BuildNavigation(); BuildItemDetail(); BuildAccountOverlays(); frontRoot.gameObject.SetActive(false);
        }
        void BuildCommandHeader()
        {
            var header=Panel("CommandHeader",frontRoot,ink,new Vector2(0,.90f),Vector2.one);
            DrawIcon("CommanderAvatar",header,SquadIcon.Helmet,new Vector2(.02f,.12f),new Vector2(.145f,.94f));
            BoxText("CommandTitle",header,"SQUAD RUSH",17,Color.white,new Vector2(.16f,.50f),new Vector2(.55f,.98f));
            profileStage=BoxText("ProfileStage",header,"",13,Color.white,new Vector2(.57f,.50f),new Vector2(.76f,.98f));
            profileBag=BoxText("ProfileBag",header,"",13,gold,new Vector2(.77f,.50f),new Vector2(.98f,.98f));
            walletLabels=new TextMeshProUGUI[3];
            string[] currencies={"ジェム","チケット","コイン"};SquadIcon[] currencyIcons={SquadIcon.Gem,SquadIcon.Ticket,SquadIcon.Coin};
            for(int i=0;i<3;i++)
            {
                float x=.15f+i*.275f;var wallet=Panel("Currency"+i,header,new Color(.09f,.15f,.23f),new Vector2(x,.035f),new Vector2(x+.26f,.48f));
                DrawIcon("CurrencyIcon"+i,wallet,currencyIcons[i],new Vector2(.015f,.08f),new Vector2(.28f,.92f));
                BoxText("CurrencyName"+i,wallet,currencies[i],10,new Color(.65f,.78f,.88f),new Vector2(.29f,.50f),new Vector2(.98f,.99f));
                walletLabels[i]=BoxText("Wallet"+i,wallet,"",15,i==0?new Color(.48f,.88f,1):i==1?new Color(.77f,.68f,1):gold,new Vector2(.28f,.02f),new Vector2(.98f,.57f));
            }
        }
        void BuildHomePage()
        {
            homeStage=BoxText("HomeStage",homePage,"",23,Color.white,new Vector2(.20f,.80f),new Vector2(.80f,.875f));
            previousHomeStage=PageButton("HomePreviousStage",homePage,"←",new Vector2(.045f,.81f),new Vector2(.18f,.865f),slate,()=>game.SelectStage(game.StageNumber-1)).GetComponent<UnityEngine.UI.Button>();
            nextHomeStage=PageButton("HomeNextStage",homePage,"→",new Vector2(.82f,.81f),new Vector2(.955f,.865f),slate,()=>game.SelectStage(game.StageNumber+1)).GetComponent<UnityEngine.UI.Button>();
            homeStageArt=StagePicture("HomeStageIllustration",homePage,new Vector2(0.055f,0.405f),new Vector2(0.945f,0.785f));
            Panel("LocationShade",homePage,new Color(.02f,.04f,.07f,.82f),new Vector2(.055f,.405f),new Vector2(.945f,.49f));
            homeLocation=BoxText("HomeLocation",homePage,"",25,Color.white,new Vector2(0.08f,0.42f),new Vector2(0.92f,0.48f));
            homeStatus = PageText("HomeStatus", homePage, "", 19, new Color(0.76f, 0.86f, 0.94f), 0.36f, 60);
            var sortie = PageButton("HomeSortie", homePage, "出撃", new Vector2(0.19f, 0.17f), new Vector2(0.81f, 0.26f), gold, game.PlaySelectedStage);
            var label = sortie.GetComponentInChildren<TextMeshProUGUI>(); label.color = ink; label.fontSize = 32; label.fontStyle = FontStyles.Bold;
            DrawIcon("SortieIcon",sortie.transform,SquadIcon.Battle,new Vector2(.23f,.15f),new Vector2(.40f,.85f));SetAnchors(label.rectTransform,new Vector2(.43f,.04f),new Vector2(.78f,.96f));
            homeDrop=BoxText("HomeDrop",homePage,"",17,new Color(.75f,.85f,.94f),new Vector2(.055f,.275f),new Vector2(.945f,.335f));
            PageText("HomeHint", homePage, "ステージを選んで、部隊と出撃しよう", 17, new Color(0.53f, 0.70f, 0.82f), 0.135f, 29);
        }
        void BuildStagesPage()
        {
            PageText("StagesTitle", stagesPage, "ステージ選択", 28, Color.white, 0.85f, 50);
            unlockedInfo = PageText("UnlockedStages", stagesPage, "", 17, new Color(0.63f, 0.78f, 0.9f), 0.795f, 34);
            var card = Panel("Card", stagesPage, slate, new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.76f));
            selectedStage = PageText("SelectedStage", card, "", 30, new Color(0.35f, 0.83f, 1), 0.85f, 60);
            stageSelectionArt=StagePicture("StageSelectionIllustration",card,new Vector2(0.035f,0.39f),new Vector2(0.965f,0.76f));
            stageBiome = PageText("StageBiome", card, "", 27, Color.white, 0.33f, 46);
            stageInfo = PageText("StageInfo", card, "", 21, new Color(0.75f, 0.85f, 0.94f), 0.17f, 65);
            rewardCaption = PageText("RewardCaption", stagesPage, "クリア報酬", 17, gold, 0.315f, 27);
            stageReward = PageText("StageReward", stagesPage, "", 22, Color.white, 0.28f, 35);
            previousStage = PageButton("PreviousStage", stagesPage, "←", new Vector2(0.08f, 0.18f), new Vector2(0.24f, 0.26f), slate, () => game.SelectStage(game.StageNumber - 1)).GetComponent<UnityEngine.UI.Button>();
            var start = PageButton("StageStart", stagesPage, "出撃", new Vector2(0.28f, 0.18f), new Vector2(0.72f, 0.26f), gold, game.PlaySelectedStage);
            start.GetComponentInChildren<TextMeshProUGUI>().color = ink;
            nextStage = PageButton("NextStage", stagesPage, "→", new Vector2(0.76f, 0.18f), new Vector2(0.92f, 0.26f), slate, () => game.SelectStage(game.StageNumber + 1)).GetComponent<UnityEngine.UI.Button>();
        }
        void BuildEquipmentPage()
        {
            PageText("EquipmentTitle", equipmentPage, "装備", 28, Color.white, 0.855f, 50);
            Panel("Card",equipmentPage,new Color(0.10f,0.19f,0.25f),new Vector2(0.035f,0.625f),new Vector2(0.965f,0.835f));
            equipmentHeroImage=Illustration("EquipmentHeroIllustration",equipmentPage,heroIllustration,new Vector2(0.28f,0.625f),new Vector2(0.72f,0.83f));
            equippedButtons=new UnityEngine.UI.Button[3]; equippedLabels=new TextMeshProUGUI[3]; equippedEmptyLabels=new TextMeshProUGUI[3]; equippedIcons=new UnityEngine.UI.Image[3];
            for(int i=0;i<3;i++)
            {
                EquipmentSlot slot=(EquipmentSlot)i;
                float x=i==0?0.055f:0.735f,y=i==2?0.635f:0.725f;
                var button=PageButton("EquippedSlot"+slot,equipmentPage,"",new Vector2(x,y),new Vector2(x+0.21f,y+0.10f),slate,()=>InspectSlot(slot));
                equippedButtons[i]=button.GetComponent<UnityEngine.UI.Button>();
                var surface=Panel("Card",button.transform,new Color(0.10f,0.16f,0.22f),new Vector2(0.025f,0.025f),new Vector2(0.975f,0.975f)); surface.SetAsFirstSibling();
                equippedLabels[i]=button.GetComponentInChildren<TextMeshProUGUI>(); equippedLabels[i].fontSize=13;
                SetAnchors(equippedLabels[i].rectTransform,new Vector2(0.01f,0.015f),new Vector2(0.99f,0.48f));
                equippedIcons[i]=Illustration("EquippedIllustration"+i,button.transform,null,new Vector2(0.10f,0.40f),new Vector2(0.90f,0.97f));
                equippedEmptyLabels[i]=BoxText("EmptySlotMarker"+i,button.transform,"＋",36,new Color(0.35f,0.625f,0.64f),new Vector2(0.10f,0.40f),new Vector2(0.90f,0.97f));
                equippedEmptyLabels[i].text="";var emptyIcon=DrawIcon("EmptySlotIcon"+i,equippedEmptyLabels[i].transform,i==0?SquadIcon.Rifle:i==1?SquadIcon.Vest:SquadIcon.Magazine,Vector2.zero,Vector2.one);emptyIcon.color=new Color(.55f,.65f,.72f,.65f);
            }
            BoxText("LoadoutHint",equipmentPage,"各種類1つ\n装備できます",15,new Color(0.68f,0.82f,0.89f),new Vector2(0.055f,0.64f),new Vector2(0.265f,0.70f));
            equipmentStats=new TextMeshProUGUI[3]; string[] captions={"武器の威力","仲間ボーナス","連射ボーナス"};
            for(int i=0;i<3;i++)
            {
                float x=0.045f+i*0.308f;
                var stat=Panel("EquipmentStat"+i,equipmentPage,slate,new Vector2(x,0.54f),new Vector2(x+0.294f,0.615f));
                BoxText("StatCaption"+i,stat,captions[i],15,new Color(0.65f,0.80f,0.88f),new Vector2(0.02f,0.53f),new Vector2(0.98f,0.96f));
                DrawIcon("StatIcon"+i,stat,i==0?SquadIcon.Target:i==1?SquadIcon.Allies:SquadIcon.Lightning,new Vector2(.08f,.035f),new Vector2(.34f,.57f));
                equipmentStats[i]=BoxText("StatValue"+i,stat,"",23,gold,new Vector2(0.35f,0.02f),new Vector2(0.95f,0.56f));
            }
            BuildCompactInventory();
        }

        void BuildGachaPage() => BuildRecruitmentPage();
        void BuildNavigation()
        {
            navigationRoot=Panel("BottomNavigation",safeRoot,ink,Vector2.zero,new Vector2(1,.105f));
            tabs=new UnityEngine.UI.Button[6];tabAccents=new UnityEngine.UI.Image[6];tabLabels=new TextMeshProUGUI[6];navNotices=new UnityEngine.UI.Image[6];
            string[] names={"HomeCommand","HomeEquipment","HomeCharacters","HomeGacha","HomeMissions","HomeMail"};
            string[] labels={"ステージ","装備","キャラ","ガチャ","ミッション","メール"};
            SquadIcon[] icons={SquadIcon.Map,SquadIcon.Vest,SquadIcon.Helmet,SquadIcon.Ticket,SquadIcon.Missions,SquadIcon.Mail};
            for(int i=0;i<6;i++)
            {
                int target=i;float x=i/6f;var tab=PageButton(names[i],navigationRoot,labels[i],new Vector2(x+.003f,.03f),new Vector2(x+1/6f-.003f,.97f),slate,()=>game.NavigateMenu(target));
                tabs[i]=tab.GetComponent<UnityEngine.UI.Button>();tabLabels[i]=tab.GetComponentInChildren<TextMeshProUGUI>();tabLabels[i].fontSize=13;
                SetAnchors(tabLabels[i].rectTransform,new Vector2(.015f,.06f),new Vector2(.985f,.39f));
                DrawIcon("TabIcon",tab.transform,icons[i],new Vector2(.18f,.40f),new Vector2(.82f,.94f));
                tabAccents[i]=Panel("SelectedTab",tab.transform,gold,new Vector2(.08f,.94f),new Vector2(.92f,1)).GetComponent<UnityEngine.UI.Image>();
                navNotices[i]=Panel("NavigationNotice"+i,tab.transform,gold,new Vector2(.81f,.75f),new Vector2(.92f,.85f)).GetComponent<UnityEngine.UI.Image>();
            }
        }
        void BuildItemDetail()
        {
            itemDetail=Panel("EquipmentDetail",frontRoot,new Color(0.01f,0.025f,0.05f,0.88f),Vector2.zero,Vector2.one);
            itemDetail.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var card=Panel("Card",itemDetail,slate,new Vector2(0.055f,0.12f),new Vector2(0.945f,0.88f));
            PageButton("CloseEquipmentDetail",card,"×",new Vector2(0.85f,0.91f),new Vector2(0.97f,0.985f),ink,()=>itemDetail.gameObject.SetActive(false));
            detailTitle=BoxText("DetailTitle",card,"",23,Color.white,new Vector2(0.035f,0.845f),new Vector2(0.89f,0.97f));
            var frame=Panel("DetailArtFrame",card,gold,new Vector2(0.33f,0.66f),new Vector2(0.67f,0.84f)); detailFrame=frame.GetComponent<UnityEngine.UI.Image>();
            Panel("DetailArtSurface",frame,new Color(0.07f,0.12f,0.19f),new Vector2(0.025f,0.025f),new Vector2(0.975f,0.975f));
            detailIllustration=Illustration("DetailIllustration",frame,null,new Vector2(0.06f,0.06f),new Vector2(0.94f,0.94f));
            detailEffect=BoxText("DetailEffect",card,"",23,gold,new Vector2(0.04f,0.59f),new Vector2(0.96f,0.66f));
            detailComparison=BoxText("DetailComparison",card,"",16,Color.white,new Vector2(0.04f,0.54f),new Vector2(0.96f,0.59f));
            Panel("DetailRule",card,new Color(0.25f,0.40f,0.50f),new Vector2(0.08f,0.537f),new Vector2(0.92f,0.539f));
            detailSource=BoxText("DetailSource",card,"",14,new Color(0.72f,0.85f,0.94f),new Vector2(0.04f,0.485f),new Vector2(0.96f,0.535f));
            var action=PageButton("EquipmentAction",card,"",new Vector2(0.12f,0.035f),new Vector2(0.88f,0.125f),blue,()=>game.ToggleEquipment(selectedItem,selectedRarity));
            applyEquipment=action.GetComponent<UnityEngine.UI.Button>(); detailAction=action.GetComponentInChildren<TextMeshProUGUI>();
            BuildFusionControls(card);
            itemDetail.gameObject.SetActive(false);
        }
        void InspectSlot(EquipmentSlot slot)
        {
            int id = game.Equipment.Equipped(slot);
            if (id < 0) for (int i = 0; i < SquadRushEquipment.Count; i++) if (SquadRushEquipment.Item(i).Slot == slot) { id = i; break; }
            InspectItem(id,game.Equipment.Equipped(slot)>=0?game.Equipment.EquippedRarity(slot):0);
        }
        void InspectItem(int id,int rarity=0)
        {
            selectedItem=id; selectedRarity=rarity; var item=SquadRushEquipment.Item(id); bool owned=game.Equipment.Quantity(id,rarity)>0;
            detailTitle.text=SquadRushEquipment.RarityName(rarity)+" / "+item.SlotName+" / Lv."+game.Equipment.Level(id)+"\n"+item.Name;
            detailEffect.text=SquadRushEquipment.Effect(id,rarity,game.Equipment.Level(id));
            detailIllustration.sprite=itemIllustrations[id]; detailFrame.color=RarityColor(rarity); UpdateDetailComparison(id,rarity);
            detailSource.text=(rarity==0?"STAGE "+(id<3?1:4).ToString("D2")+"～ ランダムドロップ":"高難度ドロップ / 合成")+"  /  所持 "+game.Equipment.Quantity(id,rarity)+"個";
            applyEquipment.interactable=owned; detailAction.text=game.Equipment.IsEquipped(id,rarity)?"装備を外す":owned?"装備する":"未入手";
            RefreshFusionControls(); itemDetail.gameObject.SetActive(true);itemDetail.SetAsLastSibling();
        }
        RectTransform Page(string name) => Panel(name, frontRoot, Color.clear, Vector2.zero, Vector2.one);
        TextMeshProUGUI BoxText(string name, Transform parent, string value, int size, Color color, Vector2 min, Vector2 max)
        {
            var label = Text(name, parent, value, size, color, Vector2.zero, Vector2.zero); SetAnchors(label.rectTransform, min, max); return label;
        }
        TextMeshProUGUI PageText(string name, Transform parent, string value, int size, Color color, float y, float height)
        {
            var label = Text(name, parent, value, size, color, Vector2.zero, Vector2.zero);
            label.rectTransform.anchorMin = new Vector2(0.03f, y); label.rectTransform.anchorMax = new Vector2(0.97f, y);
            label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(0, height); return label;
        }
        GameObject PageButton(string name, Transform parent, string label, Vector2 min, Vector2 max, Color color, UnityEngine.Events.UnityAction action)
        {
            var button = Button(name, parent, label, Vector2.zero, Vector2.zero, color, action); SetAnchors(button.GetComponent<RectTransform>(), min, max);
            var image = button.GetComponent<UnityEngine.UI.Image>(); image.sprite = RoundedSprite(); image.type = UnityEngine.UI.Image.Type.Sliced; return button;
        }
        SquadRushUiIcon DrawIcon(string name, Transform parent, SquadIcon kind, Vector2 min, Vector2 max)
        {
            var iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(SquadRushUiIcon)); iconObject.transform.SetParent(parent, false);
            SetAnchors(iconObject.GetComponent<RectTransform>(), min, max); var icon = iconObject.GetComponent<SquadRushUiIcon>(); icon.Kind = kind; icon.raycastTarget = false; return icon;
        }
        void ButtonIcon(GameObject button,SquadIcon kind,float textStart=.27f)
        {
            DrawIcon(button.name+"Icon",button.transform,kind,new Vector2(.035f,.13f),new Vector2(textStart-.025f,.87f));
            SetAnchors(button.GetComponentInChildren<TextMeshProUGUI>().rectTransform,new Vector2(textStart,.04f),new Vector2(.98f,.96f));
        }
        void OpenPage(FrontEndPage page)
        {
            HideAccountOverlays();
            CurrentPage = page; menu.SetActive(false); battleRoot.gameObject.SetActive(false); frontRoot.gameObject.SetActive(true); itemDetail.gameObject.SetActive(false);
            homePage.gameObject.SetActive(page == FrontEndPage.Home); stagesPage.gameObject.SetActive(page == FrontEndPage.Stages);
            equipmentPage.gameObject.SetActive(page == FrontEndPage.Equipment); gachaPage.gameObject.SetActive(page == FrontEndPage.Gacha);
            ShowAccountPage(page);RefreshWallet();
            profileStage.text = "STAGE " + game.UnlockedStage.ToString("D2"); profileBag.text = "装備 " + game.Equipment.OwnedCount + "個";
            RefreshNavigation();
        }
        public void ShowHome()
        {
            RefreshStagePictures();
            homeStage.text = "STAGE " + game.StageNumber.ToString("D2") + "   " + game.Stage.Name;
            homeStatus.text = game.StageDuration.ToString("0") + "秒 ＋ ボス戦\n初期の仲間 " + (game.Stage.StartingAllies + game.Equipment.StartingAllies) + "人";
            previousHomeStage.interactable=game.StageNumber>1;nextHomeStage.interactable=game.StageNumber<game.UnlockedStage;
            homeDrop.text="装備ドロップ："+SquadRushEquipment.DropRange(game.StageNumber)+"\n武器・防具・サポートからランダムで1個";OpenPage(FrontEndPage.Home);
        }
        public void ShowStages()
        {
            RefreshStagePictures();
            selectedStage.text = "STAGE " + game.StageNumber.ToString("D2"); stageBiome.text = StageLocation(game.StageNumber);
            stageInfo.text = game.StageDuration.ToString("0") + "秒 ＋ ボス戦\n初期の仲間 " + (game.Stage.StartingAllies + game.Equipment.StartingAllies) + "人";
            stageReward.text = "ランダム装備 ×1 / "+SquadRushEquipment.DropRange(game.StageNumber);stageReward.fontSize=17;
            rewardCaption.text = "毎回 装備×1・ジェム100 / 初クリア＋100";rewardCaption.fontSize=14;
            unlockedInfo.text = "解放済み  STAGE 01 ～ " + game.UnlockedStage.ToString("D2");
            previousStage.interactable = game.StageNumber > 1; nextStage.interactable = game.StageNumber < game.UnlockedStage; OpenPage(FrontEndPage.Stages);
        }
        public void ShowEquipment()
        {
            equipmentStats[0].text=game.WeaponDamage.ToString(); equipmentStats[1].text="+"+game.Equipment.StartingAllies;
            equipmentStats[2].text="+"+Mathf.RoundToInt(game.Equipment.FireRateBonus*100)+"%";
            equipmentHeroImage.sprite=characterPortraits[game.Account.SelectedCharacter];
            bagCount.text="所持 "+game.Equipment.OwnedCount+"個";
            RefreshCompactInventory();
            for(int i=0;i<3;i++)
            {
                EquipmentSlot slot=(EquipmentSlot)i; int id=game.Equipment.Equipped(slot); bool equipped=id>=0;
                equippedButtons[i].image.color=equipped?RarityColor(game.Equipment.EquippedRarity(slot)):new Color(0.25f,0.39f,0.48f);
                equippedIcons[i].sprite=equipped?itemIllustrations[id]:null; equippedIcons[i].enabled=equipped;
                equippedEmptyLabels[i].gameObject.SetActive(!equipped);
                equippedLabels[i].text=(slot==EquipmentSlot.Weapon?"武器":slot==EquipmentSlot.Armor?"防具":"サポート")+(equipped?" Lv."+game.Equipment.Level(id):"")+"\n"+(equipped?SquadRushEquipment.RarityName(game.Equipment.EquippedRarity(slot)):"未装備");
                equippedLabels[i].color=equipped?gold:new Color(0.68f,0.82f,0.89f);
            }
            OpenPage(FrontEndPage.Equipment);
        }
        public void ShowGacha() {RefreshRecruitment();SetGachaSection(false);OpenPage(FrontEndPage.Gacha);}
        public void HideFrontEnd()
        { if (frontRoot == null) return; CurrentPage = FrontEndPage.Hidden; frontRoot.gameObject.SetActive(false); battleRoot.gameObject.SetActive(true); }
    }
}
