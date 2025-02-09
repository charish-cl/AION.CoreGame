using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace GameDevKitEditor
{
    // PlayerPrefs管理工具窗口类
    [TreeWindow("PlayerPrefs管理工具")]
    public class PlayerPrefsManagerWindow : OdinEditorWindow
    {
        // 存储所有 PlayerPrefs 的数据
        [Title("PlayerPrefs数据")]
        [TableList]
        public List<PlayerPrefsData> PlayerPrefsList = new List<PlayerPrefsData>();

        
     
        protected override void OnEnable()
        {
	        base.OnEnable();
	    
	        // 初始化加载所有 PlayerPrefs 数据
	        InitializePlayerPrefs();
        }

        // 初始化，加载所有 PlayerPrefs 的键和值
        [Button("初始化加载所有 PlayerPrefs")]
        private void InitializePlayerPrefs()
        {
            // 清空当前列表
            PlayerPrefsList.Clear();

            
            
            // 遍历所有的 PlayerPrefs 键，加载键值对
            var allKeys = GetAll(PlayerSettings.companyName, PlayerSettings.productName);
            foreach (var playerPrefPair in allKeys)
            {
                string value = PlayerPrefs.GetString(playerPrefPair.Key, ""); // 你可以根据需要用 GetInt/GetFloat 来处理不同类型
                PlayerPrefsList.Add(new PlayerPrefsData(playerPrefPair.Key, value,Save,Delete));
            }

            // 刷新窗口
            Repaint();
        }
        private void Save(string Key, string Value)
        {
	        PlayerPrefs.SetString(Key, Value);
	        PlayerPrefs.Save();
	        Debug.Log($"已保存 PlayerPrefs: {Key} = {Value}");
        }

        private void Delete(string Key)
        {
	        PlayerPrefs.DeleteKey(Key);
	        Debug.Log($"已删除 PlayerPrefs: {Key}");
        }
		public static PlayerPrefPair[] GetAll(string companyName, string productName)
	{
		// if (Application.platform == RuntimePlatform.OSXEditor)
		// {
		// 	// From Unity docs: On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players.
		//
		// 	// Construct the plist filename from the project's settings
		// 	string plistFilename = string.Format("unity.{0}.{1}.plist", companyName, productName);
		// 	// Now construct the fully qualified path
		// 	string playerPrefsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"),
		// 		plistFilename);
		//
		// 	// Parse the player prefs file if it exists
		// 	if (File.Exists(playerPrefsPath))
		// 	{
		// 		// Parse the plist then cast it to a Dictionary
		// 		object plist = Plist.readPlist(playerPrefsPath);
		//
		// 		Dictionary<string, object> parsed = plist as Dictionary<string, object>;
		//
		// 		// Convert the dictionary data into an array of PlayerPrefPairs
		// 		PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[parsed.Count];
		// 		int i = 0;
		// 		foreach (KeyValuePair<string, object> pair in parsed)
		// 		{
		// 			if (pair.Value.GetType() == typeof(double))
		// 			{
		// 				// Some float values may come back as double, so convert them back to floats
		// 				tempPlayerPrefs[i] = new PlayerPrefPair() {Key = pair.Key, Value = (float) (double) pair.Value};
		// 			}
		// 			else
		// 			{
		// 				tempPlayerPrefs[i] = new PlayerPrefPair() {Key = pair.Key, Value = pair.Value};
		// 			}
		//
		// 			i++;
		// 		}
		//
		// 		// Return the results
		// 		return tempPlayerPrefs;
		// 	}
		// 	else
		// 	{
		// 		// No existing player prefs saved (which is valid), so just return an empty array
		// 		return new PlayerPrefPair[0];
		// 	}
		// }
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			// From Unity docs: On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
#if UNITY_5_5_OR_NEWER
			// From Unity 5.5 editor player prefs moved to a specific location
			Microsoft.Win32.RegistryKey registryKey =
				Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Unity\\UnityEditor\\" + companyName + "\\" + productName);
#else
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\" + companyName + "\\" + productName);
#endif

			// Parse the registry if the specified registryKey exists
			if (registryKey != null)
			{
				// Get an array of what keys (registry value names) are stored
				string[] valueNames = registryKey.GetValueNames();

				// Create the array of the right size to take the saved player prefs
				PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[valueNames.Length];

				// Parse and convert the registry saved player prefs into our array
				int i = 0;
				foreach (string valueName in valueNames)
				{
					string key = valueName;

					// Remove the _h193410979 style suffix used on player pref keys in Windows registry
					int index = key.LastIndexOf("_");
					key = key.Remove(index, key.Length - index);

					// Get the value from the registry
					object ambiguousValue = registryKey.GetValue(valueName);

					// Unfortunately floats will come back as an int (at least on 64 bit) because the float is stored as
					// 64 bit but marked as 32 bit - which confuses the GetValue() method greatly! 
					if (ambiguousValue.GetType() == typeof(int))
					{
						// If the player pref is not actually an int then it must be a float, this will evaluate to true
						// (impossible for it to be 0 and -1 at the same time)
						if (PlayerPrefs.GetInt(key, -1) == -1 && PlayerPrefs.GetInt(key, 0) == 0)
						{
							// Fetch the float value from PlayerPrefs in memory
							ambiguousValue = PlayerPrefs.GetFloat(key);
						}
					}
					else if (ambiguousValue.GetType() == typeof(byte[]))
					{
						// On Unity 5 a string may be stored as binary, so convert it back to a string
						ambiguousValue = System.Text.Encoding.Default.GetString((byte[]) ambiguousValue);
					}

					// Assign the key and value into the respective record in our output array
					tempPlayerPrefs[i] = new PlayerPrefPair() {Key = key, Value = ambiguousValue};
					i++;
				}

				// Return the results
				return tempPlayerPrefs;
			}
			else
			{
				// No existing player prefs saved (which is valid), so just return an empty array
				return new PlayerPrefPair[0];
			}
		}
		else
		{
			// throw new NotSupportedException("PlayerPrefsEditor doesn't support this Unity Editor platform");
			return new PlayerPrefPair[0];
		}
	}

        // 删除所有 PlayerPrefs 数据
        [Button("删除所有 PlayerPrefs", ButtonSizes.Large)]
        private void DeleteAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("已删除所有 PlayerPrefs 数据");
            Repaint();
        }

        // 增加一个新的 PlayerPrefs 数据
        [Button("新增 PlayerPrefs 数据",ButtonSizes.Large)]
        private void AddNewPlayerPrefsData()
        {
            PlayerPrefsList.Add(new PlayerPrefsData("", "",Save,Delete));
            Debug.Log("新增一个 PlayerPrefs 数据");
            Repaint();
        }

        // 删除一个指定的 PlayerPrefs 数据
        [Button("删除选中 PlayerPrefs 数据",ButtonSizes.Large)]
        private void DeleteSelectedPlayerPrefsData()
        {
            foreach (var item in PlayerPrefsList)
            {
                if (item.IsSelected)
                {
                    PlayerPrefs.DeleteKey(item.Key);
                    PlayerPrefsList.Remove(item);
                    Debug.Log($"删除 PlayerPrefs: {item.Key}");
                    break;
                }
            }

            Repaint();
        }
    }

   
}
