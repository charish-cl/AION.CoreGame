using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic
{
    public class SceneBehavior : MonoBehaviour
    {
        [LabelText("出怪点")]
        public Transform SpawnPoint;
        
        [LabelText("路径")]
        public Transform PathStart;
        
        [LabelText("HP条")]
        public Canvas HPBarCanvas;
        
        [LabelText("怪物预制体")]
        public GameObject MonsterPrefab;

        [LabelText("塔预制体")]
        public GameObject TowerPrefab;
        
        [LabelText("玩家预制体")]
        public GameObject PlayerPrefab;

        [LabelText("子弹预制体")]
        public GameObject BulletPrefab;
        
        [LabelText("基地预制体")]
        public GameObject BasePrefab;
        
        [LabelText("基地生成点")]
        public Transform BaseSpawnPoint;

        [LabelText("伤害预制体")] public DamageNumber numberPrefab;

        public Vector2[] GetPath()
        {
            Vector2[] path = new Vector2[PathStart.childCount];
            for (int i = 0; i < PathStart.childCount; i++)
            {
                path[i] = PathStart.GetChild(i).position;
            }    
            return path;
        }
    }
}