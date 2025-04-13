using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    public static class ResourceDependenceHelper
    {
        /// <summary>
        /// 查找场景中依赖的对象
        /// </summary>
        /// <param name="resource">要查找依赖项的资源</param>
        public static List<Object> FindSceneDependentObjects(Object resource,Action<GameObject> action = null)
        {
            if (resource == null)
            {
                Debug.LogError("Please select a resource to find dependent objects.");
                return null;
            }
        
            // 获取场景中所有游戏对象
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();


            return GetDependceFromObjectsImp(resource, sceneObjects, action);
        }
        public static List<Object> FindPrefabDependentObjects(Object resource,Action<GameObject> action = null)
        {
            if (resource == null)
            {
                Debug.LogError("Please select a resource to find dependent objects.");
                return null;
            }
        
            // 获取场景中所有游戏对象
            var gameObjects = SelectionExtend.GetCurrentOpenPrefabRoot().GetComponentsInChildren<Transform>(true).Select(e=>e.gameObject);
            
            return GetDependceFromObjectsImp(resource, gameObjects.ToArray(), action);
        }
        public static List<Object> FindDependentObjectsFromSelectObj(Object resource,Action<GameObject> action = null)
        {
            if (resource == null)
            {
                Debug.LogError("Please select a resource to find dependent objects.");
                return null;
            }

            var gameObjects = Selection.activeGameObject.GetComponentsInChildren<Transform>(true).Select(e=>e.gameObject);
            return GetDependceFromObjectsImp(resource, gameObjects.ToArray(), action);
        }
        public static List<Object> GetDependceFromObjectsImp( Object resource,GameObject[] sceneObjects,Action<GameObject> action = null)
        {
            var dependentObjects = new List<Object>();
            // 遍历场景中所有游戏对象
            foreach (GameObject obj in sceneObjects)
            {
                // 检查游戏对象是否依赖于指定资源
                Component[] components = obj.GetComponents<Component>();
                foreach (Component component in components)
                {
                   
                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty prop = serializedObject.GetIterator();
                    while (prop.NextVisible(true))
                    {
                       
                        //这里可以读取加载一张图片的所有图片
                        if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                            prop.objectReferenceValue == resource)
                        {
                            action?.Invoke(obj);
                             EditorGUIUtility.PingObject(obj);
                            // 添加依赖资源的物体到列表中
                            dependentObjects.Add(obj);
                        }
                        //TODO：这里Texture2D与Sprite会被识别为类型不一致
                        else if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                                 IsTexture2DOrSprite(resource,prop.objectReferenceValue))
                        {
                            var referenceValue = prop.objectReferenceValue;
                            //加载所有子图片
                            var sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(resource)).Where(e=>e is Sprite);

                            if (!sprites.Any())
                            {
                                continue;
                            }
                            var sprite =sprites.FirstOrDefault(e => e == referenceValue);
                    
                            if (sprite!=null)
                            {
                                action?.Invoke(obj);
                                EditorGUIUtility.PingObject(obj);
                                // 添加依赖资源的物体到列表中
                                dependentObjects.Add(obj);
                            }
                        }
                    }
                }
            }
        
            Debug.Log("Found " + dependentObjects.Count + " objects dependent on resource.");
            
            return dependentObjects; 
        }

        public static bool IsTexture2DOrSprite(Object obj,Object refrence)
        {
            return obj is Texture2D && refrence is Sprite;
        }
        
    }
}