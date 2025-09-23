using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    using System.Collections.Generic;

    public enum NumericType
    {
        Max = 10000,

        Speed = 1000,
        SpeedBase = Speed * 10 + 1,
        SpeedAdd = Speed * 10 + 2,
        SpeedPct = Speed * 10 + 3,
        SpeedFinalAdd = Speed * 10 + 4,
        SpeedFinalPct = Speed * 10 + 5,

        Hp = 1001,
        HpBase = Hp * 10 + 1,

        MaxHp = 1002,
        MaxHpBase = MaxHp * 10 + 1,
        MaxHpAdd = MaxHp * 10 + 2,
        MaxHpPct = MaxHp * 10 + 3,
        MaxHpFinalAdd = MaxHp * 10 + 4,
        MaxHpFinalPct = MaxHp * 10 + 5,
        
        
        AttackSpeed = 1003,
        AttackSpeedBase = AttackSpeed * 10 + 1,
        AttackSpeedAdd = AttackSpeed * 10 + 2,
        AttackSpeedPct = AttackSpeed * 10 + 3,
        AttackSpeedFinalAdd = AttackSpeed * 10 + 4,
        AttackSpeedFinalPct = AttackSpeed * 10 + 5,
        
        Attack = 1004,
        AttackBase = Attack * 10 + 1,
        AttackAdd = Attack * 10 + 2,
        AttackPct = Attack * 10 + 3,
        AttackFinalAdd = Attack * 10 + 4,
        AttackFinalPct = Attack * 10 + 5,
        
        Defense = 1005,
        DefenseBase = Defense * 10 + 1,
        DefenseAdd = Defense * 10 + 2,
    }

    public class NumericComponent : GameActorCmp
    {
        public readonly Dictionary<int, int> NumericDic = new Dictionary<int, int>();

        //这里初始化基础数值
        public override void OnInit()
        {
            base.OnInit();
        }
        
        public float GetAsFloat(NumericType numericType)
        {
            return (float)GetByKey((int)numericType) / 10000;
        }

        public void Set(NumericType nt, float value)
        {
            this[nt] = (int)(value * 10000); // 通过这种方式模拟float
        }
    
        
        public int GetAsInt(NumericType numericType)
        {
            return GetByKey((int)numericType);
        }
        public void Set(NumericType nt, int value)
        {
            this[nt] = value;
        }

        public int this[NumericType numericType]
        {
            get { return this.GetByKey((int)numericType); }
            private set
            {
                int v = this.GetByKey((int)numericType);
                if (v == value)
                {
                    return;
                }

                NumericDic[(int)numericType] = value;

                //这里应该更新
                Update(numericType);
            }
        }

 

        private int GetByKey(int key)
        {
            int value = 0;
            this.NumericDic.TryGetValue(key, out value);
            return value;
        }

        //每次数值改变的时候调用下这个函数
        public void Update(NumericType numericType)
        {
            //不能直接传Speed 1000 这种值，要传SpeedBase 10001 这种值
            if (numericType < NumericType.Max)
            {
                Log.Error($"NumericType error {numericType} 不能小于 {NumericType.Max}");
            	return;
            }
            // eg : 10021/10 = 1002  这里传进的偏移值最终都会定位到1002这个数值
            int final = (int)numericType / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            float preValue = GetByKey(final);
            
            // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
            // final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
            this.NumericDic[final] =
                ((this.GetByKey(bas) + this.GetByKey(add)) * (100 + this.GetByKey(pct)) / 100 +
                 this.GetByKey(finalAdd)) * (100 + this.GetByKey(finalPct)) / 100;


            if (!Mathf.Approximately(preValue, GetAsInt((NumericType)final)))
            {
                Log.Info("NumericComponent Update {0} {1} {2}", (NumericType)final, preValue, GetAsInt(numericType));
            }
            //这个不行ActorEvent没有Get方法，后面看看能不能封装个接口出来
            // GameEvent.Get<IActorEvent_Gen>().NumbericChange((NumericType)final, preValue,  (float)GetAsInt(numericType));
            Actor.EventDispatcher.SendEvent(IActorEvent_Event.NumbericChange, (NumericType)final, preValue,  (float)GetAsInt(numericType));
            // GameEvent.Get<INumeric>().NumericChange(this.Entity.Id, (NumericType)final, preValue, GetAsInt(numericType));
            // Game.EventSystem.Run(EventIdType.NumbericChange, this.Entity.Id, numericType, final);
        }
    }
}