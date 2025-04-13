using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace AION.CoreFramework
{
    public class BuildWindow:OdinEditorWindow
    {
        [MenuItem("TEngine/ABBuilder")]
        public static void Open()
        {
            var window = GetWindow<BuildWindow>();
            window.Show();
        }
        [Button("Build AB")]
        public void BuildAB()
        {
            BuilderControl builderControl = new BuilderControl();
            builderControl.Build();
        }
    }
}