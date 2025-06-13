using UnityEngine;

namespace GameDevKit.Utility
{
   
    public static class RandomPosition {
        
        /// <summary>
        ///  生成一个在内接圆和外接圆之间的随机位置
        /// </summary>
        /// <param name="radius"> 圆的半径</param>
        /// <param name="center">圆的中心点</param>
        /// <returns></returns>
        public static Vector2 GeneratePositionInCirce(float outerradius,float innerradius, Vector2 center) {
            // 生成一个随机角度和距离
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(innerradius, 2f * outerradius);

            // 将极坐标转换为笛卡尔坐标
            float x = center.x + distance * Mathf.Cos(angle);
            float y = center.y + distance * Mathf.Sin(angle);
            // 返回结果
            return new Vector2(x, y);
        }
    
        /// <summary>
        /// 生成一个在椭圆内的随机位置
        /// </summary>
        /// <param name="a">椭圆长轴半径</param>
        /// <param name="b"> 椭圆短轴半径</param>
        /// <param name="center"> 椭圆中心点位置</param>
        /// <returns></returns>
        public static Vector2 GenerateEllipsePosition(float a, float b,Vector2 center)
        {
            Vector2 randomPoint = Random.insideUnitCircle;
            float x = randomPoint.x * a;
            float y = randomPoint.y * b;
            Vector2 randomPos = new Vector2(x, y) + center;
            return randomPos;
        }
        
    }

}