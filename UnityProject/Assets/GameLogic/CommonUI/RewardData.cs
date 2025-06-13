namespace AION.CoreFramework
{
    public class RewardData
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        public int PropId;
        
        /// <summary>
        /// 数量
        /// </summary>
        public int Num;
        
        public RewardData(int propId, int num)
        {
            PropId = propId;
            Num = num;
        }
    }

    /// <summary>
    /// 道具数据
    /// </summary>
    public class PropData
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        public int PropId;
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
        public int Quality;
        /// <summary>
        /// 价格
        /// </summary>
        public int Price;
        
    }
    
}