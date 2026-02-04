using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private float knockbackForce = 5.0f; 
    [SerializeField] private float stunTime = 0.3f;       

    [Header("Type")]
    [SerializeField] private bool isBoss = false; 

    [Header("드랍 & 이펙트")]
    public GameObject dropItemPrefab;   
    public GameObject deathEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;

    private Coroutine flashRoutine; 

    public Vector2 spawnOffset = new Vector2(0f, 0.5f); 

    private Collider2D col; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        col = GetComponent<Collider2D>(); // Awake에서 찾기
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        // Debug.Log($"몬스터 피격! 남은 체력: {currentHealth}");

        if (!isBoss)
        {
            // AI 경직
            if (enemyAI != null) enemyAI.HitStun(stunTime);

            // 넉백
            if (attacker != null && rb != null)
            {
                float directionX = transform.position.x - attacker.position.x;
                float knockbackSign = (directionX > 0) ? 1 : -1;
                Vector2 knockbackDir = new Vector2(knockbackSign, 0.5f).normalized;

                rb.linearVelocity = Vector2.zero; 
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        // 깜빡임 효과
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        // ★ [핵심 1] 올바른 위치 계산 (함수 사용)
        Vector3 spawnPos = GetSpawnPosition();

        // 1. 사망 이펙트 (계산된 위치 사용)
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
        }

        // 2. 아이템 드랍 (계산된 위치 사용)
        // ★ 기존 코드에선 transform.position을 쓰고 있어서 위치 보정이 안 됐던 것!
        if (dropItemPrefab != null)
        {
            Instantiate(dropItemPrefab, spawnPos, Quaternion.identity);
        }

        // 3. 사망 처리 분기 (보스 vs 일반)
        if (isBoss)
        {
            // 보스라면 매니저에게 보고
            BossBattleManager manager = FindFirstObjectByType<BossBattleManager>();
            if (manager != null)
            {
                manager.OnBossDefeated();
            }
            // 보스 오브젝트는 매니저가 꺼주거나, 여기서 꺼도 됨.
            // 보통 매니저가 연출 후 끄므로 여기선 놔둡니다.
        }
        else
        {
            // ★ [핵심 2] 일반 몬스터는 스스로 죽어야 함!
            // AI 기능 정지 (움직임 멈춤 등)
            if (enemyAI != null) enemyAI.OnDeath();
            
            // 즉시 삭제하지 않고 이펙트 등을 위해 잠시 기다릴 수도 있지만,
            // 보통은 바로 없애거나 비활성화함.
            // 여기서는 EnemyAI의 OnDeath가 2초 뒤 삭제를 담당하므로 
            // 중복되지 않게 하거나, 확실하게 여기서 처리.
            
            // 깔끔하게 여기서 바로 삭제 (이펙트는 이미 생성했으니까)
            Destroy(gameObject); 
        }
    }

    private IEnumerator HitColorRoutine()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.red; // 피격 시 빨강 추천
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    Vector3 GetSpawnPosition()
    {
        // 콜라이더가 없으면 현재 위치 사용
        Vector3 basePos = (col != null) ? col.bounds.center : transform.position;
        return basePos + (Vector3)spawnOffset;
    }
}