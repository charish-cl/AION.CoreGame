using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AION.CoreFramework
{
    public class SelectionHelper
    {
        public static string GetTransformPath(Transform parent, Transform child)
        {
            if (parent == null || child == null)
            {
                return string.Empty;
            }

            string path = child.name;
            Transform current = child.parent;
            while (current != null && current != parent)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return current == parent ? path : string.Empty;
        }
        public static (string comment,string name) GenerateConstantName(string tabName)
        {
            // Remove special characters and spaces from tab name and convert to PascalCase
            var a = tabName.Split('_');
        
            return (a.First(),a.Last());
        }
        // 获取最上层父物体的方法
        public static Transform GetRoot(Transform child)
        {
            // 如果对象没有父物体，那么它就是最上层父物体
            if (child.parent == null)
            {
                return child;
            }

            // 递归查找父物体
            return GetRoot(child.parent);
        }
    }
}