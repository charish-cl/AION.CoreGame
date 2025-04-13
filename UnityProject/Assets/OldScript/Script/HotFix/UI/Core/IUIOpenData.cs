namespace AION.CoreFramework
{
    public interface IUIOpenData
    {
        void Open();
    }
    public interface IUIOpenData<T1>
    {
        void Open(T1 t1);
    }
    public interface IUIOpenData<T1,T2>
    {
        void Open(T1 t1, T2 t2);
        
    }
    
    public interface IUIOpenData<T1,T2,T3>
    {
        void Open(T1 t1, T2 t2,T3 t3);    
    }
    
    
    
    
}