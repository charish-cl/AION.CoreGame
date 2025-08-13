using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Scriban;
using Scriban.Parsing;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace AION.CoreFramework
{
    [CreateAssetMenu(fileName = "UIDataSourceBindData", menuName = "AION/UIDataSourceBindData", order = 0)]
    public class UIDataSourceBindData:SerializedScriptableObject
    {
        
        [TitleGroup("UI相关")]
        [LabelText("UI预制体")]
        [ValueDropdown("GetUIWindowPrefab")]
        public GameObject prefab;
        List<GameObject> GetUIWindowPrefab()
        {
            var prefabList = AssetDatabase.FindAssets("t:prefab",new []{"Assets/Game/UIForm"}); 
            return prefabList.Select(x => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(x))).ToList();
        }
        
        
        [TitleGroup("UI数据相关")]
        [LabelText("UI数据源")]
        public UIDataSource UIDataSource;
        
        
        
     
        [TitleGroup("UI数据")]
        [LabelText("UI列表数据")]
        [ListDrawerSettings(ElementColor = "#2aaa5f", CustomAddFunction = "AddUIList",ShowFoldout = false)]
        public List<UIDataListCode> DataList;
        public void AddUIList()
        {
            var data = new UIDataListCode();
            data.InitParent(this);
            DataList.Add(data);
        }
        
       
        [TitleGroup("事件相关")]
        [LabelText("监听的事件")]
        [ListDrawerSettings(CustomAddFunction = "AddUIEvent")]
        public List<UIEventData> UIEventList;
        void AddUIEvent()
        {
            UIEventList.Add(new UIEventData());
        }
        
        
        
        [TitleGroup("UI赋值")]
        [LabelText("UI赋值列表")]
        [ListDrawerSettings(ElementColor = "#2aaa5f", CustomAddFunction = "AddUIAssignment")]
        public List<UIAssignmentCode> UIAssignmentList;
        public void AddUIAssignment()
        {
            var data = new UIAssignmentCode();
            data.InitParent(this);
            UIAssignmentList.Add(data);
        }
        
        
        
        
        [TitleGroup("自定义逻辑")]
        [LabelText("组件集合")]
        [ValueDropdown("GetUICodeComponentList")]
        public List<BaseUICodeLogic> UICodeComponentList;
       

        public IEnumerable GetUICodeComponentList()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BaseUICodeLogic>();
            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type) as BaseUICodeLogic;

                if (instance != null)
                {
                    instance.InitParent(this);

                    string name = instance.GetType().Name;
                    
                    LabelTextAttribute labelTextAttribute = type.GetCustomAttribute<LabelTextAttribute>();
                    
                    if (labelTextAttribute!= null)
                    {
                        name = labelTextAttribute.Text;
                    }
                    yield return new ValueDropdownItem<BaseUICodeLogic>(name, instance);
                }
            }
            
        }

        private string prefix = "Assets/OldScript/Module/UIModule/BindTool/Template/";

        
        
        [Button("生成代码", ButtonSizes.Large)]
        public void GenerateCode()
        {
            if (prefab == null)
            {
                Debug.LogError("请选择UI预制体");
                return;
            }
            string templateText = File.ReadAllText(Path.Combine(prefix, "UIWindow.sbn"));
            var context = new TemplateContext {MemberRenamer = member => member.Name};
            var template = Template.Parse(templateText,parserOptions: new ParserOptions {  },lexerOptions: new LexerOptions
            {
               
            });
            
            
            StringBuilder methodBuilder = new StringBuilder();
            StringBuilder refeshUiBuilder = new StringBuilder();
            StringBuilder fieldCodeBuilder = new StringBuilder();
            
            List<BaseUICodeLogic> m_codeList = new List<BaseUICodeLogic>();
            m_codeList.Add(UIDataSource);
            m_codeList.AddRange(DataList);
            m_codeList.AddRange(UIAssignmentList);
            m_codeList.AddRange(UICodeComponentList);
            
            foreach (var logic in m_codeList)
            {
                if (logic == null)
                {
                    continue;
                }
                AddCode(refeshUiBuilder, logic, EnumCodeMemberType.RefreshCode);
                AddCode(methodBuilder, logic, EnumCodeMemberType.MethodCode);
                AddCode(fieldCodeBuilder, logic, EnumCodeMemberType.FieldCode);
            }
            
            var data = new {
                @namespace = "GameLogic",
                class_name = prefab.name,
                event_list = UIEventList,
                refresh_ui_codelist = GetCodeList(refeshUiBuilder),
                method_codelist = GetCodeList(methodBuilder),
                field_codelist = GetCodeList(fieldCodeBuilder),
                init_codelist = 1,
                
                // switch_tab_page_list = SwitchTabPageList,
                // 其他需要传递给模板的数据
            };
            //按照目标名去匹配，而不是默认的无驼峰下划线如event_list这种写法
            string result = template.Render(data,memberRenamer: member => member.Name);
            Debug.Log(result);
        }

        public enum EnumCodeMemberType
        {
            FieldCode,
            MethodCode,
            RefreshCode,
        }
        //StringBuilder转List<string>
        public List<string> GetCodeList(StringBuilder builder)
        {
            var codeList = new List<string>();
            var codeStr = builder.ToString();
            var lines = codeStr.Split('\n');
            foreach (var line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    codeList.Add(line.Trim());
                }
            }
            return codeList;
        }
        public void AddCode(StringBuilder builder, BaseUICodeLogic logic,EnumCodeMemberType memberType)
        {
            switch (memberType)
            {
                case EnumCodeMemberType.FieldCode:
                    if (logic.FieldCode != null)
                    {
                        builder.AppendLine(logic.FieldCode);
                    }
                    break;
                case EnumCodeMemberType.MethodCode:
                    if (logic.MethodCode != null)
                    {
                        builder.AppendLine(logic.MethodCode);
                    }
                    break;
                case EnumCodeMemberType.RefreshCode:
                    if (logic.RefreshCode != null)
                    {
                        builder.AppendLine(logic.RefreshCode);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, null);
            }
        }
        
        
        
        private void OnEnable()
        {
            if (DataList == null)
            {
                DataList = new List<UIDataListCode>();
            }

            if (UICodeComponentList == null)
            {
                UICodeComponentList = new List<BaseUICodeLogic>();
            }
       
            if (UIEventList == null)
            {
                UIEventList = new List<UIEventData>();
            }   
        }
    }


    
    public struct UIEventData
    {
        [LabelText("事件名称")]
        [HorizontalGroup]
        [ValueDropdown("GetUIEventList")]
        public string EventClassName;
        
        [LabelText("事件方法")]
        [HorizontalGroup]
        [ValueDropdown("GetUIEventListMethod")]
        public string EventMethod;

        private string[] GetUIEventListMethod()
        {
            //获取事件方法
            var type =Assembly.Load("GameLogic").GetType($"GameLogic.{EventClassName}");

            return type.GetMethods().Select(x => x.Name).ToArray();
        }
        private string[] GetUIEventList()
        {
            //获取所有事件
            var eventList = TypeCache.GetTypesWithAttribute<EventInterfaceAttribute>();
            
            //获取事件名称
            var eventNames = new List<string>();
            foreach (var type in eventList)
            {
                eventNames.Add(type.Name);
            }

            return eventNames.ToArray();    
        }
    }
}