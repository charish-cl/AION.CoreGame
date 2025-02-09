using System;
using GameDevKit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic
{
    public class MapConfig:MonoSingleton<MapConfig>
    {
        
        //敌人
        
        [LabelText("敌人坦克")]
        public TankInfo EnermyTank;
        [LabelText("玩家坦克预制体")]
        public TankInfo  PlayerTank;


        
              
        [LabelText("子弹预制体")]
        public GameObject BulletPrefab;
        [LabelText("敌人坦克数量")]
        public int EnemyTankCount;
  
        
        
        [Serializable]
        public class TankInfo
        {
            [LabelText("坦克名称")]
            public string Name;
            
            [LabelText("坦克预制体")]
            public GameObject TankPrefab;
            
            [LabelText("坦克初始位置")]
            public Transform SpawnPoint;
            
            [LabelText("子弹速度")]
            public float BulletSpeed = 10f;
            
            [LabelText("子弹射程")]
            public float BulletRange = 100f;
            
            [LabelText("坦克生命值")]
            public int HP = 100;
            
            [LabelText("坦克攻击力")]
            public int Attack = 10;
            
            [LabelText("坦克防御力")]
            public int Defense = 10;
            
            [LabelText("坦克速度")]
            public float Speed = 10f;
            
        }
        
    }
}