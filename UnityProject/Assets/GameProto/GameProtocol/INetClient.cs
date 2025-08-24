using System;
using Google.Protobuf;

namespace GameProto
{
    public interface INetClient
    {
        
        public void Connect(string address);

        public void Emit<T>(T message) where T : IMessage;

        public void Disconnect();
		public void OnReceive(string data);
        public void OnError();
        public void OnConnect();
        public void OnDisconnect();
    }
}