
namespace GameDevKit
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ScriptObjectTreeAttribute : System.Attribute
    {
        public string treedirectoryName;
        public string path;

        /// <summary>
        /// 是否把新建方法添加到Odin右侧导航栏上
        /// </summary>
        public bool isAddMenuItem;

        public ScriptObjectTreeAttribute(string treedirectoryName, string path, bool isAddMenuItem = false)
        {
            this.treedirectoryName = treedirectoryName;
            this.path = path;
            //this.getIcon = getIcon;
            this.isAddMenuItem = isAddMenuItem;
        }
    }
}