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
            
            // 创建英雄（使用默认配置）
            ActorMgr.Instance.CreatePlayer(1);
            Log.Info("TestProcedure: 创建英雄");
            
            // 创建敌人（使用默认配置）
            ActorMgr.Instance.CreateEnemyByUnitId(1);
            Log.Info("TestProcedure: 创建敌人");
            
            // 创建塔（使用默认配置）
            Vector2 towerPos = new Vector2(0, -2);
            ActorMgr.Instance.CreateTower(1, towerPos);
            Log.Info("TestProcedure: 创建塔");
            
            // 禁用所有单位的FSM组件（用于测试，使用TestControlCmp直接控制）
            DisableAllFSMComponents();
            
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
    }
}