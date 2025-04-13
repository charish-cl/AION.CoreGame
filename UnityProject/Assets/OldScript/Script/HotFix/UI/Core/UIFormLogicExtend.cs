// using System;
// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using DG.Tweening;
// using DG.Tweening.Core;
// using DG.Tweening.Plugins.Options;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace AION.CoreFramework
// {
//     public class UIFormLogicExtend : UIFormLogic
//     {
//         private EventSubscriber eventSubscriber;
//
//         protected Camera uiCamera;
//
//         public UIItemLogicSubscriber UIItemLogicSubscriber;
//
//         public Canvas Canvas => GetComponentInParent<Canvas>();
//         public RectTransform CacheRectTransform => GetComponent<RectTransform>();
//
//         public UIConfigBindTool UIConfig { get; set; }
//
//         public CanvasGroup CanvasGroup
//         {
//             get { return gameObject.GetOrAddComponent<CanvasGroup>(); }
//         }
//
//         protected override void OnInit(object userData)
//         {
//             base.OnInit(userData);
//             uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
//             scaler = GetComponentInParent<CanvasScaler>();
//
//             //需要绑定的UIItem
//             if (GetComponentInChildren<UIItem>(true) != null)
//             {
//                 UIItemLogicSubscriber = ReferencePool.Acquire<UIItemLogicSubscriber>();
//                 //获取所有静态的UIItem,根据类型分组
//                 UIItemLogicSubscriber.OnInit(transform);
//             }
//
//             UIConfig = GetComponent<UIConfigBindTool>();
//
//
//             foreach (var button in GetComponentsInChildren<Button>(true))
//             {
//                 button.gameObject.GetOrAddComponent<ButtonOnClickSound>();
//             }
//
// #if WX
//             foreach (var input in GetComponentsInChildren<TMP_InputField>(true))
//             {
//                input.gameObject.GetOrAddComponent<WXInputFieldAdapter>();
//             }
// #endif
//         }
//
//         private bool HasAddUIOptimization = false;
//
//         public async UniTask<bool> CustomAsyncInitBeforeShow()
//         {
//             return false;
//         }
//
//
//         public static HashSet<string> HasPreInstance = new HashSet<string>();
//
//         protected UniTaskCompletionSource<bool> LoadSucceedCompletionSource;
//
//         protected override async void OnOpen(object userData)
//         {
//             base.OnOpen(userData);
//             LoadSucceedCompletionSource = new UniTaskCompletionSource<bool>();
//
//             transform.SetAsLastSibling();
// #if UNITY_EDITOR
//             GameEntry.Event?.Subscribe(DebugEvent.EventId, DebugAnim);
// #endif
//             EnableInteractable();
//
//             //需要绑定的UIItem
//             if (UIItemLogicSubscriber != null)
//             {
//                 //获取所有静态的UIItem,根据类型分组
//                 UIItemLogicSubscriber.OnShowStatic(userData);
//             }
//
//             if (UIConfig != null)
//             {
//                 if (UIConfig.IsLock)
//                 {
//                     GameEntry.UI.SetUIFormInstanceLocked(this.UIForm, true);
//                 }
//
//                 //预实例化的ui直接关闭就好
//                 if (!HasPreInstance.Contains(UIConfig.UIName) && UIConfig.IsPreInstance)
//                 {
//                     CloseUIForm();
//                     return;
//                 }
//
//                 //如果是弹窗
//                 if (UIConfig.UIType == CoreFramework.UIConfig.UITypes.UIPopWindow)
//                 {
//                     var darkBgComponent = gameObject.GetOrAddComponent<DarkBgComponent>();
//
//                     darkBgComponent.DarkenBG(transform);
//                 }
//
//                 //自定义加载
//                 if (UIConfig.IsCustomLoadFinish)
//                 {
//                     //隐藏面板,显示loading
//                     CanvasGroup.alpha = 0;
//
//                     DebugUIForm.ShowLoading();
//                     //等待加载完成
//                     await LoadSucceedCompletionSource.Task;
//
//                     DebugUIForm.HideLoading();
//                     //显示面板
//                     CanvasGroup.alpha = 1;
//
//                     //执行下面动画
//                 }
//
//                 //使用默认动画
//                 if (UIConfig.UseDefaultAnimation)
//                 {
//                     // await CustomAsyncInitBeforeShow();
//                     await AnimPanelMoveIn(isNeedBounce: UIConfig.IsNeedBounce);
//                     OnOpenAnimFinish();
//                 }
//             }
//         }
//
//         protected virtual void OnOpenAnimFinish()
//         {
//             if (UIConfig != null && UIConfig.CloseMainUI)
//             {
//                 MainForm.Instance.SetVisible(false);
//                 Log.Info($"{name}页面勾选了打开关闭主页");
//             }
//         }
//
//         /// <summary>
//         /// 监听
//         /// </summary>
//         public virtual void AddListener()
//         {
//         }
//         //protected UniTaskCompletionSource OpenAnimCompletionSource ;
//
//
//         protected virtual void DebugAnim(object sender, GameEventArgs e)
//         {
//             // nohting
//         }
//
//         protected override void OnClose(bool isShutdown, object userData)
//         {
//             base.OnClose(isShutdown, userData);
//
// #if UNITY_EDITOR
//             GameEntry.Event?.Unsubscribe(DebugEvent.EventId, DebugAnim);
// #endif
//             RemoveAllEvent();
//             if (eventSubscriber != null)
//             {
//                 ReferencePool.Release(eventSubscriber);
//                 eventSubscriber = null;
//             }
//
//             if (UIItemLogicSubscriber != null)
//             {
//                 UIItemLogicSubscriber.OnClose();
//             }
//
//             foreach (var closeAction in CloseActions)
//             {
//                 closeAction.Invoke();
//             }
//
//             CloseActions.Clear();
//
//             ReleaseAllReferenceObjs();
//         }
//
//         protected virtual async UniTask<bool> NeedShowShare()
//         {
//             if (GameEntry.PlayerLogic.CanShare)
//             {
//                 if (GameEntry.Guide.IsNewerThenCurProgress(GuideRecord.CompleteForceGuide)) return false;
//                 var (success, shareGameCenterInfoDto) = await GameEntry.Request.GetShardGameCenterInfoFunc().TryRequest(ensureSuccess: true);
//                 if (!success) return false;
//                 return shareGameCenterInfoDto.TodayShareCount < shareGameCenterInfoDto.MaxShareCount;
//             }
//
//             return false;
//         }
//
//         #region 事件
//
//         protected void AddEvent(int id, EventHandler<GameEventArgs> handler)
//         {
//             if (eventSubscriber == null)
//                 eventSubscriber = EventSubscriber.Create(this);
//
//             eventSubscriber.Subscribe(id, handler);
//         }
//
//         protected void RemoveEvent(int id, EventHandler<GameEventArgs> handler)
//         {
//             if (eventSubscriber != null)
//                 eventSubscriber.UnSubscribe(id, handler);
//         }
//
//         protected void RemoveAllEvent()
//         {
//             if (eventSubscriber != null)
//                 eventSubscriber.UnSubscribeAll();
//         }
//
//         #endregion
//
//         #region UIItem
//
//         public async UniTask<T> ShowItem<T>() where T : UIItem
//         {
//             return await ShowItem<T>(CacheRectTransform);
//         }
//
//         public async UniTask<T> ShowItem<T>(RectTransform parent, object data = null) where T : UIItem
//         {
//             if (UIItemLogicSubscriber == null)
//             {
//                 UIItemLogicSubscriber = ReferencePool.Acquire<UIItemLogicSubscriber>();
//             }
//
//             return await UIItemLogicSubscriber.ShowItem<T>(parent, data);
//         }
//
//         public async UniTask<T> ShowItem<T>(string path, Transform parent, object data) where T : UIItem
//         {
//             if (UIItemLogicSubscriber == null)
//             {
//                 UIItemLogicSubscriber = ReferencePool.Acquire<UIItemLogicSubscriber>();
//             }
//
//             var item = await UIItemLogicSubscriber.ShowDynamicUIItemImp<T>(path, parent, data);
//             return item;
//         }
//
//
//         public async UniTask<T> ShowItem<T>(string path, RectTransform parent, RectTransform sourceRectTransform,
//             object data) where T : UIItem
//         {
//             if (UIItemLogicSubscriber == null)
//             {
//                 UIItemLogicSubscriber = ReferencePool.Acquire<UIItemLogicSubscriber>();
//             }
//
//             var item = await UIItemLogicSubscriber.ShowItem<T>(path, parent, sourceRectTransform, data);
//             return item;
//         }
//
//         protected T GetItem<T>(int index) where T : UIItem
//         {
//             return GetGroups<T>()[index];
//         }
//
//         protected List<T> GetGroups<T>() where T : UIItem
//         {
//             string groupName = typeof(T).Name;
//             if (UIItemLogicSubscriber == null)
//             {
//                 UIItemLogicSubscriber = ReferencePool.Acquire<UIItemLogicSubscriber>();
//             }
//
//             //如果为空，默认为T GetType的名字
//             if (string.IsNullOrEmpty(groupName))
//             {
//                 groupName = typeof(T).Name;
//             }
//
//             return UIItemLogicSubscriber.GetGroups<T>(groupName);
//         }
//
//         #endregion
//
//         public async UniTask CloseUIForm(object userData = null)
//         {
//             DisableInteractable();
//             if (UIConfig != null)
//             {
//                 if (!HasPreInstance.Contains(UIConfig.UIName) && UIConfig.IsPreInstance)
//                 {
//                     HasPreInstance.Add(UIConfig.UIName);
//                     GameEntry.UI.CloseUIForm(this.UIForm, userData);
//                     return;
//                 }
//
//                 //如果是弹窗
//                 if (UIConfig.UIType == CoreFramework.UIConfig.UITypes.UIPopWindow)
//                 {
//                     var darkBgComponent = gameObject.GetOrAddComponent<DarkBgComponent>();
//
//                     darkBgComponent.DarkBgFadeOut(transform);
//                 }
//
//                 if (UIConfig.UseDefaultAnimation)
//                 {
//                     if (UIConfig.CloseMainUI)
//                     {
//                         MainForm.Instance.SetVisible(true);
//                         Log.Info($"{name}页面勾选了打开关闭主页,现在关闭页面打开主页");
//                     }
//
//                     await AnimPanelMoveOut(isNeedBounce: UIConfig.IsNeedBounce);
//                 }
//             }
//
//             GameEntry.UI.CloseUIForm(this.UIForm, userData);
//         }
//
//         /// <summary>
//         /// 打开页面的时候关闭主页
//         /// </summary>
//         protected void CloseMainUI()
//         {
//             if (UIConfig == null || (UIConfig != null && UIConfig.CloseMainUI && !UIConfig.UseDefaultAnimation))
//             {
//                 MainForm.Instance.SetVisible(true);
//                 Log.Info($"{name}页面勾选了打开关闭主页");
//             }
//         }
//
//         /// <summary>
//         /// 关闭页面的时候打开主页
//         /// </summary>
//         protected void OpenMainUI()
//         {
//             if (UIConfig == null || (UIConfig != null && UIConfig.CloseMainUI && !UIConfig.UseDefaultAnimation))
//             {
//                 MainForm.Instance.SetVisible(true);
//                 Log.Info($"{name}页面勾选了打开关闭主页,现在关闭页面打开主页");
//             }
//         }
//
//         public async UniTask WatchADGetBill(int? getBillNum)
//         {
//             if (getBillNum == null || getBillNum <= 0) return;
//
//             await this.ShowParticle(Vector3.zero, (ulong)getBillNum.Value, RewardType.Bill);
//             GameEntry.PlayerLogic.ModifyBill((ulong)getBillNum);
//         }
//
//         public async UniTask<bool> WatchAD()
//         {
//             bool watchAD = await GameEntry.PlatformComponent.PlayAd();
//             if (!watchAD)
//             {
//                 GameEntry.Request.AddErrorMessage(GameEntry.Localization.GetString("AD.Tip"));
//             }
//
//             return watchAD;
//         }
//
//         #region 动画
//
//         public List<Action> CloseActions = new List<Action>();
//
//         public TweenerCore<float, float, FloatOptions> AnimFadeInImp(Transform tr, float startValue = 0,
//             float endValue = 1, float duration = 1)
//         {
//             var canvasGroup = tr.gameObject.GetOrAddComponent<CanvasGroup>();
//             canvasGroup.alpha = startValue;
//             return canvasGroup.DOFade(endValue, duration);
//         }
//
//         public TweenerCore<float, float, FloatOptions> AnimFadeOutImp(Transform tr, float startValue = 1,
//             float endValue = 0, float duration = 1)
//         {
//             var canvasGroup = tr.gameObject.GetOrAddComponent<CanvasGroup>();
//             canvasGroup.alpha = startValue;
//             CloseActions.Add((() => { canvasGroup.alpha = startValue; }));
//             return canvasGroup.DOFade(endValue, duration);
//         }
//
//         /// <summary>
//         /// UI面板打开动画
//         /// </summary>
//         /// <param name="duration"></param>
//         /// <returns></returns>
//         public TweenerCore<Vector2, Vector2, VectorOptions> AnimPanelMoveIn(float duration = 0.3f,
//             bool isNeedBounce = true)
//         {
//             return AnimMoveIn(transform.GetComponent<RectTransform>(), duration: duration, isNeedBounce: isNeedBounce);
//         }
//
//         /// <summary>
//         /// UI面板关闭动画
//         /// </summary>
//         /// <param name="duration"></param>
//         /// <returns></returns>
//         public TweenerCore<Vector2, Vector2, VectorOptions> AnimPanelMoveOut(float duration = 0.3f,
//             bool isNeedBounce = true)
//         {
//             return AnimMoveOut(transform.GetComponent<RectTransform>(), duration: duration, isNeedBounce: isNeedBounce);
//         }
//
//         public void BindTipUIAnim(Button openButton, Button closeButton, Transform tipUI)
//         {
//             RectTransform rectTransform = tipUI.transform.GetChild(0).GetComponent<RectTransform>();
//             openButton.onClick.AddListener(() =>
//             {
//                 tipUI.gameObject.SetActive(true);
//                 AnimMoveIn(rectTransform);
//             });
//             closeButton.onClick.AddListener(() =>
//             {
//                 AnimMoveOut(rectTransform).onComplete += () =>
//                 {
//                     tipUI.gameObject.SetActive(false);
//                     rectTransform.anchoredPosition = Vector2.zero;
//                 };
//             });
//         }
//
//         /// <summary>
//         /// 已经有buff了之后添加时间的动画
//         /// </summary>
//         /// <param name="isHasBuffInfo"></param>
//         /// <param name="ApplyTime"></param>
//         /// <param name="Time"></param>
//         public async UniTask BuffTimeApplyTimeAnim(IsHasBuffInfo isHasBuffInfo, TextMeshProUGUI ApplyTime,
//             Vector3 targetLocalPos)
//         {
//             ApplyTime.gameObject.SetActive(true);
//             ApplyTime.transform.localScale = Vector3.zero;
//             ApplyTime.transform.localPosition = new Vector3(targetLocalPos.x, targetLocalPos.y - 250, targetLocalPos.z);
//             ApplyTime.text = "+" + MathUtility.Second2TimeStr(isHasBuffInfo.applyEndTime * 60);
//             var s = DOTween.Sequence();
//             s.Append(ApplyTime.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f).SetEase(Ease.OutBack));
//             s.Append(ApplyTime.transform.DOLocalMove(targetLocalPos, 0.5f));
//             s.Insert(0.6f, ApplyTime.transform.DOScale(Vector3.zero, 0.2f));
//             await s;
//             ApplyTime.gameObject.SetActive(false);
//         }
//
//
//         public TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveOut(RectTransform rectTransform,
//             float duration = 0.3f,
//             float easeOvershootOrAmplitude = 0, bool top2Bottom = false, bool isNeedBounce = false)
//         {
//             var y = rectTransform.anchoredPosition.y;
//
//             CloseActions.Add(() =>
//             {
//                 if (rectTransform == null)
//                 {
//                     return;
//                 }
//
//                 rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
//             });
//             //之前的
//             //return AnimMoveImp(rectTransform, duration).SetEase(Ease.InOutBack);
//
//             Ease ease = isNeedBounce ? Ease.InBack : Ease.InCirc;
//
//             return AnimMoveImp(rectTransform, duration, top2Bottom: top2Bottom).SetEase(ease);
//         }
//
//         /// <summary>
//         /// 动画移动
//         /// </summary>
//         /// <param name="rectTransform">物体</param>
//         /// <param name="duration">间隔</param>
//         /// <param name="easeOvershootOrAmplitude">回弹系数</param>
//         /// <param name="top2Bottom">顶到底</param>
//         /// <param name="isNeedBounce">是否需要回弹</param>
//         /// <returns></returns>
//         public TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveIn(RectTransform rectTransform,
//             float duration = 0.3f,
//             float easeOvershootOrAmplitude = 0, bool top2Bottom = false, bool isNeedBounce = false)
//         {
//             var tween = AnimMoveImp(rectTransform, duration, false, top2Bottom);
//             tween.SetEase(isNeedBounce ? Ease.OutBack : Ease.OutExpo);
//             tween.easeOvershootOrAmplitude = easeOvershootOrAmplitude;
//             tween.easePeriod = duration;
//
//             return tween;
//         }
//
//         /// <summary>
//         ///  UI移动的方法
//         /// </summary>
//         /// <param name="rectTransform"></param>
//         /// <param name="duration"></param>
//         /// <param name="isOut">是否移出</param>
//         /// <returns></returns>
//         TweenerCore<Vector2, Vector2, VectorOptions> AnimMoveImp(RectTransform rectTransform, float duration = 0.3f,
//             bool isOut = true, bool top2Bottom = false)
//         {
//             var anchoredPosition = rectTransform.anchoredPosition;
//             // var bottomDistance = anchoredPosition.y -= GetBottomDistance(rectTransform);
//
//             float distance;
//             if (top2Bottom)
//             {
//                 distance = anchoredPosition.y + GetTopDistance(rectTransform);
//             }
//             else
//             {
//                 distance = anchoredPosition.y - GetBottomDistance(rectTransform);
//             }
//
//             if (isOut)
//             {
//                 return rectTransform.DOAnchorPosY(distance, duration);
//             }
//
//             return rectTransform.DOAnchorPosY(distance, duration).From();
//         }
//
//         public float GetTopDistance(RectTransform rectTransform)
//         {
//             // 获取UI元素在屏幕上的边界
//             Vector3[] corners = new Vector3[4];
//             //Get the corners of the calculated rectangle in world space.
//             rectTransform.GetWorldCorners(corners);
//             // 计算UI元素上面的Y坐标
//             float topY = corners[0].y;
//             //在这种模式下，UI元素是相对于摄像机进行渲染的，但是其世界坐标并不直接映射到屏幕坐标上。
//             //因此，使用rectTransform.GetWorldCorners(corners)获取的坐标是相对于Canvas所在的坐标系的世界坐标，而不是屏幕坐标。
//             //如果想要获取UI元素在屏幕上的边界坐标，需要将UI元素的世界坐标转换为屏幕坐标，可以使用Camera的WorldToScreenPoint方法。
//             if (GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera)
//             {
//                 // 转换UI元素的世界坐标为屏幕坐标
//                 // bottomY = Camera.main.WorldToScreenPoint(corners[1]).y;
//                 topY = uiCamera.WorldToScreenPoint(corners[0]).y;
//             }
//             //这种模式，UI元素的世界坐标与屏幕坐标是一致的
//             else if (GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay)
//             {
//             }
//             //在这种模式下，UI元素是直接放置在世界空间中的，其坐标是真实的世界坐标。
//             else if (GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace)
//             {
//             }
//
//             float radio = 0;
//
//
//             // 计算屏幕底部的Y坐标
//             float screenHeight = Screen.height;
//             float screenBottomY = 0; // 屏幕底部Y坐标
//
//             // 计算UI到屏幕底部的距离
//             float distanceToTop = screenHeight - topY;
//
//             return distanceToTop * GetCanvasScaleRadio();
//         }
//
//         /// <summary>
//         /// 获取UI上边缘（左上角）顶部到屏幕底部的距离
//         /// </summary>
//         /// <returns></returns>
//         public float GetBottomDistance(RectTransform rectTransform)
//         {
//             // 获取UI元素在屏幕上的边界
//             Vector3[] corners = new Vector3[4];
//             //Get the corners of the calculated rectangle in world space.
//             rectTransform.GetWorldCorners(corners);
//
//
//             // 计算UI元素上面的Y坐标
//             float bottomY = corners[1].y;
//             //在这种模式下，UI元素是相对于摄像机进行渲染的，但是其世界坐标并不直接映射到屏幕坐标上。
//             //因此，使用rectTransform.GetWorldCorners(corners)获取的坐标是相对于Canvas所在的坐标系的世界坐标，而不是屏幕坐标。
//             //如果想要获取UI元素在屏幕上的边界坐标，需要将UI元素的世界坐标转换为屏幕坐标，可以使用Camera的WorldToScreenPoint方法。
//             if (GetComponentInParent<Canvas>()?.renderMode == RenderMode.ScreenSpaceCamera)
//             {
//                 // 转换UI元素的世界坐标为屏幕坐标
//                 bottomY = uiCamera.WorldToScreenPoint(corners[1]).y;
//             }
//             //这种模式，UI元素的世界坐标与屏幕坐标是一致的
//             else if (GetComponentInParent<Canvas>()?.renderMode == RenderMode.ScreenSpaceOverlay)
//             {
//             }
//             //在这种模式下，UI元素是直接放置在世界空间中的，其坐标是真实的世界坐标。
//             else if (GetComponentInParent<Canvas>()?.renderMode == RenderMode.WorldSpace)
//             {
//             }
//
//
//             float screenBottomY = 0; // 屏幕底部Y坐标
//
//             // 计算UI到屏幕底部的距离
//             float distanceToBottom = bottomY - screenBottomY;
//
//
//             return GetCanvasScaleRadio() * distanceToBottom;
//         }
//
//         private CanvasScaler scaler;
//
//         public float GetCanvasScaleRadio()
//         {
//             // var scaler = GetComponentInParent<CanvasScaler>();
//
//             if (scaler == null)
//             {
//                 return 0;
//             }
//
//             float radio = 0;
//
//             float screenHeight = Screen.height;
//             float screenWidth = Screen.width;
//
//             if (scaler.matchWidthOrHeight == 0)
//             {
//                 radio = scaler.referenceResolution.x * 1f / screenWidth * 1f;
//             }
//             else if (scaler.matchWidthOrHeight == 1)
//             {
//                 radio = scaler.referenceResolution.y * 1f / screenHeight * 1f;
//             }
//             // Debug.Log(radio);
//             // Debug.Log(
//             //     $"screenHeight : {screenHeight} screenWidth:{screenWidth} ");
//
//             return radio;
//         }
//
//         public Vector3 ScreenCenter()
//         {
//             Vector3 screenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
//             // 将屏幕空间的位置转换为世界空间的位置
//             return uiCamera.ScreenToWorldPoint(screenPosition);
//         }
//
//         public TweenerCore<Vector3, Vector3, VectorOptions> AnimMove2ScreenCenter(Transform uiElement, float duration)
//         {
//             Vector3 screenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
//             // 将屏幕空间的位置转换为世界空间的位置
//             Vector3 worldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
//
//             // Log.Info($"worldPosition:{worldPosition}");
//             // 设置UI元素的位置
//             return uiElement.DOMove(new Vector3(worldPosition.x, worldPosition.y, 0), duration);
//         }
//
//
//         //参数是动画,播放时DisableInteractable,播放完EnableInteractable
//
//
//         protected async UniTask PlayAnim(Func<UniTask> task)
//         {
//             DisableInteractable();
//             await task.Invoke();
//             EnableInteractable();
//         }
//
//         private LoadingUIItem LoadingUIItem;
//
//         public async UniTask<LoadingUIItem> ShowLoading(Transform needHide = null)
//         {
//             if (needHide != null)
//             {
//                 needHide.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0;
//             }
//
//             LoadingUIItem = await ShowItem<LoadingUIItem>();
//             return LoadingUIItem;
//         }
//
//         public void CloseLoading(Transform needShow = null)
//         {
//             Debug.Log(LoadingUIItem.gameObject.activeSelf);
//             LoadingUIItem.Hide();
//             Debug.Log("隐藏后" + LoadingUIItem.gameObject.activeSelf);
//             if (needShow != null)
//             {
//                 needShow.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 1;
//             }
//         }
//
//         #endregion
//
//         private int cloudLoadingUIId;
//
//         public async UniTask EnterCloudLoading(Action<Transform> openAction = null)
//         {
//             cloudLoadingUIId = await GameEntry.UI.Open<CloudLoadingUIForm>();
//             UIForm uiForm = GameEntry.UI.GetUIForm(cloudLoadingUIId);
//             cloudLoadingUIId = uiForm.SerialId;
//
//             openAction?.Invoke(uiForm.transform.Find("UserPanel"));
//
//             CloudLoadingUIForm cloudLoadingUIForm = uiForm.GetComponent<CloudLoadingUIForm>();
//             await cloudLoadingUIForm.Enter();
//         }
//
//         public async UniTask ExitCloudLoading(Action ExitAction = null)
//         {
//             UIForm uiForm = GameEntry.UI.GetUIForm(cloudLoadingUIId);
//             cloudLoadingUIId = uiForm.SerialId;
//             CloudLoadingUIForm cloudLoadingUIForm = uiForm.GetComponent<CloudLoadingUIForm>();
//             await cloudLoadingUIForm.Exit();
//             ExitAction?.Invoke();
//         }
//
//
//         public ParticleItem ShowParticle(EnumParticle enumParticle, Vector3 Position)
//         {
//             var patricleItem = GameEntry.CustomPool.ShowParticle(enumParticle, Position, userData: gameObject, transform, onShowCompletAction:
//                 (e) =>
//                 {
//                     if (e.IsLoop)
//                     {
//                         CloseActions.Add(() => { GameEntry.CustomPool.Particle.HideItem(e); });
//                     }
//                 });
//             return patricleItem;
//         }
//
//         public ParticleItem ShowParticle(EnumParticle enumParticle, Transform parent, Vector3 Position = default)
//         {
//             var patricleItem = GameEntry.CustomPool.ShowParticle(enumParticle, Position, userData: gameObject, parent: parent, onShowCompletAction:
//                 (e) =>
//                 {
//                     if (e.IsLoop)
//                     {
//                         CloseActions.Add(() => { GameEntry.CustomPool.Particle.HideItem(e); });
//                     }
//                 });
//             return patricleItem;
//         }
//
//        
//
//         #region 交互
//
//         protected void DisableInteractable()
//         {
//             CanvasGroup.interactable = false;
//         }
//
//         protected void EnableInteractable()
//         {
//             CanvasGroup.interactable = true;
//         }
//
//         #endregion
//
//
//       
//
//        
//
//
//         #region UIConfig
//
//         //获取上一个打开的界面
//         public int GetPreUIFormInUIGroup()
//         {
//             var uiGroup = GameEntry.UI.GetUIGroup(UIForm.UIGroup.Name);
//
//             if (uiGroup.UIFormCount == 1)
//             {
//                 throw new Exception("当前界面是栈底界面，没有上一个界面");
//             }
//
//             var allUIForms = uiGroup.GetAllUIForms();
//             //排除自己
//             return allUIForms.First(e => e.SerialId != UIForm.SerialId).SerialId;
//         }
//
//
//         public enum EnumTimerCloseAction
//         {
//             DirectClose,
//
//             RefreshByReOpen,
//
//             Custom
//         }
//         private Dictionary<TextMeshProUGUI, UITimer> timers = new Dictionary<TextMeshProUGUI, UITimer>();
//
//         public UITimer CreateTimer(TextMeshProUGUI timeTxt, long endTime, UIFormLogicExtend.EnumTimerCloseAction timerCloseAction = EnumTimerCloseAction.DirectClose, string format = null)
//         {
//             UITimer uiTimer = null;
//             switch (timerCloseAction)
//             {
//                 case EnumTimerCloseAction.DirectClose:
//                     uiTimer = UITimer.Create(timeTxt, endTime, () => { CloseUIForm(); });
//                     CreateReferenceObj<UITimer>(uiTimer);
//                     break;
//                 case EnumTimerCloseAction.RefreshByReOpen:
//                     uiTimer = UITimer.Create(timeTxt, endTime, () => { OnOpen(null); });
//                     CreateReferenceObj<UITimer>(uiTimer);
//                     break;
//                 case EnumTimerCloseAction.Custom:
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(timerCloseAction), timerCloseAction, null);
//             }
//             timers.TryAdd(timeTxt, uiTimer);
//             uiTimer.StartTimer(formatStr: format).Forget();
//             return uiTimer;
//         }
//         public UITimer CreateOrResetTimer(TextMeshProUGUI timeTxt,long endTime,Action endAction,string format = null)
//         {
//             if (timers.TryGetValue(timeTxt, out var uiTimer))
//             {
//                 uiTimer.ResetTimer(timeTxt,endTime, endAction, format);
//                 return uiTimer;
//             }
//             return CreateTimer(timeTxt, endTime, endAction, format);
//         }
//         public UITimer CreateTimer(TextMeshProUGUI timeTxt, long endTime, Action endAction, string format = null, object anotherNeedFormat = null)
//         {
//             var uiTimer = UITimer.Create(timeTxt, endTime, endAction);
//             CreateReferenceObj<UITimer>(uiTimer);
//             uiTimer.StartTimer(format, anotherNeedFormat).Forget();
//             return uiTimer;
//         }
//
//         #endregion
//
//         [HideInInspector] public GameObject Mask;
//
//         public async UniTask WaitMaskClick(GameObject curItem, Button btn)
//         {
//             if (Mask == null)
//             {
//                 Mask = this.CreateMask();
//             }
//
//             Mask.SetActive(true);
//
//             Log.Info("WaitMaskClick ");
//             //令物体显示在最上层  
//             Canvas itemCanvas = curItem.AddComponent<Canvas>();
//             //开启层级排序
//             itemCanvas.overrideSorting = true;
//             //设置层级
//             itemCanvas.sortingOrder = 100;
//
//             curItem.AddComponent<GraphicRaycaster>();
//
//             //等待点击遮罩或者按钮
//             await UniTask.WhenAny(Mask.GetComponent<Button>().OnClickAsync(), btn.OnClickAsync());
//
//             Mask.SetActive(false);
//
//             Destroy(curItem.GetComponent<GraphicRaycaster>());
//             Destroy(curItem.GetComponent<Canvas>());
//         }
//
//         public void ShowMask(GameObject targetRect = null)
//         {
//             if (Mask == null)
//             {
//                 Mask = this.CreateMask(isNeedBtn: false);
//             }
//
//             Mask.transform.SetAsLastSibling();
//             Mask.SetActive(true);
//             Log.Info("ShowMask! ");
//
//
//             if (targetRect == null)
//             {
//                 return;
//             }
//
//             //令物体显示在最上层  
//             Canvas itemCanvas = targetRect.GetOrAddComponent<Canvas>();
//             //开启层级排序
//             itemCanvas.overrideSorting = true;
//             //设置层级
//             itemCanvas.sortingOrder = 100;
//
//             targetRect.GetOrAddComponent<GraphicRaycaster>().enabled = true;
//         }
//
//         protected async UniTask GetShareReward(Toggle toggle, Share2GameCenterType type, Texture2D texture2D)
//         {
//             if (GameEntry.Guide.IsNewerThenCurProgress(GuideRecord.CompleteForceGuide)) return;
//             if (toggle == null || !toggle.isOn || !toggle.gameObject.activeInHierarchy) return;
//             bool result = true;
//             try
//             {
// #if WX && !UNITY_EDITOR
//             result = await GameEntry.WXComponent.Share2GameCenter(type, texture2D);
// #endif
//             }
//             catch (Exception e)
//             {
//                 Debug.Log(" GetShareReward error " + e.Message);
//             }
//             if (result)
//             {
//                 var (success, rewards) = await GameEntry.Request.GetShareGameCenterRewardFunc(type).TryRequest(ensureSuccess: true);
//                 if (!success) return;
//                 await GameEntry.UI.OnCloseAsync(UIHelper.OpenUIForm_CommonRewardUI(rewards));
//             }
//         }
//
//         public void CloseMask(GameObject targetRect = null)
//         {
//             Log.Info("CloseMask! ");
//             if (Mask != null)
//             {
//                 Mask.SetActive(false);
//                 if (targetRect == null) return;
//                 if (targetRect.GetComponent<Canvas>())
//                 {
//                     targetRect.GetComponent<Canvas>().overrideSorting = false;
//                 }
//             }
//         }
//
//         public async UniTask<Texture2D> CaptureScreenshot()
//         {
//             Camera mainCamera = Camera.main;
//             var data = mainCamera.GetUniversalAdditionalCameraData();
//             Camera watermarkCamera = GameObject.FindGameObjectWithTag("Watermark").GetComponent<Camera>();
//             data.cameraStack.Add(watermarkCamera);
//             await UniTask.WaitForEndOfFrame(this);
//             var texture = ScreenCapture.CaptureScreenshotAsTexture();
//             data.cameraStack.Remove(watermarkCamera);
//             return texture;
//         }
//     }
// }