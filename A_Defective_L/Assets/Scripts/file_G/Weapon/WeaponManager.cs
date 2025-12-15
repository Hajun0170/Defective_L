using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Inventory")]
    public List<Weapon> meleeWeapons = new List<Weapon>();  // 근거리 무기 리스트
    public List<Weapon> rangedWeapons = new List<Weapon>(); // 원거리 무기 리스트

    [Header("UI Reference")]
    public GameObject swapUIPanel; // 교체 중 띄울 UI

    private int meleeIndex = 0;
    private int rangedIndex = 0;
    private bool isSwapping = false;

    private PlayerAttack playerAttack;
    private PlayerStats playerStats;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        EquipWeapons(); // 초기 무기 장착
    }

    private void Update()
    {
        // 1. Alt 키 누름 (진입)
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            isSwapping = true;
            Time.timeScale = 0f; // 시간 정지
            if (swapUIPanel) swapUIPanel.SetActive(true);
            Debug.Log("무기 교체 모드 진입");
        }

        // 2. Alt 키 뗌 (확정 및 버프)
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isSwapping = false;
            Time.timeScale = 1f; // 시간 재개
            if (swapUIPanel) swapUIPanel.SetActive(false);

            // 교환권을 소모하여 무기 교체 및 버프 발동
            if (playerStats.UseTicket())
            {
                EquipWeapons();
                playerStats.ActivateSwapBuff(); // 2초간 20% 공증
            }
            else
            {
                Debug.Log("교환권이 부족하여 무기를 교체할 수 없습니다!");
                // (선택 사항: 교환권 없으면 원래 무기로 되돌리는 로직 추가 가능)
            }
        }

        // 3. 교체 모드 중 조작 (십자형 선택)
        if (isSwapping)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeIndex(ref meleeIndex, meleeWeapons.Count, 1, "근거리");
            if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeIndex(ref meleeIndex, meleeWeapons.Count, -1, "근거리");
            if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeIndex(ref rangedIndex, rangedWeapons.Count, 1, "원거리");
            if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeIndex(ref rangedIndex, rangedWeapons.Count, -1, "원거리");
        }
    }

    // 인덱스 순환 헬퍼 함수
    private void ChangeIndex(ref int index, int count, int direction, string type)
    {
        if (count == 0) return;
        index = (index + direction + count) % count;
        Debug.Log($"{type} 무기 선택 변경: 인덱스 {index}");
    }

    private void EquipWeapons()
    {
        if (meleeWeapons.Count > 0) playerAttack.meleeWeapon = meleeWeapons[meleeIndex];
        if (rangedWeapons.Count > 0) playerAttack.rangedWeapon = rangedWeapons[rangedIndex];
        
        Debug.Log($"장착 완료: 근접[{playerAttack.meleeWeapon.weaponName}] / 원거리[{playerAttack.rangedWeapon.weaponName}]");
    }
}