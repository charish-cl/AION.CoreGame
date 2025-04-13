using System.Reflection;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameDevKitEditor
{
    [TreeWindow("资源工具/Texture压缩")]
  
    public class TextureCompressWindow: OdinEditorWindow
    {
        private const string AndroidPlatformName = "Android";

        private const string IOSPlatformName = "iPhone";
        private const string WebGL = "WebGL";

        [FolderPath]
        public string SpritesPath = "Assets/Game/Sprites/";
        [Button("Tools/Android/CompressTexture")]
        private  void CompressTextureOnAndroid()
        {
            CompressInPlotform(AndroidPlatformName);
        }

        [Button("Tools/IOS/CompressTexture")]
        private  void CompressTextureOnIOS()
        {
            CompressInPlotform(IOSPlatformName);
        }

        [Button("Tools/WebGL/CompressTexture")]
        private  void CompressTextureOnWebGL()
        {
            CompressInPlotform(WebGL);
        }

        [Button("Tools/WebGL/SetRawCompressTexture")]
        private  void SetRawCompressTexture()
        {
            CompressInPlotform(WebGL, true);
        }


      


        private  void CompressInPlotform(string platform, bool isRawSize = false)
        {
            var message = "You are going to perform the following operation to ALL Texture2D! Continue?\n";

            //弹出对话框进行二次确认
            if (EditorUtility.DisplayDialog("Change Max Size", message, "OK", "Cancel") == false)
            {
                return;
            }

            // 显示进度条
            EditorUtility.DisplayProgressBar("Change Max Size", "", 0.0f);

            // 找到所有纹理
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesPath });
            for (int i = 0; i < guids.Length; i++)
            {
                var dialogTitle = "Change Max Size" + " (" + i + "/" + guids.Length + ")";
                var progress = (float)i / guids.Length;
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (go == null)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(dialogTitle, go.name, progress);

                //拿到TextureImporter才能对图片进行压缩
                var assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter != null)
                {
                    var textureImporter = assetImporter as TextureImporter;
                    if (textureImporter == null)
                    {
                        Debug.LogWarningFormat(go, "MissingImporter Texture2D: '{0}'", go.name);
                        continue;
                    }

                    //拿到不压缩前的size
                    var defaultMaxSize = textureImporter.maxTextureSize;//这个是默认平台的MaxSize
                    TextureImporterPlatformSettings platformImporter =
                        textureImporter.GetPlatformTextureSettings(platform);
                    platformImporter.overridden = false;
                    platformImporter.maxTextureSize = 2048;

                    var width = 0;
                    var height = 0;
                    //拿到宽高
                    GetTextureRealWidthAndHeight(textureImporter, ref width, ref height);

                    //判断有无alpha
                    var haveAlpha = textureImporter.DoesSourceTextureHaveAlpha();
                    //Log defaultMaxSize Width Height HaveAlpha
                    Debug.LogFormat(go, "DefaultMaxSize: '{0}', Width: '{1}', Height: '{2}', HaveAlpha: '{3}'",
                        defaultMaxSize, width, height, haveAlpha);
                 //   platformImporter.format = GetCompressFormat(platform, width, height, haveAlpha);

                    textureImporter.SetPlatformTextureSettings(platformImporter);

                    EditorUtility.SetDirty(go);
                    Debug.LogFormat(go, "Processed Texture2D: '{0}'", go.name);
                    textureImporter.SaveAndReimport();
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        // 得到目标压缩格式
        private  TextureImporterFormat GetCompressFormat(string platform, int width, int height, bool haveAlpha)
        {
            //分几种情况讨论，是不是2的次幂以及是不是有alpha通道
            var isPowerOfTwo = WidthAndHeightIsPowerOfTwo(width, height);
            if (isPowerOfTwo == false)
            {
                if (haveAlpha)
                {
                    return TextureImporterFormat.RGBA16;
                }
                else
                {
                    return TextureImporterFormat.RGB16;
                }
            }
            else
            {
                if (platform == AndroidPlatformName)
                {
                    if (haveAlpha)
                    {
                        return TextureImporterFormat.ETC2_RGBA8Crunched;
                    }
                    else
                    {
                        return TextureImporterFormat.ETC_RGB4Crunched;
                    }
                }
                else if (platform == IOSPlatformName)
                {
                    if (haveAlpha)
                    {
                        return TextureImporterFormat.PVRTC_RGBA4;
                    }
                    else
                    {
                        return TextureImporterFormat.PVRTC_RGB4;
                    }
                }
                //WebGL
                else if (platform == WebGL)
                {
                    if (haveAlpha)
                    {
                        return TextureImporterFormat.DXT5;
                    }
                    else
                    {
                        return TextureImporterFormat.DXT5;
                    }
                }
            }

            return TextureImporterFormat.RGBA16;
        }

        // 判断宽和高是否是2的次幂
        private  bool WidthAndHeightIsPowerOfTwo(int width, int height)
        {
            if (IsPowerOfTwo(width) && IsPowerOfTwo(height))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //用二进制与运算，如果一个数是2的次幂，n&(n-1) = 0
        private  bool IsPowerOfTwo(int number)
        {
            if (number <= 0)
            {
                return false;
            }

            return (number & (number - 1)) == 0;
        }

        //用反射得到纹理宽高
        public  void GetTextureRealWidthAndHeight(TextureImporter texImpoter, ref int width, ref int height)
        {
            System.Type type = typeof(TextureImporter);
            System.Reflection.MethodInfo method =
                type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            var args = new object[] { width, height };
            method.Invoke(texImpoter, args);
            width = (int)args[0];
            height = (int)args[1];
        }


        [Button]
        public void Test()
        {
            Debug.Log(GetNextPowerOfTwo(42));
        }
        //得到下一个2的次幂
        public static int GetNextPowerOfTwo(int number)
        {
            if (number < 1)
                return 1;

            number--;
            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;

            return number + 1;
        }

    }
}