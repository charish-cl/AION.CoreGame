using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        
        [BoxGroup("UI相关")]
        [LabelText("UI预制体")]
        [ValueDropdown("GetUIWindowPrefab")]
        public GameObject prefab;
        
        
        List<GameObject> GetUIWindowPrefab()
        {
            var prefabList = AssetDatabase.FindAssets("t:prefab",new []{"Assets/Game/UIForm"}); 
            return prefabList.Select(x => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(x))).ToList();
        }
        [BoxGroup("UI数据相关")]
        [LabelText("UI数据源")]
        public UIDataSource UIDataSource;
        
        
        [BoxGroup("UI数据")]
        [LabelText("UI列表数据")]
        [ListDrawerSettings(ElementColor = "#2aaa5f", CustomAddFunction = "AddUIList",ShowFoldout = false)]
        public List<UIDataListCode> DataList;
        public void AddUIList()
        {
            var data = new UIDataListCode();
            data.InitParent(prefab);
            DataList.Add(data);
        }

        // [BoxGroup("UI数据")]
        // [LabelText("标签页管理")]
        // public List<UISwitchTabPage> SwitchTabPageList;
        
        [Space]
        [BoxGroup("事件相关")]
        [LabelText("监听的事件")]
        [ListDrawerSettings(CustomAddFunction = "AddUIEvent")]
        public List<UIEventData> UIEventList;
        void AddUIEvent()
        {
            UIEventList.Add(new UIEventData());
        }
        
 
        

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
                    instance.InitParent(prefab);
                    yield return instance;
                }
            }
            
        }

        private string prefix = "Assets/OldScript/Module/UIModule/BindTool/Template/";

        [Button("生成代码", ButtonSizes.Large)]
        public void GenerateCode()
        {
            string templateText = File.ReadAllText(Path.Combine(prefix, "UIWindow.sbn"));
            var context = new TemplateContext {MemberRenamer = member => member.Name};
            var template = Template.Parse(templateText,parserOptions: new ParserOptions {  },lexerOptions: new LexerOptions
            {
               
            });
      
            var go = Selection.activeGameObject;
            if (go == null)
            {
                return;
            }
            var data = new {
                @namespace = "GameLogic",
                class_name = go.name,
                event_list = UIEventList,
                data_list = DataList,
                // switch_tab_page_list = SwitchTabPageList,
                // 其他需要传递给模板的数据
            };
            //按照目标名去匹配，而不是默认的无驼峰下划线如event_list这种写法
            string result = template.Render(data,memberRenamer: member => member.Name);
            Debug.Log(result);
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
            // if (SwitchTabPageList == null)
            // {
            //     SwitchTabPageList = new List<UISwitchTabPage>();
            // }
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
  
    

    public struct UITimerData
    {
        public string Name;
    }
    
}