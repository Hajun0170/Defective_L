using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float detectRange = 5.0f; // 플레이어 감지 범위
    [SerializeField] private float attackRange = 0.8f; // 공격 사거리
    [SerializeField] private int damage = 1;

    [Header("References")]
    [SerializeField] private LayerMask playerLayer; // 플레이어 레이어
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 상태 관리
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float attackRate = 1.5f; // 공격 속도 (초)

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 씬에 있는 "Player" 태그를 가진 오브젝트를 찾음
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. 공격 범위 안에 들어옴 -> 공격
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        // 2. 감지 범위 안에 들어옴 -> 추적
        else if (distanceToPlayer <= detectRange && !isAttacking)
        {
            ChasePlayer();
        }
        // 3. 멈춤 (대기)
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer()
    {
        // 플레이어 방향 계산 (왼쪽/오른쪽)
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 이동 (Y축은 중력 영향을 받아야 하므로 유지)
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // 애니메이션: 걷기
        anim.SetBool("IsRunning", true);

        // 방향 전환 (Flip)
        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1); // 오른쪽
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1); // 왼쪽
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsRunning", false);
    }

    private void AttackPlayer()
    {
        // 멈춰서 공격
        StopMoving();

        if (Time.time >= nextAttackTime)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackRate;

            // 공격 애니메이션 실행
            anim.SetTrigger("Attack");
            
            // 실제 데미지는 애니메이션 이벤트나 코루틴으로 타이밍 맞춰서 주는 게 좋음
            // 여기서는 간단하게 즉시 데미지 예시
            // StartCoroutine(AttackRoutine());
        }
    }
    
    // 공격 애니메이션의 특정 프레임에서 이 함수를 호출하거나, 코루틴으로 처리
    public void DealDamage()
    {
        if(isDead) return;
        
        // 사거리 다시 체크 (플레이어가 피했을 수도 있으니)
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // 플레이어에게 데미지 전달
            player.GetComponent<PlayerStats>()?.TakeDamage(damage, transform);
        }
        isAttacking = false;
    }

    // 사망 처리 (외부 EnemyHealth에서 호출)
    public void OnDeath()
    {
        isDead = true;
        StopMoving();
        rb.simulated = false; // 물리 충돌 끄기
        anim.SetTrigger("Die");
        Destroy(gameObject, 2.0f); // 2초 뒤 삭제
    }

    // 에디터에서 범위 확인용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}