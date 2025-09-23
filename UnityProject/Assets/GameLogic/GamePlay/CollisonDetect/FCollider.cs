using UnityEngine;

namespace GameLogic
{
    public abstract class FCollider : MonoBehaviour
    {
        [HideInInspector] public bool isColliding;
        public abstract Rect GetAABB();
    }

}