using System;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace AION.Config
{
    [Serializable]
    public abstract class BaseTargetSelector
    {
        [LabelText("单位类型")]
        public EnumUnitType unitType;
        [FormerlySerializedAs("campType")] [LabelText("阵营")]
        public EnumCampType enumCampType;
        public abstract Unit[] GetAllTargets();
        public abstract bool IsTarget(Unit unit);
    }
    
    [Serializable]
    public class  AroundTargetSelector : BaseTargetSelector
    {
        
        [LabelText("半径")]
        public float Range = 2;
        
        public override Unit[] GetAllTargets()
        {
            //TODO：获取该单位line of sight范围内的所有单位
            return null;
        }

        public override bool IsTarget(Unit unit)
        {
            return true;
        }
    }
}