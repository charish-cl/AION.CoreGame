// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using AillieoUtils;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityGameFramework.Runtime;
// using Object = UnityEngine.Object;
// using DG.Tweening;
// using GameFramework;
// using GameKit;
// using Sirenix.Utilities;
// using TMPro;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using Random = UnityEngine.Random;
//
// namespace AION.CoreFramework
// {
//     public static class UIFormExtend
//     {
//         #region 基本
//
//         /// <summary>
//         /// 查找子元素
//         /// </summary>
//         /// <param name="uiFormLogic"></param>
//         /// <param name="rectTransform"></param>
//         /// <typeparam name="T"></typeparam>
//         /// <returns></returns>
//         public static List<T> FindInChildren<T>(this UIFormLogic uiFormLogic, RectTransform rectTransform)
//         {
//             var list = rectTransform.GetComponentsInChildren<T>().ToList();
//             //移除父元素
//             list.RemoveAt(0);
//             return list;
//         }
//
//
//         public static List<Transform> GetTopChild(this UIFormLogic uiFormLogic, Transform transform)
//         {
//             List<Transform> list = new List<Transform>();
//             for (int i = 0; i < transform.childCount; i++)
//             {
//                 list.Add(transform.GetChild(i));
//             }
//
//             return list;
//         }
//
//         public static void DestroyChildren(this Transform transform)
//         {
//             int childCount = transform.childCount;
//             for (int i = childCount - 1; i >= 0; i--)
//             {
//                 Object.Destroy(transform.GetChild(i).gameObject);
//             }
//         }
//
//         public static void DestroyChildrenImmediate(this Transform transform)
//         {
//             int childCount = transform.childCount;
//             for (int i = childCount - 1; i >= 0; i--)
//             {
//                 Object.DestroyImmediate(transform.GetChild(i).gameObject);
//             }
//         }
//
//         public static void ForeachTopChild(this Transform transform, Action<int, Transform> action)
//         {
//             int childCount = transform.childCount;
//
//             for (int i = 0; i < childCount; i++)
//             {
//                 action?.Invoke(i, transform.GetChild(i));
//             }
//         }
//
//         #endregion
//
//         public static void BtnClickOnce(this UIFormLogicExtend uiFormLogic,
//             Button btn)
//         {
//             btn.onClick.AddListener(() =>
//             {
//                 btn.interactable = false;
//
//                 uiFormLogic.CloseActions.Add(() => { btn.interactable = true; });
//             });
//         }
//
//         /// <summary>
//         /// 初始的时候会自动实例化固定容量的元素
//         /// </summary>
//         /// <param name="uiFormLogic"></param>
//         /// <param name="data"></param>
//         /// <param name="updateFunc"></param>
//         /// <typeparam name="T"></typeparam>
//         public static void ShowScrollView<T>(this UIFormLogic uiFormLogic, List<T> data,
//             Action<int, RectTransform> updateFunc, ScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<ScrollView>();
//
//             scrollView.SetItemSizeFunc((index) =>
//             {
//                 // 返回item的尺寸
//                 return new Vector2(scrollView.itemTemplate.rect.width, scrollView.itemTemplate.rect.height);
//             });
//
//             scrollView.SetUpdateFunc(updateFunc);
//
//             scrollView.SetItemCountFunc(() =>
//             {
//                 // 返回数据列表item的总数
//                 return data.Count;
//             });
//             scrollView.UpdateData(false);
//         }
//
//         public static void RefreshInfiniteScrollView(this UIFormLogic uiFormLogic, GridInfiniteScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<GridInfiniteScrollView>();
//             scrollView.Refresh();
//         }
//
//         public static async UniTask<UIScrollView> ShowScrollView(this UIFormLogic uiFormLogic,
//             int cnt,
//             Action<int, GameObject> UpdateDataFunc,
//             Func<int, Vector2> UpdateSizeFuncItem = null,
//             UIScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<UIScrollView>();
//             scrollView.Init(cnt, updateDataFunc: UpdateDataFunc, updateSizeFunc: UpdateSizeFuncItem, createItemCallBack: CreateItemCallBack);
//             return scrollView;
//         }
//
//         public static async UniTask<GridInfiniteScrollView> ShowInfiniteScrollView(this UIFormLogic uiFormLogic,
//             int cnt,
//             Action<int, GameObject> UpdateDataFunc,
//             Func<int, Vector2> UpdateSizeFuncItem = null,
//             GridInfiniteScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<GridInfiniteScrollView>();
//
//             scrollView.CreateItemCallBack = CreateItemCallBack;
//             scrollView.UpdateSizeFunc = UpdateSizeFuncItem;
//             scrollView.UpdateDataFunc = UpdateDataFunc;
//             scrollView.PopAllItem();
//             await scrollView.PushItem(cnt);
//
//             scrollView.Refresh();
//             return scrollView;
//         }
//
//         public static async UniTask<GridInfiniteScrollView> ShowInfiniteScrollView(this UIFormLogic uiFormLogic,
//             int cnt,
//             Action<int, GameObject> UpdateDataFunc,
//             Action<int> UpdateMaxIndexFunc,
//             Func<int, Vector2> UpdateSizeFuncItem = null,
//             GridInfiniteScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<GridInfiniteScrollView>();
//             scrollView.CreateItemCallBack = CreateItemCallBack;
//             scrollView.UpdateSizeFunc = UpdateSizeFuncItem;
//             scrollView.UpdateDataFunc = UpdateDataFunc;
//             scrollView.UpdateMaxIndexFunc = UpdateMaxIndexFunc;
//             scrollView.PopAllItem();
//             await scrollView.PushItem(cnt);
//             return scrollView;
//         }
//
//         private static  void CreateItemCallBack(GameObject obj)
//         { 
//             var uiItems = obj.GetComponentsInChildren<UIItem>(true);
//             foreach (var componentsInChild in uiItems)
//             {
//                 componentsInChild.OnInit(null);
//             }
//         }
//
//         public static void ShowScrollViewEx(this UIFormLogic uiFormLogic, int cnt,
//             Action<int, RectTransform> updateFunc, Func<int, Vector2> ItemSizeFunc = null, ScrollView scrollViewParam = null)
//         {
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<ScrollView>();
//             scrollView.SetItemSizeFunc(ItemSizeFunc);
//             scrollView.SetItemCreateFunc(CreateItemCallBack);
//             scrollView.SetUpdateFunc(updateFunc);
//             scrollView.SetItemCountFunc(() =>
//             {
//                 // 返回数据列表item的总数
//                 return cnt;
//             });
//             scrollView.UpdateData(false);
//         }
//         public static   void ShowScrollViewEx(this UIFormLogic uiFormLogic, int cnt,
//             Action<int, GameObject> updateFunc, Func<int, Vector2> ItemSizeFunc = null, ScrollView scrollViewParam = null)
//         {
//             
//             var scrollView = scrollViewParam ? scrollViewParam : GameObject.FindObjectOfType<ScrollView>();
//             scrollView.SetItemSizeFunc(ItemSizeFunc);
//             scrollView.SetItemCreateFunc(CreateItemCallBack);
//
//             scrollView.SetUpdateFunc((index, item) => { updateFunc(index, item.gameObject); });
//             scrollView.SetItemCountFunc(() =>
//             {
//                 // 返回数据列表item的总数
//                 return cnt;
//             });
//             
//             scrollView.UpdateData(true);
//    
//         }
//
//         // 创建一个方法CreateMaster用于创建带有Button和遮挡Image的GameObject
//         public static GameObject CreateMask(this UIFormLogic uiFormLogic, UnityAction action = null,
//             UnityAction targetAction = null, RectTransform target = null, Camera uiCamera = null, bool isNeedBtn = true)
//         {
//             // 创建GameObject和Button
//             GameObject newGameObject = new GameObject("NewMask");
//
//             newGameObject.AddComponent<RectTransform>();
//             newGameObject.AddComponent<CanvasRenderer>();
//             newGameObject.transform.SetParent(uiFormLogic.transform);
//             newGameObject.transform.localScale = Vector3.one;
//             newGameObject.transform.localPosition = Vector3.zero;
//             // 添加Image组件作为遮挡
//             Image image = newGameObject.AddComponent<Image>();
//             image.color = new Color(0, 0, 0, 0); // 设置透明色
//
//             // 设置Image全屏
//             image.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
//             image.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
//             image.GetComponent<RectTransform>().offsetMin = Vector2.zero;
//             image.GetComponent<RectTransform>().offsetMax = Vector2.zero;
//
//             // 设置Image在UI最下面
//             image.transform.SetAsLastSibling();
//
//             if (isNeedBtn)
//             {
//                 var btn = newGameObject.AddComponent<Button>();
//                 // if (action != null)
//                 // {
//                 //     btn.onClick.AddListener(action);
//                 // }
//                 //
//                 // btn.onClick.AddListener(() => { newGameObject.gameObject.SetActive(false); });
//
//                 btn.onClick.AddListener(() =>
//                 {
//                     if (target == null || !RectTransformUtility.RectangleContainsScreenPoint(target, Input.mousePosition, uiCamera))
//                     {
//                         action?.Invoke();
//                         newGameObject.gameObject.SetActive(false);
//                     }
//                     else
//                     {
//                         GameUtility.IsClickTarget(target.gameObject);
//                         targetAction?.Invoke();
//                         PassEvent(ExecuteEvents.pointerClickHandler);
//                     }
//                 });
//             }
//
//
//             newGameObject.gameObject.SetActive(false);
//             return newGameObject;
//         }
//
//         private static void PassEvent<T>(ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
//         {
//             PointerEventData data = new PointerEventData(EventSystem.current)
//             {
//                 position = Input.mousePosition
//             };
//             List<RaycastResult> results = new List<RaycastResult>();
//             EventSystem.current.RaycastAll(data, results);
//             GameObject current = data.pointerCurrentRaycast.gameObject;
//             if (current == null)
//             {
//                 current = results.FirstOrDefault().gameObject;
//             }
//
//             for (int i = 0; i < results.Count; i++)
//             {
//                 if (current != results[i].gameObject)
//                 {
//                     ExecuteEvents.Execute(results[i].gameObject, data, function);
//                     break;
//                 }
//             }
//         }
//
//         public static void  RemoveButtonDisabledColor(this UIFormLogic uiFormLogic)
//         {
//             Button[] buttons = uiFormLogic.GetComponentsInChildren<Button>(true);
//             var colors = ColorBlock.defaultColorBlock;
//             colors.disabledColor=Color.white;
//             foreach (var button in buttons)
//             {
//                 button.colors = colors;
//             }
//         }
//
//         #region 动画
//
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="button"></param>
//         /// <param name="OnEndSetInteractable">结束时打开交互，可点击</param>
//         public static void AddButtonAnimation(this Button button, bool OnEndSetInteractable = true)
//         {
//             button.onClick.AddListener(async () =>
//             {
//                 button.interactable = false;
//                 //点击动画
//                 await button.transform.DOPunchScale(new Vector3(-0.2f, -0.2f, 0), 0.4f, 12, 0.5f);
//
//                 if (OnEndSetInteractable)
//                 {
//                     button.interactable = true;
//                 }
//             });
//         }
//
//         public static void AnimationRotate(this UIFormLogicExtend uiFormLogic, RectTransform rectTransform)
//         {
//             rectTransform.DORotate(new Vector3(0, 0, 180), 0.6f)
//                 .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
//         }
//
//         public static void AnimationRotate(this UIFormLogicExtend uiFormLogic, Transform transform)
//         {
//             transform.DORotate(new Vector3(0, 0, 180), 0.6f)
//                 .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
//         }
//
//         #endregion
//
//
//     
//
//         /// <summary>
//         /// 设置图片透明度
//         /// </summary>
//         /// <param name="image"></param>
//         /// <param name="alpha"></param>
//         public static void SetAlpha(this Image image, float alpha)
//         {
//             var color = image.color;
//             color.a = alpha;
//             image.color = color;
//         }
//
//         public static async UniTask ShowMoveAnim(this UIFormLogicExtend uiFormLogic,
//             Transform RateTextTransform,
//             Vector3 target, float time = 0.7f)
//         {
//             var rawPosition = RateTextTransform.transform.position;
//             await RateTextTransform.DOMove(target, time).SetEase(Ease.InOutExpo);
//             RateTextTransform.gameObject.SetActive(false);
//             RateTextTransform.position = rawPosition;
//             //这里涉及到位置变化的还是不能放到CloseAction里，可能正在执行关闭动画，位置就不对了
//         }
//
//         /// <summary>
//         /// 显示倍率的动画，默认先移动到屏幕中心，然后移动到目标位置
//         /// </summary>
//         public static async UniTask ShowRateTextAnimLogic(this UIFormLogicExtend uiFormLogic,
//             Transform RateTextTransform,
//             Vector3 target, int ExtractMultiple, float time1 = 0.5f, float time2 = 0.8f)
//         {
//             if (ExtractMultiple > 1)
//             {
//                 Vector2 rawPos = RateTextTransform.position;
//
//
//                 //移动到屏幕中心
//                 await UniTask.WhenAll(
//                     uiFormLogic.AnimMove2ScreenCenter(RateTextTransform, time1).SetEase(Ease.InOutExpo).ToUniTask(),
//                     RateTextTransform.DOScale(2, time1).SetEase(Ease.InOutExpo).ToUniTask());
//                 //停留
//                 await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
//
//                 //移动到目标位置
//                 await UniTask.WhenAll(
//                     RateTextTransform.DOMove(target, time2).SetEase(Ease.OutExpo).ToUniTask(),
//                     RateTextTransform.DOScale(1, time2).SetEase(Ease.OutExpo).ToUniTask(),
//                     uiFormLogic.AnimFadeOutImp(RateTextTransform.transform, duration: time2).ToUniTask()
//                 );
//
//                 uiFormLogic.CloseActions.Add((() =>
//                 {
//                     RateTextTransform.position = rawPos;
//                     RateTextTransform.localScale = Vector3.one;
//                 }));
//             }
//         }
//
//
//         /// <summary>
//         /// 检查主页面是否存在
//         /// </summary>
//         /// <returns></returns>
//         public static bool CheckMainUIExist(this UIFormLogic uiFormLogic)
//         {
//             return GameEntry.UI.HasUIForm(AssetUtility.GetUIFormAsset("Main"));
//         }
//
//
//         /// <summary>
//         /// 返回倍率文本，倍率数值
//         /// </summary>
//         /// <param name="uiFormLogic"></param>
//         /// <returns></returns>
//         public static (string RateText, int RateNum) GetRateText(this UIFormLogic uiFormLogic)
//         {
//             var rateNum = GameEntry.PlayerLogic.PlayerData.extractMultiple;
//             return ($"x{rateNum}", rateNum);
//         }
//
//         public static bool IsShowRateTextLogic(this UIFormLogic uiFormLogic,
//             Transform RateTextTransform, TextMeshProUGUI RateText, bool alwaysShow = false, GameObject tipTextGo = null)
//         {
//             var (rateText, rateNum) = uiFormLogic.GetRateText();
//
//             if (!alwaysShow)
//             {
//                 //不为0显示倍率
//                 if (rateNum > 1)
//                 {
//                     RateTextTransform.gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     RateTextTransform.gameObject.SetActive(false);
//                 }
//             }
//
//             RateText.text = rateText;
//             
//           
//             RateTextTransform.GetComponentInChildren<Image>().GetSprite( GameEntry.CustomResource.spriteData.GetMultipleBg(rateNum));
//             //倍率提示文案的显示与倍率的显示一样
//             if (tipTextGo != null)
//             {
//                 tipTextGo.SetActive(RateTextTransform.gameObject.activeInHierarchy);
//             }
//
//             return (rateNum > 1);
//         }
//
//
//         #region 本地化
//
//         public static string GetLocalization(this UIFormLogic uiFormLogic, string key)
//         {
//             return GameEntry.Data.localization.GetLocalization(key);
//         }
//
//         #endregion
//
//         private static ConcurrentDictionary<Image, string> _CacheLoadTask = new ConcurrentDictionary<Image, string>();
//
//         // 下载网络图片，当url变了之后不会更新到界面中。避免了回收后再使用时显示了之前加载的图片。
//         public static async UniTask GetDownloadSprite(this Image image, string url)
//         {
//             image.sprite = await GameEntry.CustomResource.spriteData.DefaultSprite.Load();
//             _CacheLoadTask.AddOrUpdate(image, url, (image, oldV) => url);
//             await GameEntry.ImageDownload.LoadFromUrl(url).ContinueWith((sprite) =>
//             {
//                 try
//                 {
//                     if (string.Equals(url, _CacheLoadTask[image]))
//                     {
//                         image.sprite = sprite;
//                         _CacheLoadTask.Remove(image, out var tmp);
//                         // Debug.Log("_CacheLoadTask Remove " + tmp + " " + _CacheLoadTask.Count);
//                     }
//                 }
//                 catch (Exception e)
//                 {
//                     Console.WriteLine(e);
//                 }
//
//                 Debug.Log("_CacheLoadTask " + _CacheLoadTask.Count);
//             });
//         }
//         
//         public static async UniTask GetSprite(this Image image, FSprite fSprite)
//         {
//             await fSprite.Load((sprite => image.sprite = sprite ));
//         }   
//         public static async UniTask GetSprite(this SpriteRenderer spriteRenderer, FSprite fSprite)
//         {
//             await fSprite.Load((sprite => spriteRenderer.sprite = sprite ));
//         }
//         
//         public static async UniTask SetFontShareMaterial(this TextMeshProUGUI textMeshProUGUI, string materialName)
//         {
//             textMeshProUGUI.fontSharedMaterial = await GameEntry.CustomResource.materialManager.GetMaterial(materialName);
//         }
//         public static async UniTask SetFontMaterial(this TextMeshProUGUI textMeshProUGUI, string materialName)
//         {
//             textMeshProUGUI.fontMaterial = await GameEntry.CustomResource.materialManager.GetMaterial(materialName);
//         }
//         public static async UniTask SetImageMaterial(this Image image, string materialName)
//         {
//             image.material = await GameEntry.CustomResource.materialManager.GetMaterial(materialName);
//         }
//         public static async void SetFormatText(this TextMeshProUGUI textMeshProUGUI, string s, params object[] args)
//         {
//             var text = GameEntry.Localization.GetString(s);
//             textMeshProUGUI.text = string.Format(text, args);
//         }
//         
//         public static void ShowSlider(this RectTransform slider, RectTransform parent, float currentScore,
//             float maxScore, TextMeshProUGUI sliderText = null, bool animate = true, string formatText = null)
//         {
//             float scoreProcess = currentScore / maxScore;
//             float targetWidth = scoreProcess * parent.GetComponent<RectTransform>().sizeDelta.x;
//
//             // 检查是否已经到达指定位置
//             if (!Mathf.Approximately(slider.sizeDelta.x, targetWidth))
//             {
//                 // 如果需要动画则播放动画，否则直接设置大小
//                 if (animate)
//                 {
//                     slider.DOSizeDelta(new Vector2(targetWidth, parent.sizeDelta.y), 1);
//                 }
//                 else
//                 {
//                     slider.sizeDelta = new Vector2(targetWidth, parent.sizeDelta.y);
//                 }
//             }
//
//             // 更新文本显示
//             if (sliderText != null)
//             {
//                 if (string.IsNullOrEmpty(formatText))
//                 {
//                     sliderText.text = $"{currentScore}/{maxScore}";
//                 }
//                 else
//                 {
//                     sliderText.text = GameEntry.Localization.GetString(formatText, currentScore, maxScore);
//                 }
//             }
//         }
//
//
//         public static async UniTask<int> Open<TUIForm, T1>(this UIComponent UI, T1 t1)
//         {
//             return await OpenImp<TUIForm>(UI, uiFormAsync =>
//             {
//                 if (uiFormAsync.Logic is IUIOpenData<T1> @interface)
//                 {
//                     @interface.Open(t1);
//                 }
//             });
//         }
//
//         public static async UniTask<int> Open<TUIForm>(this UIComponent UI)
//         {
//             return await OpenImp<TUIForm>(UI);
//         }
//
//         public static async UniTask<int> Open<TUIForm, T1, T2>(this UIComponent UI, T1 t1, T2 t2)
//         {
//             return await OpenImp<TUIForm>(UI, uiFormAsync =>
//             {
//                 if (uiFormAsync.Logic is IUIOpenData<T1, T2> @interface)
//                 {
//                     @interface.Open(t1, t2);
//                 }
//             });
//         }
//
//         public static async UniTask<int> Open<TUIForm, T1, T2, T3>(this UIComponent UI, T1 t1, T2 t2, T3 t3)
//         {
//             return await OpenImp<TUIForm>(UI, uiFormAsync =>
//             {
//                 if (uiFormAsync.Logic is IUIOpenData<T1, T2, T3> @interface)
//                 {
//                     @interface.Open(t1, t2, t3);
//                 }
//             });
//         }
//
//         private static async UniTask<int> OpenImp<TUIForm>(UIComponent UI, Action<UIForm> action = null)
//         {
//             var uiFormName = typeof(TUIForm).Name;
//             if (GameEntry.CustomResource.UIConfig.OpenConfigs.TryGetValue(uiFormName, out var uiFormConfig))
//             {
//                 var path = Utility.Text.Format("Assets/Game/UIForm/{0}.prefab", uiFormConfig.Path);
//                 //判断是否已经存在
//                 //已经存在了，或者正在从ab里加载，且不允许多个实例直接返回，这样也可以防止打开页面时连点
//                 if (GameEntry.UI.HasUIForm(path) || GameEntry.UI.IsLoadingUIForm(path))
//                 {
//                     if (!uiFormConfig.AllowMultiInstance)
//                     {
//                         Log.Warning("UIForm is already exist, and not allow multi instance.");
//                         return -1;
//                     }
//                 }
//
//                 var uiFormAsync = await UI.OpenUIFormAsync(path, uiFormConfig.GroupName);
//                 //这里OnOpen先执行，可以写一些公用的逻辑
//                 action?.Invoke(uiFormAsync);
//                 return uiFormAsync.SerialId;
//             }
//
//             Log.Warning($"{uiFormName} is not exist in config. Please check config file.");
//             return -1;
//         }
//
//
//         public static async UniTask PreLoadUI<TUIForm>(this UIComponent UI)
//         {
//             var uiFormName = typeof(TUIForm).Name;
//             if (GameEntry.CustomResource.UIConfig.OpenConfigs.TryGetValue(uiFormName, out var uiFormConfig))
//             {
//                 var path = Utility.Text.Format("Assets/Game/UIForm/{0}.prefab", uiFormConfig.Path);
//
//                 await GameEntry.Resource.LoadAssetAsync<GameObject>(path);
//             }
//         }
//
//         /// <summary>
//         /// 兼容旧版UI打开方式
//         /// </summary>
//         /// <param name="uiComponent"></param>
//         /// <param name="path"></param>
//         /// <param name="groupName"></param>
//         /// <param name="data"></param>
//         /// <returns></returns>
//         public static int OpenUIImpBackward(this UIComponent uiComponent, string path, string groupName, object data = null)
//         {
//             // if (GameEntry.UI.HasUIForm(path)||GameEntry.UI.IsLoadingUIForm(path))
//             // {
//             //     //暂时写死，统一返回false
//             //     return -1;
//             // }
//
//             //通用奖励
//             if (path.Contains("CommonRewardUI.prefab"))
//             {
//                 return GameEntry.UI.OpenUIForm(path, groupName, (data));
//             }
//
//             if (GameEntry.UI.IsLoadingUIForm(path))
//             {
//                 //暂时写死，统一返回false
//                 return -1;
//             }
//
//             return GameEntry.UI.OpenUIForm(path, groupName, (data));
//         }
//
//         public static async UniTask<int> Close<TUIForm>(this UIComponent UI)
//         {
//             var uiFormName = typeof(TUIForm).Name;
//             if (GameEntry.CustomResource.UIConfig.OpenConfigs.TryGetValue(uiFormName, out var uiFormConfig))
//             {
//                 var path = Utility.Text.Format("Assets/Game/UIForm/{0}.prefab", uiFormConfig.Path);
//
//                 var uiForm = UI.GetUIForm(path);
//                 
//                 await uiForm.Logic.GetComponent<UIFormLogicExtend>().CloseUIForm();
//                 await GameEntry.UI.OnCloseAsync(uiForm.SerialId);
//             }
//
//             return -1;
//         }
//         
//        
//     }
// }