using System;
using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    public class SceneMgr :BaseLogicSys<SceneMgr>
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
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<MoveLogicCmp>();
            actor.AddComponent<DirectionViewCmp>();
            actor.AddComponent<BuffCmp>();
            actor.AddComponent<HealthCmp>();
            actor.AddComponent<InputLogicCmp>();
            actor.AddComponent<MoveViewCmp>();
            actor.AddComponent<ActorAnimViewCmp>();
            actor.AddComponent<UnitFSMCmp>(); // 添加Hero状态机
            actor.AddComponent<OrientationViewCmp>(); // 添加朝向组件用于攻击
            
            // 如果指定了unitId，添加UnitComponent和ModelComponent
            if (unitId > 0)
            {
                var unitComponent = actor.AddComponent<UnitComponent>();
                unitComponent.Init(unitId);
            }
            
            // 添加ModelComponent，它会自动从UnitComponent加载模型配置
            actor.AddComponent<ModelComponent>();
            
            // 设置生成位置
            Vector2 spawnPosition = Vector2.zero;
            if (SceneBehavior != null && SceneBehavior.SpawnPoint != null)
            {
                spawnPosition = SceneBehavior.SpawnPoint.position;
            }
            
            AddActor(actor, null, UnitTag.Player, (numeric) =>
            {
                numeric.Set(NumericType.SpeedBase, 5f);
                
                // 如果通过UnitComponent加载了配置，使用配置中的数值
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
            });
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.m_transform != null)
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
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<SimplePathFindingLogicCmp>();
            actor.AddComponent<MoveViewCmp>();
            actor.AddComponent<DirectionViewCmp>();
            actor.AddComponent<BuffCmp>();
            actor.AddComponent<HealthCmp>();
            actor.AddComponent<HPBarCmp>();
            actor.AddComponent<MonsterFSMCmp>(); // 添加Monster状态机
            actor.AddComponent<OrientationViewCmp>(); // 添加朝向组件用于攻击
            
            // 如果指定了unitId，添加UnitComponent并初始化
            if (unitId > 0)
            {
                var unitComponent = actor.AddComponent<UnitComponent>();
                unitComponent.Init(unitId);
            }
            
            // 添加ModelComponent，它会自动从UnitComponent加载模型配置
            actor.AddComponent<ModelComponent>();
            
            // 设置生成位置
            Vector2 spawnPosition = Vector2.zero;
            if (SceneBehavior != null && SceneBehavior.SpawnPoint != null)
            {
                spawnPosition = SceneBehavior.SpawnPoint.position;
            }
            
            AddActor(actor, null, UnitTag.Enemy, (numeric) =>
            {
                // 如果通过UnitComponent加载了配置，使用配置中的数值
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
            });
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.m_transform != null)
            {
                actor.SetPosition(spawnPosition);
            }
        }
        
        /// <summary>
        /// 创建基地
        /// </summary>
        public void CreateBase()
        {
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<BuffCmp>();
            actor.AddComponent<HealthCmp>();
            actor.AddComponent<CampComponent>(); // 添加基地组件
            
            // 获取基地预制体（如果SceneBehavior有的话，否则需要手动创建）
            GameObject go = null;
            if (SceneBehavior.BasePrefab != null)
            {
                go = GameObject.Instantiate(SceneBehavior.BasePrefab, SceneBehavior.transform);
            }
            else
            {
                // 如果没有预制体，创建一个简单的GameObject
                go = new GameObject("Base");
                go.transform.SetParent(SceneBehavior.transform);
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
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<TowerFSMCmp>();
            actor.AddComponent<OrientationViewCmp>();
            
            // 如果指定了towerId，添加TowerComponent并初始化
            if (towerId > 0)
            {
                var towerComponent = actor.AddComponent<TowerComponent>();
                towerComponent.Init(towerId);
            }
            
            // 添加ModelComponent，它会自动从TowerComponent加载模型配置
            actor.AddComponent<ModelComponent>();
            
            AddActor(actor, null, UnitTag.Tower);
            
            // 设置位置（在ModelComponent创建GameObject之后）
            if (actor.m_transform != null)
            {
                if (position.HasValue)
                {
                    actor.SetPosition(position.Value);
                }
                // 如果没有指定位置，可以保持默认位置或从配置读取
            }
        }
        
        public void SpawnBullet(Vector2 actorPosition, Vector2 monsterPosition)
        {
            
            GameObject go = GameObject.Instantiate(SceneBehavior.BulletPrefab, SceneBehavior.transform);
            go.transform.position = actorPosition;
            
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<BulletCmp>().Init(monsterPosition);
            actor.AddComponent<MoveViewCmp>();
            actor.AddComponent<OrientationViewCmp>().SetTarget(monsterPosition);
            
            
            AddActor(actor, go, UnitTag.Bullet);
        }

        public void SetBaseValue(NumericComponent NumericDic)
        {
            NumericDic.Set(NumericType.SpeedBase, 1.0f);
            NumericDic.Set(NumericType.AttackSpeedBase, 0.3f);
            NumericDic.Set(NumericType.HpBase, 100);
            NumericDic.Set(NumericType.AttackBase, 20);
            NumericDic.Set(NumericType.DefenseBase, 5);
        }

        public void AddActor(GameActor actor, GameObject go, UnitTag tag,Action<NumericComponent> onInit = null)
        {
            SetBaseValue(actor.GetComponent<NumericComponent>());
            
            if (onInit!= null)
            {
                onInit(actor.GetComponent<NumericComponent>());    
            }
            
            // 如果传入了GameObject，使用它；否则让ModelComponent创建
            if (go != null)
            {
                go.SetActive(true);
                actor.BindGo(go);
            }
            
            // 初始化Actor（ModelComponent会在OnInit时创建GameObject）
            actor.OnInit();
            
            // 如果ModelComponent创建了GameObject，确保它已绑定
            var modelComponent = actor.GetComponent<ModelComponent>();
            if (modelComponent != null && modelComponent.ModelInstance != null && go == null)
            {
                // ModelComponent已经创建并绑定了GameObject
                modelComponent.ModelInstance.SetActive(true);
            }
            else if (go == null && actor.m_transform == null)
            {
                // 如果没有GameObject也没有ModelComponent，创建一个默认的
                Log.Warning($"AddActor: Actor没有GameObject，创建一个默认的");
                var defaultGo = new GameObject($"Actor_{tag}");
                if (SceneBehavior != null)
                {
                    defaultGo.transform.SetParent(SceneBehavior.transform);
                }
                defaultGo.SetActive(true);
                actor.BindGo(defaultGo);
            }
            
            actor.SetTag(tag);
            Actors.Add(actor);
        }
        public override bool OnInit()
        {
            CreateBase(); // 先创建基地
            
            CreatePlayer();
            
            CreateMonsterActor();
            
            CreateTower();
            
            return true;
        }

        public bool TryGetMonster(Vector2 position, float radius,out GameActor actor)
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