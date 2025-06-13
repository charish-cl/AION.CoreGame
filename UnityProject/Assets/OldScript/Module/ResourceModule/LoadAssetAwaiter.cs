using System;
using System.Runtime.CompilerServices;
//https://www.cnblogs.com/walterlv/p/10236529.html
namespace AION.CoreFramework
{
    public class LoadAssetAwaiter : INotifyCompletion
    {
        // 返回LoadAssetAwaiter
        public LoadAssetAwaiter GetAwaiter()
        {
            return this;
        }

        // IsCompleted初始为false, 表示还未完成
        public bool IsCompleted { get; private set; }

        public AssetObject Result { get; set; }
        
        // GetResult在异步操作完成后调用
        public AssetObject GetResult()
        {
            return Result;   
        }
        
        public void SetResult(AssetObject result)
        {
            this.Result = result;
            this.IsCompleted = true;
            
            // 异步操作完成后执行后面的代码
           //这里为啥_continuation是空的呢？
            Action tempCallback = _mContinuation;
            _mContinuation = null;
            tempCallback.Invoke();
            
        }
        private Action _mContinuation;
  
        //OnCompleted 方法会在主线程调用的代码结束后立即执行。
        //参数中的 continuation 是对 await 后面代码的一层包装，调用它即可让 await 后面的代码开始执行。
        // 设置异步操作完成后执行的回调
        public  void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation.Invoke();
            }
            else
            {
                _mContinuation += continuation;
            }
        }
    }
}