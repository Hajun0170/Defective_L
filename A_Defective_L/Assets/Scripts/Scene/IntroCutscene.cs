using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
    [Header("설정")]
    public string nextStageName = "Stage_1"; // 스킵 시 넘어갈 스테이지 이름

    private bool isSkipping = false;

    private void Start()
    {
        // ★ 1. 시작하자마자 GameManager에게 "컷씬 시작" 알림
        // (이게 되어야 GameManager가 ESC 일시정지를 막아줍니다)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCutscene();
        }
    }

    private void Update()
    {
        // 이미 스킵 중이면 입력 무시
        if (isSkipping) return;

        // ★ 2. Enter 키(또는 스페이스바)를 누르면 스킵
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    void SkipCutscene()
    {
        isSkipping = true;
        Debug.Log("⏩ 컷씬 스킵! 다음 스테이지로 이동합니다.");

        if (GameManager.Instance != null)
        {
            // ★ GameManager의 ChangeStage를 부르면 
            // 알아서 [페이드 아웃 -> 씬 로드 -> 페이드 인] 처리를 해줍니다.
            GameManager.Instance.ChangeStage(nextStageName);
        }
    }

    // 씬이 파괴될 때(넘어갈 때) 컷씬 상태 해제
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCutscene();
        }
    }
}