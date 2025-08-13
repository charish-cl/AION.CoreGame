using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic.Config
{
    public class BaseListScriptObject<T> : SerializedScriptableObject
    {
        [Searchable]
        [TableList]
        public List<T> list;
    }
    
    [CreateAssetMenu(fileName = "GoodListConfig",menuName = "Config/GoodListConfig", order = 0)]
    [LabelText("道具表")]
    public class GoodListConfig : BaseListScriptObject<GoodConfig>
    {
        
    }
    
 
}