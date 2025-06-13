using System;
using AION.CoreFramework;

namespace GameLogic.UISys
{
    /// <summary>
    /// 通用的红点Icon UI
    /// </summary>
    public class RedNodeIcon : UIWidget
    {
        private RedNodeBase redNode;

        public void OnValueChanged()
        {
        }


        public void Bind(RedNodeType redNodeType)
        {
            redNode = RedNodeMgr.Instance.GetRedNode(redNodeType);
            redNode.Bind(OnValueChanged);
            
        }
    }
}