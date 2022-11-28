namespace NET7.Infrastructure.Fleck;

/// <summary>
/// Socket服务器
/// </summary>
public class FleckServer
{
    static readonly List<IWebSocketConnection> allSockets = new();
    static WebSocketServer server = null;
    static readonly object lockobj = new();
    public static void Start(string host = "ws://0.0.0.0:8888")
    {
        try
        {
            server = new WebSocketServer(host)
            {
                RestartAfterListenError = true
            };
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
