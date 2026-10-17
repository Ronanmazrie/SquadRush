using System;
using UnityEngine;
namespace SquadRush
{
    public sealed partial class SquadRushHud
    {
        UnityEngine.UI.Image homeStageArt,stageSelectionArt;
        TMPro.TextMeshProUGUI homeLocation;
        Sprite[] stageIllustrations;
        static readonly string[] StageArtNames={"desert-outpost","forest-checkpoint","snow-radar","desert-refinery","forest-bridge","snow-fortress"};
        static readonly string[] StageLocationNames={"砂塵の前哨基地","緑陰の検問所","白銀の観測基地","夕焼けの精製所","霧の鉄橋","氷壁の要塞"};
        static string StageLocation(int stage)=>new SquadRushStage(stage).Location;
        UnityEngine.UI.Image StagePicture(string name,Transform parent,Vector2 min,Vector2 max)
        {
            if(stageIllustrations==null)
            {
                stageIllustrations=new Sprite[StageArtNames.Length];
                for(int i=0;i<stageIllustrations.Length;i++)
                {
                    stageIllustrations[i]=Resources.Load<Sprite>("StageArt/"+StageArtNames[i]);
                    if(stageIllustrations[i]==null)throw new InvalidOperationException("Missing stage illustration: "+StageArtNames[i]);
                }
            }
            var border=Panel(name+"Frame",parent,new Color(.30f,.46f,.52f),min,max);
            border.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
            var image=Illustration(name,border,stageIllustrations[0],new Vector2(.008f,.008f),new Vector2(.992f,.992f));image.preserveAspect=false;
            var fitter=image.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fitter.aspectRatio=stageIllustrations[0].rect.width/stageIllustrations[0].rect.height;fitter.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            return image;
        }
        void RefreshStagePictures()
        {
            int index=(game.StageNumber-1)%stageIllustrations.Length;
            homeStageArt.sprite=stageIllustrations[index];stageSelectionArt.sprite=stageIllustrations[index];
            float aspect=stageIllustrations[index].rect.width/stageIllustrations[index].rect.height;
            homeStageArt.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio=aspect;stageSelectionArt.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio=aspect;
            homeLocation.text=StageLocation(game.StageNumber);
        }
    }
}
