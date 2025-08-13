using GameLogic;

namespace AION.CoreFramework
{    
    public enum ItemQuality
    {
        Normal,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 道具数据
    /// </summary>
    public class BaseItemShowData
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        public int Id;
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int GID;
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;
        
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon;
        
        /// <summary>
        /// 品质
        /// </summary>
        public ItemQuality Quality;

        /// <summary>
        /// 数量
        /// </summary>
        public int ItemCnt;
        
        
        public void InitBaseItemData()
        {
            Id = 0;
            GID = 0;
            Name = "";
            Desc = "";
            Icon = "";
            Quality = ItemQuality.Normal;
        }


        public virtual void DoIconClick()
        {
            // GameModule.UI.ShowWindow<>();
        }
    }
}