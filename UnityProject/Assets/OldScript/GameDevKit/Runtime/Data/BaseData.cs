using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameDevKit
{
    public abstract class BaseData:SerializedScriptableObject
    {
        
        public Guid Guid;
        
        [OnValueChanged("SetName")]
        [LabelText("名称")]
        public string Name;
        
        [LabelText("描述")]
        [TextArea(5,10)]
        public string Desc;
        
        [PreviewField(ObjectFieldAlignment.Left,Height = 100)]
        [LabelText("图标")]
        public Sprite Icon;

        public void OnEnable()
        {
            Guid=this.GetAssetSystemGuid();
        }

    
        public void SetName()
        {
            this.name = Name;
            AssetHelper.SaveAssets();
        }
    }
}