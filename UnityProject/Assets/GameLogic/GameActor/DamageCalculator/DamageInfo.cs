namespace AION.Config.DamageCalculator
{
    public class DamageInfo
    {
        /// <summary>
        /// 攻击者
        /// </summary>
        public Unit Attacker { get; private set; }
        
        /// <summary>
        /// 受害者
        /// </summary>
        public Unit Target { get; private set; }
        
        /// <summary>
        /// 伤害
        /// </summary>
        public float Damage { get; set; }
        
        /// <summary>
        /// 伤害类型
        /// </summary>
        public EnumDamageType DamageType { get; set; }
        
        /// <summary>
        /// 魔法伤害类型
        /// </summary>
        public EnumMagicalDamageType MagicalDamageType { get; set; }
        
    }
}