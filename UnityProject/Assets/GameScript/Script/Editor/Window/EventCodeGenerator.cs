#if UNITY_EDITOR



using System;
using System.Collections.Generic;
using System.Linq;
using GameDevKitEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine.Serialization;
namespace GameScripts.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    [CreateAssetMenu]
    public class EventCodeGeneratorData : SerializedScriptableObject
    {
        // [ListDrawerSettings(NumberOfItemsPerPage = 5, CustomAddFunction = "AddEventProperty")]
        [TableList] public List<EventData> properties = new List<EventData>(); // 属性列表

        [Button("生成所有事件",ButtonHeight = 30)]
        public void GenerateAll()
        {
            properties.ForEach(e => e.GenerateCodeFile());
        }
        
    }

    [Serializable]
    public class EventData
    {
        public string Name = "Event"; // 默认类名

        [LabelText("注释")] public string Comment = "";


        public string OpenParams;


        [Button("创建事件")]
        public void GenerateCodeFile()
        {
            string templateCode = @"
using UnityEngine;
using System;
using GameFramework.Event;
using GameFramework;
using MonoPoly;
public class {0} : GameEventArgs
{{
    public override int Id
    {{
        get {{ return EventId; }}
    }}

    public static readonly int EventId = typeof({0}).GetHashCode();

    {1}

    public static {0} Create({2})
    {{
        {0} {3} = ReferencePool.Acquire<{0}>();
        {4}
        return {3};
    }}

    public override void Clear()
    {{
        {5}
    }}
}}
";

            string propertiesCode = "";
            string createArgs = "";
            string propertyAssignments = "";
            string clearCode = "";

            string classPropName = Char.ToLower(Name[0]) + Name.Substring(1);

            if (!string.IsNullOrEmpty(OpenParams))
            {
                var s = OpenParams.Split(',').Select(e => (e.Split(' ')[0], e.Split(' ')[1]));
                foreach (var (typeName, propName) in s)
                {
                    propertiesCode += $"public {typeName} {propName} {{ get; private set; }}\n";
                    createArgs += $"{typeName} {propName}, ";
                    propertyAssignments += $"{classPropName}.{propName} = {propName};\n";
                    clearCode += $"{propName} = default({typeName});\n";
                }

                //消除掉最后一个逗号
                createArgs = createArgs.TrimEnd(' ', ',');
                propertyAssignments = propertyAssignments.TrimEnd(' ', ',');
                //暂时不需要
                clearCode = "";
               
            }
      
            string code = string.Format(templateCode, Name, propertiesCode, createArgs,
                Char.ToLower(Name[0]) + Name.Substring(1), propertyAssignments, clearCode);

         

           
            string path = Application.dataPath + "/Script/HotFix/Event/" + Name + ".cs";
            // EditorUtility.SaveFilePanel("Save Event Code File", Application.dataPath + "Assets/Scripts/Event/", className + ".cs", "cs");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, code);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif