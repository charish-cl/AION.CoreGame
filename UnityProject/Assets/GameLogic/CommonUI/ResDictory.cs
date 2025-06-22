using System.Collections.Generic;

namespace AION.CoreFramework
{
    /// <summary>
    /// 这个我发现完全可以用Luban生成工具的功能取代
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResDictory<T>
    {
        public List<T> RawList = new List<T>();
        public Dictionary<string, T> Dict = new Dictionary<string, T>();
        
        
        
    }
}