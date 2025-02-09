namespace GameLogic.UISys
{
    /// <summary>
    /// UI状态函数接口 解锁、显示 ， 未拥有 ，拥有
    /// </summary>
    public interface IUIStateFunc
    {
        /// <summary>
        /// 是否已经解锁
        /// </summary>
        /// <returns></returns>
        public bool CheckUnlock();
        
        /// <summary>
        /// 是否需要显示
        /// </summary>
        /// <returns></returns>
        public bool CheckNeedShow();
        
        
        
    }
}