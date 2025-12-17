using UnityEngine;
using UnityEngine.UI; // UI 기능을 위해 필수
using TMPro;          // TextMeshPro (글자) 사용 시 필요

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글톤 인스턴스

    [Header("HUD References")]
    [SerializeField] private Image cylinderGaugeFill; // 초록색 게이지 바
    [SerializeField] private GameObject[] ticketIcons; // 교환권 아이콘 3개 (켜고 끄기)
    [SerializeField] private TextMeshProUGUI hpText;   // (선택) 체력 표시용

    [Header("Swap UI References")]
    [SerializeField] private GameObject swapPanel;     // 교체 메뉴 전체 패널
    [SerializeField] private TextMeshProUGUI currentMeleeName;  // 현재 선택된 근접 무기 이름
    [SerializeField] private TextMeshProUGUI currentRangedName; // 현재 선택된 원거리 무기 이름

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. 실린더 게이지 업데이트 (0 ~ 10)
    public void UpdateGauge(int current, int max)
    {
        if (cylinderGaugeFill != null)
        {
            // fillAmount는 0.0 ~ 1.0 사이의 값이어야 함
            cylinderGaugeFill.fillAmount = (float)current / max;
        }
    }

    // 2. 교환권 아이콘 업데이트 (활성화/비활성화)
    public void UpdateTickets(int count)
    {
        for (int i = 0; i < ticketIcons.Length; i++)
        {
            // 현재 개수만큼만 아이콘을 켬
            if (i < count) ticketIcons[i].SetActive(true); 
            else ticketIcons[i].SetActive(false);
        }
    }

    // 3. 교체 UI 열기/닫기
    public void ToggleSwapUI(bool isOpen)
    {
        if (swapPanel != null) swapPanel.SetActive(isOpen);
    }

    // 4. 선택된 무기 이름 갱신
    public void UpdateWeaponNames(string meleeName, string rangedName)
    {
        if (currentMeleeName != null) currentMeleeName.text = $"근거리: {meleeName}";
        if (currentRangedName != null) currentRangedName.text = $"원거리: {rangedName}";
    }
}