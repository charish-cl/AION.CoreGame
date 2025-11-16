using System.Collections.Generic;
using AION.Config.Buff;
using AION.CoreFramework;
using UnityEngine;
using GameConfig.battle;

namespace GameLogic
{
    public class BuffCmp:GameActorCmp
    {
        private List<BaseBuff> buffs = new List<BaseBuff>();

        CharacterBuffAttribute _buffAttribute ;
        
        private Dictionary<BaseBuff, float> _buffTimers = new Dictionary<BaseBuff, float>();
        
        // 叠加层数管理：Key是BuffId，Value是当前层数和Buff实例
        private Dictionary<int, (int stackCount, BaseBuff buff)> _buffStacks = new Dictionary<int, (int, BaseBuff)>();

        
        /// <summary>
        /// 添加Buff，支持叠加层数
        /// </summary>
        public void AddBuff(BaseBuff buff)
        {
            if (buff == null) return;
            
            // 检查是否已存在相同ID的Buff
            var existingBuff = GetBuffById(buff.BuffId);
            if (existingBuff != null)
            {
                // 获取配置检查是否可以叠加
                var config = ConfigSystem.Instance.Tables.TbBuff.GetOrDefault(buff.BuffId);
                if (config != null && config.MaxStacks > 0)
                {
                    // 可以叠加，增加层数
                    if (_buffStacks.ContainsKey(buff.BuffId))
                    {
                        var (stackCount, existing) = _buffStacks[buff.BuffId];
                        if (stackCount < config.MaxStacks)
                        {
                            // 刷新持续时间
                            _buffTimers[existing] = Time.realtimeSinceStartup;
                            _buffStacks[buff.BuffId] = (stackCount + 1, existing);
                            Log.Info($"Buff {buff.BuffId} stacked: {stackCount + 1}/{config.MaxStacks}");
                            return;
                        }
                        else
                        {
                            // 已达到最大层数，刷新持续时间
                            _buffTimers[existing] = Time.realtimeSinceStartup;
                            Log.Info($"Buff {buff.BuffId} already at max stacks, refreshed duration");
                            return;
                        }
                    }
                }
                else
                {
                    // 不能叠加或已达到最大层数，刷新持续时间
                    _buffTimers[existingBuff] = Time.realtimeSinceStartup;
                    Log.Info($"Buff {buff.BuffId} refreshed duration");
                    return;
                }
            }
            
            // 添加新Buff
            buffs.Add(buff);
            // 如果Buff还没有设置目标，则设置目标（避免重复调用OnStart）
            if (buff.TargetActor == null)
            {
                buff.OnStart(Actor);
            }
            if (buff.Modifier != null)
            {
                _buffAttribute.AddModifier(buff.Modifier);
            }
            _buffTimers.Add(buff, Time.realtimeSinceStartup);
            
            // 记录叠加层数
            _buffStacks[buff.BuffId] = (1, buff);
            
            Log.Info("Buff added: " + buff.Id);
        }
        
        /// <summary>
        /// 根据BuffID添加Buff
        /// </summary>
        public void AddBuff(int buffId)
        {
            var buff = BuffFactory.CreateBuff(buffId, Actor);
            if (buff != null)
            {
                AddBuff(buff);
            }
        }

        public void RemoveBuff(BaseBuff buff)
        {
            if (buff == null || !buffs.Contains(buff)) return;
            
            buffs.Remove(buff);
            buff.OnEnd();
            
            if (buff.Modifier != null)
            {
                _buffAttribute.RemoveModifier(buff.Modifier);
            }
            _buffTimers.Remove(buff);
            
            // 移除叠加记录
            if (_buffStacks.ContainsKey(buff.BuffId))
            {
                _buffStacks.Remove(buff.BuffId);
            }
            
            Log.Info("Buff removed: " + buff.Id);
        }
        
        /// <summary>
        /// 根据BuffID获取Buff
        /// </summary>
        public BaseBuff GetBuffById(int buffId)
        {
            foreach (var buff in buffs)
            {
                if (buff.BuffId == buffId)
                {
                    return buff;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取Buff的叠加层数
        /// </summary>
        public int GetBuffStackCount(int buffId)
        {
            if (_buffStacks.ContainsKey(buffId))
            {
                return _buffStacks[buffId].stackCount;
            }
            return 0;
        }

        public void Update()
        {
            float currentTime = Time.realtimeSinceStartup;
            
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                BaseBuff buff = buffs[i];
                
                if (!_buffTimers.ContainsKey(buff)) continue;
                
                float elapsedTime = currentTime - _buffTimers[buff];
                buff.OnUpdate(elapsedTime);
                
                if (buff.CheckExpired())
                {
                    RemoveBuff(buff);
                }
            }
        }
        
        NumericComponent _numericComponent;
        public override void OnInit()
        {
            _numericComponent = GetComponent<NumericComponent>();
            _buffAttribute = new CharacterBuffAttribute(_numericComponent);
        }

        public override void OnUpdate()
        {
            Update();
        }

        public override void OnDestroy()
        {
            // 清理所有Buff
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                RemoveBuff(buffs[i]);
            }
            buffs.Clear();
            _buffTimers.Clear();
            _buffStacks.Clear();
        }
    }
}
