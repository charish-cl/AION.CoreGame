using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using GameDevKit.GameLogic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKit
{
    //https://www.jianshu.com/p/2f0cfdf116c8
    public static class ReflectionHelper
    {
        /// <summary>
        /// 调用MethodInfo的空方法
        /// </summary>
        /// <param name="methodInfo"></param>
        public static void InvokeMethod(this MethodInfo methodInfo)
        {
            Type methodClassType = methodInfo.DeclaringType;
            object instance = UnityCreatInstance(methodClassType);
            // object[] parameters = new object[] { "Hello, world!" };
            methodInfo.Invoke(instance, null);
        }

        private static object UnityCreatInstance(Type t)
        {
            if (t.IsSubclassOf(typeof(ScriptableObject)))
            {
                return ScriptableObject.CreateInstance(t);
            }

            return Activator.CreateInstance(t);
        }

        public static void InvokeMethod<T>(T t, string methodName, params object[] parameters)
        {
            t.GetType().GetMethod(methodName)?.Invoke(t, parameters);
        }

        public static T GetClass<T>()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// 获取类中的所有特性成员
        /// </summary>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">特性</typeparam>
        /// <returns></returns>
        public static List<Tuple<FieldInfo, T2>> GetAllFiledAttribute<T1, T2>() where T2 : class
        {
            var fieldInfos = typeof(T1).GetFields();

            var list = new List<Tuple<FieldInfo, T2>>();
            foreach (var fieldInfo in fieldInfos)
            {
                var attribute = fieldInfo.GetCustomAttributes(typeof(T2), true)[0] as T2;
                if (attribute is null) continue;
                list.Add(new Tuple<FieldInfo, T2>(fieldInfo, attribute));
            }

            return list;
        }

       

        public static List<MethodInfo> GetAllMethod<T>()
        {
            return typeof(T).GetMethods().ToList();
        }

        /// <summary>
        /// 反射枚举中所有成员
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetEnumPropToList(Type type)
        {
            List<string> list = new List<string>();
            var arr = Enum.GetValues(type);
            foreach (var o in arr)
            {
                list.Add(o.ToString());
            }

            return list;
        }


        /// <summary>
        /// 获取继承某attribute的所有类
        /// </summary>
        /// <param name="assemblename"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static List<Type> GetAllAttributeClass(string assemblename, Type attribute)
        {
            var types =
                    Assembly.Load(assemblename)
                        .GetTypes()
                        .Where(t => t.GetCustomAttribute(attribute) != null)
                ;
            return types.ToList();
        }


        /// <summary>
        /// 从所有属性里获取所有非默认值的属性
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<PropertyInfo> GetAllNonDefaultProp<T>(T t)
        {
            var allprop = t.GetType().GetProperties();
            var tmp = Activator.CreateInstance<T>();
            var list = new List<PropertyInfo>();
            foreach (var fieldInfo in allprop)
            {
                var t1 = fieldInfo.GetValue(tmp);
                var t2 = fieldInfo.GetValue(t);
                if (t1 == null && t2 == null)
                {
                    continue;
                }
                else
                {
                    if (t1 == null || t1.ToString() == t2.ToString())
                    {
                        continue;
                    }

                    list.Add(fieldInfo);
                }
            }

            return list;
        }

        /// <summary>
        /// 直接转成string比较值是否相等就好了,类型不一定对
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<FieldInfo> GetAllNonDefaultFieldInfo<T>(T t)
        {
            var allprop = t.GetType().GetFields();
            var tmp = Activator.CreateInstance<T>();
            var list = new List<FieldInfo>();
            foreach (var fieldInfo in allprop)
            {
                var t1 = fieldInfo.GetValue(tmp);
                var t2 = fieldInfo.GetValue(t);
                if (t1 == null && t2 == null)
                {
                    continue;
                }
                else
                {
                    if (t1 == null || t1.ToString() == t2.ToString())
                    {
                        continue;
                    }

                    list.Add(fieldInfo);
                }
            }

            return list;
        }

        /// <summary>
        /// 获取特性
        /// </summary>
        /// <param name="e"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo e) where T : class
        {
            return e.GetCustomAttribute(typeof(T)) as T;
        }

        public static List<Type> GetTypesDerivedFrom(this Type type)
        {
            var types = new List<Type>();
            types.AddRange(Assembly.Load("Assembly-CSharp").GetTypes()
                .Where(e => e.IsSubclassOf(type)));
            types.AddRange(type.Assembly.GetTypes()
                .Where(e => e.IsSubclassOf(type))
                .ToList());
            return types;
        }

        /// <summary>
        /// 获取嵌套类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetNestChildrenClass(this Type type)
        {
            return type.GetNestedTypes().ToList();
        }


        public static void Dump(this Type type)
        {
            Debug.Log($"Name:{type.Name} type:{type}" +
                      $"公有字段{type.GetFields().Length} " +
                      $"非公有字段{type.GetFields(BindingFlags.NonPublic).Length}" +
                      $"公有属性{type.GetProperties().Length}" +
                      $"非公有属性{type.GetProperties(BindingFlags.NonPublic).Length}");
        }
        /// <summary>
        /// 获取所有变量Properties,返回一个类json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string DisAlpayAllProperties(this object obj)
        {
            return obj.GetType().GetProperties().ToDictionary(e => e.Name, e => e.GetValue(obj)).Split(",");
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {
            const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetProperties(InstanceBindFlags);
        }

        public static FieldInfo[] GetAllFiels(this Type type)
        {
            const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetFields(InstanceBindFlags);
        }

        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="copyobj">被拷贝的对象</param>
        /// <typeparam name="T"></typeparam>
        public static void CopyValue<T>(this Object obj, Object copyobj)
        {
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo field in fields)
            {
                field.SetValue(obj, field.GetValue(copyobj));
            }
        }
        public static T DeepCopy<T>(T tIn)
        {
            T tOut = Activator.CreateInstance<T>();
            var tInType = tIn.GetType();
            foreach (var itemOut in tOut.GetType().GetFields())
            {
                var itemIn = tInType.GetField(itemOut.Name);
                ;
                if (itemIn != null)
                {
                    itemOut.SetValue(tOut, itemIn.GetValue(tIn));
                }
            }

            return tOut;
        }

        public static T DeepCopyByBin<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }

            return (T)retval;
        }
    }
}