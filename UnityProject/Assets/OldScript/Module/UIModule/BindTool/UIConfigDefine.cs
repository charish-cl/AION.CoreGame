using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Sirenix.OdinInspector;
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