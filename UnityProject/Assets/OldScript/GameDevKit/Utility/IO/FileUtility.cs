using System;
using System.IO;
using UnityEngine;

namespace GameDevKit.Utility
{
    public class FileUtility
    {
        //TryCreateDirectory if not exist,return true if success,out  DirectoryInfo
        public static bool TryCreateDirectory(string path, out DirectoryInfo directoryInfo)
        {
            directoryInfo = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (Directory.Exists(path))
            {
                directoryInfo = new DirectoryInfo(path);
                return true;
            }

            try
            {
                directoryInfo = Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        // TryCreateFile if not exist, return true if success, out FileInfo
        public static bool TryCreateFile(string filePath, out FileInfo fileInfo)
        {
            fileInfo = null;
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (File.Exists(filePath))
            {
                fileInfo = new FileInfo(filePath);
                return true;
            }

            try
            {
                fileInfo = new FileInfo(filePath);
                using (var stream = fileInfo.Create())
                {
                    // Do nothing.
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        
        
        
        
        
        
    }
}