using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;

namespace AION.CoreFramework
{
    public class UIDataSource
    {
        [ValueDropdown("GetUISourceTypeName")]
        [LabelText("数据源类型")]
        public string TypeName;
        
        
        [LabelText("获取数据源代码")]
        public string getDataSourceCode = "";
        
        public string DataSourceName
        {
            get
            {
                return $"{TypeName}Datas";
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

        [Button("创建数据源类")]
        public void CreateUIShowData()
        {
            
        }
    }
}