using System.Collections.Generic;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 随机概率条件参数类
    /// </summary>
    public class RandomConditionParams
    {
        public float Probability = 0.5f; // 触发概率 (0-1)
    }

    /// <summary>
    /// 随机概率条件
    /// </summary>
    [BuffCondition(ETriggerType.Probability)]
    public class RandomCondition : BaseCondition
    {
        public RandomCondition(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId)
        {
        }

        public override bool Check()
        {
            // 使用GetParam自动填充参数
            var param = GetParam<RandomConditionParams>();

            // 检查概率是否在有效范围内
            if (param.Probability <= 0f)
                return false;
            if (param.Probability >= 1f)
                return true;

            // 生成随机数并检查是否满足概率
            float random = Random.Range(0f, 1f);
            return random <= param.Probability;
        }
    }
}
