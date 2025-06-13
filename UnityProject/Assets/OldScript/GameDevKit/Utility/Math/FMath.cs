using UnityEngine;

namespace GameDevKit.Utility
{
    public static class FMath
    {
        /// <summary>
        /// 计算两物体之间角度
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float CulclateAngle(Vector2 loc,Vector2 target)
        {
            Vector2 direction =target -loc;
             float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
             return angle;
        }
        /// <summary>
        /// 需要-180~180度的夹角
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
       public static float VectorAngle(Vector2 from, Vector2 to)
       {
           float angle;
          
           Vector3 cross=Vector3.Cross(from, to);
           angle = Vector2.Angle(from, to);
           return cross.z > 0 ? -angle : angle;
       }
        /// <summary>
        /// A simply function to convert an angle to a -180/180 wrap
        /// </summary>
        public static float Wrap180(float angle)
        {
            angle %= 360;
            if (angle < -180)
            {
                angle += 360;
            }
            else if (angle > 180)
            {
                angle -= 360;
            }
            return angle;
        }
        /// <summary>
        /// 判断正负,正返回1,负返回-1,0返回0
        /// </summary>
        /// <returns></returns>
        public static int JudgeSign(this float num)
        {
            if (num == 0) return 0;
            return num > 0 ? 1 : -1;
        }
        /**
        *  非递归 -- 判断一个数是2的几次幂
        *  @param n	给定的数 
        */
        public static int log(int n)
        {
            int count = 0;
            if(n == 1)
                return 0;
		
            while(n > 1)
            {
                n = n>>1;	//右移 -> 除以2的1次方 
                count++;
            }
            return count; 
        }
    }
}