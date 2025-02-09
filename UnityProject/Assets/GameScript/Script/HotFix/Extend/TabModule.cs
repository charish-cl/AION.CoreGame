using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AION.CoreFramework
{
    public class TabModule
    {
        private Dictionary<int, List<GameObject>> TabDic;
        private Dictionary<int, (Action<GameObject> onEnter, Action<GameObject> onExit)> StateActionDic;
        private List<GameObject> CommonList;
        private int currentTab = 0; // 当前的tab索引

        public RectTransform StateParent;

        public Dictionary<int, string> DynammicTabPathDic;

        public Dictionary<int, GameObject> DynammicTabDic;

        public void HideAll()
        {
            foreach (var item in TabDic)
            {
                item.Value[0].SetActive(false);
            }
            CommonList.ForEach(obj => obj.SetActive(false));
        }

        public void RemoveLastTab()
        {
            TabDic.Remove(TabDic.Count - 1);
        }

        public static TabModule Create(Transform parent)
        {
            var tab = Create();
            for (int i = 0; i < parent.childCount; i++)
            {
                tab.AddTab(i, parent.GetChild(i).gameObject);
            }
            tab.StateParent = (RectTransform)parent;
            return tab;
        }

        public static TabModule Create()
        {
            var tab = new TabModule();
            tab.TabDic ??= new Dictionary<int, List<GameObject>>();
            tab.StateActionDic ??= new Dictionary<int, (Action<GameObject> onEnter, Action<GameObject> onExit)>();
            tab.CommonList ??= new List<GameObject>();
            tab.DynammicTabPathDic ??= new Dictionary<int, string>();
            tab.DynammicTabDic ??= new Dictionary<int, GameObject>();
            return tab;
        }

        // AddTab
        public void AddTab(int index, params GameObject[] gameObject)
        {
            if (TabDic.ContainsKey(index))
            {
                TabDic[index].AddRange(gameObject);
            }
            else
            {
                TabDic.Add(index, new List<GameObject>(gameObject));
            }
        }

        public void AddTab(int index, List<GameObject> gameObject)
        {
            if (TabDic.ContainsKey(index))
            {
                TabDic[index].AddRange(gameObject);
            }
            else
            {
                TabDic.Add(index, gameObject);
            }
        }

        // 添加OnEnter和OnExit行为（无参）
        public void AddSwitchAction(int index, Action onEnter, Action onExit = null)
        {
            // 包装无参的 Action 为带 GameObject 参数的 Action
            if (StateActionDic.ContainsKey(index))
            {
                var (onEnter1, onExit1) = StateActionDic[index];
                StateActionDic[index] = ((go) =>
                {
                    onEnter1?.Invoke(go);
                    onEnter?.Invoke();
                }, (go) =>
                {
                    onExit1?.Invoke(go);
                    onExit?.Invoke();
                });
                return;
            }
            StateActionDic.Add(index, ((go) => onEnter?.Invoke(), (go) => onExit?.Invoke()));
        }

        public GameObject GetTabFirstGameObject(int index)
        {
            return TabDic[index][0];
        }

        public void OpenTab(Enum @enum)
        {
            OpenTab(Convert.ToInt32(@enum));
        }
        public void OpenTabIfNeed(Enum @enum)
        {
            if (currentTab != Convert.ToInt32(@enum))
            {
                OpenTab(Convert.ToInt32(@enum));
            }
        }

        #region 异步加载逻辑
        private async UniTask<GameObject> LoadTabAssetAsync(int index)
        {
            if (!DynammicTabPathDic.ContainsKey(index))
            {
                throw new Exception($"不存在索引为{index}的Tab的路径");
            }
            var go = await Game.Resource.LoadAssetAsync<GameObject>(DynammicTabPathDic[index]);
            if (go == null)
            {
                throw new Exception($"资源加载失败，路径为{DynammicTabPathDic[index]}");
            }
            return go;
        }

        private void AddTabToDictionaries(int index, GameObject uiItem)
        {
            if (!TabDic.ContainsKey(index))
            {
                TabDic.Add(index, new List<GameObject> { uiItem.gameObject });
            }
            else
            {
                // 确保第一个元素是uiItem.gameObject
                TabDic[index].Insert(0, uiItem.gameObject);
            }
            DynammicTabDic.Add(index, uiItem.gameObject);
            SortChild();
        }

        public async UniTask<T> OpenTabAsyncImp<T>(int index, bool needCloseOtherTab = true, Action<T> action = null) where T : Component
        {
            T uiItem;
            if (!TabDic.ContainsKey(index) || !DynammicTabDic.ContainsKey(index))
            {
                var go = await LoadTabAssetAsync(index);
                var instantiate = GameObject.Instantiate(go, StateParent);
                uiItem = instantiate.GetOrAddComponent<T>();
                instantiate.gameObject.SetActive(true);
                AddTabToDictionaries(index, uiItem.gameObject);
            }
            else
            {
                uiItem = DynammicTabDic[index].GetComponent<T>();
            }
            action?.Invoke(uiItem);

            OpenTab(index, needCloseOtherTab);

            return uiItem;
        }

        public async UniTask<GameObject> OpenTabAsyncImp(int index, bool needCloseOtherTab = true, Action<GameObject> action = null)
        {
            GameObject uiItem;
            if (!TabDic.ContainsKey(index) || !DynammicTabDic.ContainsKey(index))
            {
                var go = await LoadTabAssetAsync(index);
                var instantiate = GameObject.Instantiate(go, StateParent);
                uiItem = instantiate;
                instantiate.gameObject.SetActive(true);
                AddTabToDictionaries(index, uiItem);
            }
            else
            {
                uiItem = DynammicTabDic[index];
            }

            action?.Invoke(uiItem);

            OpenTab(index, needCloseOtherTab);
            return uiItem;
        }

        public async UniTask OpenTabAsync(Enum @enum, bool needCloseOtherTab = true)
        {
            await OpenTabAsyncImp(Convert.ToInt32(@enum), needCloseOtherTab);
        }

        public async UniTask<T> OpenTabAsync<T>(Enum @enum, bool needCloseOtherTab = true, Action<T> action = null) where T : Component
        {
            return await OpenTabAsyncImp<T>(Convert.ToInt32(@enum), needCloseOtherTab, action);
        }
        #endregion

        private void SortChild()
        {
            foreach (var (_, go) in DynammicTabDic.OrderBy(kv => kv.Key))
            {
                go.transform.SetAsLastSibling();
            }
        }


        public void OpenTab(int index, bool needCloseOtherTab = true)
        {
            // 记录之前的tab
            int previousTab = currentTab;
            currentTab = index;

            if (needCloseOtherTab)
            {
                // 触发上一个Tab的OnExit
                if (StateActionDic.ContainsKey(previousTab))
                {
                    StateActionDic[previousTab].onExit?.Invoke(GetTabFirstGameObject(previousTab));
                }
            }

            if (needCloseOtherTab)
            {
                // 切换当前Tab的可见状态
                foreach (var item in TabDic)
                {
                    bool isCurrentTab = item.Key == index;
                    foreach (var gameObject in item.Value)
                    {
                        if (gameObject != null)
                        {
                            gameObject.SetActive(isCurrentTab);
                        }
                    }
                }
            }

            //确保当前Tab可见
            TabDic[index].ForEach(obj =>
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            });
            // 触发当前Tab的OnEnter
            if (StateActionDic.ContainsKey(index))
            {
                StateActionDic[index].onEnter?.Invoke(GetTabFirstGameObject(index));
            }

            // 确保公共元素可见
            CommonList.ForEach(obj => obj.SetActive(true));
        }

        public int CurrentTabIndex => currentTab;

        public GameObject GetCurrentTabGameObject()
        {
            return TabDic[currentTab][0];
        }

        public GameObject GetGameObjectByEnum(Enum @enum)
        {
            int index = Convert.ToInt32(@enum);
            if (DynammicTabDic[index] == null)
            {
                Log.Error("先Open在Get");
            }
            return DynammicTabDic[index];
        }

        public void Clear()
        {
            TabDic.Clear();
            StateActionDic.Clear();
            CommonList.Clear();
            DynammicTabDic.Clear();
            DynammicTabPathDic.Clear();
        }

        public void AddCommon(params GameObject[] go)
        {
            CommonList.AddRange(go);
        }
    }
}