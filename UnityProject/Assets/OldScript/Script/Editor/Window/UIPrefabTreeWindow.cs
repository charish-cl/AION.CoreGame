using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameDevKitEditor
{
    public class UIPrefabTreeWindow: OdinMenuEditorWindow
    {
        private static UIPrefabTreeWindow _treeWindow;
        private OdinMenuTree tree;
        
        /// <summary>
        /// ` ctrl+Space都不行啊，暂时用ctrl+g吧
        /// </summary>
        [MenuItem("GameTools/打开UI预制体 %p")]
        public static void Open()
        {
            _treeWindow = GetWindow<UIPrefabTreeWindow>();
            _treeWindow.Show();
            
        }
        protected override OdinMenuTree BuildMenuTree()
        {
        
            tree = new OdinMenuTree()
            {
                // { "Home", this, EditorIcons.House }, // Draws the this.someData field in this case.
                // { "Player Settings", Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault() }
            };
            tree.Config.DrawSearchToolbar = true;
            
            tree.DrawSearchToolbar();
            
            tree.AddAllAssetsAtPath("UI/", "Assets/Game/EditorData", true).AddThumbnailIcons();
            // tree.AddMenuItemAtPath(new )
             tree.Add("TreeConfig",tree.Config);
             tree.Add("TreeSelection",tree.Selection);
            
            //把继承了OdinWindow的添加到树里
            // var windows = GetType().Assembly.GetTypes()
            //    .Where(e=>e.IsSubclassOf(typeof(OdinEditorWindow)))
            //     .Where(e=>e.GetCustomAttribute<TreeWindowAttribute>()!=null )
            //     .Select(e => new {e.GetCustomAttribute<TreeWindowAttribute>().Path,e})
            //     .ToList();
            //
            // foreach (var window in windows)
            // {
            //     tree.Add(window.Path,CreateInstance(window.e));
            // }
            //
            tree.SortMenuItemsByName();

            return tree;
        }

     
    }
}