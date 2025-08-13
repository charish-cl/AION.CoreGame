using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AION.CoreFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    [Serializable]
    public class Group
    {
        public string TitleName;

        public string Comment;

#if UNITY_EDITOR
        [ValueDropdown("TreeViewAdd", ExpandAllMenuItems = true, IsUniqueList = false,
            DrawDropdownForListElements = true)]
#endif
        [LabelText("绑定对象")]
        public List<GameObject> _gameObjects;
#if UNITY_EDITOR
        public IEnumerable TreeViewAdd()
        {
            if (transform == null)
            {
                transform = UISelectTool.GetRoot(Selection.activeTransform);
            }

            return transform.GetComponentsInChildren<Transform>(true)
                .Select(x =>
                    new ValueDropdownItem(UISelectTool.GetTransformPath(transform, x.transform), x.gameObject));
        }
#endif
        

        [HideInInspector] public Transform transform;

#if UNITY_EDITOR
        [ValueDropdown("SwitchActionsDropdown")]
#endif
        [LabelText("进入行为")]
        [OdinSerialize, SerializeReference]
        public List<BaseAction> EnterActions = new List<BaseAction>();
#if UNITY_EDITOR
        IEnumerable SwitchActionsDropdown()
        {
            return UnityEditor.TypeCache.GetTypesDerivedFrom<BaseAction>().Where(t => t.IsAbstract == false).Select(t =>
                new ValueDropdownItem(t.Name, Activator.CreateInstance(t) as BaseAction));
        }

#endif
        
        
        [LabelText("离开行为")] [OdinSerialize, SerializeReference]
        public List<BaseAction> ExitActions = new List<BaseAction>();




        public Group()
        {
            _gameObjects = new List<GameObject>();
        }

        public Group(string titleName, string comment, List<GameObject> gameObjects)
        {
            TitleName = titleName;
            Comment = comment;
            _gameObjects = gameObjects;
        }


    }

    public class UITabBindTool : BaseTabBindTool
    {
        

        [Button("清空所有绑定对象", ButtonHeight = 50)]
        public void ClearAllBindGo()
        {
            foreach (var group in BindGo)
            {
                group._gameObjects.Clear();
            }
            
            #if UNITY_EDITOR
                   EditorUtility.SetDirty(this);
            #endif
        
        }
        
        public TabModule ConvertToTab()
        {
            var tab = TabModule.Create();
            tab.StateParent = GetComponent<RectTransform>();
            // tab.DynammicTabPathDic = DynammicTabPathDic;

            for (int i = 0; i < BindGo.Count; i++)
            {
                var group = BindGo[i];
                tab.AddTab(i, BindGo[i]._gameObjects);
                if (group.EnterActions.Count > 0)
                {
                    tab.AddSwitchAction(i, () =>
                    {
                        for (int j = 0; j < group.EnterActions.Count; j++)
                        {
                            group.EnterActions[j].Execute();
                        }
                    });
                }

                if (group.ExitActions.Count > 0)
                {
                    tab.AddSwitchAction(i, () =>
                    {
                        for (int j = 0; j < group.ExitActions.Count; j++)
                        {
                            group.ExitActions[j].Execute();
                        }
                    });
                }
            }

            return tab;
        }

        [Button("反向添加", ButtonHeight = 50)]
        public void InvertAdd([ValueDropdown("TreeViewAdd")] GameObject go)
        {
            if (go == null)
            {
                Debug.LogError("go is null.");
                return;
            }

            foreach (var group in BindGo)
            {
                if (group.Comment != SelectTabName)
                {
                    group._gameObjects.Add(go);
                }
            }
        }
#if UNITY_EDITOR
        public IEnumerable TreeViewAdd()
        {
            var root = UISelectTool.GetRoot(Selection.activeTransform);
            return root.GetComponentsInChildren<Transform>(true)
                .Select(x =>
                    new ValueDropdownItem(UISelectTool.GetTransformPath(root, x.transform), x.gameObject));
        }
#endif

      
   
        
    }
}