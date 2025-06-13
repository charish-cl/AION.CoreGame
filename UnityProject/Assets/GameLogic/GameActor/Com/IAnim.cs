using UnityEngine;

namespace AION.CoreFramework
{
    public interface IAnim
    {
        /// <summary>
        /// 暂停时的速度
        /// </summary>
        float PauseSpeed{get;set;}
        
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        bool IsPaused {get;set;}
        
        
        void PlayAnimation(string animName);

        string GetCurPlayAnim();
        
        void SetAnimSpeed(float speed);
        
        void PauseAnima();
        
        void ResumeAnim();
        
    }


   
}