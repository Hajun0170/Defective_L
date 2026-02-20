using System.Collections;
using UnityEngine;

public class BossDashPattern : MonoBehaviour //2스테이지에서 쓸 대쉬형 보스, 그 외 나머지 패턴은 기본 몬스터와 동일
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
    [SerializeField] private float dashWarningTime = 1.0f; // 돌진 전 대기
    [SerializeField] private float dashSpeed = 12.0f;    // 돌진 속도
    [SerializeField] private float dashDuration = 0.5f;  // 돌진 지속 시간
    [SerializeField] private int dashDamage = 2;         // 돌진은 데미지... 1로 변경함. 2칸 씩 닳으면 대처가 안 되는 문제

    [Header("Effects & Sound")]
    [SerializeField] private GameObject warningEffect;   // 돌진 전 경고 이펙트 (현재는 사용은 안함)
    [SerializeField] private GameObject dashDustEffect;  // 달릴 때 효과
    [SerializeField] private AudioClip dashSound;        // 사운드

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

        // 2페이즈 진입. 체력 50% 이하와 쿨타임 완료 기준
  
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

        // 기본 AI 
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

    // 돌진 코루틴
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        StopMoving(); // 일단 멈춤

        // 플레이어 바라봄
        FlipTowards(player.position);
        anim.SetTrigger("DashReady"); // 대기 자세
        
        // 경고 이펙트. 현재는 표시 안함
        if (warningEffect != null) Instantiate(warningEffect, transform.position + Vector3.up, Quaternion.identity);

        yield return new WaitForSeconds(dashWarningTime);

        // 돌진 직전 방향 결정
        Vector2 dashDir = (player.position - transform.position).normalized;
        
        //  돌진 실행
        anim.SetTrigger("DashGo"); // 돌진 애니메이션
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(dashSound);
        if (dashDustEffect != null) Instantiate(dashDustEffect, transform.position, Quaternion.identity);

        float timer = 0f;
        while (timer < dashDuration)
        {
            // 속도 적용
            rb.linearVelocity = dashDir * dashSpeed;
            
            // 돌진 중 충돌 판정 
            CheckDashCollision();

            timer += Time.deltaTime;
            yield return null;
        }

        // 정지 및 후딜레이
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("DashEnd"); // 멈추는 동작... 지정은 안해서 출력은 X
        
        yield return new WaitForSeconds(1.0f); // 1초간 대기 상태

        // 다시 반복함 - 원래 상태로 돌림
        nextDashTime = Time.time + dashCooldown;
        isDashing = false;
    }

    // 돌진 중 플레이어와 부딪혔는지 검사
    private void CheckDashCollision()
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        // 몸체 크기만큼 검사 1.0f ~ 1.5f
        if (Vector2.Distance(centerPos, player.position) <= 1.0f)
        {
            // 데미지 입히고 밀쳐냄
            player.GetComponent<PlayerStats>()?.TakeDamage(dashDamage, transform);
        }
    }

    //기본 추격 로직

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