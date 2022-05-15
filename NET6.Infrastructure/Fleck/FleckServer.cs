namespace NET6.Infrastructure.Fleck;

/// <summary>
/// Socket服务器
/// </summary>
public class FleckServer
{
    private static readonly List<IWebSocketConnection> allSockets = new();
    private static readonly WebSocketServer server = new("ws://0.0.0.0:8888");
    private static readonly object lockobj = new();
    public static void Start()
    {
        try
        {
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    lock (lockobj)
                    {
                        allSockets.Add(socket);
                    }
                };
                socket.OnClose = () =>
                {
                    lock (lockobj)
                    {
                        allSockets.Remove(socket);
                    }
                };
            });
            Log.Error("socket服务器已启动：" + server.Location);
        }
        catch (Exception ex)
        {
            Log.Error("socket启动异常：" + ex.Message);
        }
    }
    public static string SendMessage(List<string> ListUerid, string Message)
    {
        try
        {
            var userSockets = allSockets.Where(s => ListUerid.Contains(s.ConnectionInfo.Path.Split('/').Last())).ToList();
            userSockets.ForEach(async s =>
            {
                await s.Send(Message);
            });
            return "ok";
        }
        catch (Exception ex)
        {
            Log.Error("消息发送异常：" + ex.Message);
            return ex.Message;
        }
    }
    public static string SendMessageAll(string Message)
    {
        try
        {
            allSockets.ForEach(async s =>
            {
                await s.Send(Message);
            });
            return "ok";
        }
        catch (Exception ex)
        {
            Log.Error("消息发送异常：" + ex.Message);
            return ex.Message;
        }
    }
}
