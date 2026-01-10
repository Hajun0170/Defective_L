using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Battle Slots (전투용 3칸 권장)")]
    // 인벤토리 전체가 아니라, 전투에 들고 나갈 3개의 무기 리스트
    public List<Weapon> equippedMelee = new List<Weapon>(); 
    public List<Weapon> equippedRanged = new List<Weapon>();

    // 실제 장착 중인 무기 번호
    private int currentMeleeIndex = 0;
    private int currentRangedIndex = 0;

    // UI에 보여질 '예약된' 무기 번호
    private int previewMeleeIndex = 0;
    private int previewRangedIndex = 0;

    private PlayerAttack playerAttack;
    private PlayerStats playerStats;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 초기화: 현재 무기와 예약 무기를 동일하게 설정
        if (equippedMelee.Count > 0) previewMeleeIndex = currentMeleeIndex;
        if (equippedRanged.Count > 0) previewRangedIndex = currentRangedIndex;

        EquipWeapons(); // 초기 무기 실장착
        UpdateUI();     // UI 갱신
    }

    private void Update()
    {
        // 컷신, 일시정지 예외처리
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;

        // 1. [A 키] 근거리 무기 예약 (아이콘만 변경, 실제 교체 X)
        if (Input.GetKeyDown(KeyCode.A) && equippedMelee.Count > 1)
        {
            previewMeleeIndex = (previewMeleeIndex + 1) % equippedMelee.Count;
            UpdateUI(); // UI만 바뀜
        }

        // 2. [S 키] 원거리 무기 예약
        if (Input.GetKeyDown(KeyCode.S) && equippedRanged.Count > 1)
        {
            previewRangedIndex = (previewRangedIndex + 1) % equippedRanged.Count;
            UpdateUI();
        }
    }

    // ★ PlayerAttack에서 Z키 누를 때 호출
    public void TrySwapMeleeOnAttack()
    {
        // 예약된 게 현재랑 같으면 교체 로직 패스
        if (previewMeleeIndex == currentMeleeIndex) return;

        // 다르다면 티켓 쓰고 교체 시도
        if (playerStats.UseTicket())
        {
            currentMeleeIndex = previewMeleeIndex; // 확정
            EquipWeapons();
            playerStats.ActivateSwapBuff(); // 버프 발동 (기존 함수 재사용)
            
            // 이펙트나 사운드 추가 가능
            Debug.Log($"⚔️ 무기 교체 공격! -> {equippedMelee[currentMeleeIndex].weaponName}");
        }
        else
        {
            // 티켓 없으면 예약 취소 (원래 무기로 되돌림)
            previewMeleeIndex = currentMeleeIndex;
            Debug.Log("티켓 부족! 교체 실패.");
        }
        UpdateUI();
    }

    // ★ PlayerAttack에서 X키 누를 때 호출
    public void TrySwapRangedOnAttack()
    {
        if (previewRangedIndex == currentRangedIndex) return;

        if (playerStats.UseTicket())
        {
            currentRangedIndex = previewRangedIndex;
            EquipWeapons();
            // 원거리 전용 버프가 있다면 ActivateRangedBuff() 호출
            Debug.Log($"🔫 무기 교체 사격! -> {equippedRanged[currentRangedIndex].weaponName}");
        }
        else
        {
            previewRangedIndex = currentRangedIndex;
        }
        UpdateUI();
    }

    private void EquipWeapons()
    {
        if (equippedMelee.Count > 0) playerAttack.meleeWeapon = equippedMelee[currentMeleeIndex];
        if (equippedRanged.Count > 0) playerAttack.rangedWeapon = equippedRanged[currentRangedIndex];
    }

    private void UpdateUI()
    {
        if (UIManager.Instance == null) return;

        // 1. 소지 여부 확인 (Count가 0보다 커야 보여줌)
        bool hasMelee = equippedMelee.Count > 0;
        bool hasRanged = equippedRanged.Count > 0;

        // 2. 슬롯 전체 켜기/끄기 요청
        UIManager.Instance.SetSlotVisibility(hasMelee, hasRanged);

        // 3. 아이콘 이미지 갱신 (보여줄 때만 계산)
        Weapon nextMelee = null;
        Weapon nextRanged = null;

        if (hasMelee) 
            nextMelee = equippedMelee[previewMeleeIndex];
            
        if (hasRanged) 
            nextRanged = equippedRanged[previewRangedIndex];
        // UIManager에 새로 만들 함수를 호출 (아래 UI 파트에서 설명)
        UIManager.Instance.UpdateWeaponSlots(nextMelee, nextRanged);
    }

    public void AddWeapon(Weapon newWeapon)
    {
        // 무기 타입에 따라 적절한 리스트에 추가
        if (newWeapon.type == WeaponType.Melee) // Weapon 스크립트에 타입이 있다고 가정
        {
            equippedMelee.Add(newWeapon);
        }
        else
        {
            equippedRanged.Add(newWeapon);
        }

        // ★ 핵심: 무기를 먹었으니 UI를 다시 그려라! (이때 슬롯이 켜짐)
        UpdateUI(); 
    }
    
}