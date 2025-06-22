namespace GameDevKitEditor
{
    using System;

    // 定义TreeWindowAttribute属性
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TreeWindowAttribute : Attribute
    {
        // 只读的path字段
        public readonly string Path;

        // 构造函数，需要一个path参数
        public TreeWindowAttribute(string path)
        {
            this.Path = path;
        }
    }
}