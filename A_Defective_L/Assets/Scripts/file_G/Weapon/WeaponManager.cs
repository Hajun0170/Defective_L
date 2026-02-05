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

    // ★ [추가] 애니메이터를 제어하기 위해 변수 추가
    private Animator anim;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();

        // ★ [추가] 같은 오브젝트(또는 자식)에 있는 애니메이터 가져오기
        anim = GetComponent<Animator>(); 
        // 만약 애니메이터가 자식에 있다면: anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
       // ★ [핵심] 씬이 시작될 때 DataManager에서 저장된 무기 번호 불러오기
        if (DataManager.Instance != null)
        {
            currentMeleeIndex = DataManager.Instance.currentData.equippedMeleeIndex;
            currentRangedIndex = DataManager.Instance.currentData.equippedRangedIndex;
        
            // 인덱스가 범위를 벗어나지 않게 안전 장치
            currentMeleeIndex = Mathf.Clamp(currentMeleeIndex, 0, equippedMelee.Count - 1);
            currentRangedIndex = Mathf.Clamp(currentRangedIndex, 0, equippedRanged.Count - 1);
        }

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
        // ... (티켓 검사 등 기존 로직) ...
        if (playerStats.UseTicket())
        {
            currentMeleeIndex = previewMeleeIndex;
            EquipWeapons();
            
            // ★ 저장!
            SaveWeaponData();
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

            // ★ 저장!
            SaveWeaponData();
        }
        else
        {
            previewRangedIndex = currentRangedIndex;
        }
        UpdateUI();
    }

    // 데이터 저장용 헬퍼 함수
    private void SaveWeaponData()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.equippedMeleeIndex = currentMeleeIndex;
            DataManager.Instance.currentData.equippedRangedIndex = currentRangedIndex;
            // (필요하다면 DataManager.Instance.SaveGame() 호출)
        }
    }

    private void EquipWeapons()
    {
        if (equippedMelee.Count > 0) {
            playerAttack.meleeWeapon = equippedMelee[currentMeleeIndex];
            
            // 2. ★ [추가] 근접 무기에 오버라이드 컨트롤러가 있다면 교체!
            Weapon currentWeapon = equippedMelee[currentMeleeIndex];
            UpdateAnimationController(currentWeapon);

            }
        if (equippedRanged.Count > 0) playerAttack.rangedWeapon = equippedRanged[currentRangedIndex];
    }

    // ★ [새로 만듦] 애니메이터 교체 및 새로고침 함수
    private void UpdateAnimationController(Weapon weapon)
    {
        // 1. 데이터 확인
        if (anim == null || weapon == null || weapon.overrideController == null) return;

        // 2. 컨트롤러 교체 (런타임에 갈아끼우기)
        anim.runtimeAnimatorController = weapon.overrideController;

        // 3. ★ [필수] 애니메이터 강제 리셋 (Rebind)
        // 이게 없으면 유니티가 "어? 같은 Idle 상태네?" 하고 모션을 안 바꿀 수 있음
        anim.Rebind(); 

        // 4. 상태 재생
        // "Idle_E"는 님 애니메이터에 있는 기본 상태 이름이어야 합니다!
        // (Idle_E가 아니라 Idle이라면 "Idle"로 수정하세요)
        anim.Play("Idle_E", 0, 0f); 

        Debug.Log($"⚔️ 애니메이션 교체 완료: {weapon.overrideController.name}");
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

/*
  public void AddWeapon(Weapon newWeapon)
    {
        // 1. 무기 타입에 따라 적절한 리스트에 추가하고, 
        // 2. ★ [추가] 방금 추가한 무기의 인덱스로 '현재 무기'를 변경합니다.

        if (newWeapon.type == WeaponType.Melee) 
        {
            equippedMelee.Add(newWeapon);
            
            // 방금 추가된 무기는 리스트의 맨 마지막에 있음
            currentMeleeIndex = equippedMelee.Count - 1; 
            
            // UI 예약 번호도 같이 맞춰줌 (안 그러면 꼬임)
            previewMeleeIndex = currentMeleeIndex; 
        }
        else
        {
            equippedRanged.Add(newWeapon);
            
            currentRangedIndex = equippedRanged.Count - 1;
            previewRangedIndex = currentRangedIndex;
        }
    
        

        // 3. 실제 장착 실행 (애니메이션 교체 등 포함)
        EquipWeapons(); 

        // 4. 데이터 저장 (먹자마자 저장해야 안전)
        SaveWeaponData();

        // 5. UI 갱신 (슬롯 켜짐)
        UpdateUI(); 
        
        Debug.Log($"⚔️ 무기 획득 및 장착 완료: {newWeapon.weaponName}");
    }
    */
    public void AddWeapon(Weapon newWeapon)
    {
        if (newWeapon == null) return;

        // 1. 이미 가지고 있는지 확인 (중복 방지)
        if (newWeapon.type == WeaponType.Melee)
        {
            if (!equippedMelee.Contains(newWeapon))
            {
                equippedMelee.Add(newWeapon);
            }
            
            // ★ 가지고 있든 없든, 방금 먹은 이 무기를 '장착' 상태로 만듦
            // (리스트에서 이 무기의 위치를 찾아서 인덱스로 설정)
            currentMeleeIndex = equippedMelee.IndexOf(newWeapon);
            previewMeleeIndex = currentMeleeIndex;
        }
        else // 원거리
        {
            if (!equippedRanged.Contains(newWeapon))
            {
                equippedRanged.Add(newWeapon);
            }
            currentRangedIndex = equippedRanged.IndexOf(newWeapon);
            previewRangedIndex = currentRangedIndex;
        }

        // 2. DataManager '획득 장부'에 도장 찍기 (영구 저장용)
        if (DataManager.Instance != null)
        {
            // 무기 ID를 이용해 hasWeapons 배열 업데이트
            DataManager.Instance.currentData.hasWeapons[newWeapon.weaponID] = true;
            
            // 현재 장착 인덱스도 저장
            SaveWeaponData();
            
            // 즉시 파일 저장 (선택)
            DataManager.Instance.SaveDataToDisk();
        }

        // 3. 실제 장착 및 UI 갱신
        EquipWeapons();
        UpdateUI();

        Debug.Log($"⚔️ 무기 장착 완료: {newWeapon.weaponName}");
    }
    
}