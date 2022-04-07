using System;
using Mirror;

public class MirrorManager : NetworkManager
{
    // 参照
    public SoccerCanvas canvas;


    // クライアントの切断時に呼ばれる
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        this.canvas.ShowError("相手端末とのWi-Fi接続が切断されました。");
    }

    // クライアントのエラー時に呼ばれる
    public override void OnClientError(Exception exception)
    {
        base.OnClientError(exception);
        this.canvas.ShowError("相手端末とのWi-Fi接続に失敗しました。");
    }

    // サーバーの切断時に呼ばれる
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        this.canvas.ShowError("相手端末とのWi-Fi接続が切断されました。");
    }

    // サーバーのエラー時に呼ばれる
    public override void OnServerError(NetworkConnection conn, Exception exception)
    {
        base.OnServerError(conn, exception);
        this.canvas.ShowError("相手端末とのWi-Fi接続に失敗しました。");
    }
}