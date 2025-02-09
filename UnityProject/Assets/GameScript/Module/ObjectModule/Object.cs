namespace AION.CoreFramework
{
    public class Object<T> where T :ObjectBase
    {
        public T m_Object;
        
        /// <summary>
        /// 是否正在使用
        /// </summary>
        public bool IsInUse;

        /// <summary>
        /// 引用数量
        /// </summary>
        public int ReferenceCnt;

        public bool IsLock;
        public Object(T item, bool isLock = false)
        {
            m_Object = item;
            IsInUse = false;
            ReferenceCnt = 0;
            IsLock = isLock;
        }


        public  void OnSpawn()
        {
            m_Object.OnSpawn();
        }
        public  void OnUnspawn()
        {
            m_Object.OnUnspawn();
        }
        public  void OnRelease()
        {
            m_Object.OnRelease();
        }
    }
}