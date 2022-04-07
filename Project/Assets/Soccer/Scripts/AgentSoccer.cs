using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

// エージェント
public class AgentSoccer : Agent
{
    // チーム定数
    public const int TEAM_BLUE = 0;
    public const int TEAM_GREEN = 1;

    // 情報
    public int playerIndex; // プレイヤーINDEX
    [HideInInspector] public int team; // チーム

    // 参照
    public SoccerArea area; // エリア
    public ToioManager toioManager; // Toioマネージャ


    // 初期化
    public override void Initialize()
    {
        BehaviorParameters behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        this.team = (behaviorParameters.TeamId == TEAM_BLUE) ? TEAM_BLUE : TEAM_GREEN;
    }

    // 行動取得時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // キューブの移動
        int action = actionBuffers.DiscreteActions[0];
        int left = 0;
        int right = 0;
        if (action == 0) {left = 0; right = 0;}
        if (action == 1) {left = 115; right = 115;}
        if (action == 2) {left = -115; right = -115;}
        if (action == 3) {left = -8; right = 8;}
        if (action == 4) {left = 8; right = -9;}
        this.toioManager.MoveCube(this.playerIndex, left, right);
    }

    // ヒューリスティック
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        // 前後
        if (Input.GetKey(KeyCode.UpArrow)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.DownArrow)) discreteActionsOut[0] = 2;

        // 回転
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[0] = 3;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[0] = 4;
    }
}
