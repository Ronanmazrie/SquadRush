using System;
using UnityEditor;
using UnityEngine;

namespace SquadRush.Editor
{
    public static class EquipmentArtImporter
    {
        static readonly string[] Names={"hero","scout-rifle","ranger-vest","quick-magazine","assault-rifle","guardian-vest","rapid-magazine"};
        public static void Prepare()
        {
            string[] stages={"desert-outpost","forest-checkpoint","snow-radar","desert-refinery","forest-bridge","snow-fortress"};
            foreach(string name in stages) Import("Assets/Resources/StageArt/"+name+".png",1024);
            foreach(string name in Names)
            {
                string path="Assets/Resources/EquipmentArt/"+name+".png";
                Import(path,name=="hero"?1024:512);
            }
        }
        static void Import(string path,int size)
        {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null) throw new InvalidOperationException("Missing equipment illustration: "+path);
                if(importer.textureType==TextureImporterType.Sprite && importer.spriteImportMode==SpriteImportMode.Single && importer.maxTextureSize==size && !importer.mipmapEnabled && importer.alphaIsTransparency && !importer.isReadable) return;
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                importer.maxTextureSize=size; importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
                importer.alphaSource=TextureImporterAlphaSource.FromInput; importer.isReadable=false;
                importer.filterMode=FilterMode.Bilinear; importer.wrapMode=TextureWrapMode.Clamp;
                var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
        }
    }
}
