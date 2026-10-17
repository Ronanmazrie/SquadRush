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
            foreach(string name in Names)
            {
                string path="Assets/Resources/EquipmentArt/"+name+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null) throw new InvalidOperationException("Missing equipment illustration: "+path);
                int size=name=="hero"?1024:512;
                if(importer.textureType==TextureImporterType.Sprite && importer.spriteImportMode==SpriteImportMode.Single && importer.maxTextureSize==size && !importer.mipmapEnabled && importer.alphaIsTransparency && !importer.isReadable) continue;
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
}
