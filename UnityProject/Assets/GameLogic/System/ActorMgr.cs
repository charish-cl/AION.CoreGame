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
            
            // 查找 GameEntry/Root/Actor 路径
            GameObject gameEntry = GameObject.Find("GameEntry");
            if (gameEntry == null)
            {
                throw new Exception("ActorMgr: 未找到GameEntry，创建默认根节点");
          
                
            }
            
            Transform root = gameEntry.transform.Find("Root");
            if (root == null)
            {
                throw new Exception("ActorMgr: 未找到Root，创建默认根节点");
            }
            
            Transform actor = root.Find("Actor");
            if (actor == null)
            {
                Log.Info("ActorMgr: 未找到Actor，创建Actor节点");
                actor = new GameObject("Actor").transform;
                actor.SetParent(root);
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
            
            var unitCmp = actor.GetComponent<UnitComponent>();
            if (unitCmp != null && unitCmp.IsConfigValid)
            {
                configName = unitCmp.Name;
                configId = unitCmp.Id;
            }
            else
            {
                var towerCmp = actor.GetComponent<TowerComponent>();
                if (towerCmp != null && towerCmp.IsConfigValid)
                {
                    configName = towerCmp.Name;
                    configId = towerCmp.Id;
                }
                else
                {
                    var bulletCmp = actor.GetComponent<BulletComponent>();
                    if (bulletCmp != null && bulletCmp.IsConfigValid)
                    {
                        configName = bulletCmp.Name;
                        configId = bulletCmp.Id;
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
            var unitCmp = actor.GetComponent<UnitComponent>();
            if (unitCmp != null && unitCmp.IsConfigValid)
            {
                return unitCmp.Id;
            }
            
            var towerCmp = actor.GetComponent<TowerComponent>();
            if (towerCmp != null && towerCmp.IsConfigValid)
            {
                return towerCmp.Id;
            }
            
            var bulletCmp = actor.GetComponent<BulletComponent>();
            if (bulletCmp != null && bulletCmp.IsConfigValid)
            {
                return bulletCmp.Id;
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
            
            // 设置GameObject名称：配置ID_唯一ID
            go.name = $"{configId}_{uniqueId}";
            
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
        public void CreatePlayer(int unitId = 0)
        {
            var actor = new PlayerActor(unitId);
            
            // 设置生成位置
            Vector2 spawnPosition = Vector2.zero;
            if (SceneBehavior != null && SceneBehavior.SpawnPoint != null)
            {
                spawnPosition = SceneBehavior.SpawnPoint.position;
            }
            
            AddActor(actor, null, UnitTag.Player, (numeric) =>
            {
                // 优先从UnitConfig加载数值
                InitializeNumericFromUnitConfig(actor, numeric);
                
                // 如果没有配置，使用默认值
                if (actor.GetComponent<UnitComponent>() == null || !actor.GetComponent<UnitComponent>().IsConfigValid)
                {
                    numeric.Set(NumericType.SpeedBase, 5f);
                }
            });    
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.Transform != null)
            {
                actor.SetPosition(spawnPosition);
            }
        }
        public void CreateMonsterActor()
        {
            CreateEnemyByUnitId(0); // 默认创建，使用unitId=0表示使用默认配置
        }
        
        /// <summary>
        /// 根据UnitConfig ID创建敌人
        /// </summary>
        /// <param name="unitId">单位配置ID</param>
        public void CreateEnemyByUnitId(int unitId)
        {
            var actor = new EnemyActor(unitId);
            
            // 设置生成位置
            Vector2 spawnPosition = Vector2.zero;
            if (SceneBehavior != null && SceneBehavior.SpawnPoint != null)
            {
                spawnPosition = SceneBehavior.SpawnPoint.position;
            }
            
            AddActor(actor, null, UnitTag.Enemy, (numeric) =>
            {
                // 从UnitConfig初始化数值
                InitializeNumericFromUnitConfig(actor, numeric);
            });
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.Transform != null)
            {
                actor.SetPosition(spawnPosition);
            }
        }
        
        /// <summary>
        /// 创建基地
        /// </summary>
        public void CreateBase()
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
            
            // 设置基地位置（可以在SceneBehavior中配置，或者使用默认位置）
            if (SceneBehavior.BaseSpawnPoint != null)
            {
                go.transform.position = SceneBehavior.BaseSpawnPoint.position;
            }
            else
            {
                // 默认位置：路径的最后一个点（往下走的目标）
                var pathNodes = GetCurentLevelPathNodes();
                if (pathNodes != null && pathNodes.Count > 0)
                {
                    go.transform.position = pathNodes[pathNodes.Count - 1];
                }
            }
            
            AddActor(actor, go, UnitTag.Base, (numeric) =>
            {
                // 基地有更高的生命值
                numeric.Set(NumericType.MaxHpBase, 1000);
                numeric.Set(NumericType.HpBase, 1000);
            });
            
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
        /// <param name="position">塔的位置，如果不指定则使用默认位置</param>
        public void CreateTower(int towerId = 0, Vector2? position = null)
        {
            var actor = new TowerActor(towerId);
         
            AddActor(actor, null, UnitTag.Tower);
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.Transform != null)
            {
                if (position.HasValue)
                {
                    actor.SetPosition(position.Value);
                }
                // 如果没有指定位置，可以保持默认位置或从配置读取
            }
        }
        
        /// <summary>
        /// 生成子弹
        /// </summary>
        /// <param name="actorPosition">发射位置</param>
        /// <param name="monsterPosition">目标位置</param>
        /// <param name="bulletId">子弹配置ID，如果为0则使用默认配置</param>
        public void SpawnBullet(Vector2 actorPosition, Vector2 monsterPosition, int bulletId = 0)
        {
            var actor = new BulletActor(bulletId, monsterPosition);
            
            AddActor(actor, null, UnitTag.Bullet);
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.Transform != null)
            {
                actor.SetPosition(actorPosition);
            }
        }

        /// <summary>
        /// 设置基础数值（默认值）
        /// </summary>
        public void SetBaseValue(NumericComponent NumericDic)
        {
            NumericDic.Set(NumericType.SpeedBase, 1.0f);
            NumericDic.Set(NumericType.AttackSpeedBase, 0.3f);
            NumericDic.Set(NumericType.HpBase, 100);
            NumericDic.Set(NumericType.AttackBase, 20);
            NumericDic.Set(NumericType.DefenseBase, 5);
        }
        
        /// <summary>
        /// 从UnitConfig初始化数值组件
        /// </summary>
        private void InitializeNumericFromUnitConfig(GameActor actor, NumericComponent numeric)
        {
            var unitCmp = actor.GetComponent<UnitComponent>();
            if (unitCmp != null && unitCmp.IsConfigValid)
            {
                var config = unitCmp.Config;
                if (config != null)
                {
                    numeric.Set(NumericType.MaxHpBase, config.MaxHp);
                    numeric.Set(NumericType.HpBase, config.MaxHp);
                    numeric.Set(NumericType.AttackBase, config.Attack);
                    numeric.Set(NumericType.DefenseBase, config.Defense);
                    numeric.Set(NumericType.SpeedBase, config.MoveSpeed);
                    numeric.Set(NumericType.AttackSpeedBase, config.AttackInterval);
                }
            }
        }

        public void AddActor(GameActor actor, GameObject go, UnitTag tag, Action<NumericComponent> onInit = null)
        {
            // 如果传入了GameObject，使用它；否则让ModelComponent创建
            if (go != null)
            {
                go.SetActive(true);
                actor.BindGo(go);
                // 设置层级和名称
                SetupActorGameObject(actor, go, tag);
            }
            
            // 初始化Actor（这会调用InitConfig、BindCmp，并添加所有组件）
            actor.OnInit();
            
            // 在OnInit之后，组件已经被添加，可以安全地获取NumericComponent
            var numericComponent = actor.GetComponent<NumericComponent>();
            if (numericComponent != null)
            {
                SetBaseValue(numericComponent);
                
                if (onInit != null)
                {
                    onInit(numericComponent);    
                }
            }
            else
            {
                Log.Warning($"AddActor: Actor没有NumericComponent，无法设置基础数值");
            }
            
            // 如果ModelComponent创建了GameObject，确保它已绑定并设置层级
            var modelComponent = actor.GetComponent<ModelComponent>();
            if (modelComponent != null && modelComponent.ModelInstance != null && go == null)
            {
                // ModelComponent已经创建并绑定了GameObject
                modelComponent.ModelInstance.SetActive(true);
                // 设置层级和名称
                SetupActorGameObject(actor, modelComponent.ModelInstance, tag);
            }
            else if (go == null && actor.Transform == null)
            {
                // 如果没有GameObject也没有ModelComponent，创建一个默认的
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

        public bool TryGetMonster(Vector2 position, float radius, out GameActor actor)
        {
            actor = null;
            foreach (var gameActor in Actors)
            {
                if (gameActor.Tag == UnitTag.Enemy && Vector2.Distance(position, gameActor.Position) < radius)
                {
                    actor = gameActor;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 尝试获取基地
        /// </summary>
        public bool TryGetBase(out GameActor actor)
        {
            actor = null;
            foreach (var gameActor in Actors)
            {
                if (gameActor.Tag == UnitTag.Base && !gameActor.IsDestroyed)
                {
                    actor = gameActor;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 尝试获取敌人（在指定位置和范围内）
        /// </summary>
        public bool TryGetEnemy(Vector2 position, float radius, out GameActor actor)
        {
            actor = null;
            foreach (var gameActor in Actors)
            {
                if (gameActor.Tag == UnitTag.Enemy && !gameActor.IsDestroyed && 
                    Vector2.Distance(position, gameActor.Position) < radius)
                {
                    actor = gameActor;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 尝试获取玩家（在指定位置和范围内）
        /// </summary>
        public bool TryGetPlayer(Vector2 position, float radius, out GameActor actor)
        {
            actor = null;
            foreach (var gameActor in Actors)
            {
                if (gameActor.Tag == UnitTag.Player && !gameActor.IsDestroyed && 
                    Vector2.Distance(position, gameActor.Position) < radius)
                {
                    actor = gameActor;
                    return true;
                }
            }
            return false;
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

