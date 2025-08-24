using System;
using GameProto;
using Google.Protobuf;
using UnityEngine;

using Sirenix.OdinInspector;


public class SocketManager : SerializedMonoBehaviour
{

    
    // public InputField EventNameTxt;
    // public InputField DataTxt;
    // public Text ReceivedText;  
    //
    // public GameObject objectToSpin;

    public ProtoRequest request;
    
    INetClient client = new WebSocketClient();

    private void Start()
    {
        MessageDispatcher.RegisterHandler((uint)ProtocolType.LoginResponse, OnLoginResponse);
    }

    private void OnDestroy()
    {
        MessageDispatcher.ClearHandlers();
    }

    private void OnLoginResponse(IMessage obj)
    {
        LoginResponse response = (LoginResponse)obj;
        Debug.Log("收到Token:"+response.Token);
    }

    [Button]
    public void Connet()
    {
        client.Connect("ws://127.0.0.1:8000");
    }
    [Button]
    public void Disconnect()
    {
        client.Disconnect();
    }
    [Button]
    public void SendMessage()
    {
        client.Emit(new LoginRequest()
        {
            Username = "admin",
            Password = "123456"
        });
    }
    
}