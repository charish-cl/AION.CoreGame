using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameLogic;
using GameConfig;
using GameConfig.battle;
using AION.CoreFramework;

namespace GameLogic.Editor
{
    /// <summary>
    /// Actor测试工具窗口
    /// </summary>
    public class ActorTestTool : EditorWindow
    {
        private Vector2 m_unitScrollPos;
        private Vector2 m_actorScrollPos;
        private Vector2 m_buffScrollPos;
        private Vector2 m_attackerPropsScrollPos;
        private Vector2 m_targetPropsScrollPos;
        
        // Unit选择
        private List<int> m_availableUnitIds = new List<int>();
        private List<int> m_availableTowerIds = new List<int>();
        private int m_selectedUnitId = 0;
        private int m_selectedTowerId = 0;
        private string m_unitSearchFilter = "";
        
        // Actor列表
        private List<GameActor> m_actors = new List<GameActor>();
        private int m_selectedActorIndex = -1;
        private int m_selectedTargetIndex = -1;
        
        // Buff选择
        private List<int> m_availableBuffIds = new List<int>();
        private int m_selectedBuffId = 0;
        private string m_buffSearchFilter = "";
        
        // 控制选项
        private bool m_autoRefresh = true;
        
        [MenuItem("Tools/Actor Test Tool")]
        public static void ShowWindow()
        {
            GetWindow<ActorTestTool>("Actor Test Tool");
        }
        
        private void OnEnable()
        {
            // 延迟刷新，等待配置系统初始化
            EditorApplication.delayCall += () =>
            {
                RefreshUnitIds();
                RefreshTowerIds();
                RefreshBuffIds();
                RefreshActors();
            };
        }
        
        private void OnGUI()
        {
            // 检查是否在运行状态
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请在 Play 模式下使用此工具", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.BeginVertical();
            
            // 标题
            EditorGUILayout.LabelField("Actor Test Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // 自动刷新选项
            m_autoRefresh = EditorGUILayout.Toggle("Auto Refresh", m_autoRefresh);
            
            EditorGUILayout.BeginHorizontal();
            if (m_autoRefresh || GUILayout.Button("Refresh Actors"))
            {
                RefreshActors();
            }
            
            // 刷新配置按钮
            if (GUILayout.Button("Refresh Configs", GUILayout.Width(120)))
            {
                RefreshUnitIds();
                RefreshTowerIds();
                RefreshBuffIds();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            // 创建单位区域
            DrawCreateUnitSection();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            // Actor列表和控制区域
            DrawActorControlSection();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            // Buff添加区域
            DrawBuffSection();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制创建单位区域
        /// </summary>
        private void DrawCreateUnitSection()
        {
            EditorGUILayout.LabelField("Create Unit", EditorStyles.boldLabel);
            
            // 检查配置系统是否已初始化
            if (ConfigSystem.Instance == null || ConfigSystem.Instance.Tables == null)
            {
                EditorGUILayout.HelpBox("配置系统未初始化，请在 Play 模式下使用", MessageType.Warning);
                if (GUILayout.Button("Refresh Config"))
                {
                    RefreshUnitIds();
                    RefreshTowerIds();
                    RefreshBuffIds();
                }
                return;
            }
            
            // Unit ID选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unit ID:", GUILayout.Width(80));
            
            // 搜索过滤
            m_unitSearchFilter = EditorGUILayout.TextField(m_unitSearchFilter, GUILayout.Width(100));
            
            // 过滤后的Unit ID列表
            var filteredUnitIds = m_availableUnitIds.Where(id => 
                string.IsNullOrEmpty(m_unitSearchFilter) || 
                id.ToString().Contains(m_unitSearchFilter) ||
                (ConfigSystem.Instance?.Tables?.TbUnit?.GetOrDefault(id)?.Name?.Contains(m_unitSearchFilter) ?? false)).ToList();
            
            if (filteredUnitIds.Count > 0)
            {
                int currentIndex = filteredUnitIds.IndexOf(m_selectedUnitId);
                if (currentIndex < 0) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, 
                    filteredUnitIds.Select(id => 
                    {
                        var config = ConfigSystem.Instance?.Tables?.TbUnit?.GetOrDefault(id);
                        return config != null ? $"{id} - {config.Name}" : id.ToString();
                    }).ToArray(), GUILayout.Width(200));
                
                if (newIndex >= 0 && newIndex < filteredUnitIds.Count)
                {
                    m_selectedUnitId = filteredUnitIds[newIndex];
                }
            }
            else
            {
                EditorGUILayout.LabelField("No units available", EditorStyles.helpBox);
                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    RefreshUnitIds();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Tower ID选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tower ID:", GUILayout.Width(80));
            
            var filteredTowerIds = m_availableTowerIds.Where(id => 
                string.IsNullOrEmpty(m_unitSearchFilter) || 
                id.ToString().Contains(m_unitSearchFilter) ||
                (ConfigSystem.Instance?.Tables?.TbTower?.GetOrDefault(id)?.Name?.Contains(m_unitSearchFilter) ?? false)).ToList();
            
            if (filteredTowerIds.Count > 0)
            {
                int currentIndex = filteredTowerIds.IndexOf(m_selectedTowerId);
                if (currentIndex < 0) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, 
                    filteredTowerIds.Select(id => 
                    {
                        var config = ConfigSystem.Instance?.Tables?.TbTower?.GetOrDefault(id);
                        return config != null ? $"{id} - {config.Name}" : id.ToString();
                    }).ToArray(), GUILayout.Width(200));
                
                if (newIndex >= 0 && newIndex < filteredTowerIds.Count)
                {
                    m_selectedTowerId = filteredTowerIds[newIndex];
                }
            }
            else
            {
                EditorGUILayout.LabelField("No towers available", EditorStyles.helpBox);
                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    RefreshTowerIds();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 创建按钮
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Hero", GUILayout.Height(30)))
            {
                CreateUnit(m_selectedUnitId, UnitTag.Player);
            }
            
            if (GUILayout.Button("Create Enemy", GUILayout.Height(30)))
            {
                CreateUnit(m_selectedUnitId, UnitTag.Enemy);
            }
            
            if (GUILayout.Button("Create Tower", GUILayout.Height(30)))
            {
                CreateTower(m_selectedTowerId);
            }
            
            if (GUILayout.Button("Create DamageFont", GUILayout.Height(30)))
            {
                CreateDamageFont();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 测试伤害数字显示（用于调试字体初始化问题）
        /// </summary>
        private void CreateDamageFont()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", "请在 Play 模式下使用此功能", "确定");
                return;
            }
            
            // 获取伤害数字预制体
            var sceneBehavior = ActorMgr.Instance?.SceneBehavior;
            if (sceneBehavior == null || sceneBehavior.numberPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "SceneBehavior 或 numberPrefab 为空", "确定");
                return;
            }
            
            var numberPrefab = sceneBehavior.numberPrefab;
            
            // 选择一个Actor作为测试位置（如果有选中的Actor）
            Vector2 basePosition = new Vector2(10, 10);
            if (m_selectedActorIndex >= 0 && m_selectedActorIndex < m_actors.Count)
            {
                var actor = m_actors[m_selectedActorIndex];
                if (actor != null && !actor.IsDestroyed)
                {
                    basePosition = actor.Position;
                }
            }
            
            // 测试不同的伤害类型和暴击状态
            float offsetX = 0f;
            
            // 1. 普通物理伤害（浅红色）
            var dn1 = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0.5f), 100);
            dn1.SetColor(HexToColor("#FFB6C1")); // COLOR_PHYSICAL
            offsetX += 0.5f;
            
            // 2. 暴击物理伤害（深红色）
            var dn2 = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0.5f), 250);
            dn2.SetColor(HexToColor("#DC143C")); // COLOR_PHYSICAL_CRIT
            offsetX += 0.5f;
            
            // 3. 普通法术伤害（浅蓝色）
            var dn3 = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0.5f), 80);
            dn3.SetColor(HexToColor("#87CEEB")); // COLOR_MAGICAL
            offsetX += 0.5f;
            
            // 4. 暴击法术伤害（深蓝色）
            var dn4 = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0.5f), 200);
            dn4.SetColor(HexToColor("#1E90FF")); // COLOR_MAGICAL_CRIT
            offsetX += 0.5f;
            
            // 5. 元素伤害（小字体，位置靠下）
            var dn5 = numberPrefab.Spawn(basePosition + new Vector2(0, 0.3f), 50);
            dn5.SetColor(HexToColor("#FF6B35")); // COLOR_FIRE
            dn5.SetScale(0.8f); // 字体小一点
            
            Log.Info("ActorTestTool: 已创建测试伤害数字");
        }
        
        /// <summary>
        /// 将十六进制颜色字符串转换为 Color
        /// </summary>
        private UnityEngine.Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length == 6)
            {
                int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, 1f);
            }
            return UnityEngine.Color.white;
        }

        /// <summary>
        /// 绘制Actor控制区域
        /// </summary>
        private void DrawActorControlSection()
        {
            EditorGUILayout.LabelField("Actor Control", EditorStyles.boldLabel);
            
            // Actor列表
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Actors ({m_actors.Count}):", EditorStyles.miniLabel);
            
            m_actorScrollPos = EditorGUILayout.BeginScrollView(m_actorScrollPos, GUILayout.Height(150));
            
            for (int i = 0; i < m_actors.Count; i++)
            {
                var actor = m_actors[i];
                if (actor == null || actor.IsDestroyed)
                {
                    continue;
                }
                
                EditorGUILayout.BeginHorizontal();
                
                // 选择按钮
                bool isSelected = (m_selectedActorIndex == i);
                bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                if (newSelected != isSelected)
                {
                    m_selectedActorIndex = newSelected ? i : -1;
                    m_selectedTargetIndex = -1; // 清除目标选择
                }
                
                // Actor信息（使用配置表中的名字）
                string actorName = GetActorDisplayName(actor);
                string actorInfo = $"[{i}] {actor.Tag} - {actorName} - Pos:({actor.Position.x:F1}, {actor.Position.y:F1})";
                
                EditorGUILayout.LabelField(actorInfo, EditorStyles.label);
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 控制选项
            if (m_selectedActorIndex >= 0 && m_selectedActorIndex < m_actors.Count)
            {
                var selectedActor = m_actors[m_selectedActorIndex];
                if (selectedActor != null && !selectedActor.IsDestroyed)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    string attackerName = GetActorDisplayName(selectedActor);
                    EditorGUILayout.LabelField($"Attacker: {attackerName}", EditorStyles.boldLabel);
                    
                    // 显示攻击者不为0的属性
                    DrawActorProperties(selectedActor, "Attacker Properties", ref m_attackerPropsScrollPos);
                    
                    EditorGUILayout.Space(5);
                    
                    // 目标选择
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Target:", GUILayout.Width(80));
                    
                    // 目标选择
                    List<string> targetOptions = new List<string> { "None" };
                    List<int> targetIndices = new List<int> { -1 };
                    
                    for (int i = 0; i < m_actors.Count; i++)
                    {
                        var actor = m_actors[i];
                        if (actor != null && !actor.IsDestroyed && actor != selectedActor)
                        {
                            string targetName = GetActorDisplayName(actor);
                            targetOptions.Add($"[{i}] {actor.Tag} - {targetName}");
                            targetIndices.Add(i);
                        }
                    }
                    
                    int currentTargetIndex = targetIndices.IndexOf(m_selectedTargetIndex);
                    if (currentTargetIndex < 0) currentTargetIndex = 0;
                    
                    int newTargetIndex = EditorGUILayout.Popup(currentTargetIndex, targetOptions.ToArray());
                    if (newTargetIndex >= 0 && newTargetIndex < targetIndices.Count)
                    {
                        m_selectedTargetIndex = targetIndices[newTargetIndex];
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // 显示目标不为0的属性
                    if (m_selectedTargetIndex >= 0 && m_selectedTargetIndex < m_actors.Count)
                    {
                        var targetActor = m_actors[m_selectedTargetIndex];
                        if (targetActor != null && !targetActor.IsDestroyed)
                        {
                            EditorGUILayout.Space(5);
                            string targetName = GetActorDisplayName(targetActor);
                            EditorGUILayout.LabelField($"Target: {targetName}", EditorStyles.boldLabel);
                            DrawActorProperties(targetActor, "Target Properties", ref m_targetPropsScrollPos);
                        }
                    }
                    
                    EditorGUILayout.Space(5);
                    
                    // 攻击按钮
                    if (m_selectedTargetIndex >= 0 && m_selectedTargetIndex < m_actors.Count)
                    {
                        var targetActor = m_actors[m_selectedTargetIndex];
                        if (targetActor != null && !targetActor.IsDestroyed)
                        {
                            if (GUILayout.Button("Attack Once", GUILayout.Height(30)))
                            {
                                AttackOnce(selectedActor, targetActor);
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Please select a target first", MessageType.Info);
                    }
                    
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space(5);
                    
                    // 批量攻击按钮
                    EditorGUILayout.LabelField("Batch Attack", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("All Towers/Players → Enemies", GUILayout.Height(30)))
                    {
                        BatchAttack(UnitTag.Player, UnitTag.Enemy);
                        BatchAttack(UnitTag.Tower, UnitTag.Enemy);
                    }
                    
                    if (GUILayout.Button("All Enemies → Players/Towers", GUILayout.Height(30)))
                    {
                        BatchAttack(UnitTag.Enemy, UnitTag.Player);
                        BatchAttack(UnitTag.Enemy, UnitTag.Tower);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
        /// <summary>
        /// 绘制Actor属性（只显示不为0的属性）
        /// </summary>
        private void DrawActorProperties(GameActor actor, string title, ref Vector2 scrollPos)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniLabel);
            
            var numericCmp = actor.GetComponent<NumericComponent>();
            if (numericCmp == null)
            {
                EditorGUILayout.LabelField("No NumericComponent", EditorStyles.helpBox);
                return;
            }
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 获取所有数值
            var numericDicField = typeof(NumericComponent).GetField("NumericDic", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (numericDicField != null)
            {
                var numericDic = numericDicField.GetValue(numericCmp) as Dictionary<int, int>;
                
                if (numericDic != null && numericDic.Count > 0)
                {
                    // 只显示不为0的属性
                    var nonZeroProps = numericDic.Where(kvp => kvp.Value != 0)
                        .OrderBy(kvp => kvp.Key)
                        .ToList();
                    
                    if (nonZeroProps.Count > 0)
                    {
                        foreach (var kvp in nonZeroProps)
                        {
                            NumericType numericType = (NumericType)kvp.Key;
                            bool isFloat = NumericComponent.IsFloatType(numericType);
                            
                            EditorGUILayout.BeginHorizontal();
                            
                            if (isFloat)
                            {
                                float value = numericCmp.Get<float>(numericType);
                                EditorGUILayout.LabelField($"{numericType}:", GUILayout.Width(150));
                                EditorGUILayout.LabelField($"{value:F4}", EditorStyles.miniLabel);
                            }
                            else
                            {
                                int value = numericCmp.Get<int>(numericType);
                                EditorGUILayout.LabelField($"{numericType}:", GUILayout.Width(150));
                                EditorGUILayout.LabelField($"{value}", EditorStyles.miniLabel);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No non-zero properties", EditorStyles.miniLabel);
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 获取Actor的显示名称（从配置表）
        /// </summary>
        private string GetActorDisplayName(GameActor actor)
        {
            // 尝试从UnitConfig获取
            var unitConfig = actor.GetConfig<UnitConfig>();
            if (unitConfig != null)
            {
                return unitConfig.Name;
            }
            
            // 尝试从TowerConfig获取
            var towerConfig = actor.GetConfig<TowerConfig>();
            if (towerConfig != null)
            {
                return towerConfig.Name;
            }
            
            // 尝试从BulletConfig获取
            var bulletConfig = actor.GetConfig<BulletConfig>();
            if (bulletConfig != null)
            {
                return bulletConfig.Name;
            }
            
            // 默认返回Tag
            return actor.Tag.ToString();
        }
        
        /// <summary>
        /// 绘制Buff区域
        /// </summary>
        private void DrawBuffSection()
        {
            EditorGUILayout.LabelField("Add Buff", EditorStyles.boldLabel);
            
            // 检查配置系统是否已初始化
            if (ConfigSystem.Instance == null || ConfigSystem.Instance.Tables == null)
            {
                EditorGUILayout.HelpBox("配置系统未初始化，请在 Play 模式下使用", MessageType.Warning);
                if (GUILayout.Button("Refresh Config"))
                {
                    RefreshUnitIds();
                    RefreshTowerIds();
                    RefreshBuffIds();
                }
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            // Buff ID选择
            EditorGUILayout.LabelField("Buff ID:", GUILayout.Width(80));
            
            // 搜索过滤
            m_buffSearchFilter = EditorGUILayout.TextField(m_buffSearchFilter, GUILayout.Width(100));
            
            // 过滤后的Buff ID列表
            var filteredBuffIds = m_availableBuffIds.Where(id => 
                string.IsNullOrEmpty(m_buffSearchFilter) || 
                id.ToString().Contains(m_buffSearchFilter) ||
                (ConfigSystem.Instance?.Tables?.TbBuff?.GetOrDefault(id)?.Name?.Contains(m_buffSearchFilter) ?? false)).ToList();
            
            if (filteredBuffIds.Count > 0)
            {
                int currentIndex = filteredBuffIds.IndexOf(m_selectedBuffId);
                if (currentIndex < 0) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, 
                    filteredBuffIds.Select(id => 
                    {
                        var config = ConfigSystem.Instance?.Tables?.TbBuff?.GetOrDefault(id);
                        return config != null ? $"{id} - {config.Name}" : id.ToString();
                    }).ToArray(), GUILayout.Width(200));
                
                if (newIndex >= 0 && newIndex < filteredBuffIds.Count)
                {
                    m_selectedBuffId = filteredBuffIds[newIndex];
                }
            }
            else
            {
                EditorGUILayout.LabelField("No buffs available", EditorStyles.helpBox);
                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    RefreshBuffIds();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 目标对象选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Actor:", GUILayout.Width(80));
            
            // 目标选择下拉框
            List<string> targetOptions = new List<string> { "None" };
            List<int> targetIndices = new List<int> { -1 };
            
            for (int i = 0; i < m_actors.Count; i++)
            {
                var actor = m_actors[i];
                if (actor != null && !actor.IsDestroyed)
                {
                    string targetName = GetActorDisplayName(actor);
                    targetOptions.Add($"[{i}] {actor.Tag} - {targetName}");
                    targetIndices.Add(i);
                }
            }
            
            int currentTargetIndex = targetIndices.IndexOf(m_selectedActorIndex);
            if (currentTargetIndex < 0) currentTargetIndex = 0;
            
            int newTargetIndex = EditorGUILayout.Popup(currentTargetIndex, targetOptions.ToArray(), GUILayout.Width(300));
            if (newTargetIndex >= 0 && newTargetIndex < targetIndices.Count)
            {
                m_selectedActorIndex = targetIndices[newTargetIndex];
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 添加Buff按钮
            if (m_selectedActorIndex >= 0 && m_selectedActorIndex < m_actors.Count)
            {
                var selectedActor = m_actors[m_selectedActorIndex];
                if (selectedActor != null && !selectedActor.IsDestroyed)
                {
                    if (GUILayout.Button($"Add Buff to Selected Actor", GUILayout.Height(30)))
                    {
                        if (m_selectedBuffId > 0)
                        {
                            AddBuffToActor(selectedActor, m_selectedBuffId);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", "请先选择一个Buff ID", "OK");
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a target actor first", MessageType.Info);
            }
        }
        
        /// <summary>
        /// 刷新Unit ID列表
        /// </summary>
        private void RefreshUnitIds()
        {
            m_availableUnitIds.Clear();
            
            try
            {
                if (ConfigSystem.Instance?.Tables?.TbUnit != null)
                {
                    var dataList = ConfigSystem.Instance.Tables.TbUnit.DataList;
                    if (dataList != null)
                    {
                        foreach (var config in dataList)
                        {
                            if (config != null)
                            {
                                m_availableUnitIds.Add(config.Id);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ActorTestTool: 刷新Unit ID列表失败: {ex.Message}");
            }
            
            m_availableUnitIds.Sort();
            
            // 如果列表为空，尝试延迟刷新
            if (m_availableUnitIds.Count == 0 && Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (ConfigSystem.Instance?.Tables?.TbUnit != null)
                    {
                        RefreshUnitIds();
                    }
                };
            }
        }
        
        /// <summary>
        /// 刷新Tower ID列表
        /// </summary>
        private void RefreshTowerIds()
        {
            m_availableTowerIds.Clear();
            
            try
            {
                if (ConfigSystem.Instance?.Tables?.TbTower != null)
                {
                    var dataList = ConfigSystem.Instance.Tables.TbTower.DataList;
                    if (dataList != null)
                    {
                        foreach (var config in dataList)
                        {
                            if (config != null)
                            {
                                m_availableTowerIds.Add(config.Id);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ActorTestTool: 刷新Tower ID列表失败: {ex.Message}");
            }
            
            m_availableTowerIds.Sort();
            
            // 如果列表为空，尝试延迟刷新
            if (m_availableTowerIds.Count == 0 && Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (ConfigSystem.Instance?.Tables?.TbTower != null)
                    {
                        RefreshTowerIds();
                    }
                };
            }
        }
        
        /// <summary>
        /// 刷新Buff ID列表
        /// </summary>
        private void RefreshBuffIds()
        {
            m_availableBuffIds.Clear();
            
            try
            {
                if (ConfigSystem.Instance?.Tables?.TbBuff != null)
                {
                    var dataList = ConfigSystem.Instance.Tables.TbBuff.DataList;
                    if (dataList != null)
                    {
                        foreach (var config in dataList)
                        {
                            if (config != null)
                            {
                                m_availableBuffIds.Add(config.Id);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ActorTestTool: 刷新Buff ID列表失败: {ex.Message}");
            }
            
            m_availableBuffIds.Sort();
            
            // 如果列表为空，尝试延迟刷新
            if (m_availableBuffIds.Count == 0 && Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (ConfigSystem.Instance?.Tables?.TbBuff != null)
                    {
                        RefreshBuffIds();
                    }
                };
            }
        }
        
        /// <summary>
        /// 刷新Actor列表
        /// </summary>
        private void RefreshActors()
        {
            m_actors.Clear();
            
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (ActorMgr.Instance != null && ActorMgr.Instance.Actors != null)
            {
                foreach (var actor in ActorMgr.Instance.Actors)
                {
                    if (actor != null && !actor.IsDestroyed)
                    {
                        m_actors.Add(actor);
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建单位
        /// </summary>
        private void CreateUnit(int unitId, UnitTag tag)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "请在 Play 模式下使用", "OK");
                return;
            }
            
            if (ActorMgr.Instance == null)
            {
                EditorUtility.DisplayDialog("Error", "ActorMgr未初始化", "OK");
                return;
            }
            
            Vector2 pos = new Vector2(10, 10);
            if (tag == UnitTag.Player)
            {
                ActorMgr.Instance.CreatePlayer(unitId, pos);
            }
            else if (tag == UnitTag.Enemy)
            {
                // 敌人在右边
                pos = new Vector2(15, 10);
                ActorMgr.Instance.CreateEnemyByUnitId(unitId, pos);
            }
            
            RefreshActors();
        }
        
        /// <summary>
        /// 创建塔
        /// </summary>
        private void CreateTower(int towerId)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "请在 Play 模式下使用", "OK");
                return;
            }
            
            if (ActorMgr.Instance == null)
            {
                EditorUtility.DisplayDialog("Error", "ActorMgr未初始化", "OK");
                return;
            }
            
            // 塔在左边
            Vector2 pos = new Vector2(5, 10);
            ActorMgr.Instance.CreateTower(towerId, pos);
            
            RefreshActors();
        }
        
        /// <summary>
        /// 攻击一次（直接调用 CombatHelper.PerformAttack）
        /// </summary>
        private async void AttackOnce(GameActor attacker, GameActor target)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (attacker == null || target == null || attacker.IsDestroyed || target.IsDestroyed)
            {
                return;
            }
            
            // 直接调用 CombatHelper.PerformAttack
            bool success = await CombatHelper.PerformAttack(attacker, target);
            if (!success)
            {
                Log.Warning($"ActorTestTool: 攻击失败，攻击者: {GetActorDisplayName(attacker)}, 目标: {GetActorDisplayName(target)}");
            }
        }
        
        /// <summary>
        /// 批量攻击：所有指定类型的攻击者攻击所有指定类型的目标
        /// </summary>
        private void BatchAttack(UnitTag attackerTag, UnitTag targetTag)
        {
            if (!Application.isPlaying || ActorMgr.Instance == null)
            {
                return;
            }
            
            // 获取所有攻击者
            List<GameActor> attackers = new List<GameActor>();
            foreach (var actor in ActorMgr.Instance.Actors)
            {
                if (actor != null && !actor.IsDestroyed && actor.Tag == attackerTag)
                {
                    attackers.Add(actor);
                }
            }
            
            if (attackers.Count == 0)
            {
                Log.Info($"ActorTestTool: 没有找到 {attackerTag} 类型的攻击者");
                return;
            }
            
            // 获取所有目标
            List<GameActor> targets = new List<GameActor>();
            foreach (var actor in ActorMgr.Instance.Actors)
            {
                if (actor != null && !actor.IsDestroyed && actor.Tag == targetTag)
                {
                    targets.Add(actor);
                }
            }
            
            if (targets.Count == 0)
            {
                Log.Info($"ActorTestTool: 没有找到 {targetTag} 类型的目标");
                return;
            }
            
            // 每个攻击者攻击所有目标
            int attackCount = 0;
            foreach (var attacker in attackers)
            {
                foreach (var target in targets)
                {
                    AttackOnce(attacker, target);
                    attackCount++;
                }
            }
            
            Log.Info($"ActorTestTool: 批量攻击完成，{attackers.Count} 个 {attackerTag} 攻击 {targets.Count} 个 {targetTag}，共 {attackCount} 次攻击");
        }
        
        /// <summary>
        /// 添加Buff到Actor
        /// </summary>
        private void AddBuffToActor(GameActor actor, int buffId)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (actor == null || actor.IsDestroyed)
            {
                return;
            }
            
            var buffCmp = actor.GetComponent<BuffCmp>();
            if (buffCmp == null)
            {
                Log.Warning($"ActorTestTool: 该Actor没有BuffCmp组件");
                return;
            }
            
            buffCmp.AddBuff(buffId);
            Log.Info($"ActorTestTool: 给 {GetActorDisplayName(actor)} 添加Buff {buffId}");
        }
        
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (m_autoRefresh)
            {
                Repaint();
            }
        }
    }
}
