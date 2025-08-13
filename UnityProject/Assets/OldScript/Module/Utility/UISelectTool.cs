using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework
{
    public class UISelectTool
    {
        public static IEnumerable<ValueDropdownItem<string>> GetChildernsHierarchyPath(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }
            return parent.GetComponentsInChildren<Transform>(true).Select(x =>
                    new ValueDropdownItem<string>(GetTransformPath(parent, x.transform), x.gameObject.name));
        }
    
        public static IEnumerable GetBindGo(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }
            return parent.GetComponentsInChildren<Transform>(true).Select(x => new ValueDropdownItem<BindData>(GetTransformPath(parent, x.transform), new BindData(x.gameObject, "Button",GetTransformPath(parent, x.transform))));
        }
        
        public  static string GetTransformPath(Transform parent, Transform child)
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

        public static Transform GetRoot(Transform transform)
        {
            while (transform.parent != null)
            {
                transform = transform.parent;
            }
            return transform;
        }


        public static (string comment, string title) GenerateConstantName(string goName)
        {
            return (goName.Replace(" ", "").Replace("-", "").Replace("_", "").ToUpper(), goName);
        }
    }
}