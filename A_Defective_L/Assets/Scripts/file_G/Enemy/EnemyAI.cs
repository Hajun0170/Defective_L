using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour //필드 몬스터의 기본적인 ai. 거리 감지로 추격.
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float detectRange = 5.0f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int damage = 1;

    [Header("Combat Settings")]
    [SerializeField] private float attackRate = 1.5f; // 공격 간격 (쿨타임)
    [Tooltip("공격 동작이 끝나고 멍하니 서있는 시간")]
    [SerializeField] private float attackAfterDelay = 1.0f; // 재공격 전 대기 시간 (후딜레이)
    [Tooltip("감지 및 공격 범위의 중심점 위치 보정 (x, y)")]
    [SerializeField] private Vector2 rangeCenterOffset; // 범위 중심 보정용 변수

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 상태 관리
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private Vector3 originalScale;
    private bool isHit = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (isDead || player == null || isHit) return;

        // 거리 계산 시 Offset을 적용한 위치(centerPos)를 사용
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        float distanceToPlayer = Vector2.Distance(centerPos, player.position);

        // 공격 범위 안에 들어옴, 공격
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        // 감지 범위 안에 들어옴, 추적
        else if (distanceToPlayer <= detectRange && !isAttacking)
        {
            ChasePlayer();
        }
        // 멈춤
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("Walk", true);

        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Walk", false);
    }

    private void AttackPlayer()
    {
        StopMoving();

        if (Time.time >= nextAttackTime && !isAttacking)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackRate;

            anim.SetTrigger("Attack1");
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        // 공격 선딜레이 (칼 휘두르기 전 준비 동작)
        yield return new WaitForSeconds(0.2f);

        DealDamage();

        // 인스펙터 변수(attackAfterDelay) 사용
        // 이 시간 동안 적은 움직이지 않고 제자리에 멈춰 있음.
        yield return new WaitForSeconds(attackAfterDelay);

        isAttacking = false; // 이제 다시 추적 가능
    }

    public void DealDamage()
    {
        if (isDead) return;

        // 데미지 판정 거리 계산에 Offset 적용
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        
        // 공격 사거리 체크
        if (Vector2.Distance(centerPos, player.position) <= attackRange)
        {
            player.GetComponent<PlayerStats>()?.TakeDamage(damage, transform);
        }
    }

    public void OnDeath()
    {
        isDead = true;
        StopMoving();
        rb.simulated = false;
        Destroy(gameObject, 2.0f);
    }

    // 기즈모(범위 표시)는 Offset이 적용된 위치에 그려짐, 직관적으로 보기 위함
    private void OnDrawGizmosSelected()
    {
        // 보정된 중심점 위치 계산
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPos, detectRange); // 감지 범위

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPos, attackRange); // 공격 범위
        
        // 중심점
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPos, 0.1f); 
    }

    public void HitStun(float duration)
    {
        StopCoroutine(nameof(StunCoroutine));
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isHit = true;
        yield return new WaitForSeconds(duration);
        isHit = false;
    }
}