using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework
{
    public class TabBindTool:SerializedMonoBehaviour
    {
        public List<TabDicClass> TabDics = new List<TabDicClass>();

        public string TabInitCode =
            @"  public TabModule {0};\n              tabModule = TabModule.Create({1});";
        public class TabDicClass
        {
            [OnValueChanged("OpenTab")]
            [ValueDropdown("GetAllTabNames")]
            public string SelectTabName;
            
            public string TabName;
            [TableList]
            public List<(string, List<GameObject>)> TabLis;
            
            public List<string> GetAllTabNames()
            {
                return TabLis.Select(e => e.Item1).ToList();
            }
            //OpenTab
            public void OpenTab()
            {
               
                if (TabLis == null)
                {
                    Debug.LogError("TabDic is null.");
                    return;
                }
                foreach (var tab in TabLis)
                {
                    foreach (var go in tab.Item2)
                    {
                        go.SetActive(tab.Item1 == SelectTabName);
                    }
                }
            }
            public Transform TabParent;
            [Button]
            public void Create()
            {
                TabLis ??= new List<(string, List<GameObject>)>();

                if (TabParent == null)
                {
                    throw new Exception("请先选择父物体");
                }
                for (int i = 0; i < TabParent.childCount; i++)
                {
                    var go = TabParent.GetChild(i).gameObject;
                    TabLis.Add((go.name, new List<GameObject>(){go}));
                }

            }

            [Button]
            public  void GenerateTabClass()
            {
                string className = TabName;
                List<string> tabNames = TabLis.Select(e=> e.Item1).ToList();
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
                    var (comment, s) = GenerateConstantName(tabNames[i]);
                    //生成注释 ///
                    classCode += $"\t/// <summary>\n\t/// {comment}\n\t/// </summary>\n";
                    
                    classCode += $"{s} = {i},\n";
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
     

        private static (string comment,string name) GenerateConstantName(string tabName)
        {
            // Remove special characters and spaces from tab name and convert to PascalCase
            var a = tabName.Split('_');
        
            return (a.First(),a.Last());
        }

        
    }
}