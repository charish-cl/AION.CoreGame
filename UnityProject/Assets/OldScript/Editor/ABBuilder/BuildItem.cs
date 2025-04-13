using System;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework
{
    [Serializable]
    public class BuildItem
    {
            
        [LabelText("资源名称")]
        public string bundleName;
        
        [HideInInspector]
        public ResourceType resourceType;
        
        
        [LabelText("打包粒度")]
        public BundleType bundleType;
        
    
        
        [LabelText("资源路径")]
        [FolderPath]
        public string bundlePath;


        [LabelText("打包规则")]
        public string rules;

    }
}