using System;
using System.Collections.Generic;
using GameBase;
using AION.CoreFramework;

namespace GameLogic.UISys
{
    public enum RedNodeType
    {
        RedNodeDynamic,
        RedNodeHome,
        RedNodeShop,
        RedNodeTask,
        RedNodeMission,
    }

    public class RedNodeMgr : Singleton<RedNodeMgr>
    {
        private Dictionary<RedNodeType, RedNodeBase> m_RedNodes = new Dictionary<RedNodeType, RedNodeBase>();


        private Dictionary<RedNodeType, List<RedNodeType>> m_RedNodeRelationDic =
            new Dictionary<RedNodeType, List<RedNodeType>>()
            {
                {
                    RedNodeType.RedNodeHome, new List<RedNodeType>()
                    {
                        RedNodeType.RedNodeShop,
                        RedNodeType.RedNodeTask,
                        RedNodeType.RedNodeMission,
                    }
                },
            };

        public RedNodeBase GetRedNode(RedNodeType type)
        {
            if (m_RedNodes.ContainsKey(type))
            {
                return m_RedNodes[type];
            }

            return null;
        }


        protected override void Init()
        {
            //绑定红点关系
            //先去重每个加一遍
            foreach (var item in m_RedNodeRelationDic)
            {
                foreach (var child in item.Value)
                {
                    if (GetRedNode(child) == null)
                    {
                        m_RedNodes[child] = new RedNodeBase(child);
                    }
                }

                if (GetRedNode(item.Key) == null)
                {
                    m_RedNodes[item.Key] = new RedNodeBase(item.Key);
                }
            }

            //绑定红点
            foreach (var item in m_RedNodeRelationDic)
            {
                RedNodeBase redNode = GetRedNode(item.Key);
                foreach (var child in item.Value)
                {
                    redNode.BindChild(GetRedNode(child));
                }
            }
        }
    }


    public class RedNodeBase
    {
        int m_RedNum;
        RedNodeType m_RedType;
        Action OnValueChanged;
        List<RedNodeBase> m_ChildrenNodes = new List<RedNodeBase>();

        public RedNodeBase(RedNodeType type)
        {
            m_RedType = type;
            m_RedNum = 0;
        }

        public void BindChild(RedNodeBase redNode)
        {
            m_ChildrenNodes.Add(redNode);
            redNode.Bind(OnValueChanged);
        }

        public int RedNum
        {
            get { return m_RedNum; }
            set
            {
                m_RedNum = value;
                OnValueChanged?.Invoke();
            }
        }

        public int ChildrenRedNum
        {
            get
            {
                int num = 0;
                for (int i = 0; i < m_ChildrenNodes.Count; i++)
                {
                    num += m_ChildrenNodes[i].RedNum;
                }

                return num;
            }
        }

        public bool IsShow
        {
            get
            {
                if (m_RedNum > 0)
                {
                    return true;
                }

                for (int i = 0; i < m_ChildrenNodes.Count; i++)
                {
                    if (m_ChildrenNodes[i].IsShow)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {
                if (value)
                {
                    m_RedNum = 1;
                }
                else
                {
                    m_RedNum = 0;
                }

                OnValueChanged?.Invoke();
            }
        }

        Dictionary<object, RedNodeBase> m_DynamicRedNodes = new Dictionary<object, RedNodeBase>();

        public RedNodeBase GetDynamicRedNode(object id)
        {
            //TODO:动态红点
            if (m_DynamicRedNodes.ContainsKey(id))
            {
                return m_DynamicRedNodes[id];
            }
            else
            {
                RedNodeBase redNode = new RedNodeBase(RedNodeType.RedNodeDynamic);
                m_DynamicRedNodes.Add(id, redNode);
                return redNode;
            }
        }

        public RedNodeBase GetChildNode(RedNodeType type)
        {
            for (int i = 0; i < m_ChildrenNodes.Count; i++)
            {
                if (m_ChildrenNodes[i].m_RedType == type)
                {
                    return m_ChildrenNodes[i];
                }
            }

            return null;
        }

        public void Bind(Action onValueChanged)
        {
            OnValueChanged += onValueChanged;
        }
    }
}