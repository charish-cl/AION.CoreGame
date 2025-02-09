using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.U2D;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    [TreeWindow("资源工具/图集工具")]
    public class SpriteAtlasPacker : OdinEditorWindow
    {
        public List<Texture2D> Texture2Ds = new List<Texture2D>();

        [ReadOnly] public string selectedFolderPath;

        [ReadOnly] public string projectFolderPath = "Assets/Game/Sprites";


        [ReadOnly] public string atlasOutputPath = "Assets/Game/SpriteAtlas";

        public List<string> ExcludeDir = new List<string>
        {
            "Assets/Game/Sprites/Dynamic",
            "Assets/Game/DynamicResource/Map",
            "Assets/Game/DynamicResource/AllCard"
        };

        [LabelText("移动到的指定文件夹")] [FolderPath] public string TargetFolder;

        public Vector2 MaxSize = new Vector2(500, 500);

        [Button("获取不符合规范的图片", ButtonHeight = 30)]
        public void FindOverFlowSprite()
        {
            Texture2Ds = SelectionExtend.GetSelectFolderAllAsset()
                .OfType<Texture2D>()
                .Where(texture2D => texture2D.width > MaxSize.x || texture2D.height > MaxSize.y)
                .ToList();
        }

        [Button("移动到指定文件夹", ButtonHeight = 30)]
        public void MoveAllToTarget()
        {
            if (!Directory.Exists(TargetFolder))
            {
                Debug.LogError($"目标文件夹不存在: {TargetFolder}");
                return;
            }

            foreach (var texture2D in Texture2Ds)
            {
                MoveTextureToTargetFolder(texture2D);
            }

            AssetDatabase.Refresh();
        }

        private void MoveTextureToTargetFolder(Texture2D texture2D)
        {
            string path = AssetDatabase.GetAssetPath(texture2D);
            string targetPath = Path.Combine(TargetFolder, Path.GetFileName(path));

            if (File.Exists(targetPath))
            {
                Debug.LogWarning($"目标文件夹中已存在文件: {targetPath}");
                return;
            }

            AssetDatabase.MoveAsset(path, targetPath);
        }

        [Button("打图集当前文件夹下所有子文件夹", ButtonHeight = 30)]
        public void PackSpritesAllIntoAtlas(string InputFolder = null)
        {
            InputFolder ??= SelectionExtend.GetCurrentAssetDirectory();

            EnsureDirectoryExists(atlasOutputPath);

            string[] directories = Directory.GetDirectories(InputFolder, "*", SearchOption.TopDirectoryOnly);

            directories = directories.Select(e => e.Replace("\\", "/")).ToArray();
            foreach (string directory in directories)
            {
                bool IsContains = ExcludeDir.Any(s => directory.Contains(s));

                if (!IsContains)
                {
                    AddDirectoryToAtlas(directory);
                }
                else
                {
                    Debug.Log($"排除{directory}");
                }
            }

            FinalizeAtlasPacking();
        }

        [Button("对项目打图集", ButtonHeight = 30)]
        public void PackProject()
        {
            //这里要把图集先清空，否则会把之前的图集的图片也打进去，导致图集冗余重复
            string[] atlasPaths = AssetDatabase.FindAssets("t:spriteatlas", new[] { atlasOutputPath });
            foreach (string atlasPath in atlasPaths)
            {
                string atlasFullPath = AssetDatabase.GUIDToAssetPath(atlasPath);
                AssetDatabase.DeleteAsset(atlasFullPath);
            }

            PackSpritesAllIntoAtlas(projectFolderPath);
        }

        [Button("打当前文件夹图集", ButtonHeight = 30)]
        public void PackSpritesIntoAtlas()
        {
            selectedFolderPath = SelectionExtend.GetCurrentAssetDirectory();
            EnsureDirectoryExists(atlasOutputPath);
            AddDirectoryToAtlas(selectedFolderPath);
            FinalizeAtlasPacking();
        }

        private void AddDirectoryToAtlas(string directoryPath)
        {
            List<Object> list = null;
            try
            {
                list = SelectionExtend.AssetDataBaseGetAllFolderAsset(directoryPath, ExcludeDir);
            }
            catch (Exception e)
            {
                return;
            }

            if (list == null || list.Count == 0) return; 
            List<Texture2D> texturesToPack = list.OfType<Texture2D>()
                .Where(texture2D => texture2D.width <= MaxSize.x && texture2D.height <= MaxSize.y)
                .ToList();
            
            string atlasPath = Path.Combine(atlasOutputPath, Path.GetFileName(directoryPath) + ".spriteatlas");
            SpriteAtlas spriteAtlas = CreateOrLoadAtlas(atlasPath);


            if (texturesToPack.Any())
            {
                spriteAtlas.Add(texturesToPack.ToArray());
            }

            if (!AssetDatabase.Contains(spriteAtlas))
            {
                AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
            }
        }

        private SpriteAtlas CreateOrLoadAtlas(string atlasPath)
        {
            SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (spriteAtlas == null)
            {
                spriteAtlas = new SpriteAtlas();

                //这个设置为发false，图片加载为白色，不知道为什么
                // spriteAtlas.SetIncludeInBuild(false);
                spriteAtlas.SetPackingSettings(new SpriteAtlasPackingSettings
                {
                    blockOffset = 1,
                    padding = 2,
                    enableRotation = false,
                    enableTightPacking = false
                });
                spriteAtlas.SetTextureSettings(new SpriteAtlasTextureSettings
                {
                    readable = false,
                    generateMipMaps = false,
                    sRGB = true,
                    filterMode = FilterMode.Bilinear
                });
            }

            return spriteAtlas;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void FinalizeAtlasPacking()
        {
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Sprite Atlas packing complete.");
        }
    }
}