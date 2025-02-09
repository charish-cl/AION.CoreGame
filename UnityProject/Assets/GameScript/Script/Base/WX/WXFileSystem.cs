// using System;
// using System.IO;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using WeChatWASM;
//
// namespace AION.CoreFramework.Platform.FileSystem
// {
//     public class WXFileSystem : IFileSystem
//     {
//         private WXFileSystemManager _wxFileSystem;
//
//         private WXFileSystemManager wxFileSystem
//         {
//             get
//             {
//                 if (_wxFileSystem == null)
//                 {
//                     _wxFileSystem = new WXFileSystemManager();
//                 }
//
//                 return _wxFileSystem;
//             }
//         }
//
//         public bool CheckExistsSync(string path)
//         {
//             bool result = path != null 
//                           && wxFileSystem.AccessSync(GetWXCachePath(path)).Equals("access:ok");
//             Debug.Log($"校验路径：{path},是否存在{result}");
//             return result;
//         }
//
//         public UniTask<bool> CheckExists(string path)
//         {
//             path = GetWXCachePath(path);
//             UniTaskCompletionSource<bool> uniTaskCompletionSource = new UniTaskCompletionSource<bool>();
//             if (string.IsNullOrEmpty(path))
//             {
//                 return UniTask.FromResult(false);
//             }
//
//             wxFileSystem.Access(new AccessParam()
//             {
//                 path = path,
//                 success = (resp) =>
//                 {
//                     Debug.Log($"校验路径：{path},success");
//                     uniTaskCompletionSource.TrySetResult(true);
//                 },
//                 fail = (resp) =>
//                 {
//                     Debug.Log($"校验路径：{path},fail");
//                     uniTaskCompletionSource.TrySetResult(false);
//                 }
//             });
//             return uniTaskCompletionSource.Task;
//         }
//
//         public async UniTask<byte[]> ReadFileBytes(string path)
//         {
//             path = GetWXCachePath(path);
//             // 读取文件内容
//             UniTaskCompletionSource<byte[]> tcs = new UniTaskCompletionSource<byte[]>();
//             wxFileSystem.ReadFile(
//                 new ReadFileParam()
//                 {
//                     filePath = path,
//                     success = (res) =>
//                     {
//                         Debug.Log(path + "读取数据成功5555 **** ：" + res.arrayBufferLength);
//                         tcs.TrySetResult(res.binData);
//                     },
//                     fail = (res) =>
//                     {
//                         Debug.LogError(path + "读取数据 报错5555 ---- ：" + res.errMsg);
//                         tcs.TrySetException(new Exception(res.errMsg));
//                     }
//                 });
//             return await tcs.Task;
//         }
//
//         public UniTask<bool> CopyFile(string sourcePath, string destPath)
//         {
//             UniTaskCompletionSource<bool> uniTaskCompletionSource = new UniTaskCompletionSource<bool>();
//             Debug.Log("复制文件：***** " + sourcePath);
//             sourcePath = GetWXCachePath(sourcePath);
//             destPath = GetWXCachePath(destPath);
//             wxFileSystem.CopyFile(new CopyFileParam()
//             {
//                 srcPath = sourcePath,
//                 destPath = destPath,
//                 success = (resp) => { uniTaskCompletionSource.TrySetResult(true); },
//                 fail = (resp) => { uniTaskCompletionSource.TrySetResult(false); }
//             });
//             Debug.Log("复制文件：/ " + sourcePath);
//             return uniTaskCompletionSource.Task;
//         }
//
//         public void CreateDirectorySync(string path)
//         {
//             path = GetWXCachePath(path);
//             // *** 参数为：true 不好用，不能递归创建目录的上级目录后再创建该目录。*** 
//             // *** 只能一级一级创建目录 *** 
//             string res = wxFileSystem.MkdirSync(path, true);
//             Debug.Log($"同步创建目录{path}：创建结果：{res}");
//         }
//
//         // 同步创建目录
//         public async UniTask<bool> CreateDirectory(string path)
//         {
//             path = GetWXCachePath(path);
//             UniTaskCompletionSource<bool> task = new UniTaskCompletionSource<bool>();
//             wxFileSystem.Mkdir(new()
//             {
//                 dirPath = path,
//                 recursive = true,
//                 success = (res) =>
//                 {
//                     Debug.Log($"同步创建目录{path}：创建结果：{res.errCode}");
//                     task.TrySetResult(true);
//                 },
//                 fail = (res) =>
//                 {
//                     Debug.Log($"同步创建目录{path}：创建结果：{res.errMsg}");
//                     task.TrySetResult(false);
//                 }
//             });
//             return await task.Task;
//         }
//
//         // 删除目录
//         public void DeleteDirectory(string path)
//         {
//             path = GetWXCachePath(path);
//
//             string res = wxFileSystem.UnlinkSync(path);
//
//             Debug.Log($"DeleteDirectory 删除目录结果：{res}");
//         }
//
//         public async UniTask<bool> DeleteIfExist(string path)
//         {
//             string originPath = path;
//             path = GetWXCachePath(path);
//
//             if (await CheckExists(originPath))
//             {
//                 Debug.Log("will delete");
//                 UniTaskCompletionSource<bool> task = new UniTaskCompletionSource<bool>();
//                 wxFileSystem.Unlink(new UnlinkParam()
//                 {
//                     filePath = path,
//                     success = (res) =>
//                     {
//                         Debug.Log("删除成功 " + path);
//                         task.TrySetResult(true);
//                     },
//                     fail = (res) =>
//                     {
//                         Debug.LogError("删除失败 " + res.errMsg);
//                         task.TrySetResult(false);
//                     }
//                 });
//                 return await task.Task;
//             }
//
//             Debug.Log("文件不存在 " + path);
//             return true;
//         }
//
//         // 会先写到临时文件，再写到目标文件。
//         public UniTask<bool> WriteAllBytes(string path, byte[] content)
//         {
//             string ori = path;
//             string oriTmp = path + DateTime.Now.Ticks + ".tmp";
//             path = GetWXCachePath(path);
//             string pathTmp = GetWXCachePath(oriTmp);
//
//             Debug.Log($"新建文件 路径：{path},写入内容：{content.Length}");
//             UniTaskCompletionSource<bool> result = new UniTaskCompletionSource<bool>();
//             wxFileSystem.WriteFile(new WriteFileParam()
//             {
//                 filePath = pathTmp,
//                 data = content,
//                 success = async (resp) =>
//                 {
//                     Debug.Log("CreateFile success " + pathTmp + " " + resp.errMsg + resp.errCode);
//                     if (await CopyFile(oriTmp, ori)){
//                         result.TrySetResult(true);
//                     } else {
//                         result.TrySetResult(false);
//                     }
//                 },
//                 fail = (resp) =>
//                 {
//                     Debug.Log("CreateFile fail " + path + " " + resp.errMsg + resp.errCode);
//                     result.TrySetResult(false);
//                 }
//             });
//             return result.Task;
//         }
//
//         public string GetWXCachePath(string path)
//         {
//             return Path.Combine(WX.env.USER_DATA_PATH, path);
//         }
//     }
// }