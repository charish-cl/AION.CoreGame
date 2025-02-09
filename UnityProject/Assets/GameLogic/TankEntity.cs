using UnityEngine;

namespace GameLogic
{
    public class TankEntity: MonoBehaviour
    {
        public int Health { get; set; }
        
        public float Speed { get; set; }
        
        public int Damage { get; set; }


        public void Init(MapConfig.TankInfo tankInfo)
        {
            Speed = tankInfo.Speed;
        }
        
    }
}