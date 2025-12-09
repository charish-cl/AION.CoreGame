using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 本地设置基类，用于保存项目设置（不方便放到Excel的配置）
    /// 继承此类的ScriptableObject可以通过LS.Get<T>()方法加载
    /// </summary>
    public abstract class GameLocalSetting : ScriptableObject
    {
        // 基类本身没有任何方法，只是为了方便识别
    }
}

