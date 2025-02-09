#if WX
using MonoPoly.JsLib;
using SystemInfo = WeChatWASM.SystemInfo;
using WeChatWASM;
#endif

namespace AION.CoreFramework
{
    public class WXComponent
    {
        // 需要使用宏，不然其他平台编译会因为找不到wx相关内容而报错

        public void ClearWXData()
        {
            // WX.HideLoadingPage();
        }


#if WX
        // 在设置界面推出后，用于销毁这个按钮
        private WXUserInfoButton getuserInfoButton;
#if !UNITY_EDITOR

        public async UniTask<bool> DeleteGameFrameworkVersion()
        {
            return await GFBuiltin.PlatformComponent.FileSystem
                .DeleteIfExist("__GAME_FILE_CACHE/StreamingAssets/GameFrameworkVersion.dat");
        }
        
        public async UniTask<bool> CheckExists()
        {
            return await GFBuiltin.PlatformComponent.FileSystem
                .CheckExists("__GAME_FILE_CACHE/StreamingAssets/GameFrameworkVersion.dat");
        }
#endif

        public void ShowNetworkError(Action  confirmCallback)
        {
            WX.ShowModal( new ShowModalOption()
            {
                title = "网络异常",
                content = "下载失败，请检查网络设置",
                showCancel = true,
                confirmText = "重试",
                cancelText = "取消",
                success = (res) =>
                {
                    if (res.confirm)
                    {
                        confirmCallback();
                    }
                    else
                    {
                        WX.ExitMiniProgram(new ExitMiniProgramOption());
                    }
                 
                }
            });
        }
        public async UniTask InitSdk()
        {
            
            UniTaskCompletionSource<bool> task = new UniTaskCompletionSource<bool>();
            
            
            WX.InitSDK((code) =>
            {
              
                task.TrySetResult(true);
                SystemInfo = WX.GetSystemInfoSync();

                RejectMessageSubScribeCnt = new StorageInt("SubScribeMessageTipCnt", 2);
                
                Log.Info("今日消息订阅次数");
                
                WX.OnShow(result =>
                {
                    //切换到前台时执行，游戏启动时执行一次
                    Log.Info("切换到前台");

                    GFBuiltin.Event.Fire(this, SwitchToFrontEvent.Create());
                });

                WX.OnTouchEnd((res) =>
                {
                   WXOnTouchEnd(res);
                    
                });

                WindowInfo = new WindowInfo { safeArea = new SafeArea() };
            });
            
            WX.OnShareAppMessage(new WXShareAppMessageParam()
            {
                title = "重返荣耀",
                imageUrlId= "2j8udh5yRdi8G2d4tswMBA==",
                imageUrl = "https://mmocgame.qpic.cn/wechatgame/ibnhd6k8vPow3lVGT5D6xESjiba56ylp8cC3xECYsia2liaqvJrho50DV7RvBEK7xo0Z/0"
            });
            /**
        * 群排行榜功能需要配合 WX.OnShow 来使用，整体流程为：
        * 1. WX.UpdateShareMenu 分享功能；
        * 2. 监听 WX.OnShow 回调，如果存在 shareTicket 且 query 里面带有启动特定 query 参数则为需要展示群排行的场景
        * 3. 调用 WX.ShowOpenData 和 WX.GetOpenDataContext().PostMessage 告知开放数据域侧需要展示群排行信息
        * 4. 开放数据域调用 wx.getGroupCloudStorage 接口拉取获取群同玩成员的游戏数据
        * 5. 将群同玩成员数据绘制到 sharedCanvas
        */
            WX.OnShow(( res) =>
            {
                string shareTicket = res.shareTicket;
                Dictionary<string, string> query = res.query;

                if (!string.IsNullOrEmpty(shareTicket) && query != null && query["minigame_action"] == "show_group_list")
                {
                    OpenDataMessage msgData = new OpenDataMessage();
                    msgData.type = "showGroupFriendsRank";
                    msgData.shareTicket = shareTicket;

                    string msg = JsonUtility.ToJson(msgData);

                    ShowOpenData();
                    WX.GetOpenDataContext().PostMessage(msg);
                }
            });
            await task.Task;
        }

        private  void  WXOnTouchEnd(OnTouchStartListenerResult res)
        {
            // Log.Info("WXOnTouchEnd");
            StartCoroutine(RequestSubscribeSystemMessage(RequestSubscribeMessageTmplId));
        }


        private void CreateGameClubButton()
        {
            GameClubButton = WX.CreateGameClubButton(new WXCreateGameClubButtonParam()
            {
                type = GameClubButtonType.image,
                text = "游戏圈",
                icon = GameClubButtonIcon.white,
                style = new GameClubButtonStyle()
                {
                }
            });
        }

        // get wx code
        public async UniTask<string> GetWXCode()
        {
            UniTaskCompletionSource<string> task = new UniTaskCompletionSource<string>();
            WX.Login(new LoginOption()
            {
                success = (res) =>
                {
                    Log.Info("1.Login success" + JsonUtility.ToJson(res));
                    task.TrySetResult(res.code);
                },
                fail = (res) =>
                {
                    Log.Info("Login fail" + JsonUtility.ToJson(res));
                    task.TrySetResult(null);
                }
            });
            return await task.Task;
        }

        private async UniTask<bool> CheckHasUserInfoPermission()
        {
            UniTaskCompletionSource<bool> task = new UniTaskCompletionSource<bool>();
            Log.Info("需要授权.获取用户信息");
            //查看授权
            WX.GetSetting(new GetSettingOption()
            {
                complete = e =>
                {
                    //获取完成
                    Log.Info("2.获取设置完成" + JsonUtility.ToJson(e));
                },

                fail = (res) =>
                {
                    Log.Info("2.获取设置失败" + JsonUtility.ToJson(res));
                    task.TrySetResult(false);
                },
                success = aa =>
                {
                    if (!aa.authSetting.ContainsKey("scope.userInfo") || !aa.authSetting["scope.userInfo"])
                    {
                        //《三、调起授权》
                        Log.Info("2.调起授权");
                        task.TrySetResult(false);
                    }
                    else
                    {
                        //《四、获取用户信息》
                        Log.Info("2.获取用户信息");
                        task.TrySetResult(true);
                    }
                }
            });
            return await task.Task;
        }

        // 需要提前判断具有获取用户信息的权限
        private async UniTask<(string iv, string encryptedData)> GetUserInfo()
        {
            UniTaskCompletionSource<(string iv, string encryptedData)> task =
                new UniTaskCompletionSource<(string iv, string encryptedData)>();
            WX.GetUserInfo(new GetUserInfoOption()
            {
                withCredentials = true,
                lang = "zh_CN",
                success = (data) =>
                {
                    Log.Info($"3.当前用户 {data.userInfo.nickName} data: {data}");
                    task.TrySetResult((data.iv, data.encryptedData));
                },
                fail = e =>
                {
                    Log.Info("3.获取个人信息失败");
                    task.TrySetResult((null, null));
                }
            });
            return await task.Task;
        }

        /// <summary>
        /// 没有授权过，就弹出授权弹窗让用户再点一次。
        /// 授权过就获取后直接返回用户信息
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="callback"> Invoke(code, data.encryptedData, data.iv);</param>
        public async UniTask GetInfoOrShowUserInfoButtonUI(Action showGetUserInfoUI,
            Action<string, string, string> callback)
        {
            await GetWXCode().ContinueWith(async (code) =>
            {
                if (await CheckHasUserInfoPermission())
                {
                    // 如果能获取就获取，然后返回。如果获取失败，就创建按钮来获取。
                    var userInfo = await GetUserInfo();
                    if (!string.IsNullOrEmpty(userInfo.iv) && !string.IsNullOrEmpty(userInfo.encryptedData))
                    {
                        callback?.Invoke(code, userInfo.encryptedData, userInfo.iv);
                        return;
                    }
                }

                // 没有授权过或获取失败，就需要弹获取用户信息的弹窗
                CreateUserInfoButton(code, callback);
                showGetUserInfoUI?.Invoke();
            });
        }

        // 创建授权按钮方法
        // callback?.Invoke(code, data.encryptedData, data.iv);
        private void CreateUserInfoButton(string code, Action<string, string, string> callback)
        {
            //调用请求获取用户信息
            // 直接创建右边一半的空间都可以点击
            DestroyUserInfoButton();
            getuserInfoButton = WX.CreateUserInfoButton(
                Screen.width / 2, 0, width: Screen.width / 2, height: Screen.height,
                "zh_CN",
                true
            );
            Debug.Log("show get user info button");
            getuserInfoButton.Show();
            //这里必须要点一下，才会触发回调
            getuserInfoButton.OnTap((res) =>
            {
                // 获取过了就清除
                if (res.errCode == 0)
                {
                    //用户已允许获取个人信息，返回的data即为用户信息
                    Log.Info("用户已允许获取个人信息，返回的data即为用户信息");
                    //《四、获取用户信息》
                    Log.Info("2.授权完成---获取用户信息");
                    WX.GetUserInfo(new GetUserInfoOption()
                    {
                        withCredentials = true,
                        lang = "zh_CN",
                        success = (data) =>
                        {
                            callback?.Invoke(code, data.encryptedData, data.iv);
                            Log.Info($"3.当前用户 {data.userInfo.nickName} data: {data}");
                        },
                        fail = e =>
                        {
                            Log.Info("3.获取个人信息失败");
                            callback?.Invoke(null, null, null);
                        }
                    });
                    Log.Info(res.userInfo.nickName);
                }
                else
                {
                    Log.Info("用户未允许获取个人信息");
                    callback?.Invoke(null, null, null);
                }

                DestroyUserInfoButton();
            });
        }

        public void DestroyUserInfoButton()
        {
            getuserInfoButton?.Destroy();
            getuserInfoButton = null;
        }

        public void SafeAreaAjust(RectTransform m_SafeAreaContainer, CanvasScaler cs)
        {
            
            var info = WX.GetSystemInfoSync();
            if (info == null) return;
            //顶部区域安全差异比例
            float py = (float) info.safeArea.top / (float) info.windowHeight;

            Debug.Log("WindowInfo.safeArea.top " + WindowInfo.safeArea.top + " WindowInfo.windowHeight " + WindowInfo.windowHeight + " py " + py);
            //底部区域安全差异比例
            float by = ((float) info.windowHeight -(float) info.safeArea.bottom) / (float) info.windowHeight;
            Debug.Log("WindowInfo.safeArea.bottom " + WindowInfo.safeArea.bottom + " WindowInfo.windowHeight " + WindowInfo.windowHeight + " by " + by);
            //得到当前canvasScaler
           
            //调整offsetMax.y - 顶部偏移 和offsetMin.y - 底部偏移。
            //m_SafeAreaContainer是当前界面的顶部RectTransfrom。
            //注意：这里的m_SafeAreaContainer原本是完全填充屏幕的。
            Debug.Log("m_SafeAreaContainer.offsetMax.y " + m_SafeAreaContainer.offsetMax.y + " m_SafeAreaContainer.offsetMin.y " + m_SafeAreaContainer.offsetMin.y + " cs.referenceResolution.y " + cs.referenceResolution.y + " py " + py + " by " + by);
            m_SafeAreaContainer.offsetMax = new Vector2(m_SafeAreaContainer.offsetMax.x,  m_SafeAreaContainer.offsetMax.y -(cs.referenceResolution.y * py) );
            // m_SafeAreaContainer.offsetMin = new Vector2(m_SafeAreaContainer.offsetMin.x, (m_SafeAreaContainer.offsetMin.y + cs.referenceResolution.y * (by)));
            // 重新计算缩放，让高度占满刘海屏以下的区域
            cs.referenceResolution = new Vector2(cs.referenceResolution.x , cs.referenceResolution.y * (1 - (py )));
            Debug.Log($"{cs.referenceResolution.x}  {cs.referenceResolution.y} {cs.referenceResolution.y * (1 - (py ))}");
        }

        /// <summary>
        /// 获取冷启动参数
        /// </summary>
        /// <returns></returns>
        public string GetColdLaunchOptionsSync()
        {
            var launchOptions = WX.GetLaunchOptionsSync();
            string code = "";
            //1036 来源App 分享消息卡片
            //https://developers.weixin.qq.com/minigame/dev/api/base/app/life-cycle/wx.getLaunchOptionsSync.html
            // if (Math.Abs(launchOptions.scene - 1036) < double.Epsilon)
            // {
            //     Log.Info("来源App 分享消息卡片");
            // }
            if (!launchOptions.query.ContainsKey("code"))
            {
                Log.Info("没有code参数！");
                return null;
            }
            else
            {
                //处理分享消息卡片
                code = launchOptions.query["code"];
            }

            Log.Info("获取的launchOptions-------");
            Log.Info(" scene: " + launchOptions.scene);
            Log.Info(launchOptions.query.ToString());
            Log.Info(launchOptions.referrerInfo.ToString());
            return code;
        }

        public bool IsColdLaunch = true;

        public string GetCode()
        {
            if (IsColdLaunch)
            {
                return GetColdLaunchOptionsSync();
            }
            else
            {
                return GetHotLaunchOptionsSync();
            }
        }

        /// <summary>
        /// 获取热启动参数
        /// </summary>
        /// <returns></returns>
        public string GetHotLaunchOptionsSync()
        {
            var launchOptions = WX.GetEnterOptionsSync();

            string code = "";
            //1036 来源App 分享消息卡片
            //https://developers.weixin.qq.com/minigame/dev/api/base/app/life-cycle/wx.getLaunchOptionsSync.html
            // if (Math.Abs(launchOptions.scene - 1036) < double.Epsilon)
            // {
            //     Log.Info("来源App 分享消息卡片");
            // }
            if (!launchOptions.query.ContainsKey("code"))
            {
                Log.Info("没有code参数！");
                return null;
            }
            else
            {
                //处理分享消息卡片
                code = launchOptions.query["code"];
            }

            Log.Info("获取的launchOptions-------");
            Log.Info(" scene: " + launchOptions.scene);
            Log.Info(launchOptions.query.ToString());
            Log.Info(launchOptions.referrerInfo.ToString());
            return code;
        }


        #region 消息提醒

         StorageInt RejectMessageSubScribeCnt;

         bool  IsStarShowSubScribeMessageTip = false;

         string[] RequestSubscribeMessageTmplId;
        
        /// <summary>
        /// 订阅消息字典，将各种提醒信息类型与对应的提醒消息集合进行映射
        /// </summary>
        public readonly Dictionary<MessageTipType, string[]> SubScribeMessage =
            new Dictionary<MessageTipType, string[]>
            {
                /// <summary>
                /// 礼包领取提醒，进入商店后，弹窗提示
                /// 包括礼包领取提醒、孵蛋完成通知、领取奖励提醒
                /// </summary>
                {
                    MessageTipType.GiftTip, new string[]
                    {
                        Constant.WXSubScribeMessage.PackageClaimReminder,
                        // Constant.WXSubScribeMessage.EggHatchingCompletionNotification,
                        // Constant.WXSubScribeMessage.ClaimRewardReminder
                        // Constant.WXSubScribeMessage.FreeDrawReward, // 备用
                    }
                },

                /// <summary>
                /// 游戏互动提醒
                /// 包括好友申请通知、游戏互动提醒
                /// </summary>
                {
                    MessageTipType.SocialRecordTip, new string[]
                    {
                        // Constant.WXSubScribeMessage.FriendRequestNotification,
                        Constant.WXSubScribeMessage.GameInteractionReminder
                    }
                },

                /// <summary>
                /// 体力恢复提醒
                /// 仅包含体力恢复提醒
                /// </summary>
                {
                    MessageTipType.DiceRecoveryTip, new string[]
                    {
                        Constant.WXSubScribeMessage.StaminaRecoveryReminder
                    }
                },
                {
                    MessageTipType.All, new string[]
                    {
                        Constant.WXSubScribeMessage.PackageClaimReminder,
                        Constant.WXSubScribeMessage.GameInteractionReminder,
                        Constant.WXSubScribeMessage.StaminaRecoveryReminder,
                    }
                }
                
            };

        enum SubScribeMessageStatus
        {
            /// <summary>
            /// 有未订阅
            /// </summary>
            HasNoSubscribed,

            /// <summary>
            /// 全部订阅了
            /// </summary>
            HasAllSubscribed,

            /// <summary>
            /// 订阅弹出过，但是用户取消了
            /// </summary>
            SubscribedButCanceled,
        }

        /// <summary>
        /// 检查有没有订阅消息
        /// </summary>
        /// <param name="tmplId"></param>
        /// <returns></returns>
        async UniTask<SubScribeMessageStatus> CheckHasSubscribeMessagePermission(string[] tmplId)
        {
            UniTaskCompletionSource<SubScribeMessageStatus>
                task = new UniTaskCompletionSource<SubScribeMessageStatus>();
            GetSettingOption settingOption = new GetSettingOption()
            {
                withSubscriptions = true,
                success = (res) =>
                {
                    bool allSubscribed = true;

                    Log.Info("订阅设置：" + JsonUtility.ToJson(res));

                    foreach (var id in tmplId)
                    {
                        if (!res.subscriptionsSetting.itemSettings.TryGetValue(id, out var itemSetting))
                        {
                            allSubscribed = false;
                            Log.Info("没有订阅过");
                            task.TrySetResult(SubScribeMessageStatus.HasNoSubscribed);
                            break; // 直接跳出循环
                        }
                        else if (itemSetting != "accept")
                        {
                            allSubscribed = false;
                            Log.Info("订阅弹出过，但是用户取消了");
                            task.TrySetResult(SubScribeMessageStatus.SubscribedButCanceled);
                           
                            break; // 直接跳出循环
                        }
                    }

                    if (allSubscribed)
                    {
                        Log.Info("全部订阅了");
                        ShowSubScribe(MessageTipType.All);
                        task.TrySetResult(SubScribeMessageStatus.HasAllSubscribed);
                    }
                },
                fail = (res) =>
                {
                    Log.Info("获取设置失败" + JsonUtility.ToJson(res));
                    task.TrySetResult(SubScribeMessageStatus.HasNoSubscribed);
                }
            };

            WX.GetSetting(settingOption);
            return await task.Task;
        }


        /// <summary>
        /// 订阅系统消息
        /// </summary>
         IEnumerator  RequestSubscribeSystemMessage(string[] tmplIds)
         {
             
            if (!IsStarShowSubScribeMessageTip)
            {
               yield break;
            }
            IsStarShowSubScribeMessageTip = false;
            
            
            if (tmplIds == null || tmplIds.Length == 0)
            {
                Log.Warning("没有需要订阅的消息模板");
                yield break;
            }
            Log.Info("--------------------开始订阅系统消息---------------------");
            
            
            //TODO:这里需要判断是否已经订阅过了
            WX.RequestSubscribeMessage(new RequestSubscribeMessageOption()
            {
                tmplIds = tmplIds,
                success = (res) =>
                {
                    Log.Info(res.errMsg);
                    Log.Info("订阅系统消息成功");
                },
                fail = (res) =>
                {
                    Log.Info(res.errCode + " " + res.errMsg);
                    Log.Info("订阅系统消息失败");
                }
            });
        }


        /// <summary>
        /// 显示订阅消息
        /// </summary>
        /// <param name="messageTipType"></param>
        public void ShowSubScribe(MessageTipType messageTipType)
        {
            if (SubScribeMessage.TryGetValue(messageTipType, out var tmplIds))
            {
               IsStarShowSubScribeMessageTip = true;
               RequestSubscribeMessageTmplId = tmplIds;
            }
            else
            {
                Log.Warning($"ShowSubScribe 没有找到对应的消息模板：{messageTipType}");
            }
        }

        /// <summary>
        /// 检查订阅消息
        /// </summary>
        /// <param name="messageTipType"></param>
        /// <returns></returns>
        public async UniTask<bool> CheckSubScribe(MessageTipType messageTipType)
        {
          
            if (SubScribeMessage.TryGetValue(messageTipType, out var tmplIds))
            {
                // 先检查是否有权限
               var status = await CheckHasSubscribeMessagePermission(tmplIds);
               if (status == SubScribeMessageStatus.HasAllSubscribed)
               {
                   Log.Info("全部订阅了");
                   //这里即使全部订阅了，也要弹出授权弹窗，因为是一次性的
                   ShowSubScribe(messageTipType);
                   return true;
               }
               else if (status == SubScribeMessageStatus.HasNoSubscribed)
               {
                   Log.Info("有未订阅的");
                   //这里即使全部订阅了，也要弹出授权弹窗，因为是一次性的
                   ShowSubScribe(messageTipType);
                   return false;
               }
               //拒绝过，弹出授权弹窗
               else
               {
                   Log.Info("订阅弹出过，但是用户取消了");
                   
                   if (RejectMessageSubScribeCnt.Value <= 0)
                   {
                       Log.Info("今天订阅消息提醒次数已用完");
                       return false;
                   }
                   GoToSetting();
                   RejectMessageSubScribeCnt.Modify(-1);
                   return false;
               }
            }
            else
            {
                Log.Warning("没有找到对应的消息模板");
                return true;
            }
        }

        #endregion


        #region 广告

        WXRewardedVideoAd rewardedVideoAd;

        void InitRewardedAd()
        {
            rewardedVideoAd = WX.CreateRewardedVideoAd(
                new WXCreateRewardedVideoAdParam()
                {
                    adUnitId = "", //输入你的广告位ID
                    multiton = true
                });


            //关闭广告事件监听
            void RewardAdClose(WXRewardedVideoAdOnCloseResponse res)
            {
                if ((res != null && res.isEnded) || res == null)
                {
                    // 正常播放结束，可以下发游戏奖励
                }
                else
                {
                    // 播放中途退出，不下发游戏奖励
                }
            }

            rewardedVideoAd.OnClose(RewardAdClose);
        }

        public async UniTask<bool> ShowRewardedAd()
        {
            if (rewardedVideoAd == null)
            {
                InitRewardedAd();
            }

            UniTaskCompletionSource<bool> task = new UniTaskCompletionSource<bool>();
            rewardedVideoAd.Show((wxRes) =>
            {
                Log.Info("播放广告成功：" + wxRes.errMsg);

                task.TrySetResult(true);
            }, (wxErr) =>
            {
                Log.Warning("播放广告失败：" + wxErr.errMsg);

                task.TrySetResult(false);
            });
            return await task.Task;
        }

        #endregion

        /// <summary>
        /// 跳转到设置页授权
        /// </summary>
        public void GoToSetting()
        {
            WX.ShowModal(new ShowModalOption()
            {
                title = "提示",
                content = "是否跳转到设置页授权",
                showCancel = true,
                confirmText = "确定",
                success = (res) =>
                {
                    if (res.confirm)
                    {
                        WX.OpenSetting(new OpenSettingOption()
                        {
                            success = (res2) => { Debug.Log(res2); }
                        });
                    }
                }
            });
        }
        private SystemInfo SystemInfo;

        private WindowInfo WindowInfo;


        #region 游戏圈

        private WXGameClubButton GameClubButton;

        public Vector2 GetOffsetWithCanvas(GameObject button)
        {
            var canvas = button.GetComponentInParent<Canvas>();
            ;
            // 获取 Canvas 和按钮的 RectTransform 组件
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

            // 计算 Canvas 左上角的局部位置
            Vector2 canvasTopLeftLocalPosition = new Vector2(
                -canvasRectTransform.rect.width * canvasRectTransform.pivot.x,
                canvasRectTransform.rect.height * (1 - canvasRectTransform.pivot.y)
            );

            // 计算按钮左上角的局部位置
            Vector2 buttonTopLeftLocalPosition = new Vector2(
                buttonRectTransform.anchoredPosition.x
                - buttonRectTransform.rect.width * buttonRectTransform.pivot.x,
                buttonRectTransform.anchoredPosition.y
                + buttonRectTransform.rect.height * (1 - buttonRectTransform.pivot.y)
            );

            // 将 Canvas 和按钮左上角的局部位置转换为世界坐标系中的位置
            Vector3 canvasTopLeftWorldPosition = canvasRectTransform.TransformPoint(
                canvasTopLeftLocalPosition
            );
            Vector3 buttonTopLeftWorldPosition = buttonRectTransform.TransformPoint(
                buttonTopLeftLocalPosition
            );

            var x = canvasTopLeftWorldPosition.x - buttonTopLeftWorldPosition.x;
            var y = canvasTopLeftWorldPosition.y - buttonTopLeftWorldPosition.y;
            return new Vector2(x, y);
        }

        public void CreateGameClubButton(RectTransform rectTransform)
        {
            var size = rectTransform.sizeDelta;

            var position = GetOffsetWithCanvas(rectTransform.gameObject);

            var GameClubButton = WX.CreateGameClubButton(
                new WXCreateGameClubButtonParam()
                {
                    type = GameClubButtonType.image,
                    text = "显示游戏圈按钮测试--测试",

                    style = new GameClubButtonStyle()
                    {
                        left = Math.Abs((int)(position.x / WindowInfo.pixelRatio)),
                        top = Math.Abs((int)(position.y / WindowInfo.pixelRatio)),
                        width = (int)(size.x * WindowInfo.screenWidth / 1080f),
                        height = (int)(size.y * WindowInfo.screenWidth / 1080f),
                    }
                }
            );

            // Debug.Log("windowInfo.pixelRatio" + windowInfo.pixelRatio + "position.x " + position.x + " position.y " +
            //           position.y + " size.x " + size.x + " size.y " + size.y);
            // // 390 844
            // Debug.Log("windowInfo.screenWidth " + windowInfo.screenWidth + " windowInfo.screenHeight " +
            //           windowInfo.screenHeight + "windowInfo.pixelRatio " + windowInfo.pixelRatio);
            // Debug.Log("CreateGameClubButton: " + GameClubButton);
            // Debug.Log("CreateGameClubButton: " + GameClubButton.style.left + " " + GameClubButton.style.top + " " +
            //           GameClubButton.style.width + " " + GameClubButton.style.height);
            // 78 144 116 36
            GameClubButton.Show();
        }

        public UniTaskCompletionSource<bool>  ShareImgToGameClubCompletionSource;


        public void SetShareResult(string result)
        {
            Log.Info($"传递给游戏圈的分享结果：{result}");
            if (ShareImgToGameClubCompletionSource == null)
            {
                Log.Error("ShareImgToGameClubCompletionSource is null");
                return;
            }
            
            ShareImgToGameClubCompletionSource.TrySetResult(result == "true");
        }
        
        public async UniTask<bool> Share2GameCenter(Share2GameCenterType share2GameCenterType, Texture2D texture2D)
        {
            //TODO 分享到游戏圈
            ShareImgToGameClubCompletionSource = new UniTaskCompletionSource<bool>();
#if UNITY_EDITOR
            ShareImgToGameClubCompletionSource.TrySetResult(true);
#else
            var (title, content) = ShareContent.GetTitleAndContent(share2GameCenterType);
            ShareManager.ShareCanvas(title, content);
#endif

            return await ShareImgToGameClubCompletionSource.Task;
        }
        
        public void SetEnabledGameClubButton(bool enabled)
        {
            if (GameClubButton == null)
            {
                return;
            }

            if (enabled)
            {
                GameClubButton.Show();
            }
            else
            {
                GameClubButton.Hide();
            }
        }

        //销毁游戏圈按钮
        public void DestroyGameClubButton()
        {
            if (GameClubButton == null)
            {
                return;
            }

            GameClubButton.Destroy();
            GameClubButton = null;
        }

        #endregion



        #region 排行榜

        
        [Serializable]

        public class OpenDataMessage
        {
            // type 用于表明时间类型
            public string type;

            public string shareTicket;

            public int score;
        }

        [LabelText("排行榜内容显示")]
          RawImage RankBody;
         [LabelText("排行榜画布")]
          CanvasScaler CanvasScaler;

        public void InitRankCanvas(RawImage rankBody, CanvasScaler canvasScaler)
        {
            RankBody = rankBody;
            CanvasScaler = canvasScaler;
        }
        /// <summary>
        /// 渲染到画布上
        /// </summary>
        public void ShowOpenData()
        {
            RankBody.gameObject.SetActive(true);
            var referenceResolution = CanvasScaler.referenceResolution;
            var p = RankBody.transform.position;
            
            Log.Info("ShowOpenData: " + p.x + " " + p.y + " " + RankBody.rectTransform.rect.width + " " +
                      RankBody.rectTransform.rect.height);
            WX.ShowOpenData(RankBody.texture, (int)p.x, Screen.height - (int)p.y, (int)((Screen.width / referenceResolution.x) * RankBody.rectTransform.rect.width),
                (int)((Screen.width / referenceResolution.x) * RankBody.rectTransform.rect.height));
        }

        public void HideOpenData()
        {
            if (!RankBody.gameObject.activeSelf)
            {
                Log.Info("不用隐藏了");
                return;
            }
            WX.HideOpenData();
            RankBody.gameObject.SetActive(false);
        }
        /// <summary>
        /// 这个给按钮绑定
        /// </summary>
        public void GetOpenData()
        {
            OpenDataMessage msgData = new OpenDataMessage();
            msgData.type = "showFriendsRank";
            string msg = JsonUtility.ToJson(msgData);
            WX.GetOpenDataContext().PostMessage(msg);
        }

        public void SetScore(int score)
        {
            OpenDataMessage msgData = new OpenDataMessage();
            msgData.type = "setUserRecord";
            msgData.score = score;
            string msg = JsonUtility.ToJson(msgData);
            Debug.Log(msg);
            WX.GetOpenDataContext().PostMessage(msg);
        }

        
        

        #endregion

        #region 保存UUid

        public void SetStorageUid(uint uid)
        {
            WX.SetUserCloudStorage(new SetUserCloudStorageOption()
            {
                KVDataList = new KVData[] {
                    new KVData()
                    {
                        key = "uid",
                        value = uid.ToString()
                    }
                },
                success = (res) =>
                {
                    Log.Info("设置用户云数据成功");
                },
                fail = (res) =>
                {
                    Log.Info("设置用户云数据失败：" + res.errMsg);
                }
            });
        }

        #endregion


        public void RestartGame()
        {
            WX.RestartMiniProgram(new RestartMiniProgramOption()
            {
                success = (res) =>
                {
                    Log.Info("重启小程序成功");
                },
                fail = (res) =>
                {
                    Log.Info("重启小程序失败：" + res.errMsg);
                }
            });
        }
        
        /// <summary>
        /// 异常需要重启游戏
        /// </summary>
        public void ShowRestartDialog()
        {
            WX.ShowModal(new ShowModalOption()
            {
                title = "与服务器断开连接",
                content = "请重新启动小游戏。",
                showCancel = false,
                confirmText = "确定",
                success = (res) =>
                {
                        RestartGame();
                }
            });
        }
        
#endif
    }
}