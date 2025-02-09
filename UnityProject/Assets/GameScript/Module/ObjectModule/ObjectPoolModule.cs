using System.Collections.Generic;

namespace AION.CoreFramework
{
    public class ObjectPoolModule:Module
    {
        private IObjectPoolManager _objectPoolManager;
        
        protected override void Awake()
        {
            base.Awake();

            _objectPoolManager = ModuleImpSystem.GetModule<IObjectPoolManager>();
            if (_objectPoolManager == null)
            {
                Log.Fatal("ObjectPoolManager invalid.");
                return;
            }
        }
        public ObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, int capacity, float releaseInterval) where T : ObjectBase
        {
            return _objectPoolManager.CreateObjectPool<T>(allowMultiSpawn, capacity, releaseInterval);
        }
        
        public ObjectPool<T> CreateObjectPool<T>() where T : ObjectBase
        {
            return _objectPoolManager.CreateObjectPool<T>(true, 10, 10);
        }
    }
    
}