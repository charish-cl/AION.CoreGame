using System.Collections.Generic;

namespace GameLogic
{
    public enum ItemSystemType
    {
        
    }
    public enum GoodsNumStrType
    {
        Normal,
        NeedNum
    }

    public enum ColorQuality
    {
        Gray,
        Blue, // 蓝色
        Green, // 绿色 
        Yellow, // 黄色
        Red, // 红色
        Purple, // 紫色
    }

    public abstract class ItemBaseShowData
    {
        public ItemSystemType SystemType { get; protected set; }
        public GoodsNumStrType NumStrType { get; set; }
        public ColorQuality Quality { get; protected set; }
        public uint GID { get; protected set; }
        public uint ID { get; protected set; }
        
        public string Name { get; protected set; }
        
        public string IconName { get; protected set; }
        
        public string Desc { get; protected set; }
        
        public string ItemTypeName { get; protected set; }
        public List<uint> GetWays { get; protected set; }

        public int NeedNum { get; protected set; }

        // public ItemLabelType ItemLabelType { get; protected set; }
        // public virtual CSBootyAwardItem BootyAwardItemEntry { get; protected set; }
        public virtual bool IsCanShowGetWay => GetWays != null && GetWays.Count > 0;
        public virtual string IconBgName => $"com_pz_A{(int)Quality}";
        public abstract int HaveNum { get; }

        /// <summary>
        /// 使用数据前判断该数据是否有效
        /// </summary>
        public abstract bool IsInvalid { get; }

        /// <summary>
        /// 是否是玩家拥有的
        /// </summary>
        public abstract bool IsOwned { get; }

        /// <summary>
        /// 拥有角标的显示
        /// </summary>
        // public virtual RewardItemChoseOrGotType OwnedGotType { get; }

        public bool IsEnough => HaveNum >= NeedNum;

        protected bool m_useGoodsNum;
        protected int m_goodsNum;

        public ItemBaseShowData(ItemSystemType systemType, uint id)
        {
            SystemType = systemType;
            ID = id;
            // InternalInitBaseData();
            SetItemNum(-1);
            SetNeedNum(-1);
        }

        // public virtual void UpdateAwardItemEntry(CSBootyAwardItem awardItem)
        // {
        //     BootyAwardItemEntry = awardItem;
        // }

        public void SetItemNum(int num)
        {
            if (num < 0)
            {
                m_useGoodsNum = false;
                m_goodsNum = 0;
            }
            else
            {
                m_useGoodsNum = true;
                m_goodsNum = num;
            }
        }

        public void SetNeedNum(int needNum)
        {
            if (needNum < 0)
            {
                NeedNum = 0;
                NumStrType = GoodsNumStrType.Normal;
            }
            else
            {
                NeedNum = needNum;
                NumStrType = GoodsNumStrType.NeedNum;
            }
        }

        public virtual string GetNumStr()
        {
            var str = string.Empty;
            // switch (NumStrType)
            // {
            //     case GoodsNumStrType.Normal:
            //         if (HaveNum > 1)
            //         {
            //             str = TextConfigMgr.Instance.NumDisposeEn(HaveNum);
            //         }
            //
            //         break;
            //     case GoodsNumStrType.NeedNum:
            //         string haveStr = TextConfigMgr.Instance.NumDisposeEn(HaveNum);
            //         string needStr = TextConfigMgr.Instance.NumDisposeEn(NeedNum);
            //         if (HaveNum >= NeedNum)
            //         {
            //             haveStr = TextConfigMgr.Instance.GetText(TextDefine.ID_COLOR_COMM_CURRENCY_ENOUGH, haveStr);
            //         }
            //         else
            //         {
            //             haveStr = TextConfigMgr.Instance.GetText(TextDefine.ID_COLOR_COMM_CURRENCY_NOT_ENOUGH, haveStr);
            //         }
            //
            //         str = $"{haveStr}/{needStr}";
            //         break;
            // }

            return str;
        }

        public virtual void DoClick()
        {
            // UISys.Mgr.ShowWindowAsync<ItemTipsUI>(ui => ui.InitData(this));
        }
    }
}