using UnityEngine;

public class IntroCutscene : MonoBehaviour //타이틀에서 새 시작하면 영상 띄우고 튜토리얼 스테이지로 넘어감
{
    [Header("설정")]
    public string nextStageName = "Stage_1"; // 스킵 시 이동할 스테이지

    private bool isSkipping = false;

    private void Start()
    {
        // 시작하자마자 GameManager에게 "컷씬 시작" 알림
        // GameManager가 ESC 일시정지를 막아줌
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCutscene();
        }
    }

    private void Update()
    {
        // 이미 스킵 중이면 입력 무시
        if (isSkipping) return;

        // Enter 키나 스페이스바를 누르면 스킵
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    void SkipCutscene()
    {
        isSkipping = true;
        Debug.Log("컷씬 스킵, 다음 스테이지로 이동");

        if (GameManager.Instance != null)
        {
            // GameManager의 ChangeStage가 알아서 페이드 아웃 처리
            GameManager.Instance.ChangeStage(nextStageName);
        }
    }

    // 씬이 파괴될 때 컷씬 상태 해제
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCutscene();
        }
    }
}