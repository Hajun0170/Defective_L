using UnityEngine;
using UnityEngine.UI; // UI 기능을 위해 필수
using TMPro;          // TextMeshPro (글자) 사용 시 필요

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글톤 인스턴스

    [Header("Health UI")]
    [SerializeField] private Image[] hpCells; // 체력 칸 배열 (하트 5개)
    [SerializeField] private Color hpFullColor = Color.red; // 꽉 찼을 때 (빨강)
    [SerializeField] private Color hpEmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 비었을 때 (반투명 회색)

    [Header("HUD References")]
    // [수정] 하나의 이미지 대신, 여러 개의 게이지 칸(Cell)을 담을 배열로 변경
    // 기존: [SerializeField] private Image cylinderGaugeFill; 
    [SerializeField] private GameObject[] gaugeCells;

    [SerializeField] private GameObject[] ticketIcons; // 교환권 아이콘 3개 (켜고 끄기)
    [SerializeField] private TextMeshProUGUI hpText;   // (선택) 체력 표시용

    [Header("Swap UI References")]
    [SerializeField] private GameObject swapPanel;     // 교체 메뉴 전체 패널
    [SerializeField] private TextMeshProUGUI currentMeleeName;  // 현재 선택된 근접 무기 이름
    [SerializeField] private TextMeshProUGUI currentRangedName; // 현재 선택된 원거리 무기 이름

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // UI(Canvas)도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 UI 파괴
        }
    }

    // 1. 실린더 게이지 업데이트 (스택형)
    public void UpdateGauge(int current, int max)
    {
       // 배열에 들어있는 모든 칸을 순회하며 검사
        for (int i = 0; i < gaugeCells.Length; i++)
        {
            // 현재 게이지 수치보다 인덱스가 작으면 켜기 (Active True)
            // 예: current가 3이면, 인덱스 0, 1, 2가 켜짐
            if (i < current)
            {
                gaugeCells[i].SetActive(true);
            }
            else
            {
                // 나머지는 끄기 (Active False)
                gaugeCells[i].SetActive(false);
            }
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

    // [신규 기능] 체력 UI 업데이트
    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hpCells.Length; i++)
        {
            // 오브젝트는 끄지 않고 색상만 변경 (레이아웃 고정)
            hpCells[i].gameObject.SetActive(true);

            if (i < currentHealth)
            {
                hpCells[i].color = hpFullColor; // 남은 체력
            }
            else
            {
                hpCells[i].color = hpEmptyColor; // 닳은 체력
            }
        }
    }
}