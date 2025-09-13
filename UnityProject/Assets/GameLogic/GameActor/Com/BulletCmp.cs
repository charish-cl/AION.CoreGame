

using UnityEngine;

namespace GameLogic
{
    public class BulletCmp : MoveLogicCmp
    {

        public Vector2 m_target;
        
        public void Init(Vector2 target)
        {
            m_target = target;
            Velocity = 5;
        }
        public override void OnUpdate()
        {
            Move(m_target - Position);

            if (SceneMgr.Instance.TryGetMonster(Position,1 ,out var monster))
            {
                Enable = false;
                Actor.Destroy();
            }
        }
    }
}