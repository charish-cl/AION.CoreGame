using AION.CoreFramework;
using DamageNumbersPro;
using UnityEngine;

namespace GameLogic
{
    public class HealthCmp : GameActorCmp
    {
        public int HP
        {
            get
            {
                return this.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp);
            }
            set
            {
                this.GetComponent<NumericComponent>().Set(NumericType.HpBase, value);
            }
        }
        public DamageNumber numberPrefab;

        public override void OnInit()
        {
            base.OnInit();
            numberPrefab = SceneMgr.Instance.SceneBehavior.numberPrefab;
        }
        
        public static float CalculateDamage(float attackerAtk, float defenderDef, float skillMultiplier, 
            float attackerCritRate, float attackerCritDmg, 
            float baseCritChance = 0.05f, float baseCritDmg = 1.5f)
        {
            
            //此处还可以计算miss效果
            
            // 1. 计算基础伤害 (攻击 - 防御)
            float baseDamage = Mathf.Max(0, attackerAtk - defenderDef);
            baseDamage *= skillMultiplier;

            // 2. 判断暴击
            float randomValue = Random.Range(0f, 1f);
            float totalCritChance = baseCritChance + attackerCritRate;
            totalCritChance = Mathf.Clamp01(totalCritChance); // 暴击率钳制在0-1之间
            bool isCrit = randomValue <= totalCritChance;
            float critMultiplier = isCrit ? (baseCritDmg + attackerCritDmg) : 1f;
            

            // 3. 计算最终伤害
            float finalDamage = baseDamage * critMultiplier;

            // 4. 可以继续叠加其他乘区（元素伤害、易伤等）...
            // finalDamage *= (1 + elementDmgBonus); 

            
            Log.Info($"Damage: {finalDamage}, IsCrit: {isCrit}");
            
            return finalDamage;
        }
        public void TakeDamage(NumericComponent other)
        {
            NumericComponent self = this.GetComponent<NumericComponent>();
            
            int damage = (int)CalculateDamage(other[NumericType.Attack],self[NumericType.Defense],1f,0f,0f);
            HP -= damage;
            if (HP <= 0)
            {
                //Destroy the actor.
                Actor.Destroy();
            }
            
            numberPrefab.Spawn(Actor.Position + new Vector2(0, 0.5f), damage);
            
        }
    }
}