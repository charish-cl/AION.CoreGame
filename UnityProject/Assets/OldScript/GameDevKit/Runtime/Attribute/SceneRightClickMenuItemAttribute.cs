using System;

namespace GameDevKit
{
    /// <summary>
    /// 场景右键点击弹出的菜单栏
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SceneRightClickMenuItemAttribute : Attribute
    {
        public string MenuItemName { get; }

        public SceneRightClickMenuItemAttribute(string menuItemName)
        {
            MenuItemName = menuItemName;
        }
    }
}