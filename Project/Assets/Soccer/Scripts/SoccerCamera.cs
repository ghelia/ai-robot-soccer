using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using OpenCvSharp;

// サッカーカメラ
public class SoccerCamera : MonoBehaviour
{
    // サイズ
    private const int W = 640; // カメラの画像幅(dot)
    private const int H = 480; // カメラの画面高さ(dot)
    private const int FPS = 10; // カメラのFPS
    private const int C = 20; // フィールドの列数(cube)
    private const int R = 14; // フィールドの行数(cube)
    private const int GC = 2; // ゴールの列数(cube)
    private const int GR = 8; // ゴールの行数(cube)
    private const float US = 24; // キューブのユニットサイズ

    // 色
    private Scalar COL_BLACK = new Scalar(0, 0, 0, 255); // 黒
    private Scalar COL_RED = new Scalar(0, 0, 255, 255); // 赤
    private Scalar COL_GREEN = new Scalar(0, 255, 0, 255); // 緑
    private Scalar COL_YELLOW = new Scalar(0, 255, 255, 255); // 黄

    // マスク
    private Point MASK0_START = new Point(0, 0); // マスク上の始点
    private Point MASK0_END = new Point(W, (H - R * US) / 2 - US / 2); // マスク上の終点
    private Point MASK1_START = new Point(0, H - (H - R * US) / 2 + US / 2); // マスク下の始点
    private Point MASK1_END = new Point(W, H); // マスク下の終点

    // フィールド
    private Point RECT0_START = new Point(W / 2 - (C / 2) * US, H / 2 - (R / 2) * US); // フィールドの矩形の始点
    private Point RECT0_END = new Point(W / 2 + (C / 2) * US, H / 2 + (R / 2) * US); // フィールドの矩形の終点
    private Point RECT1_START = new Point(W / 2 - ((C / 2) + GC) * US, H / 2 - (GR / 2) * US); // ゴール左の矩形の始点
    private Point RECT1_END = new Point(W / 2 - (C / 2) * US, H / 2 + (GR / 2) * US); // ゴール左の矩形の終点
    private Point RECT2_START = new Point(W / 2 + (C / 2) * US, H / 2 - (GR / 2) * US); // ゴール右の矩形の始点
    private Point RECT2_END = new Point(W / 2 + ((C / 2) + GC) * US, H / 2 + (GR / 2) * US); // ゴール右の矩形の終点
    private Point RECT3_START = new Point(W / 2 - (42.05f / 3.18f) * US, H / 2 - (29.7f / 3.18f) * US); // マット矩形の始点
    private Point RECT3_END = new Point(W / 2 + (42.05f / 3.18f) * US, H / 2 + (29.7f / 3.18f) * US); // マット矩形の終点

    private Point LINE0_START = new Point(W / 2, H / 2 - (29.7f / 3.18f) * US); // ライン縦の始点
    private Point LINE0_END = new Point(W / 2, H / 2 + (29.7f / 3.18f) * US); // ライン縦の終点
    private Point LINE1_START = new Point(W / 2 - (42.05f / 3.18f) * US, H / 2); // ライン横の始点
    private Point LINE1_END = new Point(W / 2 + (42.05f / 3.18f) * US, H / 2); // ライン横の終点

    // ワーク変数
    private Mat hsvMat = new Mat();
    private Mat red0Mat = new Mat();
    private Mat red1Mat = new Mat();
    private Mat redMat = new Mat();
    private Point point = new Point();
    private Mat[] contours = new Mat[] { };
    private Mat hierarchy = new Mat();

    // ボールの検出
    private Scalar RED_RANGE0_MIN = new Scalar(0, 100, 50); // Hの最小値
    private Scalar RED_RANGE0_MAX = new Scalar(20, 255, 255); // Hの最大値
    private Scalar RED_RANGE1_MIN = new Scalar(245, 100, 50); // Hの最小値
    private Scalar RED_RANGE1_MAX = new Scalar(255, 255, 255); // Hの最大値
    private const int RED_AREA_MIN = 510; // 面積の最小値 (610)
    private const int RED_AREA_MAX = 1000; // 面積の最大値 (610)
    private const int RED_RADIUS_MIN = 10; // 半径の最小値 (15)
    private const int RED_RADIUS_MAX = 100; // 半径の最大値 (15)

    // ピクセル座標 → マット座標
    private Point PX_POS = new Point(W / 2 - (42.05f / 3.18f) * US, H / 2 - (29.7f / 3.18f) * US);
    private Point PX_SIZE = new Point((84.1f / 3.18f) * US, (59.4f / 3.18f) * US);
    private Point MAT_POS = new Point(34, 35);
    private Point MAT_SIZE = new Point(610, 431);

    // テクスチャ
    private WebCamTexture webCamTexture;
    private Texture2D dstTexture;
    public int cameraId = 0;

    // 情報
    private float timeElapsed; // 時間経過

    // 参照
    public SoccerCanvas canvas;
    public RawImage rawImage;
    public ToioManager toioManager;


    //====================
    // ライフサイクル
    //====================
    // 初期化
    public IEnumerator Start()
    {
        // カメラ利用の許可をユーザーに求める
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        // ユーザー許可の確認
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield break;
        }

        // カメラデバイス数の確認
        if (WebCamTexture.devices.Length <= cameraId)
        {
            yield break;
        }

        // Webカメラの開始
        WebCamDevice userCameraDevice = WebCamTexture.devices[cameraId];
        this.webCamTexture = new WebCamTexture(userCameraDevice.name, W, H, FPS);
        this.webCamTexture.Play();
    }

    // フレーム毎に呼ばれる
    public void Update()
    {
        // 50ms毎の処理
        this.timeElapsed += Time.deltaTime;
        if (this.timeElapsed < 0.05f) return;
        this.timeElapsed = 0.0f;

        // Webカメラ準備前は無処理
        if (this.webCamTexture == null ||
            this.webCamTexture.width <= 16 || this.webCamTexture.height <= 16) return;

        // Texture2Dの準備
        if (this.dstTexture == null)
        {
            this.dstTexture = new Texture2D(this.webCamTexture.width, this.webCamTexture.height, TextureFormat.RGBA32, false);
        }

        // WebCamTexture → Mat
        Mat srcMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);
        Mat dstMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);

        // サッカーフィールドの描画
        DrawSoccerField(dstMat);

        // HSV変換
        Cv2.CvtColor(srcMat, hsvMat, ColorConversionCodes.BGR2HSV_FULL);

        // マスク適用
        Cv2.Rectangle(hsvMat, MASK0_START, MASK0_END, COL_BLACK, -1);
        Cv2.Rectangle(hsvMat, MASK1_START, MASK1_END, COL_BLACK, -1);

        // ボールの描画
        DrawBall(hsvMat, dstMat);

        // モード
        if (canvas.mode == 1) OpenCvSharp.Unity.MatToTexture(dstMat, this.dstTexture);
        if (canvas.mode == 2) OpenCvSharp.Unity.MatToTexture(redMat, this.dstTexture);
        this.rawImage.texture = this.dstTexture;
    }

    // サッカーフィールドの描画
    private void DrawSoccerField(Mat mat)
    {
        // 矩形の描画
        Cv2.Rectangle(mat, RECT0_START, RECT0_END, COL_RED, 1);
        Cv2.Rectangle(mat, RECT1_START, RECT1_END, COL_RED, 1);
        Cv2.Rectangle(mat, RECT2_START, RECT2_END, COL_RED, 1);
        Cv2.Rectangle(mat, RECT3_START, RECT3_END, COL_RED, 1);

        // ラインの描画
        Cv2.Line(mat, LINE0_START, LINE0_END, COL_RED, 1); // 縦
        Cv2.Line(mat, LINE1_START, LINE1_END, COL_RED, 1); // 横
    }

    // ボールの描画
    private void DrawBall(Mat hsvMat, Mat dstMat)
    {
        // 赤検出
        Cv2.InRange(hsvMat, RED_RANGE0_MIN, RED_RANGE0_MAX, red0Mat);
        Cv2.InRange(hsvMat, RED_RANGE1_MIN, RED_RANGE1_MAX, red1Mat);
        Cv2.BitwiseOr(red0Mat, red1Mat, redMat);
        Cv2.MedianBlur(redMat, redMat, 5);

        // 輪郭抽出
        redMat.FindContours(out contours, hierarchy,
            RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        for (var i = 0; i < contours.Length; i++)
        {
            double size = Cv2.ContourArea(contours[i]);
            if (RED_AREA_MIN <= size && size <= RED_AREA_MAX) // 面積
            {
                Cv2.MinEnclosingCircle(contours[i], out Point2f center, out float radius);
                if (RED_RADIUS_MIN <= radius && radius <= RED_RADIUS_MAX) // 半径
                {
                    // ボールの描画
                    point.X = (int)center.X;
                    point.Y = (int)center.Y;
                    Cv2.Circle(dstMat, point, (int)radius, COL_YELLOW, 2);

                    // ピクセル座標 → マット座標
                    float matX = point.X - PX_POS.X;
                    float matY = point.Y - PX_POS.Y;
                    matX = matX * MAT_SIZE.X / PX_SIZE.X + MAT_POS.X;
                    matY = matY * MAT_SIZE.Y / PX_SIZE.Y + MAT_POS.Y;

                    // ボールの移動
                    this.toioManager.MoveBall(matX, matY);
                    return;
                }
            }
        }
    }
}