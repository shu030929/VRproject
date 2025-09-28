using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public TextMeshPro signText;
    public GameObject signRoot;
    public GameObject ball;

    enum Step { PushBall, NextMission }
    Step step = Step.PushBall;

    void Start()
    {
        if (signText != null)
            signText.text = "공을 목표 칸에 밀어 넣으세요!";
    }

    // ⬇️ TargetZone.cs에서 호출할 함수
    public void OnBallArrived(GameObject who)
    {
        if (step != Step.PushBall) return;
        if (who != ball) return;

        // 미션 성공 처리
        if (signText != null)
            signText.text = "잘했어요! 다음 미션: 레버를 당기세요!";
        
        step = Step.NextMission;
    }
}
