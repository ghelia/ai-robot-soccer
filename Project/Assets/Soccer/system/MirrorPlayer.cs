using UnityEngine;
using Mirror;

// Mirrorプレイヤー
public class MirrorPlayer : NetworkBehaviour
{

    readonly SyncList<Vector3> xyangle = new SyncList<Vector3>();


    // サーバーでのオブジェクト生成時に呼ばれる
    public override void OnStartServer()
    {
        // 初期位置
        Vector3 pos = gameObject.transform.position;
        pos.x = !isLocalPlayer ? 0.52f : -0.52f;
        gameObject.transform.position = pos;

        // 初期値
        for (int i = 0; i < (ToioManager.CUBE_NUM/2); i++)
        {
            this.xyangle.Add(new Vector3());
        }
    }

    // Xの取得
    public float GetX(int index)
    {
        return xyangle[index].x;
    }

    // Yの取得
    public float GetY(int index)
    {
        return xyangle[index].y;
    }

    // Angleの取得
    public float GetAngle(int index)
    {
        return xyangle[index].z;
    }

    // 移動
    [Command]
    public void CmdSetXYAngle(int index, float x, float y, float angle)
    {
        this.xyangle[index] = new Vector3(x, y, angle);
    }

    // プレイヤーの取得 (0:Local, 1:Other)
    public static MirrorPlayer[] GetPlayer()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        if (objects == null || objects.Length != 2) return null;
        MirrorPlayer[] players = new MirrorPlayer[2];
        for (int i = 0; i < 2; i++)
        {
            MirrorPlayer player = objects[i].GetComponent<MirrorPlayer>() as MirrorPlayer;
            players[player.isLocalPlayer ? 0 : 1] = player;
        }
        return players;
    }
}