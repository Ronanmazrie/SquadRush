using TMPro;
using UnityEngine;

namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        UnityEngine.UI.ScrollRect inventoryScroll;
        UnityEngine.UI.Button mergeButton,upgradeButton;
        TextMeshProUGUI mergePreview,mergeLabel,upgradePreview,upgradeLabel;
        RectTransform inventoryContent;
        static Color RarityColor(int rarity) => new[] { new Color(.72f,.77f,.82f),new Color(.31f,.85f,.48f),new Color(.25f,.66f,1),new Color(.73f,.40f,1),new Color(1,.75f,.23f) }[Mathf.Clamp(rarity,0,4)];
        void BuildCompactInventory()
        {
            BoxText("InventoryTitle",equipmentPage,"装備一覧",21,Color.white,new Vector2(.055f,.49f),new Vector2(.55f,.535f)).alignment=TextAlignmentOptions.Left;
            bagCount=BoxText("InventoryCount",equipmentPage,"",17,new Color(.66f,.80f,.90f),new Vector2(.60f,.49f),new Vector2(.95f,.535f));
            BoxText("InventoryHint",equipmentPage,"同じ装備・同じ色を3個で合成   /   下へスクロール",14,new Color(.68f,.80f,.88f),new Vector2(.045f,.452f),new Vector2(.955f,.49f));
            var scroll=Panel("InventoryScroll",equipmentPage,Color.clear,new Vector2(.04f,.119f),new Vector2(.96f,.45f));
            inventoryScroll=scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            var viewport=Panel("Viewport",scroll,new Color(.05f,.10f,.16f),Vector2.zero,Vector2.one);
            viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
            inventoryContent=Panel("Content",viewport,Color.clear,Vector2.zero,Vector2.one);
            inventoryContent.anchorMin=new Vector2(0,1);inventoryContent.anchorMax=Vector2.one;inventoryContent.pivot=new Vector2(.5f,1);
            var grid=inventoryContent.gameObject.AddComponent<SquadRushInventoryLayout>();
            grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;
            grid.cellSize=new Vector2(85,101);grid.spacing=new Vector2(9,10);grid.padding=new RectOffset(5,5,5,5);
            grid.childAlignment=TextAnchor.UpperCenter;
            var fitter=inventoryContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fitter.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            inventoryScroll.viewport=viewport;inventoryScroll.content=inventoryContent;inventoryScroll.horizontal=false;inventoryScroll.vertical=true;
            inventoryScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;inventoryScroll.scrollSensitivity=35;
            inventoryCards=new UnityEngine.UI.Image[SquadRushEquipment.EntryCount];inventoryStates=new TextMeshProUGUI[SquadRushEquipment.EntryCount];inventoryIllustrations=new UnityEngine.UI.Image[SquadRushEquipment.EntryCount];
            for(int entry=0;entry<SquadRushEquipment.EntryCount;entry++)
            {
                int id=entry%SquadRushEquipment.Count,rarity=entry/SquadRushEquipment.Count;
                var button=PageButton("InventoryItem"+entry,inventoryContent,"",Vector2.zero,Vector2.one,RarityColor(rarity),()=>InspectItem(id,rarity));
                inventoryCards[entry]=button.GetComponent<UnityEngine.UI.Image>();
                var face=Panel("Card",button.transform,new Color(.08f,.14f,.20f),new Vector2(.035f,.025f),new Vector2(.965f,.975f));face.SetAsFirstSibling();
                inventoryIllustrations[entry]=Illustration("InventoryIllustration"+entry,button.transform,itemIllustrations[id],new Vector2(.06f,.38f),new Vector2(.94f,.94f));
                var label=button.GetComponentInChildren<TextMeshProUGUI>();label.text=ItemCaption(id);label.fontSize=12;label.enableAutoSizing=true;label.fontSizeMin=9;label.fontSizeMax=12;
                SetAnchors(label.rectTransform,new Vector2(.015f,.18f),new Vector2(.985f,.40f));
                inventoryStates[entry]=BoxText("ItemState"+entry,button.transform,"",11,Color.white,new Vector2(.015f,.005f),new Vector2(.985f,.18f));
                BoxText("ItemLevel"+entry,button.transform,"",11,gold,new Vector2(.01f,.76f),new Vector2(.42f,.99f));
                var tag=Panel("QuantityBadge",button.transform,new Color(.025f,.05f,.09f,.95f),new Vector2(.40f,.75f),new Vector2(.98f,.99f));
                var quantity=BoxText("ItemQuantity"+entry,tag,"",13,Color.white,Vector2.zero,Vector2.one);quantity.enableAutoSizing=true;quantity.fontSizeMin=8;quantity.fontSizeMax=13;
            }
        }
        void RefreshCompactInventory()
        {
            for(int entry=0;entry<SquadRushEquipment.EntryCount;entry++)
            {
                int id=entry%SquadRushEquipment.Count,r=entry/SquadRushEquipment.Count,n=game.Equipment.Quantity(id,r);
                bool equipped=game.Equipment.IsEquipped(id,r);
                // Keep a source preview for unearned types; acquired rarities each get a separate stack.
                inventoryCards[entry].gameObject.SetActive(n>0 || (r==0&&!game.Equipment.Owns(id)));
                inventoryCards[entry].color=n>0?RarityColor(r):new Color(.26f,.32f,.38f);
                inventoryIllustrations[entry].color=n>0?Color.white:new Color(.43f,.48f,.53f);
                inventoryStates[entry].text=equipped?"装備中":n==0?"未入手":game.Equipment.CanMerge(id,r)?"合成可能 ↑":SquadRushEquipment.RarityName(r);
                inventoryStates[entry].color=equipped?gold:RarityColor(r);
                inventoryCards[entry].transform.Find("ItemLevel"+entry).GetComponent<TextMeshProUGUI>().text=n>0?"Lv."+game.Equipment.Level(id):"";
                var badge=inventoryCards[entry].transform.Find("QuantityBadge").GetComponentInChildren<TextMeshProUGUI>();badge.text="×"+n;
            }
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryContent);
            // Preserve scroll position on refresh; initial inventory opens at its top.
        }
        void BuildFusionControls(Transform card)
        {
            upgradePreview=BoxText("UpgradePreview",card,"",15,new Color(.73f,.85f,.93f),new Vector2(.035f,.405f),new Vector2(.965f,.485f));
            var upgrade=PageButton("EquipmentUpgrade",card,"",new Vector2(.12f,.315f),new Vector2(.88f,.40f),gold,()=>
            {
                int id=selectedItem,r=selectedRarity;if(game.UpgradeEquipment(id))InspectItem(id,r);
            });
            upgradeButton=upgrade.GetComponent<UnityEngine.UI.Button>();upgradeLabel=upgrade.GetComponentInChildren<TextMeshProUGUI>();upgradeLabel.fontSize=20;upgradeLabel.color=ink;
            ButtonIcon(upgrade,SquadIcon.Coin,.20f);
            mergePreview=BoxText("MergePreview",card,"",15,new Color(.73f,.85f,.93f),new Vector2(.045f,.225f),new Vector2(.955f,.315f));
            var action=PageButton("EquipmentMerge",card,"",new Vector2(.12f,.135f),new Vector2(.88f,.22f),new Color(.30f,.57f,.32f),()=>
            {
                int id=selectedItem,r=selectedRarity;
                if(game.MergeEquipment(id,r)) InspectItem(id,r+1);
            });
            mergeButton=action.GetComponent<UnityEngine.UI.Button>();mergeLabel=action.GetComponentInChildren<TextMeshProUGUI>();mergeLabel.fontSize=21;
            ButtonIcon(action,SquadIcon.Merge,.23f);
        }
        void RefreshFusionControls()
        {
            int level=game.Equipment.Level(selectedItem),cost=game.Equipment.UpgradeCost(selectedItem);
            bool owned=game.Equipment.Owns(selectedItem),levelMax=level>=SquadRushEquipment.MaxLevel;
            upgradeButton.interactable=game.Account.CanUpgradeEquipment(selectedItem);
            upgradePreview.text=!owned?"入手するとレベルアップできます":levelMax?"最高レベルです / 同じ種類でレベル共通":
                "Lv."+level+" → Lv."+(level+1)+"  /  "+SquadRushEquipment.Effect(selectedItem,selectedRarity,level+1)+"\n同じ種類でレベル共通・合成後も引き継ぎ";
            upgradeLabel.text=!owned?"未入手":levelMax?"Lv.20 MAX":"強化 "+cost.ToString("N0")+"コイン"+(game.Account.Coins<cost?" / 不足":"");
            int n=game.Equipment.Quantity(selectedItem,selectedRarity);
            bool max=selectedRarity==SquadRushEquipment.RarityCount-1;
            mergeButton.interactable=game.Equipment.CanMerge(selectedItem,selectedRarity);
            mergePreview.text=max?"最高レアリティです":SquadRushEquipment.RarityName(selectedRarity)+" ×3 → "+SquadRushEquipment.RarityName(selectedRarity+1)+" ×1\n"+SquadRushEquipment.Effect(selectedItem,selectedRarity+1,level);
            mergeLabel.text=max?"強化完了":n>=3?"3個を合成する":"素材 "+n+" / 3個";
        }
    }
    // Recalculate during the layout pass so tall phone aspect ratios retain all five columns.
    public sealed class SquadRushInventoryLayout : UnityEngine.UI.GridLayoutGroup
    {
        public override void SetLayoutHorizontal()
        {
            float width=Mathf.Max(1,(rectTransform.rect.width-padding.horizontal-spacing.x*4)/5);
            if(!Mathf.Approximately(cellSize.x,width))cellSize=new Vector2(width,101);
            base.SetLayoutHorizontal();
        }
    }
}
