using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace AION.Config
{
    [CreateAssetMenu(fileName = "New Tower", menuName = "TowerDefence/Tower", order = 0)]
    public class TowerSO :SerializedScriptableObject
    {

        
        [PreviewField]
        [BoxGroup("资源")]
        [LabelText("图标")]
        public Sprite Icon;
        
        [BoxGroup("资源")]
        [LabelText("名称")]
        public string Name;
        
        [BoxGroup("资源")]
        [LabelText("预制体")]
        public GameObject Prefab;

        
        [BoxGroup("逻辑")]
        [LabelText("目标筛选---包含攻击范围以及目标类型")]
        [ValueDropdown("GetDerivedBaseFindTarget")]
        public BaseTargetSelector TargetSelector;

        
        
        
        IEnumerable<ValueDropdownItem<BaseTargetSelector>> GetDerivedBaseFindTarget()
        {
           
            var types = TypeCache.GetTypesDerivedFrom<BaseTargetSelector>();
            
            for (int i = 0; i < types.Count; i++)
            {
                var target = Activator.CreateInstance(types[i]) as BaseTargetSelector;
                
                yield return new ValueDropdownItem<BaseTargetSelector>(types[i].Name, target);
            }
            
        }
        
    }
}