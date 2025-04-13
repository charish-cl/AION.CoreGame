namespace AION.CoreFramework
{
    /// <summary>
    /// 如果不声明这个抽象类，ObjectPoolManager就没法控制所有对象池
    /// </summary>
    public abstract class ObjectPoolBase
    {
        public abstract void Update(float elapseSeconds, float realElapseSeconds);
    }
}