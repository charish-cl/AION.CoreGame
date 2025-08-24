using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;

namespace GameProto
{
    public class WebSocketClient:INetClient
    {
        private ClientWebSocket webSocket;
        
        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    ProtoResponse protoResponse = new ProtoResponse();
                    protoResponse.MergeFrom(buffer,0,result.Count);
                    uint protocolId = protoResponse.ProtocolId;
                    IMessage message = MessageDispatcher.CreateMessageById(protocolId);
                    message.MergeFrom(protoResponse.Message);
                    MessageDispatcher.HandleMessage(protocolId, message);
                }
            }
        }
        
        public async void Connect(string address)
        {
            webSocket = new ClientWebSocket();
            try
            {
                // 异步连接
                await webSocket.ConnectAsync(new Uri(address), CancellationToken.None);
                Debug.Log("Connected");
                // 启动接收任务
                _ = ReceiveLoop(); 
            }
            catch (Exception e) { Debug.LogError(e); }
        }
        
        public void Emit<T>(T message) where T : IMessage
        {
            byte[] data = ProtobufSerializer.Serialize(message);
            
            ProtoRequest protoRequest = new ProtoRequest
            {
                ProtocolId = MessageDispatcher.GetId<T>(),
                Message = ByteString.CopyFrom(data)
            };
            byte[] protoRequestData = protoRequest.ToByteArray();
   
            //要用二进制帧发送
            webSocket.SendAsync(protoRequestData,
                WebSocketMessageType.Binary, 
                true,
                CancellationToken.None);
        }

        public async void Disconnect()
        {
            if (webSocket?.State == WebSocketState.Open)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }

        public void OnReceive(string data)
        {
            Debug.Log(data);
        }

   
        public void OnError()
        {
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
            Debug.Log("Disconnected");
        }
    }
}