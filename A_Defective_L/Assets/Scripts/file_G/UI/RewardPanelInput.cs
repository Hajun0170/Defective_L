using UnityEngine;

public class RewardPanelInput : MonoBehaviour
{
    // 패널이 활성화될 때마다 호출됨
    private void OnEnable()
    {
        // 게임 시간을 멈추고 싶다면 (선택사항)
        Time.timeScale = 0f; 
    }

    private void Update()
    {
        // 아무 키나 눌렀거나 마우스를 클릭했을 때
        if (Input.anyKeyDown)
        {
            ClosePanel();
        }
    }

    void ClosePanel()
    {
        // 게임 시간 재개
        Time.timeScale = 1f;

        // 패널 끄기
        gameObject.SetActive(false);

        // UIManager나 GameManager에 알릴 필요가 있다면 여기서 호출
        // 예: UIManager.Instance.CloseRewardPanel();
    }
}