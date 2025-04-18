﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AION.CoreFramework
{
    /// <summary>
    /// UI类型。
    /// </summary>
    public enum UIBaseType
    {
        /// <summary>
        /// 类型无。
        /// </summary>
        None,

        /// <summary>
        /// 类型Windows。
        /// </summary>
        Window,

        /// <summary>
        /// 类型Widget。
        /// </summary>
        Widget,
    }

    /// <summary>
    /// UI基类。
    /// </summary>
    public class UIBase : IUIBehaviour
    {
        /// <summary>
        /// 所属UI父节点。
        /// </summary>
        protected UIBase parent = null;

        /// <summary>
        /// UI父节点。
        /// </summary>
        public UIBase Parent => parent;

        /// <summary>
        /// 自定义数据集。
        /// </summary>
        protected System.Object[] userDatas;

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public virtual GameObject gameObject { protected set; get; }
        
        /// <summary>
        /// 窗口位置组件。
        /// </summary>
        public virtual Transform transform { protected set; get; }

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public virtual RectTransform rectTransform { protected set; get; }

        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIBaseType BaseType => UIBaseType.None;

        /// <summary>
        /// 资源是否准备完毕。
        /// </summary>
        public bool IsPrepare { protected set; get; }

        /// <summary>
        /// UI子组件列表。
        /// </summary>
        public List<UIWidget> ListChild = new List<UIWidget>();

        /// <summary>
        /// 存在Update更新的UI子组件列表。
        /// </summary>
        protected List<UIWidget> m_listUpdateChild = null;

        /// <summary>
        /// 是否持有Update行为。
        /// </summary>
        protected bool m_updateListValid = false;

       
        /// <summary>
        /// 代码自动生成绑定。
        /// </summary>
        public virtual void ScriptGenerator()
        {
        }

        /// <summary>
        /// 绑定UI成员元素。
        /// </summary>
        public virtual void BindMemberProperty()
        {
        }

        /// <summary>
        /// 注册事件。
        /// </summary>
        public virtual void RegisterEvent()
        {
        }

        /// <summary>
        /// 窗口创建。
        /// </summary>
        public virtual void OnCreate()
        {
        }

        /// <summary>
        /// 窗口刷新
        /// </summary>
        public virtual void OnRefresh()
        {
        }

        /// <summary>
        /// 是否需要Update
        /// </summary>
        protected bool HasOverrideUpdate = true;

        /// <summary>
        /// 窗口更新
        /// </summary>
        public virtual void OnUpdate()
        {
            HasOverrideUpdate = false;
        }

        /// <summary>
        /// 窗口销毁
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 当触发窗口的层级排序
        /// </summary>
        protected virtual void OnSortDepth(int depth)
        {
        }

        /// <summary>
        /// 当因为全屏遮挡触发窗口的显隐
        /// </summary>
        protected virtual void OnSetVisible(bool visible)
        {
        }
        
        internal void SetUpdateDirty()
        {
            m_updateListValid = false;
            if (Parent != null)
            {
                Parent.SetUpdateDirty();
            }
        }

        //事件 Timer 加载

        
        #region UIWidget
        
        /// 创建UIWidget通过游戏物体。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="goRoot">游戏物体。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (widget.Create(this, goRoot, visible))
            {
                return widget;
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByPath<T>(Transform parentTrans, string assetLocation, bool visible = true) where T : UIWidget, new()
        {
            GameObject go = GameModule.Resource.LoadAsset<GameObject>(assetLocation);
            var goInst=Object.Instantiate(go, parentTrans, false);
            return CreateWidget<T>(goInst, visible);
        }

        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByPathAsync<T>(Transform parentTrans, string assetLocation, bool visible = true) where T : UIWidget, new()
        {
            GameObject go = await GameModule.Resource.LoadAssetAsync<GameObject>(assetLocation);
            var goInst=Object.Instantiate(go, parentTrans, false);
            return CreateWidget<T>(goInst, visible);
        }

        /// <summary>
        /// 根据prefab或者模版来创建新的 widget。
        /// </summary>
        /// <param name="goPrefab">资源创建副本。</param>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByPrefab<T>(GameObject goPrefab, Transform parentTrans = null, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPrefab(this, goPrefab, parentTrans, visible))
            {
                return null;
            }

            return widget;
        }

        /// <summary>
        /// 通过UI类型来创建widget。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByType<T>(Transform parentTrans, bool visible = true) where T : UIWidget, new()
        {
            return CreateWidgetByPath<T>(parentTrans, typeof(T).Name, visible);
        }

        /// <summary>
        /// 通过UI类型来创建widget。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByTypeAsync<T>(Transform parentTrans, bool visible = true) where T : UIWidget, new()
        {
            return await CreateWidgetByPathAsync<T>(parentTrans, typeof(T).Name, visible);
        }

        /// <summary>
        /// 调整图标数量。
        /// </summary>
        /// <remarks>常用于Icon创建。</remarks>
        /// <param name="listIcon">存放Icon的列表。</param>
        /// <param name="number">创建数目。</param>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="prefab">资产副本。</param>
        /// <param name="assetPath">资产地址。</param>
        /// <typeparam name="T">图标类型。</typeparam>
        public void AdjustIconNum<T>(List<T> listIcon, int number, Transform parentTrans, GameObject prefab = null, string assetPath = "")
            where T : UIWidget, new()
        {
            if (listIcon == null)
            {
                listIcon = new List<T>();
            }

            if (listIcon.Count < number)
            {
                int needNum = number - listIcon.Count;
                for (int iconIdx = 0; iconIdx < needNum; iconIdx++)
                {
                    T tmpT = prefab == null ? CreateWidgetByType<T>(parentTrans) : CreateWidgetByPrefab<T>(prefab, parentTrans);
                    listIcon.Add(tmpT);
                }
            }
            else if (listIcon.Count > number)
            {
                RemoveUnUseItem<T>(listIcon, number);
            }
        }

        /// <summary>
        /// 异步调整图标数量。
        /// </summary>
        /// <param name="listIcon"></param>
        /// <param name="tarNum"></param>
        /// <param name="parentTrans"></param>
        /// <param name="prefab"></param>
        /// <param name="assetPath"></param>
        /// <param name="maxNumPerFrame"></param>
        /// <param name="updateAction"></param>
        /// <typeparam name="T"></typeparam>
        public void AsyncAdjustIconNum<T>(List<T> listIcon, int tarNum, Transform parentTrans, GameObject prefab = null,
            string assetPath = "", int maxNumPerFrame = 5,
            Action<T, int> updateAction = null) where T : UIWidget, new()
        {
            AsyncAdjustIconNumInternal(listIcon, tarNum, parentTrans, maxNumPerFrame, updateAction, prefab, assetPath).Forget();
        }

        /// <summary>
        /// 异步创建接口。
        /// </summary>
        /// <param name="listIcon"></param>
        /// <param name="tarNum"></param>
        /// <param name="parentTrans"></param>
        /// <param name="maxNumPerFrame"></param>
        /// <param name="updateAction"></param>
        /// <param name="prefab"></param>
        /// <param name="assetPath"></param>
        /// <typeparam name="T"></typeparam>
        private async UniTaskVoid AsyncAdjustIconNumInternal<T>(List<T> listIcon, int tarNum, Transform parentTrans, int maxNumPerFrame,
            Action<T, int> updateAction, GameObject prefab, string assetPath) where T : UIWidget, new()
        {
            if (listIcon == null)
            {
                listIcon = new List<T>();
            }

            int createCnt = 0;

            for (int i = 0; i < tarNum; i++)
            {
                T tmpT = null;
                if (i < listIcon.Count)
                {
                    tmpT = listIcon[i];
                }
                else
                {
                    if (prefab == null)
                    {
                        tmpT = await CreateWidgetByPathAsync<T>(parentTrans, assetPath);
                    }
                    else
                    {
                        tmpT = CreateWidgetByPrefab<T>(prefab, parentTrans);
                    }

                    listIcon.Add(tmpT);
                }

                int index = i;
                if (updateAction != null)
                {
                    updateAction(tmpT, index);
                }

                createCnt++;
                if (createCnt >= maxNumPerFrame)
                {
                    createCnt = 0;
                    await UniTask.Yield();
                }
            }

            if (listIcon.Count > tarNum)
            {
                RemoveUnUseItem(listIcon, tarNum);
            }
        }

        private void RemoveUnUseItem<T>(List<T> listIcon, int tarNum) where T : UIWidget
        {
            var removeIcon = new List<T>();
            for (int iconIdx = 0; iconIdx < listIcon.Count; iconIdx++)
            {
                var icon = listIcon[iconIdx];
                if (iconIdx >= tarNum)
                {
                    removeIcon.Add(icon);
                }
            }

            for (var index = 0; index < removeIcon.Count; index++)
            {
                var icon = removeIcon[index];
                listIcon.Remove(icon);
                icon.OnDestroy();
                icon.OnDestroyWidget();
                ListChild.Remove(icon);
                if (icon.gameObject != null)
                {
                    UnityEngine.Object.Destroy(icon.gameObject);
                }
            }
        }

        #endregion
    }
}