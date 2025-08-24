namespace GameProto
{
    public partial class MessageDispatcher
    {
        static MessageDispatcher()
        {
           Register<LoginRequest>(10001);
           Register<LoginResponse>(10002);
           Register<ChatNotify>(10003);
           Register<ChatRequest>(10004);
           Register<ChatResponse>(10005);
        }
    }
}