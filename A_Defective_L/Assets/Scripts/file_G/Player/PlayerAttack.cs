using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Current Weapons")]
    public Weapon meleeWeapon; 
    public Weapon rangedWeapon; 
    
    [Header("Setup")]
    public Transform attackPoint;

    // 참조 컴포넌트
    private PlayerStats playerStats;
    private WeaponManager weaponManager;
    private Animator anim;
    
    // ★ 상태 관리 변수
    private bool isAttacking = false; 
    
    // 쿨타임 관리
    private float lastAttackTime = 0f;

    

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponManager = GetComponent<WeaponManager>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 예외 상황 체크
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;

        // 공격 중이거나 쿨타임이면 입력 무시
        if (isAttacking) return;

        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        // 1. 근접 공격 (Z)
        if (Input.GetKeyDown(KeyCode.Z))
        {
        
            // 교체 예약 확인
            if (weaponManager != null) weaponManager.TrySwapMeleeOnAttack();

            if (meleeWeapon != null)
            {
                // 쿨타임 체크 (무기별 attackRate 사용)
                if (Time.time >= lastAttackTime + meleeWeapon.attackRate)
                {
                     anim.SetTrigger("A1");
                    StartAttack(meleeWeapon);
                }
            }
        }
        
        // 2. 원거리 공격 (X)
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (weaponManager != null) weaponManager.TrySwapRangedOnAttack();

            // ★ [핵심] 현재 무기가 'RangedWeapon' 타입인지 확인하고 데이터 가져오기
            if (rangedWeapon != null && rangedWeapon is RangedWeapon rangedData)
            {
                // 쿨타임 체크
                if (Time.time >= lastAttackTime + rangedWeapon.attackRate)
                {
                    // ★ [수정 핵심] 공격하기 전에 "비용"을 먼저 확인합니다.
                    int cost = rangedData.GetCurrentCost(); // RangedWeapon에 만든 함수

                    if (playerStats.CurrentGauge >= cost)
                    {
                    // ★ 1. 무기에 적혀있는 트리거 이름을 당긴다! 
                    // (개틀링이면 "R_Skill_2", 나이프면 "R_Skill_1"이 실행됨)
                    anim.SetTrigger(rangedData.skillTriggerName); 
                    
                    // ★ 2. 공격 로직 시작 (6발 발사 등)
                    StartSkillAttack(rangedWeapon);
                    }
                }
            }
        }
    }

    // 스킬 실행 헬퍼 함수
    private void StartSkillAttack(Weapon weapon)
    {
        isAttacking = true; 
        // Weapon의 UseSkill 호출
        weapon.UseSkill(attackPoint, playerStats, () => 
        {
            isAttacking = false;
            lastAttackTime = Time.time;
        });
    }

    // 공격 실행 및 상태 관리 통합 함수
    private void StartAttack(Weapon weapon)
    {
        isAttacking = true; // 1. 행동 불능 (Lock)

        // ★ [수정] 쿨타임 갱신을 공격 '시작' 시점에 함 (더 반응성 좋음)
        lastAttackTime = Time.time;

        // 2. 무기에게 공격 명령 ("끝나면 알려줘" 콜백 전달)
        weapon.PerformAttack(attackPoint, playerStats, () => 
        {
            // 이 괄호 안의 코드는 무기 공격(코루틴)이 다 끝난 뒤 실행됨
            isAttacking = false; // 3. 행동 가능 (Unlock)
            //lastAttackTime = Time.time; // 쿨타임 갱신
            // Debug.Log("공격 종료. 다음 행동 가능.");
        });
    }

    // ★ 범위 확인용 기즈모 (에디터 전용)
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // 현재 들고 있는 무기가 '차지 웨폰'이면 빨간 원 그리기
        if (meleeWeapon is ChargeWeapon chargeData)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, chargeData.attackRange);
        }
    }
}