using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    public class ActorAnimViewCmp : GameActorCmp
    {
        WrapAnimator _wrapAnimator;
        
        MoveLogicCmp moveLogic ;
    
        const string WalkAnimName = "Walking";
        const string RunAnimName = "Running";
        const string IdleAnimName = "Idle";
        const string DamageAnimName = "Attack";
        const string CriticalAnimName = "Critical";
        const string DeadAnimName = "Dead";
        
        // 基准速度（用于归一化动画速度）
        private float m_baseMoveSpeed = 1.0f;  // 基准移动速度
        private float m_baseAttackSpeed = 1.0f; // 基准攻速
        
        // 当前动画类型（用于判断是否需要更新动画速度）
        private string m_currentAnimName = "";
        private bool m_isAttackAnim = false;
        
        public override void OnInit()
        {
            base.OnInit();
            moveLogic = GetComponent<MoveLogicCmp>();
            if (Actor.Transform != null)
            {
                Animator animator = Actor.Transform.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    _wrapAnimator = new WrapAnimator(animator);
                }
                else
                {
                    Log.Warning($"ActorAnimViewCmp: Actor {Actor.GetType().Name} 的 Transform 下没有找到 Animator 组件");
                }
            }
            
            // 获取基准速度（从配置或默认值）
            InitializeBaseSpeeds();
            
            // 注册事件监听（参考 HPBarCmp 的方式）
            Actor.EventDispatcher.AddEventListener(IActorEvent_Event.OnAttack, OnAttack, this);
            Actor.EventDispatcher.AddEventListener(IActorEvent_Event.OnCriticalHit, OnCriticalHit, this);
            Actor.EventDispatcher.AddEventListener(IActorEvent_Event.OnDeath, OnDeath, this);
        }
        
        /// <summary>
        /// 初始化基准速度
        /// </summary>
        private void InitializeBaseSpeeds()
        {
            // 尝试从配置获取基准移动速度
            var unitComponent = Actor.GetComponent<UnitComponent>();
            if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
            {
                m_baseMoveSpeed = unitComponent.Config.MoveSpeed;
                if (m_baseMoveSpeed <= 0f)
                {
                    m_baseMoveSpeed = 1.0f; // 默认值
                }
            }
            else
            {
                // 从 NumericComponent 获取基准移动速度
                if (Actor.NumericComponent != null)
                {
                    float baseSpeed = Actor.NumericComponent.Get<float>(NumericType.SpeedBase);
                    if (baseSpeed > 0f)
                    {
                        m_baseMoveSpeed = baseSpeed;
                    }
                }
            }
            
            // 尝试从配置获取基准攻速
            if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
            {
                // UnitConfig 中有 AttackInterval，攻速 = 1 / AttackInterval
                float attackInterval = unitComponent.Config.AttackInterval;
                if (attackInterval > 0f)
                {
                    m_baseAttackSpeed = 1f / attackInterval;
                }
            }
            else
            {
                // 从 NumericComponent 获取基准攻速
                // 注意：AttackSpeedBase 存储的是 AttackInterval（攻击间隔），需要转换为攻速
                if (Actor.NumericComponent != null)
                {
                    float baseAttackInterval = Actor.NumericComponent.Get<float>(NumericType.AttackSpeedBase);
                    if (baseAttackInterval > 0f)
                    {
                        // AttackSpeedBase 存储的是攻击间隔，攻速 = 1 / 攻击间隔
                        m_baseAttackSpeed = 1f / baseAttackInterval;
                    }
                }
            }
            
            // 如果还是没有，使用默认值
            if (m_baseMoveSpeed <= 0f)
            {
                m_baseMoveSpeed = 1.0f;
            }
            if (m_baseAttackSpeed <= 0f)
            {
                m_baseAttackSpeed = 1.0f;
            }
        }
        
        /// <summary>
        /// 更新动画速度（根据当前属性）
        /// </summary>
        private void UpdateAnimationSpeed()
        {
            if (_wrapAnimator == null)
            {
                return;
            }
            
            // 如果是攻击动画，根据攻速调整
            if (m_isAttackAnim)
            {
                if (Actor.NumericComponent != null)
                {
                    float currentAttackSpeed = Actor.NumericComponent.Get<float>(NumericType.AttackSpeed);
                    if (currentAttackSpeed > 0f && m_baseAttackSpeed > 0f)
                    {
                        // 动画速度 = 当前攻速 / 基准攻速
                        float animSpeed = currentAttackSpeed / m_baseAttackSpeed;
                        // 限制在合理范围内（0.1 到 3.0）
                        animSpeed = Mathf.Clamp(animSpeed, 0.1f, 3.0f);
                        _wrapAnimator.SetAnimSpeed(animSpeed);
                    }
                }
            }
            // 如果是移动动画，根据移动速度调整
            else if (moveLogic != null && moveLogic.IsMoving)
            {
                if (Actor.NumericComponent != null)
                {
                    float currentMoveSpeed = Actor.NumericComponent.Get<float>(NumericType.Speed);
                    if (currentMoveSpeed > 0f && m_baseMoveSpeed > 0f)
                    {
                        // 动画速度 = 当前移动速度 / 基准移动速度
                        float animSpeed = currentMoveSpeed / m_baseMoveSpeed;
                        // 限制在合理范围内（0.1 到 3.0）
                        animSpeed = Mathf.Clamp(animSpeed, 0.1f, 3.0f);
                        _wrapAnimator.SetAnimSpeed(animSpeed);
                    }
                }
            }
            // 其他动画（Idle、Dead等）使用正常速度
            else
            {
                _wrapAnimator.SetAnimSpeed(1.0f);
            }
        }
        
        /// <summary>
        /// 攻击事件回调（攻击者触发）
        /// </summary>
        private void OnAttack()
        {
            if (_wrapAnimator != null)
            {
                _wrapAnimator.PlayAnimation(DamageAnimName);
                m_currentAnimName = DamageAnimName;
                m_isAttackAnim = true;
                // 立即更新攻击动画速度
                UpdateAnimationSpeed();
            }
        }
        
        /// <summary>
        /// 暴击事件回调（受击者触发）
        /// </summary>
        private void OnCriticalHit()
        {
            if (_wrapAnimator != null)
            {
                _wrapAnimator.PlayAnimation(CriticalAnimName);
                m_currentAnimName = CriticalAnimName;
                m_isAttackAnim = true;
                // 立即更新攻击动画速度
                UpdateAnimationSpeed();
            }
        }
        
        /// <summary>
        /// 死亡事件回调
        /// </summary>
        private void OnDeath()
        {
            if (_wrapAnimator != null)
            {
                _wrapAnimator.PlayAnimation(DeadAnimName);
                m_currentAnimName = DeadAnimName;
                m_isAttackAnim = false;
                // 死亡动画使用正常速度
                _wrapAnimator.SetAnimSpeed(1.0f);
            }
        }
        
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (CheckIsEnable(moveLogic))
            {
                if (_wrapAnimator == null)
                {
                    return;
                }
                
                // 检查当前播放的动画
                bool isPlayingAttackAnim = _wrapAnimator.IsPlayingAnimation(DamageAnimName) || 
                                          _wrapAnimator.IsPlayingAnimation(CriticalAnimName);
                
                // 如果正在播放攻击动画，不切换动画
                if (isPlayingAttackAnim)
                {
                    m_isAttackAnim = true;
                    UpdateAnimationSpeed();
                    return;
                }
                
                // 如果不在播放攻击动画，重置标志
                if (m_isAttackAnim && !isPlayingAttackAnim)
                {
                    m_isAttackAnim = false;
                }
                
                if (moveLogic.IsMoving)
                {
                    // 播放移动动画
                    if (m_currentAnimName != WalkAnimName)
                    {
                        _wrapAnimator.PlayAnimation(WalkAnimName);
                        m_currentAnimName = WalkAnimName;
                    }
                    // 更新移动动画速度
                    UpdateAnimationSpeed();
                }
                else
                {
                    // 播放待机动画
                    if (m_currentAnimName != IdleAnimName)
                    {
                        _wrapAnimator.PlayAnimation(IdleAnimName);
                        m_currentAnimName = IdleAnimName;
                        // 待机动画使用正常速度
                        _wrapAnimator.SetAnimSpeed(1.0f);
                    }
                }
            }
        }
    }
}