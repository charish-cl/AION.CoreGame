using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace AION.CoreFramework
{
    //不用管这个列表叫什么名字，只管它对应的Item类型数据类型 这里面可以涵盖标签页/无限列表/选择列表 反正就是item的集合
    [LabelText("数据列表")]
    public class UIDataListCode :BaseUICodeLogic
    {

        public override string FieldName
        {
            get
            {
            
                return $"m_data{ItemTypeName}List";
            }
            
        }
        
        public override string FieldType
        {
            get
            {
              return $"List<{ItemTypeName}>";   
            }
        }

        public override string RefreshCode
        {
            get
            {
               
                StringBuilder sd = new StringBuilder();
                if (childItem!=null)
                {
                    sd.AppendLine($"AdjustIconNum({FieldName}, {DataSource?.FieldName}.Count, {listParent}, {childItem});");
                }
                else
                {
                    sd.AppendLine($"AdjustIconNum({FieldName}, {DataSource?.FieldName}.Count, {listParent});");
                }
                
                
                
                return sd.ToString();
            }
        }


        // public void AdjustIconNum<T>(List<T> listIcon, int number, Transform parentTrans, GameObject prefab = null, string assetPath = "")
        [HorizontalGroup("1")]
        [LabelText("数据Item名称")]
        [ValueDropdown("GetUIItems")]
        public string ItemTypeName;
        
     
        
        [HorizontalGroup("2")]
        [ValueDropdown("GetHierarchyPath")]
        [LabelText("父物体")]
        public string listParent;
        
        [HorizontalGroup("2")]
        [ValueDropdown("GetHierarchyPath")]
        [LabelText("子物体(可空)")]
        public string childItem;
        
        public IEnumerable GetUIItems()
        {
            //获取事件方法
            var type =Assembly.Load("GameLogic").GetTypes().Where(x => x.Name.EndsWith("Item"));

            return type.Select(x => x.Name).ToArray();
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
        

    }
}