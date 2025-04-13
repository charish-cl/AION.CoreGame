using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AION.CoreFramework
{
    public class AnimMoveComponent:AniComponent
    {
        
          public TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveOut(RectTransform rectTransform,
            float duration = 0.3f,
            float easeOvershootOrAmplitude = 0, bool top2Bottom = false, bool isNeedBounce = false)
        {
            var y = rectTransform.anchoredPosition.y;

            // UIFormLogic.CloseActions.Add(() =>
            // {
            //     if (rectTransform == null)
            //     {
            //         return null;
            //     }
            //
            //     rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
            // });
            //之前的
            //return AnimMoveImp(rectTransform, duration).SetEase(Ease.InOutBack);

            Ease ease = isNeedBounce ? Ease.InBack : Ease.InCirc;

            return AnimMoveImp(rectTransform, duration, top2Bottom: top2Bottom).SetEase(ease);
        }

        /// <summary>
        /// 动画移动
        /// </summary>
        /// <param name="rectTransform">物体</param>
        /// <param name="duration">间隔</param>
        /// <param name="easeOvershootOrAmplitude">回弹系数</param>
        /// <param name="top2Bottom">顶到底</param>
        /// <param name="isNeedBounce">是否需要回弹</param>
        /// <returns></returns>
        public TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveIn(RectTransform rectTransform,
            float duration = 0.3f,
            float easeOvershootOrAmplitude = 0, bool top2Bottom = false, bool isNeedBounce = false)
        {
            var tween = AnimMoveImp(rectTransform, duration, false, top2Bottom);
            tween.SetEase(isNeedBounce ? Ease.OutBack : Ease.OutExpo);
            tween.easeOvershootOrAmplitude = easeOvershootOrAmplitude;
            tween.easePeriod = duration;

            return tween;
        }

        /// <summary>
        ///  UI移动的方法
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="duration"></param>
        /// <param name="isOut">是否移出</param>
        /// <returns></returns>
        TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveImp(RectTransform rectTransform, float duration = 0.3f,
            bool isOut = true, bool top2Bottom = false)
        {
            var anchoredPosition = rectTransform.anchoredPosition;
            // var bottomDistance = anchoredPosition.y -= GetBottomDistance(rectTransform);

            float distance;
            if (top2Bottom)
            {
                distance = anchoredPosition.y + GetTopDistance(rectTransform);
            }
            else
            {
                distance = anchoredPosition.y - GetBottomDistance(rectTransform);
            }

            if (isOut)
            {
                return rectTransform.DOAnchorPosY(distance, duration);
            }

            return rectTransform.DOAnchorPosY(distance, duration).From();
        }
        
        
        public override void Animate(Transform target, Action callWhenFinished)
        {
            
        }
    }
}