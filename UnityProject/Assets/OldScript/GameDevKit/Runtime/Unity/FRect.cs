using System;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct FRect
    {
        public Vector2 center;
        public float width;
        public float height;

        public FRect(Vector2 center, float width, float height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
        }
    }
}