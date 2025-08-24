using Google.Protobuf;

namespace GameProto
{
    using System;
    using System.Collections.Generic;

    public static partial class MessageDispatcher
    {
        private static readonly Dictionary<Type, uint> _typeToId = new();
        private static readonly Dictionary<uint, Type> _idToType = new();

        
        public static readonly Dictionary<uint,List<Action<IMessage>>> _msgHandlers = new();
        

        // 注册消息类型
        public static void Register<T>(uint msgId) where T : IMessage
        {
            Type type = typeof(T);
            _typeToId[type] = msgId;
            _idToType[msgId] = type;
        }

        // 根据类型获取ID
        public static uint GetId<T>() => _typeToId[typeof(T)];

        // 根据ID创建消息实例
        public static IMessage CreateMessageById(uint msgId)
        {
            return (IMessage)Activator.CreateInstance(_idToType[msgId]);
        }
        
        
        public static void RegisterHandler(uint msgId, Action<IMessage> handler)
        {
            if (!_msgHandlers.ContainsKey(msgId))
            {
                _msgHandlers[msgId] = new List<Action<IMessage>>();
            }
            _msgHandlers[msgId].Add(handler);
        }
        
        public static void UnregisterHandler(uint msgId, Action<IMessage> handler)
        {
            if (!_msgHandlers.ContainsKey(msgId))
            {
                throw new Exception("Message not registered");
                return;
            }
            _msgHandlers[msgId].Remove(handler);
        }
        
        public static void HandleMessage(uint msgId, IMessage msg)
        {
            if (_msgHandlers.ContainsKey(msgId))
            {
                foreach (var handler in _msgHandlers[msgId])
                {
                    handler(msg);
                }
            }
        }   
        
        public static void ClearHandlers()
        {
            _msgHandlers.Clear();
        }
    }
}