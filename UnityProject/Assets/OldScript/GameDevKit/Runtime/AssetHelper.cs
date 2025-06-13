using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GameDevKit
{
    public static class AssetHelper
    {
        public static T CreateScriptableObject<T>( string dataPath, string fileInfoName) where T : ScriptableObject
        {
            string save_path = $"{dataPath}/{fileInfoName}.asset";
            var itemData = ScriptableObject.CreateInstance<T>();
            itemData.name = fileInfoName;
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(itemData, save_path);
            AssetDatabase.Refresh();
#endif
            return itemData;
        }
        
        
        public static void Create(this ScriptableObject itemData,
            string dataPath, string fileInfoName) 
        {
            string save_path = $"{dataPath}/{fileInfoName}.asset";
            itemData.name = fileInfoName;
#if UNITY_EDITOR
            
            AssetDatabase.CreateAsset(itemData, save_path);
            itemData.SetAssetDirty();
            AssetDatabase.Refresh();
#endif
           
        }
        
        public static string GetAssetPath(this Object obj)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(obj);
            Debug.Log(path);
            return path;
#endif
            return null;
        }
        public static Guid GetAssetSystemGuid(this Object obj)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(obj);
            return  new Guid(AssetDatabase.AssetPathToGUID(path));
#endif
            return Guid.Empty;
        }  
        
        /// <summary>
        /// 无后缀，无Assets/Resources的Resources路径
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetResourcesPath(this  Object obj)
        {
            string path = GetAssetPath(obj);
            path= path.Replace("Assets/Resources/", "").Split('.')[0];
            return path;
        }
        /// <summary>
        /// 添加子对象,这个一些行为树节点工具会用到
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="other"></param>
        public static void AddObjectToObject(this  Object obj,Object other)
        {
#if UNITY_EDITOR

            AssetDatabase.AddObjectToAsset(other,obj);
          
          //  AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(other));
            AssetDatabase.SaveAssets();
#endif
        }

        public static void RemoveObjectFromAsset(Object other)
        {
#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(other);
            
            AssetDatabase.SaveAssets();
#endif
        }
        public static void SetAssetDirty(this Object obj)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif  
        }

        /// <summary>
        /// 销毁Asset
        /// </summary>
        /// <param name="o"></param>
        public static void DestroyAsset(this  Object o)
        {
            if (Application.isEditor)
            {
                Object.DestroyImmediate(o,true);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            else
            {
                Object.Destroy(o);
            }
        }
        public static void SaveAssets()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
     
        //系统的Guid转换为Unity的Guid,通过把字符串中的'-'去掉
        public static string GuidToUnityGuid(this  Guid guid)
        {
            return guid.ToString().Replace("-","");
        }
      
        
    }
}