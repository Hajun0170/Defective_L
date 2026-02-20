using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour //보스 ai의 기본셋. 이 뼈대로 다른 보스의 패턴을 추가하거나 줄이는 용도로 남겨둠
{
    // 보스 상태
    public enum BossState { Idle, Move, Attack_Smash, Attack_Dash, Die }

    [Header("State")]
    public BossState currentState; // 현재 상태
    public bool isActing = false;  // 작동 여부

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
        currentState = BossState.Idle; // 대기
    }

    private void Update()
    {
        if (player == null || isActing) return; // 행동 중이면 생각 금지

        // 상태에 따라 행동 분기 (스위치)
        switch (currentState)
        {
            case BossState.Idle:
                StartCoroutine(ThinkRoutine()); // 다음 패턴 대기(패턴 연결 고민 여부)
                break;
            
            case BossState.Move:
                ChasePlayer(); // 일반 몹처럼 쫓아감
                break;
                
            // 공격 상태는 여기서 처리 안 하고, Think에서 바로 코루틴으로 실행
        }
    }

    private IEnumerator ThinkRoutine()
    {
        isActing = true; 
        //대기
        yield return new WaitForSeconds(1.0f); 

        // 거리 계산
        float dist = Vector2.Distance(transform.position, player.position);

        //  패턴 선택 로직
        if (dist > detectRange)
        {
            currentState = BossState.Idle; // 너무 멀면 대기(오동작 방지.)
            isActing = false;
        }
        else if (dist > attackRange)
        {
            // 공격 사거리보다 멀면 쫓아감
            currentState = BossState.Move; 
            isActing = false; // 이동은 Update에서 계속 해야 하므로 false
        }
        else
        {
            // 사거리 안이므로 공격
            StartCoroutine(SmashAttack()); // 공격 출력
        }
    }

    private void ChasePlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        
        // 공격 범위에 들어왔으면 이동 멈추고 대기
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

    // 공격 패턴
    private IEnumerator SmashAttack()
    {
        isActing = true;
        currentState = BossState.Attack_Smash;
        
        rb.linearVelocity = Vector2.zero; // 공격할 땐 멈춤
        anim.SetTrigger("Smash"); // 애니메이션 실행

        Debug.Log("보스 공격을 출력");
        yield return new WaitForSeconds(1.5f); // 공격 모션 시간만큼 대기함
        
        Debug.Log("충격파");
        // 데미지 판정 범위 생성 

        yield return new WaitForSeconds(1.0f); // 후딜레이 (플레이어 공격을 위한 딜레이)

        currentState = BossState.Idle; // 다시 대기 상태로 복귀
        isActing = false;
    }
}