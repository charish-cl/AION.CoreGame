using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;
using GameConfig.item;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameLogic
{
    [Window(UILayer.UI)]
    public partial class BattleMainUI
    {
        // 拖拽系统
        private DragDropLogicHandler m_dragDropLogicHandler;
        private DragDropViewBinder m_dragDropViewBinder;
        private GridRenderer m_gridRenderer;
        
        // 抽卡对象列表
        private List<DrawItem> m_drawItems = new List<DrawItem>();
        
        // 配置
        private const float ITEM_SPACING = 120f; // 卡片间距
        private const float ROTATION_RANGE = 20f; // 旋转范围 ±20°
        private const float ANIMATION_DURATION = 0.3f; // 动画时长
        
        public override void OnCreate()
        {
            base.OnCreate();

            // var currencyWidget = CreateWidgetByType<CurrencyWidget>(transform);
            // currencyWidget.InitCurrency(CurrencyType.Coin);
            //
            // 初始化拖拽系统
            InitializeDragDropSystem();
        }
        
        /// <summary>
        /// 初始化拖拽系统
        /// </summary>
        private void InitializeDragDropSystem()
        {
            // 确保 GridHelper 已初始化
            var gridHelper = GridHelper.Instance;
            if (!gridHelper.IsInitialized)
            {
                gridHelper.Initialize();
            }
            
            // 创建 GridRenderer 子对象
            GameObject gridRendererObj = new GameObject("GridRenderer");
            // gridRendererObj.transform.SetParent(transform, false);
            m_gridRenderer = gridRendererObj.AddComponent<GridRenderer>();
            
            // 创建视图绑定器
            m_dragDropViewBinder = new DragDropViewBinder();
            m_dragDropViewBinder.Initialize(m_gridRenderer);
            
            // 创建逻辑处理器
            m_dragDropLogicHandler = new DragDropLogicHandler();
            m_dragDropLogicHandler.Initialize(m_dragDropViewBinder);
        }

        void RefreshUI()
        {
            
        
        }
        
        private void OnClick_button_Pause()
        {
        }

        private void OnClick_button_GameSpeed()
        {
        }

        private void OnClick_button_Refresh()
        {
            // 调用 BattleSystem 的抽卡方法
            var battleSys = BattleSystem.Instance;
            if (battleSys == null)
            {
                Log.Warning("BattleMainUI: BattleSystem 未初始化");
                return;
            }
            
            // 检查金币是否足够（DrawCard 内部会检查，但这里先提示）
            if (battleSys.Gold < battleSys.DrawCardCost)
            {
                Log.Warning($"BattleMainUI: 金币不足，需要 {battleSys.DrawCardCost}，当前只有 {battleSys.Gold}");
                return;
            }
            
            // 抽卡
            List<int> towerIds = battleSys.DrawTowerCard();
            if (towerIds == null || towerIds.Count == 0)
            {
                Log.Warning("BattleMainUI: 抽卡失败");
                return;
            }
            
            // 创建新的抽卡对象
            CreateDrawItems(towerIds);
        }

        private void OnClick_button_StartFight()
        {
        }

        private void OnClick_button_BuyGrid()
        {
            
        }
        
        /// <summary>
        /// 创建抽卡对象
        /// </summary>
        private void CreateDrawItems(List<int> towerIds)
        {
            if (m_tfDrawParent == null || m_itemDraw == null || towerIds == null || towerIds.Count == 0)
            {
                return;
            }
            
            // 使用 AdjustIconNum 统一创建子对象
            AdjustIconNum(m_drawItems, towerIds.Count, m_tfDrawParent, m_itemDraw);
            
            // 计算起始位置（居中排列）
            float totalWidth = (towerIds.Count - 1) * ITEM_SPACING;
            float startX = -totalWidth * 0.5f;
            
            // 获取世界相机
            Camera worldCamera = Camera.main;
            
            // 初始化每个抽卡对象
            for (int i = 0; i < m_drawItems.Count && i < towerIds.Count; i++)
            {
                int towerId = towerIds[i];
                DrawItem drawItem = m_drawItems[i];
                
                if (drawItem == null || drawItem.gameObject == null) continue;
                
                // 设置位置
                RectTransform rectTransform = drawItem.rectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(startX + i * ITEM_SPACING, 0f);
                }
                
                // 获取塔配置
                TowerConfig towerConfig = null;
                if (ConfigSystem.Instance?.Tables?.TbTower != null)
                {
                    towerConfig = ConfigSystem.Instance.Tables.TbTower.GetOrDefault(towerId);
                }
                
                // 创建 footprint（默认 1x1，如果有配置可以扩展）
                List<Vector2Int> footprint = new List<Vector2Int> { Vector2Int.zero };
                // TODO: 如果 TowerConfig 有 footprint 字段，可以从配置读取
                
                // 创建 DragItemData
                DragItemData itemData = new DragItemData(towerId, 1, footprint);
                
                // 初始化 Widget
                drawItem.Init(towerId, itemData, m_dragDropLogicHandler, worldCamera);
                
                // 添加动画和旋转
                AnimateDrawItem(drawItem.transform, i);
            }
        }
        
        /// <summary>
        /// 动画抽卡对象（从小变大 + 随机旋转）
        /// </summary>
        private void AnimateDrawItem(Transform itemTransform, int index)
        {
            if (itemTransform == null) return;
            
            // 随机旋转角度（±20°）
            float randomRotation = Random.Range(-ROTATION_RANGE, ROTATION_RANGE);
            itemTransform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);
            
            // 从小变大的动画
            itemTransform.localScale = Vector3.zero;
            itemTransform.DOScale(Vector3.one, ANIMATION_DURATION)
                .SetDelay(index * 0.1f) // 错开动画时间
                .SetEase(Ease.OutBack);
        }
    }
}