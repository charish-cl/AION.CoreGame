using System.Collections.Generic;

namespace SimpleSaveSystem
{
    /// <summary>通用键值对存储，用于零散配置</summary>
    public class PrefsSaveData
    {
        public Dictionary<string, int> Ints = new Dictionary<string, int>();
        public Dictionary<string, float> Floats = new Dictionary<string, float>();
        public Dictionary<string, string> Strings = new Dictionary<string, string>();
        public Dictionary<string, bool> Bools = new Dictionary<string, bool>();

        // --- 静态 API ---
        public static void SetInt(string k, int v) { var d = Get(); d.Ints[k] = v; Save(d); }
        public static int GetInt(string k, int def = 0) => Get().Ints.TryGetValue(k, out int v) ? v : def;

        public static void SetFloat(string k, float v) { var d = Get(); d.Floats[k] = v; Save(d); }
        public static float GetFloat(string k, float def = 0f) => Get().Floats.TryGetValue(k, out float v) ? v : def;

        public static void SetString(string k, string v) { var d = Get(); d.Strings[k] = v; Save(d); }
        public static string GetString(string k, string def = "") => Get().Strings.TryGetValue(k, out string v) ? v : def;

        public static void SetBool(string k, bool v) { var d = Get(); d.Bools[k] = v; Save(d); }
        public static bool GetBool(string k, bool def = false) => Get().Bools.TryGetValue(k, out bool v) ? v : def;

        private static PrefsSaveData Get() => SaveManager.Instance.Get<PrefsSaveData>();
        private static void Save(PrefsSaveData d) => SaveManager.Instance.Save(d);
    }
}