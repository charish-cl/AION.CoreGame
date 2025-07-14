using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace AION.CoreFramework
{
    public class GameUIConfigTreeWindow: OdinMenuEditorWindow
    {
        private static GameUIConfigTreeWindow _treeWindow;
        private OdinMenuTree tree;
        
        /// <summary>
        /// ` ctrl+Space都不行啊，暂时用ctrl+g吧
        /// </summary>
        [MenuItem("GameTools/打开UI配置工具 %h")]
        public static void Open()
        {
            _treeWindow = GetWindow<GameUIConfigTreeWindow>();
            _treeWindow.Show();
        }
        
        protected override OdinMenuTree BuildMenuTree()
        {
        
            tree = new OdinMenuTree()
            {
                // { "Home", this, EditorIcons.House }, // Draws the this.someData field in this case.
            };
            tree.Config.DrawSearchToolbar = true;
            
            tree.DrawSearchToolbar();
            
            tree.AddAllAssetsAtPath("UI配置", "Assets/Game/Config/UIConfig", typeof(ScriptableObject), true);
            
            tree.SortMenuItemsByName();

            return tree;
        }

     
    }
}