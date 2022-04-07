using UnityEngine;

// サッカーボールコントローラ
public class SoccerBallController : MonoBehaviour
{
    // 参照
    public SoccerArea area; // エリア


    // 衝突時に呼ばれる
    public void OnCollisionEnter(Collision col)
    {
        // 緑ゴールと衝突
        if (col.gameObject.CompareTag("greenGoal"))
        {
            this.area.GoalTouched(AgentSoccer.TEAM_BLUE);
        }

        // 青ゴールと衝突
        if (col.gameObject.CompareTag("blueGoal"))
        {
            this.area.GoalTouched(AgentSoccer.TEAM_GREEN);
        }
    }
}
