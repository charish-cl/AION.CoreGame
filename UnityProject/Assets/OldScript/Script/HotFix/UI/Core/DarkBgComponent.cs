// using System;
// using Cysharp.Threading.Tasks;
// using DG.Tweening;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace AION.CoreFramework
// {
//     public class DarkBgComponent:MonoBehaviour
//     {
//         
//         public GameObject DarkBg;
//         public Color Bg = new Color(0, 0, 0, 0.75f);
//
//         private void Awake()
//         {
//             DarkBg = new GameObject(nameof(DarkBg));
//             DarkBg.transform.SetParent(transform);
//             var image = DarkBg.GetOrAddComponent<Image>();
//             var rectTransform = DarkBg.GetComponent<RectTransform>();
//             // 设置Image全屏
//             rectTransform.anchorMin = new Vector2(0, 0);
//             rectTransform.anchorMax = new Vector2(1, 1);
//             rectTransform.offsetMin = Vector2.zero;
//             rectTransform.offsetMax = Vector2.zero;
//             image.color = Bg;
//             DarkBg.gameObject.SetActive(false);
//         }
//              /// <summary>
//         /// 暗黑层
//         /// </summary>
//         public void DarkenBG(Transform tr)
//         {
//             var uiconfigTool = tr.GetComponent<UIConfigBindTool>();
//             if (uiconfigTool == null)
//             {
//                 throw new Exception(" UIConfigBindTool is null");
//             }
//             
//             DarkBg.SetActive(true);
//             DarkBg.GetComponent<Image>().color = Bg;
//             DarkBg.GetOrAddComponent<CanvasGroup>().alpha = 0;
//             DarkBg.GetOrAddComponent<CanvasGroup>().DOFade(1, 0.4f);
//           
//             MoveAboveTarget(tr, DarkBg.transform);
//         }
//
//         public  void DarkBgFadeOut(Transform closeUI)
//         {
//
//             int index = Int32.MinValue;
//             //如果UI组里面还有其他的UI，则不关闭弹窗，移动到最前面,从后往前遍历
//             DarkBg.GetComponent<Image>().DOFade(0, 0.4f);
//              
//             DarkBg.SetActive(false);
//         }
//          
//         // 封装的函数：将指定的 Transform 移动到目标对象的上一层
//         public void MoveAboveTarget(Transform target, Transform objectToMove)
//         {
//             if (target == null || objectToMove == null)
//             {
//                 throw new ArgumentNullException("Target or object to move cannot be null");
//             }
//             DarkBg.transform.SetParent(null);//先移出去，不作为计算参考
//             int targetIndex = target.GetSiblingIndex(); // 获取目标物体的索引
//
//             Debug.Log("MoveAboveTarget: " + objectToMove.name + " to " + target.name + " index: " + targetIndex);
//             
//             objectToMove.SetParent(target.parent);
//             // 将 objectToMove 移动到目标物体的上一层
//             objectToMove.SetSiblingIndex(targetIndex);
//         }
//
//         private void OnDestroy()
//         {
//             Destroy(DarkBg);
//         }
//     }
// }