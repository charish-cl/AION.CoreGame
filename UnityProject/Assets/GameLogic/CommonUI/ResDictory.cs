using System.Collections.Generic;

namespace AION.CoreFramework
{
    public class ResDictory<T>
    {
        public List<T> RawList = new List<T>();
        public Dictionary<string, T> Dict = new Dictionary<string, T>();
        
        
        
    }
}