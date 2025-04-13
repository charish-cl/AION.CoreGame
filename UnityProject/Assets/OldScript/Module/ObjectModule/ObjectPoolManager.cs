using System.Collections.Generic;

namespace AION.CoreFramework
{
    public interface IObjectPoolManager
    {
        public ObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, int capacity, float releaseInterval)
            where T : ObjectBase;

    }
    public class ObjectPoolManager:ModuleImp,IObjectPoolManager
    {
        List<ObjectPoolBase> m_ObjectPools = new List<ObjectPoolBase>();
        public ObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, int capacity, float releaseInterval) where T : ObjectBase
        {
            ObjectPool<T> pool = new ObjectPool<T>(allowMultiSpawn, capacity, releaseInterval);
            m_ObjectPools.Add(pool);
            return pool;
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);
            foreach (var mObjectPool in m_ObjectPools)
            {
                mObjectPool.Update(elapseSeconds, realElapseSeconds);
            }
            //轮询所有对象池，回收对象池中的对象
        }

        internal override void Shutdown()
        {
            
        }
    }
}