namespace GameLogic
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
    
    
}