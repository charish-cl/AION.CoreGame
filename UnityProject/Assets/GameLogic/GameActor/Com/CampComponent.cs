using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 基地组件，用于管理基地的生命值和游戏结束事件
    /// </summary>
    public class CampComponent : GameActorCmp
    {
        private HealthCmp m_healthCmp;
        
        /// <summary>
        /// 基地最大生命值
        /// </summary>
        public int MaxHp { get; set; } = 1000;
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHp => m_healthCmp?.HP ?? 0;
        
        /// <summary>
        /// 游戏结束事件（当基地HP<=0时触发）
        /// </summary>
        public System.Action OnGameOver;
        
        public override void OnInit()
        {
            base.OnInit();
            m_healthCmp = GetComponent<HealthCmp>();
            
            if (m_healthCmp == null)
            {
                Log.Error("BaseCampComponent: 基地需要HealthCmp组件");
                return;
            }
            
            // 设置初始生命值
            m_healthCmp.HP = MaxHp;
            
            // 监听生命值变化事件
            Actor.EventDispatcher.AddEventListener<NumericType, float, float>(IActorEvent_Event.NumbericChange, OnNumericChange, Actor);
        }
        
        /// <summary>
        /// 数值变化事件处理
        /// </summary>
        private void OnNumericChange(NumericType numericType, float oldValue, float newValue)
        {
            if (numericType == NumericType.Hp)
            {
                int currentHp = (int)newValue;
                if (currentHp <= 0)
                {
                    // 基地被摧毁，游戏结束
                    OnGameOver?.Invoke();
                    Log.Warning("基地被摧毁，游戏结束！");
                }
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}

