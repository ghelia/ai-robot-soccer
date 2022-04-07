using Unity.MLAgents;
using UnityEngine;

// エリア
public class SoccerArea : MonoBehaviour
{
    // 学習 (MA-POCA)
    private const int MAX_STEP = 5000;
    private int step;
    private float ballX;
    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup greenAgentGroup;

    // 参照
    public AgentSoccer[] cubeAgents;
    public GameObject ball;
    public ToioManager toioManager;


    // 初期化
    public void Start()
    {
        // エージェントグループの準備
        this.blueAgentGroup = new SimpleMultiAgentGroup();
        this.greenAgentGroup = new SimpleMultiAgentGroup();
        for (int i = 0; i < ToioManager.CUBE_NUM; i++)
        {
            if (this.cubeAgents[i].team == AgentSoccer.TEAM_BLUE)
            {
                this.blueAgentGroup.RegisterAgent(this.cubeAgents[i]);
            }
            else
            {
                this.greenAgentGroup.RegisterAgent(this.cubeAgents[i]);
            }
        }

        // シーンのリセット
        ResetScene();
    }

    // フレーム毎に呼ばれる
    public void FixedUpdate()
    {
        // ボール位置報酬
        float currentX = ball.transform.localPosition.x;
        float reward = (currentX - ballX) / 0.5f;
        if (Mathf.Abs(reward) > 0.0001f)
        {
            this.blueAgentGroup.AddGroupReward(reward);
            this.greenAgentGroup.AddGroupReward(-reward);
            this.ballX = currentX;
        }

        // シミュレーション
        if (this.toioManager.simulation)
        {
            // 最大ステップ数によるエピソード完了
            this.step += 1;
            if (this.step >= MAX_STEP)
            {
                this.blueAgentGroup.GroupEpisodeInterrupted();
                this.greenAgentGroup.GroupEpisodeInterrupted();
                ResetScene();
            }
        }
    }

    // ゴールタッチ時に呼ばれる
    public void GoalTouched(int scoredTeam)
    {
        // ゴール報酬とゴールによるエピソード完了
        if (scoredTeam == AgentSoccer.TEAM_BLUE)
        {
            this.blueAgentGroup.AddGroupReward(1f - (float)this.step / (float)MAX_STEP);
            this.greenAgentGroup.AddGroupReward(-1f);
        }
        else
        {
            this.greenAgentGroup.AddGroupReward(1f - (float)this.step / (float)MAX_STEP);
            this.blueAgentGroup.AddGroupReward(-1f);
        }
        this.blueAgentGroup.EndGroupEpisode();
        this.greenAgentGroup.EndGroupEpisode();

        // シーンのリセット
        ResetScene();
    }

    // シーンのリセット
    private void ResetScene()
    {
        // シミュレーション
        if (this.toioManager.simulation)
        {
            this.step = 0;
            this.ballX = 0;
        }

        // リセット
        this.toioManager.Reset();
    }
}
