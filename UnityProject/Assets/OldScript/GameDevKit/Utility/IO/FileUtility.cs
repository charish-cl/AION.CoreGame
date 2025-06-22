using System;
using System.IO;
using UnityEngine;

namespace GameDevKit.Utility
{
    public class FileUtility
    {
        public void CopyFolder(string sourceFolder, string destinationFolder,string pattern = "*",string notPattern = "")
        {
            if (!Directory.Exists(sourceFolder))
            {
                Debug.Log("源文件夹不存在！");
                return;
            }
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder, pattern, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (notPattern == "" || !file.Contains(notPattern))
                {
                    string destFile = Path.Combine(destinationFolder, file.Substring(sourceFolder.Length + 1));
                    if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                    }
                    File.Copy(file, destFile, true);
                }
            }
            
        }
        
        
        
        
        
        
    }
}