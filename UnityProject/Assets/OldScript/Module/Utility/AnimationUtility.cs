using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AION.CoreFramework
{
    public static class AnimationUtility
    {
#if DOTWEEN
         public static Tween IconShake(Transform transform)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.one * 1.3f, 0.5f));
            sequence.Insert(0.4f, transform.DOPunchRotation(
                new Vector3(0, 0, 20), 1f,
                vibrato: 6,
                elasticity: 0.5f
            ));
            sequence.Insert(1.2f, transform.DOScale(Vector3.one, 0.5f));
            sequence.Append(transform.DOScale(Vector3.one, 2));
            sequence.SetLoops(-1);
            return sequence.Play();
        }

        public static Tween ButtonPress(RectTransform transform, int TargetY = -10, float easeOvershootOrAmplitude = 10f)
        {
            Tween t = transform.DOAnchorPosY(TargetY, 0.15f);
            t.easeOvershootOrAmplitude = easeOvershootOrAmplitude;
            t.SetEase(Ease.OutBack);
            return t;
        }


        public static Tween RewardPreviewBubble(RectTransform transform, Vector2 pivot, bool show)
        {
            if (show)
            {
                return RewardPreviewBubble(transform, pivot);
            }
            else
            {
                return RewardPreviewBubbleHide(transform, pivot);
            }
        }

        public static Tween RewardPreviewBubble(RectTransform transform, Vector2 pivot)
        {
            transform.localScale = Vector3.zero;
            transform.pivot = pivot;
            return transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        public static Tween RewardPreviewBubbleHide(RectTransform transform, Vector2 pivot)
        {
            // transform.localScale = Vector3.one;
            transform.pivot = pivot;
            return transform.DOScale(Vector3.zero, 0.2f);
        }

        public static Tween NumTextGradient(TextMeshProUGUI textMeshProUGUI, ulong targetNum, float time = 0.5f)
        {
            string text = textMeshProUGUI.text.Replace(",", "");

            if (!ulong.TryParse(text, out var startNum))
            {
                startNum = 0;
            }

            return DOTween.To(value => textMeshProUGUI.ShowTextNumImp((ulong)Mathf.FloorToInt(value), false), startNum, targetNum, time).OnComplete(() =>
            {
                textMeshProUGUI.ShowTextNumImp(targetNum, false);
            });
        }

        public static Tween MultipleNumScaleAnim(Transform transform, float scale)
        {
            return transform.DOPunchScale(Vector3.one * scale, 0.5f);
        }

        public static async UniTask PauseAtFrame(Animator animator, int targetFrame)
        {
            var clip = animator.runtimeAnimatorController.animationClips[0];
            var targetTime = targetFrame / clip.frameRate;
            animator.speed = 1;
            while (true)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= targetTime / clip.length)
                {
                    animator.speed = 0;
                    break;
                }

                await UniTask.NextFrame();
            }
        }

        public static async UniTask WaitCompleteAnim(Animator animator)
        {
            animator.speed = 1;
            while (true)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1)
                {
                    animator.speed = 0;
                    break;
                }

                await UniTask.NextFrame();
            }
        }

        public static Sequence ShieldCountAnim(TextMeshProUGUI text, int count)
        {
            text.gameObject.SetActive(true);
            text.color = new Color(1, 1, 1, 0);
            text.transform.localScale = Vector3.one;
            text.text = "X" + count;
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            var pos = rectTransform.anchoredPosition;
                Sequence sequence = DOTween.Sequence();

            sequence.Append(text.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutBack));
            sequence.Insert(0, text.DOFade(1, 0.25f));
            sequence.Insert(1, rectTransform.DOAnchorPosY(pos.y + 100, 0.4f));
            sequence.Insert(1f, text.DOFade(0, 0.4f));
            sequence.OnComplete(() => { rectTransform.anchoredPosition = pos; });

            return sequence;
        }
#endif
       
    }
}