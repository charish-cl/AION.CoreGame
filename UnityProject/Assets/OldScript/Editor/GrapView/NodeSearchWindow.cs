using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameDevKit
{
    public class MySearchWindow:ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow m_Window;
        private MyGraphView m_NodeGraphView;

        private Texture2D _indentationIcon;
        private Type[] nodeTypes;

        public void Init(EditorWindow window,MyGraphView myGraphView)
        {
            m_Window = window;
            m_NodeGraphView = myGraphView;
            var editorAssembly = typeof(MyNode).Assembly;
            editorAssembly.GetTypes();
            
            nodeTypes = editorAssembly.GetTypes()
                .Where(a => a.IsSubclassOf(typeof(MyNode))).ToArray();

        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Comment Block",_indentationIcon))
                {
                    level = 1,
                    userData = new Group()
                }
            };

            if (nodeTypes != null && nodeTypes.Length > 0)
            {
                for (int i = 0; i < nodeTypes.Length; i++)
                {
                    var entry = new SearchTreeEntry(new GUIContent(nodeTypes[i].FullName))
                    {
                        level = 2, userData = nodeTypes[i].FullName
                    };
                    tree.Add(entry);
                }
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {

            //Editor window-based mouse position
            var mousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(m_Window.rootVisualElement.parent,
                context.screenMousePosition - m_Window.position.position);
            var graphMousePosition = m_NodeGraphView.contentViewContainer.WorldToLocal(mousePosition);
            
            var editorAssembly = typeof(MyNode).Assembly;
            var typeName = (string) SearchTreeEntry.userData;
            var newNode = editorAssembly.CreateInstance(typeName);
            var nodeType = newNode.GetType();
            var onCreated = nodeType.GetMethod("OnCreated");
            onCreated?.Invoke(newNode, new[] {m_NodeGraphView});
            var node = newNode as MyNode;
            node.SetPosition(new Rect(graphMousePosition, node.GetPosition().size));
            m_NodeGraphView.AddElement(node);
            
            return true;

        }
    }
}