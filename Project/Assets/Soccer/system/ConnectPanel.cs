using UnityEngine;
using UnityEngine.UI;

// 接続パネル
public class ConnectPanel : MonoBehaviour
{
    // 状態定数
    public const int S_SETUP = 0; // 設定画面
    public const int S_CONNECTED = 1; // 接続完了

    // UI
    public Text titleLabel;
    public Text textLabel;
    public Button standaloneButton;
    public Button hostButton;
    public Button clientButton;
    public InputField ipField;
    public Button disconnectButton;

    // 参照
    public ToioManager toioManager;
    public SoccerCanvas canvas;

    // 情報
    private int state;


    // 起動時に呼ばれる
    public void Start()
    {
        SetState(S_SETUP);
    }

    // 状態の指定
    public void SetState(int state, string error = null)
    {
        this.state = state;

        // UI
        if (this.state == S_SETUP)
        {
            if (error == null)
            {
                this.titleLabel.text = "接続モードの選択";
                this.textLabel.text = "接続モードを選択してください。";
            }
            else
            {
                this.titleLabel.text = "エラー";
                this.textLabel.text = error;
                this.toioManager.Disconnect();
            }
            this.standaloneButton.interactable = true;
            this.hostButton.interactable = true;
            this.clientButton.interactable = true;
            this.ipField.interactable = true;
            disconnectButton.interactable = false;
        }
        else if (this.state == S_CONNECTED)
        {
            this.titleLabel.text = "接続中";
            this.textLabel.text = "";
            this.standaloneButton.interactable = false;
            this.hostButton.interactable = false;
            this.clientButton.interactable = false;
            this.ipField.interactable = false;
            disconnectButton.interactable = true;
        }
    }

    // スタンドアロンボタン押下時に呼ばれる
    public void OnClickStandalone()
    {
        this.toioManager.Connect(ToioManager.M_STANDALONE);
        SetState(S_CONNECTED);
        this.canvas.SetMode(0);
    }

    // ホストボタン押下時に呼ばれる
    public void OnClickHost()
    {
        this.toioManager.Connect(ToioManager.M_HOST);
        SetState(S_CONNECTED);
        this.canvas.SetMode(0);
    }

    // クライアントボタン押下時に呼ばれる
    public void OnClickClient()
    {
        this.toioManager.Connect(ToioManager.M_CLIENT, this.ipField.text);
        SetState(S_CONNECTED);
        this.canvas.SetMode(0);
    }

    // 戻るボタン押下時に呼ばれる
    public void OnClickDisconnect()
    {
        this.toioManager.Disconnect();
        SetState(S_SETUP);
    }
}
