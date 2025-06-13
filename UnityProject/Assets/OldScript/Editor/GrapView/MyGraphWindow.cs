using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameDevKit
{
    public class MyGraphWindow:EditorWindow
    {
        private static MyGraphWindow Window;
        private MyGraphView _graphView;
        [MenuItem("Tool/打开")]
        public static void Open()
        {
            if (Window==null)
            {
                Window = GetWindow<MyGraphWindow>();
            }
            else
            {
                Window.rootVisualElement.Clear();
            }
            Window.Show();
        }

        private void OnEnable()
        {
            //绑定视图
            rootVisualElement.Add(new MyGraphView(this)
            {
                style = { flexGrow = 1}
            });
           
        }
        
        
    }

    public class MyGraphView : GraphView
    {
       

        
        public MyGraphView(EditorWindow window)
        {
            SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);
            Insert(0,new GridBackground());
            //拖拽背景
            this.AddManipulator(new ContentDragger());
            //拖拽节点
            this.AddManipulator(new SelectionDragger());
            Add(new RootNode());
            AddSearchWindow(window);
        }
        private void AddSearchWindow(EditorWindow window)
        {
            //初始搜索栏
            var searchWindow = ScriptableObject.CreateInstance<MySearchWindow>();
            searchWindow.Init(window, this);
            nodeCreationRequest += (e) =>
            {
                SearchWindow.Open(new SearchWindowContext(e.screenMousePosition), searchWindow);
            };
        }
    
        /// <summary>
        /// 返回连接的端口
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts=new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (startPort.node ==port.node||
                    startPort.direction == port.direction||
                    startPort.portType!= port.portType)
                {
                    continue;
                }
                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }
        
    }
}