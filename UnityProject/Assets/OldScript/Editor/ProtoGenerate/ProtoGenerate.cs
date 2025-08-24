

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AION.CoreFramework;
using Scriban;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ShellHelper = GameDevKitEditor.ShellHelper;

public class ProtoGenerate : OdinEditorWindow
{
    
    public string prefix = "Assets/OldScript/Editor/ProtoGenerate";
    
    public string mainProtoPath = "C:\\Users\\47643\\WebstormProjects\\gameserver\\protos\\main.proto";

    public const string GenerateProtoFolder = "C:\\Users\\47643\\WebstormProjects\\gameserver\\protos\\";

    
    [MenuItem("Tools/协议生成工具")]
    public static void OpenWindow()
    {
        var window = GetWindow<ProtoGenerate>();
        window.position = new Rect(100, 100, 500, 500);
        window.titleContent = new GUIContent("协议生成工具");
    }

    List<string> messageNames = new List<string>();
    List<string> enumNames = new List<string>();
    Dictionary<string,int> messageNameToId = new Dictionary<string, int>();
    List<(string,string)> messageNameToEnumName = new List<(string,string)>();
    public void Initialize()
    {
        messageNames.Clear();
        messageNameToEnumName.Clear();
        enumNames.Clear();
        messageNameToId.Clear();
        
        string protoContent = File.ReadAllText(mainProtoPath);
        messageNames = ExtractMessageNames(protoContent);
        
 
        messageNameToId = new Dictionary<string, int>();
        for (var i = 0; i < messageNames.Count; i++)
        {
            string messageName = messageNames[i];
            messageNameToId[messageName] = i + startingEnumValue;
        }

    
        for (var i = 0; i < messageNames.Count; i++)
        {
            string enumName = ToEnumFieldName(messageNames[i]);
            enumNames.Add(enumName);
            messageNameToEnumName.Add((messageNames[i],enumName));
        }
       
      
    }

    [Button("生成所有")]
    public void GenerateAll()
    {
        Initialize();
        RunProtoCmd();
        GenerateProtoEnum();
        GenetateClientProto();
        GenetateServerProto();
    }

    [Button("运行proto命令")]
    public void RunProtoCmd()
    {
        ShellHelper.Run("npm run generatecs","C:\\Users\\47643\\WebstormProjects\\gameserver");
    }
    public void GenerateProtoEnum()
    {
        string enumContent = GenerateEnumContent(messageNames);
        Debug.Log(enumContent);
        File.WriteAllText(Path.Combine(GenerateProtoFolder, "prototype.proto"), enumContent);
    }
    [Button("生成客户端协议绑定模板" )]
    public void GenetateClientProto()
    {
        Initialize();
        
        string templateText = File.ReadAllText(Path.Combine(prefix, "ClientProtoTemplate.sbn"));
        var template = Template.Parse(templateText);
        
        
        var data = new {
            messageNameToId = messageNameToId,
            // 其他需要传递给模板的数据
        };
        string result = template.Render(data,memberRenamer: member => member.Name);
     
      
        File.WriteAllText("Assets/GameProto/GameProtocol/MessageDispatcher.Register.cs", result);
    }
    
    [Button("生成服务器端协议绑定模板" )]
    public void GenetateServerProto()
    {
        Initialize();

        string templateText = File.ReadAllText(Path.Combine(prefix, "ServerProtoTemplate.sbn"));
        var template = Template.Parse(templateText);
        
        
        var data = new {
            messageNameToEnumName = messageNameToEnumName,
            // 其他需要传递给模板的数据
        };
        string result = template.Render(data,memberRenamer: member => member.Name);
     
        Debug.Log(result);
        File.WriteAllText("C:\\Users\\47643\\WebstormProjects\\gameserver\\protos\\ProtoIndex.ts", result);
    }
    
    //提取所有的message名称
    private List<string> ExtractMessageNames(string protoContent)
    {
        List<string> messageNames = new List<string>();
        Regex messageRegex = new Regex(@"message\s+(\w+)\s*\{", RegexOptions.Multiline);
        
        MatchCollection matches = messageRegex.Matches(protoContent);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                string value = match.Groups[1].Value;
                if (value.Contains("Request")||value.Contains("Response")||value.Contains("Notify"))
                {
                    messageNames.Add(value);
                }
            }
        }

        return messageNames;
    }
    
    
    private string enumName = "ProtocolType";
    private int startingEnumValue = 10001;
    private string GenerateEnumContent(List<string> messageNames)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("syntax = \"proto3\";");
        sb.AppendLine();
        sb.AppendLine($"enum {enumName} {{");
        sb.AppendLine("  None = 0;");
        sb.AppendLine();

        int enumValue = startingEnumValue;
        for (var i = 0; i < enumNames.Count; i++)
        {
            sb.AppendLine($"  {enumNames[i]} = {enumValue};");
            enumValue++;
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
    private string ToEnumFieldName(string messageName)
    {
        // 将消息名称转换为大写下划线形式 (如 LoginRequest -> LOGIN_REQUEST)
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < messageName.Length; i++)
        {
            char c = messageName[i];
            if (i > 0 && char.IsUpper(c))
            {
                sb.Append('_');
            }
            sb.Append(char.ToUpper(c));
        }
        return sb.ToString();
    }
}
