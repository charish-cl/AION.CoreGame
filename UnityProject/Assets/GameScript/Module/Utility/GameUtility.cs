using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AION.CoreFramework
{
    public static class GameUtility
    {
        
        public static bool IsClickTarget(GameObject target)
        {
            var raycastResults = new List<RaycastResult>();
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            List<GameObject> list = raycastResults.Select(x => x.gameObject).ToList();

            return list.Contains(target);
        }

        /// <summary>
        /// 互斥对象，同时只有一个显示
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="check">a是否显示</param>
        /// <returns>当前显示对象</returns>
        public static GameObject MutuallyGameObject(GameObject a, GameObject b, bool check)
        {
            a.SetActive(check);
            b.SetActive(!check);
            return check ? a : b;
        }

        public static GameObject DisaplayOne(GameObject o, int index)
        {
            var parent = o.transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(i == index);
            }

            return parent.GetChild(index).gameObject;
        }

       

        /// <summary>
        /// 从子物体创建物体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="cnt"></param>
        /// <param name="createFunc"></param>
        public static void CreateFromChild(Transform parent, int cnt,Action<Transform,int> createFunc=null)
        {
            if (parent.childCount<=0)
            {
                throw new Exception($"{parent.name}没有子物体");
            }
            
            var childcnt = parent.childCount;
            var prefab = parent.GetChild(0).gameObject;
            for (int i = 0; i < cnt; i++)
            {
                Transform child;
                if (i< childcnt)
                {
                    child = parent.GetChild(i);
                }
                else
                {
                    child = GameObject.Instantiate(prefab,parent).transform;
                }

                child.gameObject.SetActive(true);
                if (createFunc!=null)
                {
                    createFunc(child,i);
                }
            }
            //多出来的隐藏
            for (int i = cnt; i < childcnt; i++)
            {
                parent.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 遍历子物体,超过cnt的隐藏
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="cnt"></param>
        /// <param name="action"></param>
        public static void ForEachChildSetActive(Transform parent,int cnt,Action<int,Transform> action = null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (i>=cnt)
                {
                    parent.GetChild(i).gameObject.SetActive(false);
                    continue;
                }
                parent.GetChild(i).gameObject.SetActive(true);

                if (action!=null)
                    action.Invoke(i, parent.GetChild(i));
            }
        }
        public static void ForEachChild(Transform parent,Action<int,Transform> action = null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (action!=null)
                    action.Invoke(i, parent.GetChild(i));
            }
        }
    }
}