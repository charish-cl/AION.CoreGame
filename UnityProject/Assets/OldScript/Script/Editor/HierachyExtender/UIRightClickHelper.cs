using System.IO;
using System.Text;
#if SPINE_EXISTS
using Spine.Unity;
#endif
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace GameDevKitEditor
{
    /// <summary>
    /// 右键点击的一些帮助方法
    /// </summary>
    public class UIRightClickHelper
    {
        [MenuItem("GameObject/UIHelper/调整父物体大小", false, 0)]
        public static void  AjustParentSize()
        {
            // 获取父物体的 RectTransform 组件
            RectTransform parentRectTransform = Selection.activeTransform.GetComponent<RectTransform>();

            // 初始化父物体的边界框
            Bounds parentBounds = new Bounds(Vector3.zero, Vector3.zero);

            // 遍历所有子物体
            foreach (RectTransform childRectTransform in parentRectTransform)
            {
                // 获取子物体的边界框
                Bounds childBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRectTransform, childRectTransform);

                // 扩展父物体的边界框以包含子物体的边界框
                parentBounds.Encapsulate(childBounds.min);
                parentBounds.Encapsulate(childBounds.max);
            }

            // 将父物体的宽度和高度设置为子物体的包围盒的宽度和高度
            parentRectTransform.sizeDelta = new Vector2(parentBounds.size.x, parentBounds.size.y);
        }
        [MenuItem("GameObject/UIHelper/Copy RectTransform Properties", false, 0)]
        public static void CopyRectTransformProperties()
        {
            // 获取选中的对象
            GameObject[] selectedObjects = Selection.gameObjects;

            // 确保至少选中了两个对象
            if (selectedObjects.Length < 2)
            {
                Debug.LogWarning("Please select at least two GameObjects to copy RectTransform properties.");
                return;
            }

            // 获取选中对象的 RectTransform 组件
            RectTransform rectTransform1 = selectedObjects[0].GetComponent<RectTransform>();
            RectTransform rectTransform2 = selectedObjects[1].GetComponent<RectTransform>();

            // 确保选中的对象都有 RectTransform 组件
            if (rectTransform1 == null || rectTransform2 == null)
            {
                Debug.LogWarning("Selected GameObjects must have RectTransform components.");
                return;
            }

            // 复制 RectTransform 属性
            CopyRectTransformProperties(rectTransform1, rectTransform2);
        }

        public static void CopyRectTransformProperties(RectTransform source, RectTransform destination)
        {
            // 复制位置
            destination.position = source.position;

            // 复制尺寸
            destination.sizeDelta = source.sizeDelta;

            // 复制其他属性，例如锚点、偏移量等
            // 这里只是示例，你可以根据需求添加其他属性的复制
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.offsetMin = source.offsetMin;
            destination.offsetMax = source.offsetMax;

            // 其他属性的复制类似，根据需要进行调整
        }


        public const string UIComponentPath = "Assets/Game/UIComponent";
        public const string UIFormPath = "Assets/Game/UIForm/LocalResources";
        public const string UISpritePath = "Assets/Game/Sprites";
        
        [MenuItem("GameObject/UIHelper/CreateUI Prefab")]
        public static void CreateUIPrefab()
        {
            // 获取选中的对象

            var selectedObject = Selection.activeGameObject;

            //打开路径选择窗口
            string path = EditorUtility.SaveFilePanel("Save Prefab", UIFormPath, selectedObject.name, "prefab");
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObject,path, InteractionMode.UserAction);
        }
        [MenuItem("GameObject/UIHelper/CreateUIComponent Prefab")]
        public static void CreateUIComponentPrefab()
        {
            // 获取选中的对象

            var selectedObject = Selection.activeGameObject;
            var targetPath = Path.Combine(UISpritePath, selectedObject.name);
            //目标没有创建
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            
            //打开路径选择窗口
            string path = EditorUtility.SaveFilePanel("Save Prefab", UIComponentPath, selectedObject.name, "prefab");
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObject,path, InteractionMode.UserAction);
        }

        [MenuItem("GameObject/UIHelper/Move UISprites")]
        public static void CreateUISpriteFolder()
        {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null) return;

            var images = selectedObject.GetComponentsInChildren<Image>();
            var targetPath = Path.Combine(UISpritePath, selectedObject.name);
            //目标没有创建
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
                
            string savePath = EditorUtility.SaveFolderPanel("Select Save Folder", UISpritePath, selectedObject.name);
            
            if (string.IsNullOrEmpty(savePath)) return;
            
            foreach (var image in images)
            {
                if (image.sprite == null) continue;
                string assetPath = AssetDatabase.GetAssetPath(image.sprite);
                if (!assetPath.StartsWith("Assets/Game/Sprites/Common"))
                {
                    string destPath = Path.Combine(savePath, Path.GetFileName(assetPath));
                    destPath= IOUtility.GetAssetPath(destPath);
                    Debug.Log(destPath);
                    AssetDatabase.MoveAsset(assetPath, destPath);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Sprites moved successfully.");
        }

      
        [MenuItem("GameObject/UIHelper/Get Animator Anim Names")]
        public static void ListAnimatorAnimations()
        {
            // 获取当前选中的 GameObject
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Selected GameObject does not have an Animator component.");
                return;
            }

            var controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning("Animator does not have a valid AnimatorController.");
                return;
            }

            // 构建类的代码
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public class {go.name}Animations");
            sb.AppendLine("{");

            // 获取 AnimatorController 中的所有动画片段
            foreach (var clip in controller.animationClips)
            {
                string constName = clip.name.Replace(" ", "_"); // 确保常量名合法
                sb.AppendLine($"    public const string {constName} = \"{clip.name}\";");
                Debug.Log($"Animation Clip Name: {clip.name}");
            }

            sb.AppendLine("}");

            // 将生成的代码复制到剪贴板
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("Animation clip names have been copied to the clipboard as a class.");
        }


        [MenuItem("GameObject/UIHelper/CreateUltimateFolder")]
        public static void CreateUltimateFolder()
        {
            var selectedObject = Selection.activeGameObject;

            var childCount = selectedObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = selectedObject.transform.GetChild(i);
                var childName = child.name;
                var left = child.GetChild(0);
                left.name = "Left";
                var right = child.GetChild(1);
                right.name = "Right";
                var title = child.GetChild(2);
                title.name = "Title";
                
                IOUtility.CreateDirectoryIfNotExists(Path.Combine("Assets/Game/DynamicResource/UltimateGift", child.name));

                RenameSpriteAndMove(left);
                RenameSpriteAndMove(right);
                RenameSpriteAndMove(title);

                void RenameSpriteAndMove(Transform t)
                {
                    var image = t.GetComponent<Image>();
                    FileUtil.MoveFileOrDirectory(
                        Path.Combine("Assets/Game/DynamicResource/UltimateGift", image.sprite.name+".png"), 
                        Path.Combine("Assets/Game/DynamicResource/UltimateGift", child.name+"/"+t.name+".png"));

                }

            }

            
        }
      
        #region Spine相关功能
#if SPINE_EXISTS
        using Spine.Unity;

        [MenuItem("GameObject/UIHelper/Get Spine Anim Name")]
        public static void ListSpineAnimations()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            var skeletonAnimation = go.GetComponent<SkeletonGraphic>();
            if (skeletonAnimation == null)
            {
                Debug.LogWarning("Selected GameObject does not have a SkeletonGraphic component.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public  class {go.name}");
            sb.AppendLine("{");

            var skeletonData = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(true);
            if (skeletonData != null)
            {
                foreach (var animation in skeletonData.Animations)
                {
                    string constName = animation.Name.Replace(" ", "_");
                    sb.AppendLine($"    public const string {constName} = \"{animation.Name}\";");
                    Debug.Log($"Animation Name: {animation.Name}");
                }
            }

            sb.AppendLine("}");
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("Animation names have been copied to the clipboard as a class.");
        }
#endif
        #endregion

    }

}