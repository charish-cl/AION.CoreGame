using System;

namespace AION.CoreFramework
{
    public class StorageInt
    {
        private int _value;
        private string _key;
        private int _defaultValue;
        private TimeSpan _interval;
        private DateTime _lastCheckTime;

        public int Value
        {
            get
            {
                if (IsIntervalElapsed())
                {
                    _value = _defaultValue;
                    Save();
                }
                else
                {
                    _value = Load();
                }
                return _value;
            }
            set
            {
                _value = value;
                Save();
            }
        }

        public StorageInt(string key, int defaultValue = 0, TimeSpan? interval = null)
        {
            _key = key;
            _defaultValue = defaultValue;
            _interval = interval ?? TimeSpan.FromDays(1); // 默认间隔为一天
            _lastCheckTime = LoadLastCheckTime();

            if (IsIntervalElapsed())
            {
                _value = _defaultValue;
                Save();
            }
            else
            {
                _value = Load();
            }
        }

        /// <summary>
        /// 判断是否超过了指定的间隔时间
        /// </summary>
        /// <returns></returns>
        private bool IsIntervalElapsed()
        {
            return DateTime.Now - _lastCheckTime >= _interval;
        }

        // 加载值
        private int Load()
        {
            // return GFBuiltin.Setting.GetInt(_key, _defaultValue);
            return _defaultValue;
        }

        // 保存值到设置
        private void Save()
        {
            // GFBuiltin.Setting.SetInt(_key, _value);
            // GFBuiltin.Setting.SetDateTime("LastCheckTime_" + _key, DateTime.Now);
            // GFBuiltin.Setting.Save();
            // _lastCheckTime = DateTime.Now;
        }

        // 加载上次检查时间
        private DateTime LoadLastCheckTime()
        {
            // return GFBuiltin.Setting.GetDateTime("LastCheckTime_" + _key, DateTime.MinValue);
            return DateTime.MinValue;
        }

        public int Modify(int delta)
        {
            _value += delta;
            Save();
            return _value;
        }
    }
}