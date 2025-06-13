using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Feif.UI
{
    public class HorizontalSplit : MonoBehaviour
    {
        [Range(0, 1)]
        public List<float> targetRadio = new List<float>();

        public GameObject SplitGameObject;

        private RectTransform selfRect;
        private RectTransform SelfRect
        {
            get
            {
                if (selfRect == null)
                {
                    selfRect = GetComponent<RectTransform>();
                }
                return selfRect;
            }
        }

        private List<RectTransform> splitObjects = new List<RectTransform>();
        
        
        [Button]
        public  void Adapt( List<float> targetRadio,GameObject splitGameObject=null)
        {
            this.targetRadio = targetRadio;
            if (splitGameObject!=null)
            {
                this.SplitGameObject = splitGameObject;
            }
            float parentWidth = SelfRect.rect.width;

            int childCount = splitObjects.Count;

            for (int i = 0; i < targetRadio.Count; i++)
            {
                GameObject splitObject = null;
                RectTransform rectTransform = null;
                // 如果子物体数量不够，实例化新的模板对象
                if (i < childCount)
                {
                    splitObject = splitObjects[i].gameObject;
                    rectTransform = splitObject.GetComponent<RectTransform>();
                }
                else
                {
                    splitObject = Instantiate(SplitGameObject, SelfRect);
                    rectTransform = splitObject.GetComponent<RectTransform>();
                    splitObjects.Add(rectTransform);
                }
    

                float targetPositionX = parentWidth * targetRadio[i];
                rectTransform.localPosition = new Vector3(targetPositionX, 0, 0);
            }
        }

        public Vector3 GeSplittWorldPosition(int index)
        {
            if (index >= 0 && index < splitObjects.Count)
            {
                return splitObjects[index].position;
            }
            else
            {
                Debug.LogError("Index out of range.");
                return Vector3.zero;
            }
        }

        public Vector3 GetSplitTopWorldPosition(int index)
        {
            if (index >= 0 && index < splitObjects.Count)
            {
                RectTransform rectTransform = splitObjects[index];
                Vector3 topLocalPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y + rectTransform.rect.height / 2, rectTransform.localPosition.z);
                return rectTransform.TransformPoint(topLocalPosition);
            }
            else
            {
                Debug.LogError("Index out of range.");
                return Vector3.zero;
            }
        }
    }
}
