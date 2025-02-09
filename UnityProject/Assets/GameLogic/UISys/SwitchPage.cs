using System;
using System.Collections.Generic;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic.UISys
{
    //主页上的功能涉及的数据较少，不应该用这个
    /// <summary>
    /// 页签切换。主要是动态皮肤之类的，需要实时更新
    /// </summary>
    public class SwitchPage<T>
    {
     
        SwitchTabItem _currentTab;
        Dictionary<int,SwitchTabItem> TabItemDic = new Dictionary<int, SwitchTabItem>();
        

        Func<int,T> IdToData;
        Func<T,int> DataToId; 
        Action<SwitchTabItem,T> OnSelectTab;
        //什么时候需要直接获取某个tabitem ，网络请求下发的时候，如果局部更新的话，需要先获取tabitem，然后再更新

        private UIBase ui;
        //只存引用可以吗，不行的应该，只绑定的话，有事要对数据clear操作，会导致绑定失效
        public SwitchPage(UIBase _ui, Transform parent, 
            List<T> Datas,
            Func<T,int> _DataToId,
            Func<int,T> _IdToData,
            Action<SwitchTabItem,T> _onSelectTab)
        {
            ui = _ui;
            OnSelectTab = _onSelectTab;
            IdToData = _IdToData;
            DataToId = _DataToId;
            for (var i = 0; i < Datas.Count; i++)
            {
                var data = Datas[i];
                
                var tabItem = ui.CreateWidget<SwitchTabItem>(parent.gameObject);
                
                int id = DataToId(data);
                tabItem._id = id;
                
                TabItemDic[id]=(tabItem);
                
                tabItem.BindClickEvent((tabItem) =>
                {
                    OnSelectTab(tabItem, IdToData(tabItem._id));
                });
            }
        }
        
    }



    public class SwitchTabItem:UIEventItem<SwitchTabItem>
    {
        /// <summary>
        /// 页签的Id,用于访问Data
        /// </summary>
        public int _id;
        
        
        
        public void BindRedNode(RedNodeType redNodeType, Transform redNodeParent)
        {
            RedNodeIcon icon = CreateWidget<RedNodeIcon>(redNodeParent.gameObject);
            
            icon.Bind(redNodeType);
        }
    }
}