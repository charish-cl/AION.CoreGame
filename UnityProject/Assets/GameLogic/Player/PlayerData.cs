using System.Collections.Generic;
using GameBase;
using AION.CoreFramework;

namespace GameLogic.Player
{
    public enum PlayerDataType
    {
        //钻石
        Diamonds,
        //金币
        Coins,
        //经验
        Experience,
        //体力
        Stamina,
        //力量
        Power,
    }
    public class PlayerData:Singleton<PlayerData>
    {
        private Dictionary<PlayerDataType, int> _data = new Dictionary<PlayerDataType, int>();


        public PlayerData()
        {
            _data.Add(PlayerDataType.Diamonds, 0);
            _data.Add(PlayerDataType.Coins, 0);
            _data.Add(PlayerDataType.Experience, 0);
            _data.Add(PlayerDataType.Stamina, 0);
            _data.Add(PlayerDataType.Power, 0);
        }


        //索引器访问字典
        public int this[PlayerDataType dataType]
        {
            get
            {
                if (!_data.ContainsKey(dataType))
                {
                    Log.Error("PlayerData: " + dataType + " not exist");
                    return 0;
                }
                return _data[dataType];
            }
          
        }
        
        
        public bool CheckEnough(PlayerDataType dataType, int value)
        {
            return _data[dataType] >= value;
        }
    }
}