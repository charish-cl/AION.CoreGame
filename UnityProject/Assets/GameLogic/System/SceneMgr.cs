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

        public void CreatePlayer()
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
            
            GameObject go = GameObject.Instantiate(SceneBehavior.PlayerPrefab, SceneBehavior.transform);
            go.transform.position = SceneBehavior.SpawnPoint.position;
            
            AddActor(actor, go, UnitTag.Player, (numeric) =>
            {
                numeric.Set(NumericType.SpeedBase, 5f);
            });    
        }
        public void CreateMonsterActor()
        {
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<SimplePathFindingLogicCmp>();
            actor.AddComponent<MoveViewCmp>();
            actor.AddComponent<DirectionViewCmp>();
            actor.AddComponent<BuffCmp>();
            actor.AddComponent<HealthCmp>();
            actor.AddComponent<HPBarCmp>();
            
            
            GameObject go = GameObject.Instantiate(SceneBehavior.MonsterPrefab, SceneBehavior.transform);
            go.transform.position = SceneBehavior.SpawnPoint.position;
            
            AddActor(actor, go, UnitTag.Enemy);
        }

        public void CreateTower()
        {
            var actor = new GameActor();
            actor.AddComponent<NumericComponent>();
            actor.AddComponent<TowerFSMCmp>();
            actor.AddComponent<OrientationViewCmp>();
            
            GameObject go = GameObject.Instantiate(SceneBehavior.TowerPrefab, SceneBehavior.transform);
            // go.transform.position = SceneBehavior.SpawnPoint.position;
         
            AddActor(actor, go, UnitTag.Tower);
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
            go.SetActive(true);
            actor.BindGo(go);
            actor.OnInit();
            actor.SetTag(tag);
            Actors.Add(actor);
        }
        public override bool OnInit()
        {
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