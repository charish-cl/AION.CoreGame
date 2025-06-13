using Sirenix.OdinInspector;

namespace AION.CoreFramework
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(CanvasScaler))]
    public class FixCanvasTool : MonoBehaviour
    {
        void Awake()
        {
            FixResolution();
        }

        [Button]
        public void FixResolution()
        {
            CanvasScaler scaler = GetComponent<CanvasScaler>();

            float rawRadio = scaler.referenceResolution.y/scaler.referenceResolution.x * 1.0f ;
            float currentRadio = Screen.height/Screen.width * 1.0f;
            if (currentRadio > rawRadio)
            {
                //匹配宽
                scaler.matchWidthOrHeight = 0;
            }
            else
            {
                //匹配高
                scaler.matchWidthOrHeight = 1;
            }
        }
    }
}