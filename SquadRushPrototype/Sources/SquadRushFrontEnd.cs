using TMPro;
using UnityEngine;

namespace SquadRush
{
    public enum FrontEndPage { Hidden, Home, Stages, Equipment, Gacha }

    // Front-end pages share the existing Canvas, font, safe area and button styling.
    public sealed partial class SquadRushHud
    {
        public FrontEndPage CurrentPage { get; private set; }
        RectTransform frontRoot, homePage, stagesPage, equipmentPage, gachaPage;
        TextMeshProUGUI homeStatus, homeStageText, homeEquipmentText, selectedStage, stageBiome, stageInfo, stageReward, rewardCaption, unlockedInfo, equipmentSummary;
        UnityEngine.UI.Button previousStage, nextStage;
        UnityEngine.UI.Button[] equipmentButtons;
        UnityEngine.UI.Image[] equipmentCards;
        TextMeshProUGUI[] equipmentActions;
        readonly Color slate = new Color(0.12f, 0.20f, 0.31f);
        readonly Color gold = new Color(1, 0.78f, 0.32f);

        void BuildFrontEnd()
        {
            frontRoot = Panel("FrontEnd", safeRoot, new Color(0.035f, 0.065f, 0.11f, 0.98f), Vector2.zero, Vector2.one);
            frontRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            homePage = Page("HomePage"); stagesPage = Page("StagesPage"); equipmentPage = Page("EquipmentPage"); gachaPage = Page("GachaPage");
            PageText("HomeTag", homePage, "SQUAD COMMAND  /  HOME", 16, new Color(0.47f, 0.68f, 0.84f), 0.95f, 32);
            PageText("HomeTitle", homePage, "SQUAD\n<color=#53CFFF>RUSH</color>", 60, Color.white, 0.79f, 170).fontStyle = FontStyles.Bold;
            homeStatus = PageText("HomeStatus", homePage, "", 22, new Color(0.75f, 0.85f, 0.94f), 0.61f, 70);
            var stageButton = PageButton("HomeStages", homePage, "", new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.54f), blue, game.OpenStages);
            homeStageText = stageButton.GetComponentInChildren<TextMeshProUGUI>();
            var equipmentButton = PageButton("HomeEquipment", homePage, "", new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.36f), slate, game.OpenEquipment);
            homeEquipmentText = equipmentButton.GetComponentInChildren<TextMeshProUGUI>();
            PageButton("HomeGacha", homePage, "ガチャ\n準備中", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.20f), new Color(0.22f, 0.18f, 0.29f), game.OpenGacha);
            PageText("HomeHint", homePage, "ステージをクリアして、装備を集めよう", 17, new Color(0.53f, 0.66f, 0.79f), 0.035f, 32);

            PageText("StagesTitle", stagesPage, "ステージ選択", 33, Color.white, 0.92f, 60);
            unlockedInfo = PageText("UnlockedStages", stagesPage, "", 18, new Color(0.63f, 0.78f, 0.9f), 0.84f, 45);
            var stageCard = Panel("Card", stagesPage, slate, new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.78f));
            selectedStage = PageText("SelectedStage", stageCard, "", 32, new Color(0.35f, 0.83f, 1), 0.85f, 65);
            stageBiome = PageText("StageBiome", stageCard, "", 42, Color.white, 0.66f, 75);
            stageInfo = PageText("StageInfo", stageCard, "", 21, new Color(0.75f, 0.85f, 0.94f), 0.43f, 85);
            rewardCaption = PageText("RewardCaption", stageCard, "クリア報酬", 17, gold, 0.24f, 35);
            stageReward = PageText("StageReward", stageCard, "", 22, Color.white, 0.13f, 50);
            previousStage = PageButton("PreviousStage", stagesPage, "←", new Vector2(0.08f, 0.19f), new Vector2(0.24f, 0.27f), slate, () => game.SelectStage(game.StageNumber - 1)).GetComponent<UnityEngine.UI.Button>();
            PageButton("StageStart", stagesPage, "出撃", new Vector2(0.28f, 0.19f), new Vector2(0.72f, 0.27f), blue, game.PlaySelectedStage);
            nextStage = PageButton("NextStage", stagesPage, "→", new Vector2(0.76f, 0.19f), new Vector2(0.92f, 0.27f), slate, () => game.SelectStage(game.StageNumber + 1)).GetComponent<UnityEngine.UI.Button>();
            BackHome(stagesPage, "StagesHome");

            PageText("EquipmentTitle", equipmentPage, "装備", 33, Color.white, 0.94f, 60);
            PageText("EquipmentHint", equipmentPage, "各種類を1つずつ装備できます", 18, new Color(0.63f, 0.78f, 0.9f), 0.875f, 38);
            equipmentSummary = PageText("EquipmentSummary", equipmentPage, "", 19, gold, 0.79f, 72);
            equipmentButtons = new UnityEngine.UI.Button[SquadRushEquipment.Count];
            equipmentCards = new UnityEngine.UI.Image[SquadRushEquipment.Count];
            equipmentActions = new TextMeshProUGUI[SquadRushEquipment.Count];
            for (int i = 0; i < SquadRushEquipment.Count; i++)
            {
                int id = i; EquipmentItem item = SquadRushEquipment.Item(i);
                float x = i % 2 == 0 ? 0.06f : 0.51f, y = 0.53f - i / 2 * 0.19f;
                var card = Panel("Card", equipmentPage, slate, new Vector2(x, y), new Vector2(x + 0.43f, y + 0.17f));
                equipmentCards[i] = card.GetComponent<UnityEngine.UI.Image>();
                PageText("Slot" + i, card, item.SlotName, 15, new Color(0.54f, 0.73f, 0.88f), 0.85f, 28);
                PageText("ItemName" + i, card, item.Name, 20, Color.white, 0.65f, 43);
                PageText("ItemEffect" + i, card, item.Effect, 17, gold, 0.43f, 30);
                var button = PageButton("EquipItem" + i, card, "", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.31f), blue, () => game.ToggleEquipment(id));
                equipmentButtons[i] = button.GetComponent<UnityEngine.UI.Button>();
                equipmentActions[i] = button.GetComponentInChildren<TextMeshProUGUI>(); equipmentActions[i].fontSize = 17;
            }
            BackHome(equipmentPage, "EquipmentHome");

            PageText("GachaTitle", gachaPage, "ガチャ", 33, Color.white, 0.92f, 60);
            var comingCard = Panel("Card", gachaPage, new Color(0.16f, 0.14f, 0.24f), new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.78f));
            PageText("ComingSoon", comingCard, "COMING\nSOON", 44, new Color(0.79f, 0.69f, 0.94f), 0.57f, 150);
            PageText("GachaNotice", gachaPage, "ガチャは準備中です\n今はステージクリアで\n装備を入手できます", 23, new Color(0.76f, 0.84f, 0.93f), 0.32f, 120);
            PageButton("GachaEquipment", gachaPage, "装備を見る", new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.25f), slate, game.OpenEquipment);
            BackHome(gachaPage, "GachaHome");
            frontRoot.gameObject.SetActive(false);
        }
        RectTransform Page(string name) => Panel(name, frontRoot, Color.clear, Vector2.zero, Vector2.one);
        TextMeshProUGUI PageText(string name, Transform parent, string value, int size, Color color, float y, float height)
        {
            var label = Text(name, parent, value, size, color, Vector2.zero, Vector2.zero);
            label.rectTransform.anchorMin = new Vector2(0.03f, y); label.rectTransform.anchorMax = new Vector2(0.97f, y);
            label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(0, height);
            return label;
        }
        GameObject PageButton(string name, Transform parent, string label, Vector2 min, Vector2 max, Color color, UnityEngine.Events.UnityAction action)
        {
            var button = Button(name, parent, label, Vector2.zero, Vector2.zero, color, action);
            SetAnchors(button.GetComponent<RectTransform>(), min, max);
            var image = button.GetComponent<UnityEngine.UI.Image>(); image.sprite = RoundedSprite(); image.type = UnityEngine.UI.Image.Type.Sliced;
            return button;
        }
        void BackHome(Transform parent, string name) => PageButton(name, parent, "ホームへ", new Vector2(0.2f, 0.055f), new Vector2(0.8f, 0.12f), slate, game.ShowHome);
        void OpenPage(FrontEndPage page)
        {
            CurrentPage = page; menu.SetActive(false); battleRoot.gameObject.SetActive(false); frontRoot.gameObject.SetActive(true);
            homePage.gameObject.SetActive(page == FrontEndPage.Home); stagesPage.gameObject.SetActive(page == FrontEndPage.Stages);
            equipmentPage.gameObject.SetActive(page == FrontEndPage.Equipment); gachaPage.gameObject.SetActive(page == FrontEndPage.Gacha);
        }
        public void ShowHome()
        {
            homeStatus.text = "次の挑戦  STAGE " + game.UnlockedStage.ToString("D2") + "\n所持装備 " + game.Equipment.OwnedCount + " / " + SquadRushEquipment.Count;
            homeStageText.text = "ステージ\n出撃するステージを選択"; homeEquipmentText.text = "装備\n集めた装備を付け替える";
            OpenPage(FrontEndPage.Home);
        }
        public void ShowStages()
        {
            selectedStage.text = "STAGE " + game.StageNumber.ToString("D2"); stageBiome.text = game.Stage.Name;
            stageInfo.text = game.StageDuration.ToString("0") + "秒 ＋ ボス戦\n初期の仲間 " + (game.Stage.StartingAllies + game.Equipment.StartingAllies) + "人";
            EquipmentItem reward = SquadRushEquipment.StageReward(game.StageNumber);
            stageReward.text = reward.Name;
            rewardCaption.text = game.Equipment.Owns(reward.Id) ? "クリア報酬 ／ 入手済み" : "クリア報酬";
            unlockedInfo.text = "解放済み  STAGE 01 ～ " + game.UnlockedStage.ToString("D2");
            previousStage.interactable = game.StageNumber > 1; nextStage.interactable = game.StageNumber < game.UnlockedStage;
            OpenPage(FrontEndPage.Stages);
        }
        public void ShowEquipment()
        {
            equipmentSummary.text = "弾の威力 +" + game.Equipment.WeaponPower + "   連射 +" + Mathf.RoundToInt(game.Equipment.FireRateBonus * 100) + "%\n初期の仲間 +" + game.Equipment.StartingAllies;
            for (int i = 0; i < SquadRushEquipment.Count; i++)
            {
                bool owned = game.Equipment.Owns(i), equipped = game.Equipment.IsEquipped(i);
                equipmentButtons[i].interactable = owned;
                equipmentActions[i].text = equipped ? "装備中 ／ 外す" : owned ? "装備する" : "STAGE " + SquadRushEquipment.Item(i).RewardStage.ToString("D2") + " 報酬";
                equipmentCards[i].color = equipped ? new Color(0.08f, 0.32f, 0.43f) : slate;
            }
            OpenPage(FrontEndPage.Equipment);
        }
        public void ShowGacha() => OpenPage(FrontEndPage.Gacha);
        public void HideFrontEnd()
        { if (frontRoot == null) return; CurrentPage = FrontEndPage.Hidden; frontRoot.gameObject.SetActive(false); battleRoot.gameObject.SetActive(true); }
    }
}
