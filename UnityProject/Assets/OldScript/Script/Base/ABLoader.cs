// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Text;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Networking;
// using Debug = UnityEngine.Debug;
//
// namespace AION.CoreFramework
// {
//     public class ABLoader
//     {
//
//         public string remotePath;
//         
//         public string localPath;
//         
//         
//         public string GetLocalPath(string remotePath)
//         {
//             return null;
//         }
//         /// <summary>
//         /// 获取MD5码
//         /// </summary>
//         /// <param name="filePath">文件路径</param>
//         /// <returns>该文件的MD5码</returns>
//         private string GetFileMD5(string filePath)
//         {
//             //打开文件
//             using (FileStream file = new FileStream(filePath, FileMode.Open))
//             {
//                 //使用MD5格式
//                 MD5 md5 = new MD5CryptoServiceProvider();
//                 //获取文件MD码
//                 byte[] data = md5.ComputeHash(file);
//                 //关闭文件
//                 file.Close();
//                 //释放占用资源
//                 file.Dispose();
//                 StringBuilder sb = new StringBuilder();
//                 for (int i = 0; i < data.Length; i++)
//                 {
//                     //将数据转化成小写的16进制
//                     sb.Append(data[i].ToString("x2"));
//                 }
//                 return sb.ToString();
//             }
//         }
//
//         public async UniTask<Sprite> LoadSync(string path,string name,bool isNeedCache=false)
//         {
//             
//             Stopwatch  sw = new Stopwatch();
//             sw.Start();
//             path = GameFramework.Utility.Path.GetRemotePath(path);
//             UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
//              
//          
//             await request.SendWebRequest();
//             var assetBundle = DownloadHandlerAssetBundle.GetContent(request);
//             var sprite = assetBundle.LoadAsset<Sprite>(name);
//             sw.Stop();
//             Debug.Log($"下载ab成功{path} 耗时:{sw.ElapsedMilliseconds}ms");
//             assetBundle.Unload(false);
//            
//             
//             return sprite;
//         }
//
//         public async UniTask DownloadAB(string url)
//         {
//             var request = UnityWebRequest.Get(url);
//             await request.SendWebRequest();
//             if (request.isNetworkError || request.isHttpError)
//             {
//                 Debug.LogError("Error downloading: " + request.error);
//                 return;
//             }
//             
//             SaveAB(localPath, request.downloadHandler.data);
//         }
//         public void SaveAB(string path, byte[] data)
//         {
//             FileInfo fileInfo = new FileInfo(path);
//
//             using (FileStream fileStream = fileInfo.Create())
//             {
//                 fileStream.Write(data, 0, data.Length);
//             
//                 fileStream.Flush();
//             
//                 fileStream.Close();
//             }
//             Debug.Log("save ab success");
//         }
//     }
// }