
using System.Collections.Generic;

namespace AION.CoreFramework
{
    public class ObjectPool<T>:ObjectPoolBase where T : ObjectBase
    {
        /// <summary>
        /// 允许多次获取吗
        /// </summary>
        public bool allowMultiSpawn = true;
        
        public int capacity = 10;
        
        public float releaseInterval = 10;
        
        public float Timer { get; set; } = 0;
        
        public GameFrameworkMultiDictionary<string, Object<T>> mPool = new GameFrameworkMultiDictionary<string, Object<T>>();
        /// <summary>
        /// 这个用于查找对象
        /// </summary>
        public Dictionary<object, Object<T>> objMap = new Dictionary<object, Object<T>>();

        public ObjectPool(bool allowMultiSpawn, int capacity, float releaseInterval)
        {
            this.allowMultiSpawn = allowMultiSpawn;
            this.capacity = capacity;
            this.releaseInterval = releaseInterval;
        }

        public void Register(T item,bool isLock)
        {
            var obj = new Object<T>(item,isLock);
            mPool.Add(item.Name,obj );
            objMap.Add(item, obj);
        }

        public T Spawn(string name)
        {
          
            Object<T> item = null;
            if (mPool.TryGetValue(name, out var list))
            {
                foreach (var o in list)
                {
                    if (allowMultiSpawn)
                    {
                        item = o;
                    }
                    else if (o.ReferenceCnt <= 0)
                    {
                        item = o;
                    }
                }
            }
            else
            {
                return null;
            }
            
            item.ReferenceCnt++;
          
            item.OnSpawn();
            
            return item.m_Object;
        }

        protected Object<T> GetObject(object obj)
        {
            if (objMap.TryGetValue(obj, out var item))
            {
                return item;
            }
            
            return null;
        }
        public void UnSpawn(T item)
        {
            var obj = GetObject(item);
            obj.OnUnspawn();
            obj.ReferenceCnt--;
        }

    

        protected bool Release(T item)
        {
            var obj=GetObject(item);

            if (obj.IsInUse||obj.IsLock)
            {
                return false;
            }
            
            obj.OnRelease();

            //从两个地方删除
            mPool.Remove(item.Name, obj);
            objMap.Remove(item);
            
            return true;
        }

        private void ReleaseUnUseObject()
        {
            foreach (var (key, value) in objMap)
            {
                if (value.ReferenceCnt <= 0)
                {
                    Release(value.m_Object);
                }
            }
        }
        void Update(float deltaTime)
        {
            Timer += deltaTime;


            if (Timer >= releaseInterval)
            {
                Timer = 0;
                
                ReleaseUnUseObject();
            }
        }
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Update(elapseSeconds);
        }
    }
}