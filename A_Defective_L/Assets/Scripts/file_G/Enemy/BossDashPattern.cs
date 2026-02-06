using System.Collections;
using UnityEngine;

public class BossDashPattern : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectRange = 8.0f;
    [SerializeField] private float attackRange = 1.0f; // 평타 사거리
    [SerializeField] private int normalDamage = 1;

    [Header("Basic Combat")]
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private Vector2 rangeCenterOffset;

    [Header("★ Dash Pattern (Phase 2)")]
    [SerializeField] private float phaseTwoRatio = 0.5f; // 체력 50% 이하
    [SerializeField] private float dashCooldown = 5.0f;  // 돌진 빈도
    [SerializeField] private float dashWarningTime = 1.0f; // 돌진 전 뜸 들이기 (경고)
    [SerializeField] private float dashSpeed = 12.0f;    // 돌진 속도
    [SerializeField] private float dashDuration = 0.5f;  // 돌진 지속 시간
    [SerializeField] private int dashDamage = 2;         // 돌진은 더 아프게

    [Header("Effects & Sound")]
    [SerializeField] private GameObject warningEffect;   // 돌진 전 경고 이펙트 (! 표시 등)
    [SerializeField] private GameObject dashDustEffect;  // 달릴 때 먼지
    [SerializeField] private AudioClip dashSound;        // 콰광! 소리

    [Header("References")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private BossController bossController; // 체력 확인용

    // 상태 변수
    private bool isAttacking = false;       // 평타 중
    private bool isDashing = false;         // 돌진 패턴 중
    private float nextAttackTime = 0f;
    private float nextDashTime = 0f;
    private Vector3 originalScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossController = GetComponent<BossController>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 시작하자마자 돌진하지 않도록 쿨타임 초기화
        nextDashTime = Time.time + 3.0f;
    }

    private void Update()
    {
        // 플레이어 없거나, 보스 죽었거나, 돌진 중이면 기본 AI 무시
        if (player == null || isDashing) return;
        if (bossController != null && bossController.GetCurrentHealth() <= 0) return;

        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        float distanceToPlayer = Vector2.Distance(centerPos, player.position);

        // ====================================================
        // ★ 2페이즈 체크: 체력 50% 이하 & 쿨타임 완료
        // ====================================================
        bool isPhaseTwo = false;
        if (bossController != null && bossController.GetHealthPercentage() <= phaseTwoRatio)
        {
            isPhaseTwo = true;
        }

        if (isPhaseTwo && Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
            return;
        }

        // --- 기본 AI (1페이즈 및 쿨타임 중) ---
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectRange && !isAttacking)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    // ================================================================
    // ★ [핵심] 돌진 코루틴
    // ================================================================
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        StopMoving(); // 일단 멈춤

        // 1. [경고] 뜸 들이기 (플레이어 바라보기)
        FlipTowards(player.position);
        anim.SetTrigger("DashReady"); // 준비 자세 (없으면 Idle)
        
        // 경고 이펙트
        if (warningEffect != null) Instantiate(warningEffect, transform.position + Vector3.up, Quaternion.identity);
        
        // 붉게 깜빡이거나 하는 연출 (선택)
        // StartCoroutine(FlashWarningColor()); 

        yield return new WaitForSeconds(dashWarningTime);

        // 2. [조준] 발사 직전 방향 확정
        Vector2 dashDir = (player.position - transform.position).normalized;
        
        // Y축 보정 (너무 위아래로 날아다니면 이상하니까 X축 위주로 하려면 아래 주석 해제)
        // dashDir.y = 0; dashDir.Normalize(); 

        // 3. [돌진] 발사!
        anim.SetTrigger("DashGo"); // 달리기/돌진 애니메이션
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(dashSound);
        if (dashDustEffect != null) Instantiate(dashDustEffect, transform.position, Quaternion.identity);

        float timer = 0f;
        while (timer < dashDuration)
        {
            // 속도 적용
            rb.linearVelocity = dashDir * dashSpeed;
            
            // ★ 돌진 중 충돌 판정 (몸통 박치기)
            CheckDashCollision();

            timer += Time.deltaTime;
            yield return null;
        }

        // 4. [정지 및 후딜레이]
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("DashEnd"); // 멈추는 동작 (없으면 Idle)
        
        yield return new WaitForSeconds(1.0f); // 1초간 헉헉거림

        // 5. 복귀
        nextDashTime = Time.time + dashCooldown;
        isDashing = false;
    }

    // 돌진 중 플레이어와 부딪혔는지 검사
    private void CheckDashCollision()
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        // 몸체 크기만큼 검사 (약 1.0f ~ 1.5f)
        if (Vector2.Distance(centerPos, player.position) <= 1.0f)
        {
            // 데미지 입히고 밀쳐내기
            player.GetComponent<PlayerStats>()?.TakeDamage(dashDamage, transform);
        }
    }

    // --- 기본 AI 함수들 (이전과 동일) ---

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("Walk", true);
        FlipTowards(player.position);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Walk", false);
    }

    private void FlipTowards(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    private void AttackPlayer()
    {
        StopMoving();
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackRate;
            anim.SetTrigger("Attack1");
            StartCoroutine(NormalAttackRoutine());
        }
    }

    private IEnumerator NormalAttackRoutine()
    {
        yield return new WaitForSeconds(0.3f); // 선딜
        
        // 평타 범위 체크
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        if (Vector2.Distance(centerPos, player.position) <= attackRange)
        {
            player.GetComponent<PlayerStats>()?.TakeDamage(normalDamage, transform);
        }

        yield return new WaitForSeconds(0.5f); // 후딜
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(centerPos, detectRange);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(centerPos, attackRange);
    }
}