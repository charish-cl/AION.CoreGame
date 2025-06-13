using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace GameDevKit
{
    public abstract class MyNode:Node
    {
        public abstract void OnCreated(MyGraphView view);
  
        
    }

    public class NormalNode : MyNode
    {
        
        public override void OnCreated(MyGraphView view)
        {
            title = "节点";

            var inputport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                typeof(Port));
            inputContainer.Add(inputport);
            var outputport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                typeof(Port));
            outputContainer.Add(outputport);
        }
    }
    public class RootNode : MyNode
    {

        public override void OnCreated(MyGraphView view)
        {
            title = "Root";

            capabilities -= Capabilities.Deletable;
            var outputport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                typeof(Port));
            outputport.portName = "Out";
            outputContainer.Add(outputport);
        }
    }

    public class ShowScriptObjectNode : MyNode
    {
        private UnityEditor.Editor editor;
        public override void OnCreated(MyGraphView view)
        {
            UnityEngine.Object.DestroyImmediate(editor);
            // var tree=AssetDatabase.LoadAssetAtPath<BehaviourTree>("Assets/TheKiwiCoder/BehaviourTree/Example/BehaviourTree_RandomWalk.asset");
            // editor = UnityEditor.Editor.CreateEditor(tree);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (editor.target != null)
                    editor.OnInspectorGUI();
            });
            Add(container);
            
            title = "行为树";
            var inputport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                typeof(Port));
            inputContainer.Add(inputport);
        }
    }
    
}