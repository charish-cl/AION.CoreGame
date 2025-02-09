
using UnityEngine;
namespace GameDevKit
{
    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        protected Singleton() { }

        protected static T _inst = new T();
        public static T Instance
        {
            get
            {
                if (null == _inst)
                    _inst = new T();
                return _inst;
            }
        }
    }

    /// <summary>
    /// 单例Mono基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T>: MonoBehaviour where T : MonoSingleton<T>
    {
        protected MonoSingleton() { }
        
        private static T _inst ;
        public static T Instance
        {
            get
            {
                if (null == _inst)
                {
                    _inst=FindObjectOfType<T>();
                    if ( null == _inst )
                    {
                        _inst = new GameObject(typeof(T).Name).AddComponent<T>();
                        if ( _inst.GetType() != typeof(MonoMain) )
                        {
                            _inst.transform.parent = MonoMain.Instance.transform;
                        }
                    }
                        
                }
                   
                return _inst;
            }
        }

        public void Active(){}
    }

    public class MonoMain : MonoSingleton<MonoMain>
    {
        
    }
}