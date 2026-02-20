using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour //적의 체력을 담당하는 코드
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private float knockbackForce = 5.0f; 
    [SerializeField] private float stunTime = 0.3f;       

    [Header("Type & Persistence")]
    [SerializeField] private bool isBoss = false; 
    
    // 저장 기능을 위해 몬스터의 고유 ID 필요
    // Stage1_Boss
    public string uniqueID; 

    [Header("드랍 & 이펙트")]
    public GameObject dropItemPrefab;   
    public GameObject deathEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;
    private Coroutine flashRoutine; 
    public Vector2 spawnOffset = new Vector2(0f, 0.5f); 
    private Collider2D col; 

    [Header("Reward")]
    public Weapon dropWeapon; // 보상은 여기서 관리
    public AudioClip deathSound;

    [Header("씬 전용 보상 패널")]
    // 여기에 패널을 넣으면, UIManager 대신 이 패널을 직접 띄움
    public GameObject directRewardPanel; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        col = GetComponent<Collider2D>(); 
        currentHealth = maxHealth;
    }

    // 게임 시작 시 이미 죽은 몬스터인지 확인
    private void Start()
    {
        if (!string.IsNullOrEmpty(uniqueID) && DataManager.Instance != null)
        {
            if (DataManager.Instance.IsBossDefeated(uniqueID)) 
            {
                gameObject.SetActive(false); // 이미 잡았으면 삭제(비활성화)
            }
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (!isBoss)
        {
            if (enemyAI != null) enemyAI.HitStun(stunTime);

            if (attacker != null && rb != null)
            {
                float directionX = transform.position.x - attacker.position.x;
                float knockbackSign = (directionX > 0) ? 1 : -1;
                Vector2 knockbackDir = new Vector2(knockbackSign, 0.5f).normalized;

                rb.linearVelocity = Vector2.zero; 
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitColorRoutine());

        // 체력이 0이 되면 코루틴 시작
        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }
   
   IEnumerator DeathSequence()
    {
        
        // 충돌 끄기 (플레이어가 닿아도 피격이 없음)
        if (col != null) col.enabled = false; 

        // 물리 끔 (바닥으로 툭 떨어지는 것 방지)
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero; // 움직임 멈춤
            rb.gravityScale = 0f;            // 중력 끔
            rb.simulated = false;            // 물리 연산 제외
        }

        // 스프라이트 끄기
        // Destroy는 아니지만 플레이어 눈에는 사라진 것처럼 보임
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        
        // AI 정지
        if (enemyAI != null) enemyAI.enabled = false;

        // 특정 중요 몬스터일 경우 목록 최신화
        if (!string.IsNullOrEmpty(uniqueID) && DataManager.Instance != null)
        {
            DataManager.Instance.RegisterBossKill(uniqueID);
            DataManager.Instance.SaveDataToDisk();
        }

        // 이펙트 & 아이템 생성
        // 몬스터가 사라진 위치에서 이펙트 출력
        Vector3 spawnPos = GetSpawnPosition();
        Vector3 itemSpawnPos = spawnPos + new Vector3(0, 0.5f, 0); 

        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(deathSound);

        if (dropItemPrefab != null) 
        {
            GameObject item = Instantiate(dropItemPrefab, itemSpawnPos, Quaternion.identity);
            
            // 아이템이 살짝 튀어오르게
            Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                itemRb.AddForce(new Vector2(0, 3f), ForceMode2D.Impulse);
            }
        }

        // 대기 (플레이어는 이펙트를 보고, 1.5초 뒤 패널을 기다림)
        yield return new WaitForSeconds(1.5f); 

        // 보상 패널 띄우기
        if (dropWeapon != null)
        {
            if (directRewardPanel != null)
            {
                directRewardPanel.SetActive(true);
                
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if(player != null) 
                {
                    player.GetComponent<WeaponManager>()?.AddWeapon(dropWeapon);
                }
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.GetWeaponReward(dropWeapon);
            }
        }

        // 삭제 
        if (isBoss)
        {
            BossBattleManager manager = FindFirstObjectByType<BossBattleManager>();
            if (manager != null) manager.OnBossDefeated();
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void Die()
    {
        // 죽는 순간 목록에 기록 (다시 나오지 않음)
        if (!string.IsNullOrEmpty(uniqueID) && DataManager.Instance != null)
        {
            DataManager.Instance.RegisterBossKill(uniqueID);
            DataManager.Instance.SaveDataToDisk(); // 즉시 저장
        }

        Vector3 spawnPos = GetSpawnPosition();

        // 이펙트 & 아이템
        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
        if (dropItemPrefab != null) Instantiate(dropItemPrefab, spawnPos, Quaternion.identity);

        // 사운드
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(deathSound);

        // 보상 패널 및 획득 처리
        if (dropWeapon != null)
        {
            // 직접 연결된 패널이 있다면? (씬 전용)
            if (directRewardPanel != null)
            {
                directRewardPanel.SetActive(true); // 패널 켜기
                
                // WeaponManager에게 무기 쥐어주기
                if (GameManager.Instance != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if(player != null) 
                    {
                        player.GetComponent<WeaponManager>()?.AddWeapon(dropWeapon);
                    }
                }

                Time.timeScale = 0f; // 일시정지
                Debug.Log("씬 전용 보상 패널");
            }
            // 없다면 매니저 시스템 이용
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.GetWeaponReward(dropWeapon);
            }
        }

        // 사망 처리
        if (isBoss)
        {
            BossBattleManager manager = FindFirstObjectByType<BossBattleManager>();
            if (manager != null) manager.OnBossDefeated();
        }
        else
        {
            if (enemyAI != null) enemyAI.OnDeath();
            Destroy(gameObject); 
        }
    }

    private IEnumerator HitColorRoutine()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.red; 
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 basePos = (col != null) ? col.bounds.center : transform.position;
        return basePos + (Vector3)spawnOffset;
    }

    
}