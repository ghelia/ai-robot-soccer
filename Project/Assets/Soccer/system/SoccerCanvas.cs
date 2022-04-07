using UnityEngine;
using UnityEngine.UI;

// サッカーキャンバス
public class SoccerCanvas : MonoBehaviour
{
    // 情報
    public int mode = 0; // モード

    // 参照
    public GameObject camera;
    public RawImage rawImage;
    public ConnectPanel connectPanel;


    // 初期化
    void Start()
    {
        SetMode(0);
    }

    // モードの指定
    public void SetMode(int mode)
    {
        this.mode = mode;
        this.rawImage.gameObject.SetActive(this.mode == 1 || this.mode == 2);
        this.connectPanel.gameObject.SetActive(this.mode == 3);
    }

    // エラー表示
    public void ShowError(string error)
    {
        print("error>>>" + error);
        this.connectPanel.SetState(ConnectPanel.S_SETUP, error);
        SetMode(3);
    }

    // モードボタン押下時に呼ばれる
    public void OnClickMode()
    {
        SetMode((this.mode + 1) % 4);
    }
}
