using UnityEngine.Windows;

namespace AION.CoreFramework
{
    /// <summary>
    /// 
    /// </summary>
    public enum BundleType
    {
        /// <summary>
        /// 按照每个文件打包
        /// </summary>
        File,
        /// <summary>
        /// 按照文件夹打包
        /// </summary>
        Directory,
        /// <summary>
        /// 所有打到一起
        /// </summary>
        All
    }

    public enum ResourceType
    {
        Direct,
        Dependence,
        Generate
    }
}