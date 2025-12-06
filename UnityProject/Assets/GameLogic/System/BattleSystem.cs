using System;
using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 抽卡结果数据
    /// </summary>
    [System.Serializable]
    public class DrawCardResult
    {
        /// <summary>
        /// 抽到的建筑ID列表（1-3个）
        /// </summary>
        public List<int> towerIds;
        
        /// <summary>
        /// 抽卡时间戳
        /// </summary>
        public float timestamp;
        
        public DrawCardResult(List<int> towerIds, float timestamp)
        {
            this.towerIds = towerIds ?? new List<int>();
            this.timestamp = timestamp;
        }
    }
    
    /// <summary>
    /// 抽Buff结果数据
    /// </summary>
    [System.Serializable]
    public class DrawBuffResult
    {
        /// <summary>
        /// 抽到的BuffID
        /// </summary>
        public int buffId;
        
        /// <summary>
        /// Buff配置
        /// </summary>
        public BuffConfig buffConfig;
        
        /// <summary>
        /// 抽卡时间戳
        /// </summary>
        public float timestamp;
        
        public DrawBuffResult(int buffId, BuffConfig buffConfig, float timestamp)
        {
            this.buffId = buffId;
            this.buffConfig = buffConfig;
            this.timestamp = timestamp;
        }
    }
    
    /// <summary>
    /// 战斗系统 - 管理局内战斗相关数据（基地血量、经验、等级、金币、抽卡等）
    /// </summary>
    public class BattleSystem : BaseLogicSys<BattleSystem>
    {
        // ========== 基地血量 ==========
        private float m_baseHp = 100f;
        private float m_baseMaxHp = 100f;
        
        /// <summary>
        /// 基地当前血量（只读）
        /// </summary>
        public float BaseHp => m_baseHp;
        
        /// <summary>
        /// 基地最大血量（只读）
        /// </summary>
        public float BaseMaxHp => m_baseMaxHp;
        
        // ========== 经验与等级 ==========
        private int m_currentExp = 0;
        private int m_level = 1;
        private int m_expToNextLevel = 100; // 升级所需经验
        
        /// <summary>
        /// 当前经验值（只读）
        /// </summary>
        public int CurrentExp => m_currentExp;
        
        /// <summary>
        /// 当前等级（只读）
        /// </summary>
        public int Level => m_level;
        
        /// <summary>
        /// 升级所需经验（只读）
        /// </summary>
        public int ExpToNextLevel => m_expToNextLevel;
        
        // ========== 金币 ==========
        private int m_gold = 0;
        
        /// <summary>
        /// 已获取的金币（只读）
        /// </summary>
        public int Gold => m_gold;
        
        // ========== 抽卡系统 ==========
        private int m_drawCardCost = 10; // 每次抽取的金币消耗
        private List<DrawCardResult> m_drawCardHistory = new List<DrawCardResult>(); // 抽取历史记录
        private List<DrawBuffResult> m_drawBuffHistory = new List<DrawBuffResult>(); // 抽Buff历史记录
        
        // 抽奖系统
        private LotterySystem<TowerConfig> m_towerLottery; // 建筑抽奖系统
        private LotterySystem<BuffConfig> m_buffLottery; // Buff抽奖系统
        
        /// <summary>
        /// 每次抽取的金币消耗（只读）
        /// </summary>
        public int DrawCardCost => m_drawCardCost;
        
        /// <summary>
        /// 抽卡历史记录（只读）
        /// </summary>
        public IReadOnlyList<DrawCardResult> DrawCardHistory => m_drawCardHistory;
        
        /// <summary>
        /// 抽Buff历史记录（只读）
        /// </summary>
        public IReadOnlyList<DrawBuffResult> DrawBuffHistory => m_drawBuffHistory;
        
        // ========== 游戏状态控制 ==========
        private bool m_isPaused = false;
        private float m_timeScale = 1f; // 游戏倍速（1.0 = 正常速度，2.0 = 2倍速）
        
        /// <summary>
        /// 游戏是否暂停（只读）
        /// </summary>
        public bool IsPaused => m_isPaused;
        
        /// <summary>
        /// 游戏倍速（只读）
        /// </summary>
        public float TimeScale => m_timeScale;
        
        // ========== 事件 ==========
        /// <summary>
        /// 基地血量变化事件 (currentHp, maxHp)
        /// </summary>
        public Action<float, float> OnBaseHpChanged;
        
        /// <summary>
        /// 经验变化事件 (currentExp, level)
        /// </summary>
        public Action<int, int> OnExpChanged;
        
        /// <summary>
        /// 等级提升事件 (newLevel)
        /// </summary>
        public Action<int> OnLevelUp;
        
        /// <summary>
        /// 金币变化事件 (newGold)
        /// </summary>
        public Action<int> OnGoldChanged;
        
        /// <summary>
        /// 抽卡事件 (towerIds, result)
        /// </summary>
        public Action<List<int>, DrawCardResult> OnCardDrawn;
        
        /// <summary>
        /// 抽Buff事件 (buffId, result)
        /// </summary>
        public Action<int, DrawBuffResult> OnBuffDrawn;
        
        /// <summary>
        /// 游戏暂停/恢复事件 (isPaused)
        /// </summary>
        public Action<bool> OnPauseChanged;
        
        /// <summary>
        /// 游戏倍速变化事件 (timeScale)
        /// </summary>
        public Action<float> OnTimeScaleChanged;
        
        public override void OnStart()
        {
            base.OnStart();
            ResetBattleData();
            InitializeLotterySystems();
        }
        
        /// <summary>
        /// 初始化抽奖系统（从配置表加载并设置稀有度评分）
        /// </summary>
        private void InitializeLotterySystems()
        {
            // 初始化建筑抽奖系统
            m_towerLottery = new LotterySystem<TowerConfig>();
            m_towerLottery.SetDistributionFactor(0.5f); // 默认分布参数
            m_towerLottery.SetDistributionParams(0.5f, 0.2f); // 期望值0.5，标准差0.2
            
            // 初始化Buff抽奖系统
            m_buffLottery = new LotterySystem<BuffConfig>();
            m_buffLottery.SetDistributionFactor(0.5f);
            m_buffLottery.SetDistributionParams(0.5f, 0.2f);
            
            // 从配置表加载建筑并设置稀有度评分
            LoadTowerLotteryItems();
            
            // 从配置表加载Buff并设置稀有度评分
            LoadBuffLotteryItems();
            
            Log.Info("BattleSystem: 抽奖系统初始化完成");
        }
        
        /// <summary>
        /// 加载建筑抽奖项（从配置表读取并设置稀有度评分）
        /// </summary>
        private void LoadTowerLotteryItems()
        {
            if (ConfigSystem.Instance?.Tables?.TbTower == null)
            {
                Log.Warning("BattleSystem: 建筑表未初始化，无法加载抽奖项");
                return;
            }
            
            var towerTable = ConfigSystem.Instance.Tables.TbTower;
            foreach (var towerConfig in towerTable.DataList)
            {
                // 根据建筑ID或其他属性计算稀有度评分
                // 这里使用简单的规则：ID越大，稀有度越高（可以根据实际需求修改）
                float rarityScore = CalculateTowerRarity(towerConfig);
                m_towerLottery.AddItem(towerConfig.Id, towerConfig, rarityScore);
            }
            
            Log.Info($"BattleSystem: 加载了 {towerTable.DataList.Count} 个建筑抽奖项");
        }
        
        /// <summary>
        /// 计算建筑稀有度评分（可以根据实际需求修改算法）
        /// </summary>
        private float CalculateTowerRarity(TowerConfig towerConfig)
        {
            // 简单规则：根据ID计算（ID越大越稀有）
            // 实际应该根据配置表的稀有度字段或其他属性计算
            float baseRarity = towerConfig.Id / 100f; // 假设ID范围在1-100
            
            // 可以根据其他属性调整，例如：
            // - 攻击范围越大，稀有度越高
            // - 攻击间隔越小，稀有度越高
            // 这里简化处理，实际应该从配置表读取稀有度字段
            
            return Mathf.Clamp01(baseRarity);
        }
        
        /// <summary>
        /// 加载Buff抽奖项（从配置表读取并设置稀有度评分）
        /// </summary>
        private void LoadBuffLotteryItems()
        {
            if (ConfigSystem.Instance?.Tables?.TbBuff == null)
            {
                Log.Warning("BattleSystem: Buff表未初始化，无法加载抽奖项");
                return;
            }
            
            var buffTable = ConfigSystem.Instance.Tables.TbBuff;
            foreach (var buffConfig in buffTable.DataList)
            {
                // 根据Buff属性计算稀有度评分
                float rarityScore = CalculateBuffRarity(buffConfig);
                m_buffLottery.AddItem(buffConfig.Id, buffConfig, rarityScore);
            }
            
            Log.Info($"BattleSystem: 加载了 {buffTable.DataList.Count} 个Buff抽奖项");
        }
        
        /// <summary>
        /// 计算Buff稀有度评分（可以根据实际需求修改算法）
        /// </summary>
        private float CalculateBuffRarity(BuffConfig buffConfig)
        {
            // 简单规则：根据ID计算（ID越大越稀有）
            // 实际应该根据配置表的稀有度字段或其他属性计算
            float baseRarity = buffConfig.Id / 100f; // 假设ID范围在1-100
            
            // 可以根据其他属性调整，例如：
            // - 持续时间越长，稀有度越高
            // - 效果数量越多，稀有度越高
            // - MaxStacks越大，稀有度越高
            // 这里简化处理，实际应该从配置表读取稀有度字段
            
            return Mathf.Clamp01(baseRarity);
        }
        
        /// <summary>
        /// 设置建筑抽奖分布参数
        /// </summary>
        public void SetTowerLotteryDistribution(float distributionFactor, float expectedValue = 0.5f, float standardDeviation = 0.2f)
        {
            if (m_towerLottery != null)
            {
                m_towerLottery.SetDistributionFactor(distributionFactor);
                m_towerLottery.SetDistributionParams(expectedValue, standardDeviation);
                Log.Info($"BattleSystem: 设置建筑抽奖分布参数 - Factor={distributionFactor}, Expected={expectedValue}, StdDev={standardDeviation}");
            }
        }
        
        /// <summary>
        /// 设置Buff抽奖分布参数
        /// </summary>
        public void SetBuffLotteryDistribution(float distributionFactor, float expectedValue = 0.5f, float standardDeviation = 0.2f)
        {
            if (m_buffLottery != null)
            {
                m_buffLottery.SetDistributionFactor(distributionFactor);
                m_buffLottery.SetDistributionParams(expectedValue, standardDeviation);
                Log.Info($"BattleSystem: 设置Buff抽奖分布参数 - Factor={distributionFactor}, Expected={expectedValue}, StdDev={standardDeviation}");
            }
        }
        
        /// <summary>
        /// 重置战斗数据（开始新战斗时调用）
        /// </summary>
        public void ResetBattleData()
        {
            m_baseHp = 100f;
            m_baseMaxHp = 100f;
            m_currentExp = 0;
            m_level = 1;
            m_expToNextLevel = 100;
            m_gold = 0;
            m_drawCardCost = 1;
            m_drawCardHistory.Clear();
            m_drawBuffHistory.Clear();
            m_isPaused = false;
            m_timeScale = 1f;
            
            // 更新Unity的Time.timeScale
            Time.timeScale = m_timeScale;
            
            Log.Info("BattleSystem: 重置战斗数据");
        }
        
        // ========== 基地血量管理 ==========
        
        /// <summary>
        /// 设置基地最大血量
        /// </summary>
        public void SetBaseMaxHp(float maxHp)
        {
            if (maxHp <= 0)
            {
                Log.Warning($"BattleSystem: 设置基地最大血量无效，MaxHp={maxHp}");
                return;
            }
            
            m_baseMaxHp = maxHp;
            if (m_baseHp > m_baseMaxHp)
            {
                m_baseHp = m_baseMaxHp;
            }
            
            OnBaseHpChanged?.Invoke(m_baseHp, m_baseMaxHp);
            Log.Info($"BattleSystem: 设置基地最大血量 = {maxHp}");
        }
        
        /// <summary>
        /// 设置基地当前血量
        /// </summary>
        public void SetBaseHp(float hp)
        {
            m_baseHp = Mathf.Clamp(hp, 0f, m_baseMaxHp);
            OnBaseHpChanged?.Invoke(m_baseHp, m_baseMaxHp);
            
            if (m_baseHp <= 0f)
            {
                Log.Warning("BattleSystem: 基地血量归零！");
            }
        }
        
        /// <summary>
        /// 基地受到伤害
        /// </summary>
        public void DamageBase(float damage)
        {
            if (damage <= 0f) return;
            
            SetBaseHp(m_baseHp - damage);
            Log.Info($"BattleSystem: 基地受到伤害 {damage}，当前血量 = {m_baseHp}/{m_baseMaxHp}");
        }
        
        /// <summary>
        /// 基地恢复血量
        /// </summary>
        public void HealBase(float heal)
        {
            if (heal <= 0f) return;
            
            SetBaseHp(m_baseHp + heal);
            Log.Info($"BattleSystem: 基地恢复血量 {heal}，当前血量 = {m_baseHp}/{m_baseMaxHp}");
        }
        
        // ========== 经验与等级管理 ==========
        
        /// <summary>
        /// 添加经验
        /// </summary>
        public void AddExp(int exp)
        {
            if (exp <= 0) return;
            
            m_currentExp += exp;
            
            // 检查是否升级
            while (m_currentExp >= m_expToNextLevel)
            {
                m_currentExp -= m_expToNextLevel;
                LevelUp();
            }
            
            OnExpChanged?.Invoke(m_currentExp, m_level);
            Log.Info($"BattleSystem: 获得经验 {exp}，当前经验 = {m_currentExp}/{m_expToNextLevel}，等级 = {m_level}");
        }
        
        /// <summary>
        /// 升级
        /// </summary>
        private void LevelUp()
        {
            m_level++;
            // 升级所需经验递增（简单公式：每级增加50）
            m_expToNextLevel = 100 + (m_level - 1) * 50;
            
            OnLevelUp?.Invoke(m_level);
            Log.Info($"BattleSystem: 升级！当前等级 = {m_level}，下一级所需经验 = {m_expToNextLevel}");
        }
        
        // ========== 金币管理 ==========
        
        /// <summary>
        /// 添加金币
        /// </summary>
        public void AddGold(int gold)
        {
            if (gold <= 0) return;
            
            m_gold += gold;
            OnGoldChanged?.Invoke(m_gold);
            Log.Info($"BattleSystem: 获得金币 {gold}，当前金币 = {m_gold}");
        }
        
        /// <summary>
        /// 消耗金币
        /// </summary>
        public bool ConsumeGold(int gold)
        {
            if (gold <= 0) return true;
            
            if (m_gold < gold)
            {
                Log.Warning($"BattleSystem: 金币不足，需要 {gold}，当前只有 {m_gold}");
                return false;
            }
            
            m_gold -= gold;
            OnGoldChanged?.Invoke(m_gold);
            Log.Info($"BattleSystem: 消耗金币 {gold}，剩余金币 = {m_gold}");
            return true;
        }
        
        // ========== 抽卡系统 ==========
        
        /// <summary>
        /// 设置抽卡消耗
        /// </summary>
        public void SetDrawCardCost(int cost)
        {
            if (cost < 0)
            {
                Log.Warning($"BattleSystem: 设置抽卡消耗无效，Cost={cost}");
                return;
            }
            
            m_drawCardCost = cost;
            Log.Info($"BattleSystem: 设置抽卡消耗 = {cost}");
        }
        
        /// <summary>
        /// 抽卡（本地逻辑）- 使用抽奖系统抽取1-3个建筑
        /// </summary>
        /// <returns>抽到的建筑ID列表，如果失败返回null</returns>
        public List<int> DrawCard()
        {
            // 检查金币是否足够
            if (!ConsumeGold(m_drawCardCost))
            {
                return null;
            }
            
            // 使用抽奖系统抽取
            if (m_towerLottery == null || m_towerLottery.Items.Count == 0)
            {
                Log.Warning("BattleSystem: 建筑抽奖系统未初始化，无法抽卡");
                return null;
            }
            
            // 随机抽取1-3个建筑
            int count = UnityEngine.Random.Range(1, 4); // 1-3个
            var results = m_towerLottery.DrawMultiple(count);
            
            if (results == null || results.Count == 0)
            {
                Log.Warning("BattleSystem: 抽奖失败，未获得任何建筑");
                return null;
            }
            
            List<int> towerIds = results.Select(r => r.item.id).ToList();
            
            // 记录抽卡结果
            DrawCardResult result = new DrawCardResult(towerIds, Time.time);
            m_drawCardHistory.Add(result);
            
            // 触发事件
            OnCardDrawn?.Invoke(towerIds, result);
            Log.Info($"BattleSystem: 抽卡成功，获得 {towerIds.Count} 个建筑，ID = [{string.Join(", ", towerIds)}]，消耗金币 = {m_drawCardCost}");
            
            return towerIds;
        }
        
        /// <summary>
        /// 抽Buff（本地逻辑）- 使用抽奖系统抽取
        /// </summary>
        /// <returns>抽到的BuffID，如果失败返回-1</returns>
        public int DrawBuff()
        {
            // 检查金币是否足够
            if (!ConsumeGold(m_drawCardCost))
            {
                return -1;
            }
            
            // 使用抽奖系统抽取
            if (m_buffLottery == null || m_buffLottery.Items.Count == 0)
            {
                Log.Warning("BattleSystem: Buff抽奖系统未初始化，无法抽Buff");
                return -1;
            }
            
            var result = m_buffLottery.Draw();
            if (result == null || result.item == null)
            {
                Log.Warning("BattleSystem: 抽奖失败，未获得任何Buff");
                return -1;
            }
            
            int buffId = result.item.id;
            BuffConfig selectedBuff = result.item.data;
            
            // 记录抽Buff结果
            DrawBuffResult drawResult = new DrawBuffResult(buffId, selectedBuff, Time.time);
            m_drawBuffHistory.Add(drawResult);
            
            // 触发事件
            OnBuffDrawn?.Invoke(buffId, drawResult);
            Log.Info($"BattleSystem: 抽Buff成功，获得BuffID = {buffId} ({selectedBuff.Name})，稀有度评分 = {result.item.rarityScore:F2}，消耗金币 = {m_drawCardCost}");
            
            // 自动应用Buff（根据Buff类型的目标选择施加）
            ApplyBuff(selectedBuff);
            
            return buffId;
        }
        
        /// <summary>
        /// 应用Buff（根据Buff类型的目标选择施加）
        /// </summary>
        /// <param name="buffConfig">Buff配置</param>
        /// <returns>成功应用的目标数量</returns>
        public int ApplyBuff(BuffConfig buffConfig)
        {
            if (buffConfig == null)
            {
                Log.Warning("BattleSystem: Buff配置为空，无法应用");
                return 0;
            }
            
            if (ActorMgr.Instance == null)
            {
                Log.Warning("BattleSystem: ActorMgr未初始化，无法应用Buff");
                return 0;
            }
            
            // 根据TargetType选择目标
            List<GameActor> targets = SelectBuffTargets(buffConfig);
            
            if (targets == null || targets.Count == 0)
            {
                Log.Warning($"BattleSystem: 未找到Buff目标，BuffID = {buffConfig.Id}, TargetType = {buffConfig.TargetType}");
                return 0;
            }
            
            // 对每个目标应用Buff
            int successCount = 0;
            foreach (var target in targets)
            {
                if (target == null || target.IsDestroyed) continue;
                
                bool success = BuffFactory.CreateAndAddBuff(buffConfig, target);
                if (success)
                {
                    successCount++;
                }
            }
            
            Log.Info($"BattleSystem: 应用Buff成功，BuffID = {buffConfig.Id}，目标数量 = {successCount}/{targets.Count}");
            return successCount;
        }
        
        /// <summary>
        /// 根据Buff配置选择目标
        /// </summary>
        private List<GameActor> SelectBuffTargets(BuffConfig buffConfig)
        {
            if (buffConfig == null) return new List<GameActor>();
            
            List<GameActor> targets = new List<GameActor>();
            
            // 根据TargetType选择目标
            switch (buffConfig.TargetType)
            {
                case ETargetType.Friendly:
                    // 友方目标：所有塔和基地
                    targets = ActorMgr.Instance.Actors
                        .Where(actor => 
                            (actor.Tag == UnitTag.Tower || actor.Tag == UnitTag.Base) &&
                            !actor.IsDestroyed)
                        .ToList();
                    break;
                    
                case ETargetType.Enemy:
                    // 敌方目标：所有敌人
                    targets = ActorMgr.Instance.Actors
                        .Where(actor => 
                            actor.Tag == UnitTag.Enemy &&
                            !actor.IsDestroyed)
                        .ToList();
                    break;
                    
                case ETargetType.Self:
                    // 自身：如果有基地，选择基地；否则选择第一个塔
                    var baseActor = ActorMgr.Instance.Actors.FirstOrDefault(a => a.Tag == UnitTag.Base && !a.IsDestroyed);
                    if (baseActor != null)
                    {
                        targets.Add(baseActor);
                    }
                    else
                    {
                        var firstTower = ActorMgr.Instance.Actors.FirstOrDefault(a => a.Tag == UnitTag.Tower && !a.IsDestroyed);
                        if (firstTower != null)
                        {
                            targets.Add(firstTower);
                        }
                    }
                    break;
                    
                default:
                    Log.Warning($"BattleSystem: 未知的TargetType = {buffConfig.TargetType}");
                    break;
            }
            
            // 如果TargetParams有参数，可能需要进一步筛选（例如：范围、数量等）
            // 这里简化处理，实际应该根据TargetParams进行更精确的选择
            if (buffConfig.TargetParams != null && buffConfig.TargetParams.Count > 0)
            {
                // 例如：TargetParams[0]可能是范围，TargetParams[1]可能是数量
                // 这里可以根据实际需求实现更复杂的目标选择逻辑
            }
            
            return targets;
        }
        
        /// <summary>
        /// 获取抽卡历史记录数量
        /// </summary>
        public int GetDrawCardCount()
        {
            return m_drawCardHistory.Count;
        }
        
        /// <summary>
        /// 获取抽Buff历史记录数量
        /// </summary>
        public int GetDrawBuffCount()
        {
            return m_drawBuffHistory.Count;
        }
        
        /// <summary>
        /// 清空抽卡历史
        /// </summary>
        public void ClearDrawCardHistory()
        {
            m_drawCardHistory.Clear();
            Log.Info("BattleSystem: 清空抽卡历史");
        }
        
        /// <summary>
        /// 清空抽Buff历史
        /// </summary>
        public void ClearDrawBuffHistory()
        {
            m_drawBuffHistory.Clear();
            Log.Info("BattleSystem: 清空抽Buff历史");
        }
        
        // ========== 游戏状态控制 ==========
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (m_isPaused) return;
            
            m_isPaused = true;
            Time.timeScale = 0f; // Unity暂停
            
            OnPauseChanged?.Invoke(true);
            Log.Info("BattleSystem: 游戏已暂停");
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (!m_isPaused) return;
            
            m_isPaused = false;
            Time.timeScale = m_timeScale; // 恢复倍速
            
            OnPauseChanged?.Invoke(false);
            Log.Info($"BattleSystem: 游戏已恢复，倍速 = {m_timeScale}");
        }
        
        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (m_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        /// <summary>
        /// 设置游戏倍速
        /// </summary>
        public void SetTimeScale(float timeScale)
        {
            if (timeScale < 0.1f || timeScale > 5f)
            {
                Log.Warning($"BattleSystem: 设置游戏倍速无效，TimeScale={timeScale}，应在0.1-5.0之间");
                return;
            }
            
            m_timeScale = timeScale;
            
            // 如果游戏未暂停，立即应用倍速
            if (!m_isPaused)
            {
                Time.timeScale = m_timeScale;
            }
            
            OnTimeScaleChanged?.Invoke(m_timeScale);
            Log.Info($"BattleSystem: 设置游戏倍速 = {timeScale}");
        }
        
        /// <summary>
        /// 获取战斗数据快照（用于保存）
        /// </summary>
        public BattleDataSnapshot GetSnapshot()
        {
            return new BattleDataSnapshot
            {
                baseHp = m_baseHp,
                baseMaxHp = m_baseMaxHp,
                currentExp = m_currentExp,
                level = m_level,
                expToNextLevel = m_expToNextLevel,
                gold = m_gold,
                drawCardCost = m_drawCardCost,
                drawCardHistory = new List<DrawCardResult>(m_drawCardHistory),
                drawBuffHistory = new List<DrawBuffResult>(m_drawBuffHistory),
                isPaused = m_isPaused,
                timeScale = m_timeScale
            };
        }
        
        /// <summary>
        /// 从快照恢复战斗数据（用于加载）
        /// </summary>
        public void LoadFromSnapshot(BattleDataSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Log.Warning("BattleSystem: 快照为空，无法加载");
                return;
            }
            
            m_baseHp = snapshot.baseHp;
            m_baseMaxHp = snapshot.baseMaxHp;
            m_currentExp = snapshot.currentExp;
            m_level = snapshot.level;
            m_expToNextLevel = snapshot.expToNextLevel;
            m_gold = snapshot.gold;
            m_drawCardCost = snapshot.drawCardCost;
            m_drawCardHistory = snapshot.drawCardHistory ?? new List<DrawCardResult>();
            m_drawBuffHistory = snapshot.drawBuffHistory ?? new List<DrawBuffResult>();
            m_isPaused = snapshot.isPaused;
            m_timeScale = snapshot.timeScale;
            
            // 应用游戏状态
            Time.timeScale = m_isPaused ? 0f : m_timeScale;
            
            Log.Info("BattleSystem: 从快照恢复战斗数据");
        }
    }
    
    /// <summary>
    /// 战斗数据快照（用于保存/加载）
    /// </summary>
    [System.Serializable]
    public class BattleDataSnapshot
    {
        public float baseHp;
        public float baseMaxHp;
        public int currentExp;
        public int level;
        public int expToNextLevel;
        public int gold;
        public int drawCardCost;
        public List<DrawCardResult> drawCardHistory;
        public List<DrawBuffResult> drawBuffHistory;
        public bool isPaused;
        public float timeScale;
    }
}

