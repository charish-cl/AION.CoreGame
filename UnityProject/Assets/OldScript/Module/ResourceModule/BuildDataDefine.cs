using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace AION.CoreFramework
{
    //这两个保存到配置里的类
    [Serializable]
    public class BuildDataDefine
    {
        public string ABName;
        public List<string> AssetList;
    }

    [Serializable]
    public class ResItem
    {
        public string ResName;

        /// <summary>
        /// 所属的AB包名
        /// </summary>
        public string ABName;

       public List<string> DependList;
    }
//TODO: 这里的ABItemLis和ResItemLis是用来保存到配置里的，可以有更好的方法的暂时这样吧
//主要JsonUtility.SerializeObject(ABItemLis)和JsonUtility.SerializeObject(ResItemLis)
//这两个方法可以将类序列化为json字符串，然后保存到配置文件里。
    [Serializable]
    public class ABItemLis
    {
        public List<BuildDataDefine> mABList = new List<BuildDataDefine>();
    }

    [Serializable]
    public class ResItemLis
    {
        public List<ResItem> mResList = new List<ResItem>();
    }
}