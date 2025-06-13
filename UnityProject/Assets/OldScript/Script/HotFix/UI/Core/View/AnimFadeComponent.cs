using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AION.CoreFramework
{
    public class AnimFadeComponent:AniComponent
    {

        public bool IsFadeIn;
        
        
        public TweenerCore<float, float, FloatOptions> AnimFadeInImp(Transform tr, float startValue = 0,
            float endValue = 1)
        {
            var canvasGroup = tr.gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = startValue;
            return canvasGroup.DOFade(endValue, duration);
        }

        public TweenerCore<float, float, FloatOptions> AnimFadeOutImp(Transform tr, float startValue = 1,
            float endValue = 0)
        {
            var canvasGroup = tr.gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = startValue;
            return canvasGroup.DOFade(endValue, duration);
        }
        public override void Animate(Transform target, Action callWhenFinished)
        {
            
        }
    }
}