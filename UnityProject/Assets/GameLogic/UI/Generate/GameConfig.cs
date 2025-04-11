using System.IO;
using UnityEngine;

namespace GameLogic.UI.Generate
{
    [CreateAssetMenu]
    public class GameConfig : ScriptableObject
    {
        string path = "Assets/Resources/GameConfig.asset";

        
        
        
        public GameConfig Instance
        {
            get
            {
#if UNITY_EDITOR
                //不存在就创建
                if (!File.Exists(path))
                {
                    ScriptableObject.CreateInstance(typeof(GameConfig));
                    UnityEditor.AssetDatabase.CreateAsset(Instance, path);
                }

                var instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(path);
                return instance;
#endif
                return null;
            }
        }
    }
}