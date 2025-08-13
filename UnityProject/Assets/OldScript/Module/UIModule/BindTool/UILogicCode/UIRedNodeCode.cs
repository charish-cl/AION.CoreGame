using System.Collections;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace AION.CoreFramework
{
    [LabelText("UI红点节点逻辑代码")]
    public class UIRedNodeCode:BaseUICodeLogic
    {

        [ValueDropdown("GetRedNodeTypes",NumberOfItemsBeforeEnablingSearch=1)]
        [LabelText("绑定红点点类型")]
        public string BindRedNodeType;

        [ValueDropdown("GetHierarchyPath")]
        [LabelText("绑定目标")]
        public string BindTarget;
        
        public override string FieldName
        {
            get
            {
                return "m_redNode" + GetLowerFirstLetterName(BindRedNodeType);
            }
        }

        public override string FieldType
        {
            get
            {
              return "RedNodeType";
            }
        }

        public override string RefreshCode
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(BindTarget))
                {
                    throw new System.Exception("红点组件绑定目标不能为空，请选择绑定目标");
                }
                return $"{FieldName} = CreateWidget<RedNodeIcon>({BindTarget}.gameObject);\n";
                sb.AppendLine($"{FieldName} = ");
                sb.AppendLine("m_redNode" + GetLowerFirstLetterName(BindRedNodeType) + ".Refresh();");
                sb.AppendLine("}");
                return sb.ToString();   
            }
        }

        public IEnumerable GetHierarchyPath()
        {
            var gameObject = Parent.prefab;
            if (gameObject == null)
            {
                return null;
            }
            var transform = gameObject.transform;
            return UISelectTool.GetChildernsHierarchyPath(transform).Select(x=>x.Value);
        }

        public IEnumerable GetRedNodeTypes()
        {
            var type =  GetTypeByAssemblies("RedNodeType");
            var fields = type.GetFields();
            var dataNames = fields.Select(x => x.Name).ToArray();
            return dataNames;
        }
    }
}