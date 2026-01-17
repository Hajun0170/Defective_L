using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private float knockbackForce = 5.0f; // 넉백 파워
    [SerializeField] private float stunTime = 0.3f;       // 경직 시간

    [Header("Type")]
    [SerializeField] private bool isBoss = false; // ★ 이거 체크하면 보스 취급

    // ★ 이 변수 선언들이 지워져서 에러가 났던 겁니다!
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;

    private Coroutine flashRoutine; // 깜빡이 코루틴 참조용

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        currentHealth = maxHealth;
    }

    // ★ 때린 사람(attacker) 정보를 받는 수정된 함수
    public void TakeDamage(int damage, Transform attacker)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"몬스터 피격! 남은 체력: {currentHealth}");

    if(!isBoss)
    {
        // 1. AI 경직 (잠깐 멈춤)
        if (enemyAI != null) enemyAI.HitStun(stunTime);

        // 2. 넉백 (밀려남)
        if (attacker != null && rb != null)
        {
            // 때린 사람 반대 방향 계산
           // Vector2 knockbackDir = (transform.position - attacker.position).normalized;

            // [수정 후] Y축 차이를 무시하고, 오직 왼쪽(-1)인지 오른쪽(1)인지만 판단
            float directionX = transform.position.x - attacker.position.x;

            // 0보다 크면 오른쪽(1), 작으면 왼쪽(-1)
            float knockbackSign = (directionX > 0) ? 1 : -1;

            // X축으로는 강하게, Y축으로는 아주 살짝(0.5f)만 띄움 
            // (살짝 띄워야 바닥 마찰력 때문에 안 밀리는 현상을 방지함)
            Vector2 knockbackDir = new Vector2(knockbackSign, 0.5f).normalized;

            // 기존 속도 초기화 후 힘 가하기
            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        // ★ 수정: 기존에 깜빡이던 게 있다면 끄고(Reset), 새로 시작
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());

       //기존 // StartCoroutine(HitColorRoutine());
    }
        else
        {
             //타격 이펙트만 적용
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());
        }
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("몬스터 사망!");
        if (enemyAI != null) enemyAI.OnDeath();
        // 필요 시 아이템 드랍 로직 추가
    }


    private IEnumerator HitColorRoutine()
    {
        // 1. 빨간색으로 변경
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        // 2. 0.1초 대기
        yield return new WaitForSeconds(0.1f);

        // 3. 원래 색(흰색)으로 복구
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        
        // 4. 변수 초기화 (끝났음 표시)
        flashRoutine = null;
    }
}