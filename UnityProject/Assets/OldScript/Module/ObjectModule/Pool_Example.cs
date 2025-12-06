// using System;
// using AION.CoreFramework;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace AION.CoreFramework
// {
//     /// <summary>
//     /// Pool 使用示例 - 展示如何使用新的泛型 Pool 接口
//     /// </summary>
//     public static class PoolExample
//     {
//         /// <summary>
//         /// 示例1：使用 Pool.Get<T>() 和 Pool.Release<T>() 简化 HPBarLogic
//         /// </summary>
//         public static void Example_HPBarLogic()
//         {
//             // 1. 注册工厂函数（可选，在初始化时注册一次）
//             Pool.RegisterFactory<HPBarLogic>(() =>
//             {
//                 GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
//                 Transform hpBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
//                 GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, hpBarParent);
//                 hpBarGameObject.name = "HPBar";
//                 return new HPBarLogic(hpBarGameObject, null);
//             });
//             
//             // 2. 使用 Pool.Get<T>() 获取对象（自动从池中获取或创建）
//             Transform heroTransform = null; // 示例
//             HPBarLogic hpBar = Pool.Get<HPBarLogic>("HPBar", () =>
//             {
//                 // 如果未注册工厂，可以在这里提供工厂函数
//                 GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
//                 Transform hpBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
//                 GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, hpBarParent);
//                 hpBarGameObject.name = "HPBar";
//                 return new HPBarLogic(hpBarGameObject, heroTransform);
//             });
//             
//             hpBar.SetParent(heroTransform);
//             hpBar.Init(100f);
//             
//             // 3. 使用 Pool.Release<T>() 释放对象
//             Pool.Release(hpBar);
//         }
//         
//         /// <summary>
//         /// 示例2：自定义对象类型
//         /// </summary>
//         public class MyCustomLogic : ObjectBase
//         {
//             public string Data;
//             
//             public MyCustomLogic(string data)
//             {
//                 Data = data;
//             }
//             
//             public override void OnSpawn()
//             {
//                 base.OnSpawn();
//                 // 初始化逻辑
//             }
//             
//             public override void OnUnspawn()
//             {
//                 base.OnUnspawn();
//                 // 清理逻辑
//             }
//         }
//         
//         public static void Example_CustomLogic()
//         {
//             // 注册工厂
//             Pool.RegisterFactory<MyCustomLogic>(() => new MyCustomLogic("default"));
//             
//             // 获取对象
//             MyCustomLogic obj = Pool.Get<MyCustomLogic>();
//             
//             // 使用对象
//             obj.Data = "custom data";
//             
//             // 释放对象
//             Pool.Release(obj);
//         }
//     }
// }
//
