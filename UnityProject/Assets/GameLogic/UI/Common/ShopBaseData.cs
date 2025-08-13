using System.Collections.Generic;

namespace GameLogic
{
    public enum PriceType
    {
        Ad,
        Money,
        Item
    }
    public class ShopBaseData
    {
        public int ID { get; set; }
        
        
        public int Price { get; set; }
        /// <summary>
        /// 最大购买数量
        /// </summary>
        public int MaxBuyCnt { get; set; }
        
        /// <summary>
        /// 当前购买数量
        /// </summary>
        public int BuyCnt { get; set; }
        
        /// <summary>
        ///  是否可以购买
        /// </summary>
        public bool CanBuy{ get; set; }

        public bool HasBuyAll
        {
            get
            {
                return BuyCnt > 0 && BuyCnt >= MaxBuyCnt;   
            }
        }
        
        public bool HasBuyAny
        {
            get
            {
                return BuyCnt > 0;
            }
        }

        public List<ItemBaseShowData> Rewards { get; protected set; }

        /// <summary>
        /// 项目通用的购买逻辑
        /// </summary>
        public void DoBuy()
        {
            
            
        }

    }
}