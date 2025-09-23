

using UnityEngine;

namespace GameLogic
{
    public class BulletCmp : MoveLogicCmp
    {

        public Vector2 m_target;
        
        public void Init(Vector2 target)
        {
            m_target = target;
        }

        public override void OnInit()
        {
            base.OnInit();
            Velocity = 5;
        }

        public override void OnUpdate()
        {
            Move(m_target - Position);

            if (SceneMgr.Instance.TryGetMonster(Position,1 ,out var monster))
            {
                Enable = false;

                var healthCmp = monster.GetComponent<HealthCmp>();
                if (healthCmp != null)
                {
                    healthCmp.TakeDamage(GetComponent<NumericComponent>());
                }
                
                Actor.Destroy();
            }
        }
    }
}