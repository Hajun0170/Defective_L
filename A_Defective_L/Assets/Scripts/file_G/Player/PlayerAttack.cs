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
   // private bool isAttacking = false; 
    
    // 쿨타임 관리
    //private float lastAttackTime = 0f;

    
    // =========================================================
    // ★ [핵심 1] 선입력 시스템 변수
    // =========================================================
    [Header("Combo & Input Buffer")]
    public float inputBufferTime = 0.4f; // 0.4초까지는 미리 눌러도 봐줌
    private float lastInputTime = -99f;  // 마지막으로 키 누른 시간
    
    // 공격 종류 기억 (Z: 근접, X: 원거리)
    private enum AttackType { None, Melee, Ranged }
    private AttackType bufferedAttackType = AttackType.None;

    // 콤보 가능 시점 (모션의 60%가 지나면 다음 공격 허용)
    [Range(0f, 1f)] public float comboStartTime = 0.6f;

    // ★ isAttacking 변수는 삭제하거나, 단순 정보용으로만 씁니다.
    // (애니메이션 상태를 직접 확인하는 게 더 정확함)

    // ★ 안전장치: 너무 빠른 중복 실행 방지
    private float lastExecuteTime = -99f; 
    private const float MIN_ATTACK_INTERVAL = 0.1f; // 0.1초 내 재실행 금지


    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponManager = GetComponent<WeaponManager>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;

        CaptureInput();
        TryExecuteAttack();

        // ★ [핵심 추가] 공격 중이 아니면 속도 정상화
        RestoreAnimationSpeed();
    }

    // ★ 속도 관리 전용 함수
    private void RestoreAnimationSpeed()
    {
        // 현재 애니메이션 상태 확인
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // "Attack" 태그가 붙은 동작(공격)을 하는 중이 아니라면?
        if (!stateInfo.IsTag("Attack")) 
        {
            // 속도가 1이 아니라면 강제로 1로 맞춤 (걷기, 대기 등 정상 속도)
            if (Mathf.Abs(anim.speed - 1.0f) > 0.01f) 
            {
                anim.speed = 1.0f;
            }
        }
        
        // (참고) 만약 공격 중이라면? 
        // ExecuteMelee에서 설정한 속도(예: 2.0)가 유지되어야 하므로 건드리지 않음
    }
    
    // 1. 입력 감지
    private void CaptureInput()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            lastInputTime = Time.time;
            bufferedAttackType = AttackType.Melee;
            if (weaponManager != null) weaponManager.TrySwapMeleeOnAttack();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            lastInputTime = Time.time;
            bufferedAttackType = AttackType.Ranged;
            if (weaponManager != null) weaponManager.TrySwapRangedOnAttack();
        }
    }

    // 2. 공격 실행 판단
    private void TryExecuteAttack()
    {
        // 입력 유효성 검사
        if (Time.time - lastInputTime > inputBufferTime) return;

        // ★ 안전장치: 방금 막 공격 명령 내렸으면 잠시 대기
        if (Time.time - lastExecuteTime < MIN_ATTACK_INTERVAL) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool isAttackingNow = stateInfo.IsTag("Attack");

        bool canAttack = false;

        if (!isAttackingNow)
        {
            canAttack = true; // 공격 중 아니면 즉시 실행
        }
        else
        {
            // 공격 중이면 콤보 타이밍(60% 이상) 확인 + 끝자락(1.0) 아님
            if (stateInfo.normalizedTime >= comboStartTime && stateInfo.normalizedTime < 1.0f)
            {
                canAttack = true;
            }
        }

        if (canAttack)
        {
            // 실행 전 타입에 따라 분기
            if (bufferedAttackType == AttackType.Melee)
            {
                ExecuteMelee();
            }
            else if (bufferedAttackType == AttackType.Ranged)
            {
                // ★ 원거리는 쿨타임이 별도로 필요할 수 있음 (여기선 그냥 실행)
                ExecuteRanged();
            }

            // 실행 후 정리
            lastInputTime = -99f; 
            bufferedAttackType = AttackType.None;
            lastExecuteTime = Time.time; // ★ 실행 시간 기록
        }
    }

    private void ExecuteMelee()
    {
        if (meleeWeapon == null) return;

        // 1. 공격 속도 조절 (무기 딜레이를 애니메이션 속도로 변환)
        float attackSpeedMultiplier = 1.0f;
        if (meleeWeapon.attackRate > 0)
        {
            // 딜레이가 짧을수록(0.2) -> 속도는 빨라짐(5배)
            // 너무 빠르거나 느려지지 않게 Clamp (0.5배 ~ 3배)
            attackSpeedMultiplier = Mathf.Clamp(1f / meleeWeapon.attackRate, 0.5f, 3.0f);
        }
        anim.speed = attackSpeedMultiplier;

        // 2. 실행
        anim.SetTrigger("A1"); 
        meleeWeapon.PerformAttack(attackPoint, playerStats, null);
    }
    private void ExecuteRanged()
    {
        if (rangedWeapon == null) return;
        
        // 원거리는 비용 체크 필요
        if (rangedWeapon is RangedWeapon rangedData)
        {
            int cost = rangedData.GetCurrentCost();
            if (playerStats.CurrentGauge >= cost)
            {
                anim.SetTrigger(rangedData.skillTriggerName);
                
                // 스킬 로직 실행
                rangedWeapon.UseSkill(attackPoint, playerStats, null);
            }
            else
            {
                // 게이지 부족하면 입력 취소
                // Debug.Log("게이지 부족!");
                lastInputTime = -99f; 
            }
        }
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