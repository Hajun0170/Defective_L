using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip swapSound; // 교체 사운드 

    [Header("Battle Slots")]
    // 인벤토리 전체가 아니라, 전투에 들고 나갈 3개의 무기 리스트
    public List<Weapon> equippedMelee = new List<Weapon>(); 
    public List<Weapon> equippedRanged = new List<Weapon>();

    // 실제 장착 중인 무기 번호
    private int currentMeleeIndex = 0;
    private int currentRangedIndex = 0;

    // UI에 보여질 예약된 무기 번호
    private int previewMeleeIndex = 0;
    private int previewRangedIndex = 0;

    private PlayerAttack playerAttack;
    private PlayerStats playerStats;

    // 애니메이터를 제어하기 위해 변수 추가
    private Animator anim;

    [Header("게임의 모든 무기 리스트 등록")]

    public Weapon[] allGameWeapons;

    
    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();

        // 같은 오브젝트자식)에 있는 애니메이터 가져오기
        anim = GetComponent<Animator>(); 
    }

    private void Start()
    {

        // 리스트 초기화
        equippedMelee.Clear();
        equippedRanged.Clear();

         LoadWeaponsFromData();

       // 씬이 시작될 때 DataManager에서 저장된 무기 번호 불러오기
        if (DataManager.Instance != null)
        {
            currentMeleeIndex = DataManager.Instance.currentData.equippedMeleeIndex;
            currentRangedIndex = DataManager.Instance.currentData.equippedRangedIndex;

            // 무기가 하나라도 있을 때만 Clamp 처리
            if (equippedMelee.Count > 0)
                currentMeleeIndex = Mathf.Clamp(currentMeleeIndex, 0, equippedMelee.Count - 1);
            else
                currentMeleeIndex = -1; // 없으면 -1

            if (equippedRanged.Count > 0)
                currentRangedIndex = Mathf.Clamp(currentRangedIndex, 0, equippedRanged.Count - 1);
            else
                currentRangedIndex = -1;
   
            // UI에 보여질 예약 번호를 현재 번호와 맞춤
            previewMeleeIndex = currentMeleeIndex;
            previewRangedIndex = currentRangedIndex;
        }

        EquipWeapons(); // 초기 무기 장착
        UpdateUI();     // UI 갱신

        // UI 매니저에게 현재 든 무기를 보고 (Start 시점에 동기화)
        if (UIManager.Instance != null && equippedMelee.Count > 0)
        {
            // 현재 들고 있는 무기를 슬롯에 바로 장착
            UIManager.Instance.UpdateWeaponSlots(equippedMelee[currentMeleeIndex], 
                (equippedRanged.Count > 0) ? equippedRanged[currentRangedIndex] : null);
        }
    }

    private void Update()
    {
        // 컷신, 일시정지 예외처리
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;

        // A 근거리 무기 예약 (아이콘만 변경, 실제 교체 X)
        if (Input.GetKeyDown(KeyCode.A) && equippedMelee.Count > 1)
        {
            previewMeleeIndex = (previewMeleeIndex + 1) % equippedMelee.Count;
            UpdateUI(); // UI만 바뀜
        }

        // S 원거리 무기 예약
        if (Input.GetKeyDown(KeyCode.S) && equippedRanged.Count > 1)
        {
            previewRangedIndex = (previewRangedIndex + 1) % equippedRanged.Count;
            UpdateUI();
        }
    }

    // PlayerAttack에서 Z키 누를 때 호출
    public void TrySwapMeleeOnAttack()
    {
        // 예약된 게 현재랑 같으면 교체 패스
        if (previewMeleeIndex == currentMeleeIndex) return;

        // 다르다면 티켓 쓰고 교체 시도
        if (playerStats.UseTicket())
        {
            currentMeleeIndex = previewMeleeIndex;
            EquipWeapons();
            
            // 저장
            SaveWeaponData();
        }
        else
        {
            // 티켓 없으면 예약 취소 (원래 무기로 되돌림)
            previewMeleeIndex = currentMeleeIndex;
            Debug.Log("티켓 부족 교체 실패.");
        }
        UpdateUI();
    }

    // PlayerAttack에서 X키 누를 때 호출
    public void TrySwapRangedOnAttack()
    {
        if (previewRangedIndex == currentRangedIndex) return;

        if (playerStats.UseTicket())
        {
            currentRangedIndex = previewRangedIndex;
            EquipWeapons();

            // 저장!
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
        }
    }

    private void EquipWeapons()
    {
        // 근거리 무기 장착 처리
        if (equippedMelee.Count > 0 && currentMeleeIndex >= 0 && currentMeleeIndex < equippedMelee.Count)
        {
            Weapon currentWeapon = equippedMelee[currentMeleeIndex];
            playerAttack.meleeWeapon = currentWeapon;
            
            // 애니메이터 교체 (무기 있을 때)
            UpdateAnimationController(currentWeapon);

            // 교체 사운드 재생
        if (AudioManager.Instance != null && swapSound != null)
        {
            // PlaySFX만 사용
            AudioManager.Instance.PlaySFX(swapSound); 
        }

        }
        else
        {
            // 무기 없으면 슬롯 비우기
            playerAttack.meleeWeapon = null;
        }

        // 원거리 무기 장착 처리
        if (equippedRanged.Count > 0 && currentRangedIndex >= 0 && currentRangedIndex < equippedRanged.Count)
        {
            playerAttack.rangedWeapon = equippedRanged[currentRangedIndex];

            // 교체 사운드 재생
        if (AudioManager.Instance != null && swapSound != null)
        {
            AudioManager.Instance.PlaySFX(swapSound); 
        }
        
        }
        else
        {
            playerAttack.rangedWeapon = null;
        }

    }

    // 애니메이터 교체 및 새로고침 함수
    private void UpdateAnimationController(Weapon weapon)
    {
        // 데이터 확인
        if (anim == null || weapon == null || weapon.overrideController == null) return;

        // 컨트롤러 교체 (런타임에 갈아끼움)
        anim.runtimeAnimatorController = weapon.overrideController;

        // 애니메이터 강제 리셋
        anim.Rebind(); 

        // 상태 재생
        anim.Play("Idle_E", 0, 0f); //무기 공통 대기 모션

        Debug.Log($"애니메이션 교체 {weapon.overrideController.name}");
    }

    private void UpdateUI()
    {
        if (UIManager.Instance == null) return;

        // 소지 여부 확인 (Count가 0보다 커야 보여줌)
        bool hasMelee = equippedMelee.Count > 0;
        bool hasRanged = equippedRanged.Count > 0;

        //슬롯 전체 켜기/끄기 요청
        UIManager.Instance.SetSlotVisibility(hasMelee, hasRanged);

        // 아이콘 이미지 갱신 (보여줄 때만 계산)
        Weapon nextMelee = null;
        Weapon nextRanged = null;

        if (hasMelee) 
            nextMelee = equippedMelee[previewMeleeIndex];
            
        if (hasRanged) 
            nextRanged = equippedRanged[previewRangedIndex];
        // UIManager에 새로 만들 함수를 호출 
        UIManager.Instance.UpdateWeaponSlots(nextMelee, nextRanged);
    }
    public void AddWeapon(Weapon newWeapon)
    {
        if (newWeapon == null) return;

        // 이미 가지고 있는지 확인 (중복 방지)
        if (newWeapon.type == WeaponType.Melee)
        {
            if (!equippedMelee.Contains(newWeapon))
            {
                equippedMelee.Add(newWeapon);
            }
            
            // 획득한 무기를 장착 상태로 만듦
            // 리스트에서 이 무기의 위치를 찾아서 인덱스로 설정
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

        // DataManager 획득 장부에 도장 찍기 (영구 저장용)
        if (DataManager.Instance != null)
        {
            // 무기 ID를 이용해 hasWeapons 배열 업데이트
            DataManager.Instance.currentData.hasWeapons[newWeapon.weaponID] = true;
            
            // 현재 장착 인덱스도 저장
            SaveWeaponData();
            
            // 즉시 파일 저장 
            DataManager.Instance.SaveDataToDisk();
        }

        // 실제 장착 및 UI 갱신
        EquipWeapons();
        UpdateUI();

        Debug.Log($"무기 장착 완료: {newWeapon.weaponName}");
    }
    
    private void LoadWeaponsFromData()
    {
        if (DataManager.Instance == null) return;

        bool[] hasWeapons = DataManager.Instance.currentData.hasWeapons;

        // 전체 무기를 훑음
        for (int i = 0; i < allGameWeapons.Length; i++)
        {
            // 만약 i번 무기를 가지고 있다면? (hasWeapons[i] == true)
            if (i < hasWeapons.Length && hasWeapons[i]) 
            {
                Weapon w = allGameWeapons[i];
                
                // 타입에 맞춰 리스트에 추가
                if (w.type == WeaponType.Melee) equippedMelee.Add(w);
                else equippedRanged.Add(w);
            }
        }
    }
}