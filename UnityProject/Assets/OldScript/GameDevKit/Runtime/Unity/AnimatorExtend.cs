using System;
using System.Collections;
using UnityEngine;

namespace GameDevKit.Anim
{
    public static class AnimatorExtend
    {
        public static float GetCurAnimLength(this Animator animator)
        {
            return animator.GetCurrentAnimatorStateInfo(0).length;
        }
        public static int GetCurAnimFrameNum(this Animator anim)
        {
            //动画片段长度
             float length = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //获取动画片段帧频
            float frameRate = anim.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
              //计算动画片段总帧数
            int totalFrame = Mathf.RoundToInt(length / (1 / frameRate));

            return totalFrame;
        }
        public static IEnumerator  WaitFinish(this Animator animator,Action action)
        {
            var sawAnimState =animator.GetCurrentAnimatorStateInfo(0);//读取当前动画事件的时间
            yield return new WaitForSeconds(sawAnimState.length);//动画执行完成后
            action.Invoke();
        }    
        /// <summary>
        /// 动画重复播放几次结束
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="action"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static IEnumerator  WaitTimesFinish(this Animator animator,float times,Action action)
        {
            var sawAnimState =animator.GetCurrentAnimatorStateInfo(0);//读取当前动画事件的时间
            yield return new WaitForSeconds(sawAnimState.length*times);//动画执行完成后
            action.Invoke();
        }
    }
}