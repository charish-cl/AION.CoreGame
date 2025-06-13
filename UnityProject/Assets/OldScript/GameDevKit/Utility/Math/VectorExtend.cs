
using UnityEngine;

namespace GameDevKit.Utility
{
    public static class VectorExtend
    {
        //Vector转Vector3,并且设置z为0,用于2D,扩展方法
        public static Vector3 XYZ(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y, 0);
        }
        public static Vector2 XY(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
        
    }
}