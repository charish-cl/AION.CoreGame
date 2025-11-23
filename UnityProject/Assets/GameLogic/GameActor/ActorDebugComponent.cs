using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// Actor调试组件，用于在编辑器中显示GameActor的组件信息
    /// </summary>
    public class ActorDebugComponent : MonoBehaviour
    {
        [SerializeField]
        private bool m_showInInspector = true;
        
        /// <summary>
        /// 是否在Inspector中显示
        /// </summary>
        public bool ShowInInspector
        {
            get { return m_showInInspector; }
            set { m_showInInspector = value; }
        }
        
        /// <summary>
        /// 获取关联的GameActor
        /// </summary>
        public GameActor GetActor()
        {
            // 从ActorMgr中查找关联的GameActor
            if (ActorMgr.Instance != null && ActorMgr.Instance.Actors != null)
            {
                foreach (var actor in ActorMgr.Instance.Actors)
                {
                    if (actor != null && actor.m_Owner == this.gameObject)
                    {
                        return actor;
                    }
                }
            }
            
            return null;
        }
        
        private void OnValidate()
        {
            // 在编辑器中验证时，确保组件已正确设置
        }
    }
}

