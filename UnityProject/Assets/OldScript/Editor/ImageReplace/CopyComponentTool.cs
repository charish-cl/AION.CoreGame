using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class CopyComponentTool : OdinEditorWindow
{
    [MenuItem("Tools/拷贝组件工具")]
    static void OpenWindow()
    {
        GetWindow<CopyComponentTool>();
    }

    [TabGroup("拷贝组件")] public Queue<GameObject> queue = new Queue<GameObject>();
    [TabGroup("拷贝组件")] public GameObject current;

    void Update()
    {
        var selected = Selection.activeGameObject;
        if (selected == null || selected == current)
        {
            return;
        }

        current = selected;

        queue.Enqueue(current);

        if (queue.Count > 2)
        {
            queue.Dequeue();
        }
    }


    [TabGroup("拷贝组件")]
    [Button("Copy Component", ButtonSizes.Large)]
    public void Copy()
    {
        if (queue.Count < 2)
        {
            return;
        }

        var source = queue.Dequeue();
        var target = queue.Dequeue();
        if (source == null || target == null)
        {
            return;
        }

        Copy(source, target);

        EditorUtility.SetDirty(target);
    }

    [TabGroup("拷贝组件")]
    [Button("拷贝RectTransform组件", ButtonSizes.Large)]
    public void CopyOnlyRectTransform()
    {
        if (queue.Count < 2)
        {
            return;
        }

        var source = queue.Dequeue();
        var target = queue.Dequeue();
        if (source == null || target == null)
        {
            return;
        }

        var temp = GameObject.Instantiate(source, target.transform.parent, true);

        var rectTransform = temp.GetComponent<RectTransform>();
        var targetRectTransform = target.GetComponent<RectTransform>();
        UnityEditorInternal.ComponentUtility.CopyComponent(rectTransform);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(targetRectTransform);
        if (IsNeedCopyDestroy)
        {
            Object.DestroyImmediate(source);
        }

        Object.DestroyImmediate(temp);
    }

    //仅copy样式
    public void CopyOnlyStyle()
    {
    }

    [TabGroup("拷贝组件")]
    [Button("拷贝除了RectTransform的组件", ButtonSizes.Large)]
    public void CopyExceptRectTransform()
    {
        if (queue.Count < 2)
        {
            return;
        }

        var source = queue.Dequeue();
        var target = queue.Dequeue();
        if (source == null || target == null)
        {
            return;
        }

        Copy(source, target, true);
    }

    [TabGroup("拷贝组件")]
    [Button("仅拷贝Text字体样式", ButtonSizes.Large)]
    public void CopyTextStyleOnly()
    {
        if (queue.Count < 2)
        {
            return;
        }

        var source = queue.Dequeue();
        var target = queue.Dequeue();
        if (source == null || target == null)
        {
            return;
        }

        CopyTextStyle(source, target);
    }

    [TabGroup("拷贝组件")]
    [Button("仅拷贝Image和尺寸", ButtonSizes.Large)]
    public void CopyImageAndSizeOnly()
    {
        if (queue.Count < 2)
        {
            return;
        }

        var source = queue.Dequeue();
        var target = queue.Dequeue();
        if (source == null || target == null)
        {
            return;
        }

        CopyImageAndSize(source, target);
    }

    [LabelText("拷贝后删除原物体")] public bool IsNeedCopyDestroy = false;
    [LabelText("拷贝后隐藏原物体")] public bool IsNeedHide = false;
    [LabelText("RectTransform只拷贝位置和尺寸")] public bool PreserveAnchorPivot = true;

    public static void Copy(GameObject source, GameObject target, bool isExceptRectTransform = false,
        bool isNeedRemove = false, bool preserveAnchorPivot = false)
    {
        var temp = GameObject.Instantiate(source, target.transform.parent, true);

        Canvas.ForceUpdateCanvases();
        CopyComponent(temp, target, isExceptRectTransform, isNeedRemove, preserveAnchorPivot);

        // if (IsNeedCopyDestroy)
        // {
        //     Object.DestroyImmediate(source);
        // }

        Object.DestroyImmediate(temp);
    }

    static void CopyComponent(GameObject source, GameObject target, bool isExceptRectTransform = false,
        bool isNeedRemove = false, bool preserveAnchorPivot = false)
    {
        var SourceComponets = source.GetComponents<Component>().ToDictionary(e => e.GetType(), e => e);
        var TargetComponets = target.GetComponents<Component>().ToDictionary(e => e.GetType(), e => e);

        // 特殊处理Text组件：如果源对象有Text但没有DodTextOutline，目标对象有DodTextOutline，则移除并清除material
        var sourceText = source.GetComponent<Text>();
        var targetText = target.GetComponent<Text>();

        // if (sourceText != null && targetText != null)
        // {
        //   
        //     var sourceDodTextOutline = source.GetComponent<DodTextOutline>();
        //     var targetDodTextOutline = target.GetComponent<DodTextOutline>();
        //     
        //     // 如果源对象没有DodTextOutline，但目标对象有
        //     if (sourceDodTextOutline == null && targetDodTextOutline != null)
        //     {
        //         Debug.Log($"移除目标对象 {target.name} 的DodTextOutline组件并清除material");
        //         Object.DestroyImmediate(targetDodTextOutline);
        //         targetText.material = null;
        //         EditorUtility.SetDirty(target);
        //     }
        //     
        // }

        //先移除目标对象上多余的组件
        foreach (var componet in TargetComponets)
        {
            if (SourceComponets.TryGetValue(componet.Key, out var sourceComponet))
            {
            }
            else
            {
                if (isNeedRemove)
                {
                    Debug.Log($"移除组件：{componet.Value.gameObject.name} 身上{componet.Key.Name}");
                    Object.DestroyImmediate(componet.Value);
                }
            }
        }

        foreach (var componet in SourceComponets)
        {
            if (componet.Key == typeof(RectTransform) && isExceptRectTransform)
            {
                continue;
            }

            if (componet.Key == typeof(ScrollRect) || componet.Key == typeof(Button))
            {
                continue;
            }

            Component targetComponet = null;
            if (TargetComponets.TryGetValue(componet.Key, out targetComponet))
            {
                Debug.Log("复制组件：" + componet.Key.Name);
            }
            //给目标添加组件
            else
            {
                Debug.Log("添加组件：" + componet.Key.Name);
                targetComponet = target.AddComponent(componet.Key);
            }

            // 特殊处理RectTransform：如果preserveAnchorPivot为true，则只复制位置和尺寸
            if (componet.Key == typeof(RectTransform) && preserveAnchorPivot)
            {
                var sourceRect = componet.Value as RectTransform;
                var targetRect = targetComponet as RectTransform;
                if (sourceRect != null && targetRect != null)
                {
                    //对面是自适应布局就不要设置anchor和pivot了
                    if (targetRect.anchorMin == Vector2.zero && targetRect.anchorMax == Vector2.one)
                    {
                        continue;
                    }

                    // 保持目标的anchor和pivot不变，只复制位置、尺寸、旋转、缩放
                    targetRect.position = sourceRect.position;
                    targetRect.sizeDelta = sourceRect.sizeDelta;
                    targetRect.localRotation = sourceRect.localRotation;
                    targetRect.localScale = sourceRect.localScale;

                    EditorUtility.SetDirty(targetRect);
                }
            }
            else
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(componet.Value);
                UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponet);
            }
        }

        EditorUtility.SetDirty(target);
    }

    [MenuItem("GameObject/SetAnchorPivot", false, 1)]
    public static void SetAnchorPivot()
    {
        //设置选中对象的

        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            return;
        }

        var rectTransform = selected.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            return;
        }

        //获取当前的锚点和中心点
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    [TableList] public List<CopyData> CopyList = new List<CopyData>();

    // 静态方法用于从其他工具设置CopyList
    public static void SetCopyList(List<CopyData> copyDataList)
    {
        var window = GetWindow<CopyComponentTool>();
        window.CopyList = copyDataList;
    }

    [Button]
    public void CopyListToTarget()
    {
        if (CopyList.Count == 0)
        {
            return;
        }

        foreach (var copyData in CopyList)
        {
            if (copyData.source == null || copyData.target == null)
            {
                continue;
            }

            Copy(copyData.source, copyData.target, false, IsNeedCopyDestroy);
            if (IsNeedHide && copyData.source != null)
            {
                copyData.source.SetActive(false);
            }
        }
    }


    [MenuItem("GameObject/CopyListByTheFirst", false, 1)]
    public static void CopyListByTheFirst()
    {
        //选中对象
        var gameObjects = Selection.gameObjects;
        if (gameObjects.Length < 2)
        {
            Debug.LogWarning("请至少选择两个同层级对象");
            return;
        }

        // 检查是否为同层级
        Transform parentTransform = gameObjects[0].transform.parent;
        foreach (var obj in gameObjects)
        {
            if (obj.transform.parent != parentTransform)
            {
                Debug.LogWarning("所有选中对象必须在同一层级");
                return;
            }
        }

        // 第一个对象作为源对象
        GameObject firstObject = gameObjects[0];

        // 依次复制到其他对象
        for (int i = 1; i < gameObjects.Length; i++)
        {
            CopyObjectRecursive(firstObject, gameObjects[i]);
        }

        Debug.Log($"<color=green>成功！以 {firstObject.name} 为源，复制到 {gameObjects.Length - 1} 个对象及其子物体</color>");
    }

    // 递归复制对象及其子对象
    private static void CopyObjectRecursive(GameObject source, GameObject target)
    {
        // 复制当前对象的组件
        Copy(source, target, false, false, true);

        // 如果子对象数量不一致，给出警告但继续执行
        int childCount = Mathf.Min(source.transform.childCount, target.transform.childCount);
        if (source.transform.childCount != target.transform.childCount)
        {
            Debug.LogWarning(
                $"子对象数量不一致: {source.name}({source.transform.childCount}) vs {target.name}({target.transform.childCount})，将只复制前{childCount}个子对象");
        }

        // 递归复制子对象
        for (int i = 0; i < childCount; i++)
        {
            GameObject sourceChild = source.transform.GetChild(i).gameObject;
            GameObject targetChild = target.transform.GetChild(i).gameObject;

            CopyObjectRecursive(sourceChild, targetChild);
        }
    }

    // 仅复制Text字体样式（不复制RectTransform和文本内容）
    public static void CopyTextStyle(GameObject source, GameObject target)
    {
        var sourceText = source.GetComponent<Text>();
        var targetText = target.GetComponent<Text>();

        if (sourceText == null || targetText == null)
        {
            Debug.LogWarning("源对象或目标对象没有Text组件");
            return;
        }

        // 保存目标的文本内容
        string originalText = targetText.text;

        // 复制Text组件
        UnityEditorInternal.ComponentUtility.CopyComponent(sourceText);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(targetText);

        // 恢复目标的文本内容
        targetText.text = originalText;

        EditorUtility.SetDirty(target);
        Debug.Log($"<color=green>已复制Text字体样式: {source.name} → {target.name}（保留原文本内容）</color>");
    }

    // 仅复制Image和尺寸（不复制位置）
    public static void CopyImageAndSize(GameObject source, GameObject target)
    {
        var sourceImage = source.GetComponent<Image>();
        var targetImage = target.GetComponent<Image>();
        var sourceRect = source.GetComponent<RectTransform>();
        var targetRect = target.GetComponent<RectTransform>();

        if (sourceImage == null || targetImage == null)
        {
            Debug.LogWarning("源对象或目标对象没有Image组件");
            return;
        }

        if (sourceRect == null || targetRect == null)
        {
            Debug.LogWarning("源对象或目标对象没有RectTransform组件");
            return;
        }

        // 保存目标的位置信息
        Vector3 originalPosition = targetRect.position;
        Vector2 originalAnchoredPosition = targetRect.anchoredPosition;
        Vector2 originalAnchorMin = targetRect.anchorMin;
        Vector2 originalAnchorMax = targetRect.anchorMax;
        Vector2 originalPivot = targetRect.pivot;

        // 复制Image组件
        UnityEditorInternal.ComponentUtility.CopyComponent(sourceImage);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(targetImage);

        // 复制尺寸
        targetRect.sizeDelta = sourceRect.sizeDelta;

        // 恢复目标的位置信息
        targetRect.anchorMin = originalAnchorMin;
        targetRect.anchorMax = originalAnchorMax;
        targetRect.pivot = originalPivot;
        targetRect.position = originalPosition;
        targetRect.anchoredPosition = originalAnchoredPosition;

        EditorUtility.SetDirty(target);
        Debug.Log($"<color=green>已复制Image和尺寸: {source.name} → {target.name}（保留原位置）</color>");
    }
}

public struct CopyData
{
    [OnValueChanged("Refresh")] public GameObject source;

    [OnValueChanged("Refresh")] public GameObject target;

    [PreviewField] public Sprite SourceIcon;

    public string SourceText;

    [PreviewField] public Sprite TargetIcon;
    public string TargetText;
    private bool isImage;
    private bool isText;

    public void Refresh()
    {
        if (source != null)
        {
            isImage = source.GetComponent<Image>() != null;
            isText = source.GetComponent<Text>() != null;

            if (isImage)
            {
                SourceIcon = source.GetComponent<Image>().sprite;
            }

            if (isText)
            {
                SourceText = source.GetComponent<Text>().text;
            }
        }

        if (target != null)
        {
            isImage = target.GetComponent<Image>() != null;
            isText = target.GetComponent<Text>() != null;


            if (isImage)
            {
                TargetIcon = target.GetComponent<Image>().sprite;
            }

            if (isText)
            {
                TargetText = target.GetComponent<Text>().text;
            }
        }
    }
}