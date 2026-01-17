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

    // 1. 변수 추가
private Vector3 originalScale;

    // ★ 추가: 경직 상태인지 확인하는 변수
    private bool isHit = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. 시작할 때 에디터에서 설정한 크기를 기억함!
    originalScale = transform.localScale;
    }

    private void Start()
    {
        // 씬에 있는 "Player" 태그를 가진 오브젝트를 찾음
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (isDead || player == null || isHit) return;

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

        // ★ 추가: 죽었거나, 타겟이 없거나, "맞는 중(isHit)"이면 AI 정지
        if (isDead || player == null || isHit) return;
    }

    private void ChasePlayer()
    {
        // 플레이어 방향 계산 (왼쪽/오른쪽)
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 이동 (Y축은 중력 영향을 받아야 하므로 유지)
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // 애니메이션: 걷기
      //  anim.SetBool("IsRunning", true);

        // 3. 방향 전환 시 '기억해둔 크기'를 사용
    if (direction.x > 0) 
    {
        // 오른쪽 볼 때는 원본 크기 그대로 (X를 양수로)
        transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
    else if (direction.x < 0) 
    {
        // 왼쪽 볼 때는 X만 뒤집음 (X를 음수로)
        transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
       // anim.SetBool("IsRunning", false);
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
            //  anim.SetTrigger("Attack"); //현재 애니메이터 없으니 일단 보류
            
    
             StartCoroutine(AttackRoutine()); //공격 모션 만큼의 딜레이 계산 필요
        }
    }
    
    private IEnumerator AttackRoutine()
    {
        // 예: 칼을 휘두르는 타이밍이 0.4초 뒤라면?
        yield return new WaitForSeconds(0.4f); 

        // 3. 0.4초 뒤에 데미지 판정 실행
        DealDamage();
        
        // (선택) 공격 후딜레이: 공격이 완전히 끝나고 다시 추적하게 하려면 여기서 잠시 더 대기
        yield return new WaitForSeconds(0.5f);
        
        // 공격 끝남 (다시 추적 가능)
        isAttacking = false; 
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
       // isAttacking = false;
        // 주의: 여기서 isAttacking = false를 하면 안 됨 (코루틴에서 제어)
    }

    // 사망 처리 (외부 EnemyHealth에서 호출)
    public void OnDeath()
    {
        isDead = true;
        StopMoving();
        rb.simulated = false; // 물리 충돌 끄기
      //  anim.SetTrigger("Die");
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

    // ★ 추가: 외부(Health)에서 호출할 경직 함수
    public void HitStun(float duration)
    {
        // 이미 맞고 있는 중이라면 기존 코루틴을 끄고 새로 시작 (연타 맞을 때 경직 갱신)
        StopCoroutine(nameof(StunCoroutine));
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isHit = true; // 1. AI 생각 정지 (Update 로직 차단)
        //anim.SetTrigger("Hit"); // 2. 피격 애니메이션 (있다면)
        
        // 주의: 여기서 rb.linearVelocity = zero를 하지 않습니다. 
        // 왜냐하면 넉백 힘(AddForce)에 의해 밀려나야 하니까요!

        yield return new WaitForSeconds(duration); // 3. 경직 시간 대기

        isHit = false; // 4. AI 다시 작동
    }
}