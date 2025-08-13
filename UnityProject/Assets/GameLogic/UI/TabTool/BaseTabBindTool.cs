using System;
using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic
{
    public class BaseTabBindTool:SerializedMonoBehaviour
    {
        public Transform Root => UISelectTool.GetRoot(transform);

        [LabelText("父物体")] 
        public Transform TabParent;

        [TableList] 
        public List<Group> BindGo = new List<Group>();

 
        [OnValueChanged("OpenTab")]
        [ValueDropdown("GetAllTabNames")]
        public string SelectTabName;
        public List<string> GetAllTabNames()
        {
            return BindGo.Select(e => e.Comment).ToList();
        }
        
        [HideInInspector]
        public string LastSelectTabName;


        [ReadOnly] 
        public string TabPath;
        
          
        [Button("创建", ButtonHeight = 50)]
        public void Create()
        {
            if (TabParent == null)
            {
                throw new Exception("请先选择父物体");
            }

            for (int i = 0; i < TabParent.childCount; i++)
            {
                var go = TabParent.GetChild(i).gameObject;

                var (comment, s) = UISelectTool.GenerateConstantName(go.name);
                BindGo.Add(new Group(s, comment, new List<GameObject>() { go }));
            }
        }
        
        [OnInspectorInit]
        public virtual void OnInit()
        {
            BindGo ??= new List<Group>();
            if (TabParent == null)
            {
                TabParent = transform;
            }

            if (string.IsNullOrEmpty(TabPath))
            {
                var relativePath = UISelectTool.GetTransformPath(TabParent.transform.root, TabParent);

                relativePath = relativePath.Substring(relativePath.IndexOf("/", StringComparison.Ordinal) + 1);
                TabPath = relativePath;
            }
        }
        
        //OpenTab
        public virtual void OpenTab()
        {
            //一致则不处理
            if (LastSelectTabName == SelectTabName)
            {
                return;
            }

            //触发上一个Tab的退出动作
            if (!string.IsNullOrEmpty(LastSelectTabName))
            {
                var lastTab = BindGo.Find(e => e.Comment == LastSelectTabName);
                if (lastTab != null)
                {
                    lastTab.ExitActions.ForEach(e => e.Execute());
                }
            }

            //触发当前Tab的进入动作
            var currentTab = BindGo.Find(e => e.Comment == SelectTabName);
            if (currentTab != null)
            {
                currentTab.EnterActions.ForEach(e => e.Execute());
            }

            LastSelectTabName = SelectTabName;

            if (BindGo == null)
            {
                Debug.LogError("BindGo is null.");
                return;
            }

            foreach (var tab in BindGo)
            {
                foreach (var go in tab._gameObjects)
                {
                    go.SetActive(tab.Comment == SelectTabName);
                }
            }

            //确保当前选择的tab是可见的
            BindGo.Find(e => e.Comment == SelectTabName)._gameObjects.ForEach(e => e.SetActive(true));
        }
        
        
        [Button("生成Tab枚举", ButtonHeight = 50)]
        public void GenerateTabClass()
        {
            string className = gameObject.name + "_Tab";
            List<string> tabNames = BindGo.Select(e => e.TitleName).ToList();
            List<string> commentNames = BindGo.Select(e => e.Comment).ToList();
            if (string.IsNullOrEmpty(className) || tabNames.Count == 0)
            {
                Debug.LogError("Invalid input parameters.");
                return;
            }

            // Generate the class header
            string classCode = $"public enum Enum{className}\n{{\n";

            // Generate constants for each tab
            for (int i = 0; i < tabNames.Count; i++)
            {
                var comment = commentNames[i];
                var tabName = tabNames[i];
                //生成注释 ///
                classCode += $"\t/// <summary>\n\t/// {comment}\n\t/// </summary>\n";
                classCode += $"{tabName} = {i},\n";
            }

            classCode.TrimEnd(',');
            // Close the class
            classCode += $"}}";
            //
            // StringBuilder builder = new StringBuilder();
            // Print the generated code
            GUIUtility.systemCopyBuffer = classCode;
        }
    }
}