using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameConfig.level;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 关卡阶段枚举
    /// </summary>
    public enum LevelPhase
    {
        None,           // 未开始
        BuildPhase,     // 建塔阶段（回合开始/结束，玩家可以建造塔）
        BattlePhase,    // 战斗阶段（敌人生成和战斗）
        WaveEnd,        // 波次结束（等待玩家点击下一波）
        LevelComplete,  // 关卡完成
        LevelFailed,    // 关卡失败
    }
    
    /// <summary>
    /// 关卡系统，管理关卡和波次逻辑
    /// </summary>
    public class LevelSystem : BaseLogicSys<LevelSystem>
    {
        /// <summary>
        /// 当前关卡阶段
        /// </summary>
        public LevelPhase CurrentPhase { get; private set; } = LevelPhase.None;
        
        /// <summary>
        /// 当前关卡配置
        /// </summary>
        public LevelBaseConfig CurrentLevelConfig { get; private set; }
        
        /// <summary>
        /// 当前关卡ID
        /// </summary>
        public int CurrentLevelId { get; private set; }
        
        /// <summary>
        /// 当前波次索引（在WaveIds列表中的索引）
        /// </summary>
        public int CurrentWaveIndex { get; private set; } = -1;
        
        /// <summary>
        /// 当前波次配置
        /// </summary>
        public WaveConfig CurrentWaveConfig { get; private set; }
        
        /// <summary>
        /// 当前波次ID
        /// </summary>
        public int CurrentWaveId { get; private set; }
        
        /// <summary>
        /// 当前波次已生成的敌人数量（按SpawnGroup统计）
        /// </summary>
        private Dictionary<GameConfig.SpawnGroup, int> m_spawnedCounts = new Dictionary<GameConfig.SpawnGroup, int>();
        
        /// <summary>
        /// 当前活跃的计时器ID列表
        /// </summary>
        private List<int> m_activeTimers = new List<int>();
        
        /// <summary>
        /// SpawnGroup对应的计时器ID（用于停止特定SpawnGroup的计时器）
        /// </summary>
        private Dictionary<GameConfig.SpawnGroup, int> m_spawnGroupTimers = new Dictionary<GameConfig.SpawnGroup, int>();
        
        /// <summary>
        /// 阶段变化事件
        /// </summary>
        public System.Action<LevelPhase> OnPhaseChanged;
        
        /// <summary>
        /// 波次开始事件
        /// </summary>
        public System.Action<WaveConfig> OnWaveStart;
        
        /// <summary>
        /// 波次结束事件
        /// </summary>
        public System.Action<WaveConfig> OnWaveEnd;
        
        public override void OnStart()
        {
            base.OnStart();
        }
        
        /// <summary>
        /// 开始关卡
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        public void StartLevel(int levelId)
        {
            // 读取关卡配置
            if (ConfigSystem.Instance?.Tables?.TbLevelBase == null)
            {
                Log.Error("LevelSystem: ConfigSystem未初始化或TbLevelBase为空");
                return;
            }
            
            CurrentLevelConfig = ConfigSystem.Instance.Tables.TbLevelBase.GetOrDefault(levelId);
            if (CurrentLevelConfig == null)
            {
                Log.Error($"LevelSystem: 未找到关卡配置，LevelId = {levelId}");
                return;
            }
            
            CurrentLevelId = levelId;
            CurrentWaveIndex = -1;
            CurrentWaveConfig = null;
            CurrentWaveId = 0;
            m_spawnedCounts.Clear();
            m_spawnGroupTimers.Clear();
            ClearAllTimers();
            
            // 设置基地生命值
            if (SceneMgr.Instance.TryGetBase(out var baseActor))
            {
                var baseCampComponent = baseActor.GetComponent<CampComponent>();
                
                if (baseCampComponent != null)
                {
                    baseCampComponent.MaxHp = CurrentLevelConfig.BaseHp;
                    var healthCmp = baseActor.GetComponent<HealthCmp>();
                    if (healthCmp != null)
                    {
                        healthCmp.HP = CurrentLevelConfig.BaseHp;
                    }
                }
            }
            
            // 进入建塔阶段
            ChangePhase(LevelPhase.BuildPhase);
            
            Log.Info($"LevelSystem: 开始关卡 {CurrentLevelConfig.Name} (ID: {levelId})");
        }
        
        /// <summary>
        /// 下一波（由玩家点击触发）
        /// </summary>
        public void NextWave()
        {
            if (CurrentPhase != LevelPhase.BuildPhase && CurrentPhase != LevelPhase.WaveEnd)
            {
                Log.Warning($"LevelSystem: 当前阶段 {CurrentPhase} 不能进入下一波");
                return;
            }
            
            if (CurrentLevelConfig == null)
            {
                Log.Error("LevelSystem: 当前没有加载关卡配置");
                return;
            }
            
            // 检查是否还有下一波
            if (CurrentWaveIndex + 1 >= CurrentLevelConfig.WaveIds.Count)
            {
                // 所有波次完成，关卡完成
                ChangePhase(LevelPhase.LevelComplete);
                Log.Info($"LevelSystem: 关卡 {CurrentLevelConfig.Name} 完成！");
                return;
            }
            
            // 进入下一波
            CurrentWaveIndex++;
            int waveId = CurrentLevelConfig.WaveIds[CurrentWaveIndex];
            
            // 读取波次配置
            if (ConfigSystem.Instance?.Tables?.TbWave == null)
            {
                Log.Error("LevelSystem: TbWave为空");
                return;
            }
            
            CurrentWaveConfig = ConfigSystem.Instance.Tables.TbWave.GetOrDefault(waveId);
            if (CurrentWaveConfig == null)
            {
                Log.Error($"LevelSystem: 未找到波次配置，WaveId = {waveId}");
                return;
            }
            
            CurrentWaveId = waveId;
            m_spawnedCounts.Clear();
            m_spawnGroupTimers.Clear();
            ClearAllTimers();
            
            // 进入战斗阶段
            ChangePhase(LevelPhase.BattlePhase);
            OnWaveStart?.Invoke(CurrentWaveConfig);
            
            Log.Info($"LevelSystem: 开始第 {CurrentWaveConfig.WaveIndex} 波 - {CurrentWaveConfig.Name}");
            
            // 开始生成敌人
            StartSpawningEnemies();
        }
        
        /// <summary>
        /// 开始生成敌人
        /// </summary>
        private void StartSpawningEnemies()
        {
            if (CurrentWaveConfig == null || CurrentWaveConfig.SpawnGroups == null)
            {
                Log.Warning("LevelSystem: 当前波次配置无效");
                return;
            }
            
            foreach (var spawnGroup in CurrentWaveConfig.SpawnGroups)
            {
                if (spawnGroup == null)
                    continue;
                
                // 初始化已生成数量
                m_spawnedCounts[spawnGroup] = 0;
                
                // 如果有延迟，先等待延迟时间
                if (spawnGroup.SpawnDelay > 0)
                {
                    int delayTimerId = GameModule.Timer.AddTimer(
                        (args) => StartSpawnGroup(spawnGroup),
                        spawnGroup.SpawnDelay,
                        false,
                        false
                    );
                    m_activeTimers.Add(delayTimerId);
                }
                else
                {
                    // 立即开始生成
                    StartSpawnGroup(spawnGroup);
                }
            }
        }
        
        /// <summary>
        /// 开始生成某个SpawnGroup的敌人
        /// </summary>
        private void StartSpawnGroup(GameConfig.SpawnGroup spawnGroup)
        {
            if (spawnGroup == null)
                return;
            
            // 检查是否已经生成完毕
            if (!m_spawnedCounts.ContainsKey(spawnGroup))
            {
                m_spawnedCounts[spawnGroup] = 0;
            }
            
            if (m_spawnedCounts[spawnGroup] >= spawnGroup.SpawnCount)
            {
                return; // 已经生成完毕
            }
            
            // 立即生成第一个敌人
            SpawnEnemy(spawnGroup);
            
            // 如果还有剩余敌人，设置循环计时器（从第二个敌人开始）
            if (m_spawnedCounts[spawnGroup] < spawnGroup.SpawnCount && spawnGroup.SpawnInterval > 0)
            {
                int timerId = GameModule.Timer.AddTimer(
                    (args) => SpawnEnemy(spawnGroup),
                    spawnGroup.SpawnInterval,
                    true, // 循环
                    false
                );
                m_activeTimers.Add(timerId);
                m_spawnGroupTimers[spawnGroup] = timerId;
            }
        }
        
        /// <summary>
        /// 生成一个敌人
        /// </summary>
        private void SpawnEnemy(GameConfig.SpawnGroup spawnGroup)
        {
            if (spawnGroup == null)
                return;
            
            // 检查是否已经生成完毕
            if (!m_spawnedCounts.ContainsKey(spawnGroup))
            {
                m_spawnedCounts[spawnGroup] = 0;
            }
            
            if (m_spawnedCounts[spawnGroup] >= spawnGroup.SpawnCount)
            {
                // 已经生成完毕，停止对应的计时器
                if (m_spawnGroupTimers.ContainsKey(spawnGroup))
                {
                    int timerId = m_spawnGroupTimers[spawnGroup];
                    GameModule.Timer.Stop(timerId);
                    m_activeTimers.Remove(timerId);
                    m_spawnGroupTimers.Remove(spawnGroup);
                }
                return;
            }
            
            // 检查当前阶段
            if (CurrentPhase != LevelPhase.BattlePhase)
            {
                // 如果不在战斗阶段，停止生成
                return;
            }
            
            // 生成敌人
            SceneMgr.Instance.CreateEnemyByUnitId(spawnGroup.UnitId);
            m_spawnedCounts[spawnGroup]++;
            
            Log.Info($"LevelSystem: 生成敌人 UnitId={spawnGroup.UnitId}, 已生成 {m_spawnedCounts[spawnGroup]}/{spawnGroup.SpawnCount}");
            
            // 检查是否所有SpawnGroup都生成完毕
            CheckWaveComplete();
        }
        
        /// <summary>
        /// 检查波次是否完成
        /// </summary>
        private void CheckWaveComplete()
        {
            if (CurrentWaveConfig == null || CurrentWaveConfig.SpawnGroups == null)
                return;
            
            // 检查所有SpawnGroup是否都生成完毕
            bool allComplete = true;
            foreach (var spawnGroup in CurrentWaveConfig.SpawnGroups)
            {
                if (spawnGroup == null)
                    continue;
                
                if (!m_spawnedCounts.ContainsKey(spawnGroup))
                {
                    allComplete = false;
                    break;
                }
                
                if (m_spawnedCounts[spawnGroup] < spawnGroup.SpawnCount)
                {
                    allComplete = false;
                    break;
                }
            }
            
            // 检查场景中是否还有敌人
            bool hasEnemies = false;
            foreach (var actor in SceneMgr.Instance.Actors)
            {
                if (actor.Tag == UnitTag.Enemy && !actor.IsDestroyed)
                {
                    hasEnemies = true;
                    break;
                }
            }
            
            // 如果所有敌人都生成完毕且场景中没有敌人了，波次结束
            if (allComplete && !hasEnemies)
            {
                EndWave();
            }
        }
        
        /// <summary>
        /// 波次结束
        /// </summary>
        private void EndWave()
        {
            if (CurrentPhase != LevelPhase.BattlePhase)
                return;
            
            ClearAllTimers();
            ChangePhase(LevelPhase.WaveEnd);
            OnWaveEnd?.Invoke(CurrentWaveConfig);
            
            Log.Info($"LevelSystem: 第 {CurrentWaveConfig.WaveIndex} 波结束");
        }
        
        /// <summary>
        /// 改变阶段
        /// </summary>
        private void ChangePhase(LevelPhase newPhase)
        {
            if (CurrentPhase == newPhase)
                return;
            
            LevelPhase oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            
            Log.Info($"LevelSystem: 阶段变化 {oldPhase} -> {newPhase}");
            OnPhaseChanged?.Invoke(newPhase);
        }
        
        /// <summary>
        /// 清除所有计时器
        /// </summary>
        private void ClearAllTimers()
        {
            foreach (var timerId in m_activeTimers)
            {
                GameModule.Timer.Stop(timerId);
            }
            m_activeTimers.Clear();
            m_spawnGroupTimers.Clear();
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            
            // 在战斗阶段，持续检查波次是否完成
            if (CurrentPhase == LevelPhase.BattlePhase)
            {
                CheckWaveComplete();
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            ClearAllTimers();
        }
    }
}