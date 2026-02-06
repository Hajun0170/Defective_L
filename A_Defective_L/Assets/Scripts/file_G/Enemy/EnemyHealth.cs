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

    [Header("Type & Persistence")]
    [SerializeField] private bool isBoss = false; 
    
    // ★ [복구] 저장 기능을 위해 이 몬스터만의 고유 ID가 필요합니다.
    // (예: Stage1_Boss, Map2_HiddenItemMob)
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
    public Weapon dropWeapon; // 보상은 여기서 관리!
    public AudioClip deathSound;

    [Header("★ 씬 전용 보상 패널")]
    // 여기에 패널을 넣으면, UIManager 대신 이 패널을 직접 띄웁니다.
    public GameObject directRewardPanel; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        col = GetComponent<Collider2D>(); 
        currentHealth = maxHealth;
    }

    // ★ [복구] 게임 시작 시 이미 죽은 몬스터인지 확인
    private void Start()
    {
        if (!string.IsNullOrEmpty(uniqueID) && DataManager.Instance != null)
        {
            if (DataManager.Instance.IsBossDefeated(uniqueID)) 
            {
                gameObject.SetActive(false); // 이미 잡았으면 삭제(비활성화)
                // Debug.Log($"💀 {uniqueID} 이미 처치됨.");
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

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        // ★ [복구] 죽는 순간 장부에 기록 (다시 안 나오게)
        if (!string.IsNullOrEmpty(uniqueID) && DataManager.Instance != null)
        {
            DataManager.Instance.RegisterBossKill(uniqueID);
            DataManager.Instance.SaveDataToDisk(); // 확실하게 즉시 저장
        }

        Vector3 spawnPos = GetSpawnPosition();

        // 1. 이펙트 & 아이템
        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
        if (dropItemPrefab != null) Instantiate(dropItemPrefab, spawnPos, Quaternion.identity);

        // 2. 사운드
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(deathSound);

        // 3. 보상 패널 및 획득 처리
        if (dropWeapon != null)
        {
            // A. 직접 연결된 패널이 있다면? (씬 전용)
            if (directRewardPanel != null)
            {
                directRewardPanel.SetActive(true); // 패널 켜기
                
                // ★ [오류 수정 부분] WeaponManager에게 무기 쥐어주기
                if (GameManager.Instance != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if(player != null) 
                    {
                        // dropWeapon.weaponID (int) 대신 dropWeapon (Weapon 객체) 자체를 넘깁니다.
                        // (WeaponManager에 AddWeapon(Weapon w) 함수가 있다고 가정)
                        // 만약 int만 받는다면 AddWeapon(dropWeapon.weaponID)가 맞지만,
                        // 에러가 났다는 건 AddWeapon이 Weapon 타입을 원한다는 뜻입니다.
                        player.GetComponent<WeaponManager>()?.AddWeapon(dropWeapon);
                    }
                }

                Time.timeScale = 0f; // 일시정지
                Debug.Log("🎁 씬 전용 보상 패널 활성화!");
            }
            // B. 없다면 매니저 시스템 이용
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.GetWeaponReward(dropWeapon);
            }
        }

        // 4. 사망 처리
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