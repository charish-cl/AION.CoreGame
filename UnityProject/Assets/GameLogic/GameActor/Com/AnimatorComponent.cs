using UnityEngine;

namespace AION.CoreFramework
{
    public class AnimatorComponent: IAnim
    {
        public Animator animator;


        public float PauseSpeed { get; set; }
        public bool IsPaused { get; set; }

        public void PlayAnimation(string animName)
        {
            if (animator != null)
            {
                animator.Play(animName);
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
    }
}