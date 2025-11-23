using AION.CoreFramework;

namespace GameLogic
{
    [EventInterface(EEventGroup.BattleLogic)]

    public interface IActorEvent
    {
        void NumbericChange(NumericType type, float previousValue, float newValue);
        
        /// <summary>
        /// 攻击事件（攻击者触发）
        /// </summary>
        void OnAttack();
        
        /// <summary>
        /// 暴击事件（受击者触发，当受到暴击伤害时）
        /// </summary>
        void OnCriticalHit();
        
        /// <summary>
        /// 死亡事件
        /// </summary>
        void OnDeath();
    }
}