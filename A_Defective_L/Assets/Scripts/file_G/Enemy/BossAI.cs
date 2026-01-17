using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    // 보스의 상태 정의 (열거형)
    public enum BossState { Idle, Move, Attack_Smash, Attack_Dash, Die }

    [Header("State")]
    public BossState currentState; // 현재 상태
    public bool isActing = false;  // 행동 중인가? (행동 중엔 다른 생각 안 함)

    [Header("References")]
    public Transform player;
    public float detectRange = 10f;
    public float attackRange = 2f;

    private Animator anim;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentState = BossState.Idle; // 태어나면 대기
    }

    private void Update()
    {
        if (player == null || isActing) return; // 행동 중이면 생각 금지

        // 상태에 따라 행동 분기 (스위치 문)
        switch (currentState)
        {
            case BossState.Idle:
                StartCoroutine(ThinkRoutine()); // 다음엔 뭐 하지? 고민 시작
                break;
            
            case BossState.Move:
                ChasePlayer(); // 일반 몹처럼 쫓아가기
                break;
                
            // 공격 상태는 여기서 처리 안 하고, Think에서 바로 코루틴으로 실행
        }
    }

    // 보스의 뇌: "다음엔 뭐 하지?"
    private IEnumerator ThinkRoutine()
    {
        isActing = true; // 고민 중에는 방해 금지
        
        // 1. 잠깐 노려보는 시간 (위압감)
        yield return new WaitForSeconds(1.0f); 

        // 2. 거리 계산
        float dist = Vector2.Distance(transform.position, player.position);

        // 3. 패턴 선택 로직
        if (dist > detectRange)
        {
            currentState = BossState.Idle; // 너무 멀면 멍때리기
            isActing = false;
        }
        else if (dist > attackRange)
        {
            // 공격 사거리보다 멀면 -> 쫓아가자
            currentState = BossState.Move; 
            isActing = false; // 이동은 Update에서 계속 해야 하므로 false
        }
        else
        {
            // 사거리 안이다 -> 공격하자!
            // (나중에 여기서 랜덤으로 패턴을 섞습니다)
            StartCoroutine(SmashAttack()); // 예: 내려찍기
        }
    }

    private void ChasePlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        
        // 공격 범위에 들어왔으면 이동 멈추고 대기(-> 바로 공격으로 이어짐)
        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = BossState.Idle; // 상태를 Idle로 바꾸면 Think가 다시 돔
            return;
        }

        // 이동 로직 (일반 몹과 동일)
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * 2f, rb.linearVelocity.y);

        // 방향 전환 (Flip)
        if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);
    }

    // 패턴 1: 내려찍기 (예시)
    private IEnumerator SmashAttack()
    {
        isActing = true;
        currentState = BossState.Attack_Smash;
        
        rb.linearVelocity = Vector2.zero; // 공격할 땐 멈춤
        anim.SetTrigger("Smash"); // 애니메이션 실행

        Debug.Log("보스: 내려찍기 준비!");
        yield return new WaitForSeconds(1.5f); // 공격 모션 시간만큼 대기
        
        Debug.Log("보스: 쾅!!");
        // 여기서 데미지 판정 범위 생성 (OverlapBox 등)

        yield return new WaitForSeconds(1.0f); // 후딜레이 (플레이어 공격 기회)

        currentState = BossState.Idle; // 다시 대기 상태로 복귀
        isActing = false;
    }
}