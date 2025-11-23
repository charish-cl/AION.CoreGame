// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;
// using System.Linq;
// using GameLogic;
// using GameConfig;
// using GameConfig.battle;
// using AION.CoreFramework;
//
// namespace GameLogic.Editor
// {
//     /// <summary>
//     /// Actor测试工具窗口
//     /// </summary>
//     public class ActorTestTool : EditorWindow
//     {
//         private Vector2 m_unitScrollPos;
//         private Vector2 m_actorScrollPos;
//         private Vector2 m_buffScrollPos;
//         
//         // Unit选择
//         private List<int> m_availableUnitIds = new List<int>();
//         private int m_selectedUnitId = 0;
//         private string m_unitSearchFilter = "";
//         
//         // Actor列表
//         private List<GameActor> m_actors = new List<GameActor>();
//         private int m_selectedActorIndex = -1;
//         private int m_selectedTargetIndex = -1;
//         
//         // Buff选择
//         private List<int> m_availableBuffIds = new List<int>();
//         private int m_selectedBuffId = 0;
//         private string m_buffSearchFilter = "";
//         
//         // 控制选项
//         private bool m_autoRefresh = true;
//         private float m_attackInterval = 1f;
//         private float m_attackRange = 5f;
//         
//         [MenuItem("Tools/Actor Test Tool")]
//         public static void ShowWindow()
//         {
//             GetWindow<ActorTestTool>("Actor Test Tool");
//         }
//         
//         private void OnEnable()
//         {
//             RefreshUnitIds();
//             RefreshBuffIds();
//             RefreshActors();
//         }
//         
//         private void OnGUI()
//         {
//             EditorGUILayout.BeginVertical();
//             
//             // 标题
//             EditorGUILayout.LabelField("Actor Test Tool", EditorStyles.boldLabel);
//             EditorGUILayout.Space(5);
//             
//             // 自动刷新选项
//             m_autoRefresh = EditorGUILayout.Toggle("Auto Refresh", m_autoRefresh);
//             
//             if (m_autoRefresh || GUILayout.Button("Refresh Actors"))
//             {
//                 RefreshActors();
//             }
//             
//             EditorGUILayout.Space(10);
//             EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//             EditorGUILayout.Space(10);
//             
//             // 创建单位区域
//             DrawCreateUnitSection();
//             
//             EditorGUILayout.Space(10);
//             EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//             EditorGUILayout.Space(10);
//             
//             // Actor列表和控制区域
//             DrawActorControlSection();
//             
//             EditorGUILayout.Space(10);
//             EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//             EditorGUILayout.Space(10);
//             
//             // Buff添加区域
//             DrawBuffSection();
//             
//             EditorGUILayout.EndVertical();
//         }
//         
//         /// <summary>
//         /// 绘制创建单位区域
//         /// </summary>
//         private void DrawCreateUnitSection()
//         {
//             EditorGUILayout.LabelField("Create Unit", EditorStyles.boldLabel);
//             
//             EditorGUILayout.BeginHorizontal();
//             
//             // Unit ID选择
//             EditorGUILayout.LabelField("Unit ID:", GUILayout.Width(80));
//             
//             // 搜索过滤
//             m_unitSearchFilter = EditorGUILayout.TextField(m_unitSearchFilter, GUILayout.Width(100));
//             
//             // 过滤后的Unit ID列表
//             var filteredUnitIds = m_availableUnitIds.Where(id => 
//                 string.IsNullOrEmpty(m_unitSearchFilter) || 
//                 id.ToString().Contains(m_unitSearchFilter)).ToList();
//             
//             if (filteredUnitIds.Count > 0)
//             {
//                 int currentIndex = filteredUnitIds.IndexOf(m_selectedUnitId);
//                 if (currentIndex < 0) currentIndex = 0;
//                 
//                 int newIndex = EditorGUILayout.Popup(currentIndex, 
//                     filteredUnitIds.Select(id => 
//                     {
//                         var config = ConfigSystem.Instance?.Tables?.TbUnit?.GetOrDefault(id);
//                         return config != null ? $"{id} - {config.Name}" : id.ToString();
//                     }).ToArray(), GUILayout.Width(200));
//                 
//                 if (newIndex >= 0 && newIndex < filteredUnitIds.Count)
//                 {
//                     m_selectedUnitId = filteredUnitIds[newIndex];
//                 }
//             }
//             else
//             {
//                 EditorGUILayout.LabelField("No units available", EditorStyles.helpBox);
//             }
//             
//             EditorGUILayout.EndHorizontal();
//             
//             EditorGUILayout.Space(5);
//             
//             // 创建按钮
//             EditorGUILayout.BeginHorizontal();
//             
//             if (GUILayout.Button("Create Hero", GUILayout.Height(30)))
//             {
//                 CreateUnit(m_selectedUnitId, UnitTag.Player);
//             }
//             
//             if (GUILayout.Button("Create Enemy", GUILayout.Height(30)))
//             {
//                 CreateUnit(m_selectedUnitId, UnitTag.Enemy);
//             }
//             
//             if (GUILayout.Button("Create Tower", GUILayout.Height(30)))
//             {
//                 CreateTower(m_selectedUnitId);
//             }
//             
//             EditorGUILayout.EndHorizontal();
//         }
//         
//         /// <summary>
//         /// 绘制Actor控制区域
//         /// </summary>
//         private void DrawActorControlSection()
//         {
//             EditorGUILayout.LabelField("Actor Control", EditorStyles.boldLabel);
//             
//             // Actor列表
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField($"Actors ({m_actors.Count}):", EditorStyles.miniLabel);
//             
//             m_actorScrollPos = EditorGUILayout.BeginScrollView(m_actorScrollPos, GUILayout.Height(150));
//             
//             for (int i = 0; i < m_actors.Count; i++)
//             {
//                 var actor = m_actors[i];
//                 if (actor == null || actor.IsDestroyed)
//                 {
//                     continue;
//                 }
//                 
//                 EditorGUILayout.BeginHorizontal();
//                 
//                 // 选择按钮
//                 bool isSelected = (m_selectedActorIndex == i);
//                 bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
//                 if (newSelected != isSelected)
//                 {
//                     m_selectedActorIndex = newSelected ? i : -1;
//                 }
//                 
//                 // Actor信息
//                 string actorInfo = $"[{i}] {actor.Tag} - Pos:({actor.Position.x:F1}, {actor.Position.y:F1})";
//                 var unitCmp = actor.GetComponent<UnitComponent>();
//                 if (unitCmp != null && unitCmp.IsConfigValid)
//                 {
//                     actorInfo += $" - {unitCmp.Name}";
//                 }
//                 
//                 EditorGUILayout.LabelField(actorInfo, EditorStyles.label);
//                 
//                 EditorGUILayout.EndHorizontal();
//             }
//             
//             EditorGUILayout.EndScrollView();
//             EditorGUILayout.EndVertical();
//             
//             EditorGUILayout.Space(5);
//             
//             // 控制选项
//             if (m_selectedActorIndex >= 0 && m_selectedActorIndex < m_actors.Count)
//             {
//                 var selectedActor = m_actors[m_selectedActorIndex];
//                 if (selectedActor != null && !selectedActor.IsDestroyed)
//                 {
//                     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//                     EditorGUILayout.LabelField($"Selected: {selectedActor.Tag}", EditorStyles.boldLabel);
//                     
//                     // 攻击控制
//                     EditorGUILayout.BeginHorizontal();
//                     EditorGUILayout.LabelField("Target:", GUILayout.Width(80));
//                     
//                     // 目标选择
//                     List<string> targetOptions = new List<string> { "None" };
//                     List<int> targetIndices = new List<int> { -1 };
//                     
//                     for (int i = 0; i < m_actors.Count; i++)
//                     {
//                         var actor = m_actors[i];
//                         if (actor != null && !actor.IsDestroyed && actor != selectedActor)
//                         {
//                             targetOptions.Add($"[{i}] {actor.Tag}");
//                             targetIndices.Add(i);
//                         }
//                     }
//                     
//                     int currentTargetIndex = targetIndices.IndexOf(m_selectedTargetIndex);
//                     if (currentTargetIndex < 0) currentTargetIndex = 0;
//                     
//                     int newTargetIndex = EditorGUILayout.Popup(currentTargetIndex, targetOptions.ToArray());
//                     if (newTargetIndex >= 0 && newTargetIndex < targetIndices.Count)
//                     {
//                         m_selectedTargetIndex = targetIndices[newTargetIndex];
//                     }
//                     
//                     EditorGUILayout.EndHorizontal();
//                     
//                     EditorGUILayout.Space(5);
//                     
//                     // 攻击参数
//                     m_attackInterval = EditorGUILayout.FloatField("Attack Interval:", m_attackInterval);
//                     m_attackRange = EditorGUILayout.FloatField("Attack Range:", m_attackRange);
//                     
//                     EditorGUILayout.Space(5);
//                     
//                     // 控制按钮
//                     EditorGUILayout.BeginHorizontal();
//                     
//                     if (GUILayout.Button("Set Attack Target"))
//                     {
//                         SetAttackTarget(selectedActor, m_selectedTargetIndex);
//                     }
//                     
//                     if (GUILayout.Button("Remove Control"))
//                     {
//                         RemoveControl(selectedActor);
//                     }
//                     
//                     EditorGUILayout.EndHorizontal();
//                     
//                     EditorGUILayout.EndVertical();
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 绘制Buff区域
//         /// </summary>
//         private void DrawBuffSection()
//         {
//             EditorGUILayout.LabelField("Add Buff", EditorStyles.boldLabel);
//             
//             EditorGUILayout.BeginHorizontal();
//             
//             // Buff ID选择
//             EditorGUILayout.LabelField("Buff ID:", GUILayout.Width(80));
//             
//             // 搜索过滤
//             m_buffSearchFilter = EditorGUILayout.TextField(m_buffSearchFilter, GUILayout.Width(100));
//             
//             // 过滤后的Buff ID列表
//             var filteredBuffIds = m_availableBuffIds.Where(id => 
//                 string.IsNullOrEmpty(m_buffSearchFilter) || 
//                 id.ToString().Contains(m_buffSearchFilter)).ToList();
//             
//             if (filteredBuffIds.Count > 0)
//             {
//                 int currentIndex = filteredBuffIds.IndexOf(m_selectedBuffId);
//                 if (currentIndex < 0) currentIndex = 0;
//                 
//                 int newIndex = EditorGUILayout.Popup(currentIndex, 
//                     filteredBuffIds.Select(id => 
//                     {
//                         var config = ConfigSystem.Instance?.Tables?.TbBuff?.GetOrDefault(id);
//                         return config != null ? $"{id} - {config.Name}" : id.ToString();
//                     }).ToArray(), GUILayout.Width(200));
//                 
//                 if (newIndex >= 0 && newIndex < filteredBuffIds.Count)
//                 {
//                     m_selectedBuffId = filteredBuffIds[newIndex];
//                 }
//             }
//             else
//             {
//                 EditorGUILayout.LabelField("No buffs available", EditorStyles.helpBox);
//             }
//             
//             EditorGUILayout.EndHorizontal();
//             
//             EditorGUILayout.Space(5);
//             
//             // 添加Buff按钮
//             if (m_selectedActorIndex >= 0 && m_selectedActorIndex < m_actors.Count)
//             {
//                 var selectedActor = m_actors[m_selectedActorIndex];
//                 if (selectedActor != null && !selectedActor.IsDestroyed)
//                 {
//                     if (GUILayout.Button($"Add Buff to Selected Actor", GUILayout.Height(30)))
//                     {
//                         AddBuffToActor(selectedActor, m_selectedBuffId);
//                     }
//                 }
//             }
//             else
//             {
//                 EditorGUILayout.HelpBox("Please select an actor first", MessageType.Info);
//             }
//         }
//         
//         /// <summary>
//         /// 刷新Unit ID列表
//         /// </summary>
//         private void RefreshUnitIds()
//         {
//             m_availableUnitIds.Clear();
//             
//             if (ConfigSystem.Instance?.Tables?.TbUnit != null)
//             {
//                 var dataList = ConfigSystem.Instance.Tables.TbUnit.DataList;
//                 if (dataList != null)
//                 {
//                     foreach (var config in dataList)
//                     {
//                         if (config != null)
//                         {
//                             m_availableUnitIds.Add(config.Id);
//                         }
//                     }
//                 }
//             }
//             
//             m_availableUnitIds.Sort();
//         }
//         
//         /// <summary>
//         /// 刷新Buff ID列表
//         /// </summary>
//         private void RefreshBuffIds()
//         {
//             m_availableBuffIds.Clear();
//             
//             if (ConfigSystem.Instance?.Tables?.TbBuff != null)
//             {
//                 var dataList = ConfigSystem.Instance.Tables.TbBuff.DataList;
//                 if (dataList != null)
//                 {
//                     foreach (var config in dataList)
//                     {
//                         if (config != null)
//                         {
//                             m_availableBuffIds.Add(config.Id);
//                         }
//                     }
//                 }
//             }
//             
//             m_availableBuffIds.Sort();
//         }
//         
//         /// <summary>
//         /// 刷新Actor列表
//         /// </summary>
//         private void RefreshActors()
//         {
//             m_actors.Clear();
//             
//             if (ActorMgr.Instance != null && ActorMgr.Instance.Actors != null)
//             {
//                 foreach (var actor in ActorMgr.Instance.Actors)
//                 {
//                     if (actor != null && !actor.IsDestroyed)
//                     {
//                         m_actors.Add(actor);
//                     }
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 创建单位
//         /// </summary>
//         private void CreateUnit(int unitId, UnitTag tag)
//         {
//             if (ActorMgr.Instance == null)
//             {
//                 EditorUtility.DisplayDialog("Error", "ActorMgr未初始化", "OK");
//                 return;
//             }
//             
//             if (tag == UnitTag.Player)
//             {
//                 ActorMgr.Instance.CreatePlayer(unitId);
//             }
//             else if (tag == UnitTag.Enemy)
//             {
//                 ActorMgr.Instance.CreateEnemyByUnitId(unitId);
//             }
//             
//             RefreshActors();
//         }
//         
//         /// <summary>
//         /// 创建塔
//         /// </summary>
//         private void CreateTower(int towerId)
//         {
//             if (ActorMgr.Instance == null)
//             {
//                 EditorUtility.DisplayDialog("Error", "ActorMgr未初始化", "OK");
//                 return;
//             }
//             
//             Vector2 pos = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
//             ActorMgr.Instance.CreateTower(towerId, pos);
//             
//             RefreshActors();
//         }
//         
//         /// <summary>
//         /// 设置攻击目标
//         /// </summary>
//         private void SetAttackTarget(GameActor actor, int targetIndex)
//         {
//             if (targetIndex < 0 || targetIndex >= m_actors.Count)
//             {
//                 return;
//             }
//             
//             var targetActor = m_actors[targetIndex];
//             if (targetActor == null || targetActor.IsDestroyed)
//             {
//                 return;
//             }
//             
//             // 添加或获取TestControlCmp
//             var controlCmp = actor.GetComponent<TestControlCmp>();
//             if (controlCmp == null)
//             {
//                 controlCmp = actor.AddComponent<TestControlCmp>();
//             }
//             
//             // 禁用FSM组件
//             var unitFSM = actor.GetComponent<UnitFSMCmp>();
//             var monsterFSM = actor.GetComponent<MonsterFSMCmp>();
//             var towerFSM = actor.GetComponent<TowerFSMCmp>();
//             
//             if (unitFSM != null) unitFSM.Enable = false;
//             if (monsterFSM != null) monsterFSM.Enable = false;
//             if (towerFSM != null) towerFSM.Enable = false;
//             
//             // 设置控制参数
//             controlCmp.SetTarget(targetActor);
//             controlCmp.SetAttackInterval(m_attackInterval);
//             controlCmp.SetAttackRange(m_attackRange);
//             
//             Log.Info($"ActorTestTool: 设置 {actor.Tag} 攻击目标 {targetActor.Tag}");
//         }
//         
//         /// <summary>
//         /// 移除控制
//         /// </summary>
//         private void RemoveControl(GameActor actor)
//         {
//             var controlCmp = actor.GetComponent<TestControlCmp>();
//             if (controlCmp != null)
//             {
//                 actor.RemoveComponent<TestControlCmp>();
//             }
//             
//             // 恢复FSM组件
//             var unitFSM = actor.GetComponent<UnitFSMCmp>();
//             var monsterFSM = actor.GetComponent<MonsterFSMCmp>();
//             var towerFSM = actor.GetComponent<TowerFSMCmp>();
//             
//             if (unitFSM != null) unitFSM.Enable = true;
//             if (monsterFSM != null) monsterFSM.Enable = true;
//             if (towerFSM != null) towerFSM.Enable = true;
//             
//             Log.Info($"ActorTestTool: 移除 {actor.Tag} 的控制");
//         }
//         
//         /// <summary>
//         /// 添加Buff到Actor
//         /// </summary>
//         private void AddBuffToActor(GameActor actor, int buffId)
//         {
//             if (actor == null || actor.IsDestroyed)
//             {
//                 return;
//             }
//             
//             var buffCmp = actor.GetComponent<BuffCmp>();
//             if (buffCmp == null)
//             {
//                 EditorUtility.DisplayDialog("Error", "该Actor没有BuffCmp组件", "OK");
//                 return;
//             }
//             
//             buffCmp.AddBuff(buffId);
//             Log.Info($"ActorTestTool: 给 {actor.Tag} 添加Buff {buffId}");
//         }
//         
//         private void Update()
//         {
//             if (m_autoRefresh)
//             {
//                 Repaint();
//             }
//         }
//     }
// }
//
