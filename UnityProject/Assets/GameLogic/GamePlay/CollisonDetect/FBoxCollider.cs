using System;
using UnityEngine;

namespace GameLogic
{
    public class FBoxCollider : FCollider
    {
        public Vector2 size = Vector2.one;
        public Vector2 center => transform.position;
        public Vector2 min => center - size / 2;
        public Vector2 max => center + size / 2;

        public override Rect GetAABB()
        {
            return new Rect(min.x, min.y, size.x, size.y);
        }
        
    }
}