namespace GameDevKit
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 调整精灵数量（编辑器与运行时均可用）。
    /// </summary>
    /// <remarks>可在编辑器模式下直接运行，prefab 必须有效。</remarks>
    /// <param name="listSprites">存放 Sprite 物体的列表。</param>
    /// <param name="number">目标数量。</param>
    /// <param name="parentTrans">父节点。</param>
    /// <param name="prefab">精灵预制体（不能为空）。</param>
    public static class SpriteHelper
    {
        /// <summary>
        /// 调整精灵数量。
        /// </summary>
        /// <remarks>常用于Sprite对象的批量创建。</remarks>
        /// <param name="listSprites">存放Sprite物体的列表。</param>
        /// <param name="number">目标数量。</param>
        /// <param name="parentTrans">父节点。</param>
        /// <param name="prefab">精灵预制体（不能为空）。</param>
        public static void AdjustSpriteNum(List<GameObject> listSprites, int number, Transform parentTrans, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("AdjustSpriteNum 需要一个有效的 prefab！");
                return;
            }

            if (listSprites == null)
            {
                listSprites = new List<GameObject>();
            }

            int currentCount = listSprites.Count;

            if (currentCount < number)
            {
                // 创建缺少的部分
                int needNum = number - currentCount;
                for (int i = 0; i < needNum; i++)
                {
                    GameObject go = Object.Instantiate(prefab, parentTrans);
                    go.name = prefab.name + "_" + (currentCount + i);
                    listSprites.Add(go);
                }
            }
            else if (currentCount > number)
            {
                // 移除多余的部分
                for (int i = listSprites.Count - 1; i >= number; i--)
                {
                    GameObject toRemove = listSprites[i];
                    listSprites.RemoveAt(i);

                   
                    if (toRemove != null)
                        if (!Application.isPlaying)
                        {
                            Object.DestroyImmediate(toRemove);
                        }
                        else
                        {
                            Object.Destroy(toRemove);
                        }
                }
            }
        }
    }
}

