using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    public static class ThranformExtend
    {
        public static float GetDis(this Transform transform, Transform other)
        {
            return Vector2.Distance(transform.position, other.position);
        }
        public static float GetDis(this Transform transform, Vector2 other)
        {
            return Vector2.Distance(transform.position, other);
        }
         public static T AddScriptGameObject<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.CreateObject(typeof(T).Name).AddComponent<T>();
        }

        ///主要查找物体的所有层级是否有叫_name的物体,返回第一个
        public static GameObject FindObj(this GameObject gameObject, string _name)
        {
            //获得所有层级包括隐藏的
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);
            //长度
            int transLength = transforms.Length;
            //迭代和返回
            for (int i = 0; i < transLength; i++)
            {
                if (transforms[i].name.Equals(_name))
                {
                    return transforms[i].gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// 递归查找子物体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static Transform FindChildInTransformRecursion(this Transform parent, string child)
        {
            Transform childTF = parent.Find(child);
            if (childTF != null)
            {
                return childTF;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                childTF = FindChildInTransformRecursion(parent.GetChild(i), child);
                if (childTF)
                    return childTF;
            }

            return null;
        }

        public static T FindChildComponent<T>(this Transform parent, string path)
        {
            return parent.Find(path).GetComponent<T>();
        }

        /// <summary>
        /// 获取相对于父物体的路径以便于 transform.Find("UI/Btn")
        /// </summary>
        /// <param name="child"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string GetRelativePath(this Transform child, Transform root)
        {
            StringBuilder path = new StringBuilder();
            path.Append(child.name);
            while (child.parent != null && child.parent != root)
            {
                child = child.parent;
                path.Insert(0, '/');
                path.Insert(0, child.name);
            }

            return path.ToString();
        }

        public static Transform GetTopParent(this Transform transform)
        {
            Transform tran = transform;
            while (tran.parent)
            {
                tran = tran.parent;
            }

            return tran;
        }

        public static Transform SetNullParent(this GameObject go)
        {
            go.transform.SetParent(null);
            return go.transform;
        }

        public static Transform SetThransActive(this Transform transform, bool isactive)
        {
            transform.gameObject.SetActive(isactive);
            return transform;
        }

        public static void SetRotation(this Transform transform, Quaternion quaternion)
        {
            transform.rotation = quaternion;
        }

        public static void SetPos(this Transform transform, Transform other)
        {
            transform.position = other.position;
        }

        public static void SetPos(this Transform transform, Vector3 other)
        {
            transform.position = other;
        }

        public static void LookAt2DY(this Transform transform, Vector3 target)
        {
            transform.up = (target - transform.position).normalized;
        }

        public static void LookAt2DTargetX(this Transform transform, Transform target)
        {
            transform.right = (target.transform.position - transform.position).normalized;
        }

        public static void LookAt2DTargetX(this Transform transform, Vector3 target)
        {
            transform.right = (target - transform.position).normalized;
        }

        public static void LookAtDirX(this Transform transform, Vector3 dir)
        {
            transform.right = dir;
        }

        public static void LookAt2D_Qua(Transform transform, Vector3 target, float offset)
        {
            Vector3 v = target - transform.position; //首先获得目标方向
            v.z = 0; //这里一定要将z设置为0
            float angle = Vector3.SignedAngle(Vector2.up, v, Vector3.forward); //得到围绕z轴旋转的角度

            float result = angle + offset;

            Quaternion rotation = Quaternion.Euler(0, 0, result); //将欧拉角转换为四元数
            transform.rotation = rotation;
        }


        /// <summary>
        /// 向一个角度发射n条射线
        /// </summary>
        /// <param name="rotatGameObjects">旋转对象</param>
        /// <param name="num">数量</param>
        /// <param name="offset">角度</param>
        /// <param name="dir">方向</param>
        public static void RotateMultiLine(List<GameObject> rotatGameObjects, int num, float offset, Vector2 dir)
        {
            dir = dir.normalized;
            float angle = -offset / 2 * (num - 1);
            for (int i = 0; i < num; i++)
            {
                //旋转后的方向
                var rotatedir = Rotate2ByTraigle(dir, angle + offset * i);
                LookAtDirX(rotatGameObjects[i].transform, rotatedir);
            }
        }


        public static void SetXScale(this Transform transform, float x)
        {
            var scale = transform.localScale;
            transform.localScale = new Vector3(x, scale.y, scale.z);
        }

        public static void SetLocalPosAndAngle(this Transform transform, Vector3 pos, Quaternion quaternion)
        {
            transform.localPosition = pos;
            transform.localRotation = quaternion;
        }

        /// <summary>
        /// Transform重置
        /// </summary>
        /// <param name="transform"></param>
        public static void ResetLocalPosAndAngle(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Transform重置
        /// </summary>
        /// <param name="transform"></param>
        public static void ResetLocalTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Transform重置
        /// </summary>
        /// <param name="transform"></param>
        public static GameObject ResetLocalTransform(this GameObject go)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        public static void ResetTransform(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        public static void ResetAngle(this Transform transform)
        {
            transform.rotation = Quaternion.identity;
        }

        public static void SetLocalTransformY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }

        public static GameObject SetParent(this GameObject go, GameObject parent)
        {
            go.transform.SetParent(parent.transform);
            return go;
        }
        public static GameObject SetParent(this GameObject go, Transform parent)
        {
            go.transform.SetParent(parent.transform);
            return go;
        }

        /// <summary>
        /// 获取括号分割后的原始name
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string GetRawName(this GameObject go)
        {
            return go.name.Split('(')[0].TrimEnd();
        }

        public static string GetSpliteName(this GameObject go, int index, char patten)
        {
            return go.name.Split(patten)[index];
        }

        /// <summary>
        /// 获取括号里面的数
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string GetBracketNum(this GameObject go)
        {
            var str = go.name;

            if (!str.Contains("("))
            {
                return "0";
            }

            int arr = str.IndexOf(")") - 1 - str.IndexOf("("); //结束位置 减 1 再减 开始位置 获取中间位置数
            String str2 = str.Substring(str.IndexOf("(") + 1, arr); //参数1：开始位置加1 参数2：长度：中间位置数
            return str2;
        }

        /// <summary>
        /// 分割后后面的字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string SplitAfter(this string s,char c)
        {
            return s.Substring(s.IndexOf(c) + 1);
        }
        public static string SplitBefore(this string s,char c)
        {
            return s.Substring(0,s.IndexOf(c));
        }
        public static GameObject CreateObject(this GameObject parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            return go;
        }
        public static void AddChild(this GameObject parent, GameObject other)
        {
            other.transform.SetParent(parent.transform);
        }
        public static void AddChild(this Transform parent, MonoBehaviour other)
        {
            other.transform.SetParent(parent.transform);
        }

        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TryGetOrAddComponent<T>(this GameObject go) where T : Component
        {
            go.TryGetComponent(out T component);
            if (component is null)
                component = go.AddComponent<T>();
            return component;
        }


        /// <summary>
        /// 只遍历子物体，不包含孙子
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action"></param>
        public static void ForEachChild(this Transform transform, Action<Transform> action)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int temp = i;
                Transform go = transform.GetChild(temp);
                action.Invoke(go);
            }
        }  
        
      
        /// <summary>
        /// 遍历所有子物体
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action"></param>
        public static void ForEachAllChild(this Transform transform, Action<Transform> action)
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                Transform t = child;
                action.Invoke(t);
            }
        }

        public static void DestoryChildren(this Transform t)
        {
            for (int i = 0; i < t.childCount + i; i++)
            {
                Transform go = t.GetChild(0);
                Object.DestroyImmediate(go.gameObject);
            }
        }

        public static Vector2 GetRandomDir()
        {
            return new Vector2(Random.Range(-10, 11), Random.Range(-10, 11)).normalized;
        }

        /// <summary>
        /// 以方向朝右为例,返回物体应该调整的角度
        /// </summary>
        /// <param name="normal">法线</param>
        /// <returns></returns>
        public static Quaternion GetDirQuateration(Vector2 normal)
        {
            Quaternion quaternion = Quaternion.identity;
            var newnormal = new Vector2(Mathf.RoundToInt(normal.x), Mathf.RoundToInt(normal.y));
//            Debug.Log(normal+"-----"+newnormal);
            if (newnormal == Vector2.right)
            {
                quaternion = Quaternion.Euler(0, 0, 180);
            }
            else if (newnormal == Vector2.up)
            {
                quaternion = Quaternion.Euler(0, 0, 270);
            }
            else if (newnormal == Vector2.down)
            {
                quaternion = Quaternion.Euler(0, 0, 90);
            }

            return quaternion;
        }

        /// <summary>
        /// 得到一个范围的随即方向
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetRandomDirofScope(Vector2 dir, float upAngle, float downAngle)
        {
            dir = dir.normalized;
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            float randomangle = (angle + Random.Range(-downAngle, upAngle)) * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(randomangle), Mathf.Sin(randomangle)).normalized;
        }

        /// <summary>
        /// 获取向量偏移角度后的方向,向上偏移 x1＝xcosθ+ysinθ, y1＝-xsinθ+ycosθ  用的这种
        /// </summary>
        public static Vector2 Rotate2ByTraigle(this Vector2 vector2, float upAngle)
        {
            float x = vector2.x;
            float y = vector2.y;

            // 1、四元数
            // Vector3 newVec = Quaternion.AngleAxis(angle,axis)*oldVec;
            // Vector3 newVec = Quaternion.Eular(0,angle,0)*oldVec;
            // 2、二维向量运算
            //     旋转θ 

            // x1＝xcosθ+ysinθ, y1＝-xsinθ+ycosθ  用的这种
            float A = (upAngle) * Mathf.Deg2Rad;

            return new Vector2(x * Mathf.Cos(A) + y * Mathf.Sin(A), -x * Mathf.Sin(A) + y * Mathf.Cos(A));
        }

        /// <summary>
        /// 旋转一个二维向量 用的 Quaternion.AngleAxis旋转
        /// </summary>
        /// <param name="vector2"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector2 Rotate2ByAngleAxis(this Vector2 vector2, float angle)
        {
            return Quaternion.AngleAxis(angle, Vector3.forward) * vector2;
        }
        public static Vector3 RotateV2ByEuler(Vector3 vec, float angle) {
            return Quaternion.Euler(0, 0, angle) * vec;
        }
        
        /// <summary>
        /// 获取圆周围的点,从CodeMonkey那里借鉴的方法
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="distance">半径</param>
        /// <param name="positionCount">数量</param>
        /// <returns></returns>
        public static List<Vector3> GetCirCleAround(Vector3 startPosition, float distance, int positionCount) {
            List<Vector3> positionList = new List<Vector3>();
            for (int i = 0; i < positionCount; i++) {
                float angle = i * (360f / positionCount);
                Vector3 dir = RotateV2ByEuler(new Vector3(1, 0), angle);
                Vector3 position = startPosition + dir * distance;
                positionList.Add(position);
            }
            return positionList;
        }

        
        /// <summary>
        /// 获取目标的法线
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector2 GetTargetNormal(this Transform transform, Transform target)
        {
            return (target.position - transform.position).normalized;
        }

        public static Vector2 GetTargetNormal(this Transform transform, Vector2 target)
        {
            return (target - (Vector2)transform.position).normalized;
        }

        public static void Hide(this MonoBehaviour go)
        {
            if (!go.gameObject.activeSelf)
            {
                return;
            }
            go.gameObject.SetActive(false);
        }

        public static void Disaplay(this MonoBehaviour go)
        {
            go.gameObject.SetActive(true);
        }
        
     
    }
}