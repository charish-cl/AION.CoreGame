using System.Collections.Generic;

namespace GameLogic
{
    public class TaskBaseData
    {
        public int ID { get; set; }
        
        public int CurrentProgress { get; set; }
        
        public int MaxProgress { get; set; }
        
        public List<ItemBaseShowData> Rewards { get; protected set; }

        public bool IsCompleted
        {
            get
            {
                return CurrentProgress >= MaxProgress;
            }
        }
        
        public bool IsInProgress
        {
            get
            {
                return CurrentProgress < MaxProgress;
            }
        }
        
        public virtual bool IsClaimReward
        {
            get
            {
                return false;
            }
        }
        
    }
}