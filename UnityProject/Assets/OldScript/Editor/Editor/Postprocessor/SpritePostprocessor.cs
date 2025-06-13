using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

/// <summary>
/// 图集导入管线。
/// </summary>
public class SpritePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;

        var path = AssetDatabase.GetAssetPath(textureImporter);
        if (!path.Contains(@"Assets/Game/Sprites")
            &&!path.Contains(@"Assets/Game/CachePreviews")
            &&!path.Contains(@"Assets/Game/DynamicResource/UltimateGift")
            )//主要是针对是否导入项目某个文件夹下进行判断
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        // textureImporter.spriteImportMode = SpriteImportMode.Single;
        //设置可读
        // textureImporter.isReadable = false;
        //
    }

  

}
