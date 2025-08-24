using System.Linq;
using GameDevKitEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OldScript.Editor.SuperTableBuilder
{
    public class LubanBeanBuilderWindow : OdinMenuEditorWindow
    {
        
        [MenuItem("Tools/Luban对象构建器")]
        private static void Open()
        {
            var window = GetWindow<LubanBeanBuilderWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            tree.DrawSearchToolbar();
            
            tree.AddAllAssetsAtPath("", "Assets/Game/Config/Define", typeof(SerializedScriptableObject), true);
            
            return tree;
        }
        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    GUILayout.Label(selected.Name);
                }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Item")))
                {
                    ScriptableObjectCreator.ShowDialog<LubanBeanBuilderSo>("Assets/Game/Config/Define", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

         
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
    
    
    public class LubanBeanBuilderSo : SerializedScriptableObject
    {
        public string Name;
        
        // public BeanField[] Fields;
    }
}