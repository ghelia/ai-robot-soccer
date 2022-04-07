# **6-3. ToioManager**

AI ロボットサッカー での操作は、ToioManager 経由で行います。サンプルでは、「SoccerArea.cs」「AgentSoccer.cs」 で利用しています。

<br>

## **6-3-1. toioManager.simulation**

<br>

シミュレーションモードかどうか (true/false)を取得します。

シミュレーションモードの時のみ、環境リセット時のプレイヤーとボールの位置の初期化を行ってください。

<br>

## **6-3-2. toioManager.Move(index, left, right)**

<br>

toio の操作を指定します。10FPS で呼び出してください。

<br>

- **index** : サッカープレイヤーのインデックス (0〜5)
- **left** : 左モーターの回転 (-115〜115)
- **right** : 右モーターの回転 (-115〜115)

<br>
