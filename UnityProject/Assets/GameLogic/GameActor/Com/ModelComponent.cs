using AION.CoreFramework;
using GameConfig.res;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    /// <summary>
    /// 模型组件，根据 ModelConfig 实例化模型对象
    /// </summary>
    public class ModelComponent : GameActorCmp
    {
        /// <summary>
        /// 模型配置
        /// </summary>
        public ModelConfig ModelConfig { get; private set; }
        
        /// <summary>
        /// 实例化的 GameObject
        /// </summary>
        public GameObject ModelInstance { get; private set; }
        
        /// <summary>
        /// 初始化模型组件
        /// </summary>
        /// <param name="modelConfig">模型配置</param>
        public void Init(ModelConfig modelConfig)
        {
            ModelConfig = modelConfig;
        }
        
        /// <summary>
        /// 初始化模型组件（通过模型ID）
        /// </summary>
        /// <param name="modelId">模型配置ID</param>
        public void Init(int modelId)
        {
            if (ConfigSystem.Instance?.Tables?.TbModel != null)
            {
                ModelConfig = ConfigSystem.Instance.Tables.TbModel.GetOrDefault(modelId);
                if (ModelConfig == null)
                {
                    Log.Warning($"ModelComponent: 未找到模型配置，ModelId = {modelId}");
                }
            }
            else
            {
                Log.Error("ModelComponent: ConfigSystem 未初始化或 TbModel 为空");
            }
        }
        
        public override void OnInit()
        {
            base.OnInit();
         
            // 如果还没有加载模型，从Actor获取配置
            if (ModelConfig == null)
            {
                LoadModelFromActor();
            }
            
            // 实例化模型
            if (ModelConfig != null)
            {
                InstantiateModel();
            }
            else
            {
                Log.Warning($"ModelComponent: 模型配置为空，无法实例化模型");
            }
            
            Log.Info($"ModelComponent: 初始化模型组件，ModelId = {ModelConfig?.Id}");
        }
        
        /// <summary>
        /// 从Actor获取模型配置
        /// </summary>
        private void LoadModelFromActor()
        {
            // 尝试从UnitConfig获取
            var unitConfig = Actor.GetConfig<GameConfig.battle.UnitConfig>();
            if (unitConfig != null && unitConfig.ModelId_Ref != null)
            {
                ModelConfig = unitConfig.ModelId_Ref;
                return;
            }
            
            // 尝试从TowerConfig获取
            var towerConfig = Actor.GetConfig<GameConfig.battle.TowerConfig>();
            if (towerConfig != null && towerConfig.ModelId_Ref != null)
            {
                ModelConfig = towerConfig.ModelId_Ref;
                return;
            }
            
            // 尝试从BulletConfig获取
            var bulletConfig = Actor.GetConfig<GameConfig.battle.BulletConfig>();
            if (bulletConfig != null && bulletConfig.ModelId_Ref != null)
            {
                ModelConfig = bulletConfig.ModelId_Ref;
                return;
            }
        }
        
        /// <summary>
        /// 实例化模型
        /// </summary>
        private void InstantiateModel()
        {
            if ( string.IsNullOrEmpty(ModelConfig.Path))
            {
                Log.Warning($"ModelComponent: 模型配置无效或路径为空");
                return;
            }
            
            // 加载模型资源
            GameObject prefab = GameModule.Resource.LoadAsset<GameObject>(ModelConfig.Path);
            if (prefab == null)
            {
                Log.Error($"ModelComponent: 加载模型资源失败，路径 = {ModelConfig.Path}");
                return;
            }
            
            // 实例化模型（暂时不设置父对象，由ActorMgr.SetupActorGameObject统一处理）
            ModelInstance = Object.Instantiate(prefab);
            if (ModelInstance == null)
            {
                Log.Error($"ModelComponent: 实例化模型失败，路径 = {ModelConfig.Path}");
                return;
            }
            
            // 绑定到 GameActor
            Actor.BindGo(ModelInstance);
            
            Log.Info($"ModelComponent: 成功实例化模型，路径 = {ModelConfig.Path}");
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            // 销毁模型实例
            if (ModelInstance != null)
            {
                Object.Destroy(ModelInstance);
                ModelInstance = null;
            }
        }
        
        /// <summary>
        /// 检查模型配置是否有效
        /// </summary>
        public bool IsConfigValid => ModelConfig != null && !string.IsNullOrEmpty(ModelConfig.Path);
    }
}

