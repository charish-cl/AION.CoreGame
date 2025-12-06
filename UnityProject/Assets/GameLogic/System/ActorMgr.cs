using System;
using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;
using UnityEngine;
using Object = UnityEngine.Object;
using GameConfig;
using GameConfig.battle;
using GameConfig.res;

namespace GameLogic
{
    public class ActorMgr : BaseLogicSys<ActorMgr>
    {
        public List<GameActor> Actors = new List<GameActor>();
        
        SceneBehavior _sceneBehavior;
        
        public SceneBehavior SceneBehavior
        {
            get
            {
                if (_sceneBehavior == null)
                {
                    _sceneBehavior = Object.FindObjectOfType<SceneBehavior>();
                }
                return _sceneBehavior;
            }
        }
        
        // Actor根节点：GameEntry/Root/Actor
        private Transform m_actorRoot;
        
        // 父对象缓存：Key = "类型_配置ID"，Value = Transform
        private Dictionary<string, Transform> m_parentCache = new Dictionary<string, Transform>();
        
        // 唯一ID生成器：每个配置ID对应一个计数器
        private Dictionary<int, int> m_uniqueIdCounters = new Dictionary<int, int>();
        
        private List<Vector2> _pathNodes = new List<Vector2>();
        public List<Vector2> GetCurentLevelPathNodes()
        {
            if (_pathNodes == null || _pathNodes.Count == 0)
            {
                var path = SceneBehavior.GetPath().ToList();
                
                //按照x轴,y轴的顺序排序
                _pathNodes = path.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
                
            }
            return _pathNodes;
        }
        
        /// <summary>
        /// 获取或创建Actor根节点
        /// </summary>
        private Transform GetActorRoot()
        {
            if (m_actorRoot != null)
            {
                return m_actorRoot;
            }
            
          
            
            Transform actor =  GameModule.Base.transform.Find("Actor");
            if (actor == null)
            {
                throw new Exception("ActorMgr: 未找到Root，创建默认根节点");
            }
            
            m_actorRoot = actor;
            return m_actorRoot;
        }
        
        /// <summary>
        /// 获取或创建父对象
        /// </summary>
        /// <param name="typeName">类型名称（如"子弹"、"英雄"等）</param>
        /// <param name="configId">配置ID</param>
        /// <returns>父对象的Transform</returns>
        private Transform GetOrCreateParent(string typeName, int configId)
        {
            string key = $"{typeName}_{configId}";
            
            if (m_parentCache.TryGetValue(key, out Transform parent))
            {
                if (parent != null)
                {
                    return parent;
                }
                else
                {
                    // 如果父对象被销毁了，从缓存中移除
                    m_parentCache.Remove(key);
                }
            }
            
            // 创建新的父对象
            Transform actorRoot = GetActorRoot();
            GameObject parentGo = new GameObject(key);
            parentGo.transform.SetParent(actorRoot);
            
            m_parentCache[key] = parentGo.transform;
            return parentGo.transform;
        }
        
        /// <summary>
        /// 生成唯一ID
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <returns>唯一ID</returns>
        private int GenerateUniqueId(int configId)
        {
            if (!m_uniqueIdCounters.ContainsKey(configId))
            {
                m_uniqueIdCounters[configId] = 0;
            }
            
            m_uniqueIdCounters[configId]++;
            return m_uniqueIdCounters[configId];
        }
        
        /// <summary>
        /// 获取类型名称（根据UnitTag和配置）
        /// </summary>
        private string GetTypeName(GameActor actor, UnitTag tag)
        {
            // 根据配置获取名称
            string configName = "";
            int configId = 0;
            
            var unitConfig = actor.GetConfig<UnitConfig>();
            if (unitConfig != null)
            {
                configName = unitConfig.Name;
                configId = unitConfig.Id;
            }
            else
            {
                var towerConfig = actor.GetConfig<TowerConfig>();
                if (towerConfig != null)
                {
                    configName = towerConfig.Name;
                    configId = towerConfig.Id;
                }
                else
                {
                    var bulletConfig = actor.GetConfig<BulletConfig>();
                    if (bulletConfig != null)
                    {
                        configName = bulletConfig.Name;
                        configId = bulletConfig.Id;
                    }
                }
            }
            
            // 如果没有配置名称，使用Tag作为类型名称
            if (string.IsNullOrEmpty(configName))
            {
                switch (tag)
                {
                    case UnitTag.Player:
                        return "英雄";
                    case UnitTag.Enemy:
                        return "敌人";
                    case UnitTag.Tower:
                        return "塔";
                    case UnitTag.Bullet:
                        return "子弹";
                    case UnitTag.Base:
                        return "基地";
                    default:
                        return "未知";
                }
            }
            
            return configName;
        }
        
        /// <summary>
        /// 获取配置ID
        /// </summary>
        private int GetConfigId(GameActor actor)
        {
            var unitConfig = actor.GetConfig<UnitConfig>();
            if (unitConfig != null)
            {
                return unitConfig.Id;
            }
            
            var towerConfig = actor.GetConfig<TowerConfig>();
            if (towerConfig != null)
            {
                return towerConfig.Id;
            }
            
            var bulletConfig = actor.GetConfig<BulletConfig>();
            if (bulletConfig != null)
            {
                return bulletConfig.Id;
            }
            
            return 0; // 默认配置ID为0
        }
        
        /// <summary>
        /// 设置GameObject的层级和名称
        /// </summary>
        public void SetupActorGameObject(GameActor actor, GameObject go, UnitTag tag)
        {
            if (go == null || actor == null)
            {
                return;
            }
            
            // 获取类型名称和配置ID
            string typeName = GetTypeName(actor, tag);
            int configId = GetConfigId(actor);
            
            // 获取或创建父对象
            Transform parent = GetOrCreateParent(typeName, configId);
            
            // 生成唯一ID
            int uniqueId = GenerateUniqueId(configId);
            
            // 设置GameObject名称：优先使用 Prefab 名字，如果没有则使用配置ID_唯一ID
            string prefabName = actor.PrefabName;
            if (!string.IsNullOrEmpty(prefabName))
            {
                go.name = prefabName;
            }
            else
            {
                go.name = $"{configId}_{uniqueId}";
            }
            
            // 设置父对象
            go.transform.SetParent(parent);
        }
        
        public void RemoveActor(GameActor actor)
        {
            actor.OnDestroy();
            Actors.Remove(actor);
        }

        /// <summary>
        /// 创建玩家（通过UnitConfig ID）
        /// </summary>
        /// <param name="unitId">单位配置ID，如果为0则使用默认配置</param>
        /// <param name="position">生成位置，如果不指定则使用SceneBehavior的SpawnPoint</param>
        public void CreatePlayer(int unitId = 0, Vector2? position = null)
        {
            var actor = new PlayerActor(unitId);
            
            // 如果没有指定位置，使用SceneBehavior的SpawnPoint
            Vector2 spawnPosition = position ?? (SceneBehavior?.SpawnPoint?.position ?? Vector2.zero);
            
            CreateActorInternal(actor, null, UnitTag.Player, spawnPosition);
        }
        public void CreateMonsterActor()
        {
            CreateEnemyByUnitId(0); // 默认创建，使用unitId=0表示使用默认配置
        }
        
        /// <summary>
        /// 根据UnitConfig ID创建敌人
        /// </summary>
        /// <param name="unitId">单位配置ID</param>
        /// <param name="position">生成位置，如果不指定则使用SceneBehavior的SpawnPoint</param>
        public void CreateEnemyByUnitId(int unitId, Vector2? position = null)
        {
            var actor = new EnemyActor(unitId);
            
            // 如果没有指定位置，使用SceneBehavior的SpawnPoint
            Vector2 spawnPosition = position ?? (SceneBehavior?.SpawnPoint?.position ?? Vector2.zero);
            
            CreateActorInternal(actor, null, UnitTag.Enemy, spawnPosition);
        }
        
        /// <summary>
        /// 创建基地
        /// </summary>
        /// <param name="position">基地位置，如果不指定则使用SceneBehavior的BaseSpawnPoint或路径最后一个点</param>
        public void CreateBase(Vector2? position = null)
        {
            var actor = new BaseActor();
            
            // 获取基地预制体（如果SceneBehavior有的话，否则需要手动创建）
            GameObject go = null;
            if (SceneBehavior.BasePrefab != null)
            {
                go = GameObject.Instantiate(SceneBehavior.BasePrefab);
            }
            else
            {
                // 如果没有预制体，创建一个简单的GameObject
                go = new GameObject("Base");
            }
            
            // 确定基地位置
            Vector2 basePosition = position ?? Vector2.zero;
            if (!position.HasValue)
            {
                if (SceneBehavior?.BaseSpawnPoint != null)
                {
                    basePosition = SceneBehavior.BaseSpawnPoint.position;
                }
                else
                {
                    // 默认位置：路径的最后一个点（往下走的目标）
                    var pathNodes = GetCurentLevelPathNodes();
                    if (pathNodes != null && pathNodes.Count > 0)
                    {
                        basePosition = pathNodes[pathNodes.Count - 1];
                    }
                }
            }
            
            // BaseActor 的 InitializeNumericFromConfig 已经设置了生命值
            CreateActorInternal(actor, go, UnitTag.Base, basePosition);
            
            // 设置基地组件的游戏结束回调
            var baseCampComponent = actor.GetComponent<CampComponent>();
            if (baseCampComponent != null)
            {
                baseCampComponent.OnGameOver = OnBaseDestroyed;
            }
        }
        
        /// <summary>
        /// 基地被摧毁时的回调
        /// </summary>
        private void OnBaseDestroyed()
        {
            Log.Error("基地被摧毁，游戏结束！");
            // 这里可以触发游戏结束事件，比如显示游戏结束UI等
            // 可以通过事件系统通知其他系统
        }

        /// <summary>
        /// 创建塔（通过TowerConfig ID）
        /// </summary>
        /// <param name="towerId">塔配置ID，如果为0则使用默认配置</param>
        /// <param name="position">塔的位置</param>
        public GameActor CreateTower(int towerId = 0, Vector2? position = null)
        {
            var actor = new TowerActor(towerId);
            
            CreateActorInternal(actor, null, UnitTag.Tower, position);
            
            return actor;
        }
        
        /// <summary>
        /// 生成子弹
        /// </summary>
        /// <param name="actorPosition">发射位置</param>
        /// <param name="monsterPosition">目标位置</param>
        /// <param name="bulletId">子弹配置ID，如果为0则使用默认配置</param>
        /// <param name="attackerNumeric">攻击者的数值组件（用于传递攻击力等信息）</param>
        /// <param name="attackerActor">攻击者Actor（用于日志）</param>
        /// <param name="targetActor">目标Actor（用于日志）</param>
        public void SpawnBullet(Vector2 actorPosition, Vector2 monsterPosition, int bulletId = 0, NumericComponent attackerNumeric = null, GameActor attackerActor = null, GameActor targetActor = null)
        {
            var actor = new BulletActor(bulletId, monsterPosition);
            
            // 设置真正的攻击者（发射子弹的单位）
            actor.RealAttacker = attackerActor;
            
            CreateActorInternal(actor, null, UnitTag.Bullet, actorPosition);
            
            // 如果提供了攻击者的数值组件，复制攻击力等关键属性到子弹
            if (attackerNumeric != null)
            {
                var bulletNumeric = actor.GetComponent<NumericComponent>();
                if (bulletNumeric != null)
                {
                    // 复制攻击力
                    int attack = attackerNumeric.Get<int>(NumericType.Attack);
                    bulletNumeric.Set(NumericType.AttackBase, attack);
                    
                    // 复制其他可能需要的关键属性（如元素伤害等）
                    // 可以根据需要添加更多属性
                }
            }
            
            // 打印详细的生成日志
            string attackerName = GetActorDisplayNameForLog(attackerActor);
            string bulletName = GetActorDisplayNameForLog(actor);
            string targetName = targetActor != null ? GetActorDisplayNameForLog(targetActor) : $"位置({monsterPosition.x:F1}, {monsterPosition.y:F1})";
            
            Log.Info($"[子弹生成] {attackerName} → 发射 {bulletName} → 目标: {targetName}");
        }
        
        /// <summary>
        /// 获取Actor的显示名称（用于日志），包括GameObject名字和配置名字
        /// </summary>
        private string GetActorDisplayNameForLog(GameActor actor)
        {
            if (actor == null)
            {
                return "未知";
            }
            
            string goName = actor.m_Owner != null ? actor.m_Owner.name : "无GameObject";
            string configName = GetTypeName(actor, actor.Tag);
            
            return $"{goName}({configName})";
        }


        /// <summary>
        /// 基础创建Actor方法（统一入口）
        /// </summary>
        private void CreateActorInternal(GameActor actor, GameObject go, UnitTag tag, Vector2? position = null)
        {
            // 先设置位置（在 OnInit 之前，这样 BulletCmp.OnInit 可以正确读取初始位置）
            if (position.HasValue)
            {
                actor.SetPosition(position.Value);
            }
            
            AddActor(actor, go, tag);
            
            // 再次设置位置（确保 Transform 和 Position 同步，因为 CreateModel 可能创建了新的 GameObject）
            if (position.HasValue && actor.Transform != null)
            {
                actor.SetPosition(position.Value);
                
                actor.Transform.position = position.Value;
                // 确保初始角度为0（不旋转）
                actor.Transform.rotation = Quaternion.identity;
            }
        }

        public void AddActor(GameActor actor, GameObject go, UnitTag tag)
        {
            // 如果传入了GameObject，使用它；否则让CreateModel创建
            if (go != null)
            {
                go.SetActive(true);
                actor.BindGo(go);
                // 设置层级和名称
                SetupActorGameObject(actor, go, tag);
            }
            
            // 初始化Actor（这会调用InitConfig、CreateModel、BindCmp，并添加所有组件）
            actor.OnInit();
            
            // 如果CreateModel创建了GameObject，确保它已设置层级
            if (go == null && actor.Transform != null)
            {
                // CreateModel已经创建并绑定了GameObject
                actor.m_Owner.SetActive(true);
                // 设置层级和名称
                SetupActorGameObject(actor, actor.m_Owner, tag);
            }
            else if (go == null && actor.Transform == null)
            {
                // 如果没有GameObject也没有创建模型，创建一个默认的
                Log.Warning($"AddActor: Actor没有GameObject，创建一个默认的");
                var defaultGo = new GameObject($"Actor_{tag}");
                actor.BindGo(defaultGo);
                // 设置层级和名称
                SetupActorGameObject(actor, defaultGo, tag);
                defaultGo.SetActive(true);
            }
            
            actor.SetTag(tag);
            Actors.Add(actor);
            
            // 自动添加ActorDebugComponent（如果GameObject上还没有）
            if (actor.m_Owner != null)
            {
                var debugComp = actor.m_Owner.GetComponent<ActorDebugComponent>();
                if (debugComp == null)
                {
                    debugComp = actor.m_Owner.AddComponent<ActorDebugComponent>();
                }
            }
        }
        
        public override bool OnInit()
        {
            return true;
        }

        /// <summary>
        /// 尝试获取指定类型的单位（通用方法）
        /// </summary>
        /// <param name="unitType">单位类型</param>
        /// <param name="position">查找位置</param>
        /// <param name="radius">查找半径，如果为0或负数则不限制距离</param>
        /// <param name="actor">找到的Actor</param>
        /// <returns>是否找到</returns>
        public bool TryGetUnit(EUnitType unitType, Vector2 position, float radius, out GameActor actor)
        {
            actor = null;
            UnitTag targetTag = UnitTag.Enemy; // 默认值
            
            // 将EUnitType转换为UnitTag
            switch (unitType)
            {
                case EUnitType.HERO:
                    targetTag = UnitTag.Player;
                    break;
                case EUnitType.ENEMY:
                    targetTag = UnitTag.Enemy;
                    break;
                case EUnitType.TOWER:
                    targetTag = UnitTag.Tower;
                    break;
                case EUnitType.Base:
                    targetTag = UnitTag.Base;
                    break;
            }
            
            foreach (var gameActor in Actors)
            {
                if (gameActor.Tag == targetTag && !gameActor.IsDestroyed)
                {
                    // 如果radius <= 0，不限制距离，直接返回第一个匹配的
                    if (radius <= 0 || Vector2.Distance(position, gameActor.Position) < radius)
                    {
                        actor = gameActor;
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 尝试获取玩家（在指定位置和范围内）
        /// </summary>
        public bool TryGetPlayer(Vector2 position, float radius, out GameActor actor)
        {
            return TryGetUnit(EUnitType.HERO, position, radius, out actor);
        }
        
        /// <summary>
        /// 尝试获取敌人（在指定位置和范围内）
        /// </summary>
        public bool TryGetEnemy(Vector2 position, float radius, out GameActor actor)
        {
            return TryGetUnit(EUnitType.ENEMY, position, radius, out actor);
        }
        
        /// <summary>
        /// 尝试获取基地
        /// </summary>
        public bool TryGetBase(out GameActor actor)
        {
            return TryGetUnit(EUnitType.Base, Vector2.zero, 0, out actor);
        }
        
        /// <summary>
        /// 尝试获取怪物（兼容旧方法）
        /// </summary>
        [System.Obsolete("使用 TryGetEnemy 代替")]
        public bool TryGetMonster(Vector2 position, float radius, out GameActor actor)
        {
            return TryGetEnemy(position, radius, out actor);
        }
        
        /// <summary>
        /// 获取扇形范围内的所有敌人
        /// </summary>
        /// <param name="center">扇形中心位置</param>
        /// <param name="direction">扇形方向（归一化向量）</param>
        /// <param name="radius">扇形半径</param>
        /// <param name="angle">扇形角度（度）</param>
        /// <returns>在扇形范围内的敌人列表</returns>
        public List<GameActor> GetMonstersInSector(Vector2 center, Vector2 direction, float radius, float angle)
        {
            List<GameActor> result = new List<GameActor>();
            
            // 计算扇形的半角（度转弧度）
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;
            
            // 归一化方向向量
            Vector2 normalizedDir = direction.normalized;
            
            // 遍历所有敌人
            foreach (var actor in Actors)
            {
                if (actor.Tag != UnitTag.Enemy || actor.IsDestroyed)
                    continue;
                
                Vector2 toEnemy = actor.Position - center;
                float distance = toEnemy.magnitude;
                
                // 检查距离
                if (distance > radius || distance < 0.01f)
                    continue;
                
                // 归一化到敌人的方向
                Vector2 toEnemyNormalized = toEnemy.normalized;
                
                // 计算方向向量与到敌人向量的点积（用于计算角度）
                float dot = Vector2.Dot(normalizedDir, toEnemyNormalized);
                
                // 使用点积计算角度（acos返回0到π之间的角度）
                float angleToEnemy = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f));
                
                // 检查是否在扇形角度范围内
                if (angleToEnemy <= halfAngle)
                {
                    result.Add(actor);
                }
            }
            
            return result;
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();

            for (int i = 0; i < Actors.Count; i++)
            {
                var gameActor = Actors[i];
                if (gameActor.IsDestroyed)
                {
                    RemoveActor(gameActor);
                    continue;
                }
                gameActor.OnUpdate();
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            foreach (var gameActor in Actors)
            {
                gameActor.OnDestroy();
            }
        }
    }
}

