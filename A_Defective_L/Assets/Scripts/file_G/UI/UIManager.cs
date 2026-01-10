using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; 

    [Header("1. Health UI")]
    [SerializeField] private Image[] hpCells; // 하트 이미지 배열
    [SerializeField] private Color hpFullColor = Color.red; 
    [SerializeField] private Color hpEmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); 

    [Header("2. Resource UI")]
    [SerializeField] private GameObject[] gaugeCells; // 게이지 칸 배열
    [SerializeField] private GameObject[] ticketIcons; // 티켓 오브젝트 배열

    [Header("3. Weapon UI - Parents (켜고 끄기용)")]
    // 무기가 없을 때 통째로 숨길 부모 오브젝트 (배경 + 아이콘 + 키 설명)
    public GameObject meleeSlotGroup;   
    public GameObject rangedSlotGroup;  
    public GameObject ticketInfoGroup;  

    [Header("4. Weapon UI - Children (이미지 교체용)")]
    // 실제 스프라이트(그림)를 갈아 끼울 자식 Image 컴포넌트
    public Image meleeIcon;
    public Image rangedIcon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --- 기능 함수들 ---

    // 1. 체력 업데이트 (색상 변경 방식)
    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hpCells.Length; i++)
        {
            // 인덱스가 현재 체력보다 작으면 '채워진 색', 아니면 '빈 색'
            hpCells[i].color = (i < currentHealth) ? hpFullColor : hpEmptyColor;
            hpCells[i].gameObject.SetActive(true);
        }
    }

    // 2. 게이지 업데이트 (스택형)
    public void UpdateGauge(int current, int max)
    {
        for (int i = 0; i < gaugeCells.Length; i++)
        {
            // 현재 수치만큼 켜고, 나머지는 끔
            gaugeCells[i].SetActive(i < current);
        }
    }

    // 3. 교환권 업데이트
    public void UpdateTickets(int count)
    {
        for (int i = 0; i < ticketIcons.Length; i++)
        {
            ticketIcons[i].SetActive(i < count);
        }
    }

    // 4. 무기 슬롯 전체 보이기/숨기기 (무기 획득 시 사용)
    public void SetSlotVisibility(bool showMelee, bool showRanged)
    {
        if (meleeSlotGroup != null) meleeSlotGroup.SetActive(showMelee);
        if (rangedSlotGroup != null) rangedSlotGroup.SetActive(showRanged);
        
        // 근접이나 원거리 중 하나라도 있으면 티켓 정보창도 보여줌
        if (ticketInfoGroup != null)
            ticketInfoGroup.SetActive(showMelee || showRanged);
    }

    // 5. 무기 아이콘 그림 바꾸기 (무기 교체 시 사용)
    public void UpdateWeaponSlots(Weapon melee, Weapon ranged)
    {
        // 안전장치: 연결 안 되어 있으면 에러 로그 출력
        if (meleeIcon == null || rangedIcon == null)
        {
            Debug.LogError("❌ UIManager에 아이콘 이미지가 연결되지 않았습니다!");
            return;
        }

        // 근접 무기 아이콘 갱신
        if (melee != null)
        {
            meleeIcon.sprite = melee.icon;
            // 혹시 모르니 켜주기
            meleeIcon.gameObject.SetActive(true);
        }
        
        // 원거리 무기 아이콘 갱신
        if (ranged != null)
        {
            rangedIcon.sprite = ranged.icon;
            rangedIcon.gameObject.SetActive(true);
        }
    }
}