using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace AION.CoreFramework
{
    public class SearchModule
    {
        private TMP_InputField InputField;

        private CancellationTokenSource cancellationTokenSource;
        Action<string> SearchAction;

        Action ResetAction;
        
        public bool IsSearching => string.IsNullOrEmpty(InputField.text) == false;
        public SearchModule(TMP_InputField inputField, Action<string> searchAction, Action resetAction)
        {
            InputField = inputField;
            SearchAction = searchAction;
            ResetAction = resetAction;
            Init();
        }

        void Init()
        {
            cancellationTokenSource = new CancellationTokenSource();
            InputField.onValueChanged.AddListener((value) =>
            {
                GetSearchFriendResult(value);
            });
        }
        /// <summary>
        /// 搜索好友
        /// </summary>
        /// <param name="value"></param>
        private async void GetSearchFriendResult(string value)
        {
            //搜索校验
            value = value.Replace(" ", "");
            if (string.IsNullOrEmpty(value))
            { 
                cancellationTokenSource?.Cancel();
                ResetAction?.Invoke();
                return;
            }
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var token = cancellationTokenSource.Token;

            try
            {
                await UniTask.Delay(500, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                // 搜索已经被取消
                return;
            }

            SearchAction?.Invoke(value);
        }

        public void ReSearch()
        {
            SearchAction?.Invoke(InputField.text);
        }

        public void Reset()
        {
            InputField.text = "";
            GetSearchFriendResult("");
        }
        // var searchModule = new SearchModule(InputField, async (value) =>
        // {
        //     Debug.Log("Search "+value);
        //     var (isSuccess, response) = await GameEntry.Request.GetSearchCollectCardFriendListFunc(value).TryRequest();
        //     if (!isSuccess)
        //     {
        //         return;
        //     }
        //     SearchList = response;
        //     await this.ShowInfiniteScrollView(SearchList.Count, UpdateFriendList);
        // }, async () =>
        // {
        //     var (isSuccess, response) = await GameEntry.Request.GetCollectCardFriendListFunc().TryRequest();
        //
        //     if (!isSuccess)
        //     {
        //         return;
        //     }
        //     UserList = response;
        //     SelectUserIds = new HashSet<uint>();
        //     await this.ShowInfiniteScrollView(UserList.Count, UpdateFriendList);
        // });

      
    }
}