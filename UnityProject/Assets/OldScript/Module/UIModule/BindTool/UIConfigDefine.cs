using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AION.CoreFramework
{
    public class UIConfigDefine
    {
        public const string GeneratePartialCodePath = "Assets/GameLogic/UI/Generate/";
        public const string GenerateFormCodePath = "Assets/GameLogic/UI/";

        public const string Namespace = "GameLogic";
    
        // UI元素名与类型的映射字典
        public static List<string> dicWidget = new List<string>()
        {
            "None",
            "TextMeshProUGUI", "GameObject", "Image", "Transform", "RectTransform", "Text", "Button",
            "RawImage", "ScrollRect", "InputField", "TMP_InputField", "Slider", "ToggleGroup", "Toggle", "TabModule"
        };
        
        public static Dictionary<string,int> dicWidgetIndex = new Dictionary<string, int>()
        {
            {"None",0},
            {"TextMeshProUGUI",1}, {"GameObject",2}, {"Image",3}, {"Transform",4}, {"RectTransform",5}, {"Text",6}, {"Button",7},            
            {"RawImage",8}, {"ScrollRect",9}, {"InputField",10}, {"TMP_InputField",11}, {"Slider",12}, {"ToggleGroup",13}, {"Toggle",14}, {"TabModule",15}
        };
        
        public const string prefix = "Assets/OldScript/Module/UIModule/BindTool/Template/";
        
        
        
        public static void BuildBindCode(List<BindData> BindDatas,GameObject gameObject)
        {
            string templateText = File.ReadAllText(Path.Combine(prefix, "UIBindTemplate.sbn"));
            var template = Template.Parse(templateText);
            
            
            bool IsItem = gameObject.name.Contains("Item");
            
            
            var data = new {
                @namespace = Namespace,
                class_name = gameObject.name,
                bind_datas = BindDatas,
                derived_class_name = IsItem? "UIWidget" : "UIWindow",
                // 其他需要传递给模板的数据
            };
            string result = template.Render(data,memberRenamer: member => member.Name);
            
            Debug.Log(result);
            string className = gameObject.name;
            string folderPath = IsItem?  GenerateFormCodePath+"/UIWidget":GeneratePartialCodePath;
            string BindCodeFilePath = $"{folderPath}/{className}.Bind.cs";
            
            //UIFormCodeFilePath只生成一次,搜索整个脚本目录
            File.WriteAllText(BindCodeFilePath, result);
            AssetDatabase.Refresh();
            
        }

    }
    [Serializable]
    public class BindData
    {
        public BindData()
        {
        }

        [HideInInspector]
        public string path;
        
        public BindData(Object bindCom, string TypeName,string path = "")
        {
            BindCom = bindCom;
            this.TypeName = TypeName;
            this.path = path;
        }

        public Object BindCom;

        public string propName
        {
            get
            {
                //小写第一个字母
                return BindCom.name.Substring(0, 1).ToLower() + BindCom.name.Substring(1);
            }
        }
               

        [ValueDropdown("GetTypeName")] 
        public string TypeName;
        
        public List<string> GetTypeName()
        {
            return UIConfigDefine.dicWidget;
        }

    }

}