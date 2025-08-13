using System.Collections.Generic;

namespace GameLogic
{
    public class ItemHelper
    {
        /// <summary>
        /// 按照奖励品质排序
        /// </summary>
        /// <param name="rewards"></param>
        public static void SortRewardByQuality(List<RewardData> rewards)
        {
            
        }

        public static RewardData MergeRewards(List<RewardData> rewards)
        {
            // for (var i = 0; i < rewards.Count; i++)
            // {
            //     if (rewards[i].Type == RewardType.Gold)
            //     {
            //         return rewards[i];
            //     }
            // }

            return rewards[0];
        }
    }
}