namespace GameDevKitEditor
{
 using UnityEngine;
using UnityEditor;

public class SetTextureToPowerOfTwo : Editor
{
    [MenuItem("Tools/Set Texture to Power of Two")]
    public static void SetSelectedTexturesToPowerOfTwo()
    {
        // 获取当前选中的所有纹理
        Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

        foreach (Object obj in textures)
        {
            Texture2D texture = obj as Texture2D;
            if (texture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(texture);
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (textureImporter != null)
                {
                    // 读取纹理设置
                    TextureImporterSettings settings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(settings);

                    // 确保纹理可读写
                    if (!textureImporter.isReadable)
                    {
                        textureImporter.isReadable = true;
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    }

                    // 调整纹理尺寸为2的幂次方
                    Texture2D resizedTexture = ResizeTextureToPowerOfTwo(texture);

                    // 覆盖原始纹理
                    byte[] bytes = resizedTexture.EncodeToPNG();
                    System.IO.File.WriteAllBytes(assetPath, bytes);

                    // 更新资产数据库
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    // 恢复原始纹理设置
                    textureImporter.SetTextureSettings(settings);
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    // 清理内存
                    DestroyImmediate(resizedTexture);

                    Debug.Log($"Texture '{texture.name}' resized to {resizedTexture.width}x{resizedTexture.height}.");
                }
            }
        }
    }

    private static Texture2D ResizeTextureToPowerOfTwo(Texture2D texture)
    {
        int width = Mathf.NextPowerOfTwo(texture.width);
        int height = Mathf.NextPowerOfTwo(texture.height);

        // 创建一个新的2的幂次方尺寸的纹理，使用RGBA32格式确保有透明度
        Texture2D resizedTexture = new Texture2D(width, height, TextureFormat.RGBA32, texture.mipmapCount > 1);
        resizedTexture.name = texture.name;

        // 填充透明
        Color32[] fillColor = new Color32[width * height];
        for (int i = 0; i < fillColor.Length; i++)
        {
            fillColor[i] = new Color32(0, 0, 0, 0); // 透明色
        }
        resizedTexture.SetPixels32(fillColor);

        // 计算原始纹理在新纹理中的位置，使其居中
        int xOffset = (width - texture.width) / 2;
        int yOffset = (height - texture.height) / 2;

        // 拷贝原始纹理的像素到新的纹理
        Color32[] originalPixels = texture.GetPixels32();
        resizedTexture.SetPixels32(xOffset, yOffset, texture.width, texture.height, originalPixels);
        resizedTexture.Apply();

        return resizedTexture;
    }
}


}