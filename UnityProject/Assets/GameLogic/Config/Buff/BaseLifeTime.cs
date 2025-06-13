namespace AION.Config.Buff
{
    public abstract class BaseLifeTime
    {
        /// <summary>
        /// Check if the buff can be destroyed
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckCanDestroy();
    }
}