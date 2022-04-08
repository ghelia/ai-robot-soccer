using System;
using UnityEngine;
using UnityEngine.UI;
using toio;
using Unity.MLAgents;

// Toioマネージャ
public class ToioManager : MonoBehaviour
{
    public const int CUBE_NUM = 6;

    // 状態定数
    public const int S_INIT = 0; // 初期状態
    public const int S_CONNECTING = 1; // 接続中
    public const int S_READY = 2; // 準備 (実環境のみ)
    public const int S_PLAY = 3; // プレイ

    // モード定数
    public const int M_STANDALONE = 0;
    public const int M_HOST = 1;
    public const int M_CLIENT = 2;

    // マット座標 → ユニット座標
    public const float CUBE_ORIGIN_X = 340f;
    public const float CUBE_ORIGIN_Z = 251f;
    public const float CUBE_SCALE_X = 0.00136f;
    public const float CUBE_SCALE_Z = -0.00136f;

    // 初期位置定数 (マット座標)
    private int DY = 0;
    private int[,] INIT_POS = new int[,]{
        {265, 222, 0},
        {265, 278, 0},
        {119, 260, 0},
        {413, 222, 180},
        {413, 278, 180},
        {559, 260, 180},
    };

    // 情報
    public ConnectType connectType; // 接続種別
    public bool simulation; // シミュレーションモードかどうか
    [HideInInspector] public int state = S_INIT; // 状態
    [HideInInspector] public int mode = M_STANDALONE; // モード
    private float readyTime; // 準備開始経過
    private float timeElapsed; // 時間経過

    // 参照
    private CubeManager cubeManager; // キューブマネージャ
    private Cube[] cubes = new Cube[CUBE_NUM]; // キューブ
    public GameObject[] cubeAgents; // キューブエージェントのGameObject
    public GameObject[] cubeObjs = new GameObject[CUBE_NUM]; // キューブのGameObject (学習のみ)
    public GameObject ball; // ボール
    public MirrorManager mirrorManager; // Mirrorマネージャ
    public SoccerCanvas canvas; // キャンバス
    public Text toioLabel; // toioラベル


    // 初期化
    public void Start()
    {
        // システム
        this.cubeManager = new CubeManager(connectType);

        // 接続
        if (this.simulation)
        {
            Connect(M_STANDALONE);
        }
        else
        {
            this.canvas.gameObject.SetActive(true);
        }
    }

    // フレーム毎に呼ばれる
    public void Update()
    {
        // フルスクリーン解除
        if (Input.GetKey(KeyCode.Escape) && Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }

        // 50ms毎の処理
        this.timeElapsed += Time.deltaTime;
        if (this.timeElapsed < 0.05f) return;
        this.timeElapsed = 0.0f;

        // 接続中
        if (this.state == S_CONNECTING)
        {
            if (IsConnected())
            {
                this.toioLabel.text = "";
                this.state = S_PLAY;

                // 実環境のリセット
                if (!this.simulation)
                {
                    Ready();
                }
            }
        }

        // 準備
        else if (this.state == S_READY)
        {
            // 時間経過
            this.readyTime += Time.deltaTime;

            // 移動
            if (this.readyTime < 3f)
            {
                for (int i = 0; i < CUBE_NUM; i++)
                {
                    if (this.cubes[i] != null)
                    {
                        float d = Distance(this.cubes[i].x, this.cubes[i].y - DY, this.INIT_POS[i, 0], this.INIT_POS[i, 1]);
                        if (d > 10) this.cubes[i].TargetMove(this.INIT_POS[i, 0], this.INIT_POS[i, 1] + DY, this.INIT_POS[i, 2], 10);
                    }
                }
            }
            // 回転
            else
            {
                for (int i = 0; i < CUBE_NUM; i++)
                {
                    if (this.cubes[i] != null)
                    {
                        float d = Distance(this.cubes[i].x, this.cubes[i].y - DY, this.INIT_POS[i, 0], this.INIT_POS[i, 1]);
                        if (d > 10) this.cubes[i].TargetMove(this.cubes[i].x, this.cubes[i].y, Rand(360), 10);
                    }
                }
                if (this.readyTime > 3.5) this.readyTime = 0.0f;
            }

            // ゲーム開始
            float distance = Vector3.Distance(ball.transform.localPosition, Vector3.zero);
            if (distance < 0.1f)
            {
                this.toioLabel.text = "";
                this.state = S_PLAY;
            }
        }

        // キューブとキューブエージェントの位置の同期
        if (this.state == S_READY || this.state == S_PLAY)
        {
            MirrorPlayer[] players = MirrorPlayer.GetPlayer();
            for (int i = 0; i < CUBE_NUM; i++)
            {
                float x = 0f;
                float y = 0f;
                float angle = 0f;
                if (this.cubes[i] != null)
                {
                    // マット座標 → ユニット座標
                    x = (float)(this.cubes[i].x - CUBE_ORIGIN_X) * CUBE_SCALE_X;
                    y = (float)(this.cubes[i].y - DY - CUBE_ORIGIN_Z) * CUBE_SCALE_Z;
                    angle = (float)this.cubes[i].angle + 90f;

                    // 送信
                    if (this.mode != M_STANDALONE && players != null)
                    {
                        players[0].CmdSetXYAngle(i % (CUBE_NUM/2), x, y, angle);
                    }
                }
                else
                {
                    // 受信
                    if (players == null) continue;
                    x = players[1].GetX(i % (CUBE_NUM/2));
                    y = players[1].GetY(i % (CUBE_NUM/2));
                    angle = players[1].GetAngle(i % (CUBE_NUM/2));
                    if (x == 0f && y == 0f) continue;
                }
                this.cubeAgents[i].transform.position = new Vector3(x, 0.0159f, y);
                this.cubeAgents[i].transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }
    }

    // 接続
    public async void Connect(int mode, String ip = null)
    {
        this.mode = mode;

        // キューブの接続
        int cubeNum = (this.mode == M_STANDALONE) ? CUBE_NUM : (CUBE_NUM/2);
        for (int i = 0; i < CUBE_NUM; i++)
        {
            this.cubeObjs[i].SetActive(i < cubeNum);
        }
        await this.cubeManager.MultiConnect(cubeNum);
        this.state = S_CONNECTING;

        // Mirrorの接続
        if (mode == M_HOST)
        {
            this.mirrorManager.StartHost();
        }
        else if (mode == M_CLIENT)
        {
            this.mirrorManager.networkAddress = ip;
            this.mirrorManager.StartClient();
        }
    }

    // 切断
    public async void Disconnect()
    {
        this.state = S_INIT;

        // キューブの切断
        this.cubeManager.DisconnectAll();

        // Mirrorの切断
        if (mode == M_HOST)
        {
            this.mirrorManager.StopHost();
        }
        else if (mode == M_CLIENT)
        {
            this.mirrorManager.StopClient();
        }
    }

    // 接続完了かどうか
    private bool IsConnected()
    {
        int blueIdx = 0;
        int greenIdx = 0;
        for (int i = 0; i < CUBE_NUM; i++) this.cubes[i] = null;
        for (int i = 0; i < this.cubeManager.cubes.Count; i++)
        {
            Cube cube = this.cubeManager.cubes[i];
            if (cube.x == 0 || cube.y == 0) return false;

            // 青キューブ
            if ((this.mode == M_STANDALONE && cube.x < CUBE_ORIGIN_X) || this.mode == M_HOST)
            {
                if (blueIdx < (CUBE_NUM/2)) this.cubes[blueIdx] = cube;
                blueIdx += 1;
            }
            // 緑キューブ
            else
            {
                if (greenIdx < (CUBE_NUM/2)) this.cubes[greenIdx + (CUBE_NUM/2)] = cube;
                greenIdx += 1;
            }
        }
        if (this.mode == M_STANDALONE)
        {
            if (blueIdx == (CUBE_NUM/2) && greenIdx == (CUBE_NUM/2)) {
                if (this.connectType == ConnectType.Real) {
                    for (int i = 0; i < CUBE_NUM; i++)
                    {
                        this.cubeObjs[i].SetActive(false);
                    }
                }
                return true;
            }
            this.canvas.ShowError("接続失敗 青:" + blueIdx + "/"+(CUBE_NUM/2)+" 緑:" + greenIdx + "/"+(CUBE_NUM/2));
        }
        else if (this.mode == M_HOST)
        {
            if (blueIdx == (CUBE_NUM/2)) return true;
            this.canvas.ShowError("接続失敗 青:" + blueIdx + "/"+(CUBE_NUM/2));
        }
        else if (this.mode == M_CLIENT)
        {
            if (greenIdx == (CUBE_NUM/2)) return true;
            this.canvas.ShowError("接続失敗 緑:" + greenIdx + "/"+(CUBE_NUM/2));
        }
        return false;
    }

    // リセット
    public void Reset()
    {
        if (this.simulation)
        {
            // キューブのリセット
            for (int i = 0; i < CUBE_NUM; i++)
            {
                this.ResetCube(i);
            }

            // ボールのリセット
            this.ball.transform.position = new Vector3(0f, 0.025f, 0f);
            this.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        else
        {
            // Ready
            this.Ready();
        }
    }

    // 実環境のリセット（ゴール時）
    private void Ready()
    {
        if (this.state != S_PLAY) return;

        // キューブのリセット
        this.readyTime = 0f;
        this.toioLabel.text = "Ready";
        this.state = S_READY;
    }

    // 学習環境のリセット（エピソード完了時）
    private void ResetCube(int index)
    {
        if (this.state != S_PLAY || this.cubes[index] == null) return;

        // マット座標 → ユニット座標
        float x = (float)(INIT_POS[index, 0] - CUBE_ORIGIN_X) * CUBE_SCALE_X;
        float y = (float)(INIT_POS[index, 1] - CUBE_ORIGIN_Z) * CUBE_SCALE_Z;
        float angle = (float)(INIT_POS[index, 2] + 90);

        // キューブのリセット
        this.cubeObjs[index].transform.localRotation = Quaternion.Euler(0, angle, 0);
        this.cubeObjs[index].transform.localPosition = new Vector3(x, 0.001f, y);
    }

    // キューブの移動
    public void MoveCube(int index, int left, int right)
    {
        if (this.state != S_PLAY || this.cubes[index] == null) return;
        if (cubeManager.IsControllable(this.cubes[index]))
        {
            this.cubes[index].Move(left, right, 0);
        }
    }

    // ボールの移動
    public void MoveBall(float matX, float matY)
    {
        // マット座標 → ユニット座標
        float x = (float)(matX - CUBE_ORIGIN_X) * CUBE_SCALE_X;
        float y = (float)(matY - CUBE_ORIGIN_Z) * CUBE_SCALE_Z;

        // 範囲制限
        if (x > 0.5f) x = 0.5f;
        if (x < -0.5f) x = -0.5f;
        if (y > 0.2f) y = 0.2f;
        if (y < -0.2f) y = -0.2f;

        // ボール位置の更新
        this.ball.transform.localPosition = new Vector3(x, 0.025f, y);
    }

    // 距離の計算
    private float Distance(float x0, float y0, float x1, float y1)
    {
        return Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
    }

    // 乱数の取得
    private int Rand(int max)
    {
        return UnityEngine.Random.Range(0, max);
    }
}
