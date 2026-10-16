using TMPro;
using UnityEngine;

namespace SquadRush
{
    public enum FrontEndPage { Hidden, Home, Stages, Equipment, Gacha }

    public sealed partial class SquadRushHud
    {
        public FrontEndPage CurrentPage { get; private set; }
        RectTransform frontRoot, homePage, stagesPage, equipmentPage, gachaPage, itemDetail;
        SquadRushPortrait portrait;
        TextMeshProUGUI profileStage, profileBag, homeStage, homeStatus, selectedStage, stageBiome, stageInfo, stageReward, rewardCaption, unlockedInfo, equipmentSummary, bagCount;
        TextMeshProUGUI detailTitle, detailEffect, detailSource, detailAction;
        UnityEngine.UI.Button previousStage, nextStage, applyEquipment;
        UnityEngine.UI.Button[] tabs, equippedButtons;
        UnityEngine.UI.Image[] tabAccents, inventoryCards;
        TextMeshProUGUI[] inventoryStates, equippedLabels, tabLabels;
        SquadRushUiIcon[] equippedIcons;
        int selectedItem;
        readonly Color slate = new Color(0.12f, 0.20f, 0.31f);
        readonly Color gold = new Color(1, 0.78f, 0.32f);

        void BuildFrontEnd()
        {
            frontRoot = Panel("FrontEnd", safeRoot, new Color(0.035f, 0.065f, 0.11f, 0.98f), Vector2.zero, Vector2.one);
            frontRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            homePage = Page("HomePage"); stagesPage = Page("StagesPage"); equipmentPage = Page("EquipmentPage"); gachaPage = Page("GachaPage");
            portrait = gameObject.AddComponent<SquadRushPortrait>(); portrait.Initialize(game);
            BuildHomePage(); BuildStagesPage(); BuildEquipmentPage(); BuildGachaPage();
            BuildCommandHeader(); BuildNavigation(); BuildItemDetail(); frontRoot.gameObject.SetActive(false);
        }
        void BuildCommandHeader()
        {
            var header = Panel("CommandHeader", frontRoot, ink, new Vector2(0, 0.90f), Vector2.one);
            DrawIcon("CommanderAvatar", header, SquadIcon.Helmet, new Vector2(0.025f, 0.12f), new Vector2(0.19f, 0.96f));
            BoxText("CommandTitle", header, "SQUAD RUSH", 22, Color.white, new Vector2(0.21f, 0.47f), new Vector2(0.57f, 0.96f));
            BoxText("CommandSubtitle", header, "部隊本部", 15, new Color(0.54f, 0.75f, 0.90f), new Vector2(0.21f, 0.08f), new Vector2(0.57f, 0.48f));
            var stagePlate = Panel("Card", header, slate, new Vector2(0.59f, 0.12f), new Vector2(0.77f, 0.92f));
            profileStage = BoxText("ProfileStage", stagePlate, "", 15, Color.white, Vector2.zero, Vector2.one);
            var bagPlate = Panel("Card", header, slate, new Vector2(0.79f, 0.12f), new Vector2(0.975f, 0.92f));
            profileBag = BoxText("ProfileBag", bagPlate, "", 15, gold, Vector2.zero, Vector2.one);
        }
        void BuildHomePage()
        {
            var chapter = PageButton("HomeStages", homePage, "", new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.875f), slate, game.OpenStages);
            homeStage = chapter.GetComponentInChildren<TextMeshProUGUI>(); homeStage.fontSize = 25;
            Panel("Portrait shadow", homePage, new Color(0.02f, 0.04f, 0.07f, 0.7f), new Vector2(0.27f, 0.385f), new Vector2(0.73f, 0.43f));
            Panel("Portrait platform", homePage, new Color(0.16f, 0.29f, 0.37f), new Vector2(0.30f, 0.405f), new Vector2(0.70f, 0.445f));
            PortraitImage("HomeSoldier", homePage, new Vector2(0.22f, 0.405f), new Vector2(0.78f, 0.79f));
            homeStatus = PageText("HomeStatus", homePage, "", 21, new Color(0.76f, 0.86f, 0.94f), 0.34f, 65);
            var sortie = PageButton("HomeSortie", homePage, "出撃", new Vector2(0.19f, 0.17f), new Vector2(0.81f, 0.26f), gold, game.PlaySelectedStage);
            var label = sortie.GetComponentInChildren<TextMeshProUGUI>(); label.color = ink; label.fontSize = 32; label.fontStyle = FontStyles.Bold;
            PageText("HomeHint", homePage, "ステージを選んで、部隊と出撃しよう", 17, new Color(0.53f, 0.70f, 0.82f), 0.135f, 29);
        }
        void BuildStagesPage()
        {
            PageText("StagesTitle", stagesPage, "ステージ選択", 28, Color.white, 0.85f, 50);
            unlockedInfo = PageText("UnlockedStages", stagesPage, "", 17, new Color(0.63f, 0.78f, 0.9f), 0.795f, 34);
            var card = Panel("Card", stagesPage, slate, new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.76f));
            selectedStage = PageText("SelectedStage", card, "", 30, new Color(0.35f, 0.83f, 1), 0.85f, 60);
            stageBiome = PageText("StageBiome", card, "", 40, Color.white, 0.66f, 72);
            stageInfo = PageText("StageInfo", card, "", 21, new Color(0.75f, 0.85f, 0.94f), 0.43f, 85);
            rewardCaption = PageText("RewardCaption", card, "クリア報酬", 17, gold, 0.24f, 35);
            stageReward = PageText("StageReward", card, "", 22, Color.white, 0.13f, 50);
            previousStage = PageButton("PreviousStage", stagesPage, "←", new Vector2(0.08f, 0.18f), new Vector2(0.24f, 0.26f), slate, () => game.SelectStage(game.StageNumber - 1)).GetComponent<UnityEngine.UI.Button>();
            var start = PageButton("StageStart", stagesPage, "出撃", new Vector2(0.28f, 0.18f), new Vector2(0.72f, 0.26f), gold, game.PlaySelectedStage);
            start.GetComponentInChildren<TextMeshProUGUI>().color = ink;
            nextStage = PageButton("NextStage", stagesPage, "→", new Vector2(0.76f, 0.18f), new Vector2(0.92f, 0.26f), slate, () => game.SelectStage(game.StageNumber + 1)).GetComponent<UnityEngine.UI.Button>();
        }
        void BuildEquipmentPage()
        {
            PageText("EquipmentTitle", equipmentPage, "装備", 28, Color.white, 0.855f, 50);
            PortraitImage("EquipmentSoldier", equipmentPage, new Vector2(0.28f, 0.50f), new Vector2(0.72f, 0.82f));
            equippedButtons = new UnityEngine.UI.Button[3]; equippedLabels = new TextMeshProUGUI[3]; equippedIcons = new SquadRushUiIcon[3];
            for (int i = 0; i < 3; i++)
            {
                EquipmentSlot slot = (EquipmentSlot)i;
                float x = i == 0 ? 0.055f : 0.745f, y = i == 2 ? 0.515f : 0.67f;
                var button = PageButton("EquippedSlot" + slot, equipmentPage, "", new Vector2(x, y), new Vector2(x + 0.20f, y + 0.13f), slate, () => InspectSlot(slot));
                equippedButtons[i] = button.GetComponent<UnityEngine.UI.Button>();
                equippedLabels[i] = button.GetComponentInChildren<TextMeshProUGUI>(); equippedLabels[i].fontSize = 15;
                SetAnchors(equippedLabels[i].rectTransform, new Vector2(0, 0), new Vector2(1, 0.40f));
                equippedIcons[i] = DrawIcon("SlotIcon", button.transform, IconFor(slot), new Vector2(0.12f, 0.36f), new Vector2(0.88f, 0.95f));
            }
            var info = Panel("Card", equipmentPage, new Color(0.10f, 0.16f, 0.24f), new Vector2(0.055f, 0.515f), new Vector2(0.255f, 0.635f));
            BoxText("LoadoutHint", info, "3種類\n各1つ装備", 16, new Color(0.62f, 0.78f, 0.9f), new Vector2(0.03f, 0), new Vector2(0.97f, 1));
            var stats = Panel("Card", equipmentPage, slate, new Vector2(0.045f, 0.427f), new Vector2(0.955f, 0.50f));
            equipmentSummary = BoxText("EquipmentSummary", stats, "", 19, gold, new Vector2(0.02f, 0), new Vector2(0.98f, 1));
            BoxText("InventoryTitle", equipmentPage, "装備一覧", 21, Color.white, new Vector2(0.055f, 0.377f), new Vector2(0.60f, 0.42f)).alignment = TextAlignmentOptions.Left;
            bagCount = BoxText("InventoryCount", equipmentPage, "", 17, new Color(0.66f, 0.80f, 0.9f), new Vector2(0.60f, 0.377f), new Vector2(0.95f, 0.42f));
            inventoryCards = new UnityEngine.UI.Image[SquadRushEquipment.Count]; inventoryStates = new TextMeshProUGUI[SquadRushEquipment.Count];
            for (int i = 0; i < SquadRushEquipment.Count; i++)
            {
                int id = i; EquipmentItem item = SquadRushEquipment.Item(i);
                float x = 0.055f + i % 3 * 0.305f, y = 0.253f - i / 3 * 0.124f;
                var button = PageButton("InventoryItem" + i, equipmentPage, "", new Vector2(x, y), new Vector2(x + 0.28f, y + 0.112f), slate, () => InspectItem(id));
                button.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
                inventoryCards[i] = button.GetComponent<UnityEngine.UI.Image>();
                DrawIcon("InventoryIcon", button.transform, IconFor(item.Slot), new Vector2(0.12f, 0.29f), new Vector2(0.88f, 0.96f));
                BoxText("ItemValue" + i, button.transform, "+" + item.Value + (item.Slot == EquipmentSlot.Support ? "%" : ""), 15, gold, new Vector2(0.64f, 0.64f), new Vector2(0.96f, 0.96f));
                inventoryStates[i] = BoxText("ItemState" + i, button.transform, "", 15, Color.white, new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.34f));
            }
        }
        void BuildGachaPage()
        {
            PageText("GachaTitle", gachaPage, "ガチャ", 28, Color.white, 0.85f, 50);
            var card = Panel("Card", gachaPage, new Color(0.16f, 0.14f, 0.24f), new Vector2(0.12f, 0.43f), new Vector2(0.88f, 0.78f));
            DrawIcon("SupplyPreview", card, SquadIcon.Supply, new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.94f));
            PageText("ComingSoon", card, "COMING SOON", 26, new Color(0.79f, 0.69f, 0.94f), 0.23f, 55);
            PageText("GachaNotice", gachaPage, "ガチャは準備中です\n今はステージクリアで\n装備を入手できます", 22, new Color(0.76f, 0.84f, 0.93f), 0.33f, 110);
            PageButton("GachaEquipment", gachaPage, "装備を見る", new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.25f), slate, game.OpenEquipment);
        }
        void BuildNavigation()
        {
            var bar = Panel("BottomNavigation", frontRoot, ink, Vector2.zero, new Vector2(1, 0.105f));
            tabs = new UnityEngine.UI.Button[3]; tabAccents = new UnityEngine.UI.Image[3]; tabLabels = new TextMeshProUGUI[3];
            string[] names = { "HomeGacha", "HomeCommand", "HomeEquipment" }, labels = { "ガチャ", "出撃", "装備" };
            SquadIcon[] icons = { SquadIcon.Supply, SquadIcon.Battle, SquadIcon.Vest };
            UnityEngine.Events.UnityAction[] actions = { game.OpenGacha, game.ShowHome, game.OpenEquipment };
            for (int i = 0; i < 3; i++)
            {
                float x = i / 3f;
                var tab = PageButton(names[i], bar, labels[i], new Vector2(x + 0.008f, 0.03f), new Vector2(x + 1 / 3f - 0.008f, 0.97f), slate, actions[i]);
                tabs[i] = tab.GetComponent<UnityEngine.UI.Button>(); tabLabels[i] = tab.GetComponentInChildren<TextMeshProUGUI>(); tabLabels[i].fontSize = 20;
                SetAnchors(tabLabels[i].rectTransform, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.43f));
                DrawIcon("TabIcon", tab.transform, icons[i], new Vector2(0.30f, 0.42f), new Vector2(0.70f, 0.94f));
                tabAccents[i] = Panel("SelectedTab", tab.transform, gold, new Vector2(0.10f, 0.94f), new Vector2(0.90f, 1)).GetComponent<UnityEngine.UI.Image>();
            }
        }
        void BuildItemDetail()
        {
            itemDetail = Panel("EquipmentDetail", frontRoot, new Color(0.01f, 0.025f, 0.05f, 0.84f), Vector2.zero, Vector2.one);
            itemDetail.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            var card = Panel("Card", itemDetail, slate, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.79f));
            PageButton("CloseEquipmentDetail", card, "×", new Vector2(0.84f, 0.85f), new Vector2(0.97f, 0.98f), ink, () => itemDetail.gameObject.SetActive(false));
            detailTitle = PageText("DetailTitle", card, "", 25, Color.white, 0.78f, 85);
            detailEffect = PageText("DetailEffect", card, "", 28, gold, 0.58f, 65);
            detailSource = PageText("DetailSource", card, "", 20, new Color(0.72f, 0.85f, 0.94f), 0.38f, 85);
            var action = PageButton("EquipmentAction", card, "", new Vector2(0.12f, 0.10f), new Vector2(0.88f, 0.24f), blue, () => game.ToggleEquipment(selectedItem));
            applyEquipment = action.GetComponent<UnityEngine.UI.Button>(); detailAction = action.GetComponentInChildren<TextMeshProUGUI>(); itemDetail.gameObject.SetActive(false);
        }
        void InspectSlot(EquipmentSlot slot)
        {
            int id = game.Equipment.Equipped(slot);
            if (id < 0) for (int i = 0; i < SquadRushEquipment.Count; i++) if (SquadRushEquipment.Item(i).Slot == slot) { id = i; break; }
            InspectItem(id);
        }
        void InspectItem(int id)
        {
            selectedItem = id; EquipmentItem item = SquadRushEquipment.Item(id); bool owned = game.Equipment.Owns(id);
            detailTitle.text = item.SlotName + "\n" + item.Name; detailEffect.text = item.Effect;
            detailSource.text = owned ? "入手済み\nステージを越えて効果を引き継ぎます" : "STAGE " + item.RewardStage.ToString("D2") + " クリア報酬\n未入手";
            applyEquipment.interactable = owned; detailAction.text = game.Equipment.IsEquipped(id) ? "装備を外す" : owned ? "装備する" : "未入手";
            itemDetail.gameObject.SetActive(true); itemDetail.SetAsLastSibling();
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
        void PortraitImage(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var holder = Panel(name, parent, Color.clear, min, max);
            var imageObject = new GameObject("Soldier render", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(UnityEngine.UI.AspectRatioFitter)); imageObject.transform.SetParent(holder, false);
            var image = imageObject.GetComponent<UnityEngine.UI.RawImage>(); image.texture = portrait.Texture; image.raycastTarget = false;
            var fitter = imageObject.GetComponent<UnityEngine.UI.AspectRatioFitter>(); fitter.aspectRatio = 384f / 512; fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
        }
        static SquadIcon IconFor(EquipmentSlot slot) => slot == EquipmentSlot.Weapon ? SquadIcon.Rifle : slot == EquipmentSlot.Armor ? SquadIcon.Vest : SquadIcon.Magazine;
        static Color SlotTint(EquipmentSlot slot) => slot == EquipmentSlot.Weapon ? new Color(0.11f, 0.37f, 0.54f) : slot == EquipmentSlot.Armor ? new Color(0.13f, 0.38f, 0.34f) : new Color(0.40f, 0.31f, 0.18f);
        void OpenPage(FrontEndPage page)
        {
            CurrentPage = page; menu.SetActive(false); battleRoot.gameObject.SetActive(false); frontRoot.gameObject.SetActive(true); itemDetail.gameObject.SetActive(false);
            homePage.gameObject.SetActive(page == FrontEndPage.Home); stagesPage.gameObject.SetActive(page == FrontEndPage.Stages);
            equipmentPage.gameObject.SetActive(page == FrontEndPage.Equipment); gachaPage.gameObject.SetActive(page == FrontEndPage.Gacha);
            profileStage.text = "STAGE\n" + game.UnlockedStage.ToString("D2"); profileBag.text = "装備\n" + game.Equipment.OwnedCount + "/" + SquadRushEquipment.Count;
            int active = page == FrontEndPage.Gacha ? 0 : page == FrontEndPage.Equipment ? 2 : 1;
            for (int i = 0; i < tabs.Length; i++) { tabs[i].image.color = i == active ? new Color(0.13f, 0.35f, 0.46f) : slate; tabAccents[i].gameObject.SetActive(i == active); tabLabels[i].color = i == active ? gold : Color.white; }
            portrait.Refresh(game.Equipment);
        }
        public void ShowHome()
        {
            homeStage.text = "STAGE " + game.StageNumber.ToString("D2") + "   " + game.Stage.Name + "   →";
            homeStatus.text = game.StageDuration.ToString("0") + "秒 ＋ ボス戦\n初期の仲間 " + (game.Stage.StartingAllies + game.Equipment.StartingAllies) + "人"; OpenPage(FrontEndPage.Home);
        }
        public void ShowStages()
        {
            selectedStage.text = "STAGE " + game.StageNumber.ToString("D2"); stageBiome.text = game.Stage.Name;
            stageInfo.text = game.StageDuration.ToString("0") + "秒 ＋ ボス戦\n初期の仲間 " + (game.Stage.StartingAllies + game.Equipment.StartingAllies) + "人";
            EquipmentItem reward = SquadRushEquipment.StageReward(game.StageNumber); stageReward.text = reward.Name;
            rewardCaption.text = game.Equipment.Owns(reward.Id) ? "クリア報酬 ／ 入手済み" : "クリア報酬";
            unlockedInfo.text = "解放済み  STAGE 01 ～ " + game.UnlockedStage.ToString("D2");
            previousStage.interactable = game.StageNumber > 1; nextStage.interactable = game.StageNumber < game.UnlockedStage; OpenPage(FrontEndPage.Stages);
        }
        public void ShowEquipment()
        {
            equipmentSummary.text = "威力 " + game.WeaponDamage + "   仲間 +" + game.Equipment.StartingAllies + "   連射 +" + Mathf.RoundToInt(game.Equipment.FireRateBonus * 100) + "%";
            bagCount.text = "所持 " + game.Equipment.OwnedCount + " / " + SquadRushEquipment.Count;
            for (int i = 0; i < SquadRushEquipment.Count; i++)
            {
                bool owned = game.Equipment.Owns(i), equipped = game.Equipment.IsEquipped(i);
                inventoryStates[i].text = equipped ? "装備中" : owned ? "所持" : "STAGE " + SquadRushEquipment.Item(i).RewardStage.ToString("D2");
                inventoryStates[i].color = equipped ? gold : owned ? Color.white : new Color(0.53f, 0.64f, 0.73f);
                inventoryCards[i].color = owned ? SlotTint(SquadRushEquipment.Item(i).Slot) : new Color(0.12f, 0.17f, 0.23f);
            }
            for (int i = 0; i < 3; i++)
            {
                EquipmentSlot slot = (EquipmentSlot)i; bool equipped = game.Equipment.Equipped(slot) >= 0;
                equippedButtons[i].image.color = equipped ? SlotTint(slot) : slate; equippedIcons[i].color = equipped ? Color.white : new Color(0.5f, 0.6f, 0.7f);
                equippedLabels[i].text = (slot == EquipmentSlot.Weapon ? "武器" : slot == EquipmentSlot.Armor ? "防具" : "サポート") + "\n" + (equipped ? "装備中" : "未装備");
            }
            OpenPage(FrontEndPage.Equipment);
        }
        public void ShowGacha() => OpenPage(FrontEndPage.Gacha);
        public void HideFrontEnd()
        { if (frontRoot == null) return; CurrentPage = FrontEndPage.Hidden; frontRoot.gameObject.SetActive(false); battleRoot.gameObject.SetActive(true); }
    }
}
