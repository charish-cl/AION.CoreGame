namespace AION.Config.Buff
{
    public interface IBuff
    {
        void OnStart();
        void OnUpdate(float deltaTime);
        void OnEnd();
        bool CheckExpired();
    }
    public  class BaseBuff : IBuff
    {
        public bool IsExpired = false;
        public void OnStart()
        {
            
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void OnEnd()
        {
            
        }

        public bool CheckExpired()
        {
            return IsExpired;
        }
    }
}