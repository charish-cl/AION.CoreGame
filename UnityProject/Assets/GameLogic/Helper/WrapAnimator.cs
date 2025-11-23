using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    public class WrapAnimator: IAnim
    {
        public Animator animator;
        
        public WrapAnimator(Animator animator)
        {   
            this.animator = animator;
        }
        public float PauseSpeed { get; set; }
        public bool IsPaused { get; set; }

        public void PlayAnimation(string animName)
        {
            if (animator == null)
            {
                return;
            }
            
            // 检查 Animator 是否已启用
            if (!animator.enabled)
            {
                return;
            }
            
            // 检查是否有 Animator Controller
            if (animator.runtimeAnimatorController == null)
            {
                Log.Warning($"WrapAnimator: Animator 没有 Animator Controller，无法播放动画 '{animName}'");
                return;
            }
            
            // 检查 layer 数量
            if (animator.layerCount <= 0)
            {
                Log.Warning($"WrapAnimator: Animator 没有有效的 Layer，无法播放动画 '{animName}'");
                return;
            }
            
            // 使用默认 layer (0) 播放动画
            try
            {
                animator.Play(animName, 0);
            }
            catch (System.Exception ex)
            {
                Log.Warning($"WrapAnimator: 播放动画 '{animName}' 失败: {ex.Message}");
            }
        }

        public string GetCurPlayAnim()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                return animator.runtimeAnimatorController.name;
            }
            return null;
        }

        public void SetAnimSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
            }
        }

        public void PauseAnima()
        {
            if (animator != null && !IsPaused)
            {
                IsPaused = true;
                PauseSpeed = animator.speed;
                animator.speed = 0;
            }
        }

        public void ResumeAnim()
        {
            if (animator != null && IsPaused)
            {
                animator.speed = PauseSpeed;
                IsPaused = false;
            }
        }

        /// <summary>
        /// 获取当前播放动画的时长
        /// </summary>
        public float GetCurrentAnimationLength()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return 0f;
            }
            
            if (animator.layerCount <= 0)
            {
                return 0f;
            }
            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.length;
        }

        /// <summary>
        /// 获取指定动画名称的时长（从 Animator Controller 中查找）
        /// </summary>
        public float GetAnimationLength(string animName)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return 0f;
            }
            
            // 遍历所有动画片段查找
            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == animName)
                {
                    return clip.length;
                }
            }
            
            return 0f;
        }

        /// <summary>
        /// 检查当前动画是否正在播放指定动画
        /// </summary>
        public bool IsPlayingAnimation(string animName)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return false;
            }
            
            if (animator.layerCount <= 0)
            {
                return false;
            }
            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animName);
        }

        /// <summary>
        /// 获取当前动画的归一化时间（0-1）
        /// </summary>
        public float GetCurrentAnimationNormalizedTime()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return 0f;
            }
            
            if (animator.layerCount <= 0)
            {
                return 0f;
            }
            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }
    }
}