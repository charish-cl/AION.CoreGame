using System;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    public class TestProcedure:MonoBehaviour
    {
        private void Start()
        {
            GameApp.Instance.Entrance();
            
            GameModule.UI.ShowWindow<BattleMainUI>();

            StartGame();
        }
        public  void StartGame()
        {
            if (ActorMgr.Instance == null)
            {
                Log.Error("TestProcedure: ActorMgr未初始化");
                return;
            }
            
            Log.Info("TestProcedure: 开始创建测试单位");
            
            // 创建基地
            if (ActorMgr.Instance.TryGetBase(out var baseActor))
            {
                Log.Info("TestProcedure: 基地已存在");
            }
            else
            {
                ActorMgr.Instance.CreateBase();
                Log.Info("TestProcedure: 创建基地");
            }
            
            // 创建英雄（使用默认配置，设置在屏幕中心附近）
            // Vector2 playerPos = new Vector2(10, 10);
            // ActorMgr.Instance.CreatePlayer(1, playerPos);
            // Log.Info($"TestProcedure: 创建英雄，位置: {playerPos}");
            //
            // 创建塔（在左边，像回合制游戏）
            Vector2 towerPos = new Vector2(5, 10);
            ActorMgr.Instance.CreateTower(1, towerPos);
            Log.Info($"TestProcedure: 创建塔，位置: {towerPos}");
            
            // 创建敌人（在右边，像回合制游戏）
            Vector2 enemyPos = new Vector2(15, 10);
            ActorMgr.Instance.CreateEnemyByUnitId(1, enemyPos);
            Log.Info($"TestProcedure: 创建敌人，位置: {enemyPos}");
            
            // 禁用所有单位的FSM组件（用于测试，使用ActorTestTool直接控制）
            DisableAllFSMComponents();
            
            // 禁用敌人的移动组件（让敌人不动）
            DisableEnemyMoveComponents();
            
            Log.Info("TestProcedure: 测试单位创建完成");
        }
        
        /// <summary>
        /// 禁用所有Actor的FSM组件
        /// </summary>
        private void DisableAllFSMComponents()
        {
            if (ActorMgr.Instance == null || ActorMgr.Instance.Actors == null)
            {
                return;
            }
            
            foreach (var actor in ActorMgr.Instance.Actors)
            {
                if (actor == null || actor.IsDestroyed)
                    continue;
                
                // 禁用UnitFSMCmp
                var unitFSM = actor.GetComponent<UnitFSMCmp>();
                if (unitFSM != null)
                {
                    unitFSM.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 UnitFSMCmp");
                }
                
                // 禁用MonsterFSMCmp
                var monsterFSM = actor.GetComponent<MonsterFSMCmp>();
                if (monsterFSM != null)
                {
                    monsterFSM.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 MonsterFSMCmp");
                }
                
                // 禁用TowerFSMCmp
                var towerFSM = actor.GetComponent<TowerFSMCmp>();
                if (towerFSM != null)
                {
                    towerFSM.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 TowerFSMCmp");
                }
            }
        }
        
        /// <summary>
        /// 禁用敌人的移动组件（让敌人不动）
        /// </summary>
        private void DisableEnemyMoveComponents()
        {
            if (ActorMgr.Instance == null || ActorMgr.Instance.Actors == null)
            {
                return;
            }
            
            foreach (var actor in ActorMgr.Instance.Actors)
            {
                if (actor == null || actor.IsDestroyed)
                    continue;
                
                // 只禁用敌人的移动组件
                if (actor.Tag != UnitTag.Enemy)
                    continue;
                
                // 禁用MoveLogicCmp
                var moveLogic = actor.GetComponent<MoveLogicCmp>();
                if (moveLogic != null)
                {
                    moveLogic.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 MoveLogicCmp");
                }
                
                // 禁用SimplePathFindingLogicCmp
                var pathFinding = actor.GetComponent<SimplePathFindingLogicCmp>();
                if (pathFinding != null)
                {
                    pathFinding.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 SimplePathFindingLogicCmp");
                }
                
                // 禁用MoveViewCmp
                var moveView = actor.GetComponent<MoveViewCmp>();
                if (moveView != null)
                {
                    moveView.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 MoveViewCmp");
                }
                
                // 禁用DirectionViewCmp
                var directionView = actor.GetComponent<DirectionViewCmp>();
                if (directionView != null)
                {
                    directionView.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 DirectionViewCmp");
                }
                
                // 禁用OrientationViewCmp
                var orientationView = actor.GetComponent<OrientationViewCmp>();
                if (orientationView != null)
                {
                    orientationView.Enable = false;
                    Log.Info($"TestProcedure: 禁用 {actor.Tag} 的 OrientationViewCmp");
                }
            }
        }
    }
}