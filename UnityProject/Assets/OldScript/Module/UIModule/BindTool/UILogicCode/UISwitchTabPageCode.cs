using Sirenix.OdinInspector;

namespace AION.CoreFramework
{
    [LabelText("页签")]
    public class UISwitchTabPageCode
    {
        public string Name;
        
        [ValueDropdown("GetTypes")]
        public string DataType;
        
        private string[] GetTypes()
        {
            return new string[] {"int", "float", "string"};
        }
    }
}