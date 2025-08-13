using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;

namespace AION.CoreFramework
{
    /// <summary>
    /// 赋值组件
    /// </summary>
    [System.Serializable]
    [LabelText("赋值组件")]
    public class UIAssignmentCode : BaseUICodeLogic
    {
        public override string FieldName { get; }
        public override string FieldType { get; }


        [HorizontalGroup]
        [LabelText("赋值目标")]
        [ValueDropdown("GetHierarchyPath")]
        public string MemberName;

        [HorizontalGroup]
        [LabelText("变量名")]
        [ValueDropdown("GetMemberNames")]
        public string TargetValue;
        public override string RefreshCode
        {
            get
            {
                if (DataSource == null)
                {
                    return "";
                }

                var type = DataSource.GetMemberType(TargetValue);
                if (type== typeof(string))
                {
                    return $"{MemberName} = {DataSource.FieldName}.{TargetValue};";
                }

                return $"{MemberName} = {DataSource.FieldName}.{TargetValue}.ToString();";
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


        public IEnumerable GetMemberNames()
        {
            if (DataSource == null)
            {
                return null;
            }
            return DataSource.GetUIDataNames();
        }
    }
}