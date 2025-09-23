using UnityEngine;

namespace GameLogic
{
    public class FCircleollider : FCollider
    {
        public float radius = 1;
        public Vector2 center => transform.position;

        public override Rect GetAABB()
        {
            return new Rect(
                center.x - radius,
                center.y - radius,
                radius * 2,
                radius * 2
            );
        }
    }
}