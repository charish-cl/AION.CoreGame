using System.Collections.Generic;
using GameLogic.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic
{

    public class GoodConfig
    {
        [PreviewField]
        [LabelText("图标")]
        public Sprite IconSprite;

        [LabelText("Icon")]
        public string Icon;
        
        /// <summary>
        /// Id
        /// </summary>
        public int Id;

        /// <summary>
        /// Name
        /// </summary>
        public string Name;
        
        
        /// <summary>
        /// 道具类型
        /// </summary>
        public ItemSystemType ItemSystemType;

        
        /// <summary>
        /// 描述
        /// </summary>
        public int Dec;

        /// <summary>
        /// 获取方式
        /// </summary>
        [GUIColor("#FF6666")]
        [LabelText("获取方式")]
        public List<int> GetAways;


    
    }
}