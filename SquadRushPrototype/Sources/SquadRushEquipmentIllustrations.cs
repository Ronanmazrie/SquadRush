using System;
using UnityEngine;

namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        Sprite[] itemIllustrations;
        Sprite heroIllustration;
        UnityEngine.UI.Image[] inventoryIllustrations;
        UnityEngine.UI.Image detailIllustration, detailFrame;
        TMPro.TextMeshProUGUI detailComparison;
        TMPro.TextMeshProUGUI[] equipmentStats;
        void LoadEquipmentIllustrations()
        {
            heroIllustration=RequiredIllustration("hero");
            string[] names={"scout-rifle","ranger-vest","quick-magazine","assault-rifle","guardian-vest","rapid-magazine"};
            itemIllustrations=new Sprite[names.Length];
            for(int i=0;i<names.Length;i++) itemIllustrations[i]=RequiredIllustration(names[i]);
        }
        static Sprite RequiredIllustration(string name)
        {
            var sprite=Resources.Load<Sprite>("EquipmentArt/"+name);
            if(sprite==null) throw new InvalidOperationException("Equipment illustration was not imported: "+name);
            return sprite;
        }
        UnityEngine.UI.Image Illustration(string name,Transform parent,Sprite sprite,Vector2 min,Vector2 max)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); obj.transform.SetParent(parent,false);
            SetAnchors(obj.GetComponent<RectTransform>(),min,max);
            var art=obj.GetComponent<UnityEngine.UI.Image>(); art.sprite=sprite; art.preserveAspect=true; art.raycastTarget=false; return art;
        }
        static Color ItemFrame(int id) => id>=3?new Color(0.77f,0.53f,0.21f):new Color(0.22f,0.52f,0.69f);
        static string ItemCaption(int id)
        {
            string[] names={"スカウト","レンジャー","クイック","アサルト","ガーディアン","高速"}; return names[id];
        }
        void UpdateDetailComparison(int id,int rarity)
        {
            EquipmentItem item=SquadRushEquipment.Item(id); int current=game.Equipment.Equipped(item.Slot);
            int delta=game.Equipment.CurrentValue(id,rarity)-(current>=0?game.Equipment.CurrentValue(current,game.Equipment.EquippedRarity(item.Slot)):0);
            string unit=item.Slot==EquipmentSlot.Support?"%":"";
            detailComparison.text=game.Equipment.IsEquipped(id,rarity)?"現在装備しているアイテム":delta==0?"現在の装備と同じ効果":"現在の装備から "+(delta>0?"+":"")+delta+unit;
            detailComparison.color=delta>0?new Color(0.40f,0.92f,0.62f):delta<0?new Color(1,0.50f,0.44f):new Color(0.70f,0.82f,0.90f);
        }
    }
}
