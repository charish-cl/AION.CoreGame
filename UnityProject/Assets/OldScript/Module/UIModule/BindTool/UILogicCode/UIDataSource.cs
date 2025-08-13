using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;

namespace AION.CoreFramework
{
    public class UIDataSource:BaseUICodeLogic
    {
        [ValueDropdown("GetUISourceTypeName")]
        [LabelText("数据源类型")]
        public string TypeName;

        public override string FieldName
        {
            get
            {
                return $"m_data{TypeName}";
            }
        }

        public override string FieldType
        {
            get
            {
             return $"{TypeName}";   
            }
        }

        public override string MethodCode
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"public void Init({TypeName} {GetLowerFirstLetterName(TypeName)})");
                sb.AppendLine("{");
                sb.AppendLine($"    {FieldName} = {GetLowerFirstLetterName(TypeName)};");
                sb.AppendLine("     RefreshUI();");
                sb.AppendLine("}");
                return sb.ToString();
            }
        }
        
        private string[] GetUISourceTypeName()
        {
            Assembly assembly = Assembly.Load("GameLogic");
            var types = assembly.GetTypes();
            var uiSourceTypes = types.Where(x => x.Name.EndsWith("ShowData")).ToArray();
            var uiSourceNames = uiSourceTypes.Select(x => x.Name).ToArray();
            return uiSourceNames;
        }

        /// <summary>
        /// 获取数据源的成员变量
        /// </summary>
        /// <returns></returns>
        public IEnumerable GetUIDataNames()
        {
            Assembly assembly = Assembly.Load("GameLogic");
            var type = assembly.GetType("GameLogic." + TypeName);
            var fields = type.GetFields();
            var dataNames = fields.Select(x => x.Name).ToArray();
            return dataNames;
        }
        
        public Type GetMemberType(string memberName)
        {
            Assembly assembly = Assembly.Load("GameLogic");
            var type = assembly.GetType("GameLogic." + TypeName);
            var field = type.GetField(memberName);
            return field.FieldType;
        }
        
    }
}