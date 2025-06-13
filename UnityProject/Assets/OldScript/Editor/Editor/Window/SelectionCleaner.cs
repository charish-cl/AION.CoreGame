using System;
using System.Collections.Generic;
using GameDevKitEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OldScript.Editor.Editor.Window
{
    [TreeWindow("清理")]
    public class SelectionCleaner : OdinEditorWindow
    {
        // 通用组件移除方法（支持条件判断）
        public static void RemoveComponents(GameObject target, Func<Component, bool> shouldRemove)
        {
            // 获取所有组件（包含未激活的）
            Component[] components = target.GetComponents<Component>();
    
            foreach (var component in components)
            {
                // 过滤不可移除的组件[3,5](@ref)
                if (component == null || component is Transform) 
                    continue;

                if (shouldRemove(component))
                {
                    // 编辑器模式安全销毁
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(component);
#else
            Object.Destroy(component);
#endif
                }
            }
        }
// 改进后的丢失脚本移除方法
        [Button("移除选中对象丢失的脚本")]
        public static void RemoveAllMissingScripts()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("未选中任何对象");
                return;
            }

            foreach (GameObject go in Selection.gameObjects)
            {
                var transforms = go.GetComponentsInChildren<Transform>(true);
               
                foreach (var transform in transforms)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                }
            }

            AssetDatabase.Refresh();
        }

// 新增：移除刚体和碰撞器组件
        [Button("移除物理组件")]
        public void RemoveRigidBodyAndCollider()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("未选中任何对象");
                return;
            }

            foreach (GameObject go in Selection.gameObjects)
            {
                var transforms = go.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    RemoveComponents(t.gameObject, c =>
                            c is Rigidbody || // 所有刚体类型
                            c is Rigidbody2D ||
                            c is Collider || // 通用碰撞器
                            c is Collider2D || // 2D碰撞器
                            c.GetType().Name.Contains("TilemapCollider"));
                }
            }

            AssetDatabase.Refresh();
        }
    }
}