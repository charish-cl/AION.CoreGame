namespace AION.CoreFramework
{
    /// <summary>
    /// 如果直接继承ObjectBase，并把IsInUse，ReferenceCnt写到ObjectBase里，会出现能直接访问到这两个变量的情况，
    /// 所以要通过中间层
    /// </summary>
    public class ObjectBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;


        public object target;
        
        public virtual void OnSpawn()
        {
            
        }
        public virtual void OnUnspawn()
        {
            
        }
        public virtual void OnRelease()
        {
            
        }
    }
}