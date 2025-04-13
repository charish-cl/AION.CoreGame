using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AION.CoreFramework
{
    public static class AnimatorUtility
    {
        public static async UniTask PlayAnimationAsync(this Animator animator, string clipName)
        {
            // 触发动画
            animator.enabled = true;
            animator.Play(clipName, 0, 0);
            // 等待动画播放完成
            await UniTask.WaitUntil(() =>
            {
                return !animator.GetCurrentAnimatorStateInfo(0).IsName(clipName)
                       || animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
            }, cancellationToken: animator.GetCancellationTokenOnDestroy());
            animator.enabled = false;
        }


#if SPINE
        /// <summary>
        /// 异步播放 Spine 动画并等待其完成。
        /// </summary>
        /// <param name="animationState">Spine 的 AnimationState 实例。</param>
        /// <param name="animationName">要播放的动画名称。</param>
        /// <param name="trackIndex">播放的轨道索引。</param>
        /// <param name="loop">是否循环播放。</param>
        /// <returns>异步任务，等待动画完成。</returns>
        public static async UniTask PlayAnimationAsync(this AnimationState animationState, string animationName,
            int trackIndex = 0, bool loop = false)
        {
            if (animationState == null)
            {
                Debug.LogError("AnimationState is null.");
                return;
            }

            var skeletonData = animationState.Data.SkeletonData;
            if (skeletonData == null)
            {
                Debug.LogError("SkeletonData is null.");
                return;
            }

            var animation = skeletonData.FindAnimation(animationName);
            if (animation == null)
            {
                Debug.LogError($"Animation '{animationName}' not found.");
                return;
            }

            // 设置动画
            var trackEntry = animationState.SetAnimation(trackIndex, animationName, loop);

            // 使用 UniTaskCompletionSource 来提供完成通知
            var completionSource = new UniTaskCompletionSource();

            // 监听动画完成事件
            trackEntry.Complete += entry =>
            {
                // 当动画完成时设置结果
                completionSource.TrySetResult();
            };

            // 等待动画完成通知
            await completionSource.Task;
        }
#endif
        
    }
}