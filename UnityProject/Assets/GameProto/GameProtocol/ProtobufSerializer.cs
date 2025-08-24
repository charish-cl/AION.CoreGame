namespace GameProto
{
    using Google.Protobuf;
    using System;

    public static class ProtobufSerializer
    {
        // 序列化消息 → 字节数组
        public static byte[] Serialize<T>(T message) where T : IMessage
        {
            return message.ToByteArray();
        }

        // 反序列化字节数组 → 消息对象
        public static T Deserialize<T>(byte[] data) where T : IMessage, new()
        {
            T message = new T();
            message.MergeFrom(data);
            return message;
        }
    }
}